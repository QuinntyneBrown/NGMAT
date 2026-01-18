# Event Store Service Requirements

## Overview

**Domain:** Event sourcing, audit trail, and event replay.

The Event Store Service provides event persistence, event sourcing capabilities, and audit trail functionality for the NGMAT platform.

---

## Requirements

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

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/events | Store new event |
| GET | /v1/events | Query events |
| POST | /v1/events/replay/{aggregateId} | Replay events for aggregate |
| POST | /v1/events/subscribe | Subscribe to event types |
| POST | /v1/events/snapshot/{aggregateId} | Create snapshot |
| GET | /v1/events/audit | Query audit trail |

---

## Event Schema

```json
{
  "eventId": "guid",
  "eventType": "MissionCreated",
  "aggregateId": "guid",
  "aggregateType": "Mission",
  "sequenceNumber": 1,
  "timestamp": "2024-01-01T00:00:00Z",
  "userId": "guid",
  "correlationId": "guid",
  "version": 1,
  "data": { },
  "metadata": { }
}
```

---

## Dependencies

- **Event Bus**: Redis Pub/Sub for event distribution
- **Database**: Event storage (SQL Server or MongoDB recommended)
- **Redis**: For caching and pub/sub
