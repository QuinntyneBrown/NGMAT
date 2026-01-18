using Shared.Domain.Results;

namespace Shared.Domain.Tests.Results;

public class ErrorTests
{
    [Fact]
    public void Constructor_ShouldCreateErrorWithCodeAndMessage()
    {
        // Act
        var error = new Error("TestCode", "Test message");

        // Assert
        Assert.Equal("TestCode", error.Code);
        Assert.Equal("Test message", error.Message);
        Assert.Null(error.Exception);
    }

    [Fact]
    public void Constructor_WithException_ShouldCreateErrorWithException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = new Error("TestCode", "Test message", exception);

        // Assert
        Assert.Equal("TestCode", error.Code);
        Assert.Equal("Test message", error.Message);
        Assert.Equal(exception, error.Exception);
    }

    [Fact]
    public void None_ShouldReturnEmptyError()
    {
        // Act
        var error = Error.None;

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
    }

    [Fact]
    public void Unknown_ShouldReturnUnknownError()
    {
        // Act
        var error = Error.Unknown;

        // Assert
        Assert.Equal("Unknown", error.Code);
        Assert.Equal("An unknown error occurred", error.Message);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Act
        var error = Error.NotFound("User", "123");

        // Assert
        Assert.Equal("User.NotFound", error.Code);
        Assert.Equal("User with ID '123' was not found", error.Message);
    }

    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Act
        var error = Error.Validation("Invalid input");

        // Assert
        Assert.Equal("Validation", error.Code);
        Assert.Equal("Invalid input", error.Message);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Act
        var error = Error.Conflict("Resource already exists");

        // Assert
        Assert.Equal("Conflict", error.Code);
        Assert.Equal("Resource already exists", error.Message);
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        Assert.Equal("Unauthorized", error.Code);
        Assert.Equal("Unauthorized access", error.Message);
    }

    [Fact]
    public void Unauthorized_WithMessage_ShouldCreateUnauthorizedErrorWithMessage()
    {
        // Act
        var error = Error.Unauthorized("Custom message");

        // Assert
        Assert.Equal("Unauthorized", error.Code);
        Assert.Equal("Custom message", error.Message);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Act
        var error = Error.Forbidden();

        // Assert
        Assert.Equal("Forbidden", error.Code);
        Assert.Equal("Access forbidden", error.Message);
    }

    [Fact]
    public void Forbidden_WithMessage_ShouldCreateForbiddenErrorWithMessage()
    {
        // Act
        var error = Error.Forbidden("Custom message");

        // Assert
        Assert.Equal("Forbidden", error.Code);
        Assert.Equal("Custom message", error.Message);
    }

    [Fact]
    public void Internal_ShouldCreateInternalError()
    {
        // Act
        var error = Error.Internal("Internal server error");

        // Assert
        Assert.Equal("Internal", error.Code);
        Assert.Equal("Internal server error", error.Message);
        Assert.Null(error.Exception);
    }

    [Fact]
    public void Internal_WithException_ShouldCreateInternalErrorWithException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = Error.Internal("Internal server error", exception);

        // Assert
        Assert.Equal("Internal", error.Code);
        Assert.Equal("Internal server error", error.Message);
        Assert.Equal(exception, error.Exception);
    }

    [Fact]
    public void FromException_ShouldCreateErrorFromException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var error = Error.FromException(exception);

        // Assert
        Assert.Equal("Exception", error.Code);
        Assert.Equal("Test exception", error.Message);
        Assert.Equal(exception, error.Exception);
    }

    [Fact]
    public void ToString_ShouldFormatErrorCorrectly()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("[TestCode] Test message", result);
    }
}

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_ShouldCreateValidationError()
    {
        // Act
        var error = new ValidationError("Field", "Error message");

        // Assert
        Assert.Equal("Field", error.Field);
        Assert.Equal("Error message", error.Message);
    }

    [Fact]
    public void ToString_ShouldFormatValidationErrorCorrectly()
    {
        // Arrange
        var error = new ValidationError("Username", "Username is required");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("Username: Username is required", result);
    }
}

public class ValidationErrorsTests
{
    [Fact]
    public void New_ValidationErrors_ShouldHaveNoErrors()
    {
        // Act
        var errors = new ValidationErrors();

        // Assert
        Assert.Empty(errors.Errors);
        Assert.False(errors.HasErrors);
    }

    [Fact]
    public void Add_WithFieldAndMessage_ShouldAddError()
    {
        // Arrange
        var errors = new ValidationErrors();

        // Act
        errors.Add("Username", "Username is required");

        // Assert
        Assert.Single(errors.Errors);
        Assert.True(errors.HasErrors);
        Assert.Equal("Username", errors.Errors[0].Field);
        Assert.Equal("Username is required", errors.Errors[0].Message);
    }

    [Fact]
    public void Add_WithValidationError_ShouldAddError()
    {
        // Arrange
        var errors = new ValidationErrors();
        var error = new ValidationError("Email", "Email is invalid");

        // Act
        errors.Add(error);

        // Assert
        Assert.Single(errors.Errors);
        Assert.True(errors.HasErrors);
        Assert.Equal("Email", errors.Errors[0].Field);
    }

    [Fact]
    public void AddRange_ShouldAddMultipleErrors()
    {
        // Arrange
        var errors = new ValidationErrors();
        var errorList = new[]
        {
            new ValidationError("Field1", "Error1"),
            new ValidationError("Field2", "Error2")
        };

        // Act
        errors.AddRange(errorList);

        // Assert
        Assert.Equal(2, errors.Errors.Count);
        Assert.True(errors.HasErrors);
    }

    [Fact]
    public void ToError_WithErrors_ShouldReturnError()
    {
        // Arrange
        var errors = new ValidationErrors();
        errors.Add("Field1", "Error1");
        errors.Add("Field2", "Error2");

        // Act
        var error = errors.ToError();

        // Assert
        Assert.Equal("Validation", error.Code);
        Assert.Contains("Field1: Error1", error.Message);
        Assert.Contains("Field2: Error2", error.Message);
    }

    [Fact]
    public void ToError_WithoutErrors_ShouldReturnNone()
    {
        // Arrange
        var errors = new ValidationErrors();

        // Act
        var error = errors.ToError();

        // Assert
        Assert.Equal(Error.None, error);
    }

    [Fact]
    public void ToResult_WithErrors_ShouldReturnFailure()
    {
        // Arrange
        var errors = new ValidationErrors();
        errors.Add("Field1", "Error1");

        // Act
        var result = errors.ToResult();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Fact]
    public void ToResult_WithoutErrors_ShouldReturnSuccess()
    {
        // Arrange
        var errors = new ValidationErrors();

        // Act
        var result = errors.ToResult();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ToResultGeneric_WithErrors_ShouldReturnFailure()
    {
        // Arrange
        var errors = new ValidationErrors();
        errors.Add("Field1", "Error1");

        // Act
        var result = errors.ToResult(42);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Fact]
    public void ToResultGeneric_WithoutErrors_ShouldReturnSuccessWithValue()
    {
        // Arrange
        var errors = new ValidationErrors();

        // Act
        var result = errors.ToResult(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }
}
