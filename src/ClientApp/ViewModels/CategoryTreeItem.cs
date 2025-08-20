using System.Collections.ObjectModel;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientApp.ViewModels;

public class CategoryTreeItem
{
    public string Id { get; }
    public string Name { get; private set; }
    public bool IsPolicy { get; }
    public bool IsPlaceholder { get; }
    public PolicySummary? Policy { get; }
    public Category? Category { get; }
    public bool ChildrenMaterialized { get; set; }
    public ObservableCollection<CategoryTreeItem> Children { get; } = [];

    // Public helper factory for empty marker
    public static CategoryTreeItem EmptyMarker() => new CategoryTreeItem("__empty__", "(no items)", false, true);
    public static CategoryTreeItem Placeholder() => new CategoryTreeItem("__placeholder__", "(loading)", false, true);

    // Internal base ctor used by factories
    public CategoryTreeItem(string id, string name, bool isPolicy, bool isPlaceholder)
    {
        Id = id;
        Name = name;
        IsPolicy = isPolicy;
        IsPlaceholder = isPlaceholder;
    }

    // Category node constructor (new raw category based tree)
    public CategoryTreeItem(Category category, string displayName)
    {
        Category = category;
        Id = category.Id.Value;
        Name = displayName;
        IsPolicy = false;
        // Add a dummy child so WPF shows expand arrow; real children populated lazily
        Children.Add(Placeholder());
    }

    // Policy leaf constructor
    public CategoryTreeItem(PolicySummary policy)
    {
        Id = policy.Key.Name;
        Name = policy.DisplayName;
        Policy = policy;
        IsPolicy = true;
    }

    // Legacy constructor (from precomputed CategoryNode) retained temporarily for compatibility
    public CategoryTreeItem(CategoryNode categoryNode)
    {
        Id = categoryNode.Path;
        Name = categoryNode.Name;
        IsPolicy = false;
        foreach (var child in categoryNode.Children)
            Children.Add(new CategoryTreeItem(child));
        foreach (var p in categoryNode.Policies)
            Children.Add(new CategoryTreeItem(p));
        ChildrenMaterialized = true;
    }
}
