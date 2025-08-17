using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Shared;

namespace CorePolicyEngine.Parsing;

/// <summary>
/// Loads ADMX + matching ADML localized resources and produces an AdmxCatalog.
/// Simplified subset covering categories, policies, parts, enums, supportedOn mapping.
/// </summary>
public sealed class AdmxAdmlParser : IAdmxCatalogLoader
{
    public async Task<Result<AdmxCatalog>> LoadAsync(IReadOnlyList<string> definitionPaths, string? culture, CancellationToken cancellationToken)
    {
        if (definitionPaths.Count == 0) return Result<AdmxCatalog>.Fail("No ADMX files provided");
        var cats = new Dictionary<string, AdmxCategory>();
        var policies = new List<AdmxPolicy>();
        var enums = new Dictionary<string, PolicyEnum>();
        var targetCulture = culture ?? CultureInfo.CurrentUICulture.Name;
        foreach (var path in definitionPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(path)) continue;
            var xml = await File.ReadAllTextAsync(path, cancellationToken);
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            XNamespace ns = doc.Root!.Name.Namespace;
            // categories
            foreach (var c in doc.Root!.Element(ns + "categories")?.Elements(ns + "category") ?? Enumerable.Empty<XElement>())
            {
                var id = c.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(id)) continue;
                var displayName = c.Attribute("displayName")?.Value ?? id;
                var parent = c.Attribute("parentCategory")?.Value;
                cats[id] = new AdmxCategory(id, displayName, parent);
            }
            // policies
            foreach (var p in doc.Root!.Element(ns + "policies")?.Elements(ns + "policy") ?? Enumerable.Empty<XElement>())
            {
                var id = p.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(id)) continue;
                var classAttr = p.Attribute("class")?.Value; // Machine/User
                bool machine = classAttr == "Machine" || classAttr == "Both";
                bool user = classAttr == "User" || classAttr == "Both";
                var category = p.Attribute("parentCategory")?.Value ?? string.Empty;
                var supportedOn = p.Attribute("supportedOn")?.Value;
                var parts = new List<PolicyPartDefinition>();
                foreach (var elem in p.Elements())
                {
                    if (elem.Name.LocalName is "boolean" or "decimal" or "text" or "enum")
                    {
                        var partId = elem.Attribute("id")?.Value ?? elem.Attribute("valueName")?.Value ?? (id+"_part");
                        PolicyValueType vt = elem.Name.LocalName switch
                        {
                            "boolean" => PolicyValueType.Boolean,
                            "decimal" => PolicyValueType.Numeric,
                            "text" => PolicyValueType.Text,
                            "enum" => PolicyValueType.Enum,
                            _ => PolicyValueType.Text
                        };
                        decimal? min = null; decimal? max = null; string? enumId = null;
                        if (vt == PolicyValueType.Numeric)
                        {
                            if (decimal.TryParse(elem.Attribute("minValue")?.Value, out var mi)) min = mi;
                            if (decimal.TryParse(elem.Attribute("maxValue")?.Value, out var ma)) max = ma;
                        }
                        if (vt == PolicyValueType.Enum)
                        {
                            enumId = partId + "_enum"; // synthetic
                            var items = new List<PolicyEnumItem>();
                            foreach (var item in elem.Elements().Where(e => e.Name.LocalName == "item"))
                            {
                                var value = item.Attribute("value")?.Value ?? string.Empty;
                                var disp = item.Attribute("displayName")?.Value ?? value;
                                items.Add(new PolicyEnumItem(disp, value));
                            }
                            enums[enumId] = new PolicyEnum(enumId, items);
                        }
                        parts.Add(new PolicyPartDefinition(partId, vt, enumId, min, max));
                    }
                }
                var display = p.Attribute("displayName")?.Value ?? id;
                policies.Add(new AdmxPolicy(id, display, category, user, machine, parts, supportedOn));
            }
        }
        // culture override pass would map displayName tokens to ADML string table in full implementation.
        var catalog = new AdmxCatalog(cats.Values.ToList(), policies, enums.Values.ToList(), targetCulture);
        return Result<AdmxCatalog>.Ok(catalog);
    }
}
