// Project Name: ClientApp
// File Name: LogAggregateView.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows.Controls;
using KC.ITCompanion.ClientApp.ViewModels;

namespace KC.ITCompanion.ClientApp.Views;

public partial class LogAggregateView : UserControl
{
    public LogAggregateView()
    {
        InitializeComponent();
        var app = System.Windows.Application.Current as App;
        DataContext = app?.Services.GetService(typeof(LogViewerViewModel)) as LogViewerViewModel;
    }
}
