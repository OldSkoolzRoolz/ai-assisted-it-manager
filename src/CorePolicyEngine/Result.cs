// Project Name: CorePolicyEngine
// File Name: Result.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Generic;

namespace KC.ITCompanion.CorePolicyEngine;

/// <summary>
/// Represents the outcome of an operation without a return value.
/// Use <see cref="Result{T}"/> when a value is produced.
/// </summary>
public class Result
{
    /// <summary>Internal mutable error buffer (exposed read-only via <see cref="Errors"/>).</summary>
    private readonly List<string> _errors = new();

    /// <summary>True when the operation succeeded.</summary>
    public bool Success { get; private set; }

    /// <summary>Collection of error messages (empty when <see cref="Success"/> is true).</summary>
    public IReadOnlyList<string> Errors => _errors;

    /// <summary>Create a success result.</summary>
    public static Result Ok() => new() { Success = true };

    /// <summary>Create a failed result with one or more error messages.</summary>
    /// <param name="errors">Error messages (null / empty entries ignored).</param>
    public static Result Fail(params string[] errors)
    {
        var r = new Result { Success = false };
        if (errors is not null)
        {
            foreach (var e in errors)
            {
                if (!string.IsNullOrWhiteSpace(e)) r._errors.Add(e);
            }
        }
        return r;
    }

    /// <summary>
    /// Combines multiple results; aggregated errors are returned if any failed.
    /// </summary>
    /// <param name="results">Results to combine.</param>
    /// <returns>Success if all succeeded; otherwise aggregated failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    public static Result Combine(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var r = new Result();
        foreach (var x in results)
        {
            if (x is null) continue; // ignore null entries defensively
            if (!x.Success) r._errors.AddRange(x.Errors);
        }
        r.Success = r._errors.Count == 0;
        return r;
    }

    /// <summary>Sets the success flag (used by derived types / factories).</summary>
    /// <param name="success">New success state.</param>
    protected void SetSuccess(bool success) => Success = success;

    /// <summary>Append an error message (used by derived types).</summary>
    /// <param name="message">Error message.</param>
    protected void AddError(string message)
    {
        if (!string.IsNullOrWhiteSpace(message)) _errors.Add(message);
        Success = false;
    }
}

/// <summary>
/// Represents the outcome of an operation that produces a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public sealed class Result<T> : Result
{
    internal Result() { }

    /// <summary>Gets the resulting value when <see cref="Result.Success"/> is true; otherwise default.</summary>
    public T? Value { get; private set; }

    internal static Result<T> FromSuccess(T value) => new() { Value = value, }; // Success set below

    internal static Result<T> FromFailure(IEnumerable<string>? errors)
    {
        var r = new Result<T>();
        if (errors is not null)
        {
            foreach (var e in errors)
            {
                if (!string.IsNullOrWhiteSpace(e)) r.AddError(e);
            }
        }
        return r;
    }

    /// <summary>Create a projection to a new Result mapping the value if successful.</summary>
    /// <typeparam name="TOut">New value type.</typeparam>
    /// <param name="project">Projection delegate.</param>
    /// <returns>Projected result (errors propagated).</returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> project)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (!Success) return ResultFactory.Fail<TOut>(Errors);
        return ResultFactory.Ok(project(Value!));
    }

    /// <summary>Creates a flat-mapped result by invoking a function returning a Result.</summary>
    /// <typeparam name="TOut">New value type.</typeparam>
    /// <param name="bind">Bind delegate producing a new result.</param>
    /// <returns>Bound result (errors propagated).</returns>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        if (!Success) return ResultFactory.Fail<TOut>(Errors);
        return bind(Value!);
    }

    internal void MarkSuccess(T value)
    {
        Value = value;
        SetSuccess(true);
    }
}

/// <summary>
/// Factory helpers for creating <see cref="Result"/> / <see cref="Result{T}"/> instances.
/// (Separating factories avoids CA1000 static member warnings on generic types.)
/// </summary>
public static class ResultFactory
{
    /// <summary>Create a successful Result&lt;T&gt;.</summary>
    public static Result<T> Ok<T>(T value)
    {
        var r = new Result<T>();
        r.MarkSuccess(value);
        return r;
    }

    /// <summary>Create a failed Result&lt;T&gt; from errors.</summary>
    public static Result<T> Fail<T>(params string[] errors) => Result<T>.FromFailure(errors);

    /// <summary>Create a failed Result&lt;T&gt; from errors (enumerable variant).</summary>
    public static Result<T> Fail<T>(IEnumerable<string> errors) => Result<T>.FromFailure(errors);
}