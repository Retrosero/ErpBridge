using System.Threading.Channels;

namespace ErpBridge.Core.Sync;

/// <summary>
/// Ajan hızı A2: "something was just written to the ERP — sync now". <see cref="Jobs.AgentJobPump"/> requests a round
/// after it wrote a phone document; <see cref="AgentSyncLoop"/> wakes for it instead of waiting out its interval, so
/// the ERP's result (invoice number, balance, stock) reaches the phones seconds after the write.
/// <para>Coalescing: one pending request at most. Ten documents written in one poll — or while a round is running — make
/// one more round, not ten. A request made during a round is kept, so the round after it sees that write too.</para>
/// <para>One instance per process (singleton in <c>AddErpBridgeCore</c>): the Windows service and the tray agent each
/// build one container, and the pump and the loop of a process share it.</para>
/// </summary>
public sealed class AgentSyncTrigger
{
    private readonly Channel<bool> _requests = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
    });

    /// <summary>Asks for a sync round. Never blocks; repeated requests before the round collapse into one.</summary>
    public void Request() => _requests.Writer.TryWrite(true);

    /// <summary>
    /// Waits up to <paramref name="timeout"/> for a request and consumes it. False when the time ran out.
    /// </summary>
    /// <exception cref="OperationCanceledException"><paramref name="ct"/> was cancelled.</exception>
    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken ct)
    {
        if (_requests.Reader.TryRead(out _)) return true;
        using var timer = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timer.CancelAfter(timeout);
        try
        {
            await _requests.Reader.ReadAsync(timer.Token).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return false;
        }
    }
}
