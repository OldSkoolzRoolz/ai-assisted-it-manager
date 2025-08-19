// Project Name: Security
// File Name: GroupMembershipAccessPolicy.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using System.Security.Claims;
using System.Security.Principal;


namespace Security;


/// <summary>
///     Defines an authorization requirement for client usage.
/// </summary>
public interface IClientAccessPolicy
{
    bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason);
}



/// <summary>
///     Access policy based on allowed local / domain groups.
/// </summary>
public sealed class GroupMembershipAccessPolicy : IClientAccessPolicy
{
    private readonly HashSet<string> _allowedGroups; // normalized (DOMAIN\Group)





    public GroupMembershipAccessPolicy(IEnumerable<string> allowedGroups)
    {
        this._allowedGroups = allowedGroups
            .Select(g => g.Trim())
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(Normalize)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }





    public bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason)
    {
        denialReason = null;
        try
        {
            var principal = new WindowsPrincipal(identity);
            foreach (var g in this._allowedGroups)
            {
                // Split DOMAIN\Group form
                var parts = g.Split('\\');
                var targetGroup = parts.Length == 2 ? parts[1] : g;
                if (principal.Claims.Any(c => c.Type == ClaimTypes.GroupSid || c.Type == ClaimTypes.Role))
                    // Fallback to Name test (may need SID mapping later)
                    if (principal.IsInRole(targetGroup))
                        return true;
                if (principal.IsInRole(g)) return true; // direct match attempt
            }

            denialReason = "User not in allowed groups"; // TODO: LOC
            return false;
        }
        catch (Exception ex)
        {
            denialReason = "Access evaluation error: " + ex.Message; // TODO: LOC
            return false;
        }
    }





    private static string Normalize(string raw)
    {
        return !raw.Contains('\\') && Environment.UserDomainName is { Length: > 0 } dom ? dom + "\\" + raw : raw;
    }
}



/// <summary>
///     High-level access evaluator service to be injected in ClientApp startup.
/// </summary>
public interface IClientAccessEvaluator
{
    bool CheckAccess(out string? reason);
}



public sealed class ClientAccessEvaluator : IClientAccessEvaluator
{
    private readonly IClientAccessPolicy _policy;





    public ClientAccessEvaluator(IClientAccessPolicy policy)
    {
        this._policy = policy;
    }





    public bool CheckAccess(out string? reason)
    {
        using var id = WindowsIdentity.GetCurrent();
        return this._policy.IsAccessAllowed(id, out reason);
    }
}