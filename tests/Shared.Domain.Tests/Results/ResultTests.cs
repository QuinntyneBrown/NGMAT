using Shared.Domain.Results;

namespace Shared.Domain.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("TestCode", result.Error.Code);
        Assert.Equal("Test message", result.Error.Message);
    }

    [Fact]
    public void Failure_WithCodeAndMessage_ShouldCreateFailedResult()
    {
        // Act
        var result = Result.Failure("TestCode", "Test message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("TestCode", result.Error.Code);
        Assert.Equal("Test message", result.Error.Message);
    }

    [Fact]
    public void Error_OnSuccessfulResult_ShouldThrowException()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Map_WithSuccessfulResult_ShouldMapValue()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedResult = result.Map(() => 42);

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(42, mappedResult.Value);
    }

    [Fact]
    public void Map_WithFailedResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result.Failure(error);

        // Act
        var mappedResult = result.Map(() => 42);

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal("TestCode", mappedResult.Error.Code);
    }

    [Fact]
    public async Task MapAsync_WithSuccessfulResult_ShouldMapValue()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedResult = await result.MapAsync(() => Task.FromResult(42));

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal(42, mappedResult.Value);
    }

    [Fact]
    public async Task MapAsync_WithFailedResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result.Failure(error);

        // Act
        var mappedResult = await result.MapAsync(() => Task.FromResult(42));

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal("TestCode", mappedResult.Error.Code);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = new("TestCode", "Test message");

        // Act
        Result result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("TestCode", result.Error.Code);
    }
}

public class ResultTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("TestCode", result.Error.Code);
    }

    [Fact]
    public void Value_OnFailedResult_ShouldThrowException()
    {
        // Arrange
        var result = Result<int>.Failure("TestCode", "Test message");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_OnSuccessfulResult_ShouldThrowException()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Map_WithSuccessfulResult_ShouldMapValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mappedResult = result.Map(x => x.ToString());

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("42", mappedResult.Value);
    }

    [Fact]
    public void Map_WithFailedResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);

        // Act
        var mappedResult = result.Map(x => x.ToString());

        // Assert
        Assert.False(mappedResult.IsSuccess);
        Assert.Equal("TestCode", mappedResult.Error.Code);
    }

    [Fact]
    public async Task MapAsync_WithSuccessfulResult_ShouldMapValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mappedResult = await result.MapAsync(x => Task.FromResult(x.ToString()));

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("42", mappedResult.Value);
    }

    [Fact]
    public void Bind_WithSuccessfulResult_ShouldBindValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var boundResult = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("42", boundResult.Value);
    }

    [Fact]
    public void Bind_WithFailedResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TestCode", "Test message");
        var result = Result<int>.Failure(error);

        // Act
        var boundResult = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        Assert.False(boundResult.IsSuccess);
        Assert.Equal("TestCode", boundResult.Error.Code);
    }

    [Fact]
    public async Task BindAsync_WithSuccessfulResult_ShouldBindValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var boundResult = await result.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("42", boundResult.Value);
    }

    [Fact]
    public void GetValueOrDefault_WithSuccessfulResult_ShouldReturnValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.GetValueOrDefault(-1);

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetValueOrDefault_WithFailedResult_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<int>.Failure("TestCode", "Test message");

        // Act
        var value = result.GetValueOrDefault(-1);

        // Assert
        Assert.Equal(-1, value);
    }

    [Fact]
    public void GetValueOrThrow_WithSuccessfulResult_ShouldReturnValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetValueOrThrow_WithFailedResult_ShouldThrowException()
    {
        // Arrange
        var result = Result<int>.Failure("TestCode", "Test message");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.GetValueOrThrow());
    }

    [Fact]
    public void Match_WithSuccessfulResult_ShouldCallSuccessAction()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        result.Match(
            _ => successCalled = true,
            _ => failureCalled = true
        );

        // Assert
        Assert.True(successCalled);
        Assert.False(failureCalled);
    }

    [Fact]
    public void Match_WithFailedResult_ShouldCallFailureAction()
    {
        // Arrange
        var result = Result<int>.Failure("TestCode", "Test message");
        var successCalled = false;
        var failureCalled = false;

        // Act
        result.Match(
            _ => successCalled = true,
            _ => failureCalled = true
        );

        // Assert
        Assert.False(successCalled);
        Assert.True(failureCalled);
    }

    [Fact]
    public void Match_WithFunc_ShouldReturnCorrectValue()
    {
        // Arrange
        var successResult = Result<int>.Success(42);
        var failureResult = Result<int>.Failure("TestCode", "Test message");

        // Act
        var successValue = successResult.Match(x => x.ToString(), e => "error");
        var failureValue = failureResult.Match(x => x.ToString(), e => "error");

        // Assert
        Assert.Equal("42", successValue);
        Assert.Equal("error", failureValue);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = new("TestCode", "Test message");

        // Act
        Result<int> result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("TestCode", result.Error.Code);
    }
}

public class ResultExtensionsTests
{
    [Fact]
    public void ToResult_WithNonNullReferenceType_ShouldReturnSuccess()
    {
        // Arrange
        string? value = "test";
        var error = new Error("TestCode", "Test message");

        // Act
        var result = value.ToResult(error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void ToResult_WithNullReferenceType_ShouldReturnFailure()
    {
        // Arrange
        string? value = null;
        var error = new Error("TestCode", "Test message");

        // Act
        var result = value.ToResult(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("TestCode", result.Error.Code);
    }

    [Fact]
    public void ToResult_WithValueTypeHasValue_ShouldReturnSuccess()
    {
        // Arrange
        int? value = 42;
        var error = new Error("TestCode", "Test message");

        // Act
        var result = value.ToResult(error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ToResult_WithValueTypeNoValue_ShouldReturnFailure()
    {
        // Arrange
        int? value = null;
        var error = new Error("TestCode", "Test message");

        // Act
        var result = value.ToResult(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("TestCode", result.Error.Code);
    }

    [Fact]
    public async Task ToResultAsync_WithNonNullValue_ShouldReturnSuccess()
    {
        // Arrange
        var task = Task.FromResult<string?>("test");
        var error = new Error("TestCode", "Test message");

        // Act
        var result = await task.ToResultAsync(error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public async Task ToResultAsync_WithNullValue_ShouldReturnFailure()
    {
        // Arrange
        var task = Task.FromResult<string?>(null);
        var error = new Error("TestCode", "Test message");

        // Act
        var result = await task.ToResultAsync(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("TestCode", result.Error.Code);
    }
}
