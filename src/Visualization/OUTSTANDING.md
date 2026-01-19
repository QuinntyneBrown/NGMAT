# Visualization Service - Outstanding Work

**Current Completion: 50%**  
**Priority: HIGH**

---

## Overview

The Visualization Service has a comprehensive service implementation (895 lines) with good logic for data generation, but lacks database persistence, WebSocket support for real-time updates, and 3D visualization infrastructure. The service can generate visualization data but cannot store or stream it efficiently.

---

## Implemented Components

✅ **Core Services:**
- VisualizationService (895 lines, comprehensive)
- Data generation methods

✅ **Core Models:**
- Visualization data structures
- Visualization events

✅ **API Endpoints:**
- VisualizationEndpoints (basic structure)

---

## Outstanding Requirements

### 🔴 High Priority (Critical Gaps)

#### Database Layer Implementation
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] VisualizationDbContext
- [ ] VisualizationConfiguration entity
- [ ] VisualizationCache entity for pre-computed data
- [ ] VisualizationRepository implementation
- [ ] Database migrations
- [ ] Caching strategy for expensive computations

**Implementation Tasks:**
1. Create Visualization.Infrastructure files
2. Define entity models
3. Create VisualizationDbContext
4. Implement VisualizationRepository
5. Add caching repository
6. Configure entity relationships
7. Add database migrations
8. Implement data caching strategy

**Entities Needed:**
```csharp
- VisualizationConfiguration (user preferences)
- CachedOrbitData (pre-computed orbit paths)
- GroundTrackCache (ground track data)
- PlotConfiguration (saved plot settings)
- VisualizationSnapshot (saved visualizations)
```

---

#### MS-VZ-1: Orbit Plot Data (Real-Time)
**Status:** ⚠️ Service logic exists, needs real-time updates

**Missing Components:**
- [ ] SignalR hub for real-time updates
- [ ] WebSocket endpoint for streaming
- [ ] Efficient data streaming for large orbits
- [ ] Data decimation/sampling algorithms
- [ ] Level-of-detail (LOD) support
- [ ] Incremental data updates
- [ ] Client-side caching headers

**Implementation Tasks:**
1. Install SignalR package
2. Create VisualizationHub
3. Implement streaming endpoints
4. Add data decimation algorithms
5. Implement LOD system
6. Add incremental update support
7. Configure caching headers

**Acceptance Criteria:**
- Real-time orbit updates via WebSocket
- Large orbits streamed efficiently
- LOD reduces data for distant views
- < 100ms latency for updates
- Supports 10+ concurrent visualizations

---

#### MS-VZ-2: Ground Track (Performance)
**Status:** ⚠️ Basic logic exists, needs optimization

**Missing Components:**
- [ ] Ground track caching
- [ ] Ascending/descending node detection
- [ ] Anti-meridian handling (180° crossing)
- [ ] Ground station visibility
- [ ] Coverage area visualization
- [ ] Real-time ground track updates

**Implementation Tasks:**
1. Implement ground track caching
2. Add node detection algorithm
3. Handle anti-meridian crossing
4. Calculate ground station visibility
5. Add coverage area calculations
6. Implement real-time updates

**Acceptance Criteria:**
- Ground track calculated efficiently
- Anti-meridian crossings handled
- Ascending/descending nodes marked
- Ground station visibility computed
- Real-time updates via SignalR

---

#### MS-VZ-3: 3D Model Export
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] GLTF format exporter
- [ ] OBJ format exporter
- [ ] Spacecraft 3D model support
- [ ] Orbit path mesh generation
- [ ] Celestial body mesh generation
- [ ] Texture coordinate mapping
- [ ] Scene graph export

**Implementation Tasks:**
1. Install 3D export libraries
2. Implement GLTF exporter
3. Implement OBJ exporter
4. Create mesh generation utilities
5. Add texture support
6. Implement scene assembly
7. Create export endpoints

**Acceptance Criteria:**
- Export scene to GLTF format
- Export scene to OBJ format
- Include spacecraft models
- Include orbit paths
- Include celestial bodies
- Textures embedded or referenced

---

#### MS-VZ-4: Time-Series Plot Data (Streaming)
**Status:** ⚠️ Basic logic exists, needs streaming

**Missing Components:**
- [ ] Efficient data streaming for large datasets
- [ ] Data aggregation/downsampling
- [ ] Multiple parameter support
- [ ] Real-time data updates
- [ ] CSV export
- [ ] Plot configuration storage

**Implementation Tasks:**
1. Implement data streaming
2. Add downsampling algorithms
3. Support multiple parameters
4. Add real-time updates via SignalR
5. Implement CSV export
6. Store plot configurations

**Acceptance Criteria:**
- Stream large datasets efficiently
- Downsample for performance
- Multiple parameters in one request
- Real-time updates for active plots
- Export to CSV

---

#### MS-VZ-5: Orbital Elements Plot
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Keplerian elements over time
- [ ] Osculating vs mean elements
- [ ] Element variation detection
- [ ] Real-time element tracking
- [ ] Element history caching

**Implementation Tasks:**
1. Calculate elements over time
2. Distinguish osculating/mean elements
3. Detect variations
4. Add real-time tracking
5. Implement caching

---

#### MS-VZ-6: Eclipse Plot
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Shadow function calculations
- [ ] Umbra/penumbra detection
- [ ] Sun visibility percentage
- [ ] Eclipse event detection
- [ ] Eclipse timeline generation
- [ ] Integration with Force Model service

**Implementation Tasks:**
1. Implement shadow function
2. Calculate umbra/penumbra boundaries
3. Compute sun visibility
4. Detect eclipse events
5. Generate timeline
6. Integrate with Force Model service

