using Shared.Domain.Identifiers;

namespace Shared.Domain.Tests.Identifiers;

public class MissionIdTests
{
    [Fact]
    public void Constructor_ShouldCreateIdWithGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = new MissionId(guid);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = MissionId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyId()
    {
        // Act
        var id = MissionId.Empty;

        // Assert
        Assert.Equal(Guid.Empty, id.Value);
    }

    [Fact]
    public void Parse_ShouldParseGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var id = MissionId.Parse(guidString);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new MissionId(guid);

        // Act
        Guid result = id;

        // Assert
        Assert.Equal(guid, result);
    }

    [Fact]
    public void ExplicitConversion_FromGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = (MissionId)guid;

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new MissionId(guid);
        var id2 = new MissionId(guid);

        // Act & Assert
        Assert.True(id1.Equals(id2));
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var id1 = MissionId.New();
        var id2 = MissionId.New();

        // Act & Assert
        Assert.False(id1.Equals(id2));
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new MissionId(guid);
        var id2 = new MissionId(guid);

        // Act & Assert
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new MissionId(guid);

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal(guid.ToString(), result);
    }
}

public class SpacecraftIdTests
{
    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = SpacecraftId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyId()
    {
        // Act
        var id = SpacecraftId.Empty;

        // Assert
        Assert.Equal(Guid.Empty, id.Value);
    }

    [Fact]
    public void Parse_ShouldParseGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var id = SpacecraftId.Parse(guidString);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Equals_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new SpacecraftId(guid);
        var id2 = new SpacecraftId(guid);
        var id3 = SpacecraftId.New();

        // Act & Assert
        Assert.True(id1 == id2);
        Assert.False(id1 == id3);
    }
}

public class UserIdTests
{
    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = UserId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyId()
    {
        // Act
        var id = UserId.Empty;

        // Assert
        Assert.Equal(Guid.Empty, id.Value);
    }

    [Fact]
    public void Parse_ShouldParseGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var id = UserId.Parse(guidString);

        // Assert
        Assert.Equal(guid, id.Value);
    }
}

public class PropagationIdTests
{
    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = PropagationId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Equals_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new PropagationId(guid);
        var id2 = new PropagationId(guid);

        // Act & Assert
        Assert.True(id1 == id2);
    }
}

public class ManeuverIdTests
{
    [Fact]
    public void New_ShouldCreateNewId()
    {
        // Act
        var id = ManeuverId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Equals_ShouldWorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new ManeuverId(guid);
        var id2 = new ManeuverId(guid);

        // Act & Assert
        Assert.True(id1 == id2);
    }
}
