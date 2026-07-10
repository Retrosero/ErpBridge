namespace ErpBridge.Shared;

/// <summary>
/// Lightweight success/failure result without exception throwing for expected error paths.
/// </summary>
/// <typeparam name="T">Payload type carried by a successful result.</typeparam>
public readonly struct Result<T>
{
    /// <summary>True when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Payload value when <see cref="IsSuccess"/> is true; default otherwise.</summary>
    public T? Value { get; }

    /// <summary>Human-readable error description when <see cref="IsSuccess"/> is false.</summary>
    public string? Error { get; }

    /// <summary>Machine-readable error code when <see cref="IsSuccess"/> is false.</summary>
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>Builds a successful result carrying <paramref name="value"/>.</summary>
    public static Result<T> Ok(T value) => new(true, value, null, null);

    /// <summary>Builds a failed result with a code and a human-readable message.</summary>
    public static Result<T> Fail(string errorCode, string errorMessage) =>
        new(false, default, errorMessage, errorCode);

    /// <summary>Builds a failed result wrapping the supplied <see cref="Error"/>.</summary>
    public static Result<T> Fail(Error error) => new(false, default, error.Message, error.Code);

    /// <summary>Returns the success value or throws if the result is a failure.</summary>
    public T ValueOrThrow()
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException(
                $"Result is failure: {ErrorCode} — {Error}");
        }

        return Value!;
    }
}