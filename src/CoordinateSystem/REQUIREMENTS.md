# Coordinate System Service Requirements

## Overview

**Domain:** Reference frame definitions and transformations.

The Coordinate System Service handles all coordinate frame definitions, transformations between frames, and conversions between different state representations (Cartesian, Keplerian, geodetic).

---

## Requirements

### MS-CS-1: Define Coordinate System

**Description:** Create coordinate system definition.

**Acceptance Criteria:**
- [ ] System name (e.g., ECI J2000, ECEF, Moon-Centered)
- [ ] Central body
- [ ] Axes definition
- [ ] Origin definition
- [ ] Epoch (for inertial systems)
- [ ] CoordinateSystemCreatedEvent published
- [ ] REST API: POST /v1/coordinates/systems
- [ ] Returns HTTP 201

---

### MS-CS-2: Transform State Vector

**Description:** Convert state vector between coordinate systems.

**Acceptance Criteria:**
- [ ] Source coordinate system ID
- [ ] Target coordinate system ID
- [ ] Input state vector (position, velocity)
- [ ] Epoch required
- [ ] Output state vector
- [ ] Transformation matrix logged for audit
- [ ] REST API: POST /v1/coordinates/transform
- [ ] Returns HTTP 200 with transformed state

---

### MS-CS-3: ECI to ECEF Transformation

**Description:** Inertial to Earth-Fixed transformation.

**Acceptance Criteria:**
- [ ] Epoch required (for Earth rotation)
- [ ] Precession and nutation applied
- [ ] Polar motion (IERS data)
- [ ] UT1-UTC correction
- [ ] Accurate to < 1 meter
- [ ] REST API: POST /v1/coordinates/eci-to-ecef

---

### MS-CS-4: ECEF to Geodetic

**Description:** Cartesian to Latitude/Longitude/Altitude.

**Acceptance Criteria:**
- [ ] Input: ECEF (x, y, z)
- [ ] Output: Latitude (deg), Longitude (deg), Altitude (m)
- [ ] Ellipsoid model (WGS-84)
- [ ] Iterative algorithm for altitude
- [ ] REST API: POST /v1/coordinates/ecef-to-geodetic

---

### MS-CS-5: Geodetic to ECEF

**Description:** Lat/Lon/Alt to Cartesian.

**Acceptance Criteria:**
- [ ] Input: Latitude, Longitude, Altitude
- [ ] Output: ECEF (x, y, z)
- [ ] WGS-84 ellipsoid
- [ ] REST API: POST /v1/coordinates/geodetic-to-ecef

---

### MS-CS-6: Body-Fixed Frames

**Description:** Spacecraft-centered coordinate systems.

**Acceptance Criteria:**
- [ ] VNB (Velocity-Normal-Binormal)
- [ ] LVLH (Local Vertical Local Horizontal)
- [ ] RSW (Radial-Transverse-Normal)
- [ ] Transformation from inertial frame
- [ ] Used for maneuver planning

---

### MS-CS-7: Keplerian Elements Conversion

**Description:** Convert state vector to/from Keplerian elements.

**Acceptance Criteria:**
- [ ] State vector → (a, e, i, Ω, ω, ν)
- [ ] Keplerian elements → State vector
- [ ] Handle circular/equatorial orbits
- [ ] Handle parabolic/hyperbolic orbits
- [ ] REST API: POST /v1/coordinates/state-to-keplerian
- [ ] REST API: POST /v1/coordinates/keplerian-to-state

---

### MS-CS-8: Mean Orbital Elements

**Description:** Compute mean (osculating-removed perturbations).

**Acceptance Criteria:**
- [ ] Input: Osculating elements
- [ ] Output: Mean elements
- [ ] Useful for TLE generation
- [ ] J2 perturbation removal

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/coordinates/systems | Create coordinate system |
| GET | /v1/coordinates/systems | List coordinate systems |
| POST | /v1/coordinates/transform | Transform state vector |
| POST | /v1/coordinates/eci-to-ecef | ECI to ECEF conversion |
| POST | /v1/coordinates/ecef-to-geodetic | ECEF to geodetic |
| POST | /v1/coordinates/geodetic-to-ecef | Geodetic to ECEF |
| POST | /v1/coordinates/state-to-keplerian | State to Keplerian |
| POST | /v1/coordinates/keplerian-to-state | Keplerian to state |

---

## Coordinate Systems

| System | Description | Central Body |
|--------|-------------|--------------|
| ECI J2000 | Earth-Centered Inertial | Earth |
| ECEF | Earth-Centered Earth-Fixed | Earth |
| MCI | Moon-Centered Inertial | Moon |
| HCI | Heliocentric Inertial | Sun |
| VNB | Velocity-Normal-Binormal | Spacecraft |
| LVLH | Local Vertical Local Horizontal | Spacecraft |

---

## Dependencies

- **Calculation Engine Service** - Matrix operations, quaternions
- **Ephemeris Service** - Earth orientation parameters
