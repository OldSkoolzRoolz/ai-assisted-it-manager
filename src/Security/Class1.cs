// Project Name: Security
// File Name: SecurityAccess.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Security.Principal;

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
/// </summary>
public sealed class GroupMembershipAccessPolicy : IClientAccessPolicy
{
    private readonly HashSet<string> _allowedGroups; // normalized group names
    private readonly bool _allowAny;
    private static readonly SecurityIdentifier AdminSid = new(WellKnownSidType.BuiltinAdministratorsSid, null);

    public GroupMembershipAccessPolicy(IEnumerable<string> allowedGroups)
    {
        var list = allowedGroups
            .Select(g => g.Trim())
            .Where(g => g.Length > 0)
            .ToArray();
        _allowAny = list.Any(g => g == "*");
        _allowedGroups = list
            .Where(g => g != "*")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason)
    {
        denialReason = null;
        if (_allowAny) return true;
        try
        {
            var principal = new WindowsPrincipal(identity);
            // direct role names
            foreach (var g in _allowedGroups)
            {
                if (principal.IsInRole(g)) return true;
                // Unqualified group name might map to domain or local; already covered by IsInRole
            }
            // Fallback: inspect SIDs for admin membership (handles filtered token)
            if (identity.Groups != null)
            {
                foreach (var sid in identity.Groups)
                {
                    if (sid is SecurityIdentifier s && (s.Equals(AdminSid) || s.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)))
                    {
                        if (_allowedGroups.Contains("Administrators")) return true;
                    }
                    try
                    {
                        var name = sid.Translate(typeof(NTAccount)).Value;
                        if (_allowedGroups.Contains(name)) return true;
                    }
                    catch { }
                }
            }
            denialReason = "User not in allowed groups"; // TODO: LOC
            return false;
        }
        catch (Exception ex)
        {
            denialReason = "Access evaluation error: " + ex.Message; // TODO: LOC
            return true; // fail-open to avoid accidental lockout during development
        }
    }
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


