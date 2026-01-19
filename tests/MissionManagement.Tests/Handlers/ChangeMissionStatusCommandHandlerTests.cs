using FluentAssertions;
using MissionManagement.Core.Commands;
using MissionManagement.Core.Entities;
using MissionManagement.Core.Enums;
using MissionManagement.Core.Interfaces;
using MissionManagement.Infrastructure.Handlers;
using Moq;
using Shared.Contracts.Events;
using Shared.Messaging.Abstractions;

namespace MissionManagement.Tests.Handlers;

public class ChangeMissionStatusCommandHandlerTests
{
    private readonly Mock<IMissionRepository> _mockRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly ChangeMissionStatusCommandHandler _handler;

    public ChangeMissionStatusCommandHandlerTests()
    {
        _mockRepository = new Mock<IMissionRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _handler = new ChangeMissionStatusCommandHandler(_mockRepository.Object, _mockEventBus.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidStatusChange_ChangesStatusAndPublishesEvent()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var mission = Mission.Create("Test Mission", MissionType.LEO, DateTimeOffset.UtcNow, ownerId);
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        var command = new ChangeMissionStatusCommand(
            missionId,
            MissionStatus.Active,
            ownerId,
            "Starting mission");

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        mission.Status.Should().Be(MissionStatus.Active);
        _mockRepository.Verify(r => r.UpdateAsync(mission, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockEventBus.Verify(
            e => e.PublishAsync(
                It.IsAny<MissionStateChanged>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MissionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var command = new ChangeMissionStatusCommand(
            missionId,
            MissionStatus.Active,
            Guid.NewGuid());

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Mission with ID {missionId} not found.");
    }

    [Fact]
    public async Task HandleAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var mission = Mission.Create("Test Mission", MissionType.LEO, DateTimeOffset.UtcNow, ownerId);
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        var command = new ChangeMissionStatusCommand(
            missionId,
            MissionStatus.Active,
            otherUserId);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only the mission owner can change its status.");
    }

    [Fact]
    public async Task HandleAsync_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var mission = Mission.Create("Test Mission", MissionType.LEO, DateTimeOffset.UtcNow, ownerId);
        mission.ChangeStatus(MissionStatus.Archived);
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);

        var command = new ChangeMissionStatusCommand(
            missionId,
            MissionStatus.Active,
            ownerId);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot change status of an archived mission.");
    }
}
