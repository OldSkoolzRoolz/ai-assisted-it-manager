// Project Name: Shared
// File Name: AdmxModels.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace Shared;

// NOTE:
// Legacy simplified ADMX domain model types (AdmxCatalog, AdmxPolicy, etc.) have been removed.
// The solution now uses the richer CorePolicyEngine.AdminTemplates model set (AdmxDocument, AdmlDocument, Policy, etc.).
// Any remaining references to the removed types should be refactored to the new model.
// This file now only retains Result/Result<T> primitives shared across layers.

public class Result
{
    protected readonly List<string> _errors = [];
    public bool Success { get; private set; }
    public IReadOnlyList<string> Errors => _errors;

    public static Result Ok() => new() { Success = true };

    public static Result Fail(params string[] errors)
    {
        Result r = new() { Success = false };
        r._errors.AddRange(errors);
        return r;
    }

    public static Result Combine(params Result[] results)
    {
        Result combined = new();
        foreach (Result r in results)
            if (!r.Success) combined._errors.AddRange(r.Errors);
        combined.Success = combined._errors.Count == 0;
        return combined;
    }

    protected void SetSuccess(bool success) => Success = success;
}

public class Result<T> : Result
{
    private Result() => SetSuccess(true);

    public T? Value { get; private set; }

    public static Result<T> Ok(T value) => new() { Value = value };

    public static new Result<T> Fail(params string[] errors)
    {
        Result<T> r = new();
        r._errors.AddRange(errors);
        r.SetSuccess(false);
        return r;
    }
}