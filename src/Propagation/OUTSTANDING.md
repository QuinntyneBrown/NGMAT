# Propagation Service - Outstanding Work

**Current Completion: 75%**  
**Priority: MEDIUM**

---

## Overview

The Propagation Service has core entity models and service framework but lacks advanced integrator implementations, event detection, and performance optimizations.

---

## Implemented Components

✅ **Core Entities (3):**
- PropagationConfiguration
- PropagationState
- Integrators (enumeration)

✅ **Core Services:**
- PropagationService

✅ **API Endpoints:**
- PropagationEndpoints

✅ **Infrastructure:**
- PropagationRepository
- DbContext
- ServiceCollection

---

## Outstanding Requirements

### 🔴 High Priority

#### MS-PR-1: Propagate Orbit (Integrator Implementations)
**Status:** ⚠️ Framework exists, integrators incomplete

**Missing Components:**
- [ ] RK45 (Runge-Kutta-Fehlberg) implementation
- [ ] RK89 (high-precision) implementation
- [ ] Adams-Bashforth-Moulton multi-step
- [ ] Adaptive step size control
- [ ] Error tolerance enforcement
- [ ] Force model integration
- [ ] State output at intervals

**Implementation Tasks:**
1. Implement RK45 integrator
2. Implement RK89 integrator
3. Add Adams-Bashforth
4. Implement adaptive stepping
5. Integrate force models
6. Add output interpolation
7. Optimize performance

**Acceptance Criteria:**
- RK45 adaptive stepping
- RK89 for high precision
- Error tolerance < 1e-12
- Integration with Force Model service
- < 1 second for 1 day propagation

---

#### MS-PR-5: Event Detection
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Apoapsis/periapsis detection
- [ ] Node crossing detection
- [ ] Altitude threshold crossings
- [ ] Eclipse entry/exit
- [ ] User-defined event functions
- [ ] Event timestamps
- [ ] Root finding for events

**Implementation Tasks:**
1. Implement event detection framework
2. Add standard event types
3. Integrate root finding
4. Timestamp events accurately
5. Allow custom event functions
6. Publish event notifications

**Acceptance Criteria:**
- Detect all standard events
- Timestamp accuracy < 1 second
- Custom events supported
- Events published to event bus

---

#### MS-PR-6: State Transition Matrix
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] 6x6 STM computation
- [ ] Partials with respect to initial state
- [ ] Covariance propagation
- [ ] Integration during propagation
- [ ] Optional computation toggle

**Implementation Tasks:**
1. Implement STM equations
2. Integrate STM propagation
3. Add covariance propagation
4. Make optional (performance)
5. Validate accuracy

---

### 🟡 Medium Priority

#### MS-PR-7: Two-Body Propagation
**Status:** ⚠️ Needs implementation

**Missing Components:**
- [ ] Keplerian propagation
- [ ] Analytical solution
- [ ] Fast computation
- [ ] No perturbations
- [ ] Useful for estimates

**Implementation Tasks:**
1. Implement Keplerian propagator
2. Add analytical formulas
3. Optimize for speed
4. Add validation

---

#### MS-PR-8: Parallel Propagation
**Status:** ❌ Not Implemented

**Missing Components:**
- [ ] Multiple spacecraft support
- [ ] Parallel execution
- [ ] Independent error handling
- [ ] Result aggregation
- [ ] Performance metrics

**Implementation Tasks:**
1. Design parallel architecture
2. Implement async propagation
3. Add error isolation
4. Aggregate results
5. Measure performance

---

### 🔵 Low Priority

#### Additional Features
- [ ] Variable-order integrators
- [ ] Symplectic integrators
- [ ] Encke's method
- [ ] Cowell's method
- [ ] Backward propagation
- [ ] Propagation with maneuvers
- [ ] Interpolation methods
- [ ] Propagation checkpoints

---

## Estimated Effort

- **High Priority:** 4-5 weeks
- **Medium Priority:** 2-3 weeks
- **Low Priority:** 2-3 weeks
- **Total:** 8-11 weeks

---

## Success Criteria

- ✅ All integrators working
- ✅ Event detection functional
- ✅ STM computation accurate
- ✅ Two-body propagation fast
- ✅ Parallel propagation efficient
- ✅ Validated against reference data
