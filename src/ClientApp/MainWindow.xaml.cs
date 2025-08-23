// Project Name: ClientApp
// File Name: MainWindow.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.ClientApp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;

namespace KC.ITCompanion.ClientApp;

/// <summary>
/// Primary application window hosting tabbed navigation, menu, and status bar.
/// </summary>
public partial class MainWindow : Window
{
    private IThemeService? _themeService;

    /// <summary>
    /// Constructs the main window and wires the Loaded event.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Resolves services needed post construction (theme service).
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current is App app)
        {
            _themeService = app.Services.GetService<IThemeService>();
            UpdateThemeMenuChecks(_themeService?.Current ?? Services.AppTheme.Auto);
        }
    }

    /// <summary>
    /// Handles Exit menu click.
    /// </summary>
    private void OnExit(object sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Applies a selected theme from the View->Theme submenu.
    /// </summary>
    private void OnThemeSelect(object sender, RoutedEventArgs e)
    {
        if (_themeService == null) return;
        if (sender is MenuItem mi && mi.Tag is string tag)
        {
            if (Enum.TryParse<AppTheme>(tag, out var theme))
            {
                _themeService.Apply(theme, force: true);
                UpdateThemeMenuChecks(theme);
            }
        }
    }

    /// <summary>
    /// Opens the policy search dialog by invoking the PoliciesControl private method.
    /// </summary>
    private void OnOpenSearch(object sender, RoutedEventArgs e)
    {
        var policiesControlField = this.GetType().GetField("PoliciesControl", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var ctrl = policiesControlField?.GetValue(this) as FrameworkElement;
        ctrl ??= LogicalTreeHelper.FindLogicalNode(this, "PoliciesControl") as FrameworkElement;
        var mi = ctrl?.GetType().GetMethod("OpenSearchDialog", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        mi?.Invoke(ctrl, null);
    }

    /// <summary>
    /// Updates checkmarks on the theme submenu items to reflect the active theme.
    /// </summary>
    private void UpdateThemeMenuChecks(AppTheme active)
    {
        foreach (var item in FindLogicalChildren<MenuItem>(this))
        {
            if (item.Tag is string tag && Enum.TryParse<AppTheme>(tag, out var t))
                item.IsChecked = t == active;
        }
    }

    /// <summary>
    /// Breadth-first logical tree traversal yielding controls of a type.
    /// </summary>
    private static IEnumerable<T> FindLogicalChildren<T>(DependencyObject root) where T : DependencyObject
    {
        if (root == null) yield break;
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in LogicalTreeHelper.GetChildren(current))
            {
                if (child is DependencyObject dep)
                {
                    queue.Enqueue(dep);
                    if (dep is T t) yield return t;
                }
            }
        }
    }
}