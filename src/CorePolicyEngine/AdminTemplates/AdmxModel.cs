// Project Name: CorePolicyEngine
// File Name: AdmxModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved
// Do not remove file headers


namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

public sealed record AdmxDocument(
    AdmxHeader Header,
    NamespaceBinding Namespace,
    IReadOnlyList<NamespaceBinding> RequiredNamespaces,
    IReadOnlyList<Category> Categories,
    IReadOnlyList<SupportDefinition> Support,
    IReadOnlyList<AdminPolicy> Policies,
    DocumentLineage Lineage);

public sealed record AdmxHeader(
    string SchemaVersion, // e.g., "1.0"
    string? Revision, // optional revision string
    string? TargetProduct, // optional product hint
    DateTimeOffset? LastModifiedUtc // optional modification time
);

public sealed record NamespaceBinding(string Prefix, Uri Uri);

public readonly record struct PolicyKey(Uri NamespaceUri, string Name); // stable, non-meaningful surrogate for identity

public sealed record Category(
    CategoryId Id,
    LocalizedRef DisplayName,
    CategoryRef? Parent, // null if root
    DocumentLineage Lineage);

public readonly record struct CategoryId(string Value);

public readonly record struct CategoryRef(CategoryId Id);

public sealed record SupportDefinition(
    SupportId Id,
    LocalizedRef DisplayName,
    IReadOnlyList<SupportProduct> Products,
    DocumentLineage Lineage);

public readonly record struct SupportId(string Value);

public sealed record SupportProduct(
    string Name, // e.g., "Windows 11"
    string? MinVersion,
    string? MaxVersion,
    bool? InclusiveMin,
    bool? InclusiveMax);

public enum PolicyClass
{
    Machine,
    User,
    Both // Added to align with official ADMX schema PolicyClass enumeration
}

public sealed record AdminPolicy(
    PolicyKey Key,
    PolicyClass Class,
    LocalizedRef DisplayName,
    LocalizedRef? ExplainText,
    CategoryRef Category,
    SupportId? SupportedOn, // optional for legacy or generic
    PresentationRef? Presentation, // binds to ADML presentation id
    IReadOnlyList<PolicyElement> Elements, // discriminated union below
    PolicyStateBehavior StateBehavior, // on enable/disable + default
    IReadOnlyList<Tags> Tags, // optional metadata tags
    PolicyVersion Version,
    DocumentLineage Lineage);

public sealed record PolicyVersion(int Major, int Minor);

public sealed record Tags(string Name, string? Value);

public sealed record PresentationRef(string Id);

// What to do when policy is Enabled/Disabled/NotConfigured
public sealed record PolicyStateBehavior(
    PolicyDefaultState DefaultState,
    IReadOnlyList<RegistryAction> OnEnable,
    IReadOnlyList<RegistryAction> OnDisable,
    IReadOnlyList<RegistryAction> OnNotConfigured);

public enum PolicyDefaultState
{
    NotConfigured,
    Enabled,
    Disabled
}

// Registry mapping primitives
public enum RegistryHive
{
    LocalMachine,
    CurrentUser,
    ClassesRoot,
    Users,
    CurrentConfig
}

public enum RegistryValueType
{
    None,
    RegSz,
    RegExpandSz,
    RegDword,
    RegQword,
    RegMultiSz,
    RegBinary
}

public sealed record RegistryPath(RegistryHive Hive, string KeyPath); // e.g., HKLM\Software\Policies\...

public sealed record RegistryAction(
    RegistryPath Path,
    string? ValueName, // null => default value
    RegistryValueType ValueType,
    object? Value, // typed at runtime; null => delete value/key depending on Operation
    RegistryOperation Operation);

public enum RegistryOperation
{
    SetValue,
    DeleteValue,
    DeleteTree
}

// Discriminated union for policy elements (typed constraints & data)
public abstract record PolicyElement(ElementId Id);

public readonly record struct ElementId(string Value);

public sealed record BooleanElement(
    ElementId Id,
    LocalizedRef? Label,
    IReadOnlyList<RegistryAction> WhenTrue,
    IReadOnlyList<RegistryAction> WhenFalse) : PolicyElement(Id);

public sealed record DecimalElement(
    ElementId Id,
    LocalizedRef? Label,
    long? MinInclusive,
    long? MaxInclusive,
    IReadOnlyList<RegistryActionTemplate<long>> Writes) : PolicyElement(Id);

public sealed record TextElement(
    ElementId Id,
    LocalizedRef? Label,
    int? MinLength,
    int? MaxLength,
    IReadOnlyList<RegistryActionTemplate<string>> Writes) : PolicyElement(Id);

public sealed record MultiTextElement(
    ElementId Id,
    LocalizedRef? Label,
    int? MaxItems,
    int? MaxItemLength,
    IReadOnlyList<RegistryActionTemplate<IReadOnlyList<string>>> Writes) : PolicyElement(Id);

public sealed record EnumElement(
    ElementId Id,
    LocalizedRef? Label,
    IReadOnlyList<EnumItem> Items) : PolicyElement(Id);

public sealed record EnumItem(
    string Name, // internal item name
    LocalizedRef? Label,
    IReadOnlyList<RegistryAction> Writes);

// Registry action templates allow element values to flow into the registry write
public sealed record RegistryActionTemplate<TValue>(
    RegistryPath Path,
    string? ValueName,
    RegistryValueType ValueType,
    TemplateExpression<TValue> Expression,
    RegistryOperation Operation);

public abstract record TemplateExpression<TValue>;

public sealed record LiteralExpression<TValue>(TValue Value) : TemplateExpression<TValue>;

public sealed record FormatExpression<TValue>(string Format, Func<TValue, object> Project) : TemplateExpression<TValue>;

// Reference to ADML string id
public sealed record LocalizedRef(ResourceId Id);

public readonly record struct ResourceId(string Value);

// Cross-cutting lineage for traceability
public sealed record DocumentLineage(
    Uri SourceUri, // file path/URI
    string ContentHash, // e.g., SHA-256
    DateTimeOffset LoadedAtUtc,
    string? XPath, // optional pointer for debugging
    int? LineNumber); // optional