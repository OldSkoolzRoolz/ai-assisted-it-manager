// Project Name: CorePolicyEngine
// File Name: AdmxModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved
// Do not remove file headers


using System;
using System.Collections.Generic;

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

/// <summary>
/// Represents a parsed ADMX policy definition document (language neutral) including
/// categories, support definitions, and policy objects.
/// </summary>
/// <param name="Header">Document header metadata.</param>
/// <param name="Namespace">Primary namespace binding declared by the document.</param>
/// <param name="RequiredNamespaces">Additional namespaces that must be present for dependency resolution.</param>
/// <param name="Categories">Flattened category definitions.</param>
/// <param name="Support">Support (supportedOn) definitions describing OS / product applicability.</param>
/// <param name="Policies">Administrative policy definitions contained in the document.</param>
/// <param name="Lineage">Source / provenance tracking information.</param>
public sealed record AdmxDocument(
    AdmxHeader Header,
    NamespaceBinding Namespace,
    IReadOnlyList<NamespaceBinding> RequiredNamespaces,
    IReadOnlyList<Category> Categories,
    IReadOnlyList<SupportDefinition> Support,
    IReadOnlyList<AdminPolicy> Policies,
    DocumentLineage Lineage);

/// <summary>
/// Header metadata for an ADMX document.
/// </summary>
/// <param name="SchemaVersion">Schema version string (e.g. "1.0").</param>
/// <param name="Revision">Optional document revision identifier.</param>
/// <param name="TargetProduct">Optional target product hint.</param>
/// <param name="LastModifiedUtc">Optional last modified timestamp (UTC).</param>
public sealed record AdmxHeader(
    string SchemaVersion,
    string? Revision,
    string? TargetProduct,
    DateTimeOffset? LastModifiedUtc);

/// <summary>
/// Associates a namespace prefix with its URI for policy / category identification.
/// </summary>
/// <param name="Prefix">Namespace prefix.</param>
/// <param name="Uri">Namespace URI.</param>
public sealed record NamespaceBinding(string Prefix, Uri Uri);

/// <summary>
/// Stable identity for a policy combining namespace URI + policy name.
/// </summary>
/// <param name="NamespaceUri">Namespace URI.</param>
/// <param name="Name">Policy name (local id).</param>
public readonly record struct PolicyKey(Uri NamespaceUri, string Name);

/// <summary>
/// Category definition used to group policies in administrative templates.
/// </summary>
/// <param name="Id">Unique category identifier.</param>
/// <param name="DisplayName">Localized display name reference.</param>
/// <param name="Parent">Optional parent category reference.</param>
/// <param name="Lineage">Source tracking data.</param>
public sealed record Category(
    CategoryId Id,
    LocalizedRef DisplayName,
    CategoryRef? Parent,
    DocumentLineage Lineage);

/// <summary>
/// Strongly typed category identifier wrapper.
/// </summary>
/// <param name="Value">Underlying string value.</param>
public readonly record struct CategoryId(string Value);

/// <summary>
/// Reference to a category identifier.
/// </summary>
/// <param name="Id">Category id.</param>
public readonly record struct CategoryRef(CategoryId Id);

/// <summary>
/// Support definition describing one or more products / versions a policy applies to.
/// </summary>
/// <param name="Id">Support identifier.</param>
/// <param name="DisplayName">Localized name reference.</param>
/// <param name="Products">List of product applicability descriptors.</param>
/// <param name="Lineage">Source tracking data.</param>
public sealed record SupportDefinition(
    SupportId Id,
    LocalizedRef DisplayName,
    IReadOnlyList<SupportProduct> Products,
    DocumentLineage Lineage);

/// <summary>
/// Strongly typed support definition identifier.
/// </summary>
/// <param name="Value">Underlying string value.</param>
public readonly record struct SupportId(string Value);

