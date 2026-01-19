# Mission Management Service Requirements

## Overview

**Domain:** Mission lifecycle, configuration, and metadata management.

The Mission Management Service handles the complete lifecycle of space missions including creation, configuration, sharing, and archival.

---

## Requirements

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

### MS-MM-6: Mission Status Management ✅

**Description:** Manage mission lifecycle status.

**Acceptance Criteria:**
- [x] Status transitions: Draft → Active → Completed → Archived
- [x] Invalid transitions rejected
- [x] MissionStatusChangedEvent published
- [x] REST API: PATCH /v1/missions/{id}/status
- [x] Audit trail of status changes

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

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/missions | Create mission |
| GET | /v1/missions | List missions |
| GET | /v1/missions/{id} | Get mission |
| PUT | /v1/missions/{id} | Update mission |
| DELETE | /v1/missions/{id} | Delete mission |
| PATCH | /v1/missions/{id}/status | Change status |
| POST | /v1/missions/{id}/share | Share mission |
| POST | /v1/missions/{id}/clone | Clone mission |
| POST | /v1/missions/export | Export mission |
| POST | /v1/missions/import | Import mission |

---

## Events Published

| Event | Description |
|-------|-------------|
| MissionCreatedEvent | Mission created |
| MissionUpdatedEvent | Mission updated |
| MissionDeletedEvent | Mission deleted |
| MissionStatusChangedEvent | Mission status changed |
| MissionSharedEvent | Mission shared with user |
| MissionClonedEvent | Mission cloned |

---

## Mission Statuses

| Status | Description |
|--------|-------------|
| Draft | Initial state, being configured |
| Active | Mission is being worked on |
| Completed | Mission analysis complete |
| Archived | Mission archived for reference |

---

## Dependencies

- **Identity Service** - User authentication and authorization
- **Event Store Service** - Event persistence
