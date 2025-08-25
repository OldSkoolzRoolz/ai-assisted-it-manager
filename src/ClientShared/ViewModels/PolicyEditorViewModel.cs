// Project Name: ClientShared
// File Name: PolicyEditorViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KC.ITCompanion.CorePolicyEngine;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.ClientShared.Logging;
using System.Resources;
using KC.ITCompanion.ClientShared.Localization;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// ViewModel orchestrating policy catalog loading, filtering, navigation and selection.
/// NOTE: All ObservableCollection mutations must occur on the UI thread. Background loading operations
/// parse/construct data then dispatch to UI via <see cref="IUiDispatcher"/>. Do NOT mutate the collections
/// from background continuations.
/// </summary>
public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private const int MaxBreadcrumbDepth = 32;
    private readonly IAuditWriter _audit;
    private readonly IUiDispatcher _dispatcher;
    private readonly IAdminTemplateLoader _loader;
    private readonly ILogger<PolicyEditorViewModel> _logger;
    private readonly IPolicyGroupRepository? _policyGroups;
    private readonly IMessagePromptService? _prompt;
    private readonly ResourceManager _logRm = new("KC.ITCompanion.ClientShared.Resources.PolicyEditorLog", typeof(PolicyEditorViewModel).Assembly);
    private readonly ILocalizationService? _locService;

    private string? _breadcrumb;
    private AdminTemplateCatalog? _catalog;
    private Dictionary<string, string>? _categoryDisplayMap;
    private Dictionary<string, Category>? _categoryIndex;
    private string? _lastSearch;
    private string? _searchText;
    private PolicySummary? _selectedPolicy;
    private string? _categoryFilterId;
    private Dictionary<string, string>? _policyCategoryIdMap;

    /// <summary>Create a new policy editor ViewModel.</summary>
    public PolicyEditorViewModel(
        IAdminTemplateLoader loader,
        ILogger<PolicyEditorViewModel> logger,
        IAuditWriter audit,
        IUiDispatcher dispatcher,
        IMessagePromptService? prompt = null,
        IPolicyGroupRepository? policyGroupRepository = null,
        ILocalizationService? locService = null)
    {
        _loader = loader;
        _logger = logger;
        _audit = audit;
        _dispatcher = dispatcher;
        _prompt = prompt;
        _policyGroups = policyGroupRepository;
        _locService = locService;
        LoadPoliciesCommand = new RelayCommand(async _ => await LoadDefaultSubsetAsync(), _ => true);
        SearchLocalPoliciesCommand = new RelayCommand(_ => ApplySearchFilter(SearchText), _ => Catalog != null);
        RefreshPolicyGroupsCommand = new RelayCommand(async _ => await LoadPolicyGroupsAsync(), _ => _policyGroups != null);
        OpenSearchDialogCommand = new RelayCommand(_ => OnOpenSearchDialog(), _ => Catalog != null);
        OpenPolicyDetailCommand = new RelayCommand(p => OnOpenPolicyDetail(p as PolicyGridRow), p => p is PolicyGridRow);
        LogTemplate("PolicyEditorInitialized_Template");
    }

    private CultureInfo CurrentCulture => _locService?.CurrentUICulture ?? CultureInfo.CurrentUICulture;

    private void LogTemplate(string key, params object[] args)
    {
        try
        {
            var template = _logRm.GetString(key, CurrentCulture);
            if (string.IsNullOrEmpty(template)) return;
            string message = args.Length > 0 ? string.Format(CurrentCulture, template!, args) : template!;
            _logger.LogPolicyEditorTemplate(key, message);
        }
        catch { /* swallow logging failures */ }
    }

    /// <summary>All loaded policy summaries.</summary>
    public ObservableCollection<PolicySummary> Policies { get; } = new();
    /// <summary>Policies passing the current filters.</summary>
    public ObservableCollection<PolicySummary> FilteredPolicies { get; } = new();
    /// <summary>UI-bound collection of selected policy element settings.</summary>
    public ObservableCollection<PolicySettingViewModel> SelectedPolicySettings { get; } = new();
    /// <summary>Loaded policy groups (server-defined sets).</summary>
    public ObservableCollection<PolicyGroupDto> PolicyGroups { get; } = new();
    /// <summary>Grouping by ADMX file.</summary>
    public ObservableCollection<PolicyFileGroup> PolicyFileGroups { get; } = new();
    /// <summary>Hierarchical navigation levels (category tree).</summary>
    public ObservableCollection<CategoryNavLevel> CategoryLevels { get; } = new();
    /// <summary>Rows displayed for selected navigation node.</summary>
    public ObservableCollection<PolicyGridRow> CategoryPolicyRows { get; } = new();
    /// <summary>Visibility model for policy grid columns.</summary>
    public PolicyGridColumnVisibility ColumnVisibility { get; } = new();
    /// <summary>Flat root categories list for quick navigation.</summary>
    public ObservableCollection<CategoryListItem> CategoryListItems { get; } = new();

    /// <summary>Current loaded catalog or null until loaded.</summary>
    public AdminTemplateCatalog? Catalog
    {
        get => _catalog;
        // Setter no longer raises notifications directly; notifications occur after full initialization.
        private set => _catalog = value;
    }

    /// <summary>Number of ADMX files in catalog.</summary>
    public int PolicyFileCount => Catalog?.AdmxDocuments?.Count ?? 0;

    /// <summary>Breadcrumb path of currently selected navigation node.</summary>
    public string? Breadcrumb
    {
        get => _breadcrumb;
        private set { if (_breadcrumb != value) { _breadcrumb = value; OnPropertyChanged(); } }
    }

    /// <summary>Currently selected policy summary (for details pane).</summary>
    public PolicySummary? SelectedPolicy
    {
        get => _selectedPolicy;
        set { if (_selectedPolicy != value) { _selectedPolicy = value; OnSelectedPolicyChanged(); OnPropertyChanged(); } }
    }

    /// <summary>Search query string applied to name / key / category path.</summary>
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
                LogTemplate("SearchFilterApplied_Template", _searchText ?? string.Empty);
            }
        }
    }

    /// <summary>Currently selected category filter or null.</summary>
    public string? CategoryFilterId
    {
        get => _categoryFilterId;
        private set { if (_categoryFilterId != value) { _categoryFilterId = value; OnPropertyChanged(); ReapplyFilters(); } }
    }

    /// <summary>Command to load initial subset of policies.</summary>
    public ICommand LoadPoliciesCommand { get; }
    /// <summary>Command to apply search text filter.</summary>
    public ICommand SearchLocalPoliciesCommand { get; }
    /// <summary>Command to refresh policy groups from server.</summary>
    public ICommand RefreshPolicyGroupsCommand { get; }
    /// <summary>Command to open search dialog UI.</summary>
    public ICommand OpenSearchDialogCommand { get; }
    /// <summary>Command to open detail panel for a selected policy row.</summary>
    public ICommand OpenPolicyDetailCommand { get; }

    /// <summary>Raised when a policy row requests opening a detailed view.</summary>
    public event EventHandler<PolicyGridRow>? PolicyDetailRequested;
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Ensures catalog is loaded (idempotent) using default language.</summary>
    public async Task EnsureCatalogLoadedAsync()
    { if (Catalog != null) return; await LoadEntireCatalogAsync("en-US", CancellationToken.None); }

    private async Task LoadPolicyGroupsAsync()
    {
        if (_policyGroups == null) return;
        try
        {
            IReadOnlyList<PolicyGroupDto> groups = await _policyGroups.GetGroupsAsync(CancellationToken.None).ConfigureAwait(false);
            _dispatcher.Post(() =>
            {
                PolicyGroups.Clear();
                foreach (var g in groups) PolicyGroups.Add(g);
            });
        }
        catch (Exception ex)
        {
            LogTemplate("PolicyGroupsLoadFailed_Template");
            _logger.LogError(ex, "Unexpected exception loading policy groups.");
        }
    }

    private async Task LoadDefaultSubsetAsync()
    { await EnsureCatalogLoadedAsync(); await LoadPolicyGroupsAsync(); }

    private async Task LoadEntireCatalogAsync(string languageTag, CancellationToken token)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Result<AdminTemplateCatalog> result;
        try { result = await _loader.LoadLocalCatalogAsync(languageTag, 50, token).ConfigureAwait(false); }
        catch (Exception ex) { LogTemplate("CatalogLoadFailed_Template", languageTag); _logger.LogError(ex, "Unexpected exception loading catalog language {LanguageTag}", languageTag); return; }
        sw.Stop();
        if (!result.Success || result.Value is null) { LogTemplate("CatalogLoadFailed_Template", languageTag); return; }
        _dispatcher.Post(() =>
        {
            InitializeCatalog(result.Value);
            LogTemplate("CatalogLoaded_Template", languageTag, Policies.Count, sw.ElapsedMilliseconds);
        });
    }

    /// <summary>
    /// Fully initializes the ViewModel state from a loaded catalog before raising change notifications.
    /// This prevents observers from seeing partially constructed state. MUST run on UI thread.
    /// </summary>
    /// <param name="catalog">Loaded catalog (non-null).</param>
    private void InitializeCatalog(AdminTemplateCatalog catalog)
    {
        Catalog = catalog;

        // Build indices & maps first (no notifications yet).
        IndexCategories();
        _policyCategoryIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        Policies.Clear();
        foreach (var s in Catalog!.Summaries.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase))
            Policies.Add(s);

        foreach (var pol in Catalog.AdmxDocuments.SelectMany(d => d.Policies))
        {
            try
            {
                var catId = pol.Category.Id.Value;
                if (!string.IsNullOrWhiteSpace(catId))
                    _policyCategoryIdMap[pol.Key.Name] = catId;
            }
            catch
            {
                // Swallow per original intent; malformed policy entries ignored.
            }
        }

        FilteredPolicies.Clear();
        foreach (var p in Policies) FilteredPolicies.Add(p);

        BuildFileGroups();
        RebuildNavigation();
        Breadcrumb = null;

        // Now notify that Catalog + dependent counts changed.
        OnPropertyChanged(nameof(Catalog));
        OnPropertyChanged(nameof(PolicyFileCount));

        // Also ensure filters/groups reflect final state.
        UpdateGroupsForFilter();
    }

    /// <summary>
    /// Builds category index and localized display map. Must execute on UI thread because it mutates
    /// UI-bound collections (CategoryListItems). Defensive against malformed category metadata.
    /// </summary>
    private void IndexCategories()
    {
        _categoryIndex = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        _categoryDisplayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Catalog == null) return;
        Dictionary<string, string> resourceStrings = new(StringComparer.OrdinalIgnoreCase);
        foreach (var adml in Catalog.AdmlDocuments)
            foreach (var kv in adml.StringTable)
                if (!string.IsNullOrEmpty(kv.Key.Value))
                    resourceStrings[kv.Key.Value] = kv.Value;
        foreach (var doc in Catalog.AdmxDocuments)
            foreach (var cat in doc.Categories)
            {
                try
                {
                    var catId = cat.Id.Value; // Category.Id is value type; cannot be null, but value string may be empty.
                    if (string.IsNullOrWhiteSpace(catId)) { _logger.LogWarning("Skipping category with empty Id (source {Source})", doc.Lineage.SourceUri); continue; }
                    _categoryIndex[catId] = cat;
                    string? token = null;
                    if (cat.DisplayName != null)
                    {
                        var resId = cat.DisplayName.Id; // value type
                        token = resId.Value;
                    }
                    _categoryDisplayMap[catId] = !string.IsNullOrWhiteSpace(token) && resourceStrings.TryGetValue(token, out var display) ? display : catId;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed processing category (continuing). Source={Source}", doc.Lineage.SourceUri);
                }
            }
        // Populate flat category list for navigation pane
        CategoryListItems.Clear();
        var roots = Catalog.AdmxDocuments.SelectMany(d => d.Categories).Where(c => c.Parent is null)
            .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots)
            CategoryListItems.Add(new CategoryListItem(root.Id.Value, LocalizedCategoryName(root.Id.Value)));
    }

    private void BuildFileGroups()
    {
        PolicyFileGroups.Clear(); if (Catalog == null) return;
        var summaryByName = Policies.GroupBy(p => p.Key.Name).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        foreach (var doc in Catalog.AdmxDocuments)
        {
            var path = doc.Lineage.SourceUri.LocalPath;
            var group = new PolicyFileGroup(Path.GetFileName(path), path);
            foreach (var pol in doc.Policies)
                if (summaryByName.TryGetValue(pol.Key.Name, out var summary))
                    group.Policies.Add(new PolicyItemViewModel(summary));
            if (group.Policies.Count > 0) PolicyFileGroups.Add(group);
        }
    }

    private void UpdateGroupsForFilter()
    {
        if (PolicyFileGroups.Count == 0) return;
        var current = new HashSet<PolicySummary>(FilteredPolicies);
        foreach (var g in PolicyFileGroups)
        {
            var visible = g.Policies.Count(p => current.Contains(p.Summary));
            if (!string.IsNullOrWhiteSpace(_lastSearch)) g.IsExpanded = visible > 0;
        }
    }

    private void RebuildNavigation()
    {
        CategoryLevels.Clear(); if (Catalog == null) return;
        var rootLevel = new CategoryNavLevel(0);
        foreach (var root in Catalog.AdmxDocuments.SelectMany(d => d.Categories).Where(c => c.Parent is null)
                     .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase))
            rootLevel.Options.Add(new CategoryNavOption(root.Id.Value, LocalizedCategoryName(root.Id.Value), root));
        CategoryLevels.Add(rootLevel);
        if (rootLevel.Options.Count > 0) rootLevel.Selected = rootLevel.Options[0];
        BuildNextNavigationLevels();
        UpdatePolicyGridFromNavigation();
        foreach (var level in CategoryLevels)
            level.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CategoryNavLevel.Selected)) OnNavigationSelectionChanged(level); };
    }

    private void OnNavigationSelectionChanged(CategoryNavLevel changedLevel)
    {
        int idx = CategoryLevels.IndexOf(changedLevel);
        for (int i = CategoryLevels.Count - 1; i > idx; i--) CategoryLevels.RemoveAt(i);
        BuildNextNavigationLevels();
        UpdatePolicyGridFromNavigation();
    }

    private void BuildNextNavigationLevels()
    {
        if (Catalog == null) return;
        var deepest = CategoryLevels.Select(l => l.Selected).LastOrDefault(o => o != null);
        if (deepest == null) return;
        var childCats = Catalog.AdmxDocuments.SelectMany(d => d.Categories)
            .Where(c => c.Parent.HasValue && c.Parent.Value.Id.Value == deepest.Category.Id.Value)
            .OrderBy(c => LocalizedCategoryName(c.Id.Value), StringComparer.OrdinalIgnoreCase).ToList();
        if (childCats.Count == 0) return;
        var next = new CategoryNavLevel(CategoryLevels.Count);
        foreach (var c in childCats) next.Options.Add(new CategoryNavOption(c.Id.Value, LocalizedCategoryName(c.Id.Value), c));
        CategoryLevels.Add(next);
        next.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CategoryNavLevel.Selected)) OnNavigationSelectionChanged(next); };
    }

    private void UpdatePolicyGridFromNavigation()
    {
        CategoryPolicyRows.Clear(); if (Catalog == null) return;
        var target = CategoryLevels.Select(l => l.Selected).LastOrDefault(o => o != null);
        if (target == null) return;
        var summariesSource = string.IsNullOrWhiteSpace(_lastSearch) ? Policies : FilteredPolicies;
        var policies = Catalog.AdmxDocuments.SelectMany(d => d.Policies)
            .Where(p => p.Category.Id.Value == target.Id)
            .Join(summariesSource, pol => pol.Key.Name, s => s.Key.Name, (pol, s) => new { pol, s })
            .OrderBy(x => x.s.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
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
            if (adml.StringTable.TryGetValue(pol.ExplainText.Id, out var text)) return text;
        return pol.ExplainText.Id.Value;
    }

    private string LocalizedCategoryName(string id)
    {
        return _categoryDisplayMap != null && _categoryDisplayMap.TryGetValue(id, out var name) ? name : id;
    }

    private string? BuildBreadcrumb(Category? category)
    {
        if (category == null || _categoryIndex == null) return null;
        List<string> parts = new();
        var current = category; int guard = 0;
        while (current != null && guard < MaxBreadcrumbDepth)
        {
            parts.Add(LocalizedCategoryName(current.Id.Value));
            guard++;
            if (current.Parent.HasValue && _categoryIndex.TryGetValue(current.Parent.Value.Id.Value, out var parentCat)) current = parentCat; else current = null;
        }
        parts.Reverse(); return string.Join(" > ", parts);
    }

    private async void OnSelectedPolicyChanged()
    {
        SelectedPolicySettings.Clear();
        if (Catalog == null || SelectedPolicy == null) return;
        var policy = Catalog.AdmxDocuments.SelectMany(d => d.Policies).FirstOrDefault(p => p.Key.Name == SelectedPolicy.Key.Name);
        if (policy == null) { LogTemplate("PolicyKeyNotFound_Template", SelectedPolicy.Key.Name); return; }
        foreach (var element in policy.Elements) SelectedPolicySettings.Add(new PolicySettingViewModel(policy, element));
        LogTemplate("PolicySelected_Template", SelectedPolicy.Key.Name, SelectedPolicySettings.Count);
        try { await _audit.PolicySelectedAsync(SelectedPolicy.Key.Name).ConfigureAwait(false); } catch { }
    }

    private static void OnOpenSearchDialog()
    {
        // Host app can implement search dialog.
    }

    private void OnOpenPolicyDetail(PolicyGridRow? row)
    {
        if (row == null) return; SelectedPolicy = row.Summary; PolicyDetailRequested?.Invoke(this, row);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Apply a category filter by id (null clears). Thread-affine: UI thread recommended because triggers collection updates.
    /// </summary>
    /// <param name="categoryId">Category identifier or null/empty to clear.</param>
    public void SetCategoryFilter(string? categoryId)
    { CategoryFilterId = string.IsNullOrWhiteSpace(categoryId) ? null : categoryId; }

    /// <summary>
    /// Applies a text search filter (case-insensitive substring across DisplayName, Key, CategoryPath).
    /// Safe to call from UI thread; defers actual filtering to <see cref="ReapplyFilters"/>.
    /// </summary>
    /// <param name="query">Query text (null/empty clears search filter).</param>
    public void ApplySearchFilter(string? query)
    {
        _lastSearch = query;
        ReapplyFilters();
    }

    /// <summary>
    /// Recomputes the <see cref="FilteredPolicies"/> collection according to current search and category filters.
    /// Must execute on UI thread (mutates observable collections).
    /// </summary>
    public void ReapplyFilters()
    {
        if (Catalog == null) { FilteredPolicies.Clear(); return; }
        IEnumerable<PolicySummary> source = Policies;
        if (!string.IsNullOrWhiteSpace(_lastSearch))
        {
            var q = _lastSearch.Trim();
            source = source.Where(p => (!string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                                       p.Key.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                       (!string.IsNullOrEmpty(p.CategoryPath) && p.CategoryPath.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }
        if (!string.IsNullOrWhiteSpace(CategoryFilterId) && _categoryIndex != null && _policyCategoryIdMap != null)
        {
            var targetId = CategoryFilterId;
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_categoryIndex.TryGetValue(targetId, out var targetCat)) CollectCategoryAndChildren(targetCat, allowed);
            source = source.Where(p => _policyCategoryIdMap.TryGetValue(p.Key.Name, out var catId) && allowed.Contains(catId));
        }
        FilteredPolicies.Clear(); foreach (var p in source) FilteredPolicies.Add(p);
        UpdateGroupsForFilter();
        UpdatePolicyGridFromNavigation();
    }

    private void CollectCategoryAndChildren(Category cat, HashSet<string> set)
    {
        if (set.Contains(cat.Id.Value)) return; set.Add(cat.Id.Value);
        if (Catalog == null) return;
        foreach (var child in Catalog.AdmxDocuments.SelectMany(d => d.Categories).Where(c => c.Parent.HasValue && c.Parent.Value.Id.Value == cat.Id.Value))
            CollectCategoryAndChildren(child, set);
    }
}



/// <summary>Column visibility flags for policy grid.</summary>
public sealed class PolicyGridColumnVisibility : INotifyPropertyChanged
{
    private bool _name = true, _key = true, _scope = true, _category = true, _description = true, _supportedOn = true;
    /// <summary>Show Name column.</summary>
    public bool Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(); } } }
    /// <summary>Show Key column.</summary>
    public bool Key { get => _key; set { if (_key != value) { _key = value; OnPropertyChanged(); } } }
    /// <summary>Show Scope column.</summary>
    public bool Scope { get => _scope; set { if (_scope != value) { _scope = value; OnPropertyChanged(); } } }
    /// <summary>Show Category column.</summary>
    public bool Category { get => _category; set { if (_category != value) { _category = value; OnPropertyChanged(); } } }
    /// <summary>Show Description column.</summary>
    public bool Description { get => _description; set { if (_description != value) { _description = value; OnPropertyChanged(); } } }
    /// <summary>Show SupportedOn column.</summary>
    public bool SupportedOn { get => _supportedOn; set { if (_supportedOn != value) { _supportedOn = value; OnPropertyChanged(); } } }
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

/// <summary>Navigation level (column) in hierarchical category navigation.</summary>
public sealed class CategoryNavLevel : INotifyPropertyChanged
{
    /// <summary>Category options at this level.</summary>
    public ObservableCollection<CategoryNavOption> Options { get; } = new();
    private CategoryNavOption? _selected;
    /// <summary>Currently selected option.</summary>
    public CategoryNavOption? Selected { get => _selected; set { if (_selected != value) { _selected = value; OnPropertyChanged(); } } }
    /// <summary>Depth (root = 0).</summary>
    public int Depth { get; }
    /// <summary>Create level with depth.</summary>
    public CategoryNavLevel(int depth) { Depth = depth; }
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

/// <summary>Navigation option for a single category.</summary>
public sealed record CategoryNavOption(string Id, string Name, Category Category);

/// <summary>Grid row DTO for category policy listing.</summary>
public sealed class PolicyGridRow
{
    /// <summary>Display name (fallback to key).</summary>
    public required string Name { get; init; }
    /// <summary>Policy key name.</summary>
    public required string Key { get; init; }
    /// <summary>Policy scope (Machine/User/Both).</summary>
    public required string Scope { get; init; }
    /// <summary>Category path string.</summary>
    public required string CategoryPath { get; init; }
    /// <summary>Supported platform text.</summary>
    public string? SupportedOn { get; init; }
    /// <summary>Policy description / explain text (short form).</summary>
    public string? Description { get; init; }
    /// <summary>Underlying summary reference.</summary>
    public PolicySummary Summary { get; init; } = null!;
}

/// <summary>Simple list item for root category listing.</summary>
public sealed record CategoryListItem(string Id, string Name);

