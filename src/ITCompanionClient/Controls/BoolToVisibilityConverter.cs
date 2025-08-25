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
    /// <summary>Converts a boolean value to a <see cref="Visibility"/> (true => Visible).</summary>
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    /// <summary>Converts a <see cref="Visibility"/> back to a boolean (Visible => true).</summary>
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => value is Visibility v && v == Visibility.Visible;
}
