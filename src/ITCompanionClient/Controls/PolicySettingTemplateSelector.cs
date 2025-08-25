// Project Name: ITCompanionClient
// File Name: PolicySettingTemplateSelector.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.ClientShared;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ITCompanionClient.Controls;

/// <summary>WinUI DataTemplateSelector for PolicySettingViewModel entries.</summary>
public sealed class PolicySettingTemplateSelector : DataTemplateSelector
{
    /// <summary>Template used for free-form text input elements.</summary>
    public DataTemplate? TextTemplate { get; set; }
    /// <summary>Template used for enum selection elements.</summary>
    public DataTemplate? EnumTemplate { get; set; }
    /// <summary>Template used for boolean (checkbox) elements.</summary>
    public DataTemplate? BooleanTemplate { get; set; }
    /// <summary>Template used for numeric (decimal) elements.</summary>
    public DataTemplate? NumericTemplate { get; set; }

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item) => Select(item);
    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => Select(item);

    /// <summary>Internal selection helper mapping a setting view model to template.</summary>
    private DataTemplate? Select(object item)
    {
        if (item is PolicySettingViewModel vm)
        {
            return vm.ValueType switch
            {
                "Enum" => EnumTemplate,
                "Boolean" => BooleanTemplate,
                "Numeric" => NumericTemplate,
                _ => TextTemplate
            };
        }
        return TextTemplate;
    }
}
