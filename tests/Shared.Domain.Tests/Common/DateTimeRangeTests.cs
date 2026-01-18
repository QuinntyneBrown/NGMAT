using Shared.Domain.Common;

namespace Shared.Domain.Tests.Common;

public class DateTimeRangeTests
{
    [Fact]
    public void Constructor_WithValidDates_ShouldCreateRange()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        // Act
        var range = new DateTimeRange(start, end);

        // Assert
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
        Assert.Equal(TimeSpan.FromHours(1), range.Duration);
    }

    [Fact]
    public void Constructor_WithEndBeforeStart_ShouldThrowException()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DateTimeRange(start, end));
    }

    [Fact]
    public void Constructor_WithEqualDates_ShouldCreateZeroDurationRange()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;

        // Act
        var range = new DateTimeRange(date, date);

        // Assert
        Assert.Equal(date, range.Start);
        Assert.Equal(date, range.End);
        Assert.Equal(TimeSpan.Zero, range.Duration);
    }

    [Fact]
    public void Create_ShouldCreateRange()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        // Act
        var range = DateTimeRange.Create(start, end);

        // Assert
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void FromDuration_ShouldCreateRangeWithDuration()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromHours(2);

        // Act
        var range = DateTimeRange.FromDuration(start, duration);

        // Assert
        Assert.Equal(start, range.Start);
        Assert.Equal(start.Add(duration), range.End);
        Assert.Equal(duration, range.Duration);
    }

    [Fact]
    public void Contains_WithDateInRange_ShouldReturnTrue()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = new DateTimeRange(start, end);
        var dateTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var result = range.Contains(dateTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_WithDateOutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = new DateTimeRange(start, end);
        var dateTime = new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = range.Contains(dateTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_WithStartDate_ShouldReturnTrue()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = new DateTimeRange(start, end);

        // Act
        var result = range.Contains(start);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_WithEndDate_ShouldReturnTrue()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = new DateTimeRange(start, end);

        // Act
        var result = range.Contains(end);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WithOverlappingRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WithNonOverlappingRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_RangeWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Contains(range2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_RangeOutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Contains(range2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Intersect_WithOverlappingRange_ShouldReturnIntersection()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Intersect(range2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero), result.Value.Start);
        Assert.Equal(new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero), result.Value.End);
    }

    [Fact]
    public void Intersect_WithNonOverlappingRange_ShouldReturnNull()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Intersect(range2);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Union_ShouldReturnCombinedRange()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act
        var result = range1.Union(range2);

        // Assert
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), result.Start);
        Assert.Equal(new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero), result.End);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range1 = new DateTimeRange(start, end);
        var range2 = new DateTimeRange(start, end);

        // Act & Assert
        Assert.True(range1.Equals(range2));
        Assert.True(range1 == range2);
        Assert.False(range1 != range2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)
        );
        var range2 = new DateTimeRange(
            new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero)
        );

        // Act & Assert
        Assert.False(range1.Equals(range2));
        Assert.False(range1 == range2);
        Assert.True(range1 != range2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range1 = new DateTimeRange(start, end);
        var range2 = new DateTimeRange(start, end);

        // Act & Assert
        Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var range = new DateTimeRange(start, end);

        // Act
        var result = range.ToString();

        // Assert
        Assert.Contains("2024", result);
    }
}
