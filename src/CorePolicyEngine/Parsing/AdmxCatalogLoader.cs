// Project Name: CorePolicyEngine
// File Name: AdmxCatalogLoader.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


namespace KC.ITCompanion.CorePolicyEngine.Parsing;


// Backwards compatibility shim (scheduled for removal). Now returns raw pair without Result wrapper.
[Obsolete("Use IAdminTemplateLoader directly. This shim will be removed.")]
public sealed class AdmxCatalogLoader
{
    private readonly IAdminTemplateLoader _inner = new AdmxAdmlParser();





    public Task<AdminTemplatePair?> LoadSingleAsync(string admxPath, string languageTag, CancellationToken ct)
    {
        return LoadInternalAsync(admxPath, languageTag, ct);
    }





    private async Task<AdminTemplatePair?> LoadInternalAsync(string path, string lang, CancellationToken ct)
    {
        Result<AdminTemplatePair> r = await this._inner.LoadAsync(path, lang, ct);
        return r.Success ? r.Value : null;
    }
}