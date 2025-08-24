// Project Name: CorePolicyEngine
// File Name: PolicyRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Data;
using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>
/// Policy definition row (flattened mapping to registry value semantics).
/// </summary>
/// <param name="PolicyDefId">Stable identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Category">Category path.</param>
/// <param name="RegistryRoot">Root hive name.</param>
/// <param name="RegistryPath">Relative registry path.</param>
/// <param name="ValueName">Registry value name.</param>
/// <param name="ValueType">Value kind.</param>
/// <param name="Deprecated">Deprecated flag.</param>
public sealed record PolicyDefinitionDto(
    string PolicyDefId,
    string Name,
    string Category,
    string RegistryRoot,
    string RegistryPath,
    string ValueName,
    string ValueType,
    bool Deprecated);

/// <summary>
/// Policy group definition metadata.
/// </summary>
/// <param name="PolicyGroupId">Id.</param>
/// <param name="Name">Group name.</param>
/// <param name="Priority">Evaluation priority (higher first).</param>
public sealed record PolicyGroupDto(string PolicyGroupId, string Name, int Priority);

/// <summary>
/// Policy group item row (membership + desired state).
/// </summary>
/// <param name="PolicyGroupId">Group id.</param>
/// <param name="PolicyDefId">Policy definition id.</param>
/// <param name="Enabled">Enabled flag.</param>
/// <param name="DesiredValue">Desired registry value (string serialized).</param>
/// <param name="Enforced">Whether enforcement is strict.</param>
public sealed record PolicyGroupItemDto(
    string PolicyGroupId,
    string PolicyDefId,
    bool Enabled,
    string? DesiredValue,
    bool Enforced);

/// <summary>Definition repository contract.</summary>
public interface IPolicyDefinitionRepository
{
    /// <summary>Gets all non-deprecated policy definitions.</summary>
    Task<IReadOnlyList<PolicyDefinitionDto>> GetAllActiveAsync(CancellationToken token);
}

/// <summary>Policy group repository contract.</summary>
public interface IPolicyGroupRepository
{
    /// <summary>Gets groups.</summary>
    Task<IReadOnlyList<PolicyGroupDto>> GetGroupsAsync(CancellationToken token);
    /// <summary>Gets items for a group.</summary>
    Task<IReadOnlyList<PolicyGroupItemDto>> GetGroupItemsAsync(string policyGroupId, CancellationToken token);
}

/// <summary>
/// SQL implementation of <see cref="IPolicyDefinitionRepository"/>.
/// </summary>
public sealed class PolicyDefinitionRepository : IPolicyDefinitionRepository
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>Create repository.</summary>
    public PolicyDefinitionRepository(ISqlConnectionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PolicyDefinitionDto>> GetAllActiveAsync(CancellationToken token)
    {
        const string sql =
            @"SELECT PolicyDefId,Name,Category,RegistryRoot,RegistryPath,ValueName,ValueType,Deprecated FROM dbo.PolicyDefinition WHERE Deprecated = 0 ORDER BY Category, Name";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        List<PolicyDefinitionDto> list = new(256);
        using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
            list.Add(new PolicyDefinitionDto(
                r.GetString(0), r.GetString(1), r.GetString(2),
                r.GetString(3), r.GetString(4), r.GetString(5),
                r.GetString(6), r.GetBoolean(7)));
        return list;
    }
}

/// <summary>
/// SQL implementation of <see cref="IPolicyGroupRepository"/>.
/// </summary>
public sealed class PolicyGroupRepository : IPolicyGroupRepository
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>Create repository.</summary>
    public PolicyGroupRepository(ISqlConnectionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PolicyGroupDto>> GetGroupsAsync(CancellationToken token)
    {
        const string sql = @"SELECT PolicyGroupId,Name,Priority FROM dbo.PolicyGroup ORDER BY Priority DESC, Name";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        List<PolicyGroupDto> list = new();
        using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
            list.Add(new PolicyGroupDto(r.GetString(0), r.GetString(1), r.GetInt32(2)));
        return list;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PolicyGroupItemDto>> GetGroupItemsAsync(string policyGroupId,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyGroupId);
        const string sql =
            @"SELECT PolicyGroupId,PolicyDefId,Enabled,DesiredValue,Enforced FROM dbo.PolicyGroupItem WHERE PolicyGroupId=@id ORDER BY PolicyDefId";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = "@id";
        p.Value = policyGroupId;
        cmd.Parameters.Add(p);
        List<PolicyGroupItemDto> list = new();
        using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
        {
            string? desired = await r.IsDBNullAsync(3, token).ConfigureAwait(false) ? null : r.GetString(3);
            list.Add(new PolicyGroupItemDto(r.GetString(0), r.GetString(1), r.GetBoolean(2), desired, r.GetBoolean(4)));
        }
        return list;
    }
}