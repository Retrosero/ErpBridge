using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Expiry;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Expiry;

/// <summary>
/// SKT kayıtları on the in-memory provider (no transactions, no counter row): validation of one operation,
/// the batch rules that do not need a database, and the shape the phone reads. The relational behaviour —
/// savepoints, the tenant counter, isolation over HTTP — is in <c>ExpiryRelationalTests</c>.
/// </summary>
public sealed class StockExpiryServiceTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private readonly StockExpiryService _service = new();
    private readonly Tenant _tenant = new() { Id = Guid.NewGuid() };
    private readonly MobileUser _user;

    public StockExpiryServiceTests() => _user = new MobileUser { Id = Guid.NewGuid(), TenantId = _tenant.Id, FullName = "Ali Saha" };

    [Fact]
    public async Task An_upsert_stores_trimmed_fields_and_the_creator_and_the_list_returns_them()
    {
        using var db = NewDb();
        var id = Guid.NewGuid();
        var response = await ApplyAsync(db, Upsert(Valid(id) with
        {
            StockCode = "  STK-1 ", Barcode = " 869000 ", ProductName = " Süt 1L ", Location = " A-3 / Raf 2 ",
            Warehouse = "  ", Quantity = 12.500m, Note = " ön sıra ", Closed = null,
        }));

        response.Results.Single().Status.Should().Be("applied");
        var record = response.Records.Single();
        record.Id.Should().Be(id);
        record.StockCode.Should().Be("STK-1");
        record.Barcode.Should().Be("869000");
        record.ProductName.Should().Be("Süt 1L");
        record.Location.Should().Be("A-3 / Raf 2");
        record.Warehouse.Should().BeNull("a blank optional field is stored empty");
        record.ExpiryDate.Should().Be("2027-03-15");
        record.Note.Should().Be("ön sıra");
        record.Closed.Should().BeFalse();
        record.Deleted.Should().BeFalse();
        record.CreatedBy.Should().Be("Ali Saha");
        record.UpdatedSeq.Should().BePositive();
        JsonSerializer.Serialize(record, Web).Should().Contain("\"quantity\":12.5,", "the phone gets the plain number, not numeric(18,3)'s 12.500");

        var list = await _service.ListAsync(db, _tenant.Id, 0, null, CancellationToken.None);
        list.Records.Should().ContainSingle(r => r.Id == id);
        list.LatestSeq.Should().Be(record.UpdatedSeq);
        list.HasMore.Should().BeFalse();
    }

    [Theory]
    [InlineData("stockCode", "   ", "Ürün kodu boş olamaz.")]
    [InlineData("stockCode", "123456789012345678901234567890123456789012345678901", "Ürün kodu en çok 50 karakter olabilir.")]
    [InlineData("barcode", "123456789012345678901234567890123456789012345678901", "Barkod en çok 50 karakter olabilir.")]
    [InlineData("location", "", "Reyon/raf boş olamaz.")]
    [InlineData("location", null, "Reyon/raf boş olamaz.")]
    [InlineData("expiryDate", null, "Son kullanma tarihi okunamadı.")]
    [InlineData("expiryDate", "15.03.2027", "Son kullanma tarihi okunamadı.")]
    [InlineData("expiryDate", "2027-02-30", "Son kullanma tarihi okunamadı.")]
    [InlineData("expiryDate", "1999-12-31", "Son kullanma tarihi 2000–2100 yılları arasında olmalı.")]
    [InlineData("expiryDate", "2101-01-01", "Son kullanma tarihi 2000–2100 yılları arasında olmalı.")]
    public async Task An_invalid_field_rejects_the_operation_and_names_the_field(string field, string? value, string message)
    {
        using var db = NewDb();
        var input = Valid(Guid.NewGuid());
        input = field switch
        {
            "stockCode" => input with { StockCode = value },
            "barcode" => input with { Barcode = value },
            "location" => input with { Location = value },
            "expiryDate" => input with { ExpiryDate = value },
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };

        var result = (await ApplyAsync(db, Upsert(input))).Results.Single();

        result.Status.Should().Be("rejected");
        result.ErrorCode.Should().Be("EXPIRY_INVALID");
        result.Message.Should().Be(message);
        (await db.StockExpiryRecords.CountAsync()).Should().Be(0);
        (await db.StockExpiryOpsApplied.CountAsync()).Should().Be(0, "a rejected operation is not remembered; the phone drops it");
    }

    [Theory]
    [InlineData("-1", "Miktar 0 ile 999.999.999 arasında olmalı.")]
    [InlineData("1000000000", "Miktar 0 ile 999.999.999 arasında olmalı.")]
    [InlineData("1.2345", "Miktar en çok 3 ondalık basamak içerebilir.")]
    public async Task A_quantity_out_of_range_or_too_precise_is_rejected(string quantity, string message)
    {
        using var db = NewDb();
        var input = Valid(Guid.NewGuid()) with { Quantity = decimal.Parse(quantity, System.Globalization.CultureInfo.InvariantCulture) };

        var result = (await ApplyAsync(db, Upsert(input))).Results.Single();

        result.ErrorCode.Should().Be("EXPIRY_INVALID");
        result.Message.Should().Be(message);
    }

    [Fact]
    public async Task Long_optional_texts_are_rejected_not_cut()
    {
        using var db = NewDb();
        var results = (await ApplyAsync(db,
            Upsert(Valid(Guid.NewGuid()) with { ProductName = new string('x', 201) }),
            Upsert(Valid(Guid.NewGuid()) with { Warehouse = new string('x', 101) }),
            Upsert(Valid(Guid.NewGuid()) with { Note = new string('x', 501) }),
            Upsert(Valid(Guid.NewGuid()) with { Location = new string('x', 51) }),
            Upsert(Valid(Guid.NewGuid()) with { Quantity = 999_999_999m, Note = new string('x', 500), Location = new string('x', 50) }))).Results;

        results.Select(r => r.Message).Should().Equal(
            "Ürün adı en çok 200 karakter olabilir.",
            "Depo en çok 100 karakter olabilir.",
            "Not en çok 500 karakter olabilir.",
            "Reyon/raf en çok 50 karakter olabilir.",
            null);
        results[^1].Status.Should().Be("applied", "the limits themselves are allowed");
    }

    [Fact]
    public async Task Operations_without_an_id_a_record_or_a_known_type_are_rejected()
    {
        using var db = NewDb();
        var results = (await ApplyAsync(db,
            new ExpiryOp { OpId = Guid.Empty, Type = "upsert", Record = Valid(Guid.NewGuid()).ToInput() },
            new ExpiryOp { OpId = Guid.NewGuid(), Type = "archive", Record = Valid(Guid.NewGuid()).ToInput() },
            new ExpiryOp { OpId = Guid.NewGuid(), Type = "upsert", Record = null },
            new ExpiryOp { OpId = Guid.NewGuid(), Type = "upsert", Record = Valid(Guid.Empty).ToInput() },
            new ExpiryOp { OpId = Guid.NewGuid(), Type = "delete", Record = new ExpiryRecordInput() })).Results;

        results.Select(r => r.ErrorCode).Should().Equal(
            "EXPIRY_OP_ID_REQUIRED", "EXPIRY_OP_UNKNOWN", "EXPIRY_INVALID", "EXPIRY_INVALID", "EXPIRY_INVALID");
        results[1].Message.Should().Be("Bilinmeyen işlem: archive.");
        results[2].Message.Should().Be("Kayıt bilgisi eksik.");
        results[3].Message.Should().Be("Kayıt kimliği gerekli.");
    }

    [Fact]
    public async Task A_delete_needs_only_the_id_and_a_second_delete_is_applied_without_a_new_change()
    {
        using var db = NewDb();
        var id = Guid.NewGuid();
        await ApplyAsync(db, Upsert(Valid(id)));

        var first = await ApplyAsync(db, new ExpiryOp { OpId = Guid.NewGuid(), Type = "delete", Record = new ExpiryRecordInput { Id = id } });
        var deleted = first.Records.Single();
        deleted.Deleted.Should().BeTrue();

        var again = await ApplyAsync(db, new ExpiryOp { OpId = Guid.NewGuid(), Type = "delete", Record = new ExpiryRecordInput { Id = id } });
        again.Results.Single().Status.Should().Be("applied");
        again.Records.Single().UpdatedSeq.Should().Be(deleted.UpdatedSeq, "nothing changed");
    }

    // ---- helpers -----------------------------------------------------------------------------

    /// <summary>A valid <see cref="ExpiryRecordInput"/> as a value, so a test changes one field with <c>with</c>.</summary>
    private sealed record Input(
        Guid Id, string? StockCode, string? Barcode, string? ProductName, string? Location, string? Warehouse,
        string? ExpiryDate, decimal? Quantity, string? Note, bool? Closed)
    {
        public ExpiryRecordInput ToInput() => new()
        {
            Id = Id, StockCode = StockCode, Barcode = Barcode, ProductName = ProductName, Location = Location,
            Warehouse = Warehouse, ExpiryDate = ExpiryDate, Quantity = Quantity, Note = Note, Closed = Closed,
        };
    }

    private static Input Valid(Guid id) => new(id, "STK-1", null, "Süt 1L", "A-3", null, "2027-03-15", 4m, null, false);

    private static ExpiryOp Upsert(Input input) => new() { OpId = Guid.NewGuid(), Type = "upsert", Record = input.ToInput() };

    private Task<ExpiryOpsResponse> ApplyAsync(CentralApiDbContext db, params ExpiryOp[] ops) =>
        _service.ApplyAsync(db, _tenant, _user, ops, CancellationToken.None);

    private static CentralApiDbContext NewDb() =>
        new(new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseInMemoryDatabase("StockExpiry_" + Guid.NewGuid().ToString("N"))
            .Options);
}
