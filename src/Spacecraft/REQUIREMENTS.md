# Spacecraft Service Requirements

## Overview

**Domain:** Spacecraft definitions, hardware configurations, and properties.

The Spacecraft Service manages spacecraft definitions including physical properties, hardware configuration, fuel management, and state history.

---

## Requirements

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

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/spacecraft | Create spacecraft |
| GET | /v1/spacecraft | List spacecraft |
| GET | /v1/spacecraft/{id} | Get spacecraft |
| PUT | /v1/spacecraft/{id} | Update spacecraft |
| DELETE | /v1/spacecraft/{id} | Delete spacecraft |
| GET | /v1/spacecraft/{id}/state | Get state at epoch |
| GET | /v1/spacecraft/{id}/fuel | Get fuel status |
| POST | /v1/spacecraft/{id}/hardware | Configure hardware |
| PUT | /v1/spacecraft/{id}/attitude | Set attitude |
| POST | /v1/spacecraft/{id}/validate | Validate configuration |

---

## Events Published/Subscribed

| Event | Type | Description |
|-------|------|-------------|
| SpacecraftCreatedEvent | Published | Spacecraft created |
| SpacecraftUpdatedEvent | Published | Spacecraft updated |
| SpacecraftDeletedEvent | Published | Spacecraft deleted |
| HardwareConfiguredEvent | Published | Hardware configured |
| AttitudeChangedEvent | Published | Attitude changed |
| FuelConsumedEvent | Subscribed | Fuel used by maneuver |

---

## Dependencies

- **Mission Management Service** - Mission association
- **Coordinate System Service** - State vector frames
