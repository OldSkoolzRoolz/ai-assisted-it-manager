// Project Name: CorePolicyEngine
// File Name: AdmxAdmlParser.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Xml.Linq;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.CorePolicyEngine.Parsing;

// Composite pair result for a single ADMX + ADML
public sealed record AdminTemplatePair(AdmxDocument Admx, AdmlDocument Adml);

// Aggregated catalog (multiple ADMX files consolidated)
public sealed record AdminTemplateCatalog(
    IReadOnlyList<AdmxDocument> AdmxDocuments,
    IReadOnlyList<AdmlDocument> AdmlDocuments,
    IReadOnlyList<PolicySummary> Summaries,
    IReadOnlyList<CategoryNode> CategoryTree);

public interface IAdminTemplateLoader
{
    Task<Result<AdminTemplatePair>> LoadAsync(string admxPath, string languageTag, CancellationToken cancellationToken);
    Task<Result<AdminTemplateCatalog>> LoadLocalCatalogAsync(string languageTag, int? maxFiles, CancellationToken cancellationToken);
}

/// <summary>
/// Minimal ADMX/ADML loader (phase 1) focused on building category/policy tree with localized names.
/// Parsing scope: categories + basic policy metadata (name, class, displayName, category path, supportedOn).
/// Element parsing & registry mappings intentionally deferred until later phase.
/// </summary>
public sealed class AdmxAdmlParser : IAdminTemplateLoader
{
    public async Task<Result<AdminTemplatePair>> LoadAsync(string admxPath, string languageTag, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(admxPath) || !File.Exists(admxPath))
            return Result<AdminTemplatePair>.Fail("ADMX file not found");

