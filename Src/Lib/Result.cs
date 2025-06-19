namespace EmuNX.Lib;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of an operation, which can either succeed with a value of type T,
/// or fail with an error of type E.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="E">The type of the error.</typeparam>
public class Result<T, E>
{
    /// <summary>
    /// Indicates whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The value returned if the result is successful.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// The error returned if the result is a failure.
    /// </summary>
    public E Error { get; }

    /// <summary>
    /// Protected constructor to enforce factory usage.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <param name="error">The error value.</param>
    /// <param name="isSuccess">Whether the result is successful.</param>
    protected Result(T value, E error, bool isSuccess)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;

        if (IsSuccess && error != null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!IsSuccess && EqualityComparer<E>.Default.Equals(error, default))
            throw new InvalidOperationException("A failed result must contain an error.");
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T, E> Success(T value)
    {
        return new Result<T, E>(value, default, true);
    }

    /// <summary>
    /// Creates a failed result with the given error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result.</returns>
    public static Result<T, E> Failure(E error)
    {
        return new Result<T, E>(default, error, false);
    }
}