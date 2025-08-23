// Project Name: ITCompanionClient
// File Name: PolicyDetailDialog.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text; // FontWeights
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using KC.ITCompanion.CorePolicyEngine.Storage;

namespace ITCompanionClient.Controls;

public sealed class PolicyDetailDialog : ContentDialog, INotifyPropertyChanged
{
    private readonly IAuditWriter? _audit;
    private readonly AdminPolicy _policy;
    private bool _isEditMode;
    private readonly Dictionary<string, string?> _originalValues = new();

    public PolicyDetailDialog(AdminPolicy policy, string? description, IEnumerable<PolicyElement> elements)
    {
        _policy = policy;
        _audit = App.Services.GetService(typeof(IAuditWriter)) as IAuditWriter;
        Description = description;
        foreach (var e in elements)
        {
            var vm = new PolicySettingViewModel(policy, e);
            Settings.Add(vm);
            _originalValues[vm.PartId] = vm.Value;
        }
        var categoryPath = policy.Category.Id.Value;
        SelectedRow = new PolicySummaryProxy { DisplayName = policy.DisplayName?.Id.Value ?? policy.Key.Name, CategoryPath = categoryPath, Key = policy.Key };
        DataContext = this;
        Title = "Policy Detail";
        PrimaryButtonText = "Push";
        SecondaryButtonText = "Close";
        DefaultButton = ContentDialogButton.Primary;
        PrimaryButtonClick += OnPushClick;
        BuildContent();
    }

    private void BuildContent()
    {
        var root = new Grid { RowSpacing = 12, Padding = new Thickness(4), MinWidth = 640, MaxHeight = 640 };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Spacing = 2 };
        header.Children.Add(new TextBlock { Text = SelectedRow.DisplayName, FontSize = 20, FontWeight = FontWeights.SemiBold });
        header.Children.Add(new TextBlock { Text = SelectedRow.Key.Name, FontSize = 12, Opacity = 0.7 });
        header.Children.Add(new TextBlock { Text = SelectedRow.CategoryPath, FontSize = 12, Opacity = 0.7 });
        root.Children.Add(header);

        var scroll = new ScrollViewer();
        Grid.SetRow(scroll, 1);
        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(new TextBlock { Text = "Description", FontWeight = FontWeights.SemiBold });
        stack.Children.Add(new TextBlock { Text = Description, TextWrapping = TextWrapping.Wrap });
        var settingsHeader = new TextBlock { Text = "Settings", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 4) };
        stack.Children.Add(settingsHeader);
        var itemsStack = new StackPanel();
        foreach (var s in Settings)
        {
            var itemPanel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 8) };
            itemPanel.Children.Add(new TextBlock { Text = s.PartId, FontWeight = FontWeights.SemiBold });
            var tb = new TextBox { Text = s.Value, IsEnabled = IsEditMode };
            tb.TextChanged += (_, _) => s.Value = tb.Text;
            itemPanel.Children.Add(tb);
            itemsStack.Children.Add(itemPanel);
        }
        stack.Children.Add(itemsStack);
        scroll.Content = stack;
        root.Children.Add(scroll);

        var toggle = new ToggleSwitch { Header = "Edit Mode", IsOn = IsEditMode };
        toggle.Toggled += (_, __) => { IsEditMode = toggle.IsOn; foreach (var tb in itemsStack.Children.OfType<StackPanel>().SelectMany(p => p.Children.OfType<TextBox>())) tb.IsEnabled = IsEditMode; };
        Grid.SetRow(toggle, 2);
        root.Children.Add(toggle);
        Content = root;
    }

    /// <summary>Lightweight binding proxy for summary fields.</summary>
    public PolicySummaryProxy SelectedRow { get; }

    public string? Description { get; }

    public ObservableCollection<PolicySettingViewModel> Settings { get; } = new();

    /// <summary>Whether edit mode is enabled.</summary>
    public bool IsEditMode
    {
        get => _isEditMode;
        set { if (_isEditMode != value) { _isEditMode = value; OnPropertyChanged(); } }
    }

    private async void OnPushClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_audit == null) return;
        var changes = Settings.Where(s => _originalValues.TryGetValue(s.PartId, out var ov) ? ov != s.Value : true).ToList();
        foreach (var c in changes)
        {
            try { await _audit.PolicyEditedAsync(_policy.Key.Name, c.PartId, c.Value); } catch { }
        }
        try { await _audit.PolicyEditPushedAsync(changes.Count); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

/// <summary>
/// Proxy summary for binding without full PolicySummary dependency in WinUI layer.
/// </summary>
public sealed class PolicySummaryProxy
{
    public string? DisplayName { get; set; }
    public string? CategoryPath { get; set; }
    public PolicyKey Key { get; set; }
}
