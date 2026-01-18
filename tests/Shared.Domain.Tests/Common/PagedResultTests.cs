using Shared.Domain.Common;

namespace Shared.Domain.Tests.Common;

public class PagedResultTests
{
    [Fact]
    public void Constructor_ShouldCreatePagedResult()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = new PagedResult<int>(items, 50, 1, 10);

        // Assert
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(50, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var result = new PagedResult<int>(items, 50, 1, 10);

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(5, totalPages);
    }

    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var result = new PagedResult<int>(items, 25, 1, 10);

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(3, totalPages);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldReturnFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var result = new PagedResult<int>(items, 30, 1, 10);

        // Act & Assert
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ShouldReturnTrue()
    {
        // Arrange
        var items = new List<int> { 11, 12, 13 };
        var result = new PagedResult<int>(items, 30, 2, 10);

        // Act & Assert
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldReturnFalse()
    {
        // Arrange
        var items = new List<int> { 21, 22, 23 };
        var result = new PagedResult<int>(items, 23, 3, 10);

        // Act & Assert
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_OnFirstPage_ShouldReturnTrue()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var result = new PagedResult<int>(items, 30, 1, 10);

        // Act & Assert
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Empty_ShouldCreateEmptyResult()
    {
        // Act
        var result = PagedResult<int>.Empty();

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void Empty_WithCustomPaging_ShouldCreateEmptyResultWithCustomPaging()
    {
        // Act
        var result = PagedResult<int>.Empty(2, 20);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public void Map_ShouldTransformItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var result = new PagedResult<int>(items, 30, 1, 10);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.Equal(3, mapped.Items.Count);
        Assert.Equal("1", mapped.Items[0]);
        Assert.Equal("2", mapped.Items[1]);
        Assert.Equal("3", mapped.Items[2]);
        Assert.Equal(30, mapped.TotalCount);
        Assert.Equal(1, mapped.PageNumber);
        Assert.Equal(10, mapped.PageSize);
    }
}

public class PaginationRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest()
    {
        // Act
        var request = new PaginationRequest(2, 20);

        // Assert
        Assert.Equal(2, request.PageNumber);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public void Default_ShouldCreateDefaultRequest()
    {
        // Act
        var request = PaginationRequest.Default;

        // Assert
        Assert.Equal(1, request.PageNumber);
        Assert.Equal(10, request.PageSize);
    }

    [Fact]
    public void FirstPage_ShouldCreateFirstPageRequest()
    {
        // Act
        var request = PaginationRequest.FirstPage(20);

        // Assert
        Assert.Equal(1, request.PageNumber);
        Assert.Equal(20, request.PageSize);
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectly()
    {
        // Arrange
        var request = new PaginationRequest(3, 10);

        // Act
        var skip = request.Skip;

        // Assert
        Assert.Equal(20, skip); // (3 - 1) * 10 = 20
    }

    [Fact]
    public void Skip_OnFirstPage_ShouldReturnZero()
    {
        // Arrange
        var request = new PaginationRequest(1, 10);

        // Act
        var skip = request.Skip;

        // Assert
        Assert.Equal(0, skip);
    }

    [Fact]
    public void Take_ShouldReturnPageSize()
    {
        // Arrange
        var request = new PaginationRequest(1, 25);

        // Act
        var take = request.Take;

        // Assert
        Assert.Equal(25, take);
    }

    [Fact]
    public void DefaultConstructor_ShouldUseDefaults()
    {
        // Act
        var request = new PaginationRequest();

        // Assert
        Assert.Equal(1, request.PageNumber);
        Assert.Equal(10, request.PageSize);
    }
}
