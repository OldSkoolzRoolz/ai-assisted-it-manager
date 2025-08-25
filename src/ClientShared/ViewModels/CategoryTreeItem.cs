// Project Name: ClientShared
// File Name: CategoryTreeItem.cs
// Author: Ported from WPF ClientApp
// License: MIT
// Do not remove file headers

using System;
using System.Collections.ObjectModel;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// Represents either a category node or a policy leaf in the hierarchical tree.
/// </summary>
public class CategoryTreeItem
{
    /// <summary>Stable identifier (category id, policy key, or synthetic placeholder id).</summary>
    public string Id { get; }
    /// <summary>Display name (localized where available).</summary>
    public string Name { get; private set; }
    /// <summary>True if this item represents a policy leaf.</summary>
    public bool IsPolicy { get; }
    /// <summary>True if placeholder (loading / empty marker).</summary>
    public bool IsPlaceholder { get; }
    /// <summary>Underlying policy summary (if leaf).</summary>
    public PolicySummary? Policy { get; }
    /// <summary>Underlying category (if category node).</summary>
    public Category? Category { get; }
    /// <summary>Indicates children already materialized.</summary>
    public bool ChildrenMaterialized { get; set; }
    /// <summary>Child items (categories + policies).</summary>
    public ObservableCollection<CategoryTreeItem> Children { get; } = new();

    /// <summary>Creates an empty marker entry.</summary>
    public static CategoryTreeItem EmptyMarker() => new("__empty__", "(no items)", false, true);
    /// <summary>Creates a placeholder loading entry.</summary>
    public static CategoryTreeItem Placeholder() => new("__placeholder__", "(loading)", false, true);

    private CategoryTreeItem(string id, string name, bool isPolicy, bool isPlaceholder)
    { Id = id; Name = name; IsPolicy = isPolicy; IsPlaceholder = isPlaceholder; }

    /// <summary>Create category node item.</summary>
    public CategoryTreeItem(Category category, string displayName)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Category = category; Id = category.Id.Value; Name = displayName; IsPolicy = false;
        Children.Add(Placeholder());
    }

    /// <summary>Create policy leaf item.</summary>
    public CategoryTreeItem(PolicySummary policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        Id = policy.Key.Name; Name = policy.DisplayName; Policy = policy; IsPolicy = true;
    }

    /// <summary>Create node from pre-built navigation tree node aggregate.</summary>
    public CategoryTreeItem(CategoryNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Id = node.Path; Name = node.Name; IsPolicy = false; ChildrenMaterialized = true;
        foreach (var c in node.Children) Children.Add(new CategoryTreeItem(c));
        foreach (var p in node.Policies) Children.Add(new CategoryTreeItem(p));
    }
}