/// <summary>
/// Describes applicability of a support definition to a given product / version range.
/// </summary>
/// <param name="Name">Canonical product name (e.g. "Windows 11").</param>
/// <param name="MinVersion">Optional inclusive minimum version (if <paramref name="InclusiveMin"/> true).</param>
/// <param name="MaxVersion">Optional inclusive maximum version (if <paramref name="InclusiveMax"/> true).</param>
/// <param name="InclusiveMin">Whether <paramref name="MinVersion"/> is inclusive.</param>
/// <param name="InclusiveMax">Whether <paramref name="MaxVersion"/> is inclusive.</param>
public sealed record SupportProduct(
    string Name,
    string? MinVersion,
    string? MaxVersion,
    bool? InclusiveMin,
    bool? InclusiveMax);

/// <summary>
/// Administrative policy classification (machine / user / both contexts).
/// </summary>
public enum PolicyClass
{
    /// <summary>Applies to computer (machine) configuration.</summary>
    Machine,
    /// <summary>Applies to user configuration.</summary>
    User,
    /// <summary>Applies to both user and machine contexts.</summary>
    Both // Added to align with official ADMX schema PolicyClass enumeration
}

/// <summary>
/// Represents a single administrative policy with its registry impact definition.
/// </summary>
/// <param name="Key">Policy identity.</param>
/// <param name="Class">Target policy class (user / machine / both).</param>
/// <param name="DisplayName">Localized display name reference.</param>
/// <param name="ExplainText">Optional localized extended explanation reference.</param>
/// <param name="Category">Owning category reference.</param>
/// <param name="SupportedOn">Optional support definition reference.</param>
/// <param name="Presentation">Optional presentation reference for UI layout.</param>
/// <param name="Elements">Element constraints / inputs for the policy.</param>
/// <param name="StateBehavior">Registry actions for each configuration state.</param>
/// <param name="Tags">Optional metadata tags.</param>
/// <param name="Version">Policy version information.</param>
/// <param name="Lineage">Source tracking.</param>
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

/// <summary>
/// Version components for an administrative policy.
/// </summary>
/// <param name="Major">Major revision component.</param>
/// <param name="Minor">Minor revision component.</param>
public sealed record PolicyVersion(int Major, int Minor);

/// <summary>
/// Arbitrary metadata tag associated with a policy.
/// </summary>
/// <param name="Name">Tag name.</param>
/// <param name="Value">Optional tag value.</param>
public sealed record Tags(string Name, string? Value);

/// <summary>
/// Reference to a presentation definition in the ADML document.
/// </summary>
/// <param name="Id">Presentation identifier.</param>
public sealed record PresentationRef(string Id);

/// <summary>
/// Describes registry mutation behavior for each policy state.
/// </summary>
/// <param name="DefaultState">Policy default state.</param>
/// <param name="OnEnable">Registry actions executed when enabled.</param>
/// <param name="OnDisable">Registry actions executed when disabled.</param>
/// <param name="OnNotConfigured">Registry actions executed when not configured.</param>
public sealed record PolicyStateBehavior(
    PolicyDefaultState DefaultState,
    IReadOnlyList<RegistryAction> OnEnable,
    IReadOnlyList<RegistryAction> OnDisable,
    IReadOnlyList<RegistryAction> OnNotConfigured);

/// <summary>
/// Enumeration of policy default state values.
/// </summary>
public enum PolicyDefaultState
{
    /// <summary>Policy defaults to Not Configured.</summary>
    NotConfigured,
    /// <summary>Policy defaults to Enabled.</summary>
    Enabled,
    /// <summary>Policy defaults to Disabled.</summary>
    Disabled
}

/// <summary>
/// Windows registry root hives used in policy actions.
/// </summary>
public enum RegistryHive
{
    /// <summary>HKEY_LOCAL_MACHINE hive.</summary>
    LocalMachine,
    /// <summary>HKEY_CURRENT_USER hive.</summary>
    CurrentUser,
    /// <summary>HKEY_CLASSES_ROOT hive.</summary>
    ClassesRoot,
    /// <summary>HKEY_USERS hive.</summary>
    Users,
    /// <summary>HKEY_CURRENT_CONFIG hive.</summary>
    CurrentConfig
}

