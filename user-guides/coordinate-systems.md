# Coordinate Systems Guide

Learn about reference frames, coordinate transformations, and state representations in NGMAT.

## Table of Contents

- [Coordinate System Basics](#coordinate-system-basics)
- [Common Coordinate Systems](#common-coordinate-systems)
- [State Representations](#state-representations)
- [Coordinate Transformations](#coordinate-transformations)
- [Custom Coordinate Systems](#custom-coordinate-systems)

## Coordinate System Basics

### What is a Coordinate System?

A **coordinate system** (reference frame) defines:
- **Origin** - Center point (e.g., Earth center)
- **Axes orientation** - Direction of X, Y, Z axes
- **Rotation** - Fixed (inertial) or rotating

### Why Multiple Coordinate Systems?

Different frames are natural for different purposes:
- **ECI J2000** - Orbital dynamics and propagation
- **ECEF** - Ground station tracking
- **VNB** - Maneuver planning
- **LVLH** - Rendezvous and proximity operations

## Common Coordinate Systems

### Earth-Centered Inertial (ECI)

**J2000 (ECI J2000):**
- Most common inertial frame
- Origin: Earth center
- X-axis: Vernal equinox direction at J2000 epoch
- Z-axis: Earth rotation axis at J2000
- Fixed in inertial space (doesn't rotate with Earth)

**Uses:**
- Orbit propagation
- Trajectory analysis
- Standard for most computations

**Characteristics:**
- Inertial (no Coriolis forces)
- Fixed star background
- Simple equations of motion

### Earth-Centered Earth-Fixed (ECEF)

**WGS-84 ECEF:**
- Rotates with Earth
- Origin: Earth center
- X-axis: Through prime meridian (0° longitude)
- Z-axis: Through North Pole
- Rotates once per sidereal day

**Uses:**
- Ground station locations
- Latitude/longitude conversions
- GPS navigation
- Earth-fixed targets

**Characteristics:**
- Rotating frame
- Fixed to Earth surface
- Requires transformation from ECI

### Mean of J2000 (MOJ2000 / GCRF)

**Geocentric Celestial Reference Frame:**
- Similar to ECI J2000
- Standard for high-precision work
- Accounts for precession/nutation

**Uses:**
- High-precision propagation
- Interplanetary missions
- Professional applications

### True of Date (TOD)

**Earth True Equator:**
- Accounts for precession and nutation
- Changes with time
- More accurate than J2000 for short-term

**Uses:**
- Ground tracking
- Real-time operations
- Short-duration missions

### Body-Fixed Frames

**VNB (Velocity-Normal-Binormal):**
- Origin: Spacecraft center of mass
- V: Along velocity vector
- N: Normal to orbital plane (angular momentum direction)
- B: Completes right-hand system (V × N)

**Uses:**
- Maneuver planning (most intuitive)
- Delta-V specification
- Propulsion analysis

**LVLH (Local Vertical Local Horizontal):**
- Origin: Spacecraft
- X: Radial outward (local vertical)
- Y: Along velocity direction (local horizontal)
- Z: Normal to orbital plane

**Uses:**
- Rendezvous operations
- Proximity operations
- Formation flying

**RSW (Radial-Transverse-Normal):**
- Similar to LVLH
- R: Radial outward
- S: Along-track direction
- W: Cross-track (normal)

**Uses:**
- Relative motion analysis
- Orbit perturbations
- Close-proximity operations

### Other Celestial Bodies

**Moon-Centered Inertial:**
- Origin: Moon center
- Similar to ECI but Moon-centric

**Sun-Centered Inertial (Heliocentric):**
- Origin: Sun center
- For interplanetary missions

**Body-Centered Systems:**
- Mars-Centered, Jupiter-Centered, etc.
- For planetary missions

## State Representations

### Cartesian State Vector

Position and velocity in Cartesian coordinates:

```
State = [X, Y, Z, vX, vY, vZ]

Units:
  Position: km
  Velocity: km/s

Example (LEO in ECI J2000):
  X = 3000 km
  Y = 5000 km
  Z = 3000 km
  vX = -5.5 km/s
  vY = 3.2 km/s
  vZ = 4.1 km/s
```

**Advantages:**
- Direct from integration
- No singularities
- Simple transformations

**Disadvantages:**
- Not intuitive
- Hard to interpret orbit shape

### Keplerian Elements

Classical orbital elements:

```
Elements:
  a - Semi-major axis (km)
  e - Eccentricity (dimensionless)
  i - Inclination (deg)
  Ω - Right Ascension of Ascending Node (deg)
  ω - Argument of Perigee (deg)
  ν - True Anomaly (deg)

Example (ISS):
  a = 6778 km
  e = 0.0005
  i = 51.6°
  Ω = 45°
  ω = 0°
  ν = 120°
```

**Advantages:**
- Intuitive orbit description
- Constant for two-body motion
- Easy to specify orbits

**Disadvantages:**
- Singularities for circular/equatorial orbits
- Multiple definitions (mean, osculating)

### Alternative Elements

**Modified Equinoctial Elements:**
- No singularities
- Good for near-circular/equatorial orbits

**Delaunay Elements:**
- Action-angle variables
- Used in perturbation theory

**Poincaré Elements:**
- Alternative to Delaunay
- Certain advantages for analysis

### Geodetic Coordinates

Latitude, longitude, altitude:

```
Geodetic:
  Latitude: -90° to +90°
  Longitude: -180° to +180° (or 0° to 360°)
  Altitude: Height above reference ellipsoid (km)

Example (Kennedy Space Center):
  Lat = 28.5729°N
  Lon = 80.6490°W
  Alt = 0.003 km (3 m)
```

**Reference Ellipsoids:**
- WGS-84 (most common)
- GRS-80
- NAD-83

## Coordinate Transformations

### ECI to ECEF

Transform from inertial to rotating frame:

**Requires:**
- Epoch (date and time)
- Earth rotation parameters
- UT1-UTC correction

**Rotation:**
```
r_ECEF = R_z(θ) × r_ECI

Where θ = Earth rotation angle at epoch
```

**NGMAT Usage:**
```
POST /api/coordinates/transform
{
  "fromSystem": "ECI_J2000",
  "toSystem": "ECEF",
  "epoch": "2026-01-18T12:00:00Z",
  "state": {...}
}
```

### ECEF to Geodetic

Convert Cartesian to Lat/Lon/Alt:

**Algorithm:**
- Iterative (longitude direct, latitude/altitude iterative)
- Accounts for Earth ellipsoid shape

**Formula (simplified):**
```
Longitude = atan2(Y, X)
Latitude = atan2(Z, √(X² + Y²))  (approximate)
Altitude = √(X² + Y² + Z²) - R_earth  (approximate)
```

**Precise algorithm accounts for ellipsoid.**

### Geodetic to ECEF

Convert Lat/Lon/Alt to Cartesian:

**Formula:**
```
X = (N + h) × cos(φ) × cos(λ)
Y = (N + h) × cos(φ) × sin(λ)
Z = (N × (1 - e²) + h) × sin(φ)

Where:
  N = a / √(1 - e² × sin²(φ))
  a = equatorial radius
  e = eccentricity
  φ = latitude
  λ = longitude
  h = altitude
```

### State to Keplerian Elements

Convert Cartesian state to orbital elements:

**Algorithm:**
1. Compute specific angular momentum: h = r × v
2. Compute eccentricity vector: e_vec
3. Calculate orbital elements from vectors

**NGMAT Usage:**
```
POST /api/coordinates/state-to-keplerian
{
  "position": [3000, 5000, 3000],
  "velocity": [-5.5, 3.2, 4.1],
  "mu": 398600.4418  // Earth GM
}
```

### Keplerian Elements to State

Convert elements to state vector:

**Algorithm:**
1. Calculate position in orbital plane
2. Rotate to reference frame
3. Calculate velocity

**Special Cases:**
- Circular orbits: ω undefined
- Equatorial orbits: Ω undefined
- Handle carefully in code

### Body-Fixed Frame Transformations

**ECI to VNB:**
1. Compute velocity direction: v̂ = v / |v|
2. Compute normal: n̂ = (r × v) / |r × v|
3. Compute binormal: b̂ = v̂ × n̂

**Transformation Matrix:**
```
R_ECI_to_VNB = [v̂]
                [n̂]
                [b̂]
```

## Custom Coordinate Systems

### Define Custom Frame

Create application-specific frames:

**Example: Ground Station Frame**
```
Name: KSC_Topocentric
Type: Topocentric
Origin: Kennedy Space Center
  Latitude: 28.5729°N
  Longitude: 80.6490°W
  Altitude: 3 m

Axes:
  X: East
  Y: North
  Z: Up (zenith)
```

**Uses:**
- Ground station tracking
- Azimuth/elevation calculations
- Antenna pointing

### Rotating Frames

Define frames that rotate:

**Example: Earth-Moon Rotating Frame**
```
Name: EME_Rotating
Origin: Earth-Moon barycenter
Primary Axis: Earth-Moon line
Rotation: Synodic period (27.32 days)
```

**Uses:**
- Libration point missions
- Halo orbit analysis
- Three-body problems

## Practical Tips

### Choosing Coordinate Systems

**For Propagation:**
- Use ECI J2000 (standard, simple)
- Switch to TOD if high precision needed

**For Maneuvers:**
- Use VNB (most intuitive)
- Specify delta-V in velocity direction

**For Ground Operations:**
- Use ECEF or geodetic
- Convert from ECI as needed

**For Visualization:**
- 3D View: ECI or ECEF
- Ground Track: Geodetic (lat/lon)

### Common Mistakes

**Mixing Units:**
- Always check km vs. m
- Radians vs. degrees
- Ensure consistency

**Wrong Epoch:**
- Coordinate transformations require time
- ECI to ECEF depends on Earth rotation
- Use correct epoch for transformations

**Singularities:**
- Keplerian elements singular for e=0, i=0
- Use Cartesian or modified elements
- Check for special cases

### Validation

**Check Transformations:**
1. Transform: ECI → ECEF → ECI
2. Verify state unchanged (within tolerance)
3. Check energy conservation

**Compare Tools:**
- Verify against GMAT or STK
- Use multiple methods
- Check limiting cases

## Reference Information

### Constants

**Earth Parameters (WGS-84):**
```
Equatorial Radius (a): 6378.137 km
Polar Radius (b): 6356.752 km
Flattening (f): 1/298.257223563
Eccentricity (e): 0.0818191908
GM (μ): 398600.4418 km³/s²
Rotation Rate (ω): 7.2921159e-5 rad/s
```

**Time Systems:**
- UTC: Coordinated Universal Time
- UT1: Universal Time (Earth rotation)
- TAI: International Atomic Time
- TT: Terrestrial Time
- TDB: Barycentric Dynamical Time

### Useful Conversions

**Angle:**
```
1 degree = π/180 radians
1 radian = 180/π degrees
1 arcminute = 1/60 degree
1 arcsecond = 1/3600 degree
```

**Distance:**
```
1 AU = 149597870.7 km
1 km = 0.621371 miles
1 nautical mile = 1.852 km
```

**Velocity:**
```
1 km/s = 1000 m/s
1 km/s = 3600 km/hr
```

## Additional Resources

- [Vallado, D. "Fundamentals of Astrodynamics and Applications"](https://www.celestrak.com/) - Standard reference
- [IERS Conventions](https://www.iers.org/) - Earth orientation parameters
- [SPICE Toolkit](https://naif.jpl.nasa.gov/naif/) - NASA coordinate system tools

---

**See also:**
- [Orbit Propagation Guide](orbit-propagation.md) - Using coordinate systems in propagation
- [Spacecraft Configuration](spacecraft-configuration.md) - Setting initial state in different frames
- [Maneuver Planning](maneuver-planning.md) - Using VNB and other frames for maneuvers
