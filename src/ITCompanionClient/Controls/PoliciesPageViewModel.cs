using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine.Parsing;

namespace ITCompanionClient.Controls;
/// <summary>
/// Lightweight WinUI-specific view model to surface policies until full shared extraction is completed.
/// </summary>
internal sealed class PoliciesPageViewModel : INotifyPropertyChanged
{
    private readonly IAdminTemplateLoader _loader;
    private bool _isLoading;
    private string? _error;

    /// <summary>Creates a new instance.</summary>
    public PoliciesPageViewModel(IAdminTemplateLoader loader)
    {
        _loader = loader;
    }

    /// <summary>List of loaded policy summaries.</summary>
    public ObservableCollection<PolicySummary> Policies { get; } = new();

    /// <summary>Indicates whether the catalog is loading.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set { if (_isLoading != value) { _isLoading = value; OnPropertyChanged(); } }
    }

    /// <summary>Any transient load error.</summary>
    public string? Error
    {
        get => _error;
        private set { if (_error != value) { _error = value; OnPropertyChanged(); } }
    }

    /// <summary>Load a subset of ADMX files and populate policy summaries.</summary>
    public async Task LoadAsync()
    {
        if (IsLoading || Policies.Count > 0) return;
        IsLoading = true;
        Error = null;
        try
        {
            var result = await _loader.LoadLocalCatalogAsync("en-US", 40, CancellationToken.None).ConfigureAwait(false);
            if (!result.Success || result.Value == null)
            {
                Error = "Failed to load policy catalog.";
                return;
            }
            foreach (var s in result.Value.Summaries.OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase))
                Policies.Add(s);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
