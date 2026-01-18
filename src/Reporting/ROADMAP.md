# Reporting Service Roadmap

## Overview

This roadmap outlines the implementation phases for the Reporting Service, which generates reports and exports data.

---

## Phase 1: Data Export

**Goal:** Basic data export capabilities.

### Milestone 1.1: Project Setup
- [ ] Configure project structure (Core, Infrastructure, Api)
- [ ] Add report generation libraries
- [ ] Set up Serilog logging
- [ ] Add health check endpoints

### Milestone 1.2: State Vector Export (MS-RP-2)
- [ ] Query propagation data
- [ ] Export CSV format
- [ ] Export JSON format
- [ ] Create API endpoint

### Milestone 1.3: Orbital Elements Export (MS-RP-3)
- [ ] Convert to Keplerian elements
- [ ] Export formats
- [ ] Create API endpoint

**Deliverables:**
- Data export API

---

## Phase 2: TLE & Delta-V

**Goal:** Specialized reports.

### Milestone 2.1: TLE Generation (MS-RP-4)
- [ ] Compute mean elements
- [ ] Format TLE string
- [ ] Create API endpoint

### Milestone 2.2: Delta-V Budget (MS-RP-5)
- [ ] Query maneuver data
- [ ] Calculate totals
- [ ] Generate report
- [ ] Create API endpoint

**Deliverables:**
- TLE generation
- Delta-V budget report

---

## Phase 3: Mission Reports

**Goal:** Comprehensive PDF reports.

### Milestone 3.1: Mission Report (MS-RP-1)
- [ ] Design report template
- [ ] Generate PDF output
- [ ] Include graphics
- [ ] Create async endpoint

### Milestone 3.2: Timeline Report (MS-RP-6)
- [ ] Query all events
- [ ] Sort chronologically
- [ ] Generate report

**Deliverables:**
- Mission reports
- Timeline reports

---

## Phase 4: Advanced Features

**Goal:** Custom templates and scheduling.

### Milestone 4.1: Conjunction Report (MS-RP-7)
- [ ] Query conjunction data
- [ ] Generate report

### Milestone 4.2: Custom Templates (MS-RP-8)
- [ ] Implement template engine
- [ ] Support variable substitution

### Milestone 4.3: Scheduling (MS-RP-9)
- [ ] Implement scheduler
- [ ] Email/webhook delivery

**Deliverables:**
- Custom reports
- Scheduled reports

---

## Timeline Summary

| Phase | Description | Priority |
|-------|-------------|----------|
| Phase 1 | Data Export | P0 - Critical |
| Phase 2 | TLE & Delta-V | P1 - High |
| Phase 3 | Mission Reports | P1 - High |
| Phase 4 | Advanced Features | P2 - Medium |

---

## Success Metrics

- [ ] All 9 requirements (MS-RP-1 through MS-RP-9) implemented
- [ ] PDF generation < 5 seconds
- [ ] 80%+ unit test coverage
