namespace Utils.Monad;

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type <typeparamref name="TOk"/> 
/// or fail with an error of type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TOk">The type of the value returned when the operation succeeds.</typeparam>
/// <typeparam name="TError">The type of the error returned when the operation fails.</typeparam>
public readonly record struct Result<TOk, TError>
{
    private readonly TOk? _ok;
    private readonly TError? _error;

    private Result(TOk? ok, TError? error, bool isOk)
    {
        _ok = ok;
        _error = error;
        IsOk = isOk;
    }

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// </summary>
    public bool IsErr => !IsOk;

    /// <summary>
    /// Creates a successful result containing the specified success value.
    /// </summary>
    /// <param name="success">The value to be wrapped as a successful result.</param>
    /// <returns>A result that represents a successful operation.</returns>
    public static Result<TOk, TError> Success(TOk success)
        => new(success, default, true);

    /// <summary>
    /// Creates a failed result containing the specified error value.
    /// </summary>
    /// <param name="error">The error to be wrapped as a failed result.</param>
    /// <returns>A result that represents a failed operation.</returns>
    public static Result<TOk, TError> Failure(TError error)
        => new(default, error, false);

    /// <summary>
    /// Creates a new result with the same success value but a different error type.
    /// Only valid when this result is a success.
    /// </summary>
    public Result<TOk, TNewError> WithOtherFailureType<TNewError>()
        => IsOk
            ? Result<TOk, TNewError>.Success(Ok)
            : throw new InvalidOperationException("Cannot change failure type when the result is an error.");

    /// <summary>
    /// Creates a new result with the same error value but a different success type.
    /// Only valid when this result is a failure.
    /// </summary>
    public Result<TNewOk, TError> WithOtherSuccessType<TNewOk>()
        => IsErr
            ? Result<TNewOk, TError>.Failure(Error)
            : throw new InvalidOperationException("Cannot change success type when the result is a success.");

    /// <summary>
    /// Gets the success value if the result represents a successful operation.
    /// Throws <see cref="InvalidOperationException"/> if accessed when the result is a failure.
    /// </summary>
    public TOk Ok => IsOk
        ? _ok!
        : throw new InvalidOperationException("No success value available because the result is a failure.");

    /// <summary>
    /// Gets the error value if the result represents a failed operation.
    /// Throws <see cref="InvalidOperationException"/> if accessed when the result is a failure.
    /// </summary>
    public TError Error => !IsOk
        ? _error!
        : throw new InvalidOperationException("No error value available because the result is a success.");
}