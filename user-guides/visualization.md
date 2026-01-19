# Visualization Guide

Learn how to visualize orbits, ground tracks, and mission data in NGMAT.

## Table of Contents

- [3D Orbit Visualization](#3d-orbit-visualization)
- [Ground Track Maps](#ground-track-maps)
- [Time-Series Charts](#time-series-charts)
- [Orbital Elements Plots](#orbital-elements-plots)
- [Custom Visualizations](#custom-visualizations)

## 3D Orbit Visualization

### Open 3D View

1. Open a mission with propagated results
2. Click **"Visualize"** tab
3. Select **"3D Orbit View"**

### 3D View Controls

**Mouse Navigation:**
- **Left-click + drag** - Rotate camera
- **Right-click + drag** - Pan view
- **Scroll wheel** - Zoom in/out
- **Middle-click** - Reset view

**Keyboard Navigation:**
- **Arrow keys** - Rotate view
- **+/-** - Zoom
- **Home** - Reset to default view
- **Space** - Play/pause animation

### Display Options

**Orbit Path:**
- Show/hide orbit path
- Adjust path length
- Color by velocity or altitude
- Line thickness

**Spacecraft:**
- Show spacecraft icon
- Scale icon size
- Show velocity vector
- Show attitude axes

**Central Body:**
- Earth, Moon, Sun, etc.
- Show texture
- Show atmosphere
- Show grid lines

**Reference Frames:**
- Show coordinate axes
- ECI, ECEF, or custom
- Axis labels and scale

### Animation

**Time Controls:**
- Play/pause
- Speed multiplier (1x, 10x, 100x)
- Step forward/backward
- Jump to specific epoch

**Time Display:**
- Current epoch
- Mission elapsed time
- Time to next event

### Camera Presets

**Front View** - Looking at orbital plane from front
**Top View** - Looking down on North pole
**Side View** - Perpendicular to orbital plane
**Spacecraft View** - Follow spacecraft
**Free Camera** - Manual control

## Ground Track Maps

### Open Ground Track

1. Click **"Visualize"** → **"Ground Track"**
2. Select spacecraft
3. Set time range
4. View map

### Map Features

**Ground Track Line:**
- Path of sub-satellite point
- Color coding by time or altitude
- Ascending/descending indicators

**Current Position:**
- Real-time marker
- Latitude/longitude display
- Altitude display

**Footprint:**
- Coverage area
- Elevation angle contours
- Swath width (for sensors)

**Ground Stations:**
- Add ground station markers
- Show visibility circles
- Contact windows

### Map Controls

**Zoom and Pan:**
- Scroll to zoom
- Click and drag to pan
- Double-click to center

**Map Layers:**
- Satellite imagery
- Topographic map
- Political boundaries
- Night/day terminator

**Projections:**
- Mercator (default)
- Equirectangular
- Orthographic

## Time-Series Charts

### Create Chart

1. Click **"Charts"** tab
2. Click **"+ New Chart"**
3. Select chart type:
   - Line chart
   - Scatter plot
   - Multi-series comparison
4. Configure parameters

### Available Parameters

**Orbital Parameters:**
- Altitude
- Velocity magnitude
- Orbital period
- Apoapsis/periapsis altitude

**Position Components:**
- X, Y, Z position
- Radial distance
- Latitude, longitude

**Velocity Components:**
- vX, vY, vZ
- Radial velocity
- Tangential velocity

**Keplerian Elements:**
- Semi-major axis
- Eccentricity
- Inclination
- RAAN
- Argument of perigee
- True anomaly

**Energy and Momentum:**
- Specific orbital energy
- Angular momentum magnitude
- Eccentricity vector

### Chart Customization

**Axes:**
- Axis labels and units
- Logarithmic scale
- Custom ranges
- Grid lines

**Legend:**
- Show/hide legend
- Position (top, bottom, left, right)
- Label customization

**Colors and Styles:**
- Line colors
- Line styles (solid, dashed, dotted)
- Marker shapes
- Fill areas

### Chart Export

Export charts as:
- PNG (raster image)
- SVG (vector graphics)
- PDF (publication quality)
- Data (CSV, JSON)

## Orbital Elements Plots

### Elements Over Time

Plot how orbital elements evolve:

1. Select **"Orbital Elements Chart"**
2. Choose elements to display:
   - Semi-major axis
   - Eccentricity
   - Inclination
   - RAAN
   - Argument of Perigee
3. Set time range
4. View plot

### Osculating vs. Mean Elements

**Osculating Elements:**
- Instantaneous orbital elements
- Show short-period variations
- Useful for detailed analysis

**Mean Elements:**
- Averaged over orbit
- Removes short-period effects
- Shows long-term trends

Toggle between views in chart settings.

### Perturbation Analysis

Visualize effects of different perturbations:

**Example: J2 Effect on RAAN**
```
Plot: RAAN vs. Time
Compare:
  - Two-body (no perturbations)
  - With J2 only
  - With J2 + drag
  - With all perturbations
```

Shows secular drift caused by perturbations.

## Custom Visualizations

### Dashboard View

Create custom dashboards:

1. Click **"Dashboard"** tab
2. Click **"Customize"**
3. Add widgets:
   - 3D orbit view
   - Ground track
   - Time-series chart
   - Parameter table
   - Status indicators
4. Arrange layout
5. Save dashboard

### Multi-Spacecraft Comparison

Compare multiple spacecraft:

1. Select spacecraft to compare
2. Click **"Compare"**
3. Views show:
   - All orbits in 3D view
   - Overlaid ground tracks
   - Multi-series charts
   - Relative positions

### Eclipse Visualization

Visualize eclipse periods:

1. Select **"Eclipse Plot"**
2. Shows:
   - Umbra periods (full shadow)
   - Penumbra periods (partial shadow)
   - Sun visibility percentage
   - Eclipse durations
3. Color-coded timeline

### Conjunction Analysis

Visualize close approaches:

1. Select two spacecraft
2. Click **"Conjunction Analysis"**
3. Shows:
   - Relative distance over time
   - Miss distance
   - Time of closest approach
   - Probability of collision (if covariance available)

---

**Next:** [Reporting and Export Guide](reporting-export.md) - Learn to generate reports and export data.
