using System.Windows.Input;

namespace ErpBridge.Agent.UI;

/// <summary>
/// Async-aware <see cref="ICommand"/> that disables itself while the underlying
/// <see cref="Task"/> is in flight. Prevents the user from double-firing a slow
/// operation (e.g. the Mikro "Bağlantıyı test et" probe) by re-entering the same
/// command handler. Kept in its own file — the synchronous
/// <see cref="RelayCommand"/> keeps its public surface untouched.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CanExecute"/> returns <c>false</c> while <see cref="IsExecuting"/>
/// is <c>true</c>; this is what the WPF <c>Button.IsEnabled</c> binding reads
/// to grey the button out. The WPF command manager is then re-raised on
/// completion via the <see cref="CanExecuteChanged"/> event.
/// </para>
/// <para>
/// The command also exposes <see cref="ExecuteAsync"/> so view-models can
/// <c>await</c> the operation directly from non-command code paths. When
/// <see cref="Execute(object?)"/> is invoked from a binding the
/// <c>async void</c> wrapper is intentional — it's the contract WPF expects
/// on <see cref="ICommand.Execute"/>; exceptions are observed by the
/// <c>TaskScheduler</c> rather than propagating to the dispatcher.
/// </para>
/// <para>
/// Reentrancy: <see cref="CanExecute"/> is the gate. The first invocation
/// flips <see cref="IsExecuting"/> to <c>true</c>, so a second call from the
/// UI thread (or from <see cref="ExecuteAsync"/>) short-circuits before
/// touching the underlying delegate. There is at most one in-flight
/// execution at a time per command instance.
/// </para>
/// <para>
/// Thread affinity: <see cref="CanExecuteChanged"/> must fire on the UI thread
/// because the WPF command manager re-queries <see cref="CanExecute"/> on the
/// dispatcher's binding subsystem. The async delegate runs with
/// <c>ConfigureAwait(false)</c> so the post-await continuation lands on a
/// thread-pool thread; the constructor captures
/// <see cref="SynchronizationContext.Current"/> (the WPF UI thread) and the
/// setter/raise methods post back to it. Falls back to direct invocation when
/// no context is captured (e.g. unit tests).
/// </para>
/// </remarks>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<CancellationToken, Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly SynchronizationContext? _uiContext;
    private CancellationTokenSource? _executingCts;
    private bool _isExecuting;

    /// <summary>Build an async command. <paramref name="execute"/> is required.</summary>
    public AsyncRelayCommand(Func<CancellationToken, Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        // WPF app lifetime guarantees SynchronizationContext.Current is the
        // DispatcherSynchronizationContext at construction; tests that
        // construct commands outside the dispatcher get null and fall back
        // to synchronous invocation.
        _uiContext = SynchronizationContext.Current;
    }

    /// <summary>True while a previous invocation is still running.</summary>
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting == value)
            {
                return;
            }

            _isExecuting = value;
            RaiseCanExecuteChanged();
        }
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
        => !IsExecuting && (_canExecute?.Invoke() ?? true);

    /// <inheritdoc />
    /// <remarks>
    /// Fire-and-forget by design — the <see cref="ICommand"/> contract
    /// returns <c>void</c>. Use <see cref="ExecuteAsync"/> when a caller needs
    /// to await completion directly (tests, chained view-model code).
    /// </remarks>
    public async void Execute(object? parameter)
    {
        await ExecuteAsync(parameter).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaitable variant. Returns the <see cref="Task"/> produced by the
    /// delegate; if the command is already running, returns a completed task
    /// without invoking the delegate a second time.
    /// </summary>
    public async Task ExecuteAsync(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        // Capture the in-flight CTS in a local so a concurrent Cancel call
        // (rare, but possible from another thread) cannot NRE on us.
        var cts = new CancellationTokenSource();
        _executingCts = cts;

        try
        {
            IsExecuting = true;
            await _execute(cts.Token).ConfigureAwait(false);
        }
        finally
        {
            // Only flip IsExecuting back if THIS call's CTS is still the
            // active one. If a newer command was somehow queued (shouldn't
            // happen — CanExecute gates that), we don't stomp its state.
            if (ReferenceEquals(_executingCts, cts))
            {
                _executingCts = null;
                IsExecuting = false;
            }

            cts.Dispose();
        }
    }

    /// <summary>
    /// Force a <see cref="CanExecuteChanged"/> notification. The WPF command
    /// manager re-queries <see cref="CanExecute"/> for every bound control
    /// on the next dispatcher pass. Marshals to the UI thread when called
    /// from a background continuation.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler is null)
        {
            return;
        }

        if (_uiContext is not null)
        {
            _uiContext.Post(_ => handler(this, EventArgs.Empty), null);
        }
        else
        {
            handler(this, EventArgs.Empty);
        }
    }
}
