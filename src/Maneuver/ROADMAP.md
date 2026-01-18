# Maneuver Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Maneuver Service, which handles orbital maneuver planning and execution.

---

## Phase 1: Basic Burns

**Goal:** Impulsive and finite burn capability.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up database
- [ ] Configure Redis Pub/Sub connection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Impulsive Burns (MS-MN-1)
- [ ] Create Maneuver entity
- [ ] Implement impulsive burn calculation
- [ ] Apply Tsiolkovsky equation
- [ ] Publish ManeuverCreatedEvent
- [ ] Create API endpoint

### Milestone 1.3: Finite Burns (MS-MN-2)
- [ ] Implement finite burn model
- [ ] Integrate mass flow rate
- [ ] Propagate state during burn
- [ ] Create API endpoint

**Deliverables:**
- Impulsive burn API
- Finite burn API

---

## Phase 2: Transfer Orbits

**Goal:** Standard orbital transfers.

### Milestone 2.1: Hohmann Transfer (MS-MN-4)
- [ ] Implement two-impulse calculation
- [ ] Calculate transfer time
- [ ] Create API endpoint

### Milestone 2.2: Bi-Elliptic Transfer (MS-MN-5)
- [ ] Implement three-impulse calculation
- [ ] Compare with Hohmann
- [ ] Create API endpoint

### Milestone 2.3: Plane Change (MS-MN-6)
- [ ] Implement inclination change
- [ ] Calculate node timing
- [ ] Create API endpoint

**Deliverables:**
- Transfer orbit calculations

---

## Phase 3: Advanced Planning

**Goal:** Rendezvous and station keeping.

### Milestone 3.1: Rendezvous (MS-MN-7)
- [ ] Implement phasing calculations
- [ ] Plan multi-impulse sequence
- [ ] Add collision avoidance
- [ ] Create API endpoint

### Milestone 3.2: Station Keeping (MS-MN-8)
- [ ] Implement tolerance monitoring
- [ ] Plan correction maneuvers
- [ ] Track fuel budget
- [ ] Create API endpoint

**Deliverables:**
- Rendezvous planning
- Station keeping

---

## Phase 4: Optimization

**Goal:** Maneuver optimization.

### Milestone 4.1: Optimization (MS-MN-3)
- [ ] Integrate with Optimization Service
- [ ] Define cost functions
- [ ] Implement constraints
- [ ] Create async endpoint

**Deliverables:**
- Optimized maneuver planning

---

## Phase 5: Management

**Goal:** Maneuver CRUD and listing.

### Milestone 5.1: List & Delete (MS-MN-9, MS-MN-10)
- [ ] Implement list endpoint
- [ ] Implement delete endpoint
- [ ] Handle fuel restoration
- [ ] Publish events

**Deliverables:**
- Maneuver management API

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Basic Burns | P0 - Critical |
| Phase 2 | Transfer Orbits | P0 - Critical |
| Phase 3 | Advanced Planning | P1 - High |
| Phase 4 | Optimization | P1 - High |
| Phase 5 | Management | P0 - Critical |

---

## Success Metrics

- [ ] All 10 requirements (MS-MN-1 through MS-MN-10) implemented
- [ ] Delta-V calculations validated against GMAT
- [ ] 85%+ unit test coverage
