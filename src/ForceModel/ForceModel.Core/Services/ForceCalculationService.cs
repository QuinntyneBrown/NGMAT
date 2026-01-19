using ForceModel.Core.Entities;
using ForceModel.Core.Events;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace ForceModel.Core.Services;

public sealed class ForceCalculationResult
{
    public GravityAcceleration TotalAcceleration { get; init; }
    public GravityAcceleration GravityAcceleration { get; init; }
    public GravityAcceleration DragAcceleration { get; init; }
    public SrpAcceleration SrpAcceleration { get; init; }
    public GravityAcceleration ThirdBodyAcceleration { get; init; }
    public DateTime Epoch { get; init; }
    public bool InEclipse { get; init; }

    public double TotalMagnitude => TotalAcceleration.Magnitude;
    public double GravityMagnitude => GravityAcceleration.Magnitude;
    public double DragMagnitude => DragAcceleration.Magnitude;
    public double SrpMagnitude => SrpAcceleration.Magnitude;
    public double ThirdBodyMagnitude => ThirdBodyAcceleration.Magnitude;
}

public sealed class ThirdBodyPositions
{
    public double SunX { get; init; }
    public double SunY { get; init; }
    public double SunZ { get; init; }
    public double MoonX { get; init; }
    public double MoonY { get; init; }
    public double MoonZ { get; init; }
}

public sealed class AtmosphericConditions
{
    public double F107 { get; init; } = 150.0;
    public double Ap { get; init; } = 15.0;
    public double SunRightAscensionRad { get; init; }
}

public sealed class ForceCalculationService
{
    private readonly IEventPublisher _eventPublisher;

    public ForceCalculationService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Result<ForceCalculationResult> CalculateAcceleration(
        SpacecraftState state,
        SpacecraftProperties props,
        ForceModelConfiguration? config = null,
        ThirdBodyPositions? thirdBodies = null,
        AtmosphericConditions? atmosphere = null)
    {
        config ??= StandardForceModels.CreateMediumFidelity("system");
        atmosphere ??= new AtmosphericConditions();

        var totalAccel = GravityAcceleration.Zero;
        var gravityAccel = GravityAcceleration.Zero;
        var dragAccel = GravityAcceleration.Zero;
        var srpAccel = SrpAcceleration.Zero;
        var thirdBodyAccel = GravityAcceleration.Zero;

        // Calculate gravity
        if (config.EnableCentralBodyGravity)
        {
            gravityAccel = config.GravityModel switch
            {
                GravityModelType.PointMass => PointMassGravity.Calculate(state),
                GravityModelType.J2Only => J2Gravity.Calculate(state),
                GravityModelType.J2J3 => J2J3Gravity.Calculate(state),
                GravityModelType.SphericalHarmonics => J2J3Gravity.Calculate(state), // Simplified for now
                _ => PointMassGravity.Calculate(state)
            };
            totalAccel = totalAccel + gravityAccel;
        }

        // Calculate atmospheric drag
        if (config.EnableAtmosphericDrag && state.Altitude < 1000000)
        {
            var density = config.AtmosphereModel switch
            {
                AtmosphereModelType.Exponential => ExponentialAtmosphere.Calculate(state.Altitude),
                AtmosphereModelType.HarrisPriester => ExponentialAtmosphere.Calculate(state.Altitude), // Simplified
                AtmosphereModelType.NRLMSISE00 => ExponentialAtmosphere.Calculate(state.Altitude), // Simplified
                AtmosphereModelType.JacchiaRoberts => ExponentialAtmosphere.Calculate(state.Altitude), // Simplified
                _ => ExponentialAtmosphere.Calculate(state.Altitude)
            };

            dragAccel = AtmosphericDrag.Calculate(state, props, density);
            totalAccel = totalAccel + dragAccel;
        }

        // Calculate SRP
        if (config.EnableSolarRadiationPressure && thirdBodies != null)
        {
            srpAccel = CannonBallSrp.Calculate(
                state, props,
                thirdBodies.SunX, thirdBodies.SunY, thirdBodies.SunZ,
                config.EnableEclipsing);

            totalAccel = totalAccel + new GravityAcceleration(srpAccel.Ax, srpAccel.Ay, srpAccel.Az);
        }

        // Calculate third body perturbations
        if (thirdBodies != null)
        {
            if (config.EnableThirdBodySun)
            {
                var sunAccel = ThirdBodyGravity.Calculate(
                    state,
                    thirdBodies.SunX, thirdBodies.SunY, thirdBodies.SunZ,
                    GravityConstants.SunGM);
                thirdBodyAccel = thirdBodyAccel + sunAccel;
            }

            if (config.EnableThirdBodyMoon)
            {
                var moonAccel = ThirdBodyGravity.Calculate(
                    state,
                    thirdBodies.MoonX, thirdBodies.MoonY, thirdBodies.MoonZ,
                    GravityConstants.MoonGM);
                thirdBodyAccel = thirdBodyAccel + moonAccel;
            }

            totalAccel = totalAccel + thirdBodyAccel;
        }

        var result = new ForceCalculationResult
        {
            TotalAcceleration = totalAccel,
            GravityAcceleration = gravityAccel,
            DragAcceleration = dragAccel,
            SrpAcceleration = srpAccel,
            ThirdBodyAcceleration = thirdBodyAccel,
            Epoch = state.Epoch,
            InEclipse = srpAccel.Eclipse != EclipseType.None
        };

        return Result<ForceCalculationResult>.Success(result);
    }

