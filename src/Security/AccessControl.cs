// Project Name: Security
// File Name: SecurityAccess.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace KC.ITCompanion.Security;

/// <summary>
/// Defines an authorization requirement for client usage.
/// </summary>
public interface IClientAccessPolicy
{
    bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason);
}

/// <summary>
/// Access policy based on allowed local / domain groups.
/// Supports:
///  - Semicolon-separated group list (DOMAIN\\Group or Group)
///  - Wildcard '*' to allow any authenticated user (development / fallback)
///  - Builtin Administrators SID detection even for filtered (non‑elevated) UAC tokens
///  - Raw SID token (S-1-5-32-544) equivalence for Administrators
///  - Optional environment bypass: ITC_BYPASS_ACCESS=1
/// </summary>
public sealed class GroupMembershipAccessPolicy : IClientAccessPolicy
{
    private readonly HashSet<string> _allowedGroups; // normalized group names (case-insensitive)
    private readonly bool _allowAny;
    private static readonly SecurityIdentifier AdminSid = new(WellKnownSidType.BuiltinAdministratorsSid, null);           // S-1-5-32-544
    private static readonly SecurityIdentifier LocalAccountAndAdminSid = new("S-1-5-114"); // NT AUTHORITY\Local account and member of Administrators group (deny-only filtered marker)
    private const string AdminSidString = "S-1-5-32-544";
    private const string LocalAccountAdminSidString = "S-1-5-114";
    private readonly ILogger? _logger;

    public GroupMembershipAccessPolicy(IEnumerable<string> allowedGroups, ILogger<GroupMembershipAccessPolicy>? logger = null)
    {
        _logger = logger;
        var list = allowedGroups.Select(g => g.Trim()).Where(g => g.Length > 0).ToArray();
        _allowAny = list.Any(g => g == "*");
        _allowedGroups = list.Where(g => g != "*").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (_allowedGroups.Contains(AdminSidString) && !_allowedGroups.Contains("Administrators")) _allowedGroups.Add("Administrators");
        if (_allowedGroups.Contains("BUILTIN\\Administrators") && !_allowedGroups.Contains("Administrators")) _allowedGroups.Add("Administrators");
        LogDebug($"Init allowed='{string.Join(';', _allowedGroups)}' allowAny={_allowAny}");
    }

    public bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason)
    {
        var user = identity.Name; denialReason = null;
        if (Environment.GetEnvironmentVariable("ITC_BYPASS_ACCESS") == "1") { LogInfo($"Bypass env var for '{user}'"); return true; }
        if (_allowAny) { LogInfo($"Wildcard grant '{user}'"); return true; }

        try
        {
            var principal = new WindowsPrincipal(identity);
            bool adminGroupAllowed = _allowedGroups.Contains("Administrators") || _allowedGroups.Contains("BUILTIN\\Administrators") || _allowedGroups.Contains(AdminSidString) || _allowedGroups.Contains(LocalAccountAdminSidString);

            // EARLY SID PRESENCE GRANT (handles deny-only filtered tokens before IsInRole checks)
            if (adminGroupAllowed && identity.Groups != null && identity.Groups.Cast<IdentityReference>().Any(gr => gr.Value == AdminSidString || gr.Value == LocalAccountAdminSidString))
            {
                LogInfo($"Grant (early SID presence) '{user}'");
                return true;
            }

            // Standard role API checks (may fail for deny-only)
            if (adminGroupAllowed && (principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(AdminSid)))
            { LogInfo($"Grant (IsInRole admin) '{user}'"); return true; }

            // Explicit group names
            foreach (var g in _allowedGroups) if (principal.IsInRole(g)) { LogInfo($"Grant (IsInRole '{g}') '{user}'"); return true; }

            // Enumerate groups for translated matches / fallback
            if (identity.Groups != null)
            {
                foreach (var ir in identity.Groups.Cast<IdentityReference>())
                {
                    SecurityIdentifier? sid = ir as SecurityIdentifier ?? SafeTranslateSid(ir);
                    if (sid == null) continue;
                    if (adminGroupAllowed && (sid.Equals(AdminSid) || sid.Equals(LocalAccountAndAdminSid))) { LogInfo($"Grant (enum SID '{sid.Value}') '{user}'"); return true; }
                    var translated = SafeTranslateNtAccount(sid);
                    if (!string.IsNullOrEmpty(translated))
                    {
                        if (_allowedGroups.Contains(translated)) { LogInfo($"Grant (translated '{translated}') '{user}'"); return true; }
                        var shortName = translated.Contains('\\') ? translated[(translated.LastIndexOf('\\') + 1)..] : translated;
                        if (_allowedGroups.Contains(shortName)) { LogInfo($"Grant (short '{shortName}') '{user}'"); return true; }
                        if (adminGroupAllowed && translated.EndsWith("Administrators", StringComparison.OrdinalIgnoreCase)) { LogInfo($"Grant (endswith Administrators '{translated}') '{user}'"); return true; }
                    }
                }
            }

            // Deny: elevate diagnostics to Information for visibility (since default level may filter Debug)
            var groupsDump = identity.Groups == null ? "<none>" : string.Join(" | ", identity.Groups.Cast<IdentityReference>().Select(ir => ir.Value));
            denialReason = $"User not in allowed groups (User='{user}', Allowed='{string.Join(';', _allowedGroups)}')";
            LogWarn(denialReason + $" tokenGroups=" + groupsDump);
            return false;
        }
        catch (Exception ex)
        {
            denialReason = "Access evaluation error: " + ex.Message;
            LogError(ex, $"Access evaluation error '{user}' fail-open");
            return true;
        }
    }

    private static SecurityIdentifier? SafeTranslateSid(IdentityReference ir)
    { try { return ir.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier; } catch { return null; } }
    private static string? SafeTranslateNtAccount(SecurityIdentifier sid)
    { try { return (sid.Translate(typeof(NTAccount)) as NTAccount)?.Value; } catch { return null; } }

    private void LogDebug(string msg) { _logger?.LogDebug(msg); DebugWrite(msg); }
    private void LogInfo(string msg) { _logger?.LogInformation(msg); DebugWrite(msg); }
    private void LogWarn(string msg) { _logger?.LogWarning(msg); DebugWrite(msg); }
    private void LogError(Exception ex, string msg) { _logger?.LogError(ex, msg); DebugWrite(msg + " ex=" + ex.Message); }
    private static void DebugWrite(string message) { try { Debug.WriteLine("[AccessPolicy] " + message); } catch { } }
}

/// <summary>
/// High-level access evaluator service to be injected in ClientApp startup.
/// </summary>
public interface IClientAccessEvaluator
{
    bool CheckAccess(out string? reason);
}

public sealed class ClientAccessEvaluator : IClientAccessEvaluator
{
    private readonly IClientAccessPolicy _policy;
    public ClientAccessEvaluator(IClientAccessPolicy policy) => _policy = policy;
    public bool CheckAccess(out string? reason)
    {
        using var id = WindowsIdentity.GetCurrent();
        return _policy.IsAccessAllowed(id, out reason);
    }
}


