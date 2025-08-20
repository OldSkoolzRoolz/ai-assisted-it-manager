// Project Name: CorePolicyEngine
// File Name: AdmlModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;


public sealed record AdmlDocument(
    AdmlHeader Header,
    NamespaceBinding Namespace,
    string LanguageTag, // e.g., "en-US"
    IReadOnlyDictionary<ResourceId, string> StringTable,
    IReadOnlyDictionary<string, AdmlPresentation> Presentations,
    DocumentLineage Lineage);



public sealed record AdmlHeader(
    string SchemaVersion,
    DateTimeOffset? LastModifiedUtc);



// A single <presentation id="..."> grouping containing multiple visual element parts
public sealed record AdmlPresentation(
    string Id,
    IReadOnlyList<PresentationElement> Elements);



public enum PresentationElementKind
{
    Text, // literal text block (no refId in schema; we ignore since not bound to element)
    DecimalTextBox,
    TextBox,
    CheckBox,
    ComboBox,
    DropdownList,
    ListBox,
    LongDecimalTextBox,
    MultiTextBox
}



public sealed record PresentationElement(
    PresentationElementKind Kind,
    string RefId, // maps to policy element id (refId attr in ADML)
    string? Label, // optional label text (resolved later via string table if token)
    string? DefaultValue,
    bool? DefaultChecked,
    bool? NoSort,
    uint? DefaultItem,
    bool? Spin,
    uint? SpinStep,
    bool? ShowAsDialog,
    uint? DefaultHeight,
    IReadOnlyList<string>? Suggestions);