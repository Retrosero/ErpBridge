using Dapper;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>Which phone document is being written, and by which Mikro user.</summary>
/// <param name="DocumentType">Job document type; with <paramref name="ExternalId"/> the idempotency key.</param>
/// <param name="ExternalId">The phone's document id.</param>
/// <param name="ErpUserNo">Mikro user number stamped on the rows.</param>
/// <param name="TenantId">Tenant key of the agent's local mapping cache.</param>
public sealed record MikroWriteRequest(string DocumentType, string ExternalId, int ErpUserNo, string TenantId = "");

/// <summary>What a writer put into Mikro: the document key and its first row.</summary>
public sealed record MikroWrittenDocument(string DocumentTable, int EvrakTip, string Series, int Number, int HeaderRecNo);

/// <summary>
/// Runs one document write so that a phone document becomes exactly one Mikro document
/// (goal GOAL_ERP_YAZIM Y3a, decision D4):
/// <list type="number">
/// <item><description>refuse Mikro V16 (<c>ERP_VERSION_NOT_SUPPORTED</c>, D6) — writers know V15's columns only;</description></item>
/// <item><description>in one transaction: look the document up in <c>_ERPB_EVRAK_ESLESME</c> under an update lock; when found, answer with the existing document;</description></item>
/// <item><description>otherwise let the writer insert the rows, record the document in the same transaction and commit;</description></item>
/// <item><description>after the commit, save the local SQLite mapping as a cache. Losing it (a crash right after the commit) is harmless: the next attempt finds the Mikro record.</description></item>
/// </list>
/// Failures come back as <see cref="ErpWriteResult"/> with an <see cref="ErpWriteError"/> code: a lost
/// connection, timeout or deadlock is <c>ERP_UNAVAILABLE</c> (retryable); data problems are the
/// writer's <see cref="MikroWriteException"/>.
/// </summary>
public sealed class MikroDocumentWriteRunner
{
    internal const string V16ProbeSql = "SELECT COL_LENGTH('dbo.STOKLAR', 'sto_Guid')";

    private readonly MikroConnectionFactory _connections;
    private readonly MikroDocumentLedger _ledger;
    private readonly ErpBridge.Core.Stores.IMappingStore? _cache;
    private readonly ILogger<MikroDocumentWriteRunner> _logger;

    public MikroDocumentWriteRunner(
        MikroConnectionFactory connections, MikroDocumentLedger ledger, ErpBridge.Core.Stores.IMappingStore? cache, ILogger<MikroDocumentWriteRunner> logger)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        _cache = cache;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Test seam: runs right after the commit, before the local cache is written.</summary>
    internal Func<CancellationToken, Task>? AfterCommit { get; set; }

    public async Task<ErpWriteResult> RunAsync(
        MikroConnectionSettings settings,
        MikroWriteRequest request,
        Func<MikroWriteSession, CancellationToken, Task<MikroWrittenDocument>> write,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(write);

        MikroLedgerEntry entry;
        var alreadyWritten = false;
        try
        {
            await using var connection = _connections.CreateConnection(settings);
            await connection.OpenAsync(ct).ConfigureAwait(false);

            if (await connection.ExecuteScalarAsync<int?>(new CommandDefinition(V16ProbeSql, cancellationToken: ct)).ConfigureAwait(false) is not null)
            {
                return Failed(ErpWriteError.ErpVersionNotSupported("Mikro V16"));
            }

            await _ledger.EnsureTableAsync(connection, ct).ConfigureAwait(false);

            await using (var session = await MikroWriteSession.BeginAsync(
                connection, _ledger, settings.CompanyNo, settings.BranchNo, request.ErpUserNo, ct).ConfigureAwait(false))
            {
                if (await session.FindWrittenAsync(request.DocumentType, request.ExternalId, ct).ConfigureAwait(false) is { } existing)
                {
                    entry = existing;
                    alreadyWritten = true;
                }
                else
                {
                    var document = await write(session, ct).ConfigureAwait(false);
                    entry = new MikroLedgerEntry(
                        request.DocumentType, request.ExternalId, document.DocumentTable, document.EvrakTip, document.Series, document.Number, document.HeaderRecNo);

                    // The lookup above holds the key's range lock, so this only fails if something bypassed it;
                    // rolling back and retrying then finds the recorded document.
                    if (!await session.TryRecordAsync(entry, ct).ConfigureAwait(false))
                    {
                        return Failed(ErpWriteError.ErpUnavailable());
                    }

                    await session.CommitAsync(ct).ConfigureAwait(false);
                }
            }
        }
        catch (MikroWriteException ex)
        {
            return Failed(ex.Error);
        }
        catch (SqlException ex) when (MikroSqlErrors.IsUnavailable(ex) || MikroSqlErrors.IsUniqueViolation(ex))
        {
            _logger.LogWarning(ex, "Mikro write for {DocumentType} {ExternalId} will be retried (SQL {Number})", request.DocumentType, request.ExternalId, ex.Number);
            return Failed(ErpWriteError.ErpUnavailable());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Mikro write for {DocumentType} {ExternalId} failed", request.DocumentType, request.ExternalId);
            var detail = ex is SqlException sql ? $"SQL {sql.Number}" : ex.GetType().Name;
            return new ErpWriteResult(false, ErpWriteResult.ErrorCodeUnknown, $"Belge ERP'ye yazılırken beklenmeyen bir hata oluştu ({detail}).");
        }

        _logger.LogInformation(
            "Mikro document for {DocumentType} {ExternalId} {Outcome} {Series}-{Number} (RECno {Recno})",
            entry.DocumentType, entry.ExternalId, alreadyWritten ? "was already written as" : "written as", entry.EvrakSeri, entry.EvrakSira, entry.HeaderRecNo);

        if (!alreadyWritten && AfterCommit is { } afterCommit)
        {
            await afterCommit(ct).ConfigureAwait(false);
        }

        // Also when the document was already written: the attempt that wrote it may have died before
        // saving the cache, and this is the only chance to rebuild it (PR #84 Codex).
        await CacheAsync(settings, request, entry, ct).ConfigureAwait(false);
        return Written(entry);
    }

    private async Task CacheAsync(MikroConnectionSettings settings, MikroWriteRequest request, MikroLedgerEntry entry, CancellationToken ct)
    {
        if (_cache is null) return;
        try
        {
            if (await _cache.FindAsync(request.TenantId, request.DocumentType, request.ExternalId, ct).ConfigureAwait(false) is not null) return;
            await _cache.SaveAsync(new ErpBridge.Core.Domain.MappingRecord
            {
                TenantId = request.TenantId,
                EntityType = request.DocumentType,
                DocumentType = request.DocumentType,
                ExternalId = request.ExternalId,
                ErpType = ErpType.Mikro.ToString(),
                ErpVersion = MikroVersion.V15.ToString(),
                ErpDatabaseName = settings.DatabaseName,
                DocumentSeries = entry.EvrakSeri,
                DocumentNumber = entry.EvrakSira,
                Recno = entry.HeaderRecNo,
                CreatedAt = DateTime.UtcNow,
            }, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Mikro holds the authoritative record; the cache is a convenience.
            _logger.LogWarning(ex, "Local mapping cache for {DocumentType} {ExternalId} was not saved", request.DocumentType, request.ExternalId);
        }
    }

    private static ErpWriteResult Written(MikroLedgerEntry entry) =>
        new(true, ErpRecno: entry.HeaderRecNo, DocumentSeries: entry.EvrakSeri, DocumentNumber: entry.EvrakSira);

    private static ErpWriteResult Failed(ErpWriteError error) => new(false, error.Code, error.Message);
}
