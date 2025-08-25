using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ITCompanionClient.Controls;
using KC.ITCompanion.ClientShared.Localization;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Resources;

namespace ITCompanionClient;
public sealed partial class MainWindow : Window
{
    private readonly ILocalizationService _loc;
    private string[] _cultures = [];
    private static readonly ResourceManager ShellRm = new("KC.ITCompanion.ClientShared.Resources.Shell", typeof(MainWindow).Assembly);

    public MainWindow()
    {
        InitializeComponent();
        _loc = App.Services.GetRequiredService<ILocalizationService>();
        LoadCultures();
        CultureCombo.ItemsSource = _cultures;
        CultureCombo.SelectedItem = _loc.CurrentUICulture.Name;
        RootNav.SelectedItem = RootNav.MenuItems[0];
        ContentFrame.Content = new StatusView();
    }

    private void LoadCultures()
    {
        var discovered = CultureCatalog.Discover(ShellRm, "Nav_Status");
        if (!discovered.Contains("en-US"))
        {
            var list = discovered.ToList();
            list.Add("en-US");
            discovered = list;
        }
        _cultures = discovered.Distinct().OrderBy(c => c, System.StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem nvi || nvi.Tag is not string tag) return;
        ContentFrame.Content = tag switch
        {
            "status" => new StatusView(),
            "policies" => new PoliciesPage(),
            _ => new TextBlock { Text = $"{tag} view pending port", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center }
        };
    }

    private void OnCultureSelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void OnApplyLanguage(object sender, RoutedEventArgs e)
    {
        if (CultureCombo.SelectedItem is string culture && culture != _loc.CurrentUICulture.Name)
        {
            _loc.ChangeCulture(culture);
            if (ContentFrame.Content is StatusView) ContentFrame.Content = new StatusView();
            else if (ContentFrame.Content is PoliciesPage) ContentFrame.Content = new PoliciesPage();
        }
    }
}
