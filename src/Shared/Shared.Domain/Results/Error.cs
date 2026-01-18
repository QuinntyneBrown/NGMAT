namespace Shared.Domain.Results;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message, Exception? Exception = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error Unknown = new("Unknown", "An unknown error occurred");

    public static Error NotFound(string entity, string id) =>
        new($"{entity}.NotFound", $"{entity} with ID '{id}' was not found");

    public static Error Validation(string message) =>
        new("Validation", message);

    public static Error Conflict(string message) =>
        new("Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized access") =>
        new("Unauthorized", message);

    public static Error Forbidden(string message = "Access forbidden") =>
        new("Forbidden", message);

    public static Error Internal(string message, Exception? exception = null) =>
        new("Internal", message, exception);

    public static Error FromException(Exception exception) =>
        new("Exception", exception.Message, exception);

    public override string ToString() => $"[{Code}] {Message}";
}

/// <summary>
/// Represents a validation error with field-level details.
/// </summary>
public sealed record ValidationError(string Field, string Message)
{
    public override string ToString() => $"{Field}: {Message}";
}

/// <summary>
/// Represents multiple validation errors.
/// </summary>
public sealed class ValidationErrors
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyList<ValidationError> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message) => _errors.Add(new ValidationError(field, message));
    public void Add(ValidationError error) => _errors.Add(error);
    public void AddRange(IEnumerable<ValidationError> errors) => _errors.AddRange(errors);

    public Error ToError()
    {
        if (!HasErrors) return Error.None;
        var message = string.Join("; ", _errors.Select(e => e.ToString()));
        return new Error("Validation", message);
    }

    public Result ToResult() => HasErrors ? Result.Failure(ToError()) : Result.Success();
    public Result<T> ToResult<T>(T value) => HasErrors ? Result<T>.Failure(ToError()) : Result<T>.Success(value);
}
