using System.Globalization;
using Microsoft.Data.Sqlite;
using SyncAdapter.Core.Abstractions;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Infrastructure.Sqlite;

/// <summary>
/// OfflineBilgi tablosu için SQLite implementasyonu.
/// FORA Win'in OfflineBilgi tablosu (GuncellemeServisi.cs:261) birebir.
/// </summary>
public sealed class SqliteOfflineBilgiRepository : IOfflineBilgiRepository
{
    private readonly Func<SqliteConnection> _writable;
    private readonly Func<SqliteConnection> _readOnly;

    public SqliteOfflineBilgiRepository(Func<SqliteConnection> writable, Func<SqliteConnection> readOnly)
    {
        _writable = writable;
        _readOnly = readOnly;
    }

    public async Task<OfflineBilgi> GetOrCreateAsync(int tabloId, CancellationToken ct = default)
    {
        // FORA pattern: önce SELECT, yoksa INSERT OR REPLACE
        using (var conn = _writable())
        {
            using (var select = conn.CreateCommand())
            {
                select.CommandText = @"
                    SELECT SonGuncellemeZamani, UpdateLastTriggerRecNo, DeleteLastTriggerRecNo
                    FROM [OfflineBilgi] WHERE TabloID = @id LIMIT 1;";
                select.Parameters.AddWithValue("@id", tabloId);

                using var reader = await select.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    return new OfflineBilgi
                    {
                        TabloID = tabloId,
                        SonGuncellemeZamani = ParseDate(reader.GetString(0)),
                        UpdateLastTriggerRecNo = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        DeleteLastTriggerRecNo = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                    };
                }
            }

            // Yoksa oluştur — FORA burada UpdateLastTriggerRecNo = 0 ile başlatır,
            // sync motoru sonradan sunucudan ilk değeri çeker.
            using var insert = conn.CreateCommand();
            insert.CommandText = @"
                INSERT OR REPLACE INTO [OfflineBilgi]
                    (TabloID, SonGuncellemeZamani, UpdateLastTriggerRecNo, DeleteLastTriggerRecNo)
                VALUES
                    (@id, @tarih, 0, 0);";
            insert.Parameters.AddWithValue("@id", tabloId);
            insert.Parameters.AddWithValue("@tarih", DateTime.UtcNow.AddYears(-10).ToString("o", CultureInfo.InvariantCulture));
            await insert.ExecuteNonQueryAsync(ct);

            return new OfflineBilgi { TabloID = tabloId };
        }
    }

    public async Task<bool> UpdateImleciAsync(
        int tabloId, int? updateLastTriggerRecNo = null,
        int? deleteLastTriggerRecNo = null, CancellationToken ct = default)
    {
        // FORA: ilgili kolonu Update et, sadece null olmayanlar.
        using var conn = _writable();
        using var cmd = conn.CreateCommand();
        var sets = new List<string> { "SonGuncellemeZamani = @tarih" };
        cmd.Parameters.AddWithValue("@tarih", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

        if (updateLastTriggerRecNo.HasValue)
        {
            sets.Add("UpdateLastTriggerRecNo = @upd");
            cmd.Parameters.AddWithValue("@upd", updateLastTriggerRecNo.Value);
        }
        if (deleteLastTriggerRecNo.HasValue)
        {
            sets.Add("DeleteLastTriggerRecNo = @del");
            cmd.Parameters.AddWithValue("@del", deleteLastTriggerRecNo.Value);
        }
        cmd.CommandText = $"UPDATE [OfflineBilgi] SET {string.Join(", ", sets)} WHERE TabloID = @id;";
        cmd.Parameters.AddWithValue("@id", tabloId);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<IReadOnlyList<OfflineBilgi>> TumunuAlAsync(CancellationToken ct = default)
    {
        using var conn = _readOnly();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT TabloID, SonGuncellemeZamani, UpdateLastTriggerRecNo, DeleteLastTriggerRecNo
            FROM [OfflineBilgi];";

        var result = new List<OfflineBilgi>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new OfflineBilgi
            {
                TabloID = reader.GetInt32(0),
                SonGuncellemeZamani = reader.IsDBNull(1) ? DateTime.MinValue : ParseDate(reader.GetString(1)),
                UpdateLastTriggerRecNo = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                DeleteLastTriggerRecNo = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
            });
        }
        return result;
    }

    private static DateTime ParseDate(string iso) =>
        DateTime.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
