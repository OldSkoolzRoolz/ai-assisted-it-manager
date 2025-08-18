// Project Name: CorePolicyEngine
// File Name: ReadModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers



// Project Name: CorePolicyEngine
// File Name: ReadModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace CorePolicyEngine.AdminTemplates;


public sealed record PolicySummary(
    PolicyKey Key,
    string DisplayName, // resolved via ADML
    string? ExplainText,
    string CategoryPath, // e.g., "Computer Configuration/Windows Components/..."
    PolicyClass Class,
    string? SupportedOn,
    IReadOnlyList<string> ElementKinds,
    DateTimeOffset IndexedAtUtc);



public sealed record CategoryNode(
    string Path,
    string Name,
    IReadOnlyList<CategoryNode> Children,
    IReadOnlyList<PolicySummary> Policies);



public static class Materializer
{
    // Example: create summaries by joining ADMX + ADML
    public static IEnumerable<PolicySummary> Summarize(
        AdmxDocument admx,
        AdmlDocument adml)
    {
        var strings = adml.StringTable;
        Dictionary<CategoryId, string> catNames = [];
        foreach (Category c in admx.Categories)
            catNames[c.Id] = strings.TryGetValue(c.DisplayName.Id, out var n) ? n : c.Id.Value;

        string ResolveCatPath(CategoryRef r)
        {
            Stack<string> path = new();
            Dictionary<CategoryId, Category> byId = [];
            foreach (Category c in admx.Categories) byId[c.Id] = c;

            Category cur = byId[r.Id];
            while (true)
            {
                path.Push(catNames[cur.Id]);
                if (cur.Parent is null) break;

                cur = byId[cur.Parent.Value.Id];
            }

            return string.Join("/", path);
        }

        foreach (Policy p in admx.Policies)
        {
            var display = strings.TryGetValue(p.DisplayName.Id, out var dn) ? dn : p.Key.Name;
            var explain = p.ExplainText is null ? null :
                strings.TryGetValue(p.ExplainText.Id, out var ex) ? ex : null;

            yield return new PolicySummary(
                p.Key,
                display,
                explain,
                ResolveCatPath(p.Category),
                p.Class,
                p.SupportedOn?.Value,
                ElementKinds(p.Elements),
                DateTimeOffset.UtcNow);
        }

        static IReadOnlyList<string> ElementKinds(IReadOnlyList<PolicyElement> elements)
        {
            List<string> list = new(elements.Count);
            foreach (PolicyElement e in elements)
                list.Add(e switch
                {
                    BooleanElement => "Boolean",
                    DecimalElement => "Decimal",
                    TextElement => "Text",
                    MultiTextElement => "MultiText",
                    EnumElement => "Enum",
                    _ => "Unknown"
                });

            return list;
        }
    }
}