        try
        {
            // Parse ADMX first (structure + resource references)
            var admxXml = await File.ReadAllTextAsync(admxPath, cancellationToken);
            var admxDoc = XDocument.Parse(admxXml, LoadOptions.None);
            XNamespace ns = admxDoc.Root!.Name.Namespace;

            // Namespace binding (simplified)
            var mainNs = new NamespaceBinding("policy", new Uri(admxDoc.Root!.Attribute("xmlns")?.Value ?? "urn:admx"));

            List<Category> categories = new();
            List<Policy> policies = new();

            // Load ADML string table for localization
            var adml = await LoadAdmlForAdmxAsync(admxPath, languageTag, cancellationToken);
            var strings = adml.StringTable;

            // Categories
            foreach (var c in admxDoc.Root.Element(ns + "categories")?.Elements(ns + "category") ?? Enumerable.Empty<XElement>())
            {
                var name = c.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                var displayRaw = c.Attribute("displayName")?.Value;
                var _ = ResolveString(displayRaw, strings); // currently unused but triggers resolution
                CategoryRef? parentRef = null;
                var parentName = c.Attribute("parentCategory")?.Value;
                if (!string.IsNullOrWhiteSpace(parentName)) parentRef = new CategoryRef(new CategoryId(parentName!));

                categories.Add(new Category(
                    new CategoryId(name!),
                    new LocalizedRef(new ResourceId(displayRaw ?? name!)),
                    parentRef,
                    Lineage(admxPath))); // lineage
            }

            // Policies
            foreach (var p in admxDoc.Root.Element(ns + "policies")?.Elements(ns + "policy") ?? Enumerable.Empty<XElement>())
            {
                var name = p.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                var classAttr = p.Attribute("class")?.Value;
                var policyClass = classAttr == "User" ? PolicyClass.User : PolicyClass.Machine;
                var displayRaw = p.Attribute("displayName")?.Value;
                var displayRef = new LocalizedRef(new ResourceId(displayRaw ?? name!));
                LocalizedRef? explainRef = null;
                var explainRaw = p.Attribute("explainText")?.Value;
                if (!string.IsNullOrWhiteSpace(explainRaw)) explainRef = new LocalizedRef(new ResourceId(explainRaw!));
                var catName = p.Attribute("parentCategory")?.Value ?? string.Empty;
                var supportedOn = p.Attribute("supportedOn")?.Value;

                // Minimal element list (defer parsing child element types)
                List<PolicyElement> elements = new();

                policies.Add(new Policy(
                    new PolicyKey(mainNs.Uri, name!),
                    policyClass,
                    displayRef,
                    explainRef,
                    new CategoryRef(new CategoryId(catName)),
                    supportedOn is null ? null : new SupportId(supportedOn),
                    null,
                    elements,
                    new PolicyStateBehavior(PolicyDefaultState.NotConfigured, Array.Empty<RegistryAction>(), Array.Empty<RegistryAction>(), Array.Empty<RegistryAction>()),
                    Array.Empty<Tags>(),
                    new PolicyVersion(1, 0),
                    Lineage(admxPath)));
            }

            // Support + lineage placeholders
            AdmxDocument admx = new(
                new AdmxHeader("1.0", null, null, DateTimeOffset.UtcNow),
                mainNs,
                Array.Empty<NamespaceBinding>(),
                categories,
                Array.Empty<SupportDefinition>(),
                policies,
                Lineage(admxPath));

            return Result<AdminTemplatePair>.Ok(new AdminTemplatePair(admx, adml));
        }
        catch (Exception ex)
        {
            return Result<AdminTemplatePair>.Fail($"Parse error: {ex.Message}");
        }
    }

    public async Task<Result<AdminTemplateCatalog>> LoadLocalCatalogAsync(string languageTag, int? maxFiles, CancellationToken cancellationToken)
    {
        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string policyDir = Path.Combine(winDir, "PolicyDefinitions");
        if (!Directory.Exists(policyDir)) return Result<AdminTemplateCatalog>.Fail("PolicyDefinitions directory not found");

        var admxFiles = Directory.GetFiles(policyDir, "*.admx").OrderBy(f => f).Take(maxFiles ?? int.MaxValue).ToList();
        List<AdmxDocument> admxDocs = new();
        List<AdmlDocument> admlDocs = new();
        List<PolicySummary> summaries = new();

        foreach (var file in admxFiles)
        {
            var pairResult = await LoadAsync(file, languageTag, cancellationToken);
            if (!pairResult.Success || pairResult.Value is null) continue;
            admxDocs.Add(pairResult.Value.Admx);
            admlDocs.Add(pairResult.Value.Adml);
            summaries.AddRange(Materializer.Summarize(pairResult.Value.Admx, pairResult.Value.Adml));
        }

        // Build category tree from summaries
        var tree = BuildCategoryTree(summaries);
        return Result<AdminTemplateCatalog>.Ok(new AdminTemplateCatalog(admxDocs, admlDocs, summaries, tree));
    }

    private static IReadOnlyList<CategoryNode> BuildCategoryTree(IEnumerable<PolicySummary> summaries)
    {
        // Path segments split by '/'
        var rootDict = new Dictionary<string, CategoryNodeBuilder>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in summaries)
        {
            var segments = (s.CategoryPath ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
            CategoryNodeBuilder? current = null;
            string pathAccum = string.Empty;
            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i].Trim();
                pathAccum = pathAccum.Length == 0 ? seg : pathAccum + "/" + seg;
                if (current == null)
                {
                    if (!rootDict.TryGetValue(pathAccum, out current!))
                    {
                        current = new CategoryNodeBuilder(pathAccum, seg);
                        rootDict[pathAccum] = current;
                    }
                }
                else
                {
                    current = current.GetOrAddChild(pathAccum, seg);
                }
            }
            current?.Policies.Add(s);
        }

        // Return only top-level nodes (those without '/' in path) sorted
        return rootDict.Values.Where(v => !v.Path.Contains('/'))
            .OrderBy(v => v.Name)
            .Select(v => v.Build()).ToList();
    }

    private sealed class CategoryNodeBuilder
    {
        public string Path { get; }
        public string Name { get; }
        public Dictionary<string, CategoryNodeBuilder> Children { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<PolicySummary> Policies { get; } = new();

        public CategoryNodeBuilder(string path, string name) { Path = path; Name = name; }

        public CategoryNodeBuilder GetOrAddChild(string path, string name)
        {
            if (!Children.TryGetValue(path, out var child))
            {
                child = new CategoryNodeBuilder(path, name);
                Children[path] = child;
            }
            return child;
        }

        public CategoryNode Build() => new CategoryNode(
            Path,
            Name,
            Children.Values.OrderBy(c => c.Name).Select(c => c.Build()).ToList(),
            Policies.OrderBy(p => p.DisplayName).ToList());
    }

    private static AdmlDocument CreateEmptyAdml(string admxPath, string languageTag)
    {
        var lineage = Lineage(admxPath);
        return new AdmlDocument(
            new AdmlHeader("1.0", DateTimeOffset.UtcNow),
            new NamespaceBinding("policy", new Uri("urn:admx")),
            languageTag,
            new Dictionary<ResourceId, string>(),
            new Dictionary<string, PresentationTemplate>(),
            lineage);
    }

    private static async Task<AdmlDocument> LoadAdmlForAdmxAsync(string admxPath, string languageTag, CancellationToken ct)
    {
        try
        {
            var dir = Path.GetDirectoryName(admxPath)!;
            var stem = Path.GetFileNameWithoutExtension(admxPath);
            var admlPath = Path.Combine(dir, languageTag, stem + ".adml");
            if (!File.Exists(admlPath)) return CreateEmptyAdml(admxPath, languageTag);

            var xml = await File.ReadAllTextAsync(admlPath, ct);
            var doc = XDocument.Parse(xml, LoadOptions.None);
            Dictionary<ResourceId, string> stringTable = new();

            foreach (var s in doc.Descendants().Where(e => e.Name.LocalName == "string"))
            {
                var id = s.Attribute("id")?.Value;
                if (string.IsNullOrWhiteSpace(id)) continue;
                stringTable[new ResourceId(id!)] = s.Value?.Trim() ?? string.Empty;
            }

            return new AdmlDocument(
                new AdmlHeader("1.0", DateTimeOffset.UtcNow),
                new NamespaceBinding("policy", new Uri(doc.Root!.Attribute("xmlns")?.Value ?? "urn:admx")),
                languageTag,
                stringTable,
                new Dictionary<string, PresentationTemplate>(),
                Lineage(admxPath));
        }
        catch
        {
            return CreateEmptyAdml(admxPath, languageTag);
        }
    }

    private static string? ResolveString(string? rawToken, IReadOnlyDictionary<ResourceId, string> table)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return null;
        // ADMX token pattern: $(string.id)
        if (rawToken.StartsWith("$(string.") && rawToken.EndsWith(")"))
        {
            var id = rawToken.Substring(9, rawToken.Length - 10);
            var key = new ResourceId(id);
            if (table.TryGetValue(key, out var localized)) return localized;
        }
        return null;
    }

    private static DocumentLineage Lineage(string path) => new(new Uri(path), string.Empty, DateTimeOffset.UtcNow, null, null);
}