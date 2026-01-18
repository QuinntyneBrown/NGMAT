# Maneuver Service Requirements

## Overview

**Domain:** Maneuver planning, execution, and optimization.

The Maneuver Service handles orbital maneuver planning including impulsive burns, finite burns, transfer orbits, and rendezvous planning.

---

## Requirements

### MS-MN-1: Impulsive Burn

**Description:** Instantaneous velocity change (delta-V).

**Acceptance Criteria:**
- [ ] Burn epoch required
- [ ] Delta-V vector (Vx, Vy, Vz) required
- [ ] Coordinate system specified (VNB, LVLH, inertial)
- [ ] Fuel consumption calculated (Tsiolkovsky equation)
- [ ] Spacecraft fuel updated
- [ ] State vector discontinuity at burn epoch
- [ ] ManeuverCreatedEvent published
- [ ] REST API: POST /v1/maneuvers/impulsive
- [ ] Returns HTTP 201

---

### MS-MN-2: Finite Burn

**Description:** Continuous thrust over duration.

**Acceptance Criteria:**
- [ ] Burn start epoch required
- [ ] Burn duration required
- [ ] Thrust magnitude (Newtons) required
- [ ] Thrust direction (unit vector) required
- [ ] Mass flow rate calculated from Isp
- [ ] Fuel consumption integrated over duration
- [ ] State propagated during burn
- [ ] ManeuverCreatedEvent published
- [ ] REST API: POST /v1/maneuvers/finite
- [ ] Returns HTTP 201

---

### MS-MN-3: Maneuver Optimization

**Description:** Optimize maneuver parameters to achieve target.

**Acceptance Criteria:**
- [ ] Target orbit elements specified
- [ ] Cost function: minimize delta-V, time, fuel
- [ ] Constraints: fuel limit, time window
- [ ] Optimization algorithm (gradient descent, genetic algorithm)
- [ ] Initial guess provided or auto-generated
- [ ] Convergence tolerance
- [ ] Optimal maneuver parameters returned
- [ ] ManeuverOptimizedEvent published
- [ ] REST API: POST /v1/maneuvers/optimize
- [ ] Returns HTTP 202 (async)

---

### MS-MN-4: Hohmann Transfer

**Description:** Compute Hohmann transfer between circular orbits.

**Acceptance Criteria:**
- [ ] Initial orbit radius required
- [ ] Final orbit radius required
- [ ] Two-impulse maneuver
- [ ] Delta-V for each burn calculated
- [ ] Transfer time calculated
- [ ] Total delta-V returned
- [ ] REST API: POST /v1/maneuvers/hohmann

---

### MS-MN-5: Bi-Elliptic Transfer

**Description:** Three-impulse transfer (more efficient for large radius changes).

**Acceptance Criteria:**
- [ ] Initial orbit radius
- [ ] Final orbit radius
- [ ] Intermediate apoapsis radius
- [ ] Three burns calculated
- [ ] Total delta-V compared to Hohmann
- [ ] Transfer time calculated
- [ ] REST API: POST /v1/maneuvers/bi-elliptic

---

### MS-MN-6: Plane Change Maneuver

**Description:** Change orbital inclination.

**Acceptance Criteria:**
- [ ] Target inclination required
- [ ] Burn at ascending/descending node
- [ ] Delta-V calculated
- [ ] Combined with Hohmann if desired
- [ ] REST API: POST /v1/maneuvers/plane-change

---

### MS-MN-7: Rendezvous Planning

**Description:** Compute maneuvers for spacecraft rendezvous.

**Acceptance Criteria:**
- [ ] Target spacecraft ID
- [ ] Rendezvous epoch
- [ ] Phasing orbits
- [ ] Multiple-impulse sequence
- [ ] Relative state vectors
- [ ] Safety constraints (collision avoidance)
- [ ] REST API: POST /v1/maneuvers/rendezvous

---

### MS-MN-8: Station Keeping

**Description:** Maintain spacecraft in desired orbit.

**Acceptance Criteria:**
- [ ] Target orbit elements
- [ ] Tolerance bounds
- [ ] Periodic delta-V corrections
- [ ] Fuel budget tracking
- [ ] Automated or manual triggers
- [ ] REST API: POST /v1/maneuvers/station-keeping

---

### MS-MN-9: List Maneuvers

**Description:** List all maneuvers for a mission/spacecraft.

**Acceptance Criteria:**
- [ ] Filter by spacecraft ID
- [ ] Filter by mission ID
- [ ] Sort by epoch
- [ ] Paginated results
- [ ] REST API: GET /v1/maneuvers?spacecraftId={id}

---

### MS-MN-10: Delete Maneuver

**Description:** Remove a planned maneuver.

**Acceptance Criteria:**
- [ ] Maneuver ID required
- [ ] Soft delete
- [ ] ManeuverDeletedEvent published
- [ ] Fuel restored if not yet executed
- [ ] REST API: DELETE /v1/maneuvers/{id}
- [ ] Returns HTTP 204

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/maneuvers/impulsive | Create impulsive burn |
| POST | /v1/maneuvers/finite | Create finite burn |
| POST | /v1/maneuvers/optimize | Optimize maneuver |
| POST | /v1/maneuvers/hohmann | Hohmann transfer |
| POST | /v1/maneuvers/bi-elliptic | Bi-elliptic transfer |
| POST | /v1/maneuvers/plane-change | Plane change |
| POST | /v1/maneuvers/rendezvous | Rendezvous planning |
| POST | /v1/maneuvers/station-keeping | Station keeping |
| GET | /v1/maneuvers | List maneuvers |
| DELETE | /v1/maneuvers/{id} | Delete maneuver |

---

## Events Published

| Event | Description |
|-------|-------------|
| ManeuverCreatedEvent | Maneuver created |
| ManeuverUpdatedEvent | Maneuver updated |
| ManeuverDeletedEvent | Maneuver deleted |
| ManeuverOptimizedEvent | Maneuver optimized |
| FuelConsumedEvent | Fuel consumed by burn |

---

## Dependencies

- **Propagation Service** - State propagation
- **Spacecraft Service** - Spacecraft properties, fuel
- **Coordinate System Service** - Frame transformations
- **Optimization Service** - Optimization algorithms
