# Optimization Service Requirements

## Overview

**Domain:** Trajectory optimization and parameter estimation.

The Optimization Service provides trajectory optimization algorithms including gradient-based methods, evolutionary algorithms, and differential correction for mission design.

---

## Requirements

### MS-OP-1: Optimize Trajectory

**Description:** Find optimal trajectory given constraints and cost function.

**Acceptance Criteria:**
- [ ] Mission ID required
- [ ] Objective function (minimize time, fuel, delta-V, etc.)
- [ ] Design variables (burn epochs, magnitudes, directions)
- [ ] Constraints (min altitude, max delta-V, etc.)
- [ ] Optimization algorithm (SQP, genetic, particle swarm)
- [ ] Initial guess or auto-generate
- [ ] Max iterations configurable
- [ ] Convergence tolerance
- [ ] OptimizationStartedEvent published
- [ ] OptimizationCompletedEvent published
- [ ] REST API: POST /v1/optimization/trajectory
- [ ] Returns HTTP 202 (async)

---

### MS-OP-2: Query Optimization Status

**Description:** Check progress of optimization job.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Status: Queued, Running, Converged, Failed, MaxIterations
- [ ] Current iteration number
- [ ] Current cost value
- [ ] Best solution so far
- [ ] Constraint violations
- [ ] REST API: GET /v1/optimization/jobs/{jobId}
- [ ] Returns HTTP 200

---

### MS-OP-3: Retrieve Optimal Solution

**Description:** Get optimized parameters.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Optimal design variables
- [ ] Final cost value
- [ ] Constraint satisfaction
- [ ] Convergence statistics
- [ ] REST API: GET /v1/optimization/jobs/{jobId}/solution
- [ ] Returns HTTP 200

---

### MS-OP-4: Sequential Quadratic Programming (SQP)

**Description:** Gradient-based optimizer for smooth problems.

**Acceptance Criteria:**
- [ ] Gradient computation (finite differences or automatic differentiation)
- [ ] Hessian approximation (BFGS)
- [ ] Line search
- [ ] Quadratic subproblem solver
- [ ] Handles equality and inequality constraints

---

### MS-OP-5: Genetic Algorithm

**Description:** Global optimization for non-smooth problems.

**Acceptance Criteria:**
- [ ] Population size configurable
- [ ] Crossover and mutation operators
- [ ] Fitness function evaluation
- [ ] Elitism
- [ ] Convergence on fitness stagnation
- [ ] Parallel fitness evaluations

---

### MS-OP-6: Particle Swarm Optimization

**Description:** Swarm-based global optimizer.

**Acceptance Criteria:**
- [ ] Particle count configurable
- [ ] Velocity update equations
- [ ] Global best tracking
- [ ] Inertia weight decay
- [ ] Boundary constraints

---

### MS-OP-7: Multi-Objective Optimization

**Description:** Optimize multiple conflicting objectives (Pareto front).

**Acceptance Criteria:**
- [ ] Multiple objective functions
- [ ] Pareto optimal solutions
- [ ] NSGA-II or similar algorithm
- [ ] Visualization of Pareto front
- [ ] User selects preferred solution

---

### MS-OP-8: Differential Correction

**Description:** Refine trajectory to meet target conditions.

**Acceptance Criteria:**
- [ ] Target state or orbit elements
- [ ] Control variables (maneuver parameters)
- [ ] State transition matrix used
- [ ] Iterative correction
- [ ] Converges in < 10 iterations typically

---

### MS-OP-9: Sensitivity Analysis

**Description:** Analyze sensitivity to design variables.

**Acceptance Criteria:**
- [ ] Compute partial derivatives
- [ ] Jacobian matrix output
- [ ] Identify most sensitive parameters
- [ ] Used for uncertainty quantification

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/optimization/trajectory | Start optimization |
| GET | /v1/optimization/jobs/{jobId} | Get job status |
| GET | /v1/optimization/jobs/{jobId}/solution | Get solution |
| DELETE | /v1/optimization/jobs/{jobId} | Cancel optimization |
| POST | /v1/optimization/differential-correction | Differential correction |
| POST | /v1/optimization/sensitivity | Sensitivity analysis |

---

## Events Published

| Event | Description |
|-------|-------------|
| OptimizationStartedEvent | Optimization job started |
| OptimizationCompletedEvent | Optimization converged |
| OptimizationFailedEvent | Optimization failed |

---

## Dependencies

- **Propagation Service** - Trajectory evaluation
- **Calculation Engine Service** - Numerical methods
- **Maneuver Service** - Maneuver parameters
