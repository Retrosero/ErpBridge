using Microsoft.Data.Sqlite;

namespace SyncAdapter.Infrastructure.Sqlite;

/// <summary>
/// FORA Win'in üç ayrı SQLite DB yapısının sadeleştirilmiş muadili:
///   1. Firmalar.db          → Firmalar tablosu
///   2. OfflineOutbox.db     → OFFLINE_KAYITLAR (outbox) + OfflineBilgi (delta imleçleri)
/// Tüm tablolar tek bir DB'de — operasyonel sadelik için.
/// Şema FORA'daki ile birebir aynı kolon isimleri (CREATE TABLE IF NOT EXISTS + fallback DROP).
/// </summary>
public static class SqliteSchemaInitializer
{
    public const string CurrentSchemaVersion = "1";

    /// <summary>DB dosyasını açar, tabloları oluşturur, şema versiyonunu yazar.</summary>
    public static void EnsureCreated(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // FORA pattern'i: aynı DB için iki connection (read-only + writable).
        // SQLite single-writer sınırını aşmak için.
        using var writable = OpenConnection(dbPath, readOnly: false);
        using var cmd = writable.CreateCommand();
        cmd.CommandText = SchemaScript;
        cmd.ExecuteNonQuery();
    }

    public static SqliteConnection OpenConnection(string dbPath, bool readOnly)
    {
        var csb = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        };
        var conn = new SqliteConnection(csb.ConnectionString);
        conn.Open();
        // FORA pattern: PRAGMA'lar connection başına ayarlanır
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
            pragma.ExecuteNonQuery();
        }
        return conn;
    }

    private const string SchemaScript = @"
        CREATE TABLE IF NOT EXISTS [SchemaVersion] (
            [Key]   TEXT PRIMARY KEY,
            [Value] TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS [Firmalar] (
            [FirmaID]            TEXT NOT NULL PRIMARY KEY,
            [FirmaAdi]           TEXT NOT NULL,
            [MikroDBName]        TEXT NOT NULL,
            [MikroServer]        TEXT NOT NULL,
            [SqlUserName]        TEXT NULL,
            [SqlPassword]        TEXT NULL,
            [RemoteServiceIP]    TEXT NULL,
            [LisansBitisTarihi]  TEXT NOT NULL
        );

        -- FORA Win'in OFFLINE_KAYITLAR tablosu (OfflineEvrakSqlite.cs:139-153) birebir
        CREATE TABLE IF NOT EXISTS [OFFLINE_KAYITLAR] (
            [OfflineRECno]      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            [AktarimDurumu]     INTEGER NOT NULL,
            [YeniKayit]         INTEGER NOT NULL,
            [AktarilmaTarihi]   TEXT    NOT NULL,
            [Tipi]              INTEGER NOT NULL,
            [Evrak]             BLOB    NOT NULL,
            [HataString]        TEXT    NULL
        );

        CREATE INDEX IF NOT EXISTS [IDX_OFFLINE_KAYITLAR_Durum_Tarih]
            ON [OFFLINE_KAYITLAR] ([AktarimDurumu], [AktarilmaTarihi]);

        -- FORA Win'in OfflineBilgi tablosu (GuncellemeServisi.cs:261) birebir
        CREATE TABLE IF NOT EXISTS [OfflineBilgi] (
            [TabloID]                INTEGER NOT NULL PRIMARY KEY,
            [SonGuncellemeZamani]    TEXT    NULL,
            [UpdateLastTriggerRecNo] INTEGER NULL,
            [DeleteLastTriggerRecNo] INTEGER NULL
        );

        INSERT OR IGNORE INTO [SchemaVersion] ([Key], [Value])
        VALUES ('version', '" + CurrentSchemaVersion + @"');
    ";
}
