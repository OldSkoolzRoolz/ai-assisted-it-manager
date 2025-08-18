// Project Name: CorePolicyEngine
// File Name: AdmxCatalogLoader.cs (legacy name — now acts as AdminTemplateLoader)
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Shared;

namespace CorePolicyEngine.Parsing;

// Backwards compatibility shim (UI will be updated later). For now exposes same shape to avoid compile errors.
// Will be removed when UI migrates to new AdminTemplates models directly.
public sealed class AdmxCatalogLoader
{
    private readonly IAdminTemplateLoader _inner = new AdmxAdmlParser();

    public async Task<Result<AdminTemplatePair>> LoadSingleAsync(string admxPath, string languageTag, CancellationToken ct)
        => await _inner.LoadAsync(admxPath, languageTag, ct);
}