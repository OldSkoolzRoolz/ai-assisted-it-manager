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

        // Pre-build name + lookup maps so we can validate refs and avoid rebuilding per policy
        var catNames = new Dictionary<CategoryId, string>();
        foreach (Category c in admx.Categories)
            catNames[c.Id] = strings.TryGetValue(c.DisplayName.Id, out var n) ? n : c.Id.Value;

        var catById = new Dictionary<CategoryId, Category>();
        foreach (Category c in admx.Categories)
            catById[c.Id] = c;

        string ResolveCatPath(CategoryRef r)
        {
            // Treat empty / whitespace IDs as uncategorized
            if (string.IsNullOrWhiteSpace(r.Id.Value)) return "Uncategorized";
            if (!catById.TryGetValue(r.Id, out var cur))
                return string.IsNullOrWhiteSpace(r.Id.Value) ? "Uncategorized" : r.Id.Value; // fallback to raw id to surface unknown category

            var path = new Stack<string>();
            int guard = 0; // cycle protection
            while (true)
            {
                if (++guard > 100)
                {
                    path.Push("[Cycle]");
                    break; // protect against malformed circular parent chains
                }

                path.Push(catNames.TryGetValue(cur.Id, out var name) ? name : cur.Id.Value);
                if (cur.Parent is null) break;
                // If parent ref missing from index, terminate with fallback marker
                if (!catById.TryGetValue(cur.Parent.Value.Id, out cur))
                {
                    path.Push("Uncategorized");
                    break;
                }
            }
            return string.Join("/", path);
        }

        foreach (Policy p in admx.Policies)
        {
            var display = strings.TryGetValue(p.DisplayName.Id, out var dn) ? dn : p.Key.Name;
            var explain = p.ExplainText is null ? null :
                strings.TryGetValue(p.ExplainText.Id, out var ex) ? ex : null;

            string catPath;
            try
            {
                catPath = ResolveCatPath(p.Category);
            }
            catch
            {
                // Absolute safety net – should no longer happen, but prevents single failure from halting enumeration
                catPath = "Uncategorized";
            }

            yield return new PolicySummary(
                p.Key,
                display,
                explain,
                catPath,
                p.Class,
                p.SupportedOn?.Value,
                ElementKinds(p.Elements),
                DateTimeOffset.UtcNow);
        }

        static IReadOnlyList<string> ElementKinds(IReadOnlyList<PolicyElement> elements)
        {
            var list = new List<string>(elements.Count);
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