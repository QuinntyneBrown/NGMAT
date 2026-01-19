# Reporting and Export Guide

Learn how to generate mission reports, export data, and create documentation in NGMAT.

## Table of Contents

- [Report Types](#report-types)
- [Generating Reports](#generating-reports)
- [Data Export](#data-export)
- [Custom Reports](#custom-reports)
- [Scheduled Reports](#scheduled-reports)

## Report Types

### Mission Summary Report

Comprehensive overview of mission:

**Contents:**
- Mission metadata (name, type, epochs)
- Spacecraft configuration
- Maneuver summary
- Orbit characteristics
- Delta-V budget
- Timeline of events
- Key statistics

**Format Options:** PDF, HTML, Markdown

**Use Cases:**
- Mission reviews
- Documentation
- Stakeholder presentations

### Delta-V Budget Report

Detailed fuel and maneuver analysis:

**Contents:**
- List of all maneuvers
- Delta-V per maneuver
- Cumulative delta-V
- Fuel consumption
- Fuel remaining
- Budget margins

**Format Options:** PDF, CSV, Excel

### Trajectory Report

Complete trajectory data:

**Contents:**
- State vectors over time
- Orbital elements evolution
- Position/velocity plots
- Ground track data
- Eclipse periods

**Format Options:** CSV, JSON, PDF

### Event Timeline Report

Chronological event listing:

**Contents:**
- Maneuvers
- Apoapsis/periapsis
- Node crossings
- Eclipse entry/exit
- Ground station contacts
- Custom events

**Format Options:** PDF, HTML, Excel

## Generating Reports

### Quick Report Generation

1. Open mission
2. Click **"Reports"** → **"Generate Report"**
3. Select report type
4. Choose format (PDF, HTML, etc.)
5. Click **"Generate"**
6. Download or view report

### Detailed Report Configuration

**Step 1: Select Report Type**
```
Report Type: Mission Summary
```

**Step 2: Configure Content**
```
Include Sections:
  ☑ Mission Overview
  ☑ Spacecraft Details
  ☑ Maneuver Summary
  ☑ Orbital Analysis
  ☑ Delta-V Budget
  ☐ Detailed State Vectors (very large)
  ☑ Charts and Plots
```

**Step 3: Format Options**
```
Format: PDF
Page Size: Letter (8.5" x 11")
Orientation: Portrait
Include Cover Page: Yes
Include Table of Contents: Yes
```

**Step 4: Customize Appearance**
```
Theme: Professional
Logo: Upload custom logo
Header: Mission Name and Date
Footer: Page Numbers
```

**Step 5: Generate**
- Click **"Generate Report"**
- Progress indicator shows status
- Download when complete

### Report Templates

Use built-in templates:

**Standard Templates:**
- **Executive Summary** - High-level overview
- **Technical Report** - Detailed analysis
- **Operations Report** - Operational data
- **Preliminary Design** - Mission planning phase
- **Final Report** - Mission completion

**Custom Templates:**
- Create your own templates
- Save formatting preferences
- Reuse across missions

## Data Export

### Export State Vectors

Export propagated trajectory data:

1. Open mission results
2. Click **"Export"** → **"State Vectors"**
3. Configure export:
   ```
   Format: CSV
   Time Range: Full mission
   Coordinate System: ECI J2000
   Output Interval: 60 seconds
   ```
4. Click **"Export"**
5. Save file

**CSV Format Example:**
```csv
Epoch,X(km),Y(km),Z(km),VX(km/s),VY(km/s),VZ(km/s)
2026-01-18T12:00:00.000Z,3000.0,5000.0,3000.0,-5.5,3.2,4.1
2026-01-18T12:01:00.000Z,2967.0,5019.0,3024.0,-5.6,3.1,4.0
...
```

### Export Orbital Elements

Export Keplerian elements:

1. Click **"Export"** → **"Orbital Elements"**
2. Select elements:
   ```
   ☑ Semi-major Axis
   ☑ Eccentricity
   ☑ Inclination
   ☑ RAAN
   ☑ Argument of Perigee
   ☑ True Anomaly
   ☑ Apoapsis Altitude
   ☑ Periapsis Altitude
   ```
3. Choose format (CSV, JSON)
4. Export

### Export Mission Configuration

Export complete mission definition:

**JSON Format:**
```json
{
  "mission": {
    "name": "My LEO Mission",
    "type": "LEO",
    "startEpoch": "2026-01-18T12:00:00.000Z",
    "spacecraft": [{
      "name": "LEO Sat 1",
      "dryMass": 500,
      "fuelMass": 100,
      "initialState": {
        "a": 6778,
        "e": 0.001,
        "i": 51.6,
        ...
      }
    }],
    "maneuvers": [...],
    "propagationSettings": {...}
  }
}
```

**Use Cases:**
- Backup mission configuration
- Share with collaborators
- Version control
- Import to other tools

### Export TLE

Generate Two-Line Element sets:

1. Click **"Export"** → **"TLE"**
2. Select spacecraft
3. Select epoch for TLE
4. Export

**TLE Example:**
```
LEO SAT 1
1 99999U 26018A   26018.50000000  .00002182  00000-0  41420-4 0  9990
2 99999  51.6416 247.4627 0006703 130.5360 325.0288 15.72125391000010
```

### Export to GMAT Script

Export to NASA GMAT format:

1. Click **"Export"** → **"GMAT Script"**
2. Mission converted to GMAT commands
3. Save `.script` file
4. Load in GMAT for verification

**Example GMAT Script:**
```
Create Spacecraft LEOSat1;
LEOSat1.DateFormat = UTCGregorian;
LEOSat1.Epoch = '18 Jan 2026 12:00:00.000';
LEOSat1.CoordinateSystem = EarthMJ2000Eq;
LEOSat1.DisplayStateType = Keplerian;
LEOSat1.SMA = 6778;
LEOSat1.ECC = 0.001;
LEOSat1.INC = 51.6;
...

BeginMissionSequence;
Propagate DefaultProp(LEOSat1) {LEOSat1.ElapsedDays = 1};
```

## Custom Reports

### Create Custom Template

1. Click **"Reports"** → **"Custom Template"**
2. Use template designer:
   ```
   Sections:
   - Cover Page
   - Mission Overview
   - [Custom Section 1]
   - Spacecraft Details
   - [Custom Section 2]
   - Charts
   - Appendix
   ```
3. Add custom sections with markdown
4. Insert placeholders for data:
   ```markdown
   ## Mission: {{mission.name}}
   
   **Type:** {{mission.type}}
   **Start:** {{mission.startEpoch}}
   
   ### Spacecraft: {{spacecraft.name}}
   - Dry Mass: {{spacecraft.dryMass}} kg
   - Fuel Mass: {{spacecraft.fuelMass}} kg
   ```
5. Save template
6. Generate reports using template

### Template Variables

**Mission Variables:**
- `{{mission.name}}`
- `{{mission.type}}`
- `{{mission.startEpoch}}`
- `{{mission.endEpoch}}`
- `{{mission.description}}`

**Spacecraft Variables:**
- `{{spacecraft.name}}`
- `{{spacecraft.dryMass}}`
- `{{spacecraft.fuelMass}}`
- `{{spacecraft.dragCoefficient}}`

**Computed Variables:**
- `{{results.totalDeltaV}}`
- `{{results.fuelConsumed}}`
- `{{results.orbitalPeriod}}`
- `{{results.maneuverCount}}`

### Include Charts

Embed charts in reports:

```markdown
## Altitude Profile

{{chart:altitude_vs_time}}

## Ground Track

{{map:ground_track}}
```

Charts automatically generated and embedded.

## Scheduled Reports

### Auto-Generate Reports

Schedule periodic report generation:

1. Click **"Reports"** → **"Schedule Report"**
2. Configure schedule:
   ```
   Report Type: Delta-V Budget
   Frequency: Weekly
   Day: Monday
   Time: 09:00 UTC
   ```
3. Set delivery:
   ```
   Delivery Method: Email
   Recipients: team@example.com
   Subject: Weekly Delta-V Budget Report
   ```
4. Save schedule

### Email Delivery

Configure email notifications:

**Email Settings:**
- Recipients (comma-separated)
- CC/BCC
- Subject line template
- Email body message
- Attach report as PDF

**Example Configuration:**
```
To: mission-team@example.com
CC: manager@example.com
Subject: [NGMAT] {{mission.name}} - {{report.type}} - {{date}}
Body:
  Attached is the {{report.type}} for mission {{mission.name}}.
  
  Generated: {{timestamp}}
  Next report: {{next_report_date}}
```

### Webhook Delivery

Send reports to external systems:

1. Configure webhook URL
2. Set HTTP method (POST)
3. Configure headers and authentication
4. Report sent as JSON payload

**Webhook Payload:**
```json
{
  "report": {
    "type": "Mission Summary",
    "mission": "My LEO Mission",
    "generated": "2026-01-18T12:00:00.000Z",
    "downloadUrl": "https://..."
  }
}
```

## Best Practices

### Report Organization

**Naming Convention:**
```
[Mission]_[ReportType]_[Date]_[Version].pdf

Examples:
- ISS_Reboost_MissionSummary_20260118_v1.pdf
- Starlink_DeltaVBudget_20260118_Final.pdf
```

**Version Control:**
- Include version numbers
- Track report revisions
- Archive old versions

### Data Archiving

**Regular Exports:**
- Export mission data weekly
- Store in version control (Git)
- Keep JSON backups
- Archive completed missions

**Storage Location:**
```
/missions/
  /leo-mission-2026/
    /exports/
      mission-config-20260118.json
      state-vectors-20260118.csv
      orbital-elements-20260118.csv
    /reports/
      mission-summary-20260118.pdf
      deltav-budget-20260118.pdf
```

### Sharing Reports

**Internal Sharing:**
- Use mission sharing feature
- Grant appropriate permissions
- Add comments for context

**External Sharing:**
- Export to PDF
- Remove sensitive data if needed
- Include disclaimer if preliminary

### Report Quality

**Before Publishing:**
- ✓ Review all data for accuracy
- ✓ Check charts and visualizations
- ✓ Verify calculations
- ✓ Proofread text content
- ✓ Test all links
- ✓ Ensure consistent formatting

---

**Next:** [Scripting and Automation Guide](scripting-automation.md) - Learn to automate workflows with scripts.
