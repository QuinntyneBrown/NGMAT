# Reporting Service Requirements

## Overview

**Domain:** Report generation and data export.

The Reporting Service generates mission reports, exports data in various formats, and provides TLE generation and delta-V budget summaries.

---

## Requirements

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

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /v1/reports/mission/{missionId} | Generate mission report |
| GET | /v1/reports/export/states/{spacecraftId} | Export state vectors |
| GET | /v1/reports/export/elements/{spacecraftId} | Export orbital elements |
| GET | /v1/reports/tle/{spacecraftId} | Generate TLE |
| GET | /v1/reports/delta-v/{missionId} | Delta-V budget |
| GET | /v1/reports/timeline/{missionId} | Event timeline |
| GET | /v1/reports/conjunction/{spacecraftId} | Conjunction report |
| POST | /v1/reports/custom | Custom report |
| POST | /v1/reports/schedule | Schedule report |

---

## Output Formats

| Format | Library |
|--------|---------|
| PDF | PdfSharpCore, iTextSharp |
| HTML | Razor templates |
| CSV | CsvHelper |
| JSON | System.Text.Json |

---

## Dependencies

- **Mission Management Service** - Mission data
- **Spacecraft Service** - Spacecraft data
- **Maneuver Service** - Maneuver data
- **Propagation Service** - State vectors
- **Notification Service** - Email delivery
