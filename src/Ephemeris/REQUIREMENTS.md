# Ephemeris Service Requirements

## Overview

**Domain:** Celestial body positions and orientations.

The Ephemeris Service provides high-precision positions and velocities for planets, moons, and the Sun, as well as Earth orientation parameters and solar flux data needed for force modeling.

---

## Requirements

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

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /v1/ephemeris/planet/{name} | Planet position |
| GET | /v1/ephemeris/moon | Moon position |
| GET | /v1/ephemeris/sun | Sun position |
| GET | /v1/ephemeris/eop | Earth orientation parameters |
| GET | /v1/ephemeris/solar-flux | Solar flux data |
| GET | /v1/ephemeris/stars | Star catalog query |

---

## Data Sources

| Data | Source | Update Frequency |
|------|--------|------------------|
| Planet/Moon positions | JPL DE440/DE441 | Static (embedded) |
| Earth orientation | IERS Bulletin A/B | Daily |
| Solar flux | NOAA SWPC | Daily |
| Star catalog | Hipparcos | Static |

---

## Dependencies

- **Calculation Engine Service** - Interpolation
- **Redis** - Caching
