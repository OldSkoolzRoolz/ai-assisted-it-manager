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
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? EnumTemplate { get; set; }
    public DataTemplate? BooleanTemplate { get; set; }
    public DataTemplate? NumericTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
        => Select(item);
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        => Select(item);

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
