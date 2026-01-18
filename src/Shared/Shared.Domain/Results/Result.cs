namespace Shared.Domain.Results;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public readonly struct Result
{
    private readonly Error? _error;

    private Result(Error? error)
    {
        _error = error;
    }

    public bool IsSuccess => _error is null;
    public bool IsFailure => !IsSuccess;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot access error on successful result");

    public static Result Success() => new(null);
    public static Result Failure(Error error) => new(error);
    public static Result Failure(string code, string message) => new(new Error(code, message));

    public static implicit operator Result(Error error) => Failure(error);

    public Result<T> Map<T>(Func<T> mapper)
    {
        return IsSuccess ? Result<T>.Success(mapper()) : Result<T>.Failure(Error);
    }

    public async Task<Result<T>> MapAsync<T>(Func<Task<T>> mapper)
    {
        return IsSuccess ? Result<T>.Success(await mapper()) : Result<T>.Failure(Error);
    }
}

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    public bool IsSuccess => _error is null;
    public bool IsFailure => !IsSuccess;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access value on failed result");
    public Error Error => _error ?? throw new InvalidOperationException("Cannot access error on successful result");

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
    public static Result<T> Failure(string code, string message) => new(default, new Error(code, message));

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return IsSuccess ? Result<TResult>.Success(mapper(_value!)) : Result<TResult>.Failure(Error);
    }

    public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper)
    {
        return IsSuccess ? Result<TResult>.Success(await mapper(_value!)) : Result<TResult>.Failure(Error);
    }

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        return IsSuccess ? binder(_value!) : Result<TResult>.Failure(Error);
    }

    public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> binder)
    {
        return IsSuccess ? await binder(_value!) : Result<TResult>.Failure(Error);
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? _value! : defaultValue;
    }

    public T GetValueOrThrow()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException($"Operation failed: [{Error.Code}] {Error.Message}");
        }
        return _value!;
    }

    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(_error!);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }
}

/// <summary>
/// Extension methods for Result types.
/// </summary>
public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T? value, Error errorIfNull)
        where T : class
    {
        return value is not null ? Result<T>.Success(value) : Result<T>.Failure(errorIfNull);
    }

    public static Result<T> ToResult<T>(this T? value, Error errorIfNull)
        where T : struct
    {
        return value.HasValue ? Result<T>.Success(value.Value) : Result<T>.Failure(errorIfNull);
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> task, Error errorIfNull)
        where T : class
    {
        var value = await task;
        return value.ToResult(errorIfNull);
    }
}
