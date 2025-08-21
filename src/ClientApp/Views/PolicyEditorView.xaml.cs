// Project Name: ClientApp
// File Name: PolicyEditorView.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows;
using System.Windows.Controls;
using KC.ITCompanion.ClientApp.ViewModels;

namespace KC.ITCompanion.ClientApp.Views;

public partial class PolicyEditorView : UserControl
{
    public PolicyEditorView()
    {
        InitializeComponent();
        var app = Application.Current as App;
        ViewModel = app?.Services.GetService(typeof(PolicyEditorViewModel)) as PolicyEditorViewModel ??
            throw new InvalidOperationException("PolicyEditorViewModel not registered");
        Loaded += OnLoaded;
        DataContext = ViewModel;
    }

    public PolicyEditorViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.Policies.Any())
        {
            await ViewModel.SearchLocalPoliciesAsync(null, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private void OnOpenDevDiag(object sender, RoutedEventArgs e)
    {
        var app = Application.Current as App;
        var devVm = app?.Services.GetService(typeof(DevDiagnosticsViewModel)) as DevDiagnosticsViewModel;
        if (devVm != null)
        {
            var win = new DevDiagnosticsWindow { DataContext = devVm };
            win.Show();
        }
    }

    private void OnOpenLogs(object sender, RoutedEventArgs e)
    {
        var app = Application.Current as App;
        var logVm = app?.Services.GetService(typeof(LogViewerViewModel)) as LogViewerViewModel;
        if (logVm != null)
        {
            var win = new Window
            {
                Title = "Logs",
                Width = 1000,
                Height = 600,
                Content = new LogViewerView { DataContext = logVm }
            };
            win.Show();
        }
    }
}