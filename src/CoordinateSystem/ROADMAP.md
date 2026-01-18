# Coordinate System Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Coordinate System Service, which provides reference frame definitions and transformations for NGMAT.

---

## Phase 1: Core Frame Definitions

**Goal:** Basic coordinate system management.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up database for coordinate system definitions
- [ ] Configure dependency injection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Coordinate System CRUD (MS-CS-1)
- [ ] Create CoordinateSystem entity
- [ ] Define built-in systems (ECI J2000, ECEF, etc.)
- [ ] Implement CRUD endpoints
- [ ] Publish CoordinateSystemCreatedEvent

**Deliverables:**
- Coordinate system management API
- Pre-defined standard frames

---

## Phase 2: Basic Transformations

**Goal:** Cartesian and geodetic conversions.

### Milestone 2.1: ECEF-Geodetic (MS-CS-4, MS-CS-5)
- [ ] Implement ECEF to geodetic conversion
- [ ] Implement geodetic to ECEF conversion
- [ ] Support WGS-84 ellipsoid
- [ ] Create API endpoints

### Milestone 2.2: Generic Transform (MS-CS-2)
- [ ] Implement transformation framework
- [ ] Support position and velocity transformation
- [ ] Log transformation matrices for audit
- [ ] Create transform endpoint

**Deliverables:**
- ECEF/geodetic conversions
- General transformation framework

---

## Phase 3: Inertial Transformations

**Goal:** High-precision ECI-ECEF transformations.

### Milestone 3.1: ECI-ECEF (MS-CS-3)
- [ ] Implement precession model (IAU 2006)
- [ ] Implement nutation model
- [ ] Add polar motion corrections
- [ ] Add UT1-UTC correction
- [ ] Validate accuracy < 1 meter

**Deliverables:**
- High-precision frame transformations
- IERS data integration

---

## Phase 4: Orbital Elements

**Goal:** State vector and Keplerian element conversions.

### Milestone 4.1: Keplerian Conversions (MS-CS-7)
- [ ] Implement state to Keplerian conversion
- [ ] Implement Keplerian to state conversion
- [ ] Handle edge cases (circular, equatorial, hyperbolic)
- [ ] Create API endpoints

### Milestone 4.2: Mean Elements (MS-CS-8)
- [ ] Implement J2 secular perturbation removal
- [ ] Compute mean orbital elements
- [ ] Support TLE-compatible elements

**Deliverables:**
- Complete orbital element conversions
- Mean element computation

---

## Phase 5: Body-Fixed Frames

**Goal:** Spacecraft-relative coordinate systems.

### Milestone 5.1: Spacecraft Frames (MS-CS-6)
- [ ] Implement VNB frame
- [ ] Implement LVLH frame
- [ ] Implement RSW frame
- [ ] Support dynamic frame computation

**Deliverables:**
- Spacecraft-centered frames
- Maneuver planning support

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Frame Definitions | P0 - Critical |
| Phase 2 | Basic Transformations | P0 - Critical |
| Phase 3 | Inertial Transformations | P0 - Critical |
| Phase 4 | Orbital Elements | P1 - High |
| Phase 5 | Body-Fixed Frames | P1 - High |

---

## Technical Dependencies

- **Calculation Engine Service** - Matrix operations
- **Ephemeris Service** - Earth orientation parameters

---

## Success Metrics

- [ ] All 8 requirements (MS-CS-1 through MS-CS-8) implemented
- [ ] ECI-ECEF accuracy < 1 meter
- [ ] Keplerian conversions validated against GMAT
- [ ] 85%+ unit test coverage
