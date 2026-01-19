using ForceModel.Core.Entities;
using ForceModel.Core.Events;
using ForceModel.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace ForceModel.Core.Services;

public sealed class ForceModelService
{
    private readonly IForceModelUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public ForceModelService(IForceModelUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ForceModelConfiguration>> CreateConfigurationAsync(
        string name,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Configurations.ExistsByNameAsync(name, cancellationToken))
        {
            return Result<ForceModelConfiguration>.Failure(Error.Conflict($"A force model configuration with name '{name}' already exists"));
        }

        var config = ForceModelConfiguration.Create(name, createdByUserId, description, missionId);

        await _unitOfWork.Configurations.AddAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationCreatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            MissionId = config.MissionId
        }, cancellationToken);

        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result<ForceModelConfiguration>> GetConfigurationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<ForceModelConfiguration>.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }
        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result<IReadOnlyList<ForceModelConfiguration>>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        var configs = await _unitOfWork.Configurations.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<ForceModelConfiguration>>.Success(configs);
    }

    public async Task<Result<IReadOnlyList<ForceModelConfiguration>>> GetByMissionIdAsync(
        Guid missionId,
        CancellationToken cancellationToken = default)
    {
        var configs = await _unitOfWork.Configurations.GetByMissionIdAsync(missionId, cancellationToken);
        return Result<IReadOnlyList<ForceModelConfiguration>>.Success(configs);
    }

    public async Task<Result<ForceModelConfiguration>> UpdateGravitySettingsAsync(
        Guid id,
        bool enable,
        GravityModelType model,
        int degree,
        int order,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<ForceModelConfiguration>.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }

        config.SetGravitySettings(enable, model, degree, order);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "Gravity"
        }, cancellationToken);

        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result<ForceModelConfiguration>> UpdateAtmosphereSettingsAsync(
        Guid id,
        bool enable,
        AtmosphereModelType model,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<ForceModelConfiguration>.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }

        config.SetAtmosphereSettings(enable, model);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "Atmosphere"
        }, cancellationToken);

        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result<ForceModelConfiguration>> UpdateSrpSettingsAsync(
        Guid id,
        bool enable,
        SrpModelType model,
        bool enableEclipsing,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<ForceModelConfiguration>.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }

        config.SetSrpSettings(enable, model, enableEclipsing);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "SRP"
        }, cancellationToken);

        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result<ForceModelConfiguration>> UpdateThirdBodySettingsAsync(
        Guid id,
        bool enableSun,
        bool enableMoon,
        bool enablePlanets,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<ForceModelConfiguration>.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }

        config.SetThirdBodySettings(enableSun, enableMoon, enablePlanets);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "ThirdBody"
        }, cancellationToken);

        return Result<ForceModelConfiguration>.Success(config);
    }

    public async Task<Result> DeleteConfigurationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null)
        {
            return Result.Failure(Error.NotFound("ForceModelConfiguration", id.ToString()));
        }

        config.Delete();
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ForceModelConfigurationDeletedEvent
        {
            ConfigurationId = config.Id
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> CreateStandardConfigurationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("Low Fidelity", cancellationToken))
        {
            var lowFidelity = StandardForceModels.CreateLowFidelity(userId);
            await _unitOfWork.Configurations.AddAsync(lowFidelity, cancellationToken);
            count++;
        }

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("Medium Fidelity", cancellationToken))
        {
            var mediumFidelity = StandardForceModels.CreateMediumFidelity(userId);
            await _unitOfWork.Configurations.AddAsync(mediumFidelity, cancellationToken);
            count++;
        }

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("High Fidelity", cancellationToken))
        {
            var highFidelity = StandardForceModels.CreateHighFidelity(userId);
            await _unitOfWork.Configurations.AddAsync(highFidelity, cancellationToken);
            count++;
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(count);
    }
}