/// <summary>
/// Registry value kinds supported by the engine.
/// </summary>
public enum RegistryValueType
{
    /// <summary>No value (placeholder).</summary>
    None,
    /// <summary>REG_SZ string.</summary>
    RegSz,
    /// <summary>REG_EXPAND_SZ expandable string.</summary>
    RegExpandSz,
    /// <summary>REG_DWORD 32-bit integer.</summary>
    RegDword,
    /// <summary>REG_QWORD 64-bit integer.</summary>
    RegQword,
    /// <summary>REG_MULTI_SZ multi-string.</summary>
    RegMultiSz,
    /// <summary>REG_BINARY binary blob.</summary>
    RegBinary
}

/// <summary>
/// Represents a concrete registry path (hive + key path).
/// </summary>
/// <param name="Hive">Root hive.</param>
/// <param name="KeyPath">Subkey path.</param>
public sealed record RegistryPath(RegistryHive Hive, string KeyPath); // e.g., HKLM\Software\Policies\...

/// <summary>
/// Describes a registry action (set / delete) to be applied for a policy state.
/// </summary>
/// <param name="Path">Target registry path.</param>
/// <param name="ValueName">Optional value name (null indicates default).</param>
/// <param name="ValueType">Value type for set operations.</param>
/// <param name="Value">Concrete value (null may imply delete depending on operation).</param>
/// <param name="Operation">Registry operation.</param>
public sealed record RegistryAction(
    RegistryPath Path,
    string? ValueName, // null => default value
    RegistryValueType ValueType,
    object? Value, // typed at runtime; null => delete value/key depending on Operation
    RegistryOperation Operation);

/// <summary>
/// Supported registry operations.
/// </summary>
public enum RegistryOperation
{
    /// <summary>Set or create value.</summary>
    SetValue,
    /// <summary>Delete a specific value.</summary>
    DeleteValue,
    /// <summary>Delete key tree.</summary>
    DeleteTree
}

/// <summary>
/// Base type for a policy element (input / constraint) in a policy presentation.
/// </summary>
/// <param name="Id">Element identifier.</param>
public abstract record PolicyElement(ElementId Id);

/// <summary>
/// Strongly typed element identifier wrapper.
/// </summary>
/// <param name="Value">Underlying string value.</param>
public readonly record struct ElementId(string Value);

/// <summary>
/// Boolean policy element with registry actions for true/false selections.
/// </summary>
/// <param name="Id">Element id.</param>
/// <param name="Label">Optional localized label.</param>
/// <param name="WhenTrue">Actions when element resolves to true.</param>
/// <param name="WhenFalse">Actions when element resolves to false.</param>
public sealed record BooleanElement(
    ElementId Id,
    LocalizedRef? Label,
    IReadOnlyList<RegistryAction> WhenTrue,
    IReadOnlyList<RegistryAction> WhenFalse) : PolicyElement(Id);

/// <summary>
/// Numeric (integral) policy element with optional range and registry templates.
/// </summary>
/// <param name="Id">Element id.</param>
/// <param name="Label">Optional label.</param>
/// <param name="MinInclusive">Optional inclusive minimum.</param>
/// <param name="MaxInclusive">Optional inclusive maximum.</param>
/// <param name="Writes">Registry action templates using the numeric value.</param>
public sealed record DecimalElement(
    ElementId Id,
    LocalizedRef? Label,
    long? MinInclusive,
    long? MaxInclusive,
    IReadOnlyList<RegistryActionTemplate<long>> Writes) : PolicyElement(Id);

/// <summary>
/// Single-line text policy element.
/// </summary>
/// <param name="Id">Element id.</param>
/// <param name="Label">Optional label.</param>
/// <param name="MinLength">Optional minimum length.</param>
/// <param name="MaxLength">Optional maximum length.</param>
/// <param name="Writes">Registry action templates consuming the text value.</param>
public sealed record TextElement(
    ElementId Id,
    LocalizedRef? Label,
    int? MinLength,
    int? MaxLength,
    IReadOnlyList<RegistryActionTemplate<string>> Writes) : PolicyElement(Id);

