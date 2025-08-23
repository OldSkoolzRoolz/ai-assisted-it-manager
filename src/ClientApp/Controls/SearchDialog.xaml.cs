// Project Name: ClientApp
// File Name: SearchDialog.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.ClientApp.Controls;

/// <summary>
/// Modal dialog used to capture policy search options.
/// </summary>
public partial class SearchDialog : Window
{
    /// <summary>
    /// Initializes dialog with provided view model.
    /// </summary>
    public SearchDialog(object vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}