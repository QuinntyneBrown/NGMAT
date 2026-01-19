# Scripting and Automation Guide

Learn how to use scripts to automate mission analysis and create reusable workflows in NGMAT.

## Table of Contents

- [Script Basics](#script-basics)
- [Script Editor](#script-editor)
- [GMAT Script Compatibility](#gmat-script-compatibility)
- [Script Commands](#script-commands)
- [Script Examples](#script-examples)
- [Script Library](#script-library)

## Script Basics

### What are Scripts?

Scripts in NGMAT allow you to:
- Automate repetitive tasks
- Define missions programmatically
- Run batch analyses
- Create reproducible workflows
- Share mission configurations

### Script Language

NGMAT uses a GMAT-compatible scripting language with:
- Object creation and configuration
- Propagation commands
- Maneuver definitions
- Control structures (if/while/for)
- Variables and expressions
- Functions

## Script Editor

### Open Script Editor

**Web Application:**
1. Click **"Scripts"** in sidebar
2. Click **"+ New Script"**

**Desktop Application:**
1. Select **File** → **New Script**
2. Or press `Ctrl+Shift+N`

### Editor Features

**Syntax Highlighting:**
- Keywords in blue
- Strings in green
- Comments in gray
- Numbers in orange

**Auto-Completion:**
- Press `Ctrl+Space` for suggestions
- Available objects and commands
- Parameter names

**Error Detection:**
- Real-time syntax checking
- Error markers in margin
- Hover for error details

**Line Numbers:**
- Easy reference for debugging
- Jump to line with `Ctrl+G`

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Enter` | Run script |
| `Ctrl+S` | Save script |
| `Ctrl+/` | Toggle comment |
| `Ctrl+D` | Duplicate line |
| `Ctrl+L` | Delete line |
| `Ctrl+F` | Find |
| `Ctrl+H` | Find and replace |
| `Ctrl+Space` | Auto-complete |

## GMAT Script Compatibility

### Import GMAT Scripts

1. Click **"Import Script"**
2. Select `.script` file
3. NGMAT converts to compatible format
4. Review and adjust if needed

### Compatibility Notes

**Fully Supported:**
- Spacecraft definition
- Propagator configuration
- Force models
- Basic maneuvers
- Coordinate systems

**Partially Supported:**
- Complex optimization
- Custom functions
- Some specialized commands

**Not Supported:**
- MATLAB interface
- Some legacy features

### Convert GMAT Script

Example conversion:

**GMAT Script:**
```
Create Spacecraft Sat;
Sat.Epoch = '18 Jan 2026 12:00:00.000';
Sat.CoordinateSystem = EarthMJ2000Eq;
Sat.SMA = 6778;
```

**NGMAT Equivalent:**
```
Create Spacecraft Sat
Sat.Epoch = 2026-01-18T12:00:00Z
Sat.CoordinateSystem = ECI_J2000
Sat.SemiMajorAxis = 6778
```

## Script Commands

### Object Creation

**Create Spacecraft:**
```
Create Spacecraft LEOSat
LEOSat.DryMass = 500
LEOSat.FuelMass = 100
LEOSat.Cd = 2.2
LEOSat.DragArea = 10
LEOSat.SRPArea = 8
LEOSat.Reflectivity = 1.8
```

**Create Propagator:**
```
Create Propagator RK45Prop
RK45Prop.Type = RungeKutta45
RK45Prop.StepSize = 60
RK45Prop.Tolerance = 1e-10
```

**Create Force Model:**
```
Create ForceModel FM
FM.CentralBody = Earth
FM.Gravity.Degree = 20
FM.Gravity.Order = 20
FM.Drag = NRLMSISE00
FM.SRP = On
```

**Create Burn:**
```
Create ImpulsiveBurn Burn1
Burn1.CoordinateSystem = VNB
Burn1.V = 100
Burn1.N = 0
Burn1.B = 0
```

### Set Properties

```
LEOSat.Epoch = 2026-01-18T12:00:00Z
LEOSat.SemiMajorAxis = 6778
LEOSat.Eccentricity = 0.001
LEOSat.Inclination = 51.6
LEOSat.RAAN = 0
LEOSat.ArgumentOfPerigee = 0
LEOSat.TrueAnomaly = 0
```

### Propagation

**Propagate by Time:**
```
Propagate RK45Prop(LEOSat) {LEOSat.ElapsedDays = 1}
```

**Propagate to Epoch:**
```
Propagate RK45Prop(LEOSat) {LEOSat.Epoch = 2026-01-19T12:00:00Z}
```

**Propagate to Condition:**
```
Propagate RK45Prop(LEOSat) {LEOSat.Apoapsis}
Propagate RK45Prop(LEOSat) {LEOSat.Periapsis}
Propagate RK45Prop(LEOSat) {LEOSat.Altitude = 500}
```

### Maneuvers

**Apply Impulsive Burn:**
```
Maneuver Burn1(LEOSat)
```

**Apply at Specific Location:**
```
Propagate RK45Prop(LEOSat) {LEOSat.Periapsis}
Maneuver Burn1(LEOSat)
Propagate RK45Prop(LEOSat) {LEOSat.ElapsedDays = 1}
```

### Control Structures

**If Statement:**
```
If LEOSat.Altitude < 300
  Maneuver ReboostBurn(LEOSat)
EndIf
```

**While Loop:**
```
While LEOSat.ElapsedDays < 7
  Propagate RK45Prop(LEOSat) {LEOSat.ElapsedDays = 1}
  Report Altitude
EndWhile
```

**For Loop:**
```
For i = 1:10
  Propagate RK45Prop(LEOSat) {LEOSat.ElapsedSecs = 3600}
  Report LEOSat.Altitude
EndFor
```

### Variables

**Declare and Use:**
```
Create Variable deltaV
deltaV = 100

Create ImpulsiveBurn Burn
Burn.V = deltaV
```

**Calculations:**
```
Create Variable totalDeltaV
totalDeltaV = burn1DeltaV + burn2DeltaV

Create Variable fuelNeeded
fuelNeeded = spacecraft.DryMass * (exp(totalDeltaV / (Isp * 9.80665)) - 1)
```

### Output and Reporting

**Report Values:**
```
Report LEOSat.Altitude
Report LEOSat.Velocity
Report LEOSat.Latitude LEOSat.Longitude
```

**Save to File:**
```
Create ReportFile OrbitData
OrbitData.Filename = 'orbit_data.csv'
OrbitData.WriteHeaders = On

Report OrbitData LEOSat.Epoch LEOSat.Altitude LEOSat.Velocity
```

## Script Examples

### Example 1: Simple LEO Mission

```
% Create spacecraft
Create Spacecraft LEOSat
LEOSat.DryMass = 500
LEOSat.FuelMass = 100
LEOSat.Epoch = 2026-01-18T12:00:00Z
LEOSat.SemiMajorAxis = 6778
LEOSat.Eccentricity = 0.001
LEOSat.Inclination = 51.6

% Create propagator
Create Propagator Prop
Prop.Type = RungeKutta45
Prop.StepSize = 60

% Create force model
Create ForceModel FM
FM.Gravity.Degree = 20
FM.Gravity.Order = 20
FM.Drag = NRLMSISE00
FM.SRP = On

% Propagate for one day
Propagate Prop(LEOSat) {LEOSat.ElapsedDays = 1}

% Report results
Report LEOSat.Altitude
Report LEOSat.Velocity
```

### Example 2: Hohmann Transfer

```
% Initial orbit (LEO)
Create Spacecraft Sat
Sat.SemiMajorAxis = 6778
Sat.Eccentricity = 0
Sat.Inclination = 0

% First burn at periapsis
Create ImpulsiveBurn Burn1
Burn1.V = 2441  % m/s
Burn1.N = 0
Burn1.B = 0

% Second burn at apoapsis
Create ImpulsiveBurn Burn2
Burn2.V = 1480  % m/s
Burn2.N = 0
Burn2.B = 0

% Propagator
Create Propagator Prop
Prop.Type = RungeKutta45

BeginMissionSequence

% Apply first burn
Maneuver Burn1(Sat)

% Coast to apoapsis
Propagate Prop(Sat) {Sat.Apoapsis}

% Apply second burn
Maneuver Burn2(Sat)

% Verify circular orbit at GEO
Propagate Prop(Sat) {Sat.ElapsedDays = 1}
Report Sat.SemiMajorAxis
Report Sat.Eccentricity
```

### Example 3: Station-Keeping

```
% GEO satellite
Create Spacecraft GEOSat
GEOSat.SemiMajorAxis = 42164
GEOSat.Eccentricity = 0.001
GEOSat.Inclination = 0.1

% Threshold values
Create Variable altitudeTolerance
altitudeTolerance = 10  % km

Create Variable targetAltitude
targetAltitude = 35786  % km

% Corrective burn
Create ImpulsiveBurn Correction
Correction.CoordinateSystem = VNB

% Propagator
Create Propagator Prop
Prop.Type = RungeKutta45

BeginMissionSequence

For day = 1:365
  Propagate Prop(GEOSat) {GEOSat.ElapsedDays = 1}
  
  % Check altitude
  If abs(GEOSat.Altitude - targetAltitude) > altitudeTolerance
    % Calculate correction
    Correction.V = (targetAltitude - GEOSat.Altitude) * 0.1
    Maneuver Correction(GEOSat)
    Report 'Station-keeping maneuver applied'
  EndIf
EndFor
```

### Example 4: Multi-Satellite Constellation

```
% Create 4 satellites in Walker constellation
For i = 1:4
  Create Spacecraft Sat{i}
  Sat{i}.SemiMajorAxis = 7178
  Sat{i}.Eccentricity = 0
  Sat{i}.Inclination = 55
  Sat{i}.RAAN = (i-1) * 90  % Evenly spaced
EndFor

% Propagator
Create Propagator Prop
Prop.Type = RungeKutta45

BeginMissionSequence

% Propagate all satellites
Propagate Prop(Sat1, Sat2, Sat3, Sat4) {Sat1.ElapsedDays = 1}

% Report positions
For i = 1:4
  Report Sat{i}.Latitude Sat{i}.Longitude
EndFor
```

## Script Library

### Save Script

1. Write script in editor
2. Click **"Save"** or press `Ctrl+S`
3. Enter script name
4. Add description and tags
5. Click **"Save to Library"**

### Load Script

1. Click **"Script Library"**
2. Browse or search scripts
3. Click script name
4. Script opens in editor
5. Modify if needed
6. Run or save as new script

### Share Scripts

**Within NGMAT:**
1. Save script to library
2. Set visibility to "Public"
3. Users can search and use

**Export Script:**
1. Open script
2. Click **"Export"**
3. Save as `.ngmat-script` file
4. Share file with others

**Import Script:**
1. Click **"Import Script"**
2. Select `.ngmat-script` file
3. Script added to library

### Script Categories

**Built-in Scripts:**
- LEO mission templates
- GEO transfer examples
- Rendezvous scenarios
- Station-keeping examples

**Community Scripts:**
- User-contributed scripts
- Rated and reviewed
- Download and use

**My Scripts:**
- Your saved scripts
- Private by default
- Organize with folders

## Debugging Scripts

### Run Script

1. Click **"Run"** or press `Ctrl+Enter`
2. Output appears in console
3. Errors highlighted in editor

### Debug Mode

1. Set breakpoints (click line number)
2. Click **"Debug"** instead of "Run"
3. Execution pauses at breakpoint
4. Inspect variable values
5. Step through line-by-line
6. Continue or stop

### Error Messages

**Syntax Errors:**
```
Error at line 15: Unexpected token 'Manever'
Did you mean: 'Maneuver'?
```

**Runtime Errors:**
```
Error at line 23: Spacecraft 'Sat2' not defined
Check object creation before use.
```

**Validation Errors:**
```
Warning at line 18: Eccentricity > 1 (hyperbolic orbit)
Verify this is intended.
```

## Best Practices

### Script Organization

**Structure:**
```
% Header comment describing script purpose
% Author: Your Name
% Date: 2026-01-18
% Purpose: LEO mission analysis

% Object creation section
Create Spacecraft Sat
...

% Configuration section
Sat.Property = Value
...

% Mission sequence
BeginMissionSequence
Propagate ...
```

**Comments:**
- Use `%` for single-line comments
- Document complex calculations
- Explain non-obvious choices

### Modularity

Break complex scripts into sections:

```
% Import common definitions
Import 'spacecraft-definitions.ngmat'
Import 'force-models.ngmat'

% Mission-specific code here
BeginMissionSequence
...
```

### Version Control

Save scripts in Git:
```bash
mkdir mission-scripts
cd mission-scripts
git init
# Add and commit scripts
git add leo-mission.ngmat
git commit -m "Initial LEO mission script"
```

---

**Next:** [Advanced Topics Guide](advanced-topics.md) - Explore advanced features and techniques.
