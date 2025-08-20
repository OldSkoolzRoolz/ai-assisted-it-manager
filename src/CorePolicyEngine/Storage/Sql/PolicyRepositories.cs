// Project Name: CorePolicyEngine
// File Name: PolicyRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePersistence.Sql;

public sealed record PolicyDefinitionDto(string PolicyDefId,string Name,string Category,string RegistryRoot,string RegistryPath,string ValueName,string ValueType,bool Deprecated);
public sealed record PolicyGroupDto(string PolicyGroupId,string Name,int Priority);
public sealed record PolicyGroupItemDto(string PolicyGroupId,string PolicyDefId,bool Enabled,string? DesiredValue,bool Enforced);

public interface IPolicyDefinitionRepository
{
    Task<IReadOnlyList<PolicyDefinitionDto>> GetAllActiveAsync(CancellationToken token);
}

public interface IPolicyGroupRepository
{
    Task<IReadOnlyList<PolicyGroupDto>> GetGroupsAsync(CancellationToken token);
    Task<IReadOnlyList<PolicyGroupItemDto>> GetGroupItemsAsync(string policyGroupId, CancellationToken token);
}

public sealed class PolicyDefinitionRepository : IPolicyDefinitionRepository
{
    private readonly ISqlConnectionFactory _factory;
    public PolicyDefinitionRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<PolicyDefinitionDto>> GetAllActiveAsync(CancellationToken token)
    {
        const string sql = @"SELECT PolicyDefId,Name,Category,RegistryRoot,RegistryPath,ValueName,ValueType,Deprecated FROM dbo.PolicyDefinition WHERE Deprecated = 0 ORDER BY Category, Name";
        using var conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var list = new List<PolicyDefinitionDto>(256);
        using var r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
        {
            list.Add(new PolicyDefinitionDto(
                r.GetString(0), r.GetString(1), r.GetString(2),
                r.GetString(3), r.GetString(4), r.GetString(5),
                r.GetString(6), r.GetBoolean(7)));
        }
        return list;
    }
}

public sealed class PolicyGroupRepository : IPolicyGroupRepository
{
    private readonly ISqlConnectionFactory _factory;
    public PolicyGroupRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<PolicyGroupDto>> GetGroupsAsync(CancellationToken token)
    {
        const string sql = @"SELECT PolicyGroupId,Name,Priority FROM dbo.PolicyGroup ORDER BY Priority DESC, Name";
        using var conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var list = new List<PolicyGroupDto>();
        using var r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
        {
            list.Add(new PolicyGroupDto(r.GetString(0), r.GetString(1), r.GetInt32(2)));
        }
        return list;
    }

    public async Task<IReadOnlyList<PolicyGroupItemDto>> GetGroupItemsAsync(string policyGroupId, CancellationToken token)
    {
        const string sql = @"SELECT PolicyGroupId,PolicyDefId,Enabled,DesiredValue,Enforced FROM dbo.PolicyGroupItem WHERE PolicyGroupId=@id ORDER BY PolicyDefId";
        using var conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var p = cmd.CreateParameter();
        p.ParameterName = "@id";
        p.Value = policyGroupId;
        cmd.Parameters.Add(p);
        var list = new List<PolicyGroupItemDto>();
        using var r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
        {
            list.Add(new PolicyGroupItemDto(r.GetString(0), r.GetString(1), r.GetBoolean(2), r.IsDBNull(3)? null : r.GetString(3), r.GetBoolean(4)));
        }
        return list;
    }
}