/// <summary>
/// Multi-line or multi-value text element.
/// </summary>
/// <param name="Id">Element id.</param>
/// <param name="Label">Optional label.</param>
/// <param name="MaxItems">Maximum number of items (if bounded).</param>
/// <param name="MaxItemLength">Maximum per-item length.</param>
/// <param name="Writes">Registry action templates consuming the list.</param>
public sealed record MultiTextElement(
    ElementId Id,
    LocalizedRef? Label,
    int? MaxItems,
    int? MaxItemLength,
    IReadOnlyList<RegistryActionTemplate<IReadOnlyList<string>>> Writes) : PolicyElement(Id);

/// <summary>
/// Enumeration selection element.
/// </summary>
/// <param name="Id">Element id.</param>
/// <param name="Label">Optional label.</param>
/// <param name="Items">Enumeration items.</param>
public sealed record EnumElement(
    ElementId Id,
    LocalizedRef? Label,
    IReadOnlyList<EnumItem> Items) : PolicyElement(Id);

/// <summary>
/// Enumeration item entry with associated registry writes.
/// </summary>
/// <param name="Name">Internal name.</param>
/// <param name="Label">Optional localized label.</param>
/// <param name="Writes">Registry actions applied when selected.</param>
public sealed record EnumItem(
    string Name,
    LocalizedRef? Label,
    IReadOnlyList<RegistryAction> Writes);

/// <summary>
/// Template for generating a registry action from a typed element value.
/// </summary>
/// <typeparam name="TValue">Element value type.</typeparam>
/// <param name="Path">Target registry path.</param>
/// <param name="ValueName">Optional value name.</param>
/// <param name="ValueType">Registry value type.</param>
/// <param name="Expression">Expression producing the value.</param>
/// <param name="Operation">Registry operation.</param>
public sealed record RegistryActionTemplate<TValue>(
    RegistryPath Path,
    string? ValueName,
    RegistryValueType ValueType,
    TemplateExpression<TValue> Expression,
    RegistryOperation Operation);

/// <summary>
/// Base template expression used to produce a registry value from an element value.
/// </summary>
/// <typeparam name="TValue">Element value type.</typeparam>
public abstract record TemplateExpression<TValue>;

/// <summary>
/// Literal value expression.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
/// <param name="Value">Literal value.</param>
public sealed record LiteralExpression<TValue>(TValue Value) : TemplateExpression<TValue>;

/// <summary>
/// Format projection expression using a projector function.
/// </summary>
/// <typeparam name="TValue">Element value type.</typeparam>
/// <param name="Format">Composite format string.</param>
/// <param name="Project">Projection function mapping the value to an object inserted into the format.</param>
public sealed record FormatExpression<TValue>(string Format, Func<TValue, object> Project) : TemplateExpression<TValue>;

/// <summary>
/// Reference to a localized string (ADML resource id).
/// </summary>
/// <param name="Id">Resource identifier.</param>
public sealed record LocalizedRef(ResourceId Id);

/// <summary>
/// Strongly typed resource identifier wrapper.
/// </summary>
/// <param name="Value">Underlying string value.</param>
public readonly record struct ResourceId(string Value);

/// <summary>
/// Tracks provenance for any document derived object enabling traceability and diagnostics.
/// </summary>
/// <param name="SourceUri">Source file or URI.</param>
/// <param name="ContentHash">Hash of original content (e.g. SHA-256).</param>
/// <param name="LoadedAtUtc">UTC timestamp when parsed.</param>
/// <param name="XPath">Optional XPath / pointer within source.</param>
/// <param name="LineNumber">Optional line number in source.</param>
public sealed record DocumentLineage(
    Uri SourceUri,
    string ContentHash,
    DateTimeOffset LoadedAtUtc,
    string? XPath,
    int? LineNumber);