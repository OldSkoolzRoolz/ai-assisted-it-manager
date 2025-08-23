// Project Name: ITCompanionClient
// File Name: BoolToVisibilityConverter.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ITCompanionClient.Controls;

/// <summary>Converts a boolean to Visibility.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}
