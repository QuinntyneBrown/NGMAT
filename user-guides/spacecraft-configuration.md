# Spacecraft Configuration Guide

Learn how to define spacecraft properties, configure hardware, and set initial states in NGMAT.

## Table of Contents

- [Spacecraft Basics](#spacecraft-basics)
- [Creating Spacecraft](#creating-spacecraft)
- [Physical Properties](#physical-properties)
- [Initial State](#initial-state)
- [Hardware Configuration](#hardware-configuration)
- [Fuel Management](#fuel-management)
- [Attitude Configuration](#attitude-configuration)
- [Importing TLE](#importing-tle)
- [Spacecraft Validation](#spacecraft-validation)

## Spacecraft Basics

### What is a Spacecraft?

In NGMAT, a **spacecraft** is a complete definition of a vehicle including:

- **Physical Properties** - Mass, dimensions, coefficients
- **Initial State** - Position and velocity at epoch
- **Hardware** - Thrusters, tanks, solar panels, batteries
- **Attitude** - Orientation and spin configuration
- **Fuel Budget** - Propellant mass and consumption

Each mission can contain one or more spacecraft.

### Spacecraft Types

Common spacecraft configurations:

- **Satellites** - Communications, Earth observation, scientific
- **Space Stations** - ISS, future commercial stations
- **Probes** - Interplanetary and deep space missions
- **Landers** - Lunar and planetary surface missions
- **Crewed Vehicles** - Capsules and spaceplanes

## Creating Spacecraft

### Add Spacecraft to Mission

1. Open a mission
2. Click **"+ Add Spacecraft"** button
3. Enter spacecraft name
4. Configure properties (see sections below)
5. Click **"Create Spacecraft"**

### Spacecraft Wizard

The spacecraft wizard guides you through configuration:

**Step 1: Basic Properties**
- Name, mass, dimensions

**Step 2: Aerodynamic Properties**
- Drag coefficient and area

**Step 3: Solar Radiation Properties**
- SRP area and reflectivity

**Step 4: Initial State**
- Position and velocity or Keplerian elements

**Step 5: Hardware (Optional)**
- Thrusters, tanks, solar panels

## Physical Properties

### Mass Properties

**Dry Mass** (Required)
- Spacecraft mass without propellant
- Units: kg (kilograms) or lbm (pounds mass)
- Typical values:
  - CubeSat: 1-10 kg
  - Small satellite: 100-500 kg
  - Large satellite: 1,000-10,000 kg
  - Space station: 100,000+ kg

**Fuel Mass** (Required)
- Initial propellant mass
- Units: kg or lbm
- Updated automatically as maneuvers consume fuel

**Total Mass** (Calculated)
```
Total Mass = Dry Mass + Fuel Mass
```

### Aerodynamic Properties

Required for atmospheric drag calculations:

**Drag Coefficient (Cd)**
- Dimensionless coefficient
- Typical values: 2.0 - 2.5
- Default: 2.2
- Higher values = more drag

**Drag Area**
- Cross-sectional area facing velocity vector
- Units: m² (square meters) or ft²
- Typical: 1-100 m² depending on spacecraft size

**Example:**
```
CubeSat 1U:
  Cd = 2.2
  Area = 0.01 m² (10cm × 10cm)

ISS:
  Cd = 2.0
  Area = 1,500 m²
```

### Solar Radiation Pressure Properties

Required for SRP force calculations:

**SRP Area**
- Effective area for solar radiation
- Units: m² or ft²
- Often similar to drag area but can differ

**Reflectivity Coefficient (Cr)**
- Dimensionless coefficient
- Range: 0 (perfect absorption) to 2 (perfect reflection)
- Typical: 1.0 - 1.8
- Default: 1.8 (high reflectivity)

## Initial State

Define the spacecraft's position and velocity at mission start epoch.

### Input Methods

You can specify initial state using:

1. **Keplerian Orbital Elements** (Recommended)
2. **Cartesian State Vector** (Position and Velocity)
3. **TLE (Two-Line Elements)** Import

### Keplerian Elements

Most intuitive for defining orbits:

**Semi-major Axis (a)**
- Average orbital radius
- Units: km, m, AU
- Example: 6778 km for 400 km altitude LEO

**Eccentricity (e)**
- Orbit shape (0 = circular, <1 = elliptical)
- Dimensionless: 0.0 - 0.999...
- Example: 0.0 for circular, 0.01 for nearly circular

**Inclination (i)**
- Angle between orbit and equatorial plane
- Units: degrees or radians
- Range: 0° to 180°
- Examples:
  - 0° = Equatorial
  - 51.6° = ISS orbit
  - 98° = Sun-synchronous
  - 90° = Polar

**Right Ascension of Ascending Node (RAAN, Ω)**
- Orbital plane orientation
- Units: degrees or radians
- Range: 0° to 360°

**Argument of Perigee (ω)**
- Orientation of ellipse in orbital plane
- Units: degrees or radians
- Range: 0° to 360°
- N/A for circular orbits (e = 0)

**True Anomaly (ν)**
- Spacecraft position in orbit
- Units: degrees or radians
- Range: 0° to 360°
- 0° = at perigee
- 180° = at apogee

**Example: ISS-like Orbit**
```
a = 6778 km
e = 0.0005
i = 51.6°
Ω = 45°
ω = 0°
ν = 0°
Epoch: 2026-01-18 12:00:00 UTC
```

### Cartesian State Vector

Alternative state representation:

**Position Vector (r)**
- X, Y, Z coordinates
- Units: km or m
- Coordinate system must be specified

**Velocity Vector (v)**
- vX, vY, vZ components
- Units: km/s or m/s
- Same coordinate system as position

**Example: LEO Spacecraft**
```
Position:
  X = 3000 km
  Y = 5000 km
  Z = 3000 km

Velocity:
  vX = -5.5 km/s
  vY = 3.2 km/s
  vZ = 4.1 km/s

Coordinate System: ECI J2000
Epoch: 2026-01-18 12:00:00 UTC
```

### Coordinate Systems

Specify the reference frame for initial state:

**ECI J2000** (Earth-Centered Inertial)
- Recommended for Earth orbits
- Fixed inertial frame

**ECEF** (Earth-Centered Earth-Fixed)
- Rotates with Earth
- Good for ground-fixed targets

**Moon-Centered Inertial**
- For lunar missions

**Sun-Centered Inertial**
- For interplanetary missions

**Custom**
- User-defined coordinate systems

## Hardware Configuration

### Propulsion System

Define thrusters for maneuvers:

**Thruster Properties:**
- **Name** - Identifier (e.g., "Main Engine", "RCS Thruster 1")
- **Thrust** - Force produced (Newtons)
- **Specific Impulse (Isp)** - Efficiency (seconds)
- **Fuel Type** - Hydrazine, MMH/NTO, RP-1/LOX, etc.

**Example: Monopropellant Thruster**
```
Name: Main Thruster
Thrust: 500 N
Isp: 230 s
Fuel Type: Hydrazine
```

**Example: Bipropellant Engine**
```
Name: Apogee Motor
Thrust: 2000 N
Isp: 320 s
Fuel Type: MMH/NTO
```

### Fuel Tanks

Define propellant storage:

**Tank Properties:**
- **Name** - Identifier
- **Capacity** - Maximum fuel mass (kg)
- **Current Mass** - Initial fuel mass (kg)
- **Fuel Type** - Must match thruster

**Example:**
```
Name: Main Tank
Capacity: 150 kg
Current Mass: 100 kg
Fuel Type: Hydrazine
```

### Power System

**Solar Panels:**
- **Area** - Panel area (m²)
- **Efficiency** - Conversion efficiency (0.0-1.0)
- **Orientation** - Fixed or tracking

**Batteries:**
- **Capacity** - Energy storage (Wh or kWh)
- **Depth of Discharge** - Usable percentage

### Thermal System

**Radiators:**
- **Area** - Radiating surface (m²)
- **Emissivity** - Thermal radiation efficiency

## Fuel Management

### Monitor Fuel Consumption

View current fuel status:

1. Open spacecraft properties
2. Click **"Fuel"** tab
3. See:
   - Initial fuel mass
   - Current fuel mass
   - Fuel consumed
   - Fuel remaining percentage

### Fuel Budget Planning

Plan fuel allocation:

**Calculate Delta-V Budget:**
```
Δv = Isp × g₀ × ln(m₀ / mf)

Where:
  Isp = Specific impulse (s)
  g₀ = 9.80665 m/s² (standard gravity)
  m₀ = Initial total mass (kg)
  mf = Final total mass after burn (kg)
```

**Example:**
```
Initial mass: 600 kg (500 kg dry + 100 kg fuel)
Final mass: 550 kg (500 kg dry + 50 kg fuel)
Isp: 230 s

Δv = 230 × 9.80665 × ln(600/550)
Δv ≈ 200 m/s
```

### Fuel Warnings

NGMAT warns you when:
- Fuel drops below 20% of capacity
- Planned maneuvers exceed available fuel
- Fuel depletion predicted before mission end

## Attitude Configuration

Define spacecraft orientation:

### Attitude Modes

**Inertial**
- Fixed orientation in inertial space
- No active control

**Nadir Pointing**
- Points toward Earth center
- Common for Earth observation

**Sun Pointing**
- Orients solar panels toward Sun
- Maximizes power generation

**Velocity Pointing**
- Aligns with velocity vector
- Minimizes drag

**Custom**
- User-defined quaternion or Euler angles

### Attitude Definition

**Quaternion:**
- Four components: q0, q1, q2, q3
- Normalized: √(q0² + q1² + q2² + q3²) = 1

**Euler Angles:**
- Roll, Pitch, Yaw
- Units: degrees or radians
- Specify rotation sequence (e.g., 3-2-1)

**Spin Rate:**
- Rotation rate (deg/s or rad/s)
- For spin-stabilized spacecraft

## Importing TLE

### What is TLE?

Two-Line Element sets are standard format for satellite orbital elements:

```
ISS (ZARYA)
1 25544U 98067A   26018.50000000  .00002182  00000-0  41420-4 0  9990
2 25544  51.6416 247.4627 0006703 130.5360 325.0288 15.72125391563710
```

### Import TLE Data

1. Click **"Import TLE"** in spacecraft creation
2. Paste TLE data (2 or 3 lines)
3. NGMAT extracts:
   - Orbital elements
   - Epoch
   - Ballistic coefficient
4. Click **"Import"**
5. Review and confirm parameters

### TLE Sources

- [Celestrak](https://celestrak.org/) - Comprehensive TLE database
- [Space-Track](https://www.space-track.org/) - Official U.S. database
- [N2YO](https://www.n2yo.com/) - Real-time satellite tracking

## Spacecraft Validation

### Validate Configuration

Check spacecraft configuration for errors:

1. Open spacecraft properties
2. Click **"Validate"** button
3. Review validation results:
   - ✓ Passed checks
   - ⚠ Warnings
   - ✗ Errors

### Validation Checks

**Mass Budget:**
- Dry mass > 0
- Fuel mass ≥ 0
- Total mass reasonable

**Aerodynamic Properties:**
- Cd in valid range (0.1-4.0)
- Drag area > 0
- Values consistent with spacecraft size

**Solar Radiation:**
- Cr in valid range (0.0-2.0)
- SRP area > 0

**Initial State:**
- Orbital energy valid (elliptical, parabolic, or hyperbolic)
- Perigee altitude above surface
- Inclination in valid range

**Hardware:**
- Fuel capacity ≥ current fuel mass
- Thruster Isp reasonable (100-500 s typical)
- Tank fuel type matches thruster

### Common Warnings

**Warning: High eccentricity**
- Eccentricity > 0.9
- May indicate hyperbolic or escape trajectory
- Verify this is intended

**Warning: Low perigee altitude**
- Perigee < 200 km
- Rapid atmospheric decay expected
- Check if LEO mission is short-duration

**Warning: Large fuel budget**
- Fuel mass > 50% of total mass
- Unusual for most missions
- Verify propellant requirements

## Best Practices

### Naming

- Use descriptive spacecraft names
- Include identifiers for multi-spacecraft missions
- Examples: "Starlink-2157", "ISS", "Mars-Orbiter-1"

### Documentation

- Document data sources in description
- Note any assumptions made
- Record TLE epoch if using imported data

### Validation

- Always validate after changes
- Address all errors before propagation
- Review warnings carefully

### Units

- Be consistent with unit choices
- Double-check unit conversions
- Use SI units when in doubt

## Troubleshooting

**Issue: Spacecraft state invalid**
- Check orbital elements are in valid ranges
- Verify perigee altitude > 0
- Ensure velocities are reasonable

**Issue: Cannot import TLE**
- Verify TLE format (2 or 3 lines)
- Check for corrupted data
- Try copying TLE again from source

**Issue: Fuel consumption incorrect**
- Verify thruster Isp value
- Check fuel tank configuration
- Review maneuver delta-V calculations

---

**Next:** [Orbit Propagation Guide](orbit-propagation.md) - Learn to propagate spacecraft orbits and analyze trajectories.
