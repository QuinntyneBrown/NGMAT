# Event Store Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Event Store Service, which provides event sourcing and audit trail capabilities for NGMAT.

---

## Phase 1: Core Event Storage

**Goal:** Basic event persistence and retrieval.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up database (SQL Server or MongoDB)
- [ ] Configure Redis Pub/Sub connection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Event Storage (MS-ES-1)
- [ ] Create Event entity with all required fields
- [ ] Implement append-only storage
- [ ] Generate sequence numbers per aggregate
- [ ] Add timestamp and correlation ID handling
- [ ] Create store event endpoint

### Milestone 1.3: Event Querying (MS-ES-2)
- [ ] Implement filtering by aggregate ID
- [ ] Implement filtering by event type
- [ ] Add date range filtering
- [ ] Implement pagination
- [ ] Create query endpoint

**Deliverables:**
- Working event store and retrieval
- Unit tests for storage operations

---

## Phase 2: Event Replay & Snapshots

**Goal:** State reconstruction and performance optimization.

### Milestone 2.1: Event Replay (MS-ES-3)
- [ ] Implement event stream reading
- [ ] Create replay mechanism
- [ ] Build state reconstruction logic
- [ ] Add replay endpoint

### Milestone 2.2: Snapshots (MS-ES-5)
- [ ] Create snapshot entity
- [ ] Implement snapshot creation
- [ ] Optimize replay with snapshots
- [ ] Configure automatic snapshot intervals
- [ ] Create snapshot endpoint

**Deliverables:**
- Event replay functionality
- Snapshot-based optimization

---

## Phase 3: Subscriptions & Distribution

**Goal:** Event distribution to subscribers.

### Milestone 3.1: Event Subscriptions (MS-ES-4)
- [ ] Create subscription entity
- [ ] Implement subscription management
- [ ] Integrate with Redis Pub/Sub
- [ ] Add retry logic with exponential backoff
- [ ] Implement dead letter queue

### Milestone 3.2: Event Distribution
- [ ] Publish events to Redis channels
- [ ] Handle subscriber acknowledgments
- [ ] Implement at-least-once delivery

**Deliverables:**
- Subscription management
- Reliable event distribution

---

## Phase 4: Schema Management

**Goal:** Event schema versioning and evolution.

### Milestone 4.1: Schema Versioning (MS-ES-6)
- [ ] Add version metadata to events
- [ ] Implement schema registry
- [ ] Create upcasting mechanism
- [ ] Handle backward compatibility

**Deliverables:**
- Schema versioning support
- Event migration capabilities

---

## Phase 5: Audit & Compliance

**Goal:** Tamper-proof audit trail.

### Milestone 5.1: Audit Trail (MS-ES-7)
- [ ] Implement cryptographic hashing
- [ ] Create audit query interface
- [ ] Add retention policies
- [ ] Implement tamper detection

**Deliverables:**
- Compliance-ready audit trail
- Tamper detection

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core Event Storage | P0 - Critical |
| Phase 2 | Replay & Snapshots | P1 - High |
| Phase 3 | Subscriptions | P0 - Critical |
| Phase 4 | Schema Management | P2 - Medium |
| Phase 5 | Audit & Compliance | P1 - High |

---

## Technical Dependencies

- **StackExchange.Redis** - Redis client
- **Newtonsoft.Json** or **System.Text.Json** - JSON serialization
- **MessagePack** - Binary serialization (optional)

---

## Success Metrics

- [ ] All 7 requirements (MS-ES-1 through MS-ES-7) implemented
- [ ] 80%+ unit test coverage
- [ ] Event append latency < 10ms
- [ ] Query performance < 100ms for typical queries
- [ ] Zero event loss guarantee
