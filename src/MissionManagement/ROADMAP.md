# Mission Management Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Mission Management Service, which handles mission lifecycle and configuration.

---

## Phase 1: Core CRUD Operations

**Goal:** Basic mission create, read, update, delete.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Set up database (SQL Server)
- [ ] Configure Redis Pub/Sub connection
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: Create Mission (MS-MM-1)
- [ ] Create Mission entity
- [ ] Implement create endpoint
- [ ] Generate mission ID
- [ ] Validate mission name uniqueness
- [ ] Publish MissionCreatedEvent

### Milestone 1.3: Read Operations (MS-MM-4, MS-MM-5)
- [ ] Implement get mission by ID
- [ ] Implement list missions with pagination
- [ ] Add filtering and sorting
- [ ] Add search capability

### Milestone 1.4: Update & Delete (MS-MM-2, MS-MM-3)
- [ ] Implement update endpoint
- [ ] Implement soft delete
- [ ] Add authorization checks
- [ ] Publish events

**Deliverables:**
- Complete CRUD API
- Event publishing

---

## Phase 2: Status Management

**Goal:** Mission lifecycle states.

### Milestone 2.1: Status (MS-MM-6)
- [ ] Implement status entity/enum
- [ ] Create status transition rules
- [ ] Implement status change endpoint
- [ ] Add audit trail
- [ ] Publish MissionStatusChangedEvent

**Deliverables:**
- Status management API
- Status transition validation

---

## Phase 3: Collaboration

**Goal:** Mission sharing and cloning.

### Milestone 3.1: Sharing (MS-MM-7)
- [ ] Create sharing entity
- [ ] Implement share endpoint
- [ ] Support read/write permissions
- [ ] Implement revoke access
- [ ] Publish MissionSharedEvent

### Milestone 3.2: Cloning (MS-MM-8)
- [ ] Implement deep clone logic
- [ ] Clone related entities
- [ ] Create clone endpoint
- [ ] Publish MissionClonedEvent

**Deliverables:**
- Mission sharing
- Mission cloning

---

## Phase 4: Import/Export

**Goal:** Data portability.

### Milestone 4.1: Export/Import (MS-MM-9)
- [ ] Define JSON schema
- [ ] Implement JSON export
- [ ] Implement JSON import
- [ ] Add GMAT script export
- [ ] Add GMAT script import (best effort)

**Deliverables:**
- Export/import functionality
- GMAT compatibility

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Core CRUD | P0 - Critical |
| Phase 2 | Status Management | P0 - Critical |
| Phase 3 | Collaboration | P1 - High |
| Phase 4 | Import/Export | P2 - Medium |

---

## Technical Dependencies

- **Identity Service** - User authentication
- **Event Store Service** - Event persistence

---

## Success Metrics

- [ ] All 9 requirements (MS-MM-1 through MS-MM-9) implemented
- [ ] CRUD operations < 50ms response time
- [ ] 85%+ unit test coverage
