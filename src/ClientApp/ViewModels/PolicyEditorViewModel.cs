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
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine; // for Result
using Microsoft.Extensions.Logging;
using KC.ITCompanion.ClientApp.Logging; // for source-generated logging extension methods
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePersistence.Sql;
using System.IO;

namespace KC.ITCompanion.ClientApp.ViewModels;

public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private readonly IAdminTemplateLoader _loader;
    private readonly ILogger<PolicyEditorViewModel> _logger;
    private readonly IAuditWriter _audit;
    private readonly IPolicyGroupRepository? _policyGroups;

    public ObservableCollection<CategoryTreeItem> CategoryTree { get; } = [];
    public ObservableCollection<PolicySummary> Policies { get; } = [];
    public ObservableCollection<PolicySummary> FilteredPolicies { get; } = [];
    public ObservableCollection<PolicySettingViewModel> SelectedPolicySettings { get; } = [];
    public ObservableCollection<PolicyGroupDto> PolicyGroups { get; } = [];
    public ObservableCollection<PolicyFileGroup> PolicyFileGroups { get; } = [];
    public ObservableCollection<PolicySummary> SelectedCategoryPolicies { get; } = [];

    private Dictionary<string, Category>? _categoryIndex; // categoryId -> category
    private Dictionary<string, string>? _categoryDisplayMap; // categoryId -> localized display name

    private AdminTemplateCatalog? _catalog;
    public AdminTemplateCatalog? Catalog { get => _catalog; private set { _catalog = value; OnPropertyChanged(); OnPropertyChanged(nameof(PolicyFileCount)); } }

    public int PolicyFileCount => Catalog?.AdmxDocuments.Count ?? 0;

    private string? _breadcrumb;
    public string? Breadcrumb { get => _breadcrumb; private set { if (_breadcrumb != value) { _breadcrumb = value; OnPropertyChanged(); } } }

    private PolicySummary? _selectedPolicy;
    public PolicySummary? SelectedPolicy { get => _selectedPolicy; set { if (_selectedPolicy != value) { _selectedPolicy = value; OnSelectedPolicyChanged(); OnPropertyChanged(); } } }

    private CategoryTreeItem? _selectedCategoryNode;
    public CategoryTreeItem? SelectedCategoryNode
    {
        get => _selectedCategoryNode;
        set
        {
            if (_selectedCategoryNode != value)
            {
                _selectedCategoryNode = value;
                OnSelectedCategoryNodeChanged();
            }
        }
    }

    public CategoryTreeItem? SelectedCategoryNode
    {
        get => _selectedCategoryNode;
        set => OnSelectedCategoryNodeChanged(value);
    }

    private void OnSelectedCategoryNodeChanged(CategoryTreeItem? value)
    {
        if (_selectedCategoryNode != value)
        {
            _selectedCategoryNode = value;
            OnPropertyChanged();
            if (_selectedCategoryNode?.Category != null)
                PopulateSelectedCategoryPolicies(_selectedCategoryNode);
            Breadcrumb = BuildBreadcrumb(_selectedCategoryNode?.Category);
            SelectedPolicy = null;
        }
    }

    // Remove the old OnSelectedCategoryNodeChanged() method if it exists below
    {
        OnPropertyChanged(nameof(SelectedCategoryNode));
        if (_selectedCategoryNode?.Category != null)
        {
            PopulateSelectedCategoryPolicies(_selectedCategoryNode);
        }
        Breadcrumb = BuildBreadcrumb(_selectedCategoryNode?.Category);
        SelectedPolicy = null;
    }
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
    public ICommand RefreshPolicyGroupsCommand { get; }

    public PolicyEditorViewModel(IAdminTemplateLoader loader, ILogger<PolicyEditorViewModel> logger, IAuditWriter audit, IPolicyGroupRepository? policyGroupRepository = null)
    {
        _loader = loader;
        _logger = logger;
        _audit = audit;
        _policyGroups = policyGroupRepository;
        LoadPoliciesCommand = new RelayCommand(async _ => await LoadDefaultSubsetAsync(), _ => true);
        // Search now ONLY filters existing in-memory catalog; does not trigger load
        SearchLocalPoliciesCommand = new RelayCommand(_ => ApplySearchFilter(SearchText), _ => Catalog != null);
        RefreshPolicyGroupsCommand = new RelayCommand(async _ => await LoadPolicyGroupsAsync(), _ => _policyGroups != null);
        _logger.Initialized();
    }

    public async Task EnsureCatalogLoadedAsync()
    {
        if (Catalog != null) return;
        await LoadEntireCatalogAsync("en-US", CancellationToken.None);
    }

    private async Task LoadPolicyGroupsAsync()
    {
        if (_policyGroups == null) return;
        try
        {
            var groups = await _policyGroups.GetGroupsAsync(CancellationToken.None);
            App.Current.Dispatcher.Invoke(() =>
            {
                PolicyGroups.Clear();
                foreach (var g in groups) PolicyGroups.Add(g);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed loading policy groups");
        }
    }

    private async Task LoadDefaultSubsetAsync()
    {
        await EnsureCatalogLoadedAsync();
        await LoadPolicyGroupsAsync();
    }

    private async Task LoadEntireCatalogAsync(string languageTag, CancellationToken token)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Result<AdminTemplateCatalog> result;
        try
        {
            result = await this._loader.LoadLocalCatalogAsync(languageTag, 50, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception loading catalog language {Language}", languageTag);
            return;
        }
        sw.Stop();
        if (!result.Success || result.Value is null)
        {
            _logger.CatalogLoadFailed(languageTag);
            return;
        }
        Catalog = result.Value;
        IndexCategories();
        Policies.Clear();
        foreach (var s in Catalog.Summaries.OrderBy(s => s.DisplayName)) Policies.Add(s);
        FilteredPolicies.Clear();
        foreach (var p in Policies) FilteredPolicies.Add(p);
        BuildFileGroups();
        RebuildCategoryRoots();
        Breadcrumb = null;
        _logger.CatalogLoaded(languageTag, Policies.Count, sw.ElapsedMilliseconds);
    }

    private void IndexCategories()
    {
        _categoryIndex = new(StringComparer.OrdinalIgnoreCase);
        _categoryDisplayMap = new(StringComparer.OrdinalIgnoreCase);
        if (Catalog == null) return;
        // Build resource string table union for quick lookup
        var resourceStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var adml in Catalog.AdmlDocuments)
        {
            foreach (var kv in adml.StringTable)
            {
                resourceStrings[kv.Key.Value] = kv.Value;
            }
        }
        foreach (var doc in Catalog.AdmxDocuments)
        {
            foreach (var cat in doc.Categories)
            {
                _categoryIndex[cat.Id.Value] = cat;
                var token = cat.DisplayName.Id.Value;
                if (resourceStrings.TryGetValue(token, out var display))
                {
                    _categoryDisplayMap[cat.Id.Value] = display;
                }
                else
                {
                    _categoryDisplayMap[cat.Id.Value] = cat.Id.Value;
                }
            }
        }
    }

    public void ApplySearchFilter(string? query)
    {
        _lastSearch = query;
        if (Catalog == null)
        {
            FilteredPolicies.Clear();
            return;
        }
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
        UpdateGroupsForFilter();
    }

    private void BuildFileGroups()
    {
        PolicyFileGroups.Clear();
        if (Catalog == null) return;
        var summaryByName = Policies.GroupBy(p => p.Key.Name).ToDictionary(g => g.Key, g => g.First());
        foreach (var doc in Catalog.AdmxDocuments)
        {
            var path = doc.Lineage.SourceUri.LocalPath;
            var group = new PolicyFileGroup(Path.GetFileName(path), path);
            foreach (var pol in doc.Policies)
            {
                if (summaryByName.TryGetValue(pol.Key.Name, out var summary))
                {
                    group.Policies.Add(new PolicyItemViewModel(summary));
                }
            }
            if (group.Policies.Count > 0)
                PolicyFileGroups.Add(group);
        }
    }

    private void UpdateGroupsForFilter()
    {
        if (PolicyFileGroups.Count == 0) return;
        HashSet<PolicySummary> current = new(FilteredPolicies);
        foreach (var g in PolicyFileGroups)
        {
            int visible = 0;
            foreach (var p in g.Policies)
            {
                if (current.Contains(p.Summary)) visible++;
            }
            if (!string.IsNullOrWhiteSpace(_lastSearch))
                g.IsExpanded = visible > 0;
        }
    }

    private void RebuildCategoryRoots()
    {
        CategoryTree.Clear();
        if (Catalog == null || _categoryDisplayMap == null) return;
        bool hasFilter = !string.IsNullOrWhiteSpace(_lastSearch);
        HashSet<string>? allowedRootIds = null;
        if (hasFilter)
        {
            allowedRootIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in FilteredPolicies)
            {
                if (!string.IsNullOrEmpty(p.CategoryPath))
                {
                    var firstSeg = p.CategoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstSeg)) allowedRootIds.Add(firstSeg);
                }
            }
        }
        var roots = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent is null)
            .GroupBy(c => c.Id.Value)
            .Select(g => g.First())
            .Where(r => !hasFilter || (allowedRootIds != null && allowedRootIds.Contains(r.Id.Value)))
            .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase);
        foreach (var r in roots)
            CategoryTree.Add(new CategoryTreeItem(r, LocalizedCategoryName(r.Id.Value)));
    }

    private string LocalizedCategoryName(string id)
    {
        if (_categoryDisplayMap != null && _categoryDisplayMap.TryGetValue(id, out var name)) return name;
        return id;
    }

    public void EnsureCategoryChildren(CategoryTreeItem node)
    {
        if (Catalog == null) return;
        if (node.IsPolicy || node.Category is null) return;
        if (node.ChildrenMaterialized) return;

        node.Children.Clear();

        var childCats = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent.HasValue && c.Parent.Value.Id.Value == node.Category!.Id.Value)
            .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var c in childCats)
            node.Children.Add(new CategoryTreeItem(c, LocalizedCategoryName(c.Id.Value)));

        IEnumerable<PolicySummary> summariesSource = string.IsNullOrWhiteSpace(_lastSearch) ? Policies : FilteredPolicies;

        var policyMatches = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .Where(pol => pol.Category.Id.Value == node.Category!.Id.Value)
            .Join(summariesSource, pol => pol.Key.Name, s => s.Key.Name, (pol, s) => s)
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var p in policyMatches)
            node.Children.Add(new CategoryTreeItem(p));

        node.ChildrenMaterialized = true;
        if (node.Children.Count == 0)
            node.Children.Add(CategoryTreeItem.EmptyMarker());
        _logger.CategoryExpanded(node.Category!.Id.Value, childCats.Count, policyMatches.Count);
    }

    private void PopulateSelectedCategoryPolicies(CategoryTreeItem node)
    {
        SelectedCategoryPolicies.Clear();
        if (Catalog == null || node.Category == null) return;
        IEnumerable<PolicySummary> source = string.IsNullOrWhiteSpace(_lastSearch) ? Policies : FilteredPolicies;
        // Include policies in this category (not recursive for now)
        var polys = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .Where(p => p.Category.Id.Value == node.Category.Id.Value)
            .Join(source, pol => pol.Key.Name, s => s.Key.Name, (pol, s) => s)
            .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase);
        foreach (var p in polys) SelectedCategoryPolicies.Add(p);
    }

    private string? BuildBreadcrumb(Category? category)
    {
        if (category == null || _categoryIndex == null) return null;
        List<string> parts = new();
        var current = category;
        int guard = 0;
        while (current != null && guard < 64)
        {
            parts.Add(LocalizedCategoryName(current.Id.Value));
            guard++;
            if (current.Parent.HasValue && _categoryIndex.TryGetValue(current.Parent.Value.Id.Value, out var parentCat))
            {
                current = parentCat;
            }
            else current = null;
        }
        parts.Reverse();
        return string.Join(" > ", parts);
    }

    private async void OnSelectedPolicyChanged()
    {
        SelectedPolicySettings.Clear();
        if (Catalog == null || SelectedPolicy == null) return;

        var policy = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .FirstOrDefault(p => p.Key.Name == SelectedPolicy.Key.Name);
        if (policy == null)
        {
            _logger.LogWarning("Selected policy key {PolicyKey} not found in catalog documents", SelectedPolicy.Key.Name);
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