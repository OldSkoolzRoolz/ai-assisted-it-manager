// Project Name: ClientApp
// File Name: PolicyEditorViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO; // for Path & Directory
using System.Windows.Input;

namespace ClientApp.ViewModels;

public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private readonly IAdmxCatalogLoader _loader;

    public ObservableCollection<AdmxCategory> Categories { get; } = [];
    public ObservableCollection<AdmxPolicy> Policies { get; } = [];
    public ObservableCollection<AdmxPolicy> FilteredPolicies { get; } = []; // retained
    public ObservableCollection<PolicySettingViewModel> CurrentSettings { get; } = [];
    public ObservableCollection<CategoryTreeItem> CategoryTree { get; } = [];

    private AdmxCatalog? _catalog;
    public AdmxCatalog? Catalog { get => _catalog; private set { _catalog = value; OnPropertyChanged(); } }

    private AdmxPolicy? _selectedPolicy;
    public AdmxPolicy? SelectedPolicy { get => _selectedPolicy; set { _selectedPolicy = value; OnPropertyChanged(); LoadSettingsForSelected(); } }

    private AdmxCategory? _selectedCategory;
    public AdmxCategory? SelectedCategory { get => _selectedCategory; set { if (_selectedCategory != value) { _selectedCategory = value; OnPropertyChanged(); SelectedPolicy = null; } } }

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
                BuildCategoryTree();
            }
        }
    }

    public ICommand LoadPoliciesCommand { get; }
    public ICommand SearchLocalPoliciesCommand { get; }

    public PolicyEditorViewModel(IAdmxCatalogLoader loader)
    {
        _loader = loader;
        LoadPoliciesCommand = new RelayCommand(async _ => await LoadDefaultSubsetAsync(), _ => true);
        SearchLocalPoliciesCommand = new RelayCommand(async _ => await SearchLocalPoliciesAsync(SearchText, CancellationToken.None), _ => true);
    }

    private async Task LoadDefaultSubsetAsync()
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string policyDef = Path.Combine(winDir, "PolicyDefinitions");
        if (Directory.Exists(policyDef))
        {
            List<string> admxFiles = Directory.GetFiles(policyDef, "*.admx").Take(25).ToList();
            await LoadCatalogAsync(admxFiles, "en-US", CancellationToken.None);
        }
    }

    public async Task LoadCatalogAsync(IEnumerable<string> paths, string? culture, CancellationToken token)
    {
        Result<AdmxCatalog> result = await _loader.LoadAsync(paths.ToList(), culture, token);
        if (!result.Success)
        {
            return;
        }

        Catalog = result.Value;
        Categories.Clear(); foreach (AdmxCategory? c in Catalog?.Categories.OrderBy(c => c.Name)!)
        {
            Categories.Add(c);
        }

        Policies.Clear(); foreach (AdmxPolicy? p in Catalog.Policies.OrderBy(p => p.Name))
        {
            Policies.Add(p);
        }

        ApplySearchFilter(_lastSearch);
        BuildCategoryTree();
    }

    public async Task SearchLocalPoliciesAsync(string? query, CancellationToken token)
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string policyDef = Path.Combine(winDir, "PolicyDefinitions");
        if (Directory.Exists(policyDef))
        {
            List<string> allFiles = Directory.GetFiles(policyDef, "*.admx").ToList();
            if (Catalog == null || Catalog.Policies.Count < allFiles.Count)
            {
                await LoadCatalogAsync(allFiles, "en-US", token);
            }
        }
        ApplySearchFilter(query);
        BuildCategoryTree();
    }

    public void ApplySearchFilter(string? query)
    {
        _lastSearch = query;
        FilteredPolicies.Clear();
        IEnumerable<AdmxPolicy> source = Policies;
        if (!string.IsNullOrWhiteSpace(query))
        {
            string q = query.Trim().ToLowerInvariant();
            source = source.Where(p => (p.Name?.ToLowerInvariant().Contains(q) ?? false) || (p.Id?.ToLowerInvariant().Contains(q) ?? false));
        }
        foreach (AdmxPolicy p in source)
        {
            FilteredPolicies.Add(p);
        }
    }

    private void BuildCategoryTree()
    {
        CategoryTree.Clear();
        if (Catalog == null)
        {
            return;
        }

        Dictionary<string, CategoryTreeItem> catLookup = Catalog.Categories.ToDictionary(c => c.Id, c => new CategoryTreeItem(c));
        foreach (CategoryTreeItem? cat in catLookup.Values)
        {
            if (!string.IsNullOrWhiteSpace(cat.Category!.ParentId) && catLookup.TryGetValue(cat.Category.ParentId!, out CategoryTreeItem? parent))
            {
                parent.Children.Add(cat);
            }
        }
        ObservableCollection<AdmxPolicy> policySource = FilteredPolicies.Any() || !string.IsNullOrWhiteSpace(_lastSearch) ? FilteredPolicies : Policies;
        foreach (AdmxPolicy p in policySource)
        {
            if (catLookup.TryGetValue(p.CategoryId, out CategoryTreeItem? catNode))
            {
                catNode.Children.Add(new CategoryTreeItem(p));
            }
        }
        foreach (CategoryTreeItem? cat in catLookup.Values.Where(c => string.IsNullOrWhiteSpace(c.Category!.ParentId)))
        {
            CategoryTree.Add(cat);
        }
    }

    private void LoadSettingsForSelected()
    {
        CurrentSettings.Clear();
        if (SelectedPolicy == null || Catalog == null)
        {
            return;
        }

        if (SelectedPolicy.Parts.Any())
        {
            foreach (PolicyPartDefinition part in SelectedPolicy.Parts)
            {
                CurrentSettings.Add(new PolicySettingViewModel(SelectedPolicy.Id, part, Catalog));
            }
        }
        else
        {
            CurrentSettings.Add(new PolicySettingViewModel(new PolicySetting(SelectedPolicy.Id, null, false, null, PolicyValueType.Boolean)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}