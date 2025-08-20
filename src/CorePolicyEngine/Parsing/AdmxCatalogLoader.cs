// Project Name: CorePolicyEngine
// File Name: AdmxCatalogLoader.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.CorePolicyEngine.Parsing;

namespace KC.ITCompanion.CorePolicyEngine.Parsing;

// Backwards compatibility shim (scheduled for removal). Now returns raw pair without Result wrapper.
[Obsolete("Use IAdminTemplateLoader directly. This shim will be removed.")]
public sealed class AdmxCatalogLoader
{
    private readonly IAdminTemplateLoader _inner = new AdmxAdmlParser();

    public Task<AdminTemplatePair?> LoadSingleAsync(string admxPath, string languageTag, CancellationToken ct)
        => LoadInternalAsync(admxPath, languageTag, ct);

    private async Task<AdminTemplatePair?> LoadInternalAsync(string path, string lang, CancellationToken ct)
    {
        var r = await _inner.LoadAsync(path, lang, ct);
        return r.Success ? r.Value : null;
    }
}