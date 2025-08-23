// Project Name: ClientApp
// File Name: InverseBoolConverter.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Globalization;
using System.Windows.Data;

namespace KC.ITCompanion.ClientApp.Converters;

/// <summary>
/// Inverts a boolean value (true->false, false->true).
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    /// <summary>Converts a boolean to its inverse.</summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    /// <summary>Converts back a boolean to its inverse.</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}