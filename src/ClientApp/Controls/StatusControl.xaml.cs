// Project Name: ClientApp
// File Name: StatusControl.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.ClientApp.Controls;

/// <summary>
/// Status tab user control hosting high level health, alerts, and drift indicators.
/// </summary>
public partial class StatusControl : UserControl
{
    /// <summary>
    /// Initializes the control and assigns its data context via DI.
    /// </summary>
    public StatusControl()
    {
        InitializeComponent();
        if (Application.Current is App app)
            DataContext = app.Services.GetService(typeof(StatusOverviewViewModel)) as StatusOverviewViewModel;
    }
}