// Project Name: CorePolicyEngine
// File Name: AdmlModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Collections.Generic;

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

/// <summary>
/// Represents a localized ADML resource document providing string table and presentation
/// data for an associated ADMX policy definition file.
/// </summary>
/// <param name="Header">Document header metadata.</param>
/// <param name="Namespace">Namespace binding referencing the ADMX namespace pairing.</param>
/// <param name="LanguageTag">IETF language tag (e.g. "en-US").</param>
/// <param name="StringTable">Map of resource identifiers to localized strings.</param>
/// <param name="Presentations">Collection of presentation definitions keyed by id.</param>
/// <param name="Lineage">Origin / source information for diagnostics.</param>
public sealed record AdmlDocument(
    AdmlHeader Header,
    NamespaceBinding Namespace,
    string LanguageTag,
    IReadOnlyDictionary<ResourceId, string> StringTable,
    IReadOnlyDictionary<string, AdmlPresentation> Presentations,
    DocumentLineage Lineage);

/// <summary>
/// Header metadata for an ADML document including schema version and modification time.
/// </summary>
/// <param name="SchemaVersion">Schema version string as declared in the document.</param>
/// <param name="LastModifiedUtc">Optional last modified timestamp (UTC) if available.</param>
public sealed record AdmlHeader(
    string SchemaVersion,
    DateTimeOffset? LastModifiedUtc);

/// <summary>
/// A single presentation grouping containing one or more UI element descriptions used by a policy.
/// </summary>
/// <param name="Id">Presentation identifier (referenced from ADMX elements).</param>
/// <param name="Elements">Ordered list of constituent presentation elements.</param>
public sealed record AdmlPresentation(
    string Id,
    IReadOnlyList<PresentationElement> Elements);

/// <summary>
/// Enumerates supported presentation element kinds as defined in ADML schema.
/// </summary>
public enum PresentationElementKind
{
    /// <summary>Plain static text (not bound to policy element id).</summary>
    Text,
    /// <summary>Numeric input supporting decimal values.</summary>
    DecimalTextBox,
    /// <summary>Free-form text input.</summary>
    TextBox,
    /// <summary>Boolean toggle (checkbox).</summary>
    CheckBox,
    /// <summary>Combo box with editable text.</summary>
    ComboBox,
    /// <summary>Drop-down list selection (non-editable).</summary>
    DropdownList,
    /// <summary>List box supporting multi / single selection depending on policy.</summary>
    ListBox,
    /// <summary>Numeric input with extended range semantics.</summary>
    LongDecimalTextBox,
    /// <summary>Multi-line text input.</summary>
    MultiTextBox
}

/// <summary>
/// Describes a single presentation element (control) embedded within a presentation definition.
/// </summary>
/// <param name="Kind">Element UI kind.</param>
/// <param name="RefId">Identifier linking to the ADMX policy element (refId attribute).</param>
/// <param name="Label">Optional UI label (already resolved or token – caller decides resolution timing).</param>
/// <param name="DefaultValue">Optional default textual value for input controls.</param>
/// <param name="DefaultChecked">Optional default state for boolean elements.</param>
/// <param name="NoSort">Indicates list items should retain declared order (no sort).</param>
/// <param name="DefaultItem">Index / key of default list selection if provided.</param>
/// <param name="Spin">Whether a numeric control supports spinner affordance.</param>
/// <param name="SpinStep">Increment step for spinner capable numeric controls.</param>
/// <param name="ShowAsDialog">Indicates large multi-line input should present in a dialog.</param>
/// <param name="DefaultHeight">Default pixel height for multi-line text controls.</param>
/// <param name="Suggestions">Optional suggestion / auto-complete entries.</param>
public sealed record PresentationElement(
    PresentationElementKind Kind,
    string RefId,
    string? Label,
    string? DefaultValue,
    bool? DefaultChecked,
    bool? NoSort,
    uint? DefaultItem,
    bool? Spin,
    uint? SpinStep,
    bool? ShowAsDialog,
    uint? DefaultHeight,
    IReadOnlyList<string>? Suggestions);