using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shared;

// Basic Result pattern for non-exceptional validation/operation outcomes
public class Result
{
    public bool Success { get; private set; }
    public IReadOnlyList<string> Errors => _errors;
    protected readonly List<string> _errors = new();

    public static Result Ok() => new() { Success = true };
    public static Result Fail(params string[] errors)
    {
        var r = new Result { Success = false };
        r._errors.AddRange(errors);
        return r;
    }
    public static Result Combine(params Result[] results)
    {
        var combined = new Result();
        foreach (var r in results)
        {
            if (!r.Success)
            {
                combined._errors.AddRange(r.Errors);
            }
        }
        combined.Success = combined._errors.Count == 0;
        return combined;
    }
    protected void SetSuccess(bool success) => Success = success;
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Ok(T value)
    {
        var r = new Result<T>();
        r.Value = value;
        return r;
    }
    public static new Result<T> Fail(params string[] errors)
    {
        var r = new Result<T>();
        r._errors.AddRange(errors);
        r.SetSuccess(false);
        return r;
    }

    private Result() { SetSuccess(true); }
}

// Core policy domain models (initial slice)
public enum PolicyValueType
{
    Boolean,
    Enum,
    Numeric,
    Text
}

public record PolicyPartDefinition(string Id, PolicyValueType ValueType, string? EnumId = null, decimal? Min = null, decimal? Max = null);
public record PolicyEnumItem(string Name, string Value);
public record PolicyEnum(string Id, IReadOnlyList<PolicyEnumItem> Items);

public record AdmxPolicy(string Id,
                         string Name,
                         string CategoryId,
                         bool UserScope,
                         bool MachineScope,
                         IReadOnlyList<PolicyPartDefinition> Parts,
                         string? SupportedOn);

public record AdmxCategory(string Id, string Name, string? ParentId);

public record AdmxCatalog(IReadOnlyList<AdmxCategory> Categories,
                          IReadOnlyList<AdmxPolicy> Policies,
                          IReadOnlyList<PolicyEnum> Enums,
                          string? Culture);

public record PolicySetting(string PolicyId,
                            string? PartId,
                            bool Enabled,
                            string? Value,
                            PolicyValueType ValueType);

public record PolicySet(string Id,
                         string Name,
                         string TargetScope,
                         IReadOnlyList<PolicySetting> Settings);

public enum ValidationSeverity { Info, Warning, Error }
public record ValidationMessage(string PolicyId, string? PartId, ValidationSeverity Severity, string Message);
public record ValidationResult(bool Success, IReadOnlyList<ValidationMessage> Messages)
{
    public static ValidationResult FromMessages(IEnumerable<ValidationMessage> msgs)
    {
        var list = msgs.ToList();
        var success = list.TrueForAll(m => m.Severity != ValidationSeverity.Error);
        return new ValidationResult(success, list);
    }
}

// Service abstractions
public interface IAdmxCatalogLoader
{
    Task<Result<AdmxCatalog>> LoadAsync(IReadOnlyList<string> definitionPaths, string? culture, CancellationToken cancellationToken);
}

public interface IValidationRule
{
    string Id { get; }
    IEnumerable<ValidationMessage> Evaluate(PolicySet policySet, AdmxCatalog catalog, ValidationContext context);
}

public record ValidationContext(string TargetOsProfile);

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync(PolicySet policySet, AdmxCatalog catalog, CancellationToken cancellationToken);
}

public interface IDeploymentService
{
    Task<Result<string>> DryRunAsync(PolicySet policySet, CancellationToken cancellationToken); // returns diff JSON
    Task<Result> ApplyAsync(PolicySet policySet, CancellationToken cancellationToken);
}

public interface IVersioningService
{
    Task<Result<int>> CommitAsync(PolicySet policySet, string? message, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<PolicySet>>> GetHistoryAsync(string policySetId, CancellationToken cancellationToken);
    Task<Result> RollbackAsync(string policySetId, int version, CancellationToken cancellationToken);
}

public interface IPolicyRepository
{
    Task<Result<PolicySet>> GetAsync(string id, CancellationToken cancellationToken);
    Task<Result> SaveAsync(PolicySet set, CancellationToken cancellationToken);
}
