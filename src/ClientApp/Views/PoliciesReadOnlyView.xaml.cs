// Project Name: ClientApp
// File Name: PoliciesReadOnlyView.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows.Controls;

namespace KC.ITCompanion.ClientApp.Views;

public partial class PoliciesReadOnlyView : UserControl
{
    public PoliciesReadOnlyView()
    {
        InitializeComponent();
        var app = System.Windows.Application.Current as App;
        DataContext = app?.Services.GetService(typeof(PoliciesReadOnlyViewModel)) as PoliciesReadOnlyViewModel;
    }
}
