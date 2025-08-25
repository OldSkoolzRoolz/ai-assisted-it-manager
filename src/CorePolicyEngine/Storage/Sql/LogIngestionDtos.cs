// Project Name: CorePolicyEngine
// File Name: LogIngestionDtos.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Generic;

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>
/// Log source definition row (identifies a monitored log file / application pair).
/// </summary>
/// <param name="LogSourceId">Identity key.</param>
/// <param name="Application">Application / subsystem name.</param>
/// <param name="FilePath">Canonical file path being tailed.</param>
/// <param name="Enabled">True when source is active.</param>
public sealed record LogSourceDto(int LogSourceId, string Application, string FilePath, bool Enabled);

/// <summary>
/// Tag attached to a log source (categorization metadata).
/// </summary>
/// <param name="LogSourceId">Related source id.</param>
/// <param name="Tag">Tag value.</param>
public sealed record LogSourceTagDto(int LogSourceId, string Tag);

/// <summary>
/// Cursor describing last ingestion progress for a log source.
/// </summary>
/// <param name="LogSourceId">Source id.</param>
/// <param name="LastFile">Last file name processed (rotation support).</param>
/// <param name="LastPosition">Byte position within current file.</param>
/// <param name="LastFileSize">Size of file when last processed.</param>
/// <param name="LastHash">Optional hash fingerprint of segment (wrapped as ReadOnlyMemory for CA1819).</param>
/// <param name="UpdatedUtc">Update timestamp (UTC).</param>
public sealed record LogIngestionCursorDto(
    int LogSourceId,
    string? LastFile,
    long LastPosition,
    long LastFileSize,
    ReadOnlyMemory<byte>? LastHash,
    DateTime UpdatedUtc);

/// <summary>
/// Persisted log event DTO (flattened for ingestion pipeline).
/// </summary>
/// <param name="LogEventId">Identity.</param>
/// <param name="LogSourceId">Source id.</param>
/// <param name="Ts">Event timestamp (UTC or local standardized upstream).</param>
/// <param name="Level">Severity level (see <see cref="LogLevel"/>).</param>
/// <param name="EventId">Optional numeric event id.</param>
/// <param name="Category">Event category string.</param>
/// <param name="Message">Rendered message (single-line).</param>
/// <param name="Session">Optional session / correlation id.</param>
/// <param name="Host">Host machine name.</param>
/// <param name="UserName">User principal.</param>
/// <param name="AppVersion">Application version.</param>
/// <param name="ModuleVersion">Module version.</param>
public sealed record LogEventDto(
    long LogEventId,
    int LogSourceId,
    DateTime Ts,
    int Level,
    int? EventId,
    string? Category,
    string? Message,
    string? Session,
    string? Host,
    string? UserName,
    string? AppVersion,
    string? ModuleVersion);

/// <summary>
/// Canonical log level mapping to match standard ILogger severities.
/// </summary>
public enum LogLevel
{
    /// <summary>Trace level (very high verbosity diagnostic events).</summary>
    Trace = 0,
    /// <summary>Debug level (verbose troubleshooting information).</summary>
    Debug = 1,
    /// <summary>Information level (normal operational events).</summary>
    Information = 2,
    /// <summary>Warning level (abnormal or unexpected situations not yet errors).</summary>
    Warning = 3,
    /// <summary>Error level (recoverable failures affecting operations).</summary>
    Error = 4,
    /// <summary>Critical level (non-recoverable / application or service failure conditions).</summary>
    Critical = 5
}