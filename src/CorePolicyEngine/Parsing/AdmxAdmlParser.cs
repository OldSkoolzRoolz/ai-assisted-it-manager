// Project Name: CorePolicyEngine
// File Name: AdmxAdmlParser.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Globalization;
using System.Xml.Linq;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using System.Text.RegularExpressions;

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
/// ADMX/ADML loader building domain models close to Microsoft schemas (subset).
/// Implemented: categories, policies (metadata + subset elements), supportedOn definitions (basic), presentations.
/// Deferred: registry value mapping, list elements, complex supported-on logic evaluation.
/// </summary>
public sealed class AdmxAdmlParser : IAdminTemplateLoader
{
    private static readonly Regex StringToken = new("^\\$\\(string\\.(?<id>[A-Za-z0-9_]+)\\)$", RegexOptions.Compiled);
    private static readonly Regex PresentationToken = new("^\\$\\(presentation\\.(?<id>[A-Za-z0-9_]+)\\)$", RegexOptions.Compiled);

    public async Task<Result<AdminTemplatePair>> LoadAsync(string admxPath, string languageTag, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(admxPath) || !File.Exists(admxPath))
            return Result<AdminTemplatePair>.Fail("ADMX file not found");

        try
        {
            var admxXml = await File.ReadAllTextAsync(admxPath, cancellationToken);
            var admxDoc = XDocument.Parse(admxXml, LoadOptions.None);
            XNamespace ns = admxDoc.Root!.Name.Namespace;

            var mainNs = new NamespaceBinding("policy", new Uri(admxDoc.Root!.Attribute("xmlns")?.Value ?? "urn:admx"));

            // Prepare ADML
            var adml = await LoadAdmlForAdmxAsync(admxPath, languageTag, cancellationToken);
            var strings = adml.StringTable;

            var categories = ParseCategories(admxDoc, ns, admxPath, strings).ToList();
            var supportDefinitions = ParseSupportDefinitions(admxDoc, ns, admxPath).ToList();
            var policies = ParsePolicies(admxDoc, ns, admxPath, strings, mainNs).ToList();

            var admx = new AdmxDocument(
                new AdmxHeader("1.0", admxDoc.Root.Attribute("revision")?.Value, null, DateTimeOffset.UtcNow),
                mainNs,
                Array.Empty<NamespaceBinding>(),
                categories,
                supportDefinitions,
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

        var tree = BuildCategoryTree(summaries);
        return Result<AdminTemplateCatalog>.Ok(new AdminTemplateCatalog(admxDocs, admlDocs, summaries, tree));
    }

    private static IEnumerable<Category> ParseCategories(XDocument doc, XNamespace ns, string path, IReadOnlyDictionary<ResourceId, string> strings)
    {
        foreach (var c in doc.Root!.Element(ns + "categories")?.Elements(ns + "category") ?? Enumerable.Empty<XElement>())
        {
            var name = c.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(name)) continue;
            var displayRaw = c.Attribute("displayName")?.Value;
            var parentCategoryElement = c.Element(ns + "parentCategory");
            var parentRefName = parentCategoryElement?.Attribute("ref")?.Value ?? c.Attribute("parentCategory")?.Value;
            CategoryRef? parentRef = string.IsNullOrWhiteSpace(parentRefName) ? null : new CategoryRef(new CategoryId(parentRefName!));
            yield return new Category(
                new CategoryId(name!),
                new LocalizedRef(new ResourceId(displayRaw ?? name!)),
                parentRef,
                Lineage(path));
        }
    }

    private static IEnumerable<SupportDefinition> ParseSupportDefinitions(XDocument doc, XNamespace ns, string path)
    {
        var container = doc.Root!.Element(ns + "supportedOn");
        if (container is null) yield break;

        // Products
        foreach (var products in container.Elements(ns + "products"))
        {
            foreach (var prod in products.Elements(ns + "product"))
            {
                var name = prod.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                var display = prod.Attribute("displayName")?.Value; // token expected
                yield return new SupportDefinition(
                    new SupportId(name!),
                    new LocalizedRef(new ResourceId(display ?? name!)),
                    Array.Empty<SupportProduct>(),
                    Lineage(path));
            }
        }

        // Complex definitions
        foreach (var defs in container.Elements(ns + "definitions"))
        {
            foreach (var def in defs.Elements(ns + "definition"))
            {
                var name = def.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                var display = def.Attribute("displayName")?.Value;
                // Ranges / references inside <or>/<and> not materialized yet
                yield return new SupportDefinition(
                    new SupportId(name!),
                    new LocalizedRef(new ResourceId(display ?? name!)),
                    Array.Empty<SupportProduct>(),
                    Lineage(path));
            }
        }
    }

