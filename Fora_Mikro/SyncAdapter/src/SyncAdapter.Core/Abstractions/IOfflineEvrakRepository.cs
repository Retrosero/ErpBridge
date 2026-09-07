using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyncAdapter.Core.Enums;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Core.Abstractions;

/// <summary>
/// OFFLINE_KAYITLAR tablosu için depo sözleşmesi.
/// FORA Win'in OfflineEvrakSqlite.cs (OfflineEvrakSqlite.cs:11-316) muadili.
/// </summary>
public interface IOfflineEvrakRepository
{
    /// <summary>Yerel bir evrak outbox'a eklenir. Durum Beklemede olarak başlar.</summary>
    Task<long> EkleAsync(OfflineEvrakV2 evrak, CancellationToken ct = default);

    /// <summary>Senkronizasyon motoru için: duruma göre outbox'tan al.</summary>
    Task<IReadOnlyList<OfflineEvrakV2>> BekleyenleriAlAsync(
        EvrakAktarimDurumu durum,
        int maksimumKayit,
        CancellationToken ct = default);

    /// <summary>Sunucudan gelen güncel hâli (RecId'ler, durum) ile outbox satırını güncelle.</summary>
    Task<bool> GuncelleAsync(OfflineEvrakV2 evrak, CancellationToken ct = default);

    /// <summary>Belirli RecNo'yu sil. Genelde kullanılmaz, soft-delete tercih edilir.</summary>
    Task<bool> SilAsync(long offlineRecNo, CancellationToken ct = default);
}
