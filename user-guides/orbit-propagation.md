# Orbit Propagation Guide

Learn how to propagate spacecraft orbits, configure force models, and analyze trajectory data in NGMAT.

## Table of Contents

- [Propagation Basics](#propagation-basics)
- [Numerical Integrators](#numerical-integrators)
- [Force Models](#force-models)
- [Running Propagation](#running-propagation)
- [Analyzing Results](#analyzing-results)
- [Event Detection](#event-detection)
- [Advanced Propagation](#advanced-propagation)
- [Performance Optimization](#performance-optimization)

## Propagation Basics

### What is Orbit Propagation?

**Orbit propagation** is the process of calculating a spacecraft's position and velocity over time given:
- Initial state (position and velocity)
- Force models (gravity, drag, SRP, etc.)
- Time span
- Numerical integration method

### Why Propagate?

Propagation allows you to:
- Predict future spacecraft positions
- Analyze orbital evolution over time
- Plan ground station contacts
- Assess mission feasibility
- Generate trajectory data for analysis

### Propagation Methods

**Numerical Integration** (Used by NGMAT)
- Solves equations of motion step-by-step
- High accuracy for any orbit
- Accounts for all perturbations
- Computationally intensive

**Analytical Propagation** (Two-Body)
- Closed-form Keplerian solution
- Very fast computation
- No perturbations
- Less accurate for long durations

## Numerical Integrators

### Available Integrators

NGMAT supports multiple numerical integration methods:

| Integrator | Order | Adaptive Step | Use Case |
|------------|-------|---------------|----------|
| **RK4** | 4th | No | General purpose, educational |
| **RK45** | 4-5th | Yes | **Recommended** - Good balance |
| **RK89** | 8-9th | Yes | High precision, slow |
| **Adams-Bashforth** | Variable | Yes | Multi-step, efficient |

### Runge-Kutta 4 (RK4)

**Characteristics:**
- 4th order accuracy
- Fixed step size
- Simple and robust

**Best for:**
- Short propagations
- Teaching and learning
- When simplicity is desired

**Configuration:**
```
Propagator: RK4
Step Size: 60 seconds (for LEO)
```

### Runge-Kutta-Fehlberg 4-5 (RK45)

**Characteristics:**
- 4th/5th order adaptive
- Variable step size
- Error control
- **Recommended for most missions**

**Best for:**
- General mission analysis
- LEO to GEO orbits
- Balance of speed and accuracy

**Configuration:**
```
Propagator: RK45
Initial Step: 60 seconds
Min Step: 0.001 seconds
Max Step: 300 seconds
Tolerance: 1e-10
```

### Runge-Kutta 8-9 (RK89)

**Characteristics:**
- 8th/9th order adaptive
- Very high precision
- Slower computation

**Best for:**
- High-precision requirements
- Long-duration missions
- Orbit determination
- Publications

**Configuration:**
```
Propagator: RK89
Initial Step: 60 seconds
Min Step: 0.0001 seconds
Max Step: 300 seconds
Tolerance: 1e-13
```

### Adams-Bashforth-Moulton

**Characteristics:**
- Multi-step method
- Variable order
- Efficient for smooth problems

**Best for:**
- Long propagations
- Interplanetary missions
- When computation time matters

**Configuration:**
```
Propagator: Adams-Bashforth
Order: 12
Initial Step: 60 seconds
Tolerance: 1e-11
```

### Choosing an Integrator

**Use RK45 when:**
- You need good accuracy
- Propagation time is moderate
- Standard mission analysis

**Use RK89 when:**
- Maximum accuracy required
- Verification of results
- Long-duration, high-precision needs

**Use RK4 when:**
- Learning orbital mechanics
- Quick estimates
- Simple demonstrations

**Use Adams when:**
- Very long propagations
- Computational efficiency critical
- Smooth force models

## Force Models

### Gravity Models

**Point Mass Gravity** (Two-Body)
- Simple 1/r² law
- Fast computation
- Ignores Earth's non-spherical shape

**Spherical Harmonics**
- Models Earth's actual shape
- Degree and order selection
- Higher degree = more accuracy = slower

**Recommended Settings:**
```
LEO missions: 20x20 or 30x30
GEO missions: 10x10
Interplanetary: Point mass or 4x4
```

**Available Gravity Models:**
- JGM-3 (older, historical)
- EGM-96 (commonly used)
- GGM-03 (high resolution)

### Atmospheric Drag

**When to Include:**
- Altitude < 1000 km
- Required for LEO missions
- Optional for MEO/GEO

**Atmospheric Density Models:**

**Exponential Model**
- Simple, fast
- Less accurate
- Good for quick estimates

**Jacchia-Roberts**
- Medium complexity
- Solar flux dependent
- Good accuracy

**NRLMSISE-00**
- Most accurate
- Complex computation
- **Recommended** for accurate LEO

**Configuration:**
```
Drag Model: NRLMSISE-00
Solar Flux (F10.7): 150 (typical)
Geomagnetic Index (Ap): 15 (quiet)
```

### Solar Radiation Pressure (SRP)

**When to Include:**
- Altitude > 500 km
- GEO missions (critical)
- High area-to-mass ratio spacecraft

**Configuration:**
```
SRP: Enabled
Solar Flux: 1367 W/m² (constant)
Shadow Model: Cylindrical or Conical
```

**Shadow Models:**
- **Cylindrical** - Faster, less accurate
- **Conical** - Accounts for penumbra, **recommended**

### Third-Body Perturbations

**Moon Gravity**
- Significant for high orbits
- Critical for lunar missions
- Include for altitude > 50,000 km

**Sun Gravity**
- Important for GEO
- Include for long-duration missions

**Planetary Gravity**
- Jupiter, Mars, Venus, etc.
- Only for interplanetary missions

### Relativistic Effects

**General Relativity Corrections**
- Very small effect (~1e-10 m/s²)
- Optional for high precision
- Turn off unless publishing results

## Running Propagation

### Basic Propagation

1. **Open Mission**
   - Select mission with configured spacecraft

2. **Click "Propagate"**
   - Opens propagation configuration dialog

3. **Set Time Span**
   ```
   Start Epoch: Mission start (or current state epoch)
   End Epoch: 2026-01-19 12:00:00 UTC (example)
   Duration: 1 day
   ```

4. **Choose Integrator**
   ```
   Propagator: RK45 (recommended)
   Initial Step Size: 60 seconds
   Tolerance: 1e-10
   ```

5. **Configure Force Models**
   ```
   ☑ Gravity (20x20 EGM-96)
   ☑ Atmospheric Drag (NRLMSISE-00)
   ☑ Solar Radiation Pressure
   ☑ Moon Gravity (if GEO or higher)
   ☐ Sun Gravity (optional)
   ☐ Relativity (optional)
   ```

6. **Set Output Options**
   ```
   Output Interval: 60 seconds
   Output Format: State vectors
   Save to: Results database
   ```

7. **Run**
   - Click "Run Propagation"
   - Progress bar shows status
   - View logs for details

### Propagation Settings

**Step Size Selection:**

| Orbit Type | Initial Step | Min Step | Max Step |
|------------|--------------|----------|----------|
| LEO | 60 s | 0.01 s | 120 s |
| MEO | 120 s | 0.1 s | 300 s |
| GEO | 300 s | 1 s | 600 s |
| HEO | 60 s | 0.01 s | 600 s |
| Lunar | 60 s | 0.001 s | 300 s |

**Tolerance Selection:**

| Accuracy Need | Tolerance |
|---------------|-----------|
| Quick estimate | 1e-8 |
| Standard | 1e-10 |
| High precision | 1e-12 |
| Publication | 1e-13 |

### Monitoring Propagation

**Progress Indicators:**
- Percentage complete
- Current epoch
- Estimated time remaining
- Integration steps taken
- Step size adjustments

**Pause/Resume:**
- Click "Pause" to temporarily stop
- Resume continues from current state

**Cancel:**
- Click "Cancel" to abort
- Partial results may be saved

## Analyzing Results

### State Vector Table

View propagated states in tabular format:

**Columns:**
- Epoch (date/time)
- Position X, Y, Z (km)
- Velocity vX, vY, vZ (km/s)
- Altitude (km)
- Latitude, Longitude (deg)
- Velocity magnitude (km/s)

**Features:**
- Sort by any column
- Filter by epoch range
- Export to CSV/JSON
- Copy selected rows

### Orbital Elements

Convert state vectors to Keplerian elements:

**Elements Computed:**
- Semi-major axis (a)
- Eccentricity (e)
- Inclination (i)
- RAAN (Ω)
- Argument of Perigee (ω)
- True Anomaly (ν)

**Additional Parameters:**
- Apoapsis altitude
- Periapsis altitude
- Period
- Mean motion

### Visualization

**3D Orbit View:**
- See [Visualization Guide](visualization.md#3d-orbit-view)

**Ground Track:**
- See [Visualization Guide](visualization.md#ground-track)

**Time-Series Plots:**
- See [Visualization Guide](visualization.md#time-series-plots)

## Event Detection

### Available Events

**Apoapsis/Periapsis Detection**
- Finds local extrema in radial distance
- Useful for analyzing orbit shape

**Node Crossings**
- Ascending node (southbound to northbound)
- Descending node (northbound to southbound)

**Altitude Threshold**
- Detects crossing of specified altitude
- Useful for atmospheric entry/exit

**Eclipse Entry/Exit**
- Umbra entry/exit (full shadow)
- Penumbra entry/exit (partial shadow)
- Solar panel visibility

**Ground Station Contact**
- Visibility from ground location
- Elevation above horizon
- Range to station

**Latitude/Longitude Crossing**
- Crossing specified latitude
- Crossing specified longitude

### Configure Event Detection

1. Click **"Events"** tab in propagation settings
2. Select events to detect:
   ```
   ☑ Apoapsis/Periapsis
   ☑ Eclipse
   ☐ Altitude Threshold
   ☐ Ground Station Contact
   ```
3. Configure event parameters:
   ```
   Altitude Threshold: 300 km
   Ground Station: Lat 28.5°, Lon -80.6° (KSC)
   Minimum Elevation: 10°
   ```
4. Run propagation

### View Event Results

After propagation:
1. Click **"Events"** tab in results
2. See table of detected events:
   ```
   Event Type | Epoch | Details
   Apoapsis | 2026-01-18 12:45:23 | 450.2 km
   Eclipse Entry | 2026-01-18 13:12:45 | Umbra
   Eclipse Exit | 2026-01-18 13:45:12 | Umbra
   ```
3. Export events to CSV if needed

## Advanced Propagation

### State Transition Matrix (STM)

**What is STM?**
- 6x6 matrix of partial derivatives
- ∂(final state) / ∂(initial state)
- Used for orbit determination and covariance

**Enable STM:**
1. Click **"Advanced"** in propagation settings
2. Check ☑ **"Compute State Transition Matrix"**
3. Note: Adds computational overhead

**Use Cases:**
- Orbit determination
- Covariance propagation
- Sensitivity analysis

### Parallel Propagation

Propagate multiple spacecraft simultaneously:

1. Select multiple spacecraft
2. Click **"Propagate Selected"**
3. Each spacecraft propagates in parallel
4. Results saved independently

**Benefits:**
- Faster than sequential
- Compare trajectories easily
- Analyze constellations

### Two-Body Analytical Propagation

Fast analytical solution (no perturbations):

1. Click **"Quick Propagate"**
2. Select **"Two-Body Analytical"**
3. Set time span
4. Click **"Run"**

**When to Use:**
- Quick orbit estimates
- Teaching demonstrations
- Short time spans
- Comparing with perturbed propagation

**Limitations:**
- No perturbations (drag, J2, etc.)
- Less accurate for long durations
- No event detection

## Performance Optimization

### Speed Up Propagation

**1. Choose Appropriate Integrator**
- RK4 or RK45 for most missions
- Avoid RK89 unless necessary

**2. Optimize Step Size**
- Larger steps = faster (but less accurate)
- Balance accuracy vs. speed
- Use adaptive methods (RK45)

**3. Reduce Force Model Complexity**
- Lower gravity degree/order
- Simpler drag model
- Disable unnecessary perturbations

**4. Limit Output Frequency**
- Output every 60s instead of every step
- Reduces data storage
- Faster post-processing

**5. Use Appropriate Tolerance**
- Don't over-specify accuracy
- 1e-10 is usually sufficient

### Accuracy vs. Speed Trade-offs

**Maximum Speed (less accurate):**
```
Propagator: RK4
Step Size: 120 seconds
Gravity: 4x4
Drag: Exponential
SRP: Off
Output: Every 300 seconds
```

**Balanced (recommended):**
```
Propagator: RK45
Initial Step: 60 seconds
Gravity: 20x20
Drag: NRLMSISE-00
SRP: On
Output: Every 60 seconds
Tolerance: 1e-10
```

**Maximum Accuracy (slower):**
```
Propagator: RK89
Initial Step: 30 seconds
Gravity: 70x70
Drag: NRLMSISE-00
SRP: On (Conical shadow)
Moon/Sun: On
Relativity: On
Output: Every 10 seconds
Tolerance: 1e-13
```

## Troubleshooting

**Issue: Propagation fails or crashes**
- Check initial state validity
- Reduce step size
- Increase tolerance
- Simplify force models

**Issue: Results inaccurate**
- Use tighter tolerance
- Smaller step size
- Higher-order integrator
- Check force model configuration

**Issue: Propagation too slow**
- Use simpler integrator (RK45 instead of RK89)
- Increase step size (carefully)
- Reduce gravity degree/order
- Simplify drag model

**Issue: Step size becoming very small**
- May indicate stiff problem
- Check for unrealistic forces
- Verify spacecraft properties
- Try different integrator

---

**Next:** [Maneuver Planning Guide](maneuver-planning.md) - Learn to plan and optimize orbital maneuvers.