    private static IEnumerable<Policy> ParsePolicies(XDocument doc, XNamespace ns, string path, IReadOnlyDictionary<ResourceId, string> strings, NamespaceBinding nsBinding)
    {
        foreach (var p in doc.Root!.Element(ns + "policies")?.Elements(ns + "policy") ?? Enumerable.Empty<XElement>())
        {
            var name = p.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(name)) continue;
            var classAttr = p.Attribute("class")?.Value;
            PolicyClass policyClass = classAttr switch
            {
                "User" => PolicyClass.User,
                "Machine" => PolicyClass.Machine,
                "Both" => PolicyClass.Both,
                _ => PolicyClass.Machine
            };
            var displayRaw = p.Attribute("displayName")?.Value;
            LocalizedRef? explain = null;
            var explainRaw = p.Attribute("explainText")?.Value;
            if (!string.IsNullOrWhiteSpace(explainRaw)) explain = new LocalizedRef(new ResourceId(explainRaw!));
            var catElement = p.Element(ns + "parentCategory");
            var catName = catElement?.Attribute("ref")?.Value ?? p.Attribute("parentCategory")?.Value ?? string.Empty;
            var supportedOnElement = p.Element(ns + "supportedOn");
            var supportedOnRef = supportedOnElement?.Attribute("ref")?.Value ?? p.Attribute("supportedOn")?.Value;

            // Presentation attribute pattern $(presentation.id)
            PresentationRef? presentationRef = null;
            var presAttr = p.Attribute("presentation")?.Value;
            if (!string.IsNullOrWhiteSpace(presAttr))
            {
                var m = PresentationToken.Match(presAttr);
                if (m.Success)
                {
                    presentationRef = new PresentationRef(m.Groups["id"].Value);
                }
            }

            // Elements parsing (subset) from <elements>
            List<PolicyElement> elements = new();
            var elementsNode = p.Element(ns + "elements");
            if (elementsNode != null)
            {
                foreach (var el in elementsNode.Elements())
                {
                    var id = el.Attribute("id")?.Value;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    var elId = new ElementId(id!);
                    switch (el.Name.LocalName)
                    {
                        case "boolean":
                            elements.Add(new BooleanElement(elId, null, Array.Empty<RegistryAction>(), Array.Empty<RegistryAction>()));
                            break;
                        case "decimal":
                            long? min = TryParseLong(el.Attribute("minValue")?.Value);
                            long? max = TryParseLong(el.Attribute("maxValue")?.Value);
                            elements.Add(new DecimalElement(elId, null, min, max, Array.Empty<RegistryActionTemplate<long>>()));
                            break;
                        case "longDecimal":
                            long? lmin = TryParseLong(el.Attribute("minValue")?.Value);
                            long? lmax = TryParseLong(el.Attribute("maxValue")?.Value);
                            elements.Add(new DecimalElement(elId, null, lmin, lmax, Array.Empty<RegistryActionTemplate<long>>()));
                            break;
                        case "text":
                            int? maxLen = TryParseInt(el.Attribute("maxLength")?.Value);
                            elements.Add(new TextElement(elId, null, null, maxLen, Array.Empty<RegistryActionTemplate<string>>()));
                            break;
                        case "multiText":
                            int? mtMaxLen = TryParseInt(el.Attribute("maxLength")?.Value);
                            int? mtStrings = TryParseInt(el.Attribute("maxStrings")?.Value);
                            elements.Add(new MultiTextElement(elId, null, mtStrings, mtMaxLen, Array.Empty<RegistryActionTemplate<IReadOnlyList<string>>>()));
                            break;
                        case "enum":
                            List<EnumItem> enumItems = new();
                            foreach (var item in el.Elements(ns + "item"))
                            {
                                var disp = item.Attribute("displayName")?.Value;
                                enumItems.Add(new EnumItem(item.Attribute("displayName")?.Value ?? string.Empty, disp is null ? null : new LocalizedRef(new ResourceId(disp)), Array.Empty<RegistryAction>()));
                            }
                            elements.Add(new EnumElement(elId, null, enumItems));
                            break;
                        default:
                            break; // unsupported element types for now
                    }
                }
            }

            yield return new Policy(
                new PolicyKey(nsBinding.Uri, name!),
                policyClass,
                new LocalizedRef(new ResourceId(displayRaw ?? name!)),
                explain,
                new CategoryRef(new CategoryId(catName)),
                supportedOnRef is null ? null : new SupportId(supportedOnRef),
                presentationRef,
                elements,
                new PolicyStateBehavior(PolicyDefaultState.NotConfigured, Array.Empty<RegistryAction>(), Array.Empty<RegistryAction>(), Array.Empty<RegistryAction>()),
                Array.Empty<Tags>(),
                new PolicyVersion(1, 0),
                Lineage(path));
        }
    }

    private static long? TryParseLong(string? v) => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : null;
    private static int? TryParseInt(string? v) => int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : null;

    private static AdmlDocument CreateEmptyAdml(string admxPath, string languageTag)
    {
        var lineage = Lineage(admxPath);
        return new AdmlDocument(
            new AdmlHeader("1.0", DateTimeOffset.UtcNow),
            new NamespaceBinding("policy", new Uri("urn:admx")),
            languageTag,
            new Dictionary<ResourceId, string>(),
            new Dictionary<string, AdmlPresentation>(),
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
            Dictionary<string, AdmlPresentation> presentations = new(StringComparer.OrdinalIgnoreCase);

            foreach (var s in doc.Descendants().Where(e => e.Name.LocalName == "string"))
            {
                var id = s.Attribute("id")?.Value;
                if (string.IsNullOrWhiteSpace(id)) continue;
                stringTable[new ResourceId(id!)] = s.Value?.Trim() ?? string.Empty;
            }

            foreach (var pres in doc.Descendants().Where(e => e.Name.LocalName == "presentation"))
            {
                var idAttr = pres.Attribute("id")?.Value;
                if (string.IsNullOrWhiteSpace(idAttr)) continue;
                List<PresentationElement> parts = new();
                foreach (var child in pres.Elements())
                {
                    var kind = child.Name.LocalName;
                    switch (kind)
                    {
                        case "decimalTextBox":
                        case "longDecimalTextBox":
                        case "checkBox":
                        case "textBox":
                        case "multiTextBox":
                        case "comboBox":
                        case "dropdownList":
                        case "listBox":
                            var refId = child.Attribute("refId")?.Value ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(refId)) continue;
                            var pe = BuildPresentationElement(kind, refId, child, stringTable);
                            parts.Add(pe);
                            break;
                        case "text":
                            break; // ignored free text
                        default:
                            break;
                    }
                }
                presentations[idAttr] = new AdmlPresentation(idAttr, parts);
            }

            return new AdmlDocument(
                new AdmlHeader("1.0", DateTimeOffset.UtcNow),
                new NamespaceBinding("policy", new Uri(doc.Root!.Attribute("xmlns")?.Value ?? "urn:admx")),
                languageTag,
                stringTable,
                presentations,
                Lineage(admxPath));
        }
        catch
        {
            return CreateEmptyAdml(admxPath, languageTag);
        }
    }

    private static PresentationElement BuildPresentationElement(string kind, string refId, XElement el, IReadOnlyDictionary<ResourceId, string> strings)
    {
        PresentationElementKind k = kind switch
        {
            "decimalTextBox" => PresentationElementKind.DecimalTextBox,
            "longDecimalTextBox" => PresentationElementKind.LongDecimalTextBox,
            "checkBox" => PresentationElementKind.CheckBox,
            "textBox" => PresentationElementKind.TextBox,
            "multiTextBox" => PresentationElementKind.MultiTextBox,
            "comboBox" => PresentationElementKind.ComboBox,
            "dropdownList" => PresentationElementKind.DropdownList,
            "listBox" => PresentationElementKind.ListBox,
            _ => PresentationElementKind.Text
        };

        string? label = null;
        string? defaultValue = null;
        bool? defaultChecked = null;
        bool? noSort = null;
        uint? defaultItem = null;
        bool? spin = null;
        uint? spinStep = null;
        bool? showAsDialog = null;
        uint? defaultHeight = null;
        List<string>? suggestions = null;

        if (k == PresentationElementKind.TextBox)
        {
            label = el.Element(el.Name.Namespace + "label")?.Value;
            defaultValue = el.Element(el.Name.Namespace + "defaultValue")?.Value;
        }
        else if (k == PresentationElementKind.ComboBox)
        {
            label = el.Element(el.Name.Namespace + "label")?.Value;
            defaultValue = el.Element(el.Name.Namespace + "default")?.Value;
            suggestions = el.Elements(el.Name.Namespace + "suggestion").Select(e => e.Value).ToList();
            if (!suggestions.Any()) suggestions = null;
            noSort = TryParseBool(el.Attribute("noSort")?.Value);
        }
        else if (k == PresentationElementKind.DropdownList)
        {
            noSort = TryParseBool(el.Attribute("noSort")?.Value);
            defaultItem = TryParseUInt(el.Attribute("defaultItem")?.Value);
        }
        else if (k == PresentationElementKind.CheckBox)
        {
            defaultChecked = TryParseBool(el.Attribute("defaultChecked")?.Value);
        }
        else if (k == PresentationElementKind.DecimalTextBox || k == PresentationElementKind.LongDecimalTextBox)
        {
            defaultValue = el.Attribute("defaultValue")?.Value;
            spin = TryParseBool(el.Attribute("spin")?.Value);
            spinStep = TryParseUInt(el.Attribute("spinStep")?.Value);
        }
        else if (k == PresentationElementKind.MultiTextBox)
        {
            showAsDialog = TryParseBool(el.Attribute("showAsDialog")?.Value);
            defaultHeight = TryParseUInt(el.Attribute("defaultHeight")?.Value);
        }

        return new PresentationElement(k, refId, label, defaultValue, defaultChecked, noSort, defaultItem, spin, spinStep, showAsDialog, defaultHeight, suggestions);
    }

    private static bool? TryParseBool(string? v) => bool.TryParse(v, out var b) ? b : null;
    private static uint? TryParseUInt(string? v) => uint.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var u) ? u : null;

    private static IReadOnlyList<CategoryNode> BuildCategoryTree(IEnumerable<PolicySummary> summaries)
    {
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

    private static DocumentLineage Lineage(string path) => new(new Uri(path), string.Empty, DateTimeOffset.UtcNow, null, null);
}