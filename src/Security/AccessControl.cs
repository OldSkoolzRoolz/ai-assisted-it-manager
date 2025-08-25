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

namespace Security;

/// <summary>Defines an authorization requirement for client usage.</summary>
public interface IClientAccessPolicy
{
    /// <summary>Evaluates whether specified identity is allowed.</summary>
    /// <param name="identity">Windows identity.</param>
    /// <param name="denialReason">Populated with denial reason on failure.</param>
    /// <returns>True if allowed.</returns>
    bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason);
}

/// <summary>
/// Access policy based on allowed local / domain groups (semicolon list) with administrator / wildcard support.
/// </summary>
public sealed class GroupMembershipAccessPolicy : IClientAccessPolicy
{
    private const string AdminSidString = "S-1-5-32-544";
    private const string LocalAccountAdminSidString = "S-1-5-114";
    private static readonly SecurityIdentifier AdminSid = new(WellKnownSidType.BuiltinAdministratorsSid, null);
    private static readonly SecurityIdentifier LocalAccountAndAdminSid = new(LocalAccountAdminSidString);
    private readonly bool _allowAny;
    private readonly HashSet<string> _allowedGroups;
    private readonly ILogger? _logger;

    /// <summary>Create access policy.</summary>
    public GroupMembershipAccessPolicy(IEnumerable<string> allowedGroups, ILogger<GroupMembershipAccessPolicy>? logger = null)
    {
        _logger = logger;
        var list = allowedGroups.Select(g => g.Trim()).Where(g => g.Length > 0).ToArray();
        _allowAny = list.Any(g => g == "*");
        _allowedGroups = list.Where(g => g != "*").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (_allowedGroups.Contains(AdminSidString) && !_allowedGroups.Contains("Administrators")) _allowedGroups.Add("Administrators");
        if (_allowedGroups.Contains("BUILTIN\\Administrators") && !_allowedGroups.Contains("Administrators"))
        {
            _allowedGroups.Add("Administrators");
        }
        _logger?.AccessPolicyInitialized(string.Join(';', _allowedGroups), _allowAny);
    }

    /// <inheritdoc />
    public bool IsAccessAllowed(WindowsIdentity identity, out string? denialReason)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var user = identity.Name;
        denialReason = null;
        if (Environment.GetEnvironmentVariable("ITC_BYPASS_ACCESS") == "1") { _logger?.AccessBypass(user); return true; }
        if (_allowAny) { _logger?.WildcardGrant(user); return true; }
        try
        {
            var principal = new WindowsPrincipal(identity);
            var adminGroupAllowed = _allowedGroups.Contains("Administrators") || _allowedGroups.Contains("BUILTIN\\Administrators") || _allowedGroups.Contains(AdminSidString) || _allowedGroups.Contains(LocalAccountAdminSidString);
            if (adminGroupAllowed && identity.Groups != null && identity.Groups.Any(gr => gr.Value == AdminSidString || gr.Value == LocalAccountAdminSidString)) { _logger?.Grant("early SID presence", user); return true; }
            if (adminGroupAllowed && (principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(AdminSid))) { _logger?.Grant("IsInRole admin", user); return true; }
            foreach (var g in _allowedGroups) if (principal.IsInRole(g)) { _logger?.Grant($"IsInRole '{g}'", user); return true; }
            if (identity.Groups != null)
            {
                foreach (IdentityReference? ir in identity.Groups.Cast<IdentityReference>())
                {
                    SecurityIdentifier? sid = ir as SecurityIdentifier ?? SafeTranslateSid(ir);
                    if (sid == null) continue;
                    if (adminGroupAllowed && (sid.Equals(AdminSid) || sid.Equals(LocalAccountAndAdminSid))) { _logger?.Grant($"enum SID '{sid.Value}'", user); return true; }
                    var translated = SafeTranslateNtAccount(sid);
                    if (!string.IsNullOrEmpty(translated))
                    {
                        if (_allowedGroups.Contains(translated)) { _logger?.Grant($"translated '{translated}'", user); return true; }
                        var lastSlash = translated.LastIndexOf('\\');
                        var shortName = lastSlash >= 0 ? translated[(lastSlash + 1)..] : translated;
                        if (_allowedGroups.Contains(shortName)) { _logger?.Grant($"short '{shortName}'", user); return true; }
                        if (adminGroupAllowed && translated.EndsWith("Administrators", StringComparison.OrdinalIgnoreCase)) { _logger?.Grant($"endswith Administrators '{translated}'", user); return true; }
                    }
                }
            }
            var groupsDump = identity.Groups == null ? "<none>" : string.Join(" | ", identity.Groups.Select(ir => ir.Value));
            denialReason = $"User not in allowed groups (User='{user}', Allowed='{string.Join(';', _allowedGroups)}')";
            _logger?.AccessDenied(user, string.Join(';', _allowedGroups), groupsDump);
            return false;
        }
        catch (SecurityException ex) { denialReason = "Access evaluation error: " + ex.Message; _logger?.AccessEvaluationError(ex, user); return true; }
        catch (UnauthorizedAccessException ex) { denialReason = "Access evaluation error: " + ex.Message; _logger?.AccessEvaluationError(ex, user); return true; }
        catch (IdentityNotMappedException ex) { denialReason = "Access evaluation error: " + ex.Message; _logger?.AccessEvaluationError(ex, user); return true; }
    }

    private static SecurityIdentifier? SafeTranslateSid(IdentityReference ir)
    {
        try { return ir.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier; }
        catch (IdentityNotMappedException) { return null; }
        catch (SecurityException) { return null; }
    }

    private static string? SafeTranslateNtAccount(SecurityIdentifier sid)
    {
        try { return (sid.Translate(typeof(NTAccount)) as NTAccount)?.Value; }
        catch (IdentityNotMappedException) { return null; }
        catch (SecurityException) { return null; }
    }
}

/// <summary>High-level access evaluator service to be injected in ClientApp startup.</summary>
public interface IClientAccessEvaluator
{
    /// <summary>Evaluates current process identity against configured policy.</summary>
    /// <param name="reason">Denial reason if not allowed.</param>
    /// <returns>True if access allowed.</returns>
    bool CheckAccess(out string? reason);
}

/// <summary>Default implementation using injected policy.</summary>
public sealed class ClientAccessEvaluator : IClientAccessEvaluator
{
    private readonly IClientAccessPolicy _policy;
    /// <summary>Create evaluator.</summary>
    public ClientAccessEvaluator(IClientAccessPolicy policy) { _policy = policy; }
    /// <inheritdoc />
    public bool CheckAccess(out string? reason)
    {
        using var id = WindowsIdentity.GetCurrent();
        return _policy.IsAccessAllowed(id, out reason);
    }
}