// Project Name: ClientShared
// File Name: CategoryTreeItem.cs
// Author: Ported from WPF ClientApp
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// Represents either a category node or a policy leaf in the hierarchical tree.
/// </summary>
public class CategoryTreeItem
{
    public string Id { get; }
    public string Name { get; private set; }
    public bool IsPolicy { get; }
    public bool IsPlaceholder { get; }
    public PolicySummary? Policy { get; }
    public Category? Category { get; }
    public bool ChildrenMaterialized { get; set; }
    public ObservableCollection<CategoryTreeItem> Children { get; } = new();

    public static CategoryTreeItem EmptyMarker() => new("__empty__", "(no items)", false, true);
    public static CategoryTreeItem Placeholder() => new("__placeholder__", "(loading)", false, true);

    private CategoryTreeItem(string id, string name, bool isPolicy, bool isPlaceholder)
    { Id = id; Name = name; IsPolicy = isPolicy; IsPlaceholder = isPlaceholder; }

    public CategoryTreeItem(Category category, string displayName)
    {
        Category = category; Id = category.Id.Value; Name = displayName; IsPolicy = false;
        Children.Add(Placeholder());
    }

    public CategoryTreeItem(PolicySummary policy)
    { Id = policy.Key.Name; Name = policy.DisplayName; Policy = policy; IsPolicy = true; }

    public CategoryTreeItem(CategoryNode node)
    {
        Id = node.Path; Name = node.Name; IsPolicy = false; ChildrenMaterialized = true;
        foreach (var c in node.Children) Children.Add(new CategoryTreeItem(c));
        foreach (var p in node.Policies) Children.Add(new CategoryTreeItem(p));
    }
}
