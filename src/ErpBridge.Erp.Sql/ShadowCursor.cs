using System.Text.Json;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Sql;

/// <summary>
/// Decoded form of the opaque <see cref="ErpSyncCursor"/> used by
/// <see cref="SqlServerShadowTableChangeLog"/>: one <c>TriggerRECno</c> high-water
/// mark per tracked table, per direction.
///
/// <para>
/// The wire form is compact JSON — <c>{"u":{"STOKLAR":1204},"d":{"STOKLAR":17}}</c>
/// — so a 49-table catalog costs well under a kilobyte. An unparseable or empty
/// token decodes to "start from zero" rather than throwing: a corrupted cursor
/// should cost a re-sync, never a crash loop.
/// </para>
/// </summary>
public sealed class ShadowCursor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Dictionary<string, int> _upserts;
    private readonly Dictionary<string, int> _deletes;

    /// <summary>Build an empty cursor (everything at zero).</summary>
    public ShadowCursor()
        : this(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
               new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase))
    {
    }

    private ShadowCursor(Dictionary<string, int> upserts, Dictionary<string, int> deletes)
    {
        _upserts = upserts;
        _deletes = deletes;
    }

    /// <summary>Last INSERT/UPDATE <c>TriggerRECno</c> consumed for <paramref name="tableKey"/>.</summary>
    public int Upsert(string tableKey) => _upserts.GetValueOrDefault(tableKey, 0);

    /// <summary>Last DELETE <c>TriggerRECno</c> consumed for <paramref name="tableKey"/>.</summary>
    public int Delete(string tableKey) => _deletes.GetValueOrDefault(tableKey, 0);

    /// <summary>
    /// Advance the INSERT/UPDATE mark. The cursor is monotonic — a lower value is
    /// ignored so a buggy retry cannot rewind and re-send an already-acked batch.
    /// </summary>
    public void AdvanceUpsert(string tableKey, int triggerRecNo)
    {
        if (triggerRecNo > Upsert(tableKey))
        {
            _upserts[tableKey] = triggerRecNo;
        }
    }

    /// <summary>Advance the DELETE mark (monotonic, same rule as <see cref="AdvanceUpsert"/>).</summary>
    public void AdvanceDelete(string tableKey, int triggerRecNo)
    {
        if (triggerRecNo > Delete(tableKey))
        {
            _deletes[tableKey] = triggerRecNo;
        }
    }

    /// <summary>Encode to the opaque token persisted by <see cref="IErpSyncCursorStore"/>.</summary>
    public ErpSyncCursor ToCursor()
    {
        if (_upserts.Count == 0 && _deletes.Count == 0)
        {
            return ErpSyncCursor.Start;
        }

        var dto = new CursorDto { U = _upserts, D = _deletes };
        return new ErpSyncCursor(JsonSerializer.Serialize(dto, JsonOptions));
    }

    /// <summary>
    /// Decode an opaque token. An empty, malformed or unrecognised token yields a
    /// fresh zeroed cursor — the caller then performs a full re-read.
    /// </summary>
    public static ShadowCursor Parse(ErpSyncCursor? cursor)
    {
        if (cursor is null || cursor.IsStart)
        {
            return new ShadowCursor();
        }

        try
        {
            var dto = JsonSerializer.Deserialize<CursorDto>(cursor.Value, JsonOptions);
            if (dto is null)
            {
                return new ShadowCursor();
            }

            return new ShadowCursor(
                new Dictionary<string, int>(dto.U ?? new Dictionary<string, int>(), StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, int>(dto.D ?? new Dictionary<string, int>(), StringComparer.OrdinalIgnoreCase));
        }
        catch (JsonException)
        {
            return new ShadowCursor();
        }
    }

    private sealed class CursorDto
    {
        public Dictionary<string, int>? U { get; set; }
        public Dictionary<string, int>? D { get; set; }
    }
}
