# Visualization Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Visualization Service, which generates data for graphical representations.

---

## Phase 1: Core Visualization Data

**Goal:** Basic orbit and ground track data.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Configure service dependencies
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Orbit Plot Data (MS-VZ-1)
- [ ] Query propagation results
- [ ] Format 3D coordinates
- [ ] Create API endpoint

### Milestone 1.3: Ground Track (MS-VZ-2)
- [ ] Convert to lat/lon
- [ ] Mark ascending/descending
- [ ] Create API endpoint

**Deliverables:**
- Orbit visualization data
- Ground track data

---

## Phase 2: Time-Series Data

**Goal:** Plot data generation.

### Milestone 2.1: Time-Series (MS-VZ-4)
- [ ] Extract parameter values
- [ ] Format time-value pairs
- [ ] Create API endpoint

### Milestone 2.2: Orbital Elements (MS-VZ-5)
- [ ] Convert to Keplerian elements
- [ ] Create API endpoint

**Deliverables:**
- Time-series plot data

---

## Phase 3: Eclipse & Analysis

**Goal:** Eclipse and conjunction visualization.

### Milestone 3.1: Eclipse (MS-VZ-6)
- [ ] Calculate shadow function
- [ ] Determine umbra/penumbra periods
- [ ] Create API endpoint

### Milestone 3.2: Conjunction (MS-VZ-8)
- [ ] Calculate relative positions
- [ ] Find closest approach
- [ ] Create API endpoint

**Deliverables:**
- Eclipse visualization
- Conjunction analysis

---

## Phase 4: 3D Export & Attitude

**Goal:** Model export and attitude visualization.

### Milestone 4.1: 3D Export (MS-VZ-3)
- [ ] Generate GLTF format
- [ ] Generate OBJ format
- [ ] Include spacecraft models

### Milestone 4.2: Attitude (MS-VZ-7)
- [ ] Format attitude data
- [ ] Include reference vectors
- [ ] Create API endpoint

**Deliverables:**
- 3D model export
- Attitude visualization

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Visualization | P0 - Critical |
| Phase 2 | Time-Series | P1 - High |
| Phase 3 | Eclipse & Analysis | P2 - Medium |
| Phase 4 | 3D Export | P2 - Medium |

---

## Success Metrics

- [ ] All 8 requirements (MS-VZ-1 through MS-VZ-8) implemented
- [ ] Response time < 500ms for typical requests
- [ ] 80%+ unit test coverage
