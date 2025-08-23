// Project Name: CorePolicyEngine
// File Name: RegOps.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.AdminTemplates;

public sealed record PolicyEvaluationInput(
    AdminPolicy Policy,
    PolicyState State,
    IReadOnlyDictionary<ElementId, object?> ElementValues
);

public enum PolicyState
{
    Enabled,
    Disabled,
    NotConfigured
}

public static class Evaluator
{
    public static IReadOnlyList<RegistryAction> Evaluate(PolicyEvaluationInput input)
    {
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
                if (el is BooleanElement b)
                {
                    var v = Get<bool>(input.ElementValues, el.Id);
                    actions.AddRange(v ? b.WhenTrue : b.WhenFalse);
                }
                else if (el is DecimalElement d)
                {
                    var v = Get<long>(input.ElementValues, el.Id);
                    foreach (var t in d.Writes) actions.Add(Realize(t, v));
                }
                else if (el is TextElement t)
                {
                    var v = Get<string>(input.ElementValues, el.Id);
                    foreach (var w in t.Writes) actions.Add(Realize(w, v));
                }
                else if (el is MultiTextElement mt)
                {
                    var v = Get<IReadOnlyList<string>>(input.ElementValues, el.Id);
                    foreach (var w in mt.Writes) actions.Add(Realize(w, v));
                }
                else if (el is EnumElement ee)
                {
                    var v = Get<string>(input.ElementValues, el.Id);
                    var item = ee.Items.FirstOrDefault(i => i.Name == v);
                    if (item is not null) actions.AddRange(item.Writes);
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