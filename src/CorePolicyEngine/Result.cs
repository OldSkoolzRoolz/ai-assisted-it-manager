// Project Name: CorePolicyEngine
// File Name: Result.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


namespace KC.ITCompanion.CorePolicyEngine;


/// <summary>
///     Lightweight result primitive (duplicated per-module to satisfy modular isolation requirement).
/// </summary>
public class Result
{
    protected readonly List<string> _errors = [];
    public bool Success { get; private set; }
    public IReadOnlyList<string> Errors => this._errors;





    public static Result Ok()
    {
        return new Result { Success = true };
    }





    public static Result Fail(params string[] errors)
    {
        var r = new Result { Success = false };
        r._errors.AddRange(errors);
        return r;
    }





    public static Result Combine(params Result[] results)
    {
        var r = new Result();
        foreach (Result x in results)
            if (!x.Success)
                r._errors.AddRange(x.Errors);
        r.Success = r._errors.Count == 0;
        return r;
    }





    protected void SetSuccess(bool success)
    {
        this.Success = success;
    }
}



public class Result<T> : Result
{
    private Result()
    {
        SetSuccess(true);
    }





    public T? Value { get; private set; }





    public static Result<T> Ok(T value)
    {
        return new Result<T> { Value = value };
    }





    public new static Result<T> Fail(params string[] errors)
    {
        Result<T> r = new();
        r._errors.AddRange(errors);
        r.SetSuccess(false);
        return r;
    }
}