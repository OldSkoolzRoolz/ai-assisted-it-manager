// Project Name: ClientApp
// File Name: PolicyEditorViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KC.ITCompanion.CorePolicyEngine;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;
using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientApp.ViewModels;

public sealed class PolicyGridColumnVisibility : INotifyPropertyChanged
{
    private bool _name = true;
    private bool _key = true;
    private bool _scope = true;
    private bool _category = true;
    private bool _description = true;
    private bool _supportedOn = true;

    public bool Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(); } } }
    public bool Key { get => _key; set { if (_key != value) { _key = value; OnPropertyChanged(); } } }
    public bool Scope { get => _scope; set { if (_scope != value) { _scope = value; OnPropertyChanged(); } } }
    public bool Category { get => _category; set { if (_category != value) { _category = value; OnPropertyChanged(); } } }
    public bool Description { get => _description; set { if (_description != value) { _description = value; OnPropertyChanged(); } } }
    public bool SupportedOn { get => _supportedOn; set { if (_supportedOn != value) { _supportedOn = value; OnPropertyChanged(); } } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public sealed class CategoryNavLevel : INotifyPropertyChanged
{
    public ObservableCollection<CategoryNavOption> Options { get; } = [];
    private CategoryNavOption? _selected;
    public CategoryNavOption? Selected { get => _selected; set { if (_selected != value) { _selected = value; OnPropertyChanged(); } } }
    public int Depth { get; }
    public CategoryNavLevel(int depth) { Depth = depth; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public sealed record CategoryNavOption(string Id, string Name, Category Category);

public sealed class PolicyGridRow
{
    public required string Name { get; init; }
    public required string Key { get; init; }
    public required string Scope { get; init; }
    public required string CategoryPath { get; init; }
    public string? SupportedOn { get; init; }
    public string? Description { get; init; }
    public PolicySummary Summary { get; init; } = null!; // underlying summary for potential future actions
}

public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private const int MaxBreadcrumbDepth = 32; // prevents runaway loops building breadcrumb
    private readonly IAuditWriter _audit;
    private readonly IAdminTemplateLoader _loader;
    private readonly ILogger<PolicyEditorViewModel> _logger;
    private readonly IPolicyGroupRepository? _policyGroups;

    private string? _breadcrumb;
    private AdminTemplateCatalog? _catalog;
    private Dictionary<string, string>? _categoryDisplayMap; // categoryId -> localized display name
    private Dictionary<string, Category>? _categoryIndex; // categoryId -> category
    private string? _lastSearch;
    private string? _searchText;
    private PolicySummary? _selectedPolicy;

    public PolicyEditorViewModel(IAdminTemplateLoader loader, ILogger<PolicyEditorViewModel> logger, IAuditWriter audit,
        IPolicyGroupRepository? policyGroupRepository = null)
    {
        _loader = loader;
        _logger = logger;
        _audit = audit;
        _policyGroups = policyGroupRepository;
        LoadPoliciesCommand = new RelayCommand(async _ => await LoadDefaultSubsetAsync(), _ => true);
        SearchLocalPoliciesCommand = new RelayCommand(_ => ApplySearchFilter(SearchText), _ => Catalog != null);
        RefreshPolicyGroupsCommand = new RelayCommand(async _ => await LoadPolicyGroupsAsync(), _ => _policyGroups != null);
        OpenSearchDialogCommand = new RelayCommand(_ => OnOpenSearchDialog(), _ => Catalog != null);
        OpenPolicyDetailCommand = new RelayCommand(p => OnOpenPolicyDetail(p as PolicyGridRow), p => p is PolicyGridRow);
        _logger.Initialized();
    }

    public ObservableCollection<PolicySummary> Policies { get; } = [];
    public ObservableCollection<PolicySummary> FilteredPolicies { get; } = [];
    public ObservableCollection<PolicySettingViewModel> SelectedPolicySettings { get; } = [];
    public ObservableCollection<PolicyGroupDto> PolicyGroups { get; } = [];
    public ObservableCollection<PolicyFileGroup> PolicyFileGroups { get; } = [];

    // Navigation levels & grid rows
    public ObservableCollection<CategoryNavLevel> CategoryLevels { get; } = [];
    public ObservableCollection<PolicyGridRow> CategoryPolicyRows { get; } = [];
    public PolicyGridColumnVisibility ColumnVisibility { get; } = new();

    public AdminTemplateCatalog? Catalog
    {
        get => _catalog;
        private set
        {
            _catalog = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PolicyFileCount));
        }
    }

    public int PolicyFileCount => Catalog?.AdmxDocuments.Count ?? 0;

    public string? Breadcrumb
    {
        get => _breadcrumb;
        private set
        {
            if (_breadcrumb != value)
            {
                _breadcrumb = value;
                OnPropertyChanged();
            }
        }
    }

    public PolicySummary? SelectedPolicy
    {
        get => _selectedPolicy;
        set
        {
            if (_selectedPolicy != value)
            {
                _selectedPolicy = value;
                OnSelectedPolicyChanged();
                OnPropertyChanged();
            }
        }
    }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplySearchFilter(_searchText);
                RebuildNavigation();
                _logger.SearchFilterApplied(_searchText ?? string.Empty);
            }
        }
    }

    public ICommand LoadPoliciesCommand { get; }
    public ICommand SearchLocalPoliciesCommand { get; }
    public ICommand RefreshPolicyGroupsCommand { get; }
    public ICommand OpenSearchDialogCommand { get; }
    public ICommand OpenPolicyDetailCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

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
            IReadOnlyList<PolicyGroupDto> groups = await _policyGroups.GetGroupsAsync(CancellationToken.None);
            App.Current.Dispatcher.Invoke(() =>
            {
                PolicyGroups.Clear();
                foreach (PolicyGroupDto g in groups) PolicyGroups.Add(g);
            });
        }
        catch (Exception ex)
        {
            PolicyEditorViewModelErrorLogs.LoadPolicyGroupsFailed(_logger, ex); // CA1848 fixed
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
            result = await _loader.LoadLocalCatalogAsync(languageTag, 50, token);
        }
        catch (Exception ex)
        {
            PolicyEditorViewModelErrorLogs.CatalogUnexpectedError(_logger, ex, languageTag); // CA1848
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
        foreach (PolicySummary s in Catalog.Summaries.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)) Policies.Add(s);
        FilteredPolicies.Clear();
        foreach (PolicySummary p in Policies) FilteredPolicies.Add(p);
        BuildFileGroups();
        RebuildNavigation();
        Breadcrumb = null;
        _logger.CatalogLoaded(languageTag, Policies.Count, sw.ElapsedMilliseconds);
    }

    private void IndexCategories()
    {
        _categoryIndex = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        _categoryDisplayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Catalog == null) return;
        Dictionary<string, string> resourceStrings = new(StringComparer.OrdinalIgnoreCase);
        foreach (AdmlDocument adml in Catalog.AdmlDocuments)
            foreach (KeyValuePair<ResourceId, string> kv in adml.StringTable)
                resourceStrings[kv.Key.Value] = kv.Value;

        foreach (AdmxDocument doc in Catalog.AdmxDocuments)
            foreach (Category cat in doc.Categories)
            {
                _categoryIndex[cat.Id.Value] = cat;
                var token = cat.DisplayName.Id.Value;
                if (resourceStrings.TryGetValue(token, out var display))
                    _categoryDisplayMap[cat.Id.Value] = display;
                else
                    _categoryDisplayMap[cat.Id.Value] = cat.Id.Value;
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
            var q = query.Trim();
            source = source.Where(p =>
                (!string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                p.Key.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(p.CategoryPath) && p.CategoryPath.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (PolicySummary p in source) FilteredPolicies.Add(p);
        UpdateGroupsForFilter();
        UpdatePolicyGridFromNavigation();
    }

    private void BuildFileGroups()
    {
        PolicyFileGroups.Clear();
        if (Catalog == null) return;
        Dictionary<string, PolicySummary> summaryByName =
            Policies.GroupBy(p => p.Key.Name).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        foreach (AdmxDocument doc in Catalog.AdmxDocuments)
        {
            var path = doc.Lineage.SourceUri.LocalPath;
            var group = new PolicyFileGroup(Path.GetFileName(path), path);
            foreach (AdminPolicy pol in doc.Policies)
                if (summaryByName.TryGetValue(pol.Key.Name, out PolicySummary? summary))
                    group.Policies.Add(new PolicyItemViewModel(summary));
            if (group.Policies.Count > 0) PolicyFileGroups.Add(group);
        }
    }

    private void UpdateGroupsForFilter()
    {
        if (PolicyFileGroups.Count == 0) return;
        HashSet<PolicySummary> current = new(FilteredPolicies);
        foreach (PolicyFileGroup g in PolicyFileGroups)
        {
            var visible = 0;
            foreach (PolicyItemViewModel p in g.Policies)
                if (current.Contains(p.Summary))
                    visible++;
            if (!string.IsNullOrWhiteSpace(_lastSearch))
                g.IsExpanded = visible > 0;
        }
    }

    private void RebuildNavigation()
    {
        CategoryLevels.Clear();
        if (Catalog == null) return;
        var rootLevel = new CategoryNavLevel(0);
        foreach (var root in Catalog.AdmxDocuments.SelectMany(d => d.Categories).Where(c => c.Parent is null)
                     .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase))
            rootLevel.Options.Add(new CategoryNavOption(root.Id.Value, LocalizedCategoryName(root.Id.Value), root));
        CategoryLevels.Add(rootLevel);
        if (rootLevel.Options.Count > 0)
            rootLevel.Selected = rootLevel.Options[0];
        BuildNextNavigationLevels();
        UpdatePolicyGridFromNavigation();
        foreach (var level in CategoryLevels)
            level.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CategoryNavLevel.Selected)) OnNavigationSelectionChanged(level); };
    }

    private void OnNavigationSelectionChanged(CategoryNavLevel changedLevel)
    {
        int idx = CategoryLevels.IndexOf(changedLevel);
        for (int i = CategoryLevels.Count - 1; i > idx; i--)
            CategoryLevels.RemoveAt(i);
        BuildNextNavigationLevels();
        UpdatePolicyGridFromNavigation();
    }

    private void BuildNextNavigationLevels()
    {
        if (Catalog == null) return;
        CategoryNavOption? deepest = CategoryLevels.Select(l => l.Selected).LastOrDefault(o => o != null);
        if (deepest == null) return;
        var childCats = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent.HasValue && c.Parent.Value.Id.Value == deepest.Category.Id.Value)
            .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (childCats.Count == 0) return;
        var next = new CategoryNavLevel(CategoryLevels.Count);
        foreach (var c in childCats)
            next.Options.Add(new CategoryNavOption(c.Id.Value, LocalizedCategoryName(c.Id.Value), c));
        CategoryLevels.Add(next);
        next.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CategoryNavLevel.Selected)) OnNavigationSelectionChanged(next); };
    }

    private void UpdatePolicyGridFromNavigation()
    {
        CategoryPolicyRows.Clear();
        if (Catalog == null) return;
        CategoryNavOption? target = CategoryLevels.Select(l => l.Selected).LastOrDefault(o => o != null);
        if (target == null) return;
        var summariesSource = string.IsNullOrWhiteSpace(_lastSearch) ? Policies : FilteredPolicies;
        var policies = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .Where(p => p.Category.Id.Value == target.Id)
            .Join(summariesSource, pol => pol.Key.Name, s => s.Key.Name, (pol, s) => new { pol, s })
            .OrderBy(x => x.s.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var x in policies)
        {
            CategoryPolicyRows.Add(new PolicyGridRow
            {
                Name = x.s.DisplayName ?? x.s.Key.Name,
                Key = x.s.Key.Name,
                Scope = x.pol.Class.ToString(),
                CategoryPath = x.s.CategoryPath ?? string.Empty,
                Description = ResolveExplainText(x.pol),
                SupportedOn = x.pol.SupportedOn?.Value,
                Summary = x.s
            });
        }
        Breadcrumb = BuildBreadcrumb(target.Category);
    }

    private string? ResolveExplainText(AdminPolicy pol)
    {
        if (pol.ExplainText == null || Catalog == null) return null;
        foreach (var adml in Catalog.AdmlDocuments)
            if (adml.StringTable.TryGetValue(pol.ExplainText.Id, out var text))
                return text;
        return pol.ExplainText.Id.Value;
    }

    private string LocalizedCategoryName(string id)
    {
        if (_categoryDisplayMap != null && _categoryDisplayMap.TryGetValue(id, out var name)) return name;
        return id;
    }

    private string? BuildBreadcrumb(Category? category)
    {
        if (category == null || _categoryIndex == null) return null;
        List<string> parts = new();
        Category? current = category;
        var guard = 0;
        while (current != null && guard < MaxBreadcrumbDepth)
        {
            parts.Add(LocalizedCategoryName(current.Id.Value));
            guard++;
            if (current.Parent.HasValue &&
                _categoryIndex.TryGetValue(current.Parent.Value.Id.Value, out Category? parentCat))
                current = parentCat;
            else current = null;
        }

        parts.Reverse();
        return string.Join(" > ", parts);
    }

    private async void OnSelectedPolicyChanged()
    {
        SelectedPolicySettings.Clear();
        if (Catalog == null || SelectedPolicy == null) return;

        AdminPolicy? policy = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .FirstOrDefault(p => p.Key.Name == SelectedPolicy.Key.Name);
        if (policy == null)
        {
            PolicyEditorViewModelErrorLogs.SelectedPolicyMissing(_logger, SelectedPolicy.Key.Name); // CA1848
            return;
        }

        foreach (PolicyElement element in policy.Elements)
            SelectedPolicySettings.Add(new PolicySettingViewModel(policy, element));
        _logger.PolicySelected(SelectedPolicy.Key.Name, SelectedPolicySettings.Count);
        try
        {
            await _audit.PolicySelectedAsync(SelectedPolicy.Key.Name);
        }
        catch { }
    }

    private void OnOpenSearchDialog()
    {
        // Placeholder: search dialog orchestration to be implemented
    }

    private void OnOpenPolicyDetail(PolicyGridRow? row)
    {
        if (row == null) return;
        SelectedPolicy = row.Summary;
        PolicyDetailRequested?.Invoke(this, row);
    }
    public event EventHandler<PolicyGridRow>? PolicyDetailRequested;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}