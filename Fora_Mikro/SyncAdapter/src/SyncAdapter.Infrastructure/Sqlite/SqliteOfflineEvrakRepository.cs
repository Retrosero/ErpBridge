using System.Globalization;
using Microsoft.Data.Sqlite;
using SyncAdapter.Core.Abstractions;
using SyncAdapter.Core.Enums;
using SyncAdapter.Core.Models;
using SyncAdapter.Infrastructure.Serialization;

namespace SyncAdapter.Infrastructure.Sqlite;

/// <summary>
/// OFFLINE_KAYITLAR tablosu için SQLite implementasyonu.
/// FORA Win'in OfflineEvrakSqlite.cs (OfflineEvrakSqlite.cs:11-316) muadili.
/// </summary>
public sealed class SqliteOfflineEvrakRepository : IOfflineEvrakRepository
{
    private readonly Func<SqliteConnection> _writable;
    private readonly Func<SqliteConnection> _readOnly;

    public SqliteOfflineEvrakRepository(Func<SqliteConnection> writable, Func<SqliteConnection> readOnly)
    {
        _writable = writable;
        _readOnly = readOnly;
    }

    public async Task<long> EkleAsync(OfflineEvrakV2 evrak, CancellationToken ct = default)
    {
        using var conn = _writable();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO [OFFLINE_KAYITLAR]
                (AktarimDurumu, YeniKayit, AktarilmaTarihi, Tipi, Evrak, HataString)
            VALUES
                (@durum, @yeni, @tarih, @tipi, @evrak, @hata);
            SELECT last_insert_rowid();
        ";
        cmd.Parameters.AddWithValue("@durum", (int)evrak.Durum);
        cmd.Parameters.AddWithValue("@yeni", evrak.YeniKayit ? 1 : 0);
        cmd.Parameters.AddWithValue("@tarih", evrak.AktarilmaTarihi.ToString("o", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@tipi", (int)evrak.Tipi);
        cmd.Parameters.AddWithValue("@evrak", OfflineEvrakSerializer.Sıkıştır(evrak));
        cmd.Parameters.AddWithValue("@hata", (object?)evrak.HataString ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(result);
    }

    public async Task<IReadOnlyList<OfflineEvrakV2>> BekleyenleriAlAsync(
        EvrakAktarimDurumu durum, int maksimumKayit, CancellationToken ct = default)
    {
        using var conn = _readOnly();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT OfflineRECno, Evrak, HataString
            FROM [OFFLINE_KAYITLAR]
            WHERE AktarimDurumu = @durum
            ORDER BY OfflineRECno ASC
            LIMIT @limit;
        ";
        cmd.Parameters.AddWithValue("@durum", (int)durum);
        cmd.Parameters.AddWithValue("@limit", maksimumKayit);

        var result = new List<OfflineEvrakV2>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var recNo = reader.GetInt64(0);
            var blob = (byte[])reader.GetValue(1);
            var hata = reader.IsDBNull(2) ? null : reader.GetString(2);

            var env = OfflineEvrakSerializer.Aç(blob);
            env.OfflineRecNo = (int)recNo;
            env.HataString = hata;
            env.Durum = durum;
            result.Add(env);
        }
        return result;
    }

    public async Task<bool> GuncelleAsync(OfflineEvrakV2 evrak, CancellationToken ct = default)
    {
        using var conn = _writable();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE [OFFLINE_KAYITLAR]
            SET AktarimDurumu   = @durum,
                YeniKayit       = @yeni,
                AktarilmaTarihi = @tarih,
                Tipi            = @tipi,
                Evrak           = @evrak,
                HataString      = @hata
            WHERE OfflineRECno = @recNo;
        ";
        cmd.Parameters.AddWithValue("@durum", (int)evrak.Durum);
        cmd.Parameters.AddWithValue("@yeni", evrak.YeniKayit ? 1 : 0);
        cmd.Parameters.AddWithValue("@tarih", evrak.AktarilmaTarihi.ToString("o", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@tipi", (int)evrak.Tipi);
        cmd.Parameters.AddWithValue("@evrak", OfflineEvrakSerializer.Sıkıştır(evrak));
        cmd.Parameters.AddWithValue("@hata", (object?)evrak.HataString ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@recNo", evrak.OfflineRecNo);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> SilAsync(long offlineRecNo, CancellationToken ct = default)
    {
        using var conn = _writable();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM [OFFLINE_KAYITLAR] WHERE OfflineRECno = @recNo;";
        cmd.Parameters.AddWithValue("@recNo", offlineRecNo);
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
