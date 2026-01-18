# NGMAT - .NET GMAT Requirements Specification
## Event Driven Microservices Architecture

**Document Version:** 1.0
**Date:** 2026-01-18
**Project:** NGMAT - .NET General Mission Analysis Tool
**Architecture:** Event Driven Microservices

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Microservices Overview](#microservices-overview)
3. [Global Requirements](#global-requirements)
4. [Microservice-Specific Requirements](#microservice-specific-requirements)

---

## Executive Summary

NGMAT is a .NET reimplementation of NASA's GMAT (General Mission Analysis Tool) designed as an event-driven microservices architecture.

### About GMAT

GMAT is **"the world's only enterprise, multi-mission, open source software system for space mission design, optimization, and navigation."** Developed by NASA, private industry, and public contributors, GMAT serves multiple purposes:
- Real-world mission support
- Engineering studies and analysis
- Education and training
- Public engagement

GMAT supports **missions in flight regimes ranging from low Earth orbit to lunar, libration point, and deep space missions**, making it a comprehensive tool for the aerospace community.

### NGMAT Vision

NGMAT aims to bring GMAT's powerful capabilities to the .NET ecosystem with modern microservices architecture, providing:
- **Aerospace mission analysis** across all flight regimes (LEO to deep space)
- **Trajectory optimization** for mission design
- **Orbital mechanics calculations** with high-precision numerical methods
- **Spacecraft mission planning** and simulation
- **Orbit determination** capabilities (production-quality)
- **Navigation analysis** for operational missions
- **Multi-mission support** for diverse spacecraft and objectives

### Architecture Principles

- **Event-Driven:** Services communicate via Redis Pub/Sub (UDP Multicast for local development, Redis Pub/Sub for production)
- **Microservices:** Independently deployable, scalable services
- **Domain-Driven Design:** Each service owns its domain
- **API Gateway Pattern:** Single entry point for external clients
- **CQRS where applicable:** Separation of read/write operations for complex domains
- **Event Sourcing for critical data:** Audit trail for mission-critical calculations
- **Multi-Platform UI:** Web interface and desktop application with shared backend services

### User Interface Strategy

NGMAT provides **two frontend interfaces** that consume the same backend microservices:

1. **Web Application** - Browser-based interface for accessibility and collaboration
   - **Framework:** Angular (latest stable) with Angular Material
   - **State Management:** RxJS observables with async pipe pattern
   - **Design:** Material Design 3 with BEM CSS methodology
   - **Cross-platform:** Windows, macOS, Linux (browser-based)
   - **Real-time updates:** SignalR for live notifications and data streaming
   - **Responsive design:** Mobile-first approach for tablets and desktops
   - **Standards:** See [coding-guidelines.md](./coding-guidelines.md) for Angular-specific requirements

2. **Desktop Application** - Native application for advanced features and performance
   - **Technology:** WPF (Windows), Avalonia UI (cross-platform), or .NET MAUI
   - **Platform-specific optimizations:** DirectX/OpenGL rendering
   - **Offline capabilities:** Full local computation with sync to cloud
   - **Direct GPU access:** High-performance 3D visualization
   - **File system integration:** Native file dialogs, drag-and-drop, file associations
   - **Standards:** Three-project structure (Core, Infrastructure, Desktop) per coding guidelines

---

## Microservices Overview

| Service Name | Description | Primary Responsibility |
|-------------|-------------|------------------------|
| **API Gateway** | Entry point for all client requests | Routing, authentication, rate limiting |
| **Mission Management Service** | Mission lifecycle and configuration | Create, update, delete missions; mission metadata |
| **Spacecraft Service** | Spacecraft definitions and properties | Spacecraft models, hardware, fuel systems |
| **Propagation Service** | Orbit and trajectory propagation | Numerical integration, state vector propagation |
| **Force Model Service** | Environmental force calculations | Gravity, drag, SRP, third-body effects |
| **Maneuver Service** | Maneuver planning and execution | Impulsive burns, finite burns, optimization |
| **Coordinate System Service** | Reference frame transformations | Coordinate conversions, frame rotations |
| **Ephemeris Service** | Celestial body position data | Planet/moon positions, solar system data |
| **Optimization Service** | Trajectory optimization | Cost function minimization, constraints |
| **Calculation Engine Service** | High-performance numerical computations | Matrix operations, ODE solvers, algorithms |
| **Visualization Service** | 3D orbit and trajectory visualization | Rendering data, plot generation |
| **Reporting Service** | Report generation and data export | CSV, JSON, PDF reports |
| **Script Execution Service** | Mission script parsing and execution | Command language interpreter |
| **Event Store Service** | Event sourcing and audit | Event persistence, replay capabilities |
| **Notification Service** | User notifications and alerts | Email, webhooks, real-time notifications |
| **Identity Service** | Authentication and authorization | User management, OAuth2, JWT |
| **Web Frontend** | Browser-based user interface | Mission planning, visualization, analysis (web) |
| **Desktop Frontend** | Native desktop application | Advanced 3D graphics, offline work, file integration |

---

## Global Requirements

### G-1: Event Bus Infrastructure

**Description:** Implement a reliable message bus for event-driven communication between microservices using Redis Pub/Sub.

**Technology Stack:**
- **Local Development:** UDP Multicast implementation of Redis Pub/Sub (no Redis server required)
- **Production:** Redis Pub/Sub with Redis Cluster for high availability

**Acceptance Criteria:**

**AC1: Publish/Subscribe Pattern**
- **GIVEN** a microservice has an event to publish
- **WHEN** the event is published to Redis Pub/Sub
- **THEN** all subscribed services receive the event

**AC2: Request/Reply Pattern**
- **GIVEN** a service needs a synchronous response from another service
- **WHEN** a request message is sent with a correlation ID
- **THEN** the response is routed back to the requester with the same correlation ID

**AC3: Dead Letter Queue**
- **GIVEN** a message fails processing after max retry attempts
- **WHEN** the final retry fails
- **THEN** the message is moved to a Redis-backed dead letter queue for manual inspection

**AC4: Retry Policy**
- **GIVEN** a message fails to process
- **WHEN** the failure is detected
- **THEN** retry with exponential backoff (1s, 2s, 4s, 8s, etc.)

**AC5: Message Persistence**
- **GIVEN** a message is published that requires persistence
- **WHEN** the message is stored
- **THEN** Redis Streams is used for durable message storage with acknowledgment

**AC6: Message Priority**
- **GIVEN** messages with different priority levels
- **WHEN** multiple messages are pending
- **THEN** higher priority messages are processed first using separate channels or Redis Sorted Sets

**AC7: Schema Versioning**
- **GIVEN** an event schema has multiple versions
- **WHEN** different services use different schema versions
- **THEN** messages are correctly deserialized based on version metadata

**AC8: At-Least-Once Delivery**
- **GIVEN** a message is published
- **WHEN** network issues or failures occur
- **THEN** the message is delivered at least once (duplicates possible, idempotency required)

**AC9: Message TTL**
- **GIVEN** a message has a configured TTL
- **WHEN** the TTL expires before processing
- **THEN** the message is removed using Redis key expiration

**AC10: Monitoring and Metrics**
- **GIVEN** the message bus is running
- **WHEN** messages flow through the system
- **THEN** metrics (throughput, latency, error rate) are collected via Redis INFO and custom instrumentation

**AC11: Local Development Mode (UDP Multicast)**
- **GIVEN** a developer runs the system locally without Redis
- **WHEN** services start up
- **THEN** they automatically use UDP Multicast for pub/sub communication
- **AND** no external dependencies are required for local development

**AC12: Environment-Based Configuration**
- **GIVEN** the application configuration specifies the environment
- **WHEN** the service initializes the message bus
- **THEN** it selects UDP Multicast (local) or Redis Pub/Sub (production) automatically

**Technology:** Redis Pub/Sub, Redis Streams, UDP Multicast (StackExchange.Redis for .NET)

---

### G-2: API Gateway

**Description:** Single entry point for all external client requests with routing and cross-cutting concerns.

**Acceptance Criteria:**
- [ ] Routes requests to appropriate microservices
- [ ] JWT token validation implemented
- [ ] Rate limiting per user/API key
- [ ] Request/response logging
- [ ] CORS configuration
- [ ] API versioning support (v1, v2, etc.)
- [ ] Circuit breaker pattern for resilience
- [ ] Request timeout configuration
- [ ] Response caching for read-heavy endpoints
- [ ] OpenAPI/Swagger documentation auto-generated

**Technology Options:** Ocelot, YARP, Azure API Management

---

### G-3: Service Discovery

**Description:** Dynamic service registration and discovery for microservices.

**Acceptance Criteria:**
- [ ] Services auto-register on startup
- [ ] Services de-register on shutdown
- [ ] Health check endpoints for each service
- [ ] Service metadata (version, endpoints) stored
- [ ] Load balancing across service instances
- [ ] Supports blue-green deployments
- [ ] DNS-based or registry-based discovery
- [ ] Automatic failover on service failure

**Technology Options:** Consul, Azure Service Fabric, Kubernetes Services

---

### G-4: Distributed Tracing

**Description:** End-to-end request tracing across microservices.

**Acceptance Criteria:**
- [ ] Correlation ID propagated across all services
- [ ] Trace spans created for each service operation
- [ ] Parent-child span relationships maintained
- [ ] Trace data includes timing information
- [ ] Traces queryable by correlation ID
- [ ] Integration with visualization tools
- [ ] Sampling rate configurable
- [ ] Performance overhead < 5%

**Technology:** OpenTelemetry, Jaeger, Zipkin, Application Insights

---

### G-5: Centralized Logging

**Description:** Aggregated logging from all microservices.

**Acceptance Criteria:**
- [ ] Structured logging (JSON format)
- [ ] Log levels: Trace, Debug, Info, Warn, Error, Critical
- [ ] Correlation ID in all log entries
- [ ] Searchable by service, level, timestamp, correlation ID
- [ ] Log retention policy implemented
- [ ] Real-time log streaming capability
- [ ] Sensitive data masking/redaction
- [ ] Alert rules for critical errors

**Technology:** Serilog, ELK Stack (Elasticsearch, Logstash, Kibana), Seq, Azure Monitor

---

### G-6: Configuration Management

**Description:** Centralized configuration for all microservices.

**Acceptance Criteria:**
- [ ] Externalized configuration from code
- [ ] Environment-specific configurations (dev, staging, prod)
- [ ] Configuration change without redeployment
- [ ] Secrets management (API keys, connection strings)
- [ ] Configuration versioning
- [ ] Rollback capability for configuration changes
- [ ] Encryption for sensitive configuration
- [ ] Configuration change audit log

**Technology:** Azure App Configuration, Consul KV, AWS Parameter Store, HashiCorp Vault

---

### G-7: Container Orchestration

**Description:** Containerized deployment and orchestration of microservices.

**Acceptance Criteria:**
- [ ] All services containerized with Docker
- [ ] Orchestration platform configured
- [ ] Auto-scaling based on CPU/memory/custom metrics
- [ ] Rolling updates with zero downtime
- [ ] Resource limits (CPU, memory) per service
- [ ] Container health checks
- [ ] Persistent volume support for stateful services
- [ ] Service mesh for inter-service communication (optional)

**Technology:** Kubernetes, Docker Swarm, Azure Container Apps

---

### G-8: Authentication & Authorization

**Description:** Secure authentication and role-based access control across all services.

**Acceptance Criteria:**
- [ ] OAuth 2.0 / OpenID Connect support
- [ ] JWT token-based authentication
- [ ] Role-based access control (RBAC)
- [ ] Claims-based authorization
- [ ] Multi-factor authentication (MFA) support
- [ ] Token refresh mechanism
- [ ] API key authentication for service-to-service
- [ ] Password policy enforcement
- [ ] Session management
- [ ] Audit log for authentication events

---

### G-9: API Versioning Strategy

**Description:** Consistent API versioning across all microservices.

**Acceptance Criteria:**
- [ ] URL-based versioning (e.g., /v1/missions, /v2/missions)
- [ ] Multiple versions supported simultaneously
- [ ] Deprecation warnings in older versions
- [ ] Migration guide for version upgrades
- [ ] Backward compatibility for at least 2 versions
- [ ] Version documented in OpenAPI spec

---

### G-10: Database Per Service

**Description:** Each microservice owns its database schema/instance.

**Acceptance Criteria:**
- [ ] No direct database access between services
- [ ] Service communication only via APIs/events
- [ ] Database technology choice per service needs
- [ ] Database schema migrations automated
- [ ] Database backup and recovery procedures
- [ ] Connection pooling configured
- [ ] Database monitoring and alerting

**Technology Options:** SQL Server, PostgreSQL, MongoDB, Redis, Cosmos DB

---

### G-11: Resilience Patterns

**Description:** Implement resilience patterns for fault tolerance.

**Acceptance Criteria:**
- [ ] Circuit breaker for downstream service calls
- [ ] Retry policy with exponential backoff
- [ ] Timeout policy for all external calls
- [ ] Fallback mechanisms for critical operations
- [ ] Bulkhead isolation to prevent cascading failures
- [ ] Health checks for all dependencies
- [ ] Graceful degradation when services unavailable

**Technology:** Polly library for .NET

---

### G-12: Performance Monitoring

**Description:** Application performance monitoring and metrics.

**Acceptance Criteria:**
- [ ] Response time metrics per endpoint
- [ ] Throughput (requests/second) tracking
- [ ] Error rate monitoring
- [ ] Resource utilization (CPU, memory, disk)
- [ ] Custom business metrics
- [ ] Real-time dashboards
- [ ] Alert thresholds configurable
- [ ] Historical data retention (min 90 days)

**Technology:** Prometheus + Grafana, Application Insights, Datadog

---

### G-13: Data Validation

**Description:** Consistent validation across all services.

**Acceptance Criteria:**
- [ ] Input validation on all API endpoints
- [ ] FluentValidation or DataAnnotations used
- [ ] Validation error messages standardized
- [ ] Units of measurement validated (meters, kg, etc.)
- [ ] Range checks for physical values
- [ ] Type safety enforced
- [ ] SQL injection prevention
- [ ] XSS prevention

---

### G-14: API Rate Limiting

**Description:** Prevent abuse and ensure fair usage.

**Acceptance Criteria:**
- [ ] Rate limits per user/API key
- [ ] Rate limits per endpoint
- [ ] Different tiers (free, premium, enterprise)
- [ ] Rate limit headers in responses (X-RateLimit-*)
- [ ] HTTP 429 Too Many Requests returned
- [ ] Burst allowance supported
- [ ] Rate limit metrics tracked

---

### G-15: Disaster Recovery

**Description:** Business continuity and disaster recovery plan.

**Acceptance Criteria:**
- [ ] Recovery Time Objective (RTO) defined
- [ ] Recovery Point Objective (RPO) defined
- [ ] Automated backups for all databases
- [ ] Multi-region deployment capability
- [ ] Disaster recovery runbook documented
- [ ] Regular DR drills performed
- [ ] Data replication across regions

---

### G-16: Testing Strategy

**Description:** Comprehensive testing at all levels.

**Acceptance Criteria:**
- [ ] Unit tests with > 80% code coverage
- [ ] Integration tests for each service
- [ ] Contract tests for inter-service communication
- [ ] End-to-end tests for critical workflows
- [ ] Performance/load tests defined
- [ ] Chaos engineering tests (optional)
- [ ] Automated test execution in CI/CD
- [ ] Test data management strategy

**Technology:** xUnit, NUnit, SpecFlow, Testcontainers, k6, JMeter

---

### G-17: CI/CD Pipeline

**Description:** Automated build, test, and deployment pipeline.

**Acceptance Criteria:**
- [ ] Automated build on code commit
- [ ] Automated test execution
- [ ] Code quality checks (SonarQube, etc.)
- [ ] Security scanning (SAST, DAST)
- [ ] Dependency vulnerability scanning
- [ ] Automated deployment to dev/staging
- [ ] Manual approval for production
- [ ] Rollback capability
- [ ] Deployment metrics tracked

**Technology:** Azure DevOps, GitHub Actions, Jenkins, GitLab CI

---

### G-18: Documentation Standards

**Description:** Comprehensive technical documentation.

**Acceptance Criteria:**
- [ ] Architecture decision records (ADRs)
- [ ] OpenAPI/Swagger for all REST APIs
- [ ] README per microservice
- [ ] Setup and deployment guides
- [ ] Developer onboarding guide
- [ ] API usage examples
- [ ] Event schema documentation
- [ ] Database schema diagrams
- [ ] Runbooks for operations

---

### G-19: Security Requirements

**Description:** Security best practices across all services.

**Acceptance Criteria:**
- [ ] HTTPS/TLS for all communications
- [ ] Secrets not stored in code/config files
- [ ] Principle of least privilege
- [ ] Input sanitization
- [ ] Output encoding
- [ ] OWASP Top 10 vulnerabilities addressed
- [ ] Regular security audits
- [ ] Penetration testing performed
- [ ] Data encryption at rest
- [ ] Data encryption in transit

---

### G-20: Compliance & Audit

**Description:** Audit trails and compliance requirements.

**Acceptance Criteria:**

**AC1: State Change Audit Log**
- **GIVEN** any entity state changes in the system
- **WHEN** the change is persisted
- **THEN** an audit log entry is created with timestamp, user, and change details

**AC2: User Action Tracking**
- **GIVEN** a user performs an action
- **WHEN** the action is executed
- **THEN** the action is logged with user ID, action type, and timestamp

**AC3: Data Retention Policies**
- **GIVEN** configured retention policies
- **WHEN** data exceeds retention period
- **THEN** data is automatically archived or deleted per policy

**AC4: GDPR Compliance**
- **GIVEN** a user requests their data
- **WHEN** the request is processed
- **THEN** all user data is exported in machine-readable format

**AC5: Data Deletion**
- **GIVEN** a user requests account deletion
- **WHEN** the deletion is confirmed
- **THEN** all personal data is permanently removed within 30 days

**AC6: Immutable Audit Logs**
- **GIVEN** audit logs are created
- **WHEN** stored in the audit system
- **THEN** logs cannot be modified or deleted

**AC7: Event Store Integrity**
- **GIVEN** events are stored in the event store
- **WHEN** validated for integrity
- **THEN** cryptographic hash verification confirms no tampering

---

### G-21: Coding Standards Compliance

**Description:** All code must adhere to established coding guidelines and architectural standards.

**Reference:** See [docs/coding-guidelines.md](./coding-guidelines.md) for complete specifications.

**Acceptance Criteria:**

**AC1: Namespace and File Structure**
- **GIVEN** any C# file in the codebase
- **WHEN** the namespace is compared to the physical file path
- **THEN** the namespace exactly matches the folder structure

**AC2: One Type Per File**
- **GIVEN** any source code file
- **WHEN** parsed by the compiler
- **THEN** the file contains exactly one class, interface, record, or enum

**AC3: Entity Identity Naming**
- **GIVEN** an entity with an identity property
- **WHEN** the property is defined
- **THEN** it includes the entity name (e.g., `CustomerId`, not `Id`)

**AC4: No Repository Pattern**
- **GIVEN** any data access code
- **WHEN** persistence operations are performed
- **THEN** they use the context interface directly (no repository abstraction)

**AC5: Structured Logging**
- **GIVEN** any backend service operation
- **WHEN** significant events occur (API calls, errors, warnings)
- **THEN** Serilog structured logging is used with appropriate enrichment (CorrelationId, UserId)

**AC6: Three-Project Structure**
- **GIVEN** a microservice implementation
- **WHEN** the solution structure is reviewed
- **THEN** it contains exactly Core, Infrastructure, and API projects

**AC7: Async Pipe Pattern (Frontend)**
- **GIVEN** an Angular component that loads data
- **WHEN** the component template is rendered
- **THEN** the async pipe is used (no manual `.subscribe()` calls)

**AC8: Message Envelope Pattern**
- **GIVEN** inter-service communication
- **WHEN** messages are sent via the message bus
- **THEN** all messages use the MessageEnvelope<TPayload> pattern with required headers

**AC9: Linting Compliance**
- **GIVEN** code is committed to the repository
- **WHEN** the build pipeline executes
- **THEN** StyleCop/ESLint checks pass with zero warnings/errors

**AC10: BEM CSS Naming**
- **GIVEN** frontend stylesheet code
- **WHEN** CSS classes are defined
- **THEN** they follow BEM (Block Element Modifier) naming convention

**AC11: No AutoMapper**
- **GIVEN** mapping between Core models and DTOs
- **WHEN** transformation is needed
- **THEN** extension methods with `ToDto()` pattern are used

**AC12: API Base URL Convention**
- **GIVEN** frontend API configuration
- **WHEN** baseUrl is defined
- **THEN** it contains only the base URI without `/api` suffix

**AC13: Idempotency in Message Handlers**
- **GIVEN** a message handler processes an event
- **WHEN** the same message is received multiple times
- **THEN** idempotency check prevents duplicate processing

**AC14: Schema Versioning**
- **GIVEN** message schemas evolve over time
- **WHEN** schema changes are made
- **THEN** only safe changes are allowed per schema evolution rules

**AC15: Test Coverage**
- **GIVEN** any business logic or message handler
- **WHEN** unit tests are executed
- **THEN** the code has test coverage and critical paths are tested

---

## Microservice-Specific Requirements

---

## 1. API Gateway Service

### MS-AG-1: Request Routing

**Description:** Route incoming requests to appropriate microservices.

**Acceptance Criteria:**
- [ ] Dynamic routing based on URL path
- [ ] Query string forwarding
- [ ] Header forwarding (excluding sensitive headers)
- [ ] HTTP method preservation (GET, POST, PUT, DELETE)
- [ ] WebSocket support for real-time features
- [ ] gRPC support (optional)
- [ ] Request transformation capability
- [ ] Response aggregation for composite queries

---

### MS-AG-2: Authentication Integration

**Description:** Validate user authentication before routing.

**Acceptance Criteria:**
- [ ] JWT token validation
- [ ] Token expiration check
- [ ] Unauthorized requests return HTTP 401
- [ ] Anonymous endpoints configurable
- [ ] API key validation support
- [ ] Token claims extracted and forwarded to services

---

### MS-AG-3: Rate Limiting

**Description:** Implement rate limiting to prevent abuse.

**Acceptance Criteria:**
- [ ] Rate limit per IP address
- [ ] Rate limit per authenticated user
- [ ] Rate limit per API key
- [ ] Configurable limits per endpoint
- [ ] HTTP 429 returned when limit exceeded
- [ ] Retry-After header included
- [ ] Rate limit status in response headers

---

### MS-AG-4: Response Caching

**Description:** Cache responses for read-heavy endpoints.

**Acceptance Criteria:**
- [ ] In-memory cache for frequently accessed data
- [ ] Distributed cache support (Redis)
- [ ] Cache-Control headers respected
- [ ] Cache invalidation on data updates
- [ ] Configurable TTL per endpoint
- [ ] Cache hit/miss metrics

---

### MS-AG-5: API Documentation

**Description:** Auto-generated API documentation.

**Acceptance Criteria:**
- [ ] Swagger UI accessible at /swagger
- [ ] All endpoints documented
- [ ] Request/response schemas defined
- [ ] Authentication requirements documented
- [ ] Example requests/responses provided
- [ ] Versioning visible in documentation

---

## 2. Mission Management Service

**Domain:** Mission lifecycle, configuration, and metadata management.

### MS-MM-1: Create Mission

**Description:** Create a new mission with basic metadata.

**Acceptance Criteria:**
- [ ] Mission name required (unique per user)
- [ ] Mission description optional
- [ ] Mission type (e.g., LEO, GEO, Interplanetary)
- [ ] Start epoch (date/time) required
- [ ] End epoch optional
- [ ] Created timestamp auto-generated
- [ ] Mission ID (GUID) auto-generated
- [ ] Owner/creator user ID recorded
- [ ] Mission status set to "Draft"
- [ ] MissionCreatedEvent published to event bus
- [ ] REST API: POST /v1/missions
- [ ] Returns HTTP 201 with mission ID

---

### MS-MM-2: Update Mission

**Description:** Update mission metadata.

**Acceptance Criteria:**
- [ ] Mission name updatable
- [ ] Mission description updatable
- [ ] Epochs updatable
- [ ] Only mission owner can update
- [ ] Updated timestamp auto-generated
- [ ] MissionUpdatedEvent published
- [ ] REST API: PUT /v1/missions/{id}
- [ ] Returns HTTP 200 with updated mission
- [ ] Returns HTTP 404 if mission not found
- [ ] Returns HTTP 403 if not owner

---

### MS-MM-3: Delete Mission

**Description:** Soft delete a mission.

**Acceptance Criteria:**
- [ ] Soft delete (mark as deleted, don't remove from DB)
- [ ] Only mission owner can delete
- [ ] All associated data marked for deletion
- [ ] MissionDeletedEvent published
- [ ] REST API: DELETE /v1/missions/{id}
- [ ] Returns HTTP 204 No Content
- [ ] Returns HTTP 404 if not found
- [ ] Returns HTTP 403 if not owner

---

### MS-MM-4: Get Mission

**Description:** Retrieve mission details.

**Acceptance Criteria:**
- [ ] Retrieve mission by ID
- [ ] Only owner or shared users can view
- [ ] REST API: GET /v1/missions/{id}
- [ ] Returns HTTP 200 with mission data
- [ ] Returns HTTP 404 if not found
- [ ] Returns HTTP 403 if no access

---

### MS-MM-5: List Missions

**Description:** List all missions for current user.

**Acceptance Criteria:**
- [ ] Paginated results (default 20 per page)
- [ ] Filter by status (Draft, Active, Completed, Archived)
- [ ] Sort by name, created date, updated date
- [ ] Search by name (partial match)
- [ ] Only user's missions returned
- [ ] REST API: GET /v1/missions?page=1&size=20&status=Active
- [ ] Returns HTTP 200 with mission list

---

### MS-MM-6: Mission Status Management

**Description:** Manage mission lifecycle status.

**Acceptance Criteria:**
- [ ] Status transitions: Draft → Active → Completed → Archived
- [ ] Invalid transitions rejected
- [ ] MissionStatusChangedEvent published
- [ ] REST API: PATCH /v1/missions/{id}/status
- [ ] Audit trail of status changes

---

### MS-MM-7: Mission Sharing

**Description:** Share missions with other users.

**Acceptance Criteria:**
- [ ] Owner can share with specific users
- [ ] Read-only or read-write permissions
- [ ] Shared users can view/edit based on permissions
- [ ] Revoke access capability
- [ ] MissionSharedEvent published
- [ ] REST API: POST /v1/missions/{id}/share

---

### MS-MM-8: Mission Cloning

**Description:** Clone/duplicate an existing mission.

**Acceptance Criteria:**
- [ ] Clone mission metadata
- [ ] Clone associated spacecraft
- [ ] Clone maneuvers
- [ ] New mission ID generated
- [ ] Cloned mission owned by current user
- [ ] MissionClonedEvent published
- [ ] REST API: POST /v1/missions/{id}/clone

---

### MS-MM-9: Mission Export/Import

**Description:** Export mission to file format and import.

**Acceptance Criteria:**
- [ ] Export to JSON format
- [ ] Export to GMAT script format (compatibility)
- [ ] Import from JSON
- [ ] Import from GMAT script (best effort)
- [ ] Validation on import
- [ ] REST API: POST /v1/missions/export, POST /v1/missions/import

---

## 3. Spacecraft Service

**Domain:** Spacecraft definitions, hardware configurations, and properties.

### MS-SC-1: Create Spacecraft

**Description:** Define a new spacecraft.

**Acceptance Criteria:**
- [ ] Spacecraft name required (unique per mission)
- [ ] Associated with mission ID
- [ ] Dry mass (kg) required
- [ ] Fuel mass (kg) required
- [ ] Coefficient of drag (Cd) required
- [ ] Drag area (m²) required
- [ ] Solar radiation pressure area (m²) required
- [ ] Reflectivity coefficient required
- [ ] Initial epoch required
- [ ] Initial state vector required (position, velocity)
- [ ] Coordinate system specified
- [ ] SpacecraftCreatedEvent published
- [ ] REST API: POST /v1/spacecraft
- [ ] Returns HTTP 201 with spacecraft ID

---

### MS-SC-2: Update Spacecraft

**Description:** Update spacecraft properties.

**Acceptance Criteria:**
- [ ] All properties updatable except ID
- [ ] Validation for physical constraints (mass > 0)
- [ ] SpacecraftUpdatedEvent published
- [ ] REST API: PUT /v1/spacecraft/{id}
- [ ] Returns HTTP 200

---

### MS-SC-3: Delete Spacecraft

**Description:** Remove spacecraft from mission.

**Acceptance Criteria:**
- [ ] Soft delete
- [ ] Check for dependencies (maneuvers, etc.)
- [ ] Warn if dependencies exist
- [ ] SpacecraftDeletedEvent published
- [ ] REST API: DELETE /v1/spacecraft/{id}
- [ ] Returns HTTP 204

---

### MS-SC-4: Spacecraft State History

**Description:** Track state vector changes over time.

**Acceptance Criteria:**
- [ ] Store state vector at each epoch
- [ ] Query state at specific epoch
- [ ] Interpolation for intermediate epochs
- [ ] REST API: GET /v1/spacecraft/{id}/state?epoch={datetime}
- [ ] Event sourcing for state changes

---

### MS-SC-5: Fuel Management

**Description:** Track fuel consumption.

**Acceptance Criteria:**
- [ ] Initial fuel mass set on creation
- [ ] Fuel consumption events from maneuvers
- [ ] Current fuel mass queryable
- [ ] Fuel depletion warnings
- [ ] FuelConsumedEvent subscribed
- [ ] REST API: GET /v1/spacecraft/{id}/fuel

---

### MS-SC-6: Spacecraft Hardware Configuration

**Description:** Define hardware components (thrusters, tanks, solar panels).

**Acceptance Criteria:**
- [ ] Thruster definitions (Isp, thrust, fuel type)
- [ ] Fuel tank definitions (capacity, pressure)
- [ ] Solar panel definitions (area, efficiency)
- [ ] Battery definitions (capacity)
- [ ] Power subsystem model
- [ ] HardwareConfiguredEvent published
- [ ] REST API: POST /v1/spacecraft/{id}/hardware

---

### MS-SC-7: Attitude Definition

**Description:** Spacecraft attitude (orientation) configuration.

**Acceptance Criteria:**
- [ ] Attitude mode (nadir-pointing, sun-pointing, inertial, etc.)
- [ ] Quaternion or Euler angles
- [ ] Spin rate
- [ ] AttitudeChangedEvent published
- [ ] REST API: PUT /v1/spacecraft/{id}/attitude

---

### MS-SC-8: Spacecraft Validation

**Description:** Validate spacecraft configuration.

**Acceptance Criteria:**
- [ ] Mass budget validation (dry + fuel = total)
- [ ] Center of mass calculation
- [ ] Moment of inertia tensor
- [ ] Power budget validation
- [ ] Warnings for unrealistic values
- [ ] REST API: POST /v1/spacecraft/{id}/validate

---

## 4. Propagation Service

**Domain:** Orbit and trajectory propagation using numerical integration.

### MS-PR-1: Propagate Orbit

**Description:** Propagate spacecraft state from initial epoch to final epoch.

**Acceptance Criteria:**
- [ ] Spacecraft ID required
- [ ] Start epoch required
- [ ] End epoch required
- [ ] Propagator type selectable (RK45, RK89, Adams-Bashforth)
- [ ] Step size configurable
- [ ] Tolerance configurable
- [ ] Force model IDs specified
- [ ] Output state vectors at specified intervals
- [ ] PropagationRequestedEvent published
- [ ] PropagationCompletedEvent published
- [ ] REST API: POST /v1/propagation/propagate
- [ ] Returns HTTP 202 Accepted (async operation)
- [ ] Job ID returned for status tracking

---

### MS-PR-2: Query Propagation Status

**Description:** Check status of propagation job.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Status: Queued, Running, Completed, Failed
- [ ] Progress percentage
- [ ] Estimated time remaining
- [ ] Error message if failed
- [ ] REST API: GET /v1/propagation/jobs/{jobId}
- [ ] Returns HTTP 200

---

### MS-PR-3: Retrieve Propagation Results

**Description:** Get propagated state vectors.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Paginated results
- [ ] Filter by epoch range
- [ ] Output format: JSON, CSV
- [ ] Includes position (x, y, z) and velocity (vx, vy, vz)
- [ ] Coordinate system specified
- [ ] REST API: GET /v1/propagation/jobs/{jobId}/results
- [ ] Returns HTTP 200

---

### MS-PR-4: Numerical Integrator Selection

**Description:** Support multiple propagation methods.

**Acceptance Criteria:**
- [ ] Runge-Kutta 4th order (RK4)
- [ ] Runge-Kutta-Fehlberg 4-5 (RK45) with adaptive step
- [ ] Runge-Kutta 8-9 (RK89) for high precision
- [ ] Adams-Bashforth-Moulton (multi-step)
- [ ] Configurable error tolerance
- [ ] Automatic step size adjustment

---

### MS-PR-5: Event Detection

**Description:** Detect orbital events during propagation.

**Acceptance Criteria:**
- [ ] Apoapsis/periapsis detection
- [ ] Ascending/descending node crossing
- [ ] Altitude threshold crossing
- [ ] Eclipse entry/exit
- [ ] User-defined event functions
- [ ] Event timestamps recorded
- [ ] EventDetectedEvent published

---

### MS-PR-6: State Transition Matrix

**Description:** Compute state transition matrix for orbit determination.

**Acceptance Criteria:**
- [ ] 6x6 STM computed during propagation
- [ ] Partials with respect to initial state
- [ ] Used for covariance propagation
- [ ] Optional computation (performance overhead)

---

### MS-PR-7: Two-Body Propagation

**Description:** Fast analytical two-body propagation.

**Acceptance Criteria:**
- [ ] Keplerian elements propagation
- [ ] No perturbations
- [ ] Closed-form solution (fast)
- [ ] Useful for quick estimates
- [ ] REST API: POST /v1/propagation/two-body

---

### MS-PR-8: Parallel Propagation

**Description:** Propagate multiple spacecraft in parallel.

**Acceptance Criteria:**
- [ ] Accept array of spacecraft IDs
- [ ] Parallel execution using async/await
- [ ] Independent failure handling
- [ ] Consolidated results
- [ ] Performance metrics

---

## 5. Force Model Service

**Domain:** Environmental force calculations (gravity, drag, SRP, etc.).

### MS-FM-1: Gravity Model

**Description:** Compute gravitational acceleration.

**Acceptance Criteria:**
- [ ] Point mass gravity (two-body)
- [ ] Spherical harmonics (non-spherical Earth)
- [ ] Degree and order selectable (e.g., 20x20, 70x70)
- [ ] Multiple gravity models (JGM-3, EGM-96, GGM-03)
- [ ] Third-body gravity (Moon, Sun, planets)
- [ ] Acceleration vector output (ax, ay, az)
- [ ] REST API: POST /v1/forcemodel/gravity
- [ ] Configurable per mission

---

### MS-FM-2: Atmospheric Drag

**Description:** Compute atmospheric drag force.

**Acceptance Criteria:**
- [ ] Atmospheric density models (Exponential, Jacchia-Roberts, NRLMSISE-00)
- [ ] Altitude-dependent density
- [ ] Solar flux and geomagnetic index inputs
- [ ] Drag coefficient (Cd) from spacecraft
- [ ] Drag area from spacecraft
- [ ] Velocity relative to atmosphere
- [ ] Acceleration vector output
- [ ] REST API: POST /v1/forcemodel/drag

---

### MS-FM-3: Solar Radiation Pressure

**Description:** Compute SRP force.

**Acceptance Criteria:**
- [ ] Solar flux constant
- [ ] Distance from Sun
- [ ] Shadow function (umbra, penumbra)
- [ ] Reflectivity coefficient from spacecraft
- [ ] SRP area from spacecraft
- [ ] Acceleration vector output
- [ ] REST API: POST /v1/forcemodel/srp

---

### MS-FM-4: Relativistic Effects

**Description:** General relativity corrections (optional, high-precision).

**Acceptance Criteria:**
- [ ] Schwarzschild term
- [ ] Lense-Thirring effect
- [ ] De Sitter precession
- [ ] Small correction (~1e-10 m/s²)
- [ ] Configurable on/off

---

### MS-FM-5: Force Model Configuration

**Description:** Configure which forces are active for a mission.

**Acceptance Criteria:**
- [ ] Enable/disable individual force types
- [ ] Priority/order of force computations
- [ ] Validation of force model compatibility
- [ ] ForceModelConfiguredEvent published
- [ ] REST API: PUT /v1/forcemodel/config/{missionId}

---

### MS-FM-6: Third-Body Gravity

**Description:** Gravitational perturbations from celestial bodies.

**Acceptance Criteria:**
- [ ] Moon gravity
- [ ] Sun gravity
- [ ] Planetary gravity (Venus, Mars, Jupiter, etc.)
- [ ] Body position from Ephemeris Service
- [ ] Configurable bodies
- [ ] Point mass approximation

---

### MS-FM-7: Custom Force Models

**Description:** User-defined force models.

**Acceptance Criteria:**
- [ ] Plugin architecture for custom forces
- [ ] User-supplied acceleration function
- [ ] Input: state vector, epoch
- [ ] Output: acceleration vector
- [ ] Validation and sandboxing

---

## 6. Maneuver Service

**Domain:** Maneuver planning, execution, and optimization.

### MS-MN-1: Impulsive Burn

**Description:** Instantaneous velocity change (delta-V).

**Acceptance Criteria:**
- [ ] Burn epoch required
- [ ] Delta-V vector (Vx, Vy, Vz) required
- [ ] Coordinate system specified (VNB, LVLH, inertial)
- [ ] Fuel consumption calculated (Tsiolkovsky equation)
- [ ] Spacecraft fuel updated
- [ ] State vector discontinuity at burn epoch
- [ ] ManeuverCreatedEvent published
- [ ] REST API: POST /v1/maneuvers/impulsive
- [ ] Returns HTTP 201

---

### MS-MN-2: Finite Burn

**Description:** Continuous thrust over duration.

**Acceptance Criteria:**
- [ ] Burn start epoch required
- [ ] Burn duration required
- [ ] Thrust magnitude (Newtons) required
- [ ] Thrust direction (unit vector) required
- [ ] Mass flow rate calculated from Isp
- [ ] Fuel consumption integrated over duration
- [ ] State propagated during burn
- [ ] ManeuverCreatedEvent published
- [ ] REST API: POST /v1/maneuvers/finite
- [ ] Returns HTTP 201

---

### MS-MN-3: Maneuver Optimization

**Description:** Optimize maneuver parameters to achieve target.

**Acceptance Criteria:**
- [ ] Target orbit elements specified
- [ ] Cost function: minimize delta-V, time, fuel
- [ ] Constraints: fuel limit, time window
- [ ] Optimization algorithm (gradient descent, genetic algorithm)
- [ ] Initial guess provided or auto-generated
- [ ] Convergence tolerance
- [ ] Optimal maneuver parameters returned
- [ ] ManeuverOptimizedEvent published
- [ ] REST API: POST /v1/maneuvers/optimize
- [ ] Returns HTTP 202 (async)

---

### MS-MN-4: Hohmann Transfer

**Description:** Compute Hohmann transfer between circular orbits.

**Acceptance Criteria:**
- [ ] Initial orbit radius required
- [ ] Final orbit radius required
- [ ] Two-impulse maneuver
- [ ] Delta-V for each burn calculated
- [ ] Transfer time calculated
- [ ] Total delta-V returned
- [ ] REST API: POST /v1/maneuvers/hohmann

---

### MS-MN-5: Bi-Elliptic Transfer

**Description:** Three-impulse transfer (more efficient for large radius changes).

**Acceptance Criteria:**
- [ ] Initial orbit radius
- [ ] Final orbit radius
- [ ] Intermediate apoapsis radius
- [ ] Three burns calculated
- [ ] Total delta-V compared to Hohmann
- [ ] Transfer time calculated
- [ ] REST API: POST /v1/maneuvers/bi-elliptic

---

### MS-MN-6: Plane Change Maneuver

**Description:** Change orbital inclination.

**Acceptance Criteria:**
- [ ] Target inclination required
- [ ] Burn at ascending/descending node
- [ ] Delta-V calculated
- [ ] Combined with Hohmann if desired
- [ ] REST API: POST /v1/maneuvers/plane-change

---

### MS-MN-7: Rendezvous Planning

**Description:** Compute maneuvers for spacecraft rendezvous.

**Acceptance Criteria:**
- [ ] Target spacecraft ID
- [ ] Rendezvous epoch
- [ ] Phasing orbits
- [ ] Multiple-impulse sequence
- [ ] Relative state vectors
- [ ] Safety constraints (collision avoidance)
- [ ] REST API: POST /v1/maneuvers/rendezvous

---

### MS-MN-8: Station Keeping

**Description:** Maintain spacecraft in desired orbit.

**Acceptance Criteria:**
- [ ] Target orbit elements
- [ ] Tolerance bounds
- [ ] Periodic delta-V corrections
- [ ] Fuel budget tracking
- [ ] Automated or manual triggers
- [ ] REST API: POST /v1/maneuvers/station-keeping

---

### MS-MN-9: List Maneuvers

**Description:** List all maneuvers for a mission/spacecraft.

**Acceptance Criteria:**
- [ ] Filter by spacecraft ID
- [ ] Filter by mission ID
- [ ] Sort by epoch
- [ ] Paginated results
- [ ] REST API: GET /v1/maneuvers?spacecraftId={id}

---

### MS-MN-10: Delete Maneuver

**Description:** Remove a planned maneuver.

**Acceptance Criteria:**
- [ ] Maneuver ID required
- [ ] Soft delete
- [ ] ManeuverDeletedEvent published
- [ ] Fuel restored if not yet executed
- [ ] REST API: DELETE /v1/maneuvers/{id}
- [ ] Returns HTTP 204

---

## 7. Coordinate System Service

**Domain:** Reference frame definitions and transformations.

### MS-CS-1: Define Coordinate System

**Description:** Create coordinate system definition.

**Acceptance Criteria:**
- [ ] System name (e.g., ECI J2000, ECEF, Moon-Centered)
- [ ] Central body
- [ ] Axes definition
- [ ] Origin definition
- [ ] Epoch (for inertial systems)
- [ ] CoordinateSystemCreatedEvent published
- [ ] REST API: POST /v1/coordinates/systems
- [ ] Returns HTTP 201

---

### MS-CS-2: Transform State Vector

**Description:** Convert state vector between coordinate systems.

**Acceptance Criteria:**
- [ ] Source coordinate system ID
- [ ] Target coordinate system ID
- [ ] Input state vector (position, velocity)
- [ ] Epoch required
- [ ] Output state vector
- [ ] Transformation matrix logged for audit
- [ ] REST API: POST /v1/coordinates/transform
- [ ] Returns HTTP 200 with transformed state

---

### MS-CS-3: ECI to ECEF Transformation

**Description:** Inertial to Earth-Fixed transformation.

**Acceptance Criteria:**
- [ ] Epoch required (for Earth rotation)
- [ ] Precession and nutation applied
- [ ] Polar motion (IERS data)
- [ ] UT1-UTC correction
- [ ] Accurate to < 1 meter
- [ ] REST API: POST /v1/coordinates/eci-to-ecef

---

### MS-CS-4: ECEF to Geodetic

**Description:** Cartesian to Latitude/Longitude/Altitude.

**Acceptance Criteria:**
- [ ] Input: ECEF (x, y, z)
- [ ] Output: Latitude (deg), Longitude (deg), Altitude (m)
- [ ] Ellipsoid model (WGS-84)
- [ ] Iterative algorithm for altitude
- [ ] REST API: POST /v1/coordinates/ecef-to-geodetic

---

### MS-CS-5: Geodetic to ECEF

**Description:** Lat/Lon/Alt to Cartesian.

**Acceptance Criteria:**
- [ ] Input: Latitude, Longitude, Altitude
- [ ] Output: ECEF (x, y, z)
- [ ] WGS-84 ellipsoid
- [ ] REST API: POST /v1/coordinates/geodetic-to-ecef

---

### MS-CS-6: Body-Fixed Frames

**Description:** Spacecraft-centered coordinate systems.

**Acceptance Criteria:**
- [ ] VNB (Velocity-Normal-Binormal)
- [ ] LVLH (Local Vertical Local Horizontal)
- [ ] RSW (Radial-Transverse-Normal)
- [ ] Transformation from inertial frame
- [ ] Used for maneuver planning

---

### MS-CS-7: Keplerian Elements Conversion

**Description:** Convert state vector to/from Keplerian elements.

**Acceptance Criteria:**
- [ ] State vector → (a, e, i, Ω, ω, ν)
- [ ] Keplerian elements → State vector
- [ ] Handle circular/equatorial orbits
- [ ] Handle parabolic/hyperbolic orbits
- [ ] REST API: POST /v1/coordinates/state-to-keplerian
- [ ] REST API: POST /v1/coordinates/keplerian-to-state

---

### MS-CS-8: Mean Orbital Elements

**Description:** Compute mean (osculating-removed perturbations).

**Acceptance Criteria:**
- [ ] Input: Osculating elements
- [ ] Output: Mean elements
- [ ] Useful for TLE generation
- [ ] J2 perturbation removal

---

## 8. Ephemeris Service

**Domain:** Celestial body positions and orientations.

### MS-EP-1: Planet Position

**Description:** Get position/velocity of planets at epoch.

**Acceptance Criteria:**
- [ ] Planet name (Mercury, Venus, Earth, Mars, etc.)
- [ ] Epoch required
- [ ] Coordinate system (typically heliocentric or solar system barycenter)
- [ ] Position vector (x, y, z) in km
- [ ] Velocity vector (vx, vy, vz) in km/s
- [ ] REST API: GET /v1/ephemeris/planet/{name}?epoch={datetime}
- [ ] Returns HTTP 200

---

### MS-EP-2: Moon Position

**Description:** Get Moon position relative to Earth.

**Acceptance Criteria:**
- [ ] Epoch required
- [ ] High-precision model (lunar theory)
- [ ] Position and velocity
- [ ] ECI J2000 coordinate system
- [ ] REST API: GET /v1/ephemeris/moon?epoch={datetime}

---

### MS-EP-3: Sun Position

**Description:** Get Sun position.

**Acceptance Criteria:**
- [ ] Epoch required
- [ ] Position and velocity
- [ ] Solar system barycenter or geocentric
- [ ] REST API: GET /v1/ephemeris/sun?epoch={datetime}

---

### MS-EP-4: DE440/DE441 Integration

**Description:** Use JPL Development Ephemeris for high precision.

**Acceptance Criteria:**
- [ ] DE440 or DE441 data files
- [ ] Chebyshev polynomial interpolation
- [ ] Support all major bodies
- [ ] Accuracy: meters for planets, millimeters for Moon
- [ ] Data file caching

---

### MS-EP-5: Earth Orientation Parameters

**Description:** IERS Earth orientation data.

**Acceptance Criteria:**
- [ ] UT1-UTC
- [ ] Polar motion (x, y)
- [ ] Nutation corrections
- [ ] Daily updates from IERS
- [ ] Interpolation between data points
- [ ] REST API: GET /v1/ephemeris/eop?epoch={datetime}

---

### MS-EP-6: Solar Flux Data

**Description:** Solar activity indices.

**Acceptance Criteria:**
- [ ] F10.7 solar flux index
- [ ] Geomagnetic index (Ap, Kp)
- [ ] Historical data
- [ ] Forecast data
- [ ] Used by atmospheric drag models
- [ ] REST API: GET /v1/ephemeris/solar-flux?epoch={datetime}

---

### MS-EP-7: Star Catalog

**Description:** Star positions for attitude determination.

**Acceptance Criteria:**
- [ ] Bright star catalog (e.g., Hipparcos)
- [ ] Right ascension, declination
- [ ] Magnitude
- [ ] Proper motion
- [ ] Query by position/magnitude
- [ ] REST API: GET /v1/ephemeris/stars

---

### MS-EP-8: Ephemeris Caching

**Description:** Cache frequently requested ephemeris data.

**Acceptance Criteria:**
- [ ] In-memory cache for recent queries
- [ ] Distributed cache (Redis)
- [ ] TTL based on data type
- [ ] Cache hit metrics
- [ ] Pre-compute common epochs

---

## 9. Optimization Service

**Domain:** Trajectory optimization and parameter estimation.

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

## 10. Calculation Engine Service

**Domain:** High-performance numerical computation primitives.

### MS-CE-1: Matrix Operations

**Description:** Efficient matrix computations.

**Acceptance Criteria:**
- [ ] Matrix addition, subtraction
- [ ] Matrix multiplication
- [ ] Matrix transpose
- [ ] Matrix inverse (Gaussian elimination, LU decomposition)
- [ ] Determinant
- [ ] Eigenvalues and eigenvectors
- [ ] REST API: POST /v1/calc/matrix/{operation}
- [ ] Uses optimized BLAS library (MathNet.Numerics or Intel MKL)

---

### MS-CE-2: ODE Solver

**Description:** Ordinary differential equation solver.

**Acceptance Criteria:**
- [ ] Right-hand side function (derivatives)
- [ ] Initial conditions
- [ ] Time span
- [ ] Multiple integrators (RK4, RK45, RK89, Adams)
- [ ] Adaptive step sizing
- [ ] Dense output (interpolation)
- [ ] Event detection during integration
- [ ] REST API: POST /v1/calc/ode

---

### MS-CE-3: Root Finding

**Description:** Solve f(x) = 0.

**Acceptance Criteria:**
- [ ] Newton-Raphson method
- [ ] Bisection method
- [ ] Brent's method
- [ ] Multi-dimensional root finding
- [ ] Tolerance configurable
- [ ] REST API: POST /v1/calc/root

---

### MS-CE-4: Interpolation

**Description:** Interpolate data points.

**Acceptance Criteria:**
- [ ] Linear interpolation
- [ ] Cubic spline interpolation
- [ ] Hermite interpolation
- [ ] Chebyshev polynomial interpolation
- [ ] Extrapolation handling
- [ ] REST API: POST /v1/calc/interpolate

---

### MS-CE-5: Numerical Derivatives

**Description:** Compute derivatives numerically.

**Acceptance Criteria:**
- [ ] Finite differences (forward, backward, central)
- [ ] Partial derivatives
- [ ] Gradient vector
- [ ] Jacobian matrix
- [ ] Hessian matrix
- [ ] Automatic differentiation (optional)
- [ ] REST API: POST /v1/calc/derivative

---

### MS-CE-6: Quaternion Math

**Description:** Quaternion operations for attitude.

**Acceptance Criteria:**
- [ ] Quaternion multiplication
- [ ] Quaternion conjugate
- [ ] Quaternion to rotation matrix
- [ ] Rotation matrix to quaternion
- [ ] SLERP (spherical linear interpolation)
- [ ] REST API: POST /v1/calc/quaternion

---

### MS-CE-7: Unit Conversions

**Description:** Convert between units.

**Acceptance Criteria:**
- [ ] Length: m, km, AU, ft, mi, nmi
- [ ] Mass: kg, g, lbm
- [ ] Time: s, min, hr, day
- [ ] Angle: rad, deg, arcmin, arcsec
- [ ] Velocity: m/s, km/s, km/hr
- [ ] Force: N, lbf
- [ ] REST API: POST /v1/calc/convert

---

### MS-CE-8: Statistical Functions

**Description:** Statistical computations.

**Acceptance Criteria:**
- [ ] Mean, median, mode
- [ ] Standard deviation, variance
- [ ] Covariance matrix
- [ ] Correlation
- [ ] Histogram generation
- [ ] REST API: POST /v1/calc/stats

---

## 11. Visualization Service

**Domain:** 3D visualization, plotting, and rendering.

### MS-VZ-1: Orbit Plot Data

**Description:** Generate data for 3D orbit visualization.

**Acceptance Criteria:**
- [ ] Spacecraft ID required
- [ ] Epoch range required
- [ ] State vectors at regular intervals
- [ ] Orbit ground track
- [ ] Central body mesh (Earth, Moon, etc.)
- [ ] Output format: JSON with 3D coordinates
- [ ] REST API: GET /v1/visualization/orbit/{spacecraftId}
- [ ] Returns HTTP 200

---

### MS-VZ-2: Ground Track

**Description:** Lat/Lon ground track of orbit.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch range
- [ ] Sample interval
- [ ] Latitude/longitude pairs
- [ ] Ascending/descending indicators
- [ ] REST API: GET /v1/visualization/ground-track/{spacecraftId}

---

### MS-VZ-3: 3D Model Export

**Description:** Export 3D scene for external rendering.

**Acceptance Criteria:**
- [ ] Export to GLTF format
- [ ] Export to OBJ format
- [ ] Includes spacecraft models
- [ ] Includes orbit paths
- [ ] Includes celestial bodies
- [ ] REST API: GET /v1/visualization/export?format=gltf

---

### MS-VZ-4: Time-Series Plot Data

**Description:** Data for 2D plots (altitude, velocity, etc. vs time).

**Acceptance Criteria:**
- [ ] Parameter selection (altitude, velocity magnitude, orbital elements)
- [ ] Epoch range
- [ ] Sample interval
- [ ] Output format: JSON array [{epoch, value}]
- [ ] REST API: GET /v1/visualization/timeseries/{parameter}

---

### MS-VZ-5: Orbital Elements Plot

**Description:** Plot Keplerian elements over time.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Element selection (a, e, i, RAAN, AOP, TA)
- [ ] Epoch range
- [ ] Detect osculating variations
- [ ] REST API: GET /v1/visualization/elements/{spacecraftId}

---

### MS-VZ-6: Eclipse Plot

**Description:** Show eclipse periods (umbra/penumbra).

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch range
- [ ] Umbra entry/exit times
- [ ] Penumbra entry/exit times
- [ ] Sun visibility percentage
- [ ] REST API: GET /v1/visualization/eclipse/{spacecraftId}

---

### MS-VZ-7: 3D Attitude Visualization

**Description:** Spacecraft orientation over time.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Quaternion or Euler angles
- [ ] Body-fixed axes shown
- [ ] Sun vector, Earth vector
- [ ] REST API: GET /v1/visualization/attitude/{spacecraftId}

---

### MS-VZ-8: Conjunction Analysis Plot

**Description:** Visualize close approaches between objects.

**Acceptance Criteria:**
- [ ] Two spacecraft IDs
- [ ] Relative position over time
- [ ] Closest approach distance
- [ ] Probability of collision (if covariance available)
- [ ] REST API: GET /v1/visualization/conjunction

---

## 12. Reporting Service

**Domain:** Report generation and data export.

### MS-RP-1: Generate Mission Report

**Description:** Create comprehensive mission summary report.

**Acceptance Criteria:**
- [ ] Mission ID required
- [ ] Report format: PDF, HTML, Markdown
- [ ] Includes mission metadata
- [ ] Includes spacecraft details
- [ ] Includes maneuver summary
- [ ] Includes orbit plots
- [ ] Includes delta-V budget
- [ ] ReportGeneratedEvent published
- [ ] REST API: POST /v1/reports/mission/{missionId}?format=pdf
- [ ] Returns HTTP 202 (async)

---

### MS-RP-2: Export State Vectors

**Description:** Export propagated state vectors to file.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch range
- [ ] Format: CSV, JSON, XML
- [ ] Includes timestamp, position, velocity
- [ ] Coordinate system specified
- [ ] REST API: GET /v1/reports/export/states/{spacecraftId}?format=csv
- [ ] Returns HTTP 200 with file download

---

### MS-RP-3: Export Orbital Elements

**Description:** Export Keplerian elements to file.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch range
- [ ] Format: CSV, JSON
- [ ] Includes a, e, i, RAAN, AOP, TA
- [ ] REST API: GET /v1/reports/export/elements/{spacecraftId}

---

### MS-RP-4: TLE Generation

**Description:** Generate Two-Line Element sets.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch for TLE
- [ ] Mean orbital elements
- [ ] SGP4/SDP4 compatible format
- [ ] NORAD catalog number (if available)
- [ ] REST API: GET /v1/reports/tle/{spacecraftId}

---

### MS-RP-5: Delta-V Budget Report

**Description:** Summary of all maneuvers and fuel usage.

**Acceptance Criteria:**
- [ ] Mission ID
- [ ] List all maneuvers
- [ ] Delta-V per maneuver
- [ ] Total delta-V
- [ ] Fuel consumed per maneuver
- [ ] Remaining fuel
- [ ] Format: PDF, CSV
- [ ] REST API: GET /v1/reports/delta-v/{missionId}

---

### MS-RP-6: Event Timeline Report

**Description:** Chronological list of mission events.

**Acceptance Criteria:**
- [ ] Mission ID
- [ ] Events: maneuvers, eclipses, apsis, node crossings
- [ ] Sorted by epoch
- [ ] Format: PDF, HTML
- [ ] REST API: GET /v1/reports/timeline/{missionId}

---

### MS-RP-7: Conjunction Report

**Description:** Close approach analysis.

**Acceptance Criteria:**
- [ ] Spacecraft ID
- [ ] Epoch range
- [ ] Close approach events
- [ ] Miss distance
- [ ] Relative velocity
- [ ] Time of closest approach
- [ ] REST API: GET /v1/reports/conjunction/{spacecraftId}

---

### MS-RP-8: Custom Report Template

**Description:** User-defined report templates.

**Acceptance Criteria:**
- [ ] Template upload (e.g., Razor, Liquid)
- [ ] Variable substitution
- [ ] Query data from other services
- [ ] Output format: PDF, HTML
- [ ] REST API: POST /v1/reports/custom

---

### MS-RP-9: Scheduled Reports

**Description:** Automatic report generation on schedule.

**Acceptance Criteria:**
- [ ] Cron expression for schedule
- [ ] Report type and parameters
- [ ] Email delivery
- [ ] Webhook delivery
- [ ] Storage location (blob storage)
- [ ] REST API: POST /v1/reports/schedule

---

## 13. Script Execution Service

**Domain:** Mission script parsing, validation, and execution.

### MS-SE-1: Parse Script

**Description:** Parse GMAT-compatible mission script.

**Acceptance Criteria:**
- [ ] Script text input
- [ ] Syntax validation
- [ ] AST (Abstract Syntax Tree) generation
- [ ] Error reporting with line numbers
- [ ] REST API: POST /v1/scripts/parse
- [ ] Returns HTTP 200 with AST or errors

---

### MS-SE-2: Execute Script

**Description:** Run mission script asynchronously.

**Acceptance Criteria:**
- [ ] Script ID or inline script
- [ ] Validation before execution
- [ ] Asynchronous execution
- [ ] Job ID returned
- [ ] ScriptExecutionStartedEvent published
- [ ] ScriptExecutionCompletedEvent published
- [ ] REST API: POST /v1/scripts/execute
- [ ] Returns HTTP 202 with job ID

---

### MS-SE-3: Query Script Execution Status

**Description:** Check status of running script.

**Acceptance Criteria:**
- [ ] Job ID required
- [ ] Status: Queued, Running, Completed, Failed
- [ ] Progress percentage (if determinable)
- [ ] Current line/command being executed
- [ ] Output log
- [ ] REST API: GET /v1/scripts/jobs/{jobId}
- [ ] Returns HTTP 200

---

### MS-SE-4: Script Commands

**Description:** Support GMAT command syntax.

**Acceptance Criteria:**
- [ ] Create <object> (Spacecraft, Propagator, ForceModel, etc.)
- [ ] Set property (Spacecraft.FuelMass = 500)
- [ ] Propagate <spacecraft> to <epoch>
- [ ] Maneuver <spacecraft> with <burn>
- [ ] Optimize <parameters>
- [ ] If/Else/While control structures
- [ ] Report <parameters>
- [ ] Save <object>

---

### MS-SE-5: Script Variables

**Description:** Variable assignment and usage.

**Acceptance Criteria:**
- [ ] Variable declaration (var deltaV = 1500)
- [ ] Arithmetic operations (+, -, *, /)
- [ ] Variable substitution
- [ ] Type inference (number, string, object)

---

### MS-SE-6: Script Functions

**Description:** Built-in and user-defined functions.

**Acceptance Criteria:**
- [ ] Math functions (sin, cos, sqrt, etc.)
- [ ] Coordinate conversions
- [ ] Time functions (epoch arithmetic)
- [ ] User-defined functions (optional)

---

### MS-SE-7: Script Debugging

**Description:** Debug script execution.

**Acceptance Criteria:**
- [ ] Breakpoints
- [ ] Step through execution
- [ ] Inspect variable values
- [ ] Stack trace on error
- [ ] REST API: POST /v1/scripts/debug

---

### MS-SE-8: Script Library

**Description:** Save and reuse scripts.

**Acceptance Criteria:**
- [ ] Save script with name
- [ ] List user's scripts
- [ ] Load script by ID
- [ ] Version control (optional)
- [ ] Share scripts with other users
- [ ] REST API: POST /v1/scripts/library, GET /v1/scripts/library

---

### MS-SE-9: Script Validation

**Description:** Validate script before execution.

**Acceptance Criteria:**
- [ ] Syntax validation
- [ ] Semantic validation (objects exist, types correct)
- [ ] Resource validation (fuel available, etc.)
- [ ] Warnings for potential issues
- [ ] REST API: POST /v1/scripts/validate

---

## 14. Event Store Service

**Domain:** Event sourcing, audit trail, and event replay.

### MS-ES-1: Store Event

**Description:** Persist domain events.

**Acceptance Criteria:**
- [ ] Event type (e.g., MissionCreated, ManeuverExecuted)
- [ ] Aggregate ID (e.g., MissionId)
- [ ] Event data (JSON payload)
- [ ] Timestamp auto-generated
- [ ] Sequence number per aggregate
- [ ] User ID of actor
- [ ] Correlation ID for tracing
- [ ] Events immutable (append-only)
- [ ] REST API: POST /v1/events (internal use)
- [ ] Returns HTTP 201

---

### MS-ES-2: Query Events

**Description:** Retrieve event history.

**Acceptance Criteria:**
- [ ] Filter by aggregate ID
- [ ] Filter by event type
- [ ] Filter by date range
- [ ] Paginated results
- [ ] Ordered by sequence number
- [ ] REST API: GET /v1/events?aggregateId={id}&eventType={type}
- [ ] Returns HTTP 200

---

### MS-ES-3: Replay Events

**Description:** Rebuild aggregate state from events.

**Acceptance Criteria:**
- [ ] Aggregate ID required
- [ ] Replay all events in order
- [ ] Apply event handlers
- [ ] Reconstruct current state
- [ ] Used for debugging and recovery
- [ ] REST API: POST /v1/events/replay/{aggregateId}

---

### MS-ES-4: Event Subscriptions

**Description:** Services subscribe to specific event types.

**Acceptance Criteria:**
- [ ] Subscribe to event type
- [ ] Callback URL or message queue
- [ ] Delivery guarantee (at-least-once)
- [ ] Retry on failure
- [ ] Dead letter queue
- [ ] REST API: POST /v1/events/subscribe

---

### MS-ES-5: Event Snapshots

**Description:** Periodic snapshots to optimize replay.

**Acceptance Criteria:**
- [ ] Snapshot aggregate state at intervals
- [ ] Replay from snapshot + subsequent events
- [ ] Faster than full replay
- [ ] Configurable snapshot frequency
- [ ] REST API: POST /v1/events/snapshot/{aggregateId}

---

### MS-ES-6: Event Schema Versioning

**Description:** Handle evolving event schemas.

**Acceptance Criteria:**
- [ ] Event version in metadata
- [ ] Upcasting old events to new schema
- [ ] Backward compatibility
- [ ] Schema registry

---

### MS-ES-7: Audit Trail

**Description:** Audit log for compliance.

**Acceptance Criteria:**
- [ ] All state changes logged
- [ ] User actions logged
- [ ] Tamper-proof (cryptographic hash)
- [ ] Retention policy
- [ ] Searchable by user, date, action
- [ ] REST API: GET /v1/events/audit

---

## 15. Notification Service

**Domain:** User notifications and alerts.

### MS-NT-1: Send Email Notification

**Description:** Send email to user.

**Acceptance Criteria:**
- [ ] Recipient email address
- [ ] Subject and body
- [ ] HTML or plain text
- [ ] Template support
- [ ] Asynchronous sending
- [ ] Delivery status tracking
- [ ] Retry on failure
- [ ] REST API: POST /v1/notifications/email
- [ ] Returns HTTP 202

---

### MS-NT-2: Send Webhook Notification

**Description:** POST event to external URL.

**Acceptance Criteria:**
- [ ] Webhook URL
- [ ] Event payload (JSON)
- [ ] HTTP headers configurable
- [ ] Retry on failure (exponential backoff)
- [ ] Signature for security (HMAC)
- [ ] Timeout configurable
- [ ] REST API: POST /v1/notifications/webhook

---

### MS-NT-3: Real-Time Notifications

**Description:** Push notifications to connected clients.

**Acceptance Criteria:**
- [ ] WebSocket or Server-Sent Events (SSE)
- [ ] User-specific channels
- [ ] Notification types (info, warning, error, success)
- [ ] Client acknowledgment
- [ ] Fallback to polling if connection lost
- [ ] REST API: WebSocket /v1/notifications/stream

---

### MS-NT-4: Notification Preferences

**Description:** User configures notification settings.

**Acceptance Criteria:**
- [ ] Enable/disable notification types
- [ ] Email vs push vs webhook
- [ ] Digest mode (batch notifications)
- [ ] Quiet hours
- [ ] REST API: PUT /v1/notifications/preferences

---

### MS-NT-5: Alert Rules

**Description:** Trigger notifications on conditions.

**Acceptance Criteria:**
- [ ] Condition: fuel < threshold, altitude < threshold, etc.
- [ ] Action: send email, webhook, etc.
- [ ] Evaluation frequency
- [ ] Enable/disable rules
- [ ] REST API: POST /v1/notifications/alerts

---

### MS-NT-6: Notification History

**Description:** Log all sent notifications.

**Acceptance Criteria:**
- [ ] Timestamp
- [ ] Recipient
- [ ] Type (email, webhook, push)
- [ ] Status (sent, failed)
- [ ] Retry count
- [ ] Searchable
- [ ] REST API: GET /v1/notifications/history

---

## 16. Identity Service

**Domain:** Authentication, authorization, and user management.

### MS-ID-1: User Registration

**Description:** Create new user account.

**Acceptance Criteria:**
- [ ] Email address required (unique)
- [ ] Password required (min 8 chars, complexity rules)
- [ ] Password hashing (bcrypt or Argon2)
- [ ] Email verification token sent
- [ ] Account inactive until verified
- [ ] UserRegisteredEvent published
- [ ] REST API: POST /v1/identity/register
- [ ] Returns HTTP 201

---

### MS-ID-2: User Login

**Description:** Authenticate user and issue token.

**Acceptance Criteria:**
- [ ] Email and password required
- [ ] Password verification
- [ ] Account active check
- [ ] JWT token issued (access + refresh)
- [ ] Token expiration configurable (default 1 hour)
- [ ] Refresh token stored in secure cookie
- [ ] Login attempt logging
- [ ] Rate limiting to prevent brute force
- [ ] REST API: POST /v1/identity/login
- [ ] Returns HTTP 200 with token

---

### MS-ID-3: Token Refresh

**Description:** Refresh access token using refresh token.

**Acceptance Criteria:**
- [ ] Refresh token required
- [ ] Validate refresh token
- [ ] Issue new access token
- [ ] Optionally rotate refresh token
- [ ] REST API: POST /v1/identity/refresh
- [ ] Returns HTTP 200 with new access token

---

### MS-ID-4: User Logout

**Description:** Invalidate user session.

**Acceptance Criteria:**
- [ ] Invalidate refresh token
- [ ] Blacklist access token (optional, expensive)
- [ ] REST API: POST /v1/identity/logout
- [ ] Returns HTTP 204

---

### MS-ID-5: Password Reset

**Description:** Allow user to reset forgotten password.

**Acceptance Criteria:**
- [ ] User requests reset via email
- [ ] Reset token sent to email (expire in 1 hour)
- [ ] User submits new password with token
- [ ] Password updated
- [ ] All sessions invalidated
- [ ] REST API: POST /v1/identity/password-reset/request
- [ ] REST API: POST /v1/identity/password-reset/confirm

---

### MS-ID-6: Email Verification

**Description:** Verify user email address.

**Acceptance Criteria:**
- [ ] Verification token sent on registration
- [ ] User clicks link with token
- [ ] Account activated
- [ ] REST API: GET /v1/identity/verify-email?token={token}
- [ ] Returns HTTP 200

---

### MS-ID-7: Multi-Factor Authentication (MFA)

**Description:** Two-factor authentication.

**Acceptance Criteria:**
- [ ] TOTP (Time-based One-Time Password)
- [ ] QR code generation for authenticator app
- [ ] Backup codes generated
- [ ] MFA challenge on login
- [ ] REST API: POST /v1/identity/mfa/enable
- [ ] REST API: POST /v1/identity/mfa/verify

---

### MS-ID-8: Role-Based Access Control (RBAC)

**Description:** Assign roles to users.

**Acceptance Criteria:**
- [ ] Roles: Admin, User, ReadOnly, etc.
- [ ] Permissions per role
- [ ] User assigned one or more roles
- [ ] Claims in JWT token
- [ ] Policy-based authorization in services
- [ ] REST API: POST /v1/identity/roles
- [ ] REST API: PUT /v1/identity/users/{id}/roles

---

### MS-ID-9: User Profile Management

**Description:** Update user profile information.

**Acceptance Criteria:**
- [ ] Display name
- [ ] Avatar URL
- [ ] Preferences (timezone, units)
- [ ] REST API: GET /v1/identity/profile
- [ ] REST API: PUT /v1/identity/profile

---

### MS-ID-10: OAuth2 / OpenID Connect

**Description:** Third-party authentication (Google, Microsoft, GitHub).

**Acceptance Criteria:**
- [ ] OAuth2 authorization code flow
- [ ] OIDC support
- [ ] External provider login
- [ ] Link external accounts to user
- [ ] REST API: GET /v1/identity/oauth/{provider}

---

### MS-ID-11: API Key Management

**Description:** Generate API keys for programmatic access.

**Acceptance Criteria:**
- [ ] Generate API key
- [ ] Key hashing before storage
- [ ] Key expiration date
- [ ] Revoke key
- [ ] List user's API keys
- [ ] REST API: POST /v1/identity/api-keys
- [ ] REST API: DELETE /v1/identity/api-keys/{id}

---

### MS-ID-12: Audit User Actions

**Description:** Log authentication and authorization events.

**Acceptance Criteria:**
- [ ] Login/logout events
- [ ] Failed login attempts
- [ ] Password changes
- [ ] Role changes
- [ ] API key usage
- [ ] Searchable audit log
- [ ] REST API: GET /v1/identity/audit

---

## 17. Web Frontend Application

**Domain:** Browser-based user interface for mission planning and analysis.

### MS-WEB-1: User Authentication UI

**Description:** Login, registration, and profile management interface.

**Acceptance Criteria:**
- [ ] Login page with email/password
- [ ] Registration form with validation
- [ ] Password reset workflow
- [ ] MFA setup and verification UI
- [ ] User profile editing
- [ ] Session timeout warning
- [ ] Remember me functionality
- [ ] Social login buttons (Google, Microsoft, GitHub)
- [ ] Responsive design (desktop and tablet)
- [ ] Accessibility (WCAG 2.1 AA compliant)

---

### MS-WEB-2: Mission Dashboard

**Description:** Overview of user's missions and quick actions.

**Acceptance Criteria:**
- [ ] List of missions with cards/tiles
- [ ] Mission status indicators (Draft, Active, Completed)
- [ ] Quick actions: Create, Open, Clone, Delete
- [ ] Search and filter missions
- [ ] Sort by name, date, status
- [ ] Pagination or infinite scroll
- [ ] Mission statistics (spacecraft count, total delta-V, etc.)
- [ ] Recent activity feed
- [ ] Real-time updates via SignalR
- [ ] Empty state for new users

---

### MS-WEB-3: Mission Editor

**Description:** Create and edit mission configurations.

**Acceptance Criteria:**
- [ ] Mission metadata form (name, description, epochs)
- [ ] Form validation with error messages
- [ ] Auto-save draft (debounced)
- [ ] Undo/redo functionality
- [ ] Save and Cancel buttons
- [ ] Dirty state indicator (unsaved changes)
- [ ] Toast notifications for save success/failure
- [ ] Responsive layout
- [ ] Keyboard shortcuts (Ctrl+S to save)

---

### MS-WEB-4: Spacecraft Builder

**Description:** Define spacecraft properties visually.

**Acceptance Criteria:**
- [ ] Spacecraft property form (mass, drag area, etc.)
- [ ] Unit selection dropdowns (kg/lbm, m²/ft²)
- [ ] Real-time validation
- [ ] Initial state vector input (position, velocity)
- [ ] Coordinate system selector
- [ ] Hardware configuration tabs (thrusters, tanks, solar panels)
- [ ] 3D spacecraft preview (optional)
- [ ] Template library (common spacecraft types)
- [ ] Import/export spacecraft definition
- [ ] Help tooltips for complex fields

---

### MS-WEB-5: Orbit Visualization

**Description:** Interactive 3D orbit visualization in browser.

**Acceptance Criteria:**
- [ ] WebGL-based 3D rendering (Three.js or Babylon.js)
- [ ] Orbit path rendering
- [ ] Central body (Earth, Moon, etc.) with texture
- [ ] Camera controls (rotate, pan, zoom)
- [ ] Play/pause time animation
- [ ] Time slider for epoch selection
- [ ] Spacecraft position indicator
- [ ] Ground track overlay
- [ ] Coordinate axes display
- [ ] Screenshot/export image capability
- [ ] Performance: 60 FPS with 10+ orbits
- [ ] Loading indicator for data fetch

---

### MS-WEB-6: Maneuver Planner

**Description:** Plan and visualize maneuvers.

**Acceptance Criteria:**
- [ ] Add maneuver button with type selector (impulsive, finite)
- [ ] Maneuver parameter form (epoch, delta-V, direction)
- [ ] Visual maneuver editor on orbit plot
- [ ] Drag-and-drop maneuver epoch on timeline
- [ ] Before/after orbit preview
- [ ] Delta-V budget display
- [ ] Fuel consumption preview
- [ ] Maneuver validation warnings
- [ ] Optimization wizard (Hohmann, bi-elliptic, etc.)
- [ ] Maneuver list with edit/delete actions

---

### MS-WEB-7: Propagation Control

**Description:** Configure and run orbit propagation.

**Acceptance Criteria:**
- [ ] Propagation settings form (start/end epoch, step size)
- [ ] Propagator selection (RK45, RK89, etc.)
- [ ] Force model configuration (gravity, drag, SRP toggles)
- [ ] Run propagation button
- [ ] Progress bar with percentage
- [ ] Cancel propagation button
- [ ] Real-time status updates (SignalR)
- [ ] Error display if propagation fails
- [ ] Results preview table
- [ ] Download results button

---

### MS-WEB-8: Time-Series Charts

**Description:** Plot orbital parameters over time.

**Acceptance Criteria:**
- [ ] Chart library integration (Chart.js, Plotly, or Highcharts)
- [ ] Parameter selection (altitude, velocity, orbital elements)
- [ ] Multi-series support (compare multiple spacecraft)
- [ ] Zoom and pan on chart
- [ ] Hover tooltips with values
- [ ] Export chart as PNG/SVG
- [ ] Responsive chart sizing
- [ ] Toggle series visibility
- [ ] Time range selector

---

### MS-WEB-9: Ground Track Map

**Description:** 2D map showing ground track.

**Acceptance Criteria:**
- [ ] Map library (Leaflet or Mapbox)
- [ ] Ground track line rendering
- [ ] Ascending/descending node markers
- [ ] Spacecraft current position marker
- [ ] Time slider to move along track
- [ ] Map layers (satellite imagery, terrain)
- [ ] Latitude/longitude grid
- [ ] Zoom and pan controls
- [ ] Export map as image

---

### MS-WEB-10: Script Editor

**Description:** Write and execute GMAT scripts.

**Acceptance Criteria:**
- [ ] Code editor with syntax highlighting (Monaco Editor or CodeMirror)
- [ ] Line numbers
- [ ] Auto-completion for GMAT commands
- [ ] Syntax validation on-the-fly
- [ ] Error/warning annotations
- [ ] Run script button
- [ ] Console output panel
- [ ] Script save/load functionality
- [ ] Script templates library
- [ ] Find and replace
- [ ] Keyboard shortcuts (Ctrl+Enter to run)

---

### MS-WEB-11: Reporting Interface

**Description:** Generate and download reports.

**Acceptance Criteria:**
- [ ] Report type selector (Mission Summary, Delta-V Budget, etc.)
- [ ] Report parameter configuration
- [ ] Generate report button
- [ ] Preview report in browser
- [ ] Download report (PDF, CSV, HTML)
- [ ] Report history list
- [ ] Schedule report generation
- [ ] Email report option

---

### MS-WEB-12: Real-Time Notifications

**Description:** In-app notifications for events.

**Acceptance Criteria:**
- [ ] Notification bell icon with badge count
- [ ] Notification dropdown panel
- [ ] Notification types: success, info, warning, error
- [ ] Mark as read functionality
- [ ] Clear all notifications
- [ ] Click notification to navigate to relevant page
- [ ] Toast notifications for immediate alerts
- [ ] SignalR connection for real-time push
- [ ] Sound notification (toggleable)

---

### MS-WEB-13: Settings and Preferences

**Description:** User preferences and application settings.

**Acceptance Criteria:**
- [ ] Theme selection (light, dark, auto)
- [ ] Unit preferences (metric, imperial, custom)
- [ ] Default coordinate system
- [ ] Notification preferences
- [ ] Language selection (i18n support)
- [ ] Time zone setting
- [ ] Auto-save interval
- [ ] Accessibility settings
- [ ] Export/import user settings

---

### MS-WEB-14: Responsive Layout

**Description:** Adaptive layout for different screen sizes.

**Acceptance Criteria:**
- [ ] Desktop layout (1920x1080+)
- [ ] Laptop layout (1366x768+)
- [ ] Tablet layout (768x1024+)
- [ ] Breakpoints defined
- [ ] Collapsible sidebar/navigation
- [ ] Touch-friendly controls on tablets
- [ ] Minimum supported resolution: 1024x768
- [ ] No horizontal scrolling on supported sizes

---

### MS-WEB-15: Performance Optimization

**Description:** Fast loading and responsive UI.

**Acceptance Criteria:**
- [ ] Initial page load < 3 seconds
- [ ] Time to interactive < 5 seconds
- [ ] Code splitting for routes
- [ ] Lazy loading of heavy components
- [ ] Image optimization
- [ ] Caching strategy (service workers)
- [ ] Minified and bundled assets
- [ ] Lighthouse score > 90 (Performance)
- [ ] Virtual scrolling for long lists

---

### MS-WEB-16: Offline Support (Progressive Web App)

**Description:** Limited functionality without internet connection.

**Acceptance Criteria:**
- [ ] Service worker registered
- [ ] Offline page displayed when no connection
- [ ] Cached static assets
- [ ] Background sync for data submission
- [ ] IndexedDB for local data storage
- [ ] "Add to Home Screen" prompt
- [ ] Offline indicator in UI

---

### MS-WEB-17: Help and Documentation

**Description:** Integrated help system.

**Acceptance Criteria:**
- [ ] Help icon/button in header
- [ ] Contextual help tooltips
- [ ] Embedded documentation pages
- [ ] Search documentation
- [ ] Tutorial/walkthrough for new users
- [ ] Keyboard shortcuts reference
- [ ] Link to external docs/wiki
- [ ] Video tutorials (embedded)

---

### MS-WEB-18: Collaboration Features

**Description:** Share missions and collaborate with other users.

**Acceptance Criteria:**
- [ ] Share mission dialog with user email input
- [ ] Permission selection (read-only, edit)
- [ ] List of shared users per mission
- [ ] Revoke access button
- [ ] Shared with me missions section
- [ ] Activity log showing who changed what
- [ ] Comments on missions (optional)
- [ ] @mention notifications (optional)

---

## 18. Desktop Frontend Application

**Domain:** Native desktop application for advanced users and offline work.

### MS-DESK-1: Native Application Shell

**Description:** Desktop application framework and window management.

**Acceptance Criteria:**
- [ ] Native window with title bar, menu, status bar
- [ ] Multi-window support (optional)
- [ ] Window state persistence (size, position)
- [ ] System tray integration
- [ ] Native file dialogs
- [ ] Keyboard shortcuts (platform-specific)
- [ ] Drag-and-drop file support
- [ ] Application icon and branding
- [ ] Auto-update mechanism
- [ ] Splash screen on startup

---

### MS-DESK-2: Advanced 3D Visualization

**Description:** High-performance 3D rendering with GPU acceleration.

**Acceptance Criteria:**
- [ ] DirectX or OpenGL rendering
- [ ] 60+ FPS with complex scenes
- [ ] Support for 100+ spacecraft simultaneously
- [ ] Realistic lighting and shadows
- [ ] High-resolution planet textures
- [ ] Atmospheric effects (optional)
- [ ] Spacecraft 3D models (OBJ, FBX import)
- [ ] Camera modes (orbit, free, spacecraft-relative)
- [ ] Screenshot and video recording (MP4 export)
- [ ] VR support (optional, future)

---

### MS-DESK-3: Offline Mode

**Description:** Full functionality without internet connection.

**Acceptance Criteria:**
- [ ] Local database (SQLite or LiteDB)
- [ ] All mission data stored locally
- [ ] Propagation runs locally
- [ ] Sync to cloud when online
- [ ] Conflict resolution for sync
- [ ] Offline indicator in UI
- [ ] Queue API calls when offline
- [ ] Local ephemeris data cache

---

### MS-DESK-4: File System Integration

**Description:** Direct file operations with OS file system.

**Acceptance Criteria:**
- [ ] Open mission from file (*.ngmat format)
- [ ] Save mission to file
- [ ] Import GMAT scripts from file
- [ ] Export reports to file system
- [ ] Recent files list
- [ ] File associations (.ngmat files open in app)
- [ ] Drag-and-drop files to open
- [ ] File change watchers (optional)

---

### MS-DESK-5: Multi-Tab Interface

**Description:** Tabbed interface for multiple missions.

**Acceptance Criteria:**
- [ ] Tab control for multiple open missions
- [ ] Tab close button
- [ ] Tab reordering (drag-and-drop)
- [ ] Keyboard shortcuts (Ctrl+Tab, Ctrl+W)
- [ ] Unsaved changes indicator on tab
- [ ] Pin tab feature
- [ ] Tab context menu (close others, close all)

---

### MS-DESK-6: Docking Panels

**Description:** Customizable panel layout.

**Acceptance Criteria:**
- [ ] Dockable panels (properties, explorer, console)
- [ ] Drag-and-drop panel repositioning
- [ ] Floating panels
- [ ] Tabbed panel groups
- [ ] Panel auto-hide
- [ ] Save/restore layout
- [ ] Default layouts (Analysis, Visualization, Scripting)
- [ ] Reset to default layout

---

### MS-DESK-7: Data Grid for Large Datasets

**Description:** High-performance data grid for state vectors.

**Acceptance Criteria:**
- [ ] Virtual scrolling for millions of rows
- [ ] Column sorting and filtering
- [ ] Column reordering and resizing
- [ ] Export to CSV/Excel
- [ ] Copy cells to clipboard
- [ ] Cell formatting (units, precision)
- [ ] Frozen columns
- [ ] Search/find in grid

---

### MS-DESK-8: Advanced Plotting

**Description:** Publication-quality charts and plots.

**Acceptance Criteria:**
- [ ] 2D line plots
- [ ] 3D surface plots
- [ ] Scatter plots
- [ ] Contour plots
- [ ] Multiple axes
- [ ] Custom axis labels and titles
- [ ] Legend customization
- [ ] Export to PNG, SVG, PDF
- [ ] LaTeX rendering for equations (optional)
- [ ] Plot templates/styles

---

### MS-DESK-9: Batch Processing

**Description:** Run multiple missions/analyses in batch.

**Acceptance Criteria:**
- [ ] Batch job queue
- [ ] Add missions to batch
- [ ] Configure batch parameters
- [ ] Run batch with progress tracking
- [ ] Pause/resume batch
- [ ] Batch results aggregation
- [ ] Export batch results
- [ ] Parallel execution

---

### MS-DESK-10: Plugin System

**Description:** Extensibility through plugins.

**Acceptance Criteria:**
- [ ] Plugin discovery from folder
- [ ] Plugin metadata (name, version, author)
- [ ] Plugin enable/disable
- [ ] Plugin settings UI
- [ ] Plugin API for custom force models
- [ ] Plugin API for custom propagators
- [ ] Plugin API for custom UI panels
- [ ] Sandboxed plugin execution
- [ ] Plugin marketplace (future)

---

### MS-DESK-11: Performance Profiling Tools

**Description:** Built-in profiling and diagnostics.

**Acceptance Criteria:**
- [ ] CPU profiler
- [ ] Memory profiler
- [ ] Propagation timing breakdown
- [ ] API call latency monitoring
- [ ] Frame rate monitor for 3D view
- [ ] Export profiling data
- [ ] Diagnostics report generation

---

### MS-DESK-12: Command-Line Interface

**Description:** CLI for automation and scripting.

**Acceptance Criteria:**
- [ ] Run missions from command line
- [ ] Execute scripts from CLI
- [ ] Batch mode (no GUI)
- [ ] Output to stdout/file
- [ ] Exit codes for success/failure
- [ ] CLI documentation (--help)
- [ ] Environment variable configuration

---

### MS-DESK-13: Native Notifications

**Description:** OS-level notifications.

**Acceptance Criteria:**
- [ ] Windows toast notifications
- [ ] macOS notification center (if cross-platform)
- [ ] Linux desktop notifications (if cross-platform)
- [ ] Action buttons in notifications
- [ ] Notification history
- [ ] Respect OS do-not-disturb settings

---

### MS-DESK-14: Accessibility Features

**Description:** Accessibility for desktop users.

**Acceptance Criteria:**
- [ ] Screen reader support (Narrator, JAWS)
- [ ] Keyboard navigation for all features
- [ ] High contrast themes
- [ ] Font size adjustment
- [ ] Focus indicators
- [ ] WCAG 2.1 AA compliance
- [ ] Tab order logical

---

### MS-DESK-15: Cross-Platform Support

**Description:** Run on multiple operating systems (if .NET MAUI/Avalonia).

**Acceptance Criteria:**
- [ ] Windows 10+ support
- [ ] macOS 11+ support (if cross-platform)
- [ ] Linux support (Ubuntu, Fedora) (if cross-platform)
- [ ] Platform-specific features gracefully degrade
- [ ] Consistent UI across platforms
- [ ] Platform-specific installers (MSI, DMG, DEB)

---

### MS-DESK-16: Local Computation Engine

**Description:** Embedded high-performance computation without server.

**Acceptance Criteria:**
- [ ] Local propagation engine (no API calls)
- [ ] Multi-threaded computation
- [ ] GPU acceleration for compatible tasks (optional)
- [ ] Progress reporting to UI
- [ ] Cancellation support
- [ ] Resource throttling (CPU, memory limits)

---

### MS-DESK-17: Data Import/Export

**Description:** Import data from external sources.

**Acceptance Criteria:**
- [ ] Import TLE (Two-Line Elements)
- [ ] Import GMAT scripts
- [ ] Import CSV state vectors
- [ ] Import spacecraft definitions from JSON/XML
- [ ] Export mission to GMAT format
- [ ] Export to STK ephemeris format
- [ ] Export to custom formats (extensible)

---

### MS-DESK-18: Version Control Integration

**Description:** Integration with Git for mission versioning.

**Acceptance Criteria:**
- [ ] Git repository detection
- [ ] Commit changes from app
- [ ] View diff of mission changes
- [ ] Branch visualization
- [ ] Pull/push to remote
- [ ] Conflict resolution UI
- [ ] Commit history browser

---

## Summary

This requirements document outlines a comprehensive **Event-Driven Microservices Architecture** for **NGMAT**, a .NET reimplementation of NASA's GMAT.

### Architecture Components

- **21 Global Requirements** covering cross-cutting concerns (infrastructure, security, coding standards)
- **16 Backend Microservices** with REST APIs and event-driven communication
- **2 Frontend Applications:**
  - Web Application (Angular + Angular Material)
  - Desktop Application (WPF/Avalonia/.NET MAUI)
- **100+ Microservice-Specific Requirements** with Given-When-Then acceptance criteria
- **Adherence to coding guidelines** documented in [coding-guidelines.md](./coding-guidelines.md)

### Key Characteristics

- **Event-Driven:** Services communicate asynchronously via events
- **Scalable:** Each service can scale independently
- **Resilient:** Circuit breakers, retries, and fallback mechanisms
- **Observable:** Centralized logging, distributed tracing, metrics
- **Secure:** Authentication, authorization, encryption, audit trails
- **Extensible:** Plugin architecture for custom models and forces
- **Cloud-Native:** Containerized, orchestrated, cloud-agnostic

### Technology Stack Recommendations

**Backend:**
- **Language:** C# (.NET 8 or later)
- **Message Bus:** Redis Pub/Sub (production), UDP Multicast (local development)
- **Redis Client:** StackExchange.Redis
- **Message Serialization:** MessagePack with LZ4 compression
- **API Gateway:** Ocelot or YARP
- **Databases:** SQL Server (relational), MongoDB (documents), Redis (cache + pub/sub)
- **ORM:** Entity Framework Core
- **Container Orchestration:** Kubernetes or Azure Container Apps
- **Logging:** Serilog + Seq or ELK Stack
- **Tracing:** OpenTelemetry + Jaeger
- **Monitoring:** Prometheus + Grafana or Azure Monitor
- **CI/CD:** Azure DevOps or GitHub Actions
- **Code Quality:** StyleCop.Analyzers, Microsoft.CodeAnalysis.NetAnalyzers

**Frontend - Web:**
- **Framework:** Angular (latest stable version)
- **UI Library:** Angular Material (Material Design 3)
- **State Management:** RxJS (NOT NgRx)
- **3D Graphics:** Three.js or Babylon.js (WebGL)
- **Charts:** Chart.js, Plotly, or Highcharts
- **Maps:** Leaflet or Mapbox
- **Code Editor:** Monaco Editor or CodeMirror
- **Testing:** Jest (unit), Playwright (E2E)
- **Linting:** ESLint with @angular-eslint plugin

**Frontend - Desktop:**
- **Framework:** WPF, Avalonia UI, or .NET MAUI
- **3D Graphics:** DirectX or OpenGL (Silk.NET, Veldrid)
- **Charts:** OxyPlot, LiveCharts2, or ScottPlot
- **Data Grid:** DevExpress, Syncfusion, or custom
- **Local Database:** SQLite or LiteDB
- **Testing:** xUnit, NUnit

**Numerical Computation:**
- **Math Library:** MathNet.Numerics or Intel MKL
- **ODE Solvers:** Custom implementations or DifferentialEquations.NET

### Next Steps

1. **Architecture Design:** Create detailed architecture diagrams (C4 model)
2. **Technology Selection:** Finalize choices for databases, etc.
3. **Prototype:** Build spike for critical components (propagation, Redis Pub/Sub, UDP Multicast)
4. **Team Formation:** Assign teams to microservices
5. **Sprint Planning:** Break requirements into user stories and sprints
6. **Infrastructure Setup:** Kubernetes cluster, Redis Cluster, CI/CD pipelines
7. **Development:** Iterative development with continuous integration

---

**End of Requirements Document**
