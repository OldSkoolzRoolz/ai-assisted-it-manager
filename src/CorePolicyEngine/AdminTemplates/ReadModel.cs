// Project Name: CorePolicyEngine
// File Name: ReadModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved
// Do not remove file headers

using System;
using System.Collections.Generic;

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

/// <summary>
/// Read?model summary of a policy used for quick listing / search without loading full ADMX structures.
/// </summary>
/// <param name="Key">Stable policy identity (namespace + name).</param>
/// <param name="DisplayName">Resolved localized display name.</param>
/// <param name="ExplainText">Optional resolved explanation text.</param>
/// <param name="CategoryPath">Fully qualified category path ("Computer Configuration/..."), or "Uncategorized".</param>
/// <param name="Class">Policy class (machine / user / both).</param>
/// <param name="SupportedOn">Optional support definition id (resolved reference).</param>
/// <param name="ElementKinds">Collection of element kind descriptors for coarse filtering.</param>
/// <param name="IndexedAtUtc">UTC timestamp when the summary was materialized.</param>
public sealed record PolicySummary(
    PolicyKey Key,
    string DisplayName, // resolved via ADML
    string? ExplainText,
    string CategoryPath, // e.g., "Computer Configuration/Windows Components/..."
    PolicyClass Class,
    string? SupportedOn,
    IReadOnlyList<string> ElementKinds,
    DateTimeOffset IndexedAtUtc);

/// <summary>
/// Node in a category hierarchy tree plus the associated policies for display/navigation.
/// </summary>
/// <param name="Path">Full hierarchical path of the node.</param>
/// <param name="Name">Leaf name of this category.</param>
/// <param name="Children">Child categories.</param>
/// <param name="Policies">Policies directly under this category.</param>
public sealed record CategoryNode(
    string Path,
    string Name,
    IReadOnlyList<CategoryNode> Children,
    IReadOnlyList<PolicySummary> Policies);

/// <summary>
/// Materializes lightweight read models (summaries) by joining an ADMX definition document with its ADML resources.
/// </summary>
public static class Materializer
{
    /// <summary>
    /// Creates <see cref="PolicySummary"/> sequences from the supplied ADMX + ADML documents.
    /// </summary>
    /// <param name="admx">Parsed ADMX (definition) document.</param>
    /// <param name="adml">Parsed ADML (localization) document.</param>
    /// <returns>Enumeration of materialized summaries (evaluated lazily).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="admx"/> or <paramref name="adml"/> is null.</exception>
    public static IEnumerable<PolicySummary> Summarize(
        AdmxDocument admx,
        AdmlDocument adml)
    {
        ArgumentNullException.ThrowIfNull(admx);
        ArgumentNullException.ThrowIfNull(adml);

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

        foreach (AdminPolicy p in admx.Policies)
        {
            var display = strings.TryGetValue(p.DisplayName.Id, out var dn) ? dn : p.Key.Name;
            var explain = p.ExplainText is null ? null :
                strings.TryGetValue(p.ExplainText.Id, out var ex) ? ex : null;

            string catPath;
            try
            {
                catPath = ResolveCatPath(p.Category);
            }
            catch (KeyNotFoundException)
            {
                catPath = "Uncategorized";
            }
            catch (InvalidOperationException)
            {
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