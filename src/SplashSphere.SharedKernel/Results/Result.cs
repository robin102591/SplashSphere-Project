namespace SplashSphere.SharedKernel.Results;

/// <summary>
/// Non-generic result for operations that return no value on success.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // ── Factories ────────────────────────────────────────────────────────────

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new Error(code, message));

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
    public static Result<T> Failure<T>(string code, string message) => new(default, false, new Error(code, message));
}

/// <summary>
/// Generic result monad for operations that return a value on success.
/// Supports functional composition via <c>Map</c>, <c>Bind</c>, and <c>Match</c>.
/// </summary>
public sealed class Result<T> : Result
{
    internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    // ── Implicit conversions ─────────────────────────────────────────────────

    /// <summary>Wraps a value in a successful result.</summary>
    public static implicit operator Result<T>(T value) => Success<T>(value);

    /// <summary>Wraps an error in a failed result.</summary>
    public static implicit operator Result<T>(Error error) => Failure<T>(error);

    // ── Functor: Map ─────────────────────────────────────────────────────────

    /// <summary>
    /// Transforms the success value. If this result is a failure, propagates
    /// the error without invoking <paramref name="map"/>.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess
            ? Result.Success(map(Value!))
            : Result.Failure<TOut>(Error);

    // ── Monad: Bind ──────────────────────────────────────────────────────────

    /// <summary>
    /// Chains a Result-returning function. Short-circuits on the first failure.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind) =>
        IsSuccess ? bind(Value!) : Result.Failure<TOut>(Error);

    /// <summary>
    /// Chains a non-generic Result-returning function.
    /// </summary>
    public Result Bind(Func<T, Result> bind) =>
        IsSuccess ? bind(Value!) : Result.Failure(Error);

    // ── Pattern matching ─────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> based on state.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error);

    // ── Tap / side-effects ───────────────────────────────────────────────────

    /// <summary>Executes a side-effect action if successful. Returns <c>this</c> for chaining.</summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess) action(Value!);
        return this;
    }

    /// <summary>Executes a side-effect action if failed. Returns <c>this</c> for chaining.</summary>
    public Result<T> OnFailure(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    // ── Guard ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the success value against a predicate. Returns a failure with
    /// <paramref name="error"/> if the predicate is false.
    /// </summary>
    public Result<T> Ensure(Func<T, bool> predicate, Error error) =>
        IsSuccess && !predicate(Value!) ? Result.Failure<T>(error) : this;
}
