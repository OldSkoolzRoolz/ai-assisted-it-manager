// Project Name: CorePolicyEngine
// File Name: AdmxCatalogLoader.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Parsing;

/// <summary>
/// High level convenience loader wrapping <see cref="AdmxAdmlParser"/> for caller friendly API.
/// </summary>
public sealed class AdmxCatalogLoader
{
    private readonly AdmxAdmlParser _inner = new();

    /// <summary>Loads a single pair returning null if errors.</summary>
    public async Task<AdminTemplatePair?> LoadSingleAsync(string admxPath, string languageTag, CancellationToken ct)
    {
        Result<AdminTemplatePair> r = await _inner.LoadAsync(admxPath, languageTag, ct).ConfigureAwait(false);
        return r.Success && r is Result<AdminTemplatePair> pr ? pr.Value : null;
    }
}