**Acceptance Criteria:**
- Accurate eclipse predictions
- Umbra and penumbra distinguished
- Sun visibility percentage computed
- Eclipse events timestamped
- Timeline exported

---

### 🟡 Medium Priority

#### MS-VZ-7: 3D Attitude Visualization
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Attitude data generation
- [ ] Quaternion interpolation
- [ ] Body-fixed axes visualization
- [ ] Sun/Earth vector visualization
- [ ] Attitude history

**Implementation Tasks:**
1. Generate attitude data
2. Implement quaternion SLERP
3. Calculate body-fixed axes
4. Compute sun/Earth vectors
5. Store attitude history

---

#### MS-VZ-8: Conjunction Analysis Plot
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Relative position calculations
- [ ] Closest approach detection
- [ ] Collision probability (if covariance available)
- [ ] Conjunction timeline
- [ ] Safety distance visualization

**Implementation Tasks:**
1. Calculate relative positions
2. Find closest approach
3. Compute collision probability
4. Generate timeline
5. Visualize safety distances

---

### 🔵 Low Priority

#### Additional Features
- [ ] Animation generation (MP4, GIF)
- [ ] Screenshot capture service
- [ ] 3D scene composition
- [ ] Lighting configuration
- [ ] Camera path generation
- [ ] Annotation support
- [ ] Measurement tools (distance, angle)
- [ ] Multi-spacecraft visualization
- [ ] Custom color schemes
- [ ] Grid/axis customization

---

## WebSocket/SignalR Endpoints

| Hub Method | Description | Priority |
|------------|-------------|----------|
| SubscribeOrbit | Real-time orbit updates | 🔴 High |
| SubscribeGroundTrack | Real-time ground track | 🔴 High |
| SubscribeTimeSeries | Real-time plot data | 🔴 High |
| SubscribeEclipse | Real-time eclipse | 🟡 Medium |
| SubscribeAttitude | Real-time attitude | 🟡 Medium |
| SubscribeConjunction | Real-time conjunction | 🟡 Medium |

---

## API Endpoints Status

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| GET | /v1/visualization/orbit/{id} | Orbit plot data | ⚠️ Partial |
| GET | /v1/visualization/ground-track/{id} | Ground track | ⚠️ Partial |
| GET | /v1/visualization/export | 3D model export | ❌ Missing |
| GET | /v1/visualization/timeseries/{param} | Time-series data | ⚠️ Partial |
| GET | /v1/visualization/elements/{id} | Orbital elements | ⚠️ Partial |
| GET | /v1/visualization/eclipse/{id} | Eclipse plot | ❌ Missing |
| GET | /v1/visualization/attitude/{id} | Attitude | ❌ Missing |
| GET | /v1/visualization/conjunction | Conjunction | ❌ Missing |
| WS | /hubs/visualization | Real-time stream | ❌ Missing |

---

## Performance Optimization Needs

1. **Data Decimation:** Reduce point count for large orbits
2. **LOD System:** Different detail levels based on zoom
3. **Caching:** Pre-compute expensive visualizations
4. **Streaming:** Stream large datasets incrementally
5. **Compression:** Compress JSON data for transfer
6. **CDN:** Cache static celestial body data
7. **WebGL Optimization:** Efficient mesh generation

---

## Technical Debt

1. **No real-time updates** - No SignalR/WebSocket support
2. **No database caching** - Expensive computations repeated
3. **No 3D export** - Cannot export visualizations
4. **No streaming** - Large datasets may timeout
5. **No LOD system** - Performance issues with complex scenes
6. **No eclipse calculations** - Shadow function not implemented

---

## Implementation Recommendations

### Phase 1: Real-Time Infrastructure (Week 1)
1. Install SignalR
2. Create VisualizationHub
3. Implement WebSocket endpoints
4. Add real-time updates for orbits

### Phase 2: Database Layer (Week 2)
1. Create entity models
2. Set up DbContext
3. Implement caching repository
4. Add configuration storage

### Phase 3: Performance (Week 3)
1. Implement data decimation
2. Add LOD system
3. Implement streaming
4. Add compression

### Phase 4: 3D Export (Week 4)
1. Install 3D libraries
2. Implement GLTF exporter
3. Implement OBJ exporter
4. Add scene generation

### Phase 5: Advanced Visualizations (Week 5)
1. Implement eclipse calculations
2. Add attitude visualization
3. Add conjunction analysis
4. Complete all endpoints

---

## Dependencies

**Requires:**
- Database (SQL Server or PostgreSQL)
- SignalR (real-time updates)
- Redis (caching)
- 3D export libraries (glTFLoader, Assimp)

**Integrates With:**
- Propagation service (orbit data)
- Spacecraft service (spacecraft data)
- CoordinateSystem service (transformations)
- ForceModel service (eclipse calculations)

---

## Estimated Effort

- **High Priority:** 4-5 weeks (1 developer)
- **Medium Priority:** 2 weeks
- **Low Priority:** 1-2 weeks
- **Total:** 7-9 weeks

---

## Testing Requirements

- [ ] Unit tests for visualization logic
- [ ] Integration tests for SignalR
- [ ] Performance tests for large datasets
- [ ] Load tests for concurrent users
- [ ] 3D export validation tests
- [ ] Eclipse calculation accuracy tests
- [ ] End-to-end visualization tests

---

## Success Criteria

- ✅ Real-time orbit updates via WebSocket
- ✅ Large datasets streamed efficiently
- ✅ 3D exports working (GLTF, OBJ)
- ✅ Eclipse predictions accurate
- ✅ Ground tracks rendered correctly
- ✅ Time-series plots performant
- ✅ Multiple users supported concurrently
- ✅ < 100ms latency for updates
- ✅ Supports 10+ active visualizations
