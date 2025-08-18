

using System.Collections.Generic;



using System;
using System.Collections.Generic;
using System.Linq;
namespace CorePolicyEngine.AdminTemplates;

public sealed record PolicyEvaluationInput(
    Policy Policy,
    PolicyState State,                    // Enabled/Disabled/NotConfigured
    IReadOnlyDictionary<ElementId, object?> ElementValues // user-specified values for typed elements
);

public enum PolicyState { Enabled, Disabled, NotConfigured }

public static class Evaluator
{
    public static IReadOnlyList<RegistryAction> Evaluate(PolicyEvaluationInput input)
    {
        var actions = new List<RegistryAction>();

        // State-level actions
        actions.AddRange(input.State switch
        {
            PolicyState.Enabled => input.Policy.StateBehavior.OnEnable,
            PolicyState.Disabled => input.Policy.StateBehavior.OnDisable,
            PolicyState.NotConfigured => input.Policy.StateBehavior.OnNotConfigured,
            _ => []
        });

        // Element-level actions (only when Enabled)
        if (input.State == PolicyState.Enabled)
        {
            foreach (var el in input.Policy.Elements)
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
                    var v = Get<string>(input.ElementValues, el.Id); // item name
                    var item = ee.Items.Find(i => i.Name == v);
                    if (item is not null) actions.AddRange(item.Writes);
                }
            }
        }

        return actions;
    }

    private static T Get<T>(IReadOnlyDictionary<ElementId, object?> values, ElementId id)
        => values.TryGetValue(id, out var v) && v is T t ? t :
           throw new KeyNotFoundException($"Missing or invalid value for element {id.Value}");

    private static RegistryAction Realize<T>(RegistryActionTemplate<T> t, T value)
        => t.Expression switch
        {
            LiteralExpression<T> lit => t with { Value = lit.Value },
            FormatExpression<T> fmt => t with { Value = fmt.Project(value) },
            _ => t with { Value = value }
        };
}