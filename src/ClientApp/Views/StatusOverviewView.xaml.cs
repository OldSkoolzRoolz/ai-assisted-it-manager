// Project Name: ClientApp
// File Name: StatusOverviewView.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows.Controls;

namespace KC.ITCompanion.ClientApp.Views;

public partial class StatusOverviewView : UserControl
{
    public StatusOverviewView()
    {
        InitializeComponent();
        var app = System.Windows.Application.Current as App;
        DataContext = app?.Services.GetService(typeof(StatusOverviewViewModel)) as StatusOverviewViewModel;
    }
}
