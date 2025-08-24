// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyRegistry.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Globalization;
using KC.ITCompanion.CorePolicyEngine.Models;
using Microsoft.Win32;

namespace KC.ITCompanion.CorePolicyEngine.Configuration;

/// <summary>
/// Reads / writes <see cref="BehaviorPolicy"/> settings from Windows Policies registry roots so they can be managed via ADMX.
/// Machine: HKLM\Software\Policies\KCITCompanion\Client
/// User:    HKCU\Software\Policies\KCITCompanion\Client
/// </summary>
public static class BehaviorPolicyRegistry
{
    private const string MachinePath = "Software\\Policies\\KCITCompanion\\Client";
    private const string UserPath = "Software\\Policies\\KCITCompanion\\Client";

    /// <summary>
    /// Applies machine then user overrides (where allowed) onto a provided base policy instance.
    /// </summary>
    /// <param name="basePolicy">Base policy (defaults) to override.</param>
    /// <returns>New merged <see cref="BehaviorPolicy"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="basePolicy"/> is null.</exception>
    public static BehaviorPolicy ApplyOverrides(BehaviorPolicy basePolicy)
    {
        ArgumentNullException.ThrowIfNull(basePolicy);
        BehaviorPolicy merged = basePolicy;
        merged = ReadInto(merged, RegistryHive.LocalMachine, MachinePath, true);
        merged = ReadInto(merged, RegistryHive.CurrentUser, UserPath, false);
        return merged;
    }

    private static BehaviorPolicy ReadInto(BehaviorPolicy current, RegistryHive hive, string subPath, bool machineScope)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using RegistryKey? key = baseKey.OpenSubKey(subPath, false);
            if (key is null) return current;

            // Helper local funcs
            int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
            string? GetString(string name) => key.GetValue(name) as string;

            int? GetInt(string name)
            {
                var val = key.GetValue(name);
                if (val is null) return null;
                try
                {
                    // Culture invariant per CA1305
                    return Convert.ToInt32(val, CultureInfo.InvariantCulture);
                }
                catch (FormatException) { return null; }
                catch (InvalidCastException) { return null; }
                catch (OverflowException) { return null; }
            }

            bool? GetBool(string name)
            {
                var i = GetInt(name);
                return i is null ? null : i != 0;
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
                LogViewPollSeconds = logViewPoll is null ? current.LogViewPollSeconds : Clamp(logViewPoll.Value, 5, 300),
                LogQueueMaxDepthPerModule = queueDepth is null ? current.LogQueueMaxDepthPerModule : Clamp(queueDepth.Value, 500, 50000),
                LogCircuitErrorThreshold = circuitThreshold is null ? current.LogCircuitErrorThreshold : Clamp(circuitThreshold.Value, 5, 500),
                LogCircuitErrorWindowSeconds = circuitWindow is null ? current.LogCircuitErrorWindowSeconds : Clamp(circuitWindow.Value, 10, 3600),
                LogFailoverEnabled = failoverEnabled ?? current.LogFailoverEnabled
            };
        }
        catch (System.Security.SecurityException)
        {
            // Insufficient privilege to read; ignore and retain current.
            return current;
        }
        catch (UnauthorizedAccessException)
        {
            return current;
        }
        catch (IOException)
        {
            return current;
        }
        catch (ObjectDisposedException)
        {
            return current;
        }
    }

    /// <summary>
    /// Writes explicit overrides for machine scope (only selected fields). Null leaves existing value untouched.
    /// </summary>
    /// <param name="configure">Delegate configuring fields to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    public static void WriteMachineOverrides(Action<BehaviorPolicyOverrideBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new BehaviorPolicyOverrideBuilder();
        configure(builder);
        WriteOverrides(RegistryHive.LocalMachine, MachinePath, builder);
    }

    /// <summary>
    /// Writes explicit overrides for user scope (only allowed user-scoped fields).
    /// </summary>
    /// <param name="configure">Delegate configuring fields to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    public static void WriteUserOverrides(Action<BehaviorPolicyOverrideBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
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

            void Set(string name, object value) => key.SetValue(name, value);

            if (b.LogRetentionDays.HasValue) Set("LogRetentionDays", b.LogRetentionDays.Value);
            if (b.MaxLogFileSizeMb.HasValue) Set("MaxLogFileSizeMB", b.MaxLogFileSizeMb.Value);
            if (b.MinLogLevel is not null) Set("MinLogLevel", b.MinLogLevel);
            if (b.UiLanguage is not null) Set("UiLanguage", b.UiLanguage);
            if (b.EnableTelemetry.HasValue) Set("EnableTelemetry", b.EnableTelemetry.Value ? 1 : 0);
            if (b.AllowedGroupsCsv is not null) Set("AllowedGroupsCsv", b.AllowedGroupsCsv);
            if (b.LogViewPollSeconds.HasValue) Set("LogViewPollSeconds", b.LogViewPollSeconds.Value);
            if (b.LogQueueMaxDepthPerModule.HasValue) Set("LogQueueMaxDepthPerModule", b.LogQueueMaxDepthPerModule.Value);
            if (b.LogCircuitErrorThreshold.HasValue) Set("LogCircuitErrorThreshold", b.LogCircuitErrorThreshold.Value);
            if (b.LogCircuitErrorWindowSeconds.HasValue) Set("LogCircuitErrorWindowSeconds", b.LogCircuitErrorWindowSeconds.Value);
            if (b.LogFailoverEnabled.HasValue) Set("LogFailoverEnabled", b.LogFailoverEnabled.Value ? 1 : 0);
        }
        catch (UnauthorizedAccessException) { }
        catch (System.Security.SecurityException) { }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
    }
}

/// <summary>
/// Builder for selective registry override persistence.
/// </summary>
public sealed class BehaviorPolicyOverrideBuilder
{
    /// <summary>Log retention in days.</summary>
    public int? LogRetentionDays { get; set; }
    /// <summary>Max log file size in megabytes.</summary>
    public int? MaxLogFileSizeMb { get; set; }
    /// <summary>Minimum log level (e.g. Information, Warning).</summary>
    public string? MinLogLevel { get; set; }
    /// <summary>Preferred UI language tag (e.g. en-US).</summary>
    public string? UiLanguage { get; set; }
    /// <summary>Telemetry enable flag.</summary>
    public bool? EnableTelemetry { get; set; }
    /// <summary>Comma separated list of allowed group names.</summary>
    public string? AllowedGroupsCsv { get; set; }
    /// <summary>Polling interval (seconds) for log view refresh.</summary>
    public int? LogViewPollSeconds { get; set; }
    /// <summary>Maximum queued log entries per module.</summary>
    public int? LogQueueMaxDepthPerModule { get; set; }
    /// <summary>Error threshold to trigger circuit breaker.</summary>
    public int? LogCircuitErrorThreshold { get; set; }
    /// <summary>Error window size (seconds) for circuit breaker aggregation.</summary>
    public int? LogCircuitErrorWindowSeconds { get; set; }
    /// <summary>Enables fallback log file path when primary fails.</summary>
    public bool? LogFailoverEnabled { get; set; }
}