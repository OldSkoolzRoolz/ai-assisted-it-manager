// Project Name: ClientApp
// File Name: ValueTypeToVisibilityConverter.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using System.Globalization;
using System.Windows.Data;


namespace ClientApp.Converters;


public sealed class ValueTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PolicyValueType vt)
        {
            // Only show textbox for non-boolean
            return vt == PolicyValueType.Boolean ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }





    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}