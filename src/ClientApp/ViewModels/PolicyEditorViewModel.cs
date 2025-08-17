using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CorePolicyEngine.Parsing;
using Shared;

namespace ClientApp.ViewModels;

public class PolicyEditorViewModel : INotifyPropertyChanged
{
    private readonly IAdmxCatalogLoader _loader;

    public ObservableCollection<AdmxCategory> Categories { get; } = new();
    public ObservableCollection<AdmxPolicy> Policies { get; } = new();
    public ObservableCollection<PolicySettingViewModel> CurrentSettings { get; } = new();

    private AdmxCatalog? _catalog;
    public AdmxCatalog? Catalog { get => _catalog; private set { _catalog = value; OnPropertyChanged(); } }

    private AdmxPolicy? _selectedPolicy;
    public AdmxPolicy? SelectedPolicy { get => _selectedPolicy; set { _selectedPolicy = value; OnPropertyChanged(); LoadSettingsForSelected(); } }

    public PolicyEditorViewModel(IAdmxCatalogLoader loader)
    {
        _loader = loader;
    }

    public async Task LoadCatalogAsync(IEnumerable<string> paths, string? culture, CancellationToken token)
    {
        var result = await _loader.LoadAsync(paths.ToList(), culture, token);
        if (!result.Success) return; // TODO surface errors
        Catalog = result.Value;
        Categories.Clear(); foreach (var c in this.Catalog?.Categories.OrderBy(c=>c.Name)!) Categories.Add(c);
        Policies.Clear(); foreach (var p in Catalog.Policies.OrderBy(p=>p.Name)) Policies.Add(p);
    }

    private void LoadSettingsForSelected()
    {
        CurrentSettings.Clear();
        if (SelectedPolicy == null) return;
        foreach (var part in SelectedPolicy.Parts)
        {
            var vm = new PolicySettingViewModel(new PolicySetting(SelectedPolicy.Id, part.Id, false, null, part.ValueType));
            CurrentSettings.Add(vm);
        }
        if (!SelectedPolicy.Parts.Any())
        {
            CurrentSettings.Add(new PolicySettingViewModel(new PolicySetting(SelectedPolicy.Id, null, false, null, PolicyValueType.Boolean)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name=null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
