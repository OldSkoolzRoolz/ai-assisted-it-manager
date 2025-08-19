// Project Name: CorePolicyEngine
// File Name: AdmlModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


namespace CorePolicyEngine.AdminTemplates;


public sealed record AdmlDocument(
    AdmlHeader Header,
    NamespaceBinding Namespace,
    string LanguageTag, // e.g., "en-US"
    IReadOnlyDictionary<ResourceId, string> StringTable,
    IReadOnlyDictionary<string, PresentationTemplate> PresentationTable,
    DocumentLineage Lineage);



public sealed record AdmlHeader(
    string SchemaVersion,
    DateTimeOffset? LastModifiedUtc);



public abstract record PresentationTemplate(string Id);



// Concrete presentation fragments that map to PolicyElement layouts.
// You can extend as you discover templates in the wild; keep them additive.
public sealed record BooleanPresentation(string Id, ResourceId? Label) : PresentationTemplate(Id);



public sealed record TextPresentation(string Id, ResourceId? Label, int? MaxLength) : PresentationTemplate(Id);



public sealed record DecimalPresentation(string Id, ResourceId? Label, long? Min, long? Max) : PresentationTemplate(Id);



public sealed record MultiTextPresentation(string Id, ResourceId? Label, int? MaxItems, int? MaxItemLength)
    : PresentationTemplate(Id);



public sealed record EnumPresentation(string Id, ResourceId? Label, IReadOnlyList<EnumPresentationItem> Items)
    : PresentationTemplate(Id);



public sealed record EnumPresentationItem(string Name, ResourceId? Label);