    public Result<GravityAcceleration> CalculateGravityOnly(
        SpacecraftState state,
        GravityModelType model = GravityModelType.J2Only)
    {
        var accel = model switch
        {
            GravityModelType.PointMass => PointMassGravity.Calculate(state),
            GravityModelType.J2Only => J2Gravity.Calculate(state),
            GravityModelType.J2J3 => J2J3Gravity.Calculate(state),
            GravityModelType.SphericalHarmonics => J2J3Gravity.Calculate(state),
            _ => PointMassGravity.Calculate(state)
        };

        return Result<GravityAcceleration>.Success(accel);
    }

    public Result<GravityAcceleration> CalculateDragOnly(
        SpacecraftState state,
        SpacecraftProperties props,
        AtmosphereModelType model = AtmosphereModelType.Exponential)
    {
        if (state.Altitude > 1000000)
        {
            return Result<GravityAcceleration>.Success(GravityAcceleration.Zero);
        }

        var density = ExponentialAtmosphere.Calculate(state.Altitude);
        var accel = AtmosphericDrag.Calculate(state, props, density);

        return Result<GravityAcceleration>.Success(accel);
    }

    public Result<SrpAcceleration> CalculateSrpOnly(
        SpacecraftState state,
        SpacecraftProperties props,
        double sunX, double sunY, double sunZ,
        bool checkEclipse = true)
    {
        var accel = CannonBallSrp.Calculate(state, props, sunX, sunY, sunZ, checkEclipse);
        return Result<SrpAcceleration>.Success(accel);
    }

    public Result<EclipseType> CheckEclipse(
        SpacecraftState state,
        double sunX, double sunY, double sunZ)
    {
        var (eclipseType, _) = CannonBallSrp.CalculateEclipse(state, sunX, sunY, sunZ);
        return Result<EclipseType>.Success(eclipseType);
    }

    public Result<AtmosphericDensity> GetAtmosphericDensity(
        double altitudeM,
        AtmosphereModelType model = AtmosphereModelType.Exponential)
    {
        var density = model switch
        {
            AtmosphereModelType.Exponential => ExponentialAtmosphere.Calculate(altitudeM),
            _ => ExponentialAtmosphere.Calculate(altitudeM)
        };

        return Result<AtmosphericDensity>.Success(density);
    }
}
