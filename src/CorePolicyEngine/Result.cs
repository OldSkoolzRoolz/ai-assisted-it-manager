// Project Name: CorePolicyEngine
// File Name: Result.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine;

/// <summary>
/// Lightweight result primitive (duplicated per-module to satisfy modular isolation requirement).
/// </summary>
public class Result
{
    protected readonly List<string> _errors = [];
    public bool Success { get; private set; }
    public IReadOnlyList<string> Errors => _errors;

    public static Result Ok() => new() { Success = true };
    public static Result Fail(params string[] errors)
    {
        var r = new Result { Success = false }; r._errors.AddRange(errors); return r;
    }
    public static Result Combine(params Result[] results)
    {
        var r = new Result();
        foreach (var x in results) if (!x.Success) r._errors.AddRange(x.Errors);
        r.Success = r._errors.Count == 0; return r;
    }
    protected void SetSuccess(bool success) => Success = success;
}

public class Result<T> : Result
{
    private Result() => SetSuccess(true);
    public T? Value { get; private set; }
    public static Result<T> Ok(T value) => new() { Value = value };
    public static new Result<T> Fail(params string[] errors)
    { var r = new Result<T>(); r._errors.AddRange(errors); r.SetSuccess(false); return r; }
}
