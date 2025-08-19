namespace EmuNX.Core.Common.Monad;

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type <typeparamref name="TSuccess"/> 
/// or fail with an error of type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TSuccess">The type of the value returned when the operation succeeds.</typeparam>
/// <typeparam name="TError">The type of the error returned when the operation fails.</typeparam>
public readonly record struct Result<TSuccess, TError>
{
    private readonly TSuccess? _success;
    private readonly TError? _error;

    /// <summary>
    /// Gets the success value if the result represents a successful operation.
    /// Throws <see cref="InvalidOperationException"/> if accessed when the result is a failure.
    /// </summary>
    public TSuccess Success => IsOk
        ? _success!
        : throw new InvalidOperationException("No success value available because the result is a failure.");

    /// <summary>
    /// Gets the error value if the result represents a failed operation.
    /// Throws <see cref="InvalidOperationException"/> if accessed when the result is a success.
    /// </summary>
    public TError Error => !IsOk
        ? _error!
        : throw new InvalidOperationException("No error value available because the result is a success.");

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// </summary>
    public bool IsErr => !IsOk;

    private Result(TSuccess? success, TError? error, bool isOk)
    {
        _success = success;
        _error = error;
        IsOk = isOk;
    }

    /// <summary>
    /// Creates a successful result containing the specified success value.
    /// </summary>
    /// <param name="success">The value to be wrapped as a successful result.</param>
    /// <returns>A result that represents a successful operation.</returns>
    public static Result<TSuccess, TError> Ok(TSuccess success)
        => new Result<TSuccess, TError>(success, default, true);

    /// <summary>
    /// Creates a failed result containing the specified error value.
    /// </summary>
    /// <param name="error">The error to be wrapped as a failed result.</param>
    /// <returns>A result that represents a failed operation.</returns>
    public static Result<TSuccess, TError> Err(TError error)
        => new Result<TSuccess, TError>(default, error, false);
}
