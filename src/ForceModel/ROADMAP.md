# Force Model Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Force Model Service, which computes environmental forces acting on spacecraft.

---

## Phase 1: Gravity Models

**Goal:** Core gravitational force computation.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Load gravity model coefficient files
- [ ] Configure dependency injection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Point Mass Gravity (MS-FM-1)
- [ ] Implement two-body gravity
- [ ] Support multiple central bodies
- [ ] Create API endpoint

### Milestone 1.3: Spherical Harmonics (MS-FM-1)
- [ ] Implement spherical harmonic expansion
- [ ] Load JGM-3 coefficients
- [ ] Load EGM-96 coefficients
- [ ] Configurable degree/order
- [ ] Optimize computation performance

### Milestone 1.4: Third-Body Gravity (MS-FM-6)
- [ ] Integrate with Ephemeris Service
- [ ] Implement Moon gravity
- [ ] Implement Sun gravity
- [ ] Support configurable body list

**Deliverables:**
- Complete gravity model library
- High-precision spherical harmonics

---

## Phase 2: Atmospheric Drag

**Goal:** Drag force computation.

### Milestone 2.1: Exponential Atmosphere (MS-FM-2)
- [ ] Implement simple exponential model
- [ ] Create API endpoint

### Milestone 2.2: Advanced Atmosphere Models
- [ ] Implement Jacchia-Roberts model
- [ ] Implement NRLMSISE-00 model
- [ ] Integrate solar flux from Ephemeris Service

**Deliverables:**
- Multiple atmosphere models
- Drag force computation

---

## Phase 3: Solar Radiation Pressure

**Goal:** SRP force computation.

### Milestone 3.1: SRP Model (MS-FM-3)
- [ ] Implement SRP acceleration
- [ ] Calculate shadow function
- [ ] Support umbra/penumbra
- [ ] Create API endpoint

**Deliverables:**
- SRP force model
- Eclipse computation

---

## Phase 4: Relativistic Effects

**Goal:** High-precision relativistic corrections.

### Milestone 4.1: Relativity (MS-FM-4)
- [ ] Implement Schwarzschild term
- [ ] Implement Lense-Thirring effect
- [ ] Implement de Sitter precession
- [ ] Make configurable

**Deliverables:**
- Relativistic corrections
- High-precision support

---

## Phase 5: Configuration & Extensions

**Goal:** Force model configuration and custom models.

### Milestone 5.1: Configuration (MS-FM-5)
- [ ] Create configuration entity
- [ ] Implement enable/disable per force type
- [ ] Store per-mission configuration
- [ ] Publish ForceModelConfiguredEvent

### Milestone 5.2: Custom Models (MS-FM-7)
- [ ] Design plugin interface
- [ ] Implement plugin loading
- [ ] Add sandboxing for user code

**Deliverables:**
- Configurable force models
- Plugin architecture

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Gravity Models | P0 - Critical |
| Phase 2 | Atmospheric Drag | P0 - Critical |
| Phase 3 | Solar Radiation Pressure | P1 - High |
| Phase 4 | Relativistic Effects | P3 - Low |
| Phase 5 | Configuration | P1 - High |

---

## Technical Dependencies

- **Ephemeris Service** - Body positions, solar flux
- **Calculation Engine Service** - Legendre polynomials

---

## Success Metrics

- [ ] All 7 requirements (MS-FM-1 through MS-FM-7) implemented
- [ ] Gravity acceleration validated against GMAT
- [ ] Spherical harmonics performance optimized
- [ ] 85%+ unit test coverage
