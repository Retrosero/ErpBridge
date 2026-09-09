using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Sql;
using FluentAssertions;

namespace ErpBridge.Erp.Sql.Tests;

/// <summary>
/// Key projection is what lets one shadow schema serve V15 int RECno tables,
/// V16 Guid tables, and (later) Logo's LOGICALREF. The tag prefix must survive
/// a round trip or the consumer cannot resolve which row an event refers to.
/// </summary>
public class KeyProjectionTests
{
    private static ErpTrackedTable Table(ErpRowKeyKind kind) => new(
        TableId: 13,
        TableKey: "STOKLAR",
        TableName: "STOKLAR",
        SchemaName: "dbo",
        PrimaryKeyField: "sto_RECno",
        Fields: new[] { "sto_RECno", "sto_kod" },
        KeyKind: kind);

    [Fact]
    public void Recno_projection_tags_int_keys()
    {
        TaggedKeyProjection.Recno.Project(Table(ErpRowKeyKind.Int), 12345)
            .Should().Be("recno:12345");
    }

    [Fact]
    public void Guid_projection_uses_canonical_D_format()
    {
        var g = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        TaggedKeyProjection.Guid.Project(Table(ErpRowKeyKind.Guid), g)
            .Should().Be("guid:6f9619ff-8b86-d011-b42d-00c04fc964ff");
    }

    [Fact]
    public void Logo_projection_is_available_for_the_next_adapter()
    {
        TaggedKeyProjection.LogicalRef.Project(Table(ErpRowKeyKind.Int), 987L)
            .Should().Be("logicalref:987");
    }

    [Fact]
    public void Null_key_projects_to_empty_rather_than_a_bare_tag()
    {
        TaggedKeyProjection.Recno.Project(Table(ErpRowKeyKind.Int), null)
            .Should().BeEmpty();
        TaggedKeyProjection.Recno.Project(Table(ErpRowKeyKind.Int), DBNull.Value)
            .Should().BeEmpty();
    }

    [Fact]
    public void String_keys_are_trimmed()
    {
        TaggedKeyProjection.Recno.Project(Table(ErpRowKeyKind.Int), "  42  ")
            .Should().Be("recno:42");
    }

    [Theory]
    [InlineData("recno:12345", "recno", "12345")]
    [InlineData("guid:6f9619ff-8b86-d011-b42d-00c04fc964ff", "guid", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    public void Split_recovers_tag_and_value(string tagged, string expectedTag, string expectedValue)
    {
        var (tag, value) = TaggedKeyProjection.Split(tagged);

        tag.Should().Be(expectedTag);
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void Split_tolerates_an_untagged_legacy_value()
    {
        var (tag, value) = TaggedKeyProjection.Split("12345");

        tag.Should().BeNull();
        value.Should().Be("12345");
    }

    [Fact]
    public void KeyKindProjection_picks_guid_or_recno_per_table()
    {
        var selector = KeyKindProjection.RecnoOrGuid;

        selector.For(Table(ErpRowKeyKind.Int)).Tag.Should().Be("recno");
        selector.For(Table(ErpRowKeyKind.Guid)).Tag.Should().Be("guid");
    }

    [Fact]
    public void KeyKindProjection_projects_through_the_selected_projection()
    {
        var selector = KeyKindProjection.RecnoOrGuid;
        var g = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF");

        selector.Project(Table(ErpRowKeyKind.Int), 7).Should().StartWith("recno:");
        selector.Project(Table(ErpRowKeyKind.Guid), g).Should().StartWith("guid:");
    }
}
