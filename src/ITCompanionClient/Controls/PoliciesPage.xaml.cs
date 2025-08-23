using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine.Parsing;

namespace ITCompanionClient.Controls;
/// <summary>
/// Policies list page (WinUI) – minimal subset while port in progress.
/// </summary>
public sealed partial class PoliciesPage : UserControl
{
    private readonly PoliciesPageViewModel _vm;
    private List<PolicySummary> _all = new();

    /// <summary>Creates the page and starts async load when loaded.</summary>
    public PoliciesPage()
    {
        this.InitializeComponent();
        var loader = App.Services.GetService(typeof(IAdminTemplateLoader)) as IAdminTemplateLoader;
        _vm = new PoliciesPageViewModel(loader!);
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadingRing.IsActive = true;
        await _vm.LoadAsync();
        LoadingRing.IsActive = false;
        if (!string.IsNullOrWhiteSpace(_vm.Error))
        {
            ErrorText.Text = _vm.Error;
            ErrorText.Visibility = Visibility.Visible;
            return;
        }
        _all = _vm.Policies.ToList();
        PoliciesList.ItemsSource = _all;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        if (_all.Count == 0) return;
        var q = SearchBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(q))
        {
            PoliciesList.ItemsSource = _all;
            return;
        }
        PoliciesList.ItemsSource = _all.Where(p =>
            (!string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
            p.Key.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(p.CategoryPath) && p.CategoryPath.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private async void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not PolicySummary summary) return;
        var loader = App.Services.GetService(typeof(IAdminTemplateLoader)) as IAdminTemplateLoader;
        if (loader == null) return;
        var result = await loader.LoadLocalCatalogAsync("en-US", 40, CancellationToken.None).ConfigureAwait(false);
        if (!result.Success || result.Value == null) return;
        var fullPolicy = result.Value.AdmxDocuments.SelectMany(d => d.Policies).FirstOrDefault(p => p.Key.Name == summary.Key.Name);
        if (fullPolicy == null) return;
        string? explain = null;
        if (fullPolicy.ExplainText != null)
        {
            foreach (var adml in result.Value.AdmlDocuments)
                if (adml.StringTable.TryGetValue(fullPolicy.ExplainText.Id, out var text)) { explain = text; break; }
        }
        var dialog = new PolicyDetailDialog(fullPolicy, explain, fullPolicy.Elements) { XamlRoot = this.XamlRoot };
        await dialog.ShowAsync();
    }
}
