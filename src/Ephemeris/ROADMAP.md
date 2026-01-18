# Ephemeris Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Ephemeris Service, which provides celestial body positions and environmental data for NGMAT.

---

## Phase 1: Core Ephemeris

**Goal:** Basic planet and Moon positions.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up data file storage
- [ ] Configure Redis caching
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: JPL Ephemeris (MS-EP-4)
- [ ] Download and embed DE440/DE441 data files
- [ ] Implement Chebyshev polynomial interpolation
- [ ] Create ephemeris reader library

### Milestone 1.3: Planet Positions (MS-EP-1)
- [ ] Implement planet position queries
- [ ] Support all major planets
- [ ] Create API endpoint

### Milestone 1.4: Moon & Sun (MS-EP-2, MS-EP-3)
- [ ] Implement Moon position queries
- [ ] Implement Sun position queries
- [ ] Support multiple coordinate systems

**Deliverables:**
- High-precision planetary ephemeris
- JPL DE440/DE441 integration

---

## Phase 2: Earth Orientation

**Goal:** IERS data for coordinate transformations.

### Milestone 2.1: EOP Data (MS-EP-5)
- [ ] Download IERS Bulletin A data
- [ ] Implement EOP data parser
- [ ] Store historical EOP data
- [ ] Implement interpolation
- [ ] Create API endpoint

### Milestone 2.2: Automatic Updates
- [ ] Schedule daily IERS data updates
- [ ] Handle update failures gracefully
- [ ] Validate data integrity

**Deliverables:**
- Earth orientation parameter service
- Automated data updates

---

## Phase 3: Atmospheric Data

**Goal:** Solar flux for atmospheric models.

### Milestone 3.1: Solar Flux (MS-EP-6)
- [ ] Download NOAA SWPC data
- [ ] Implement data parser
- [ ] Store historical data
- [ ] Support forecasts
- [ ] Create API endpoint

**Deliverables:**
- Solar flux data service
- Drag model support

---

## Phase 4: Star Catalog

**Goal:** Star positions for attitude determination.

### Milestone 4.1: Star Catalog (MS-EP-7)
- [ ] Load Hipparcos catalog
- [ ] Implement spatial queries
- [ ] Support proper motion corrections
- [ ] Create API endpoint

**Deliverables:**
- Star catalog service

---

## Phase 5: Performance Optimization

**Goal:** Caching and performance.

### Milestone 5.1: Caching (MS-EP-8)
- [ ] Implement in-memory caching
- [ ] Configure Redis distributed cache
- [ ] Pre-compute common epochs
- [ ] Add cache metrics

**Deliverables:**
- Optimized ephemeris queries
- Caching layer

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Ephemeris | P0 - Critical |
| Phase 2 | Earth Orientation | P0 - Critical |
| Phase 3 | Atmospheric Data | P1 - High |
| Phase 4 | Star Catalog | P2 - Medium |
| Phase 5 | Performance | P1 - High |

---

## Technical Dependencies

- **Calculation Engine Service** - Chebyshev interpolation

---

## Success Metrics

- [ ] All 8 requirements (MS-EP-1 through MS-EP-8) implemented
- [ ] Planet position accuracy validated against JPL Horizons
- [ ] Query latency < 10ms with caching
- [ ] 80%+ unit test coverage
