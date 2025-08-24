// Project Name: CorePolicyEngine
// File Name: RegOps.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved
// Do not remove file headers

using System;
using System.Collections.Generic;
using System.Linq;

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

/// <summary>
/// Input describing a single policy evaluation (policy + chosen state + element value map).
/// </summary>
/// <param name="Policy">Administrative policy definition being evaluated.</param>
/// <param name="State">Desired policy state.</param>
/// <param name="ElementValues">Provided element values keyed by element id.</param>
public sealed record PolicyEvaluationInput(
    AdminPolicy Policy,
    PolicyState State,
    IReadOnlyDictionary<ElementId, object?> ElementValues
);

/// <summary>
/// Effective configuration state applied during evaluation.
/// </summary>
public enum PolicyState
{
    /// <summary>Policy is explicitly enabled.</summary>
    Enabled,
    /// <summary>Policy is explicitly disabled.</summary>
    Disabled,
    /// <summary>Policy remains not configured (defaults apply).</summary>
    NotConfigured
}

/// <summary>
/// Evaluates policy definitions into concrete registry actions given supplied element values and state.
/// </summary>
public static class Evaluator
{
    /// <summary>
    /// Evaluates the registry actions that should be produced for a policy given element values and target state.
    /// </summary>
    /// <param name="input">Evaluation input bundle.</param>
    /// <returns>List of registry actions (possibly empty).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> or contained members are null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if a required element value is missing for an enabled policy.</exception>
    public static IReadOnlyList<RegistryAction> Evaluate(PolicyEvaluationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Policy);
        ArgumentNullException.ThrowIfNull(input.ElementValues);

        var actions = new List<RegistryAction>();

        actions.AddRange(input.State switch
        {
            PolicyState.Enabled => input.Policy.StateBehavior.OnEnable,
            PolicyState.Disabled => input.Policy.StateBehavior.OnDisable,
            PolicyState.NotConfigured => input.Policy.StateBehavior.OnNotConfigured,
            _ => []
        });

        if (input.State == PolicyState.Enabled)
        {
            foreach (PolicyElement el in input.Policy.Elements)
            {
                switch (el)
                {
                    case BooleanElement b:
                        {
                            var v = Get<bool>(input.ElementValues, el.Id);
                            actions.AddRange(v ? b.WhenTrue : b.WhenFalse);
                            break;
                        }
                    case DecimalElement d:
                        {
                            var v = Get<long>(input.ElementValues, el.Id);
                            foreach (var t in d.Writes) actions.Add(Realize(t, v));
                            break;
                        }
                    case TextElement t:
                        {
                            var v = Get<string>(input.ElementValues, el.Id);
                            foreach (var w in t.Writes) actions.Add(Realize(w, v));
                            break;
                        }
                    case MultiTextElement mt:
                        {
                            var v = Get<IReadOnlyList<string>>(input.ElementValues, el.Id);
                            foreach (var w in mt.Writes) actions.Add(Realize(w, v));
                            break;
                        }
                    case EnumElement ee:
                        {
                            var v = Get<string>(input.ElementValues, el.Id);
                            var item = ee.Items.FirstOrDefault(i => i.Name == v);
                            if (item is not null) actions.AddRange(item.Writes);
                            break;
                        }
                }
            }
        }

        return actions;
    }

    private static T Get<T>(IReadOnlyDictionary<ElementId, object?> values, ElementId id)
        => values.TryGetValue(id, out var v) && v is T t
            ? t
            : throw new KeyNotFoundException($"Missing or invalid value for element {id.Value}");

    private static RegistryAction Realize<T>(RegistryActionTemplate<T> t, T value)
    {
        object? resolved = t.Expression switch
        {
            LiteralExpression<T> lit => lit.Value,
            FormatExpression<T> fmt => fmt.Project(value),
            _ => value
        };
        return new RegistryAction(t.Path, t.ValueName, t.ValueType, resolved, t.Operation);
    }
}