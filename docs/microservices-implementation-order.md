# NGMAT Microservices Implementation Order

This document lists all microservices required for NGMAT, ordered by implementation priority based on dependencies.

---

## Phase 1: Foundation Infrastructure

These services provide the core infrastructure that all other services depend on.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 1 | **Event Bus Infrastructure** | Redis Pub/Sub (production) + UDP Multicast (local dev) for event-driven communication | None |
| 2 | **Identity Service** | Authentication, authorization, user management (OAuth2, JWT, RBAC) | Event Bus |
| 3 | **Event Store Service** | Event sourcing, audit trail, event replay | Event Bus |
| 4 | **Calculation Engine Service** | High-performance numerical computations (matrix ops, ODE solvers, quaternions) | None |

---

## Phase 2: Core Domain Services

Foundational domain services that provide essential data and computations.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 5 | **Coordinate System Service** | Reference frame definitions and transformations (ECI, ECEF, Keplerian) | Calculation Engine |
| 6 | **Ephemeris Service** | Celestial body positions (planets, Moon, Sun), DE440/441 integration | Calculation Engine, Coordinate System |
| 7 | **Force Model Service** | Environmental forces (gravity, drag, SRP, third-body) | Ephemeris, Coordinate System, Calculation Engine |
| 8 | **Propagation Service** | Orbit and trajectory propagation with numerical integration | Force Model, Coordinate System, Calculation Engine |

---

## Phase 3: Mission Management

Core business domain services for managing missions and spacecraft.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 9 | **Mission Management Service** | Mission lifecycle, configuration, sharing, import/export | Event Store, Identity |
| 10 | **Spacecraft Service** | Spacecraft definitions, hardware, fuel management, state history | Mission Management, Coordinate System |

---

## Phase 4: Advanced Mission Features

Services that build on core propagation and mission management.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 11 | **Maneuver Service** | Maneuver planning (impulsive, finite burns, Hohmann, rendezvous) | Propagation, Spacecraft, Coordinate System |
| 12 | **Optimization Service** | Trajectory optimization (SQP, genetic algorithm, differential correction) | Propagation, Maneuver, Calculation Engine |

---

## Phase 5: Supporting Services

Services that provide visualization, reporting, scripting, and notifications.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 13 | **Visualization Service** | 3D orbit visualization, ground tracks, time-series data | Propagation, Spacecraft, Coordinate System |
| 14 | **Reporting Service** | Report generation (PDF, CSV), TLE generation, delta-V budgets | Mission Management, Spacecraft, Maneuver |
| 15 | **Script Execution Service** | GMAT script parsing, validation, and execution | All domain services |
| 16 | **Notification Service** | Email, webhooks, real-time notifications, alert rules | Event Bus, Identity |

---

## Phase 6: API Gateway

Central entry point that routes to all backend services.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 17 | **API Gateway** | Request routing, JWT validation, rate limiting, caching, documentation | All backend services |

---

## Phase 7: Frontend Applications

User interfaces that consume the backend microservices.

| Order | Service | Description | Key Dependencies |
|-------|---------|-------------|------------------|
| 18 | **Web Frontend** | Angular-based browser application with 3D visualization | API Gateway |
| 19 | **Desktop Frontend** | Native WPF/Avalonia/.NET MAUI application with offline support | API Gateway (optional local computation) |

---

## Summary

| Phase | Services | Count |
|-------|----------|-------|
| Phase 1: Foundation | Event Bus, Identity, Event Store, Calculation Engine | 4 |
| Phase 2: Core Domain | Coordinate System, Ephemeris, Force Model, Propagation | 4 |
| Phase 3: Mission Management | Mission Management, Spacecraft | 2 |
| Phase 4: Advanced Features | Maneuver, Optimization | 2 |
| Phase 5: Supporting | Visualization, Reporting, Script Execution, Notification | 4 |
| Phase 6: Gateway | API Gateway | 1 |
| Phase 7: Frontends | Web Frontend, Desktop Frontend | 2 |
| **Total** | | **19** |

---

## Implementation Notes

1. **Parallel Development**: Within each phase, services can often be developed in parallel by different teams.

2. **Global Requirements**: Before starting any service, ensure these cross-cutting concerns are addressed:
   - Service Discovery (G-3)
   - Distributed Tracing (G-4)
   - Centralized Logging (G-5)
   - Configuration Management (G-6)
   - Container Orchestration (G-7)
   - Resilience Patterns (G-11)
   - CI/CD Pipeline (G-17)

3. **Database Per Service**: Each microservice owns its database schema (G-10).

4. **Coding Standards**: All services must comply with coding guidelines (G-21) - see `coding-guidelines.md`.

5. **Three-Project Structure**: Each microservice follows the Core, Infrastructure, API project structure.

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| Language | C# (.NET 8+) |
| Message Bus (Production) | Redis Pub/Sub with Redis Cluster |
| Message Bus (Local Dev) | UDP Multicast (no external dependencies) |
| Redis Client | StackExchange.Redis |
| Serialization | MessagePack with LZ4 compression |
| API Gateway | Ocelot or YARP |
| Databases | SQL Server, MongoDB, Redis |
| ORM | Entity Framework Core |
| Orchestration | Kubernetes or Azure Container Apps |
| Logging | Serilog + Seq or ELK Stack |
| Tracing | OpenTelemetry + Jaeger |
| Web Frontend | Angular + Angular Material |
| Desktop | WPF, Avalonia UI, or .NET MAUI |
