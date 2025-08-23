// Project Name: Security
// File Name: AccessControl.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Security;
using System.Security.Principal;

using Microsoft.Extensions.Logging;

using Security.Logging;
// for SecurityException


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
///     Supports:
///     - Semicolon-separated group list (DOMAIN\\Group or Group)
///     - Wildcard '*' to allow any authenticated user (development / fallback)
///     - Builtin Administrators SID detection even for filtered (non‑elevated) UAC tokens
///     - Raw SID token (S-1-5-32-544) equivalence for Administrators
///     - Optional environment bypass: ITC_BYPASS_ACCESS=1
/// </summary>
public sealed class GroupMembershipAccessPolicy : IClientAccessPolicy
{
    private const string AdminSidString = "S-1-5-32-544";
    private const string LocalAccountAdminSidString = "S-1-5-114";

    private static readonly SecurityIdentifier
        AdminSid = new(WellKnownSidType.BuiltinAdministratorsSid, null); // S-1-5-32-544

    private static readonly SecurityIdentifier
        LocalAccountAndAdminSid =
            new("S-1-5-114"); // NT AUTHORITY\Local account and member of Administrators group (deny-only filtered marker)

    private readonly bool _allowAny;
    private readonly HashSet<string> _allowedGroups; // normalized group names (case-insensitive)
    private readonly ILogger? _logger;





    public GroupMembershipAccessPolicy(IEnumerable<string> allowedGroups,
        ILogger<GroupMembershipAccessPolicy>? logger = null)
    {
        this._logger = logger;
        var list = allowedGroups.Select(g => g.Trim()).Where(g => g.Length > 0).ToArray();
        this._allowAny = list.Any(g => g == "*");
        this._allowedGroups = list.Where(g => g != "*").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (this._allowedGroups.Contains(AdminSidString) && !this._allowedGroups.Contains("Administrators"))
            this._allowedGroups.Add("Administrators");
        if (this._allowedGroups.Contains("BUILTIN\\Administrators") && !this._allowedGroups.Contains("Administrators"))
            this._allowedGroups.Add("Administrators");
        this._logger?.AccessPolicyInitialized(string.Join(';', this._allowedGroups), this._allowAny);
    }





    public bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var user = identity.Name;
        denialReason = null;
        if (Environment.GetEnvironmentVariable("ITC_BYPASS_ACCESS") == "1")
        {
            this._logger?.AccessBypass(user);
            return true;
        }

        if (this._allowAny)
        {
            this._logger?.WildcardGrant(user);
            return true;
        }

        try
        {
            var principal = new WindowsPrincipal(identity);
            var adminGroupAllowed = this._allowedGroups.Contains("Administrators") ||
                                    this._allowedGroups.Contains("BUILTIN\\Administrators") ||
                                    this._allowedGroups.Contains(AdminSidString) ||
                                    this._allowedGroups.Contains(LocalAccountAdminSidString);

            // EARLY SID PRESENCE GRANT (handles deny-only filtered tokens before IsInRole checks)
            if (adminGroupAllowed && identity.Groups != null && identity.Groups.Any(gr =>
                    gr.Value == AdminSidString || gr.Value == LocalAccountAdminSidString))
            {
                this._logger?.Grant("early SID presence", user);
                return true;
            }

            // Standard role API checks (may fail for deny-only)
            if (adminGroupAllowed &&
                (principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(AdminSid)))
            {
                this._logger?.Grant("IsInRole admin", user);
                return true;
            }

            // Explicit group names
            foreach (var g in this._allowedGroups)
                if (principal.IsInRole(g))
                {
                    this._logger?.Grant($"IsInRole '{g}'", user);
                    return true;
                }

            // Enumerate groups for translated matches / fallback
            if (identity.Groups != null)
                foreach (IdentityReference? ir in identity.Groups.Cast<IdentityReference>())
                {
                    SecurityIdentifier? sid = ir as SecurityIdentifier ?? SafeTranslateSid(ir);
                    if (sid == null) continue;
                    if (adminGroupAllowed && (sid.Equals(AdminSid) || sid.Equals(LocalAccountAndAdminSid)))
                    {
                        this._logger?.Grant($"enum SID '{sid.Value}'", user);
                        return true;
                    }

                    var translated = SafeTranslateNtAccount(sid);
                    if (!string.IsNullOrEmpty(translated))
                    {
                        if (this._allowedGroups.Contains(translated))
                        {
                            this._logger?.Grant($"translated '{translated}'", user);
                            return true;
                        }

                        var lastSlash = translated.LastIndexOf('\\');
                        var shortName = lastSlash >= 0 ? translated[(lastSlash + 1)..] : translated;
                        if (this._allowedGroups.Contains(shortName))
                        {
                            this._logger?.Grant($"short '{shortName}'", user);
                            return true;
                        }

                        if (adminGroupAllowed &&
                            translated.EndsWith("Administrators", StringComparison.OrdinalIgnoreCase))
                        {
                            this._logger?.Grant($"endswith Administrators '{translated}'", user);
                            return true;
                        }
                    }
                }

            // Deny: elevate diagnostics to Information for visibility (since default level may filter Debug)
            var groupsDump = identity.Groups == null
                ? "<none>"
                : string.Join(" | ", identity.Groups.Select(ir => ir.Value));
            denialReason =
                $"User not in allowed groups (User='{user}', Allowed='{string.Join(';', this._allowedGroups)}')";
            this._logger?.AccessDenied(user, string.Join(';', this._allowedGroups), groupsDump);
            return false;
        }
        catch (SecurityException ex)
        {
            denialReason = "Access evaluation error: " + ex.Message;
            this._logger?.AccessEvaluationError(ex, user);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            denialReason = "Access evaluation error: " + ex.Message;
            this._logger?.AccessEvaluationError(ex, user);
            return true;
        }
        catch (SystemException ex) when (ex is IdentityNotMappedException)
        {
            denialReason = "Access evaluation error: " + ex.Message;
            this._logger?.AccessEvaluationError(ex, user);
            return true;
        }
    }





    private static SecurityIdentifier? SafeTranslateSid(IdentityReference ir)
    {
        try
        {
            return ir.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
        }
        catch (IdentityNotMappedException)
        {
            return null;
        }
        catch (SystemException)
        {
            return null; // includes potential SecurityException, etc.
        }
    }





    private static string? SafeTranslateNtAccount(SecurityIdentifier sid)
    {
        try
        {
            return (sid.Translate(typeof(NTAccount)) as NTAccount)?.Value;
        }
        catch (IdentityNotMappedException)
        {
            return null;
        }
        catch (SystemException)
        {
            return null;
        }
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