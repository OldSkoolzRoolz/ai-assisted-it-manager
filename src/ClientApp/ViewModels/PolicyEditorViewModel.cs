// Project Name: ClientApp
// File Name: PolicyEditorViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CorePolicyEngine.Parsing;
using CorePolicyEngine.AdminTemplates;
using CorePolicyEngine; // for Result
using Microsoft.Extensions.Logging;
using ClientApp.Logging; // for source-generated logging extension methods
using CorePolicyEngine.Storage;

namespace ClientApp.ViewModels;

public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private readonly IAdminTemplateLoader _loader;
    private readonly ILogger<PolicyEditorViewModel> _logger;
    private readonly IAuditWriter _audit;

    public ObservableCollection<CategoryTreeItem> CategoryTree { get; } = [];
    public ObservableCollection<PolicySummary> Policies { get; } = [];
    public ObservableCollection<PolicySummary> FilteredPolicies { get; } = [];
    public ObservableCollection<PolicySettingViewModel> SelectedPolicySettings { get; } = [];

    private AdminTemplateCatalog? _catalog;
    public AdminTemplateCatalog? Catalog { get => _catalog; private set { _catalog = value; OnPropertyChanged(); } }

    private PolicySummary? _selectedPolicy;
    public PolicySummary? SelectedPolicy { get => _selectedPolicy; set { if (_selectedPolicy != value) { _selectedPolicy = value; OnSelectedPolicyChanged(); OnPropertyChanged(); } } }

    private CategoryTreeItem? _selectedCategoryNode;
    public CategoryTreeItem? SelectedCategoryNode { get => _selectedCategoryNode; set { if (_selectedCategoryNode != value) { _selectedCategoryNode = value; OnPropertyChanged(); SelectedPolicy = null; } } }

    private string? _lastSearch;
    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value; OnPropertyChanged();
                ApplySearchFilter(_searchText);
                RebuildCategoryRoots();
                _logger.SearchFilterApplied(_searchText ?? string.Empty);
            }
        }
    }

    public ICommand LoadPoliciesCommand { get; }
    public ICommand SearchLocalPoliciesCommand { get; }

    public PolicyEditorViewModel(IAdminTemplateLoader loader, ILogger<PolicyEditorViewModel> logger, IAuditWriter audit)
    {
        _loader = loader;
        _logger = logger;
        _audit = audit;
        LoadPoliciesCommand = new RelayCommand(async _ => await LoadDefaultSubsetAsync().ConfigureAwait(false), _ => true);
        SearchLocalPoliciesCommand = new RelayCommand(async _ => await LoadEntireCatalogAsync("en-US", CancellationToken.None).ConfigureAwait(false), _ => true);
        _logger.Initialized(); // TODO localize message key via resources
    }

    private async Task LoadDefaultSubsetAsync()
    {
        await LoadEntireCatalogAsync("en-US", CancellationToken.None).ConfigureAwait(false);
    }

    private async Task LoadEntireCatalogAsync(string languageTag, CancellationToken token)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Result<AdminTemplateCatalog> result;
        try
        {
            result = await this._loader.LoadLocalCatalogAsync(languageTag, 50, token).ConfigureAwait(false); // cap initial load
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception loading catalog language {Language}", languageTag); // TODO localize
            return;
        }
        sw.Stop();
        if (!result.Success || result.Value is null)
        {
            _logger.CatalogLoadFailed(languageTag);
            return;
        }
        Catalog = result.Value;
        Policies.Clear();
        foreach (var s in Catalog.Summaries.OrderBy(s => s.DisplayName)) Policies.Add(s);
        ApplySearchFilter(_lastSearch);
        RebuildCategoryRoots();
        _logger.CatalogLoaded(languageTag, Policies.Count, sw.ElapsedMilliseconds);
    }

    public async Task SearchLocalPoliciesAsync(string? query, CancellationToken token)
    {
        if (Catalog == null) await LoadEntireCatalogAsync("en-US", token).ConfigureAwait(false);
        ApplySearchFilter(query);
        RebuildCategoryRoots();
        _logger.SearchExecuted(query ?? string.Empty, FilteredPolicies.Count);
        if (!string.IsNullOrWhiteSpace(query))
        {
            try { await _audit.PolicyViewedAsync("SEARCH:" + query, token); } catch { }
        }
    }

    public void ApplySearchFilter(string? query)
    {
        _lastSearch = query;
        FilteredPolicies.Clear();
        IEnumerable<PolicySummary> source = Policies;
        if (!string.IsNullOrWhiteSpace(query))
        {
            string q = query.Trim().ToLowerInvariant();
            source = source.Where(p =>
                (p.DisplayName?.ToLowerInvariant().Contains(q) ?? false) ||
                (p.Key.Name.ToLowerInvariant().Contains(q)) ||
                (p.CategoryPath?.ToLowerInvariant().Contains(q) ?? false));
        }
        foreach (var p in source) FilteredPolicies.Add(p);
    }

    private void RebuildCategoryRoots()
    {
        CategoryTree.Clear();
        if (Catalog == null) return;

        var localizedNames = new Dictionary<CategoryId, string>();
        foreach (var admx in Catalog.AdmxDocuments)
        {
            foreach (var cat in admx.Categories)
                localizedNames[cat.Id] = cat.Id.Value; // TODO: replace with localized lookup
        }

        var roots = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent is null)
            .GroupBy(c => c.Id.Value)
            .Select(g => g.First())
            .OrderBy(c => c.Id.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var r in roots)
            CategoryTree.Add(new CategoryTreeItem(r, localizedNames[r.Id]));
    }

    public void EnsureCategoryChildren(CategoryTreeItem node)
    {
        if (Catalog == null) return;
        if (node.IsPolicy || node.Category is null) return;
        if (node.ChildrenMaterialized) return;

        node.Children.Clear();

        var childCats = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent.HasValue && c.Parent.Value.Id.Value == node.Category.Id.Value)
            .OrderBy(c => c.Id.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var c in childCats)
            node.Children.Add(new CategoryTreeItem(c, c.Id.Value));

        var policyMatches = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .Where(pol => pol.Category.Id.Value == node.Category.Id.Value)
            .Join(Catalog.Summaries, pol => pol.Key.Name, s => s.Key.Name, (pol, s) => s)
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var p in policyMatches)
            node.Children.Add(new CategoryTreeItem(p));

        node.ChildrenMaterialized = true;
        if (node.Children.Count == 0)
            node.Children.Add(CategoryTreeItem.EmptyMarker());
        _logger.CategoryExpanded(node.Category.Id.Value, childCats.Count, policyMatches.Count);
    }

    private async void OnSelectedPolicyChanged()
    {
        SelectedPolicySettings.Clear();
        if (Catalog == null || SelectedPolicy == null) return;

        var policy = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .FirstOrDefault(p => p.Key.Name == SelectedPolicy.Key.Name);
        if (policy == null)
        {
            _logger.LogWarning("Selected policy key {PolicyKey} not found in catalog documents", SelectedPolicy.Key.Name); // TODO localize
            return;
        }

        foreach (var element in policy.Elements)
            SelectedPolicySettings.Add(new PolicySettingViewModel(policy, element));
        _logger.PolicySelected(SelectedPolicy.Key.Name, SelectedPolicySettings.Count);
        try { await _audit.PolicySelectedAsync(SelectedPolicy.Key.Name); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}