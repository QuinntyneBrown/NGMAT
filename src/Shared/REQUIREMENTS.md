# Shared Library Requirements

## Overview

**Domain:** Cross-cutting infrastructure for messaging, events, and common abstractions.

The Shared Library provides the foundational messaging infrastructure and common types used by all microservices in the NGMAT system. This includes the event bus abstraction, message contracts, and serialization utilities.

---

## Requirements

### MS-SH-1: Event Bus Abstraction

**Description:** Provide a unified interface for publishing and subscribing to events across all microservices.

**Acceptance Criteria:**
- [ ] `IEventPublisher` interface for publishing events
- [ ] `IEventSubscriber` interface for subscribing to events
- [ ] `IEventBus` combined interface for full pub/sub capability
- [ ] Support for typed event handlers
- [ ] Support for wildcard/pattern subscriptions
- [ ] Async/await throughout
- [ ] Cancellation token support
- [ ] Event metadata (correlation ID, timestamp, source service)
- [ ] Dead letter queue abstraction for failed messages

---

### MS-SH-2: UDP Multicast Implementation

**Description:** Local development messaging implementation using UDP Multicast requiring no external dependencies.

**Acceptance Criteria:**
- [ ] Zero external dependencies (no Redis, no message broker)
- [ ] Multicast group configuration (IP, port)
- [ ] Automatic serialization/deserialization
- [ ] Message framing for large payloads
- [ ] Reliable delivery with acknowledgments (optional)
- [ ] Message deduplication
- [ ] Configurable TTL (Time To Live)
- [ ] Network interface selection
- [ ] Graceful handling of network errors
- [ ] Support for multiple subscribers on same machine

---

### MS-SH-3: Redis Pub/Sub Implementation

**Description:** Production messaging implementation using Redis Pub/Sub.

**Acceptance Criteria:**
- [ ] StackExchange.Redis client integration
- [ ] Connection pooling and multiplexing
- [ ] Automatic reconnection on failure
- [ ] Channel naming conventions
- [ ] Pattern-based subscriptions
- [ ] Message persistence option (Redis Streams fallback)
- [ ] Cluster support
- [ ] Sentinel support for high availability
- [ ] Connection string configuration
- [ ] Health check endpoint integration

---

### MS-SH-4: Message Serialization

**Description:** High-performance message serialization using MessagePack with compression.

**Acceptance Criteria:**
- [ ] MessagePack serialization for all events
- [ ] LZ4 compression for payload reduction
- [ ] Schema versioning support
- [ ] Backward compatibility for message evolution
- [ ] Custom formatter support
- [ ] Serialization benchmarks < 1ms for typical payloads
- [ ] Support for complex types (DateTime, Guid, enums)
- [ ] Null handling
- [ ] Collection serialization (arrays, lists, dictionaries)

---

### MS-SH-5: Event Contracts

**Description:** Shared event contracts (messages) used across all microservices.

**Acceptance Criteria:**
- [ ] Base `IEvent` interface with common properties
- [ ] `EventId` (Guid) for unique identification
- [ ] `Timestamp` (DateTimeOffset) for event time
- [ ] `CorrelationId` for request tracing
- [ ] `CausationId` for event chaining
- [ ] `UserId` for audit trails
- [ ] `Version` for schema versioning
- [ ] Domain-specific event base classes
- [ ] Event naming conventions documented

**Event Categories:**
- [ ] Mission events (MissionCreated, MissionUpdated, MissionDeleted)
- [ ] Spacecraft events (SpacecraftCreated, SpacecraftUpdated, StateVectorUpdated)
- [ ] Propagation events (PropagationStarted, PropagationCompleted, PropagationFailed)
- [ ] Maneuver events (ManeuverPlanned, ManeuverExecuted)
- [ ] Calculation events (CalculationRequested, CalculationCompleted)
- [ ] User events (UserLoggedIn, UserLoggedOut, PermissionChanged)
- [ ] System events (ServiceStarted, ServiceStopped, HealthCheckFailed)

---

### MS-SH-6: Configuration Abstractions

**Description:** Shared configuration models for messaging infrastructure.

**Acceptance Criteria:**
- [ ] `MessagingOptions` configuration class
- [ ] `RedisOptions` for Redis-specific settings
- [ ] `UdpMulticastOptions` for UDP-specific settings
- [ ] Environment-based configuration switching
- [ ] Validation of configuration on startup
- [ ] Sensible defaults for local development
- [ ] Configuration documentation

---

### MS-SH-7: Dependency Injection Extensions

**Description:** Extension methods for registering messaging services in DI container.

**Acceptance Criteria:**
- [ ] `AddEventBus()` extension method
- [ ] `AddUdpMulticastEventBus()` for local development
- [ ] `AddRedisEventBus()` for production
- [ ] `AddEventHandler<TEvent, THandler>()` for registering handlers
- [ ] Automatic handler discovery via assembly scanning
- [ ] Scoped vs Singleton lifetime configuration
- [ ] Integration with Microsoft.Extensions.DependencyInjection

---

### MS-SH-8: Observability

**Description:** Logging, metrics, and tracing for messaging infrastructure.

**Acceptance Criteria:**
- [ ] Structured logging for all publish/subscribe operations
- [ ] OpenTelemetry trace propagation through events
- [ ] Metrics: messages published, messages received, processing time
- [ ] Metrics: error counts, retry counts
- [ ] Correlation ID propagation
- [ ] Log levels configurable per channel/event type
- [ ] Integration with Serilog

---

### MS-SH-9: Error Handling

**Description:** Robust error handling and retry mechanisms.

**Acceptance Criteria:**
- [ ] Configurable retry policies (count, delay, backoff)
- [ ] Circuit breaker pattern for failing subscribers
- [ ] Dead letter handling for poison messages
- [ ] Exception logging with full context
- [ ] Graceful degradation when messaging unavailable
- [ ] Error events for monitoring
- [ ] Manual retry capability for dead letters

---

### MS-SH-10: Common Domain Types

**Description:** Shared domain types used across multiple microservices.

**Acceptance Criteria:**
- [ ] `MissionId` strongly-typed identifier
- [ ] `SpacecraftId` strongly-typed identifier
- [ ] `UserId` strongly-typed identifier
- [ ] `Result<T>` for operation results
- [ ] `Error` type for structured errors
- [ ] `PagedResult<T>` for pagination
- [ ] `DateTimeRange` for time spans
- [ ] Unit types (meters, seconds, radians, etc.)
- [ ] Coordinate types (Vector3D, Quaternion)

---

## Project Structure

```
src/Shared/
├── Shared.Messaging.Abstractions/    # Interfaces and base types
│   ├── IEventBus.cs
│   ├── IEventPublisher.cs
│   ├── IEventSubscriber.cs
│   ├── IEvent.cs
│   └── EventMetadata.cs
│
├── Shared.Messaging.UdpMulticast/    # UDP Multicast implementation
│   ├── UdpMulticastEventBus.cs
│   ├── UdpMulticastOptions.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── Shared.Messaging.Redis/           # Redis Pub/Sub implementation
│   ├── RedisEventBus.cs
│   ├── RedisOptions.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── Shared.Contracts/                 # Event contracts (messages)
│   ├── Events/
│   │   ├── MissionEvents.cs
│   │   ├── SpacecraftEvents.cs
│   │   ├── PropagationEvents.cs
│   │   └── ...
│   └── Commands/
│       └── ...
│
└── Shared.Domain/                    # Common domain types
    ├── Identifiers/
    ├── Results/
    └── Units/
```

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8 (LTS) |
| Serialization | MessagePack |
| Compression | LZ4 |
| Redis Client | StackExchange.Redis |
| UDP Multicast | System.Net.Sockets |
| DI | Microsoft.Extensions.DependencyInjection |
| Logging | Serilog |
| Tracing | OpenTelemetry |

> **Note:** All projects in the NGMAT solution target .NET 8 (Long-Term Support).

---

## Dependencies

- **No external service dependencies** for UDP Multicast implementation
- **Redis 7.0+** for Redis Pub/Sub implementation (production only)

---

## Consumers

All microservices depend on this shared library:
- Identity Service
- Event Store Service
- Calculation Engine Service
- Coordinate System Service
- Ephemeris Service
- Force Model Service
- Propagation Service
- Mission Management Service
- Spacecraft Service
- Maneuver Service
- Optimization Service
- Visualization Service
- Reporting Service
- Script Execution Service
- Notification Service
- API Gateway
- Web Interface
