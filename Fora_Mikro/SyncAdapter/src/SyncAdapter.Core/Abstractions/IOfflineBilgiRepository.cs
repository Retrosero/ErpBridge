using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Core.Abstractions;

/// <summary>
/// OfflineBilgi tablosu (delta imleçleri) için depo sözleşmesi.
/// FORA Win'in OfflineBilgi tablosu (GuncellemeServisi.cs:261) muadili.
/// </summary>
public interface IOfflineBilgiRepository
{
    /// <summary>Bir tablonun delta imlecini getirir. Yoksa yeni bir tane oluşturur.</summary>
    Task<OfflineBilgi> GetOrCreateAsync(int tabloId, CancellationToken ct = default);

    /// <summary>Senkronizasyon tamamlandıktan sonra imleci ilerletir.</summary>
    Task<bool> UpdateImleciAsync(
        int tabloId,
        int? updateLastTriggerRecNo = null,
        int? deleteLastTriggerRecNo = null,
        CancellationToken ct = default);

    /// <summary>Tüm imleçleri getir (toplu pull başlangıcında).</summary>
    Task<IReadOnlyList<OfflineBilgi>> TumunuAlAsync(CancellationToken ct = default);
}
