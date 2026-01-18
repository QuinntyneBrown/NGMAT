# Shared Library Roadmap

## Overview

This roadmap outlines the implementation phases for the Shared Library, which provides the foundational messaging infrastructure and common types for all NGMAT microservices.

---

## Phase 1: Messaging Abstractions

**Goal:** Define interfaces and base types for the event bus.

### Milestone 1.1: Project Setup
- [ ] Create Shared.Messaging.Abstractions project
- [ ] Configure .NET 8 class library
- [ ] Add MessagePack NuGet package
- [ ] Add LZ4 compression package
- [ ] Set up project references

### Milestone 1.2: Core Interfaces (MS-SH-1)
- [ ] Define `IEvent` base interface
- [ ] Define `EventMetadata` class (CorrelationId, Timestamp, etc.)
- [ ] Define `IEventPublisher` interface
- [ ] Define `IEventSubscriber` interface
- [ ] Define `IEventBus` combined interface
- [ ] Define `IEventHandler<TEvent>` interface
- [ ] Create base `EventBase` abstract class

### Milestone 1.3: Serialization (MS-SH-4)
- [ ] Implement MessagePack serializer wrapper
- [ ] Add LZ4 compression support
- [ ] Create `IMessageSerializer` interface
- [ ] Implement schema versioning attributes
- [ ] Add unit tests for serialization

**Deliverables:**
- Shared.Messaging.Abstractions NuGet package
- Core interfaces for event bus
- MessagePack serialization with LZ4

---

## Phase 2: UDP Multicast Implementation

**Goal:** Local development messaging with zero external dependencies.

### Milestone 2.1: Project Setup
- [ ] Create Shared.Messaging.UdpMulticast project
- [ ] Reference Shared.Messaging.Abstractions
- [ ] Configure UDP socket options

### Milestone 2.2: UDP Multicast Event Bus (MS-SH-2)
- [ ] Implement `UdpMulticastEventBus` class
- [ ] Configure multicast group (default: 239.0.0.1:5000)
- [ ] Implement message framing protocol
- [ ] Handle message fragmentation for large payloads
- [ ] Implement publish functionality
- [ ] Implement subscribe functionality
- [ ] Add message deduplication (sliding window)
- [ ] Handle network interface selection

### Milestone 2.3: DI Extensions (MS-SH-7)
- [ ] Create `AddUdpMulticastEventBus()` extension
- [ ] Create `UdpMulticastOptions` configuration class
- [ ] Add options validation
- [ ] Implement handler registration

### Milestone 2.4: Testing
- [ ] Unit tests for serialization
- [ ] Integration tests for pub/sub
- [ ] Multi-subscriber tests
- [ ] Network failure tests

**Deliverables:**
- Shared.Messaging.UdpMulticast NuGet package
- Zero-dependency local development messaging
- Full test coverage

---

## Phase 3: Redis Pub/Sub Implementation

**Goal:** Production-ready messaging using Redis.

### Milestone 3.1: Project Setup
- [ ] Create Shared.Messaging.Redis project
- [ ] Add StackExchange.Redis NuGet package
- [ ] Reference Shared.Messaging.Abstractions

### Milestone 3.2: Redis Event Bus (MS-SH-3)
- [ ] Implement `RedisEventBus` class
- [ ] Configure connection multiplexer
- [ ] Implement channel naming conventions
- [ ] Implement publish functionality
- [ ] Implement subscribe functionality
- [ ] Add pattern-based subscriptions
- [ ] Handle reconnection logic
- [ ] Add connection health checks

### Milestone 3.3: Advanced Features
- [ ] Implement Redis Streams fallback for persistence
- [ ] Add cluster support
- [ ] Add Sentinel support
- [ ] Implement connection pooling

### Milestone 3.4: DI Extensions (MS-SH-7)
- [ ] Create `AddRedisEventBus()` extension
- [ ] Create `RedisOptions` configuration class
- [ ] Add connection string parsing
- [ ] Implement handler registration

### Milestone 3.5: Testing
- [ ] Unit tests with mocked Redis
- [ ] Integration tests with Redis container
- [ ] Reconnection tests
- [ ] Performance benchmarks

**Deliverables:**
- Shared.Messaging.Redis NuGet package
- Production-ready Redis messaging
- High availability support

---

## Phase 4: Event Contracts

**Goal:** Define all shared event types.

### Milestone 4.1: Project Setup
- [ ] Create Shared.Contracts project
- [ ] Reference Shared.Messaging.Abstractions
- [ ] Set up MessagePack attributes

### Milestone 4.2: Event Definitions (MS-SH-5)
- [ ] Define Mission events
  - [ ] MissionCreated
  - [ ] MissionUpdated
  - [ ] MissionDeleted
  - [ ] MissionStateChanged
- [ ] Define Spacecraft events
  - [ ] SpacecraftCreated
  - [ ] SpacecraftUpdated
  - [ ] SpacecraftDeleted
  - [ ] StateVectorUpdated
- [ ] Define Propagation events
  - [ ] PropagationRequested
  - [ ] PropagationStarted
  - [ ] PropagationProgress
  - [ ] PropagationCompleted
  - [ ] PropagationFailed
  - [ ] PropagationCancelled
- [ ] Define Maneuver events
  - [ ] ManeuverPlanned
  - [ ] ManeuverUpdated
  - [ ] ManeuverExecuted
  - [ ] ManeuverDeleted
- [ ] Define Calculation events
  - [ ] CalculationRequested
  - [ ] CalculationCompleted
  - [ ] CalculationFailed
- [ ] Define User/Auth events
  - [ ] UserLoggedIn
  - [ ] UserLoggedOut
  - [ ] TokenRefreshed
  - [ ] PermissionChanged
- [ ] Define System events
  - [ ] ServiceStarted
  - [ ] ServiceStopped
  - [ ] HealthCheckFailed
  - [ ] ConfigurationChanged

### Milestone 4.3: Command Definitions
- [ ] Define command base class
- [ ] Define common commands

**Deliverables:**
- Shared.Contracts NuGet package
- All event types for inter-service communication

---

## Phase 5: Common Domain Types

**Goal:** Shared domain primitives and utilities.

### Milestone 5.1: Project Setup
- [ ] Create Shared.Domain project
- [ ] Configure for .NET 8

### Milestone 5.2: Strongly-Typed Identifiers (MS-SH-10)
- [ ] Implement `MissionId` struct
- [ ] Implement `SpacecraftId` struct
- [ ] Implement `UserId` struct
- [ ] Implement `PropagationId` struct
- [ ] Implement `ManevuerId` struct
- [ ] Add JSON converters
- [ ] Add MessagePack formatters

### Milestone 5.3: Result Types (MS-SH-10)
- [ ] Implement `Result<T>` type
- [ ] Implement `Result` (non-generic) type
- [ ] Implement `Error` type
- [ ] Implement `ValidationError` type
- [ ] Add extension methods for chaining

### Milestone 5.4: Common Types (MS-SH-10)
- [ ] Implement `PagedResult<T>`
- [ ] Implement `DateTimeRange`
- [ ] Implement unit types (Length, Duration, Angle)
- [ ] Implement `Vector3D`
- [ ] Implement `Quaternion`

**Deliverables:**
- Shared.Domain NuGet package
- Strongly-typed identifiers
- Result pattern implementation
- Common domain types

---

## Phase 6: Observability & Error Handling

**Goal:** Logging, metrics, tracing, and error handling.

### Milestone 6.1: Observability (MS-SH-8)
- [ ] Add structured logging to event bus
- [ ] Implement OpenTelemetry trace context propagation
- [ ] Add metrics collection (publish/subscribe counts)
- [ ] Add latency metrics
- [ ] Create correlation ID middleware

### Milestone 6.2: Error Handling (MS-SH-9)
- [ ] Implement retry policies (Polly)
- [ ] Implement circuit breaker
- [ ] Implement dead letter queue abstraction
- [ ] Add exception handlers
- [ ] Create error events

### Milestone 6.3: Configuration (MS-SH-6)
- [ ] Create `MessagingOptions` class
- [ ] Add environment-based switching
- [ ] Add configuration validation
- [ ] Document all options

**Deliverables:**
- Full observability integration
- Robust error handling
- Configuration documentation

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Messaging Abstractions | P0 - Critical |
| Phase 2 | UDP Multicast Implementation | P0 - Critical |
| Phase 3 | Redis Pub/Sub Implementation | P0 - Critical |
| Phase 4 | Event Contracts | P0 - Critical |
| Phase 5 | Common Domain Types | P1 - High |
| Phase 6 | Observability & Error Handling | P1 - High |

---

## Success Metrics

- [ ] All 10 requirements (MS-SH-1 through MS-SH-10) implemented
- [ ] Zero external dependencies for local development (UDP Multicast)
- [ ] Serialization performance < 1ms for typical payloads
- [ ] 90%+ unit test coverage
- [ ] All microservices successfully integrated
- [ ] Documentation complete for all public APIs

---

## Implementation Notes

### Local Development Flow
```
Developer Machine
    ├── Service A ──┐
    ├── Service B ──┼── UDP Multicast (239.0.0.1:5000)
    └── Service C ──┘
```

### Production Flow
```
Production Environment
    ├── Service A ──┐
    ├── Service B ──┼── Redis Pub/Sub (redis://cluster)
    └── Service C ──┘
```

### Configuration Example
```json
{
  "Messaging": {
    "Provider": "UdpMulticast",  // or "Redis"
    "UdpMulticast": {
      "MulticastGroup": "239.0.0.1",
      "Port": 5000,
      "TTL": 1
    },
    "Redis": {
      "ConnectionString": "localhost:6379",
      "ChannelPrefix": "ngmat:"
    }
  }
}
```
