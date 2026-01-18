# Propagation Service Requirements

## Overview

**Domain:** Orbit and trajectory propagation using numerical integration.

The Propagation Service integrates spacecraft equations of motion to compute future (or past) states, supporting various numerical integrators and event detection.

---

## Requirements

### MS-PR-1: Propagate Orbit

**Description:** Propagate spacecraft state from initial epoch to final epoch.

**Acceptance Criteria:**
- [ ] Spacecraft ID required
- [ ] Start epoch required
- [ ] End epoch required
- [ ] Propagator type selectable (RK45, RK89, Adams-Bashforth)
- [ ] Step size configurable
- [ ] Tolerance configurable
- [ ] Force model IDs specified
- [ ] Output state vectors at specified intervals
- [ ] PropagationRequestedEvent published
- [ ] PropagationCompletedEvent published
- [ ] REST API: POST /v1/propagation/propagate
- [ ] Returns HTTP 202 Accepted (async operation)
- [ ] Job ID returned for status tracking

---

### MS-PR-2: Query Propagation Status

**Description:** Check status of propagation job.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Status: Queued, Running, Completed, Failed
- [ ] Progress percentage
- [ ] Estimated time remaining
- [ ] Error message if failed
- [ ] REST API: GET /v1/propagation/jobs/{jobId}
- [ ] Returns HTTP 200

---

### MS-PR-3: Retrieve Propagation Results

**Description:** Get propagated state vectors.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Paginated results
- [ ] Filter by epoch range
- [ ] Output format: JSON, CSV
- [ ] Includes position (x, y, z) and velocity (vx, vy, vz)
- [ ] Coordinate system specified
- [ ] REST API: GET /v1/propagation/jobs/{jobId}/results
- [ ] Returns HTTP 200

---

### MS-PR-4: Numerical Integrator Selection

**Description:** Support multiple propagation methods.

**Acceptance Criteria:**
- [ ] Runge-Kutta 4th order (RK4)
- [ ] Runge-Kutta-Fehlberg 4-5 (RK45) with adaptive step
- [ ] Runge-Kutta 8-9 (RK89) for high precision
- [ ] Adams-Bashforth-Moulton (multi-step)
- [ ] Configurable error tolerance
- [ ] Automatic step size adjustment

---

### MS-PR-5: Event Detection

**Description:** Detect orbital events during propagation.

**Acceptance Criteria:**
- [ ] Apoapsis/periapsis detection
- [ ] Ascending/descending node crossing
- [ ] Altitude threshold crossing
- [ ] Eclipse entry/exit
- [ ] User-defined event functions
- [ ] Event timestamps recorded
- [ ] EventDetectedEvent published

---

### MS-PR-6: State Transition Matrix

**Description:** Compute state transition matrix for orbit determination.

**Acceptance Criteria:**
- [ ] 6x6 STM computed during propagation
- [ ] Partials with respect to initial state
- [ ] Used for covariance propagation
- [ ] Optional computation (performance overhead)

---

### MS-PR-7: Two-Body Propagation

**Description:** Fast analytical two-body propagation.

**Acceptance Criteria:**
- [ ] Keplerian elements propagation
- [ ] No perturbations
- [ ] Closed-form solution (fast)
- [ ] Useful for quick estimates
- [ ] REST API: POST /v1/propagation/two-body

---

### MS-PR-8: Parallel Propagation

**Description:** Propagate multiple spacecraft in parallel.

**Acceptance Criteria:**
- [ ] Accept array of spacecraft IDs
- [ ] Parallel execution using async/await
- [ ] Independent failure handling
- [ ] Consolidated results
- [ ] Performance metrics

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/propagation/propagate | Start propagation job |
| GET | /v1/propagation/jobs/{jobId} | Get job status |
| GET | /v1/propagation/jobs/{jobId}/results | Get propagation results |
| DELETE | /v1/propagation/jobs/{jobId} | Cancel propagation job |
| POST | /v1/propagation/two-body | Fast two-body propagation |

---

## Events Published

| Event | Description |
|-------|-------------|
| PropagationRequestedEvent | Propagation job started |
| PropagationCompletedEvent | Propagation job completed |
| PropagationFailedEvent | Propagation job failed |
| EventDetectedEvent | Orbital event detected |

---

## Dependencies

- **Force Model Service** - Force computations
- **Coordinate System Service** - Frame transformations
- **Calculation Engine Service** - ODE solvers
- **Spacecraft Service** - Spacecraft properties
