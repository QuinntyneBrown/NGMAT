# Force Model Service Requirements

## Overview

**Domain:** Environmental force calculations (gravity, drag, SRP, etc.).

The Force Model Service computes all environmental forces acting on a spacecraft, including gravitational, atmospheric, solar radiation pressure, and relativistic effects.

---

## Requirements

### MS-FM-1: Gravity Model

**Description:** Compute gravitational acceleration.

**Acceptance Criteria:**
- [ ] Point mass gravity (two-body)
- [ ] Spherical harmonics (non-spherical Earth)
- [ ] Degree and order selectable (e.g., 20x20, 70x70)
- [ ] Multiple gravity models (JGM-3, EGM-96, GGM-03)
- [ ] Third-body gravity (Moon, Sun, planets)
- [ ] Acceleration vector output (ax, ay, az)
- [ ] REST API: POST /v1/forcemodel/gravity
- [ ] Configurable per mission

---

### MS-FM-2: Atmospheric Drag

**Description:** Compute atmospheric drag force.

**Acceptance Criteria:**
- [ ] Atmospheric density models (Exponential, Jacchia-Roberts, NRLMSISE-00)
- [ ] Altitude-dependent density
- [ ] Solar flux and geomagnetic index inputs
- [ ] Drag coefficient (Cd) from spacecraft
- [ ] Drag area from spacecraft
- [ ] Velocity relative to atmosphere
- [ ] Acceleration vector output
- [ ] REST API: POST /v1/forcemodel/drag

---

### MS-FM-3: Solar Radiation Pressure

**Description:** Compute SRP force.

**Acceptance Criteria:**
- [ ] Solar flux constant
- [ ] Distance from Sun
- [ ] Shadow function (umbra, penumbra)
- [ ] Reflectivity coefficient from spacecraft
- [ ] SRP area from spacecraft
- [ ] Acceleration vector output
- [ ] REST API: POST /v1/forcemodel/srp

---

### MS-FM-4: Relativistic Effects

**Description:** General relativity corrections (optional, high-precision).

**Acceptance Criteria:**
- [ ] Schwarzschild term
- [ ] Lense-Thirring effect
- [ ] De Sitter precession
- [ ] Small correction (~1e-10 m/s²)
- [ ] Configurable on/off

---

### MS-FM-5: Force Model Configuration

**Description:** Configure which forces are active for a mission.

**Acceptance Criteria:**
- [ ] Enable/disable individual force types
- [ ] Priority/order of force computations
- [ ] Validation of force model compatibility
- [ ] ForceModelConfiguredEvent published
- [ ] REST API: PUT /v1/forcemodel/config/{missionId}

---

### MS-FM-6: Third-Body Gravity

**Description:** Gravitational perturbations from celestial bodies.

**Acceptance Criteria:**
- [ ] Moon gravity
- [ ] Sun gravity
- [ ] Planetary gravity (Venus, Mars, Jupiter, etc.)
- [ ] Body position from Ephemeris Service
- [ ] Configurable bodies
- [ ] Point mass approximation

---

### MS-FM-7: Custom Force Models

**Description:** User-defined force models.

**Acceptance Criteria:**
- [ ] Plugin architecture for custom forces
- [ ] User-supplied acceleration function
- [ ] Input: state vector, epoch
- [ ] Output: acceleration vector
- [ ] Validation and sandboxing

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/forcemodel/gravity | Compute gravity acceleration |
| POST | /v1/forcemodel/drag | Compute drag acceleration |
| POST | /v1/forcemodel/srp | Compute SRP acceleration |
| POST | /v1/forcemodel/total | Compute total acceleration |
| PUT | /v1/forcemodel/config/{missionId} | Configure force model |
| GET | /v1/forcemodel/config/{missionId} | Get force model config |

---

## Gravity Models Supported

| Model | Description | Max Degree/Order |
|-------|-------------|------------------|
| Point Mass | Simple two-body | N/A |
| JGM-3 | Joint Gravity Model 3 | 70x70 |
| EGM-96 | Earth Gravitational Model 1996 | 360x360 |
| GGM-03 | GRACE Gravity Model | 180x180 |

---

## Atmosphere Models Supported

| Model | Description |
|-------|-------------|
| Exponential | Simple altitude-based |
| Jacchia-Roberts | Temperature-based model |
| NRLMSISE-00 | High-fidelity empirical model |

---

## Dependencies

- **Ephemeris Service** - Celestial body positions, solar flux
- **Spacecraft Service** - Spacecraft properties (Cd, area, mass)
- **Calculation Engine Service** - Spherical harmonics
