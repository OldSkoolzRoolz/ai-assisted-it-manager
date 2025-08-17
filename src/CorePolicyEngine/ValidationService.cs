using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared;


namespace CorePolicyEngine;


public sealed class ValidationService : IValidationService
{
    private readonly IEnumerable<IValidationRule> _rules;

    public ValidationService(IEnumerable<IValidationRule> rules)
    {
        this._rules = rules;
    }

    public Task<ValidationResult> ValidateAsync(PolicySet policySet, AdmxCatalog catalog, CancellationToken cancellationToken)
    {
        var ctx = new ValidationContext("Windows-Default");
        var messages = this._rules.SelectMany(r => r.Evaluate(policySet, catalog, ctx)).ToList();
        return Task.FromResult(ValidationResult.FromMessages(messages));
    }
}