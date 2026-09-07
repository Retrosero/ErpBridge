using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Trigger;

public class TriggerChangeSetReaderInputValidationTests
{
    private static TriggerChangeSetReader NewReader() =>
        new(new MikroConnectionFactory(), NullLogger<TriggerChangeSetReader>.Instance);

    [Fact]
    public void ReadNewAsync_rejects_negative_lastRecNo()
    {
        var reader = NewReader();
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var act = () => reader.ReadNewAsync(schema, lastRecNo: -1, packetSize: 100, schema.Fields);
        act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("lastRecNo");
    }

    [Fact]
    public void ReadNewAsync_rejects_non_positive_packetSize()
    {
        var reader = NewReader();
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var act = () => reader.ReadNewAsync(schema, lastRecNo: 0, packetSize: 0, schema.Fields);
        act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("packetSize");
    }

    [Fact]
    public void ReadChangedAsync_rejects_non_whitelisted_field()
    {
        var reader = NewReader();
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var act = () => reader.ReadChangedAsync(
            schema,
            lastTriggerRecNo: 0,
            packetSize: 100,
            fields: new[] { "sto_kod", "drop_table" });
        act.Should().ThrowAsync<ArgumentException>().WithMessage("*not part of the*");
    }

    [Fact]
    public void ReadDeletedAsync_rejects_negative_cursor()
    {
        var reader = NewReader();
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var act = () => reader.ReadDeletedAsync(schema, lastTriggerRecNo: -1, packetSize: 100);
        act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("lastTriggerRecNo");
    }
}
