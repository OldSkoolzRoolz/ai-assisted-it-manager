// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyRegistry.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using KC.ITCompanion.CorePolicyEngine.Models;

using Microsoft.Win32;


namespace KC.ITCompanion.CorePolicyEngine.Configuration;


/// <summary>
///     Reads / writes BehaviorPolicy related settings from Windows Policies registry roots so they can be managed by ADMX.
///     Machine: HKLM\Software\Policies\KCITCompanion\Client
///     User:    HKCU\Software\Policies\KCITCompanion\Client
/// </summary>
public static class BehaviorPolicyRegistry
{
    private const string MachinePath = "Software\\Policies\\KCITCompanion\\Client";
    private const string UserPath = "Software\\Policies\\KCITCompanion\\Client";





    public static BehaviorPolicy ApplyOverrides(BehaviorPolicy basePolicy)
    {
        BehaviorPolicy merged = basePolicy;
        // Machine first
        merged = ReadInto(merged, RegistryHive.LocalMachine, MachinePath, true);
        // Then user overrides (where allowed)
        merged = ReadInto(merged, RegistryHive.CurrentUser, UserPath, false);
        return merged;
    }





    private static BehaviorPolicy ReadInto(BehaviorPolicy current, RegistryHive hive, string subPath, bool machineScope)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(subPath, false);
            if (key == null) return current;

            // Helper local funcs
            int Clamp(int value, int min, int max)
            {
                return Math.Min(Math.Max(value, min), max);
            }

            string? GetString(string name)
            {
                return key.GetValue(name) as string;
            }

            int? GetInt(string name)
            {
                var val = key.GetValue(name);
                if (val == null) return null;
                try
                {
                    return Convert.ToInt32(val);
                }
                catch
                {
                    return null;
                }
            }

            bool? GetBool(string name)
            {
                var i = GetInt(name);
                if (i == null) return null;
                return i != 0;
            }

            var logRetention = GetInt("LogRetentionDays");
            var maxFileSize = GetInt("MaxLogFileSizeMB");
            var minLogLevel = GetString("MinLogLevel");
            var uiLang = GetString("UiLanguage");
            var enableTelemetry = GetBool("EnableTelemetry");
            var allowedGroups = GetString("AllowedGroupsCsv");
            var logViewPoll = GetInt("LogViewPollSeconds");
            var queueDepth = GetInt("LogQueueMaxDepthPerModule");
            var circuitThreshold = GetInt("LogCircuitErrorThreshold");
            var circuitWindow = GetInt("LogCircuitErrorWindowSeconds");
            var failoverEnabled = GetBool("LogFailoverEnabled");

            // Merge with validation / clamp
            return current with
            {
                LogRetentionDays = logRetention is null ? current.LogRetentionDays : Clamp(logRetention.Value, 1, 365),
                MaxLogFileSizeMb = maxFileSize is null ? current.MaxLogFileSizeMb : Clamp(maxFileSize.Value, 1, 512),
                MinLogLevel = string.IsNullOrWhiteSpace(minLogLevel) ? current.MinLogLevel : minLogLevel!,
                UiLanguage = string.IsNullOrWhiteSpace(uiLang) ? current.UiLanguage : uiLang!,
                EnableTelemetry = enableTelemetry ?? current.EnableTelemetry,
                AllowedGroupsCsv = string.IsNullOrWhiteSpace(allowedGroups) ? current.AllowedGroupsCsv : allowedGroups!,
                LogViewPollSeconds =
                logViewPoll is null ? current.LogViewPollSeconds : Clamp(logViewPoll.Value, 5, 300),
                LogQueueMaxDepthPerModule = queueDepth is null
                    ? current.LogQueueMaxDepthPerModule
                    : Clamp(queueDepth.Value, 500, 50000),
                LogCircuitErrorThreshold = circuitThreshold is null
                    ? current.LogCircuitErrorThreshold
                    : Clamp(circuitThreshold.Value, 5, 500),
                LogCircuitErrorWindowSeconds = circuitWindow is null
                    ? current.LogCircuitErrorWindowSeconds
                    : Clamp(circuitWindow.Value, 10, 3600),
                LogFailoverEnabled = failoverEnabled ?? current.LogFailoverEnabled
            };
        }
        catch
        {
            return current; // swallow registry access errors (unavailable hive / ACL) and keep existing
        }
    }





    /// <summary>
    ///     Writes explicit overrides for machine scope (only selected fields). Null leaves existing value untouched.
    /// </summary>
    public static void WriteMachineOverrides(Action<BehaviorPolicyOverrideBuilder> configure)
    {
        var builder = new BehaviorPolicyOverrideBuilder();
        configure(builder);
        WriteOverrides(RegistryHive.LocalMachine, MachinePath, builder);
    }





    /// <summary>
    ///     Writes explicit overrides for user scope (only allowed user-scoped fields).
    /// </summary>
    public static void WriteUserOverrides(Action<BehaviorPolicyOverrideBuilder> configure)
    {
        var builder = new BehaviorPolicyOverrideBuilder();
        configure(builder);
        WriteOverrides(RegistryHive.CurrentUser, UserPath, builder);
    }





    private static void WriteOverrides(RegistryHive hive, string subPath, BehaviorPolicyOverrideBuilder b)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using RegistryKey key = baseKey.CreateSubKey(subPath, true)!;

            void Set(string name, object value)
            {
                key.SetValue(name, value);
            }

            if (b.LogRetentionDays.HasValue) Set("LogRetentionDays", b.LogRetentionDays.Value);
            if (b.MaxLogFileSizeMb.HasValue) Set("MaxLogFileSizeMB", b.MaxLogFileSizeMb.Value);
            if (b.MinLogLevel is not null) Set("MinLogLevel", b.MinLogLevel);
            if (b.UiLanguage is not null) Set("UiLanguage", b.UiLanguage);
            if (b.EnableTelemetry.HasValue) Set("EnableTelemetry", b.EnableTelemetry.Value ? 1 : 0);
            if (b.AllowedGroupsCsv is not null) Set("AllowedGroupsCsv", b.AllowedGroupsCsv);
            if (b.LogViewPollSeconds.HasValue) Set("LogViewPollSeconds", b.LogViewPollSeconds.Value);
            if (b.LogQueueMaxDepthPerModule.HasValue)
                Set("LogQueueMaxDepthPerModule", b.LogQueueMaxDepthPerModule.Value);
            if (b.LogCircuitErrorThreshold.HasValue) Set("LogCircuitErrorThreshold", b.LogCircuitErrorThreshold.Value);
            if (b.LogCircuitErrorWindowSeconds.HasValue)
                Set("LogCircuitErrorWindowSeconds", b.LogCircuitErrorWindowSeconds.Value);
            if (b.LogFailoverEnabled.HasValue) Set("LogFailoverEnabled", b.LogFailoverEnabled.Value ? 1 : 0);
        }
        catch
        {
        }
    }
}



/// <summary>
///     Builder for selective registry override persistence.
/// </summary>
public sealed class BehaviorPolicyOverrideBuilder
{
    public int? LogRetentionDays { get; set; }
    public int? MaxLogFileSizeMb { get; set; }
    public string? MinLogLevel { get; set; }
    public string? UiLanguage { get; set; }
    public bool? EnableTelemetry { get; set; }
    public string? AllowedGroupsCsv { get; set; }
    public int? LogViewPollSeconds { get; set; }
    public int? LogQueueMaxDepthPerModule { get; set; }
    public int? LogCircuitErrorThreshold { get; set; }
    public int? LogCircuitErrorWindowSeconds { get; set; }
    public bool? LogFailoverEnabled { get; set; }
}