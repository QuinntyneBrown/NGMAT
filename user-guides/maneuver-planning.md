# Maneuver Planning Guide

Learn how to plan impulsive burns, finite burns, orbital transfers, and optimize maneuvers in NGMAT.

## Table of Contents

- [Maneuver Basics](#maneuver-basics)
- [Impulsive Burns](#impulsive-burns)
- [Finite Burns](#finite-burns)
- [Orbital Transfers](#orbital-transfers)
- [Maneuver Optimization](#maneuver-optimization)
- [Delta-V Budget](#delta-v-budget)
- [Specialized Maneuvers](#specialized-maneuvers)

## Maneuver Basics

### What is a Maneuver?

A **maneuver** is a planned change in spacecraft velocity using propulsion systems. Maneuvers are used to:
- Change orbital altitude
- Adjust orbital plane (inclination)
- Rendezvous with other spacecraft
- Deorbit or escape orbits
- Station-keeping

### Maneuver Types

**Impulsive Burn**
- Instantaneous velocity change
- Idealization of short burns
- Used for analysis and planning

**Finite Burn**
- Continuous thrust over time
- More realistic
- Used for actual operations

### Planning Workflow

1. Define maneuver objectives
2. Calculate required delta-V
3. Choose maneuver type
4. Set maneuver parameters
5. Verify fuel availability
6. Propagate before/after
7. Analyze results

## Impulsive Burns

### Create Impulsive Burn

1. Open mission with spacecraft
2. Click **"+ Add Maneuver"**
3. Select **"Impulsive Burn"**
4. Configure parameters
5. Click **"Create"**

### Impulsive Burn Parameters

**Burn Epoch** (Required)
- Date and time of burn
- Format: YYYY-MM-DD HH:MM:SS UTC
- Example: `2026-01-18 14:30:00`

**Delta-V Vector** (Required)
- Three components: ΔvX, ΔvY, ΔvZ
- Units: m/s or km/s
- Magnitude: √(ΔvX² + ΔvY² + ΔvZ²)

**Coordinate System**
- Frame for delta-V specification
- Options:
  - **VNB** - Velocity-Normal-Binormal (recommended)
  - **LVLH** - Local Vertical Local Horizontal
  - **RSW** - Radial-Transverse-Normal
  - **ECI** - Earth-Centered Inertial

### VNB Coordinate System

Most intuitive for orbital maneuvers:

**V (Velocity)** - Along velocity vector
- Positive V: Speed up (raise apoapsis)
- Negative V: Slow down (lower periapsis)

**N (Normal)** - Perpendicular to orbital plane
- Positive N: Increase inclination
- Negative N: Decrease inclination

**B (Binormal)** - Completes right-hand system
- In-plane component perpendicular to V

**Example: Apoapsis Raise**
```
Burn Epoch: 2026-01-18 14:30:00
Delta-V:
  V = +100 m/s (speed up at periapsis)
  N = 0 m/s
  B = 0 m/s
Coordinate System: VNB
```

### Common Impulsive Maneuvers

**Circularization**
```
Burn at apoapsis:
  V = +Δv (to raise periapsis)
  N = 0
  B = 0
```

**Apoapsis Raise**
```
Burn at periapsis:
  V = +Δv
  N = 0
  B = 0
```

**Periapsis Raise**
```
Burn at apoapsis:
  V = +Δv
  N = 0
  B = 0
```

**Plane Change**
```
Burn at ascending or descending node:
  V = 0 (or small)
  N = ±Δv
  B = 0
```

## Finite Burns

### Create Finite Burn

1. Click **"+ Add Maneuver"**
2. Select **"Finite Burn"**
3. Configure parameters
4. Click **"Create"**

### Finite Burn Parameters

**Start Epoch** (Required)
- Burn start time
- Example: `2026-01-18 14:30:00`

**Duration** (Required)
- Burn duration in seconds
- Typical: 30-3600 seconds
- Depends on thrust and delta-V

**Thrust Magnitude** (Required)
- Force produced
- Units: Newtons (N)
- From thruster specification

**Thrust Direction** (Required)
- Unit vector (X, Y, Z)
- Normalized: √(X² + Y² + Z²) = 1
- Coordinate system specified

**Mass Flow Rate** (Optional/Calculated)
- Propellant consumption rate
- Calculated from thrust and Isp:
  ```
  ṁ = Thrust / (Isp × g₀)
  ```

### Finite Burn Example

**GEO Orbit Raise:**
```
Start Epoch: 2026-01-18 14:30:00
Duration: 600 seconds (10 minutes)
Thrust: 500 N
Direction:
  V = 1.0 (along velocity)
  N = 0.0
  B = 0.0
Specific Impulse: 320 s

Fuel Consumed:
  ṁ = 500 / (320 × 9.80665) = 0.159 kg/s
  Total = 0.159 × 600 = 95.4 kg
```

## Orbital Transfers

### Hohmann Transfer

Most fuel-efficient two-impulse transfer between circular orbits.

**Use Case:**
- Transfer between circular orbits
- Minimizes delta-V
- Standard for orbit raising

**Create Hohmann Transfer:**

1. Click **"Maneuvers"** → **"Hohmann Transfer"**
2. Enter:
   ```
   Initial Orbit Radius: 6778 km (400 km alt)
   Target Orbit Radius: 42164 km (GEO)
   ```
3. NGMAT calculates:
   - Burn 1 delta-V at initial periapsis
   - Burn 2 delta-V at transfer apoapsis
   - Transfer time
   - Total delta-V

4. Click **"Create Maneuvers"**

**Example: LEO to GEO**
```
Initial: 6778 km radius (400 km altitude)
Target: 42164 km radius (35786 km altitude)

Results:
  Burn 1 (at LEO): +2441 m/s
  Burn 2 (at GEO): +1480 m/s
  Total Δv: 3921 m/s
  Transfer time: 5.26 hours
```

### Bi-Elliptic Transfer

Three-impulse transfer, more efficient for large radius changes (>15x).

**Use Case:**
- Very large orbit changes
- When fuel is abundant but time is not critical

**Create Bi-Elliptic Transfer:**

1. Click **"Maneuvers"** → **"Bi-Elliptic Transfer"**
2. Enter:
   ```
   Initial Radius: 6778 km
   Final Radius: 67780 km
   Intermediate Apoapsis: 100000 km
   ```
3. NGMAT computes three burns

**Example:**
```
Initial: 6778 km
Final: 67780 km
Intermediate: 100000 km

Burn 1: +2959 m/s (raise to intermediate)
Burn 2: +678 m/s (plane change at apogee)
Burn 3: +368 m/s (circularize)
Total: 4005 m/s

Compare to Hohmann: 4125 m/s
Savings: 120 m/s (3%)
But takes longer: ~15 orbits vs. ~5 hours
```

### Plane Change Maneuver

Change orbital inclination.

**Δv for Pure Plane Change:**
```
Δv = 2 × v × sin(Δi / 2)

Where:
  v = orbital velocity
  Δi = inclination change (radians)
```

**Example:**
```
Current: i = 28.5° (Cape Canaveral)
Target: i = 51.6° (ISS)
Δi = 23.1°

At LEO (v = 7.7 km/s):
Δv = 2 × 7.7 × sin(23.1°/2)
Δv ≈ 3.0 km/s (very expensive!)
```

**Tip:** Combine plane changes with other maneuvers to save fuel.

### Combined Maneuvers

**Inclination Change + Apoapsis Raise:**
```
Burn at periapsis:
  V = Δv_tangential
  N = Δv_normal
  B = 0

More efficient than separate burns!
```

## Maneuver Optimization

### Optimize Single Maneuver

Find optimal burn parameters to achieve target:

1. Click **"Optimize Maneuver"**
2. Define target:
   ```
   Target Type: Orbital Elements
   Target Apoapsis: 42164 km
   Target Periapsis: 42164 km (circular)
   ```
3. Select variables to optimize:
   ```
   ☑ Burn Epoch
   ☑ Delta-V Magnitude
   ☑ Delta-V Direction
   ```
4. Set constraints:
   ```
   Max Delta-V: 5000 m/s
   Min Altitude: 200 km
   Max Fuel: 100 kg
   ```
5. Choose algorithm:
   - **SQP** (Sequential Quadratic Programming) - Fast, gradient-based
   - **Genetic Algorithm** - Global search, slower
6. Click **"Optimize"**

### Multi-Maneuver Optimization

Optimize entire maneuver sequence:

**Example: Rendezvous Optimization**
```
Objective: Minimize total delta-V
Target: ISS orbit at specified epoch

Variables:
  - Burn epochs (3 burns)
  - Delta-V magnitudes
  - Delta-V directions

Constraints:
  - Arrive at ISS ± 1 hour
  - Relative velocity < 0.5 m/s
  - No collision risk
  - Total fuel < 150 kg

Algorithm: SQP with multiple starting points
```

### Optimization Algorithms

**Sequential Quadratic Programming (SQP)**
- Fast convergence
- Requires smooth problem
- May find local minimum
- Best for: Well-posed problems, good initial guess

**Genetic Algorithm**
- Global optimization
- Handles discontinuities
- Slower computation
- Best for: Complex problems, multiple minima

**Particle Swarm Optimization**
- Balance of speed and global search
- Good for moderate complexity
- Best for: Medium-sized problems

## Delta-V Budget

### Calculate Delta-V Budget

View total delta-V for all maneuvers:

1. Open mission
2. Click **"Delta-V Budget"**
3. See table:
   ```
   Maneuver | Epoch | Delta-V | Cumulative
   Burn 1 | ... | 2441 m/s | 2441 m/s
   Burn 2 | ... | 1480 m/s | 3921 m/s
   Total: 3921 m/s
   ```

### Fuel Requirements

Calculate fuel needed for delta-V:

**Tsiolkovsky Rocket Equation:**
```
Δv = Isp × g₀ × ln(m₀ / mf)

Rearranged for fuel mass:
m_fuel = m_dry × (exp(Δv / (Isp × g₀)) - 1)

Where:
  Isp = Specific impulse (s)
  g₀ = 9.80665 m/s²
  m₀ = Initial mass
  mf = Final mass
  m_dry = Dry mass
```

**Example:**
```
Given:
  Dry mass = 500 kg
  Isp = 320 s
  Total Δv = 3921 m/s

Calculate fuel:
  m_fuel = 500 × (exp(3921 / (320 × 9.80665)) - 1)
  m_fuel = 500 × (exp(1.249) - 1)
  m_fuel = 500 × (3.487 - 1)
  m_fuel = 500 × 2.487
  m_fuel = 1243.5 kg

Required total mass = 1743.5 kg
```

### Budget Planning

**Reserve Delta-V:**
- Add 10-20% margin for uncertainties
- Account for attitude control
- Include station-keeping budget
- Plan for contingencies

**Example Mission Budget:**
```
Orbit Injection: 2441 m/s
Circularization: 1480 m/s
Plane Change: 100 m/s
Station-keeping (1 year): 50 m/s
Deorbit: 100 m/s
Contingency (15%): 625 m/s
------------------------
Total: 4796 m/s
```

## Specialized Maneuvers

### Rendezvous

Approach and dock with target spacecraft:

**Phases:**
1. **Phasing** - Adjust orbit to catch up
2. **Coelliptic** - Match orbit size
3. **Close Approach** - Final approach
4. **Docking** - Station-keeping and contact

**NGMAT Rendezvous Planner:**
1. Click **"Rendezvous Planner"**
2. Select target spacecraft
3. Specify arrival time
4. Set safety constraints
5. Generate maneuver sequence

### Station-Keeping

Maintain spacecraft in target orbit:

**GEO Station-Keeping:**
- North-South station-keeping (inclination control)
- East-West station-keeping (longitude control)
- Typical budget: 50 m/s per year

**LEO Station-Keeping:**
- Altitude maintenance (combat drag)
- Typical budget: varies with solar activity

**Configure Station-Keeping:**
1. Click **"Station-Keeping"**
2. Set target orbit elements
3. Define tolerance bounds:
   ```
   Apoapsis: 42164 ± 10 km
   Periapsis: 42164 ± 10 km
   Inclination: 0 ± 0.1°
   ```
4. Schedule periodic checks

### Deorbit

Plan safe disposal:

**Controlled Deorbit:**
1. Calculate delta-V to lower periapsis below 100 km
2. Plan burn at apoapsis
3. Predict reentry location

**Example: ISS Deorbit**
```
Initial orbit: 400 km circular
Target periapsis: 80 km

Burn at apoapsis:
  Δv ≈ 120 m/s retrograde
  
Reentry prediction:
  Time from burn: ~45 minutes
  Location: controlled over ocean
```

## Troubleshooting

**Issue: Maneuver results unexpected**
- Verify coordinate system selection
- Check delta-V signs (+ vs -)
- Ensure burn epoch is correct

**Issue: Insufficient fuel**
- Reduce delta-V requirements
- Optimize maneuver sequence
- Consider mission redesign

**Issue: Optimization not converging**
- Provide better initial guess
- Relax constraints
- Try different algorithm
- Break into smaller problems

---

**Next:** [Visualization Guide](visualization.md) - Learn to create visualizations and analyze mission data.
