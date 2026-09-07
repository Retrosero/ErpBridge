using System.Globalization;
using Microsoft.Data.Sqlite;
using SyncAdapter.Core.Abstractions;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Infrastructure.Sqlite;

/// <summary>
/// Firmalar tablosu için SQLite implementasyonu.
/// FORA Win'in Firmalar tablosu (sqlbaglantibilgileri.xml'den okunan karşılığı) muadili.
/// </summary>
public sealed class SqliteFirmaRepository : IFirmaRepository
{
    private readonly Func<SqliteConnection> _writable;
    private readonly Func<SqliteConnection> _readOnly;

    public SqliteFirmaRepository(Func<SqliteConnection> writable, Func<SqliteConnection> readOnly)
    {
        _writable = writable;
        _readOnly = readOnly;
    }

    public async Task<Firma?> FirmaGetirAsync(string firmaId, CancellationToken ct = default)
    {
        using var conn = _readOnly();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT FirmaAdi, MikroDBName, MikroServer, SqlUserName, SqlPassword,
                   RemoteServiceIP, LisansBitisTarihi
            FROM [Firmalar] WHERE FirmaID = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", firmaId);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new Firma
        {
            FirmaID = firmaId,
            FirmaAdi = reader.GetString(0),
            MikroDBName = reader.GetString(1),
            MikroServer = reader.GetString(2),
            SqlUserName = reader.IsDBNull(3) ? null : reader.GetString(3),
            SqlPassword = reader.IsDBNull(4) ? null : reader.GetString(4),
            RemoteServiceIP = reader.IsDBNull(5) ? null : reader.GetString(5),
            LisansBitisTarihi = DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
        };
    }

    public async Task<bool> FirmaKaydetAsync(Firma firma, CancellationToken ct = default)
    {
        using var conn = _writable();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO [Firmalar]
                (FirmaID, FirmaAdi, MikroDBName, MikroServer, SqlUserName, SqlPassword,
                 RemoteServiceIP, LisansBitisTarihi)
            VALUES
                (@id, @ad, @db, @srv, @usr, @pwd, @remote, @lisans)
            ON CONFLICT(FirmaID) DO UPDATE SET
                FirmaAdi = excluded.FirmaAdi,
                MikroDBName = excluded.MikroDBName,
                MikroServer = excluded.MikroServer,
                SqlUserName = excluded.SqlUserName,
                SqlPassword = excluded.SqlPassword,
                RemoteServiceIP = excluded.RemoteServiceIP,
                LisansBitisTarihi = excluded.LisansBitisTarihi;";
        cmd.Parameters.AddWithValue("@id", firma.FirmaID);
        cmd.Parameters.AddWithValue("@ad", firma.FirmaAdi);
        cmd.Parameters.AddWithValue("@db", firma.MikroDBName);
        cmd.Parameters.AddWithValue("@srv", firma.MikroServer);
        cmd.Parameters.AddWithValue("@usr", (object?)firma.SqlUserName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pwd", (object?)firma.SqlPassword ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@remote", (object?)firma.RemoteServiceIP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lisans", firma.LisansBitisTarihi.ToString("o", CultureInfo.InvariantCulture));

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
