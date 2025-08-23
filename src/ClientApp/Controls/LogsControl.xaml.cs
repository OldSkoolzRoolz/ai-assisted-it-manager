// Project Name: ClientApp
// File Name: LogsControl.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.ClientApp.Controls;

/// <summary>
/// Logs tab user control displaying aggregated log entries and health metrics.
/// </summary>
public partial class LogsControl : UserControl
{
    /// <summary>
    /// Initializes the control and resolves the log viewer view model from DI.
    /// </summary>
    public LogsControl()
    {
        InitializeComponent();
        if (Application.Current is App app)
            DataContext = app.Services.GetService(typeof(LogViewerViewModel)) as LogViewerViewModel;
    }
}