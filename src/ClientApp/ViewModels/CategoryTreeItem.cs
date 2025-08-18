using System.Collections.ObjectModel;

namespace ClientApp.ViewModels;

public class CategoryTreeItem
{
    public string Id { get; }
    public string Name { get; }
    public bool IsPolicy { get; }
    public AdmxPolicy? Policy { get; }
    public AdmxCategory? Category { get; }
    public ObservableCollection<CategoryTreeItem> Children { get; } = [];

    public CategoryTreeItem(AdmxCategory category)
    {
        Id = category.Id; Name = category.Name; Category = category; IsPolicy = false;
    }
    public CategoryTreeItem(AdmxPolicy policy)
    {
        Id = policy.Id; Name = policy.Name; Policy = policy; IsPolicy = true;
    }
}
