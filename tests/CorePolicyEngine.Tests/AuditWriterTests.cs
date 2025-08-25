using System;
using System.Text.Json;
using System.Threading.Tasks;
using KC.ITCompanion.CorePolicyEngine.Storage;
using Xunit;

namespace CorePolicyEngine.Tests;

/// <summary>Tests for AuditWriter public members.</summary>
public sealed class AuditWriterTests
{
    private sealed class InMemoryAuditStore : IAuditStore
    {
        public AuditRecord? Last;
        public Task InitializeAsync(System.Threading.CancellationToken token) => Task.CompletedTask;
        public Task WriteAsync(AuditRecord record, System.Threading.CancellationToken token)
        { Last = record; return Task.CompletedTask; }
    }

    [Fact]
    public async Task PolicyViewed_WritesRecord()
    {
        var store = new InMemoryAuditStore();
        var writer = new AuditWriter(store);
        await writer.PolicyViewedAsync("PolicyA");
        Assert.NotNull(store.Last);
        Assert.Equal("PolicyViewed", store.Last!.EventType);
        Assert.Equal("PolicyA", store.Last.PolicyKey);
    }

    [Fact]
    public async Task PolicyEdited_EmbedsDetails()
    {
        var store = new InMemoryAuditStore();
        var writer = new AuditWriter(store);
        await writer.PolicyEditedAsync("PolicyB", "Element1", "42");
        Assert.NotNull(store.Last);
        Assert.Equal("PolicyEdited", store.Last!.EventType);
        Assert.Contains("Element1", store.Last.DetailsJson);
        Assert.Contains("42", store.Last.DetailsJson);
        using var doc = JsonDocument.Parse(store.Last.DetailsJson!);
        Assert.Equal("Element1", doc.RootElement.GetProperty("elementId").GetString());
    }
}
