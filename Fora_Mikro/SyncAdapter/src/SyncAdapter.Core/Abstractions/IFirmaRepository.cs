using System.Threading;
using System.Threading.Tasks;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Core.Abstractions;

/// <summary>
/// Firmalar tablosu için depo sözleşmesi.
/// Windows Service çoklu tenant çalıştığı için tüm firmalar tek bir SQLite'ta tutulur.
/// </summary>
public interface IFirmaRepository
{
    Task<Firma?> FirmaGetirAsync(string firmaId, CancellationToken ct = default);
    Task<bool> FirmaKaydetAsync(Firma firma, CancellationToken ct = default);
}
