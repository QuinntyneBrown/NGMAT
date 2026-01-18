# Spacecraft Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Spacecraft Service, which manages spacecraft definitions and properties.

---

## Phase 1: Core CRUD

**Goal:** Basic spacecraft management.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up database
- [ ] Configure Redis Pub/Sub connection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Create Spacecraft (MS-SC-1)
- [ ] Create Spacecraft entity
- [ ] Implement create endpoint
- [ ] Validate physical properties
- [ ] Publish SpacecraftCreatedEvent

### Milestone 1.3: CRUD Operations (MS-SC-2, MS-SC-3)
- [ ] Implement update endpoint
- [ ] Implement delete endpoint
- [ ] Add dependency checking
- [ ] Publish events

**Deliverables:**
- Complete CRUD API

---

## Phase 2: State Management

**Goal:** State history and fuel tracking.

### Milestone 2.1: State History (MS-SC-4)
- [ ] Create state history entity
- [ ] Store states at epochs
- [ ] Implement state query endpoint
- [ ] Add interpolation

### Milestone 2.2: Fuel Management (MS-SC-5)
- [ ] Track fuel consumption
- [ ] Subscribe to FuelConsumedEvent
- [ ] Implement fuel query endpoint
- [ ] Add depletion warnings

**Deliverables:**
- State history tracking
- Fuel management

---

## Phase 3: Hardware Configuration

**Goal:** Detailed hardware modeling.

### Milestone 3.1: Hardware (MS-SC-6)
- [ ] Create hardware entities (thrusters, tanks, panels)
- [ ] Implement configuration endpoint
- [ ] Add power subsystem model
- [ ] Publish HardwareConfiguredEvent

**Deliverables:**
- Hardware configuration API

---

## Phase 4: Attitude & Validation

**Goal:** Attitude and configuration validation.

### Milestone 4.1: Attitude (MS-SC-7)
- [ ] Implement attitude modes
- [ ] Support quaternion/Euler angles
- [ ] Create attitude endpoint
- [ ] Publish AttitudeChangedEvent

### Milestone 4.2: Validation (MS-SC-8)
- [ ] Implement mass budget validation
- [ ] Calculate center of mass
- [ ] Calculate moment of inertia
- [ ] Add power budget validation
- [ ] Create validation endpoint

**Deliverables:**
- Attitude configuration
- Configuration validation

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core CRUD | P0 - Critical |
| Phase 2 | State Management | P0 - Critical |
| Phase 3 | Hardware | P1 - High |
| Phase 4 | Attitude & Validation | P2 - Medium |

---

## Success Metrics

- [ ] All 8 requirements (MS-SC-1 through MS-SC-8) implemented
- [ ] 85%+ unit test coverage
