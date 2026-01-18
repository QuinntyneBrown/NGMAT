# Propagation Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Propagation Service, which integrates spacecraft equations of motion to compute orbital trajectories.

---

## Phase 1: Core Propagation

**Goal:** Basic orbit propagation capability.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up job queue infrastructure
- [ ] Configure Redis Pub/Sub connection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Two-Body Propagation (MS-PR-7)
- [ ] Implement Keplerian propagation
- [ ] Create analytical solution
- [ ] Create API endpoint

### Milestone 1.3: Basic Numerical Propagation (MS-PR-1)
- [ ] Integrate with Calculation Engine ODE solvers
- [ ] Integrate with Force Model Service
- [ ] Implement RK4 propagation
- [ ] Create propagation job entity
- [ ] Create API endpoint

**Deliverables:**
- Two-body propagation
- Basic numerical propagation

---

## Phase 2: Job Management

**Goal:** Async job handling and results storage.

### Milestone 2.1: Job Status (MS-PR-2)
- [ ] Implement job queue
- [ ] Track job progress
- [ ] Estimate remaining time
- [ ] Create status endpoint

### Milestone 2.2: Results Storage (MS-PR-3)
- [ ] Store propagation results
- [ ] Implement pagination
- [ ] Support epoch filtering
- [ ] Support CSV/JSON export

**Deliverables:**
- Async job management
- Results retrieval API

---

## Phase 3: Advanced Integrators

**Goal:** Multiple integrator options.

### Milestone 3.1: Integrators (MS-PR-4)
- [ ] Integrate RK45 with adaptive step
- [ ] Integrate RK89 for high precision
- [ ] Integrate Adams-Bashforth-Moulton
- [ ] Configure tolerance settings

**Deliverables:**
- Multiple integrator support
- Adaptive step sizing

---

## Phase 4: Event Detection

**Goal:** Detect and record orbital events.

### Milestone 4.1: Events (MS-PR-5)
- [ ] Implement apoapsis/periapsis detection
- [ ] Implement node crossing detection
- [ ] Implement altitude threshold crossing
- [ ] Implement eclipse detection
- [ ] Support custom event functions
- [ ] Publish EventDetectedEvent

**Deliverables:**
- Orbital event detection
- Event recording

---

## Phase 5: Advanced Features

**Goal:** STM computation and parallel propagation.

### Milestone 5.1: State Transition Matrix (MS-PR-6)
- [ ] Implement variational equations
- [ ] Compute 6x6 STM
- [ ] Support covariance propagation

### Milestone 5.2: Parallel Propagation (MS-PR-8)
- [ ] Implement batch propagation
- [ ] Handle parallel execution
- [ ] Independent error handling

**Deliverables:**
- STM computation
- Batch propagation

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Propagation | P0 - Critical |
| Phase 2 | Job Management | P0 - Critical |
| Phase 3 | Advanced Integrators | P1 - High |
| Phase 4 | Event Detection | P1 - High |
| Phase 5 | Advanced Features | P2 - Medium |

---

## Technical Dependencies

- **Force Model Service** - Acceleration computation
- **Coordinate System Service** - Frame transformations
- **Calculation Engine Service** - ODE solvers
- **Spacecraft Service** - Initial state, properties

---

## Success Metrics

- [ ] All 8 requirements (MS-PR-1 through MS-PR-8) implemented
- [ ] Propagation accuracy validated against GMAT
- [ ] Job completion rate > 99%
- [ ] 85%+ unit test coverage
