# Maneuver Service - Outstanding Work

**Current Completion: 75%**  
**Priority: MEDIUM**

---

## Overview

The Maneuver Service has basic entity models and service structure but lacks advanced maneuver calculations, transfer orbit algorithms, and optimization integration.

---

## Implemented Components

✅ **Core Entities (2):**
- Maneuver
- TransferOrbits

✅ **Core Services:**
- ManeuverService

✅ **API Endpoints:**
- ManeuverEndpoints

✅ **Infrastructure:**
- ManeuverRepository
- DbContext
- ServiceCollection

---

## Outstanding Requirements

### 🔴 High Priority

#### MS-MN-1 & MS-MN-2: Burn Implementations
**Status:** ⚠️ Entities exist, calculations incomplete

**Missing Components:**
- [ ] Impulsive burn delta-V application
- [ ] Finite burn thrust integration
- [ ] Fuel consumption calculations (Tsiolkovsky)
- [ ] Coordinate frame transformations (VNB, LVLH)
- [ ] State vector updates
- [ ] Mass flow rate calculations
- [ ] Isp integration

**Implementation Tasks:**
1. Implement delta-V application
2. Add fuel consumption calculator
3. Integrate coordinate transforms
4. Implement thrust integration
5. Update spacecraft state
6. Validate burn parameters

---

#### MS-MN-4: Hohmann Transfer
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Two-impulse calculation
- [ ] Delta-V for each burn
- [ ] Transfer time calculation
- [ ] Optimal burn epochs
- [ ] Validation for circular orbits

**Implementation Tasks:**
1. Implement Hohmann equations
2. Calculate burn delta-Vs
3. Compute transfer time
4. Find optimal burn points
5. Add validation

---

#### MS-MN-5: Bi-Elliptic Transfer
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Three-impulse calculation
- [ ] Intermediate apoapsis selection
- [ ] Comparison with Hohmann
- [ ] Transfer time calculation

---

#### MS-MN-6: Plane Change Maneuver
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Inclination change delta-V
- [ ] Node detection
- [ ] Combined plane change + Hohmann
- [ ] Optimal burn location

---

### 🟡 Medium Priority

#### MS-MN-3: Maneuver Optimization
**Status:** ⚠️ Needs Optimization service integration

**Missing Components:**
- [ ] Integration with Optimization service
- [ ] Cost function definitions
- [ ] Constraint definitions
- [ ] Initial guess generation
- [ ] Convergence criteria

---

#### MS-MN-7: Rendezvous Planning
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Phasing orbit calculations
- [ ] Multi-impulse sequences
- [ ] Relative state vectors
- [ ] Collision avoidance
- [ ] Safety constraints

---

#### MS-MN-8: Station Keeping
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Orbit deviation detection
- [ ] Correction maneuver planning
- [ ] Fuel budget tracking
- [ ] Automated triggers
- [ ] Long-term planning

---

### 🔵 Low Priority

#### Additional Features
- [ ] Low-thrust trajectories
- [ ] Lambert solver
- [ ] Pork-chop plots
- [ ] Launch window analysis
- [ ] Gravity assists
- [ ] Aerobraking
- [ ] Lithobraking
- [ ] Multi-rev transfers

---

## Estimated Effort

- **High Priority:** 3-4 weeks
- **Medium Priority:** 2-3 weeks
- **Low Priority:** 2-3 weeks
- **Total:** 7-10 weeks

---

## Success Criteria

- ✅ All burn types working
- ✅ Transfer orbits calculated
- ✅ Fuel consumption accurate
- ✅ Optimization integrated
- ✅ Rendezvous planning functional
