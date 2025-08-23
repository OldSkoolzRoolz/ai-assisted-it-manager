// Project Name: CorePolicyEngine
// File Name: LogIngestionDtos.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;


public sealed record LogSourceDto(int LogSourceId, string Application, string FilePath, bool Enabled);

public sealed record LogSourceTagDto(int LogSourceId, string Tag);



public sealed record LogIngestionCursorDto(
    int LogSourceId,
    string? LastFile,
    long LastPosition,
    long LastFileSize,
    byte[]? LastHash,
    DateTime UpdatedUtc);



public sealed record LogEventDto(
    long LogEventId,
    int LogSourceId,
    DateTime Ts,
    byte Level,
    int? EventId,
    string? Category,
    string? Message,
    string? Session,
    string? Host,
    string? UserName,
    string? AppVersion,
    string? ModuleVersion);



public enum LogLevelByte : byte
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}