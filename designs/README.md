# NGMAT Design System & Mockups

This folder contains the comprehensive design system, HTML mockups, and screenshots for the NGMAT (Next Generation Mission Analysis Tool) web application.

## Directory Structure

```
designs/
├── README.md                    # This file
├── style-guide.md              # Complete design system documentation
├── mockups/                    # HTML mockup files
│   ├── 00-base-template.html   # Base CSS template
│   ├── 01-login.html           # Login/Authentication screen
│   ├── 02-dashboard.html       # Mission Dashboard
│   ├── 03-mission-editor.html  # Mission Editor
│   ├── 04-spacecraft-builder.html  # Spacecraft Builder
│   ├── 05-orbit-visualization.html # 3D Orbit Visualization
│   ├── 06-maneuver-planner.html    # Maneuver Planner
│   ├── 07-propagation-control.html # Propagation Control
│   ├── 08-charts.html          # Time-Series Charts
│   ├── 09-ground-track.html    # Ground Track Map
│   ├── 10-reporting.html       # Reporting Interface
│   ├── 11-script-editor.html   # GMAT Script Editor
│   ├── 12-settings.html        # Settings/Preferences
│   ├── 13-profile.html         # User Profile
│   └── 14-notifications.html   # Notifications Panel
├── screenshots/                # PNG screenshots (generated)
│   └── (generated PNG files)
└── generate-screenshots.js     # Screenshot generation script
```

## Design System Overview

### Theme
- **Primary Theme**: Dark Mode (Angular Material Dark Theme)
- **Primary Color**: Blue (#2196f3)
- **Accent Color**: Orange (#ff9800)
- **Typography**: Roboto (Regular), Roboto Mono (Code)

### Key Design Principles
1. **Material Design 3** - Following Google's Material Design guidelines
2. **Dark Theme First** - Optimized for extended use in mission control environments
3. **Data Clarity** - Scientific data displayed with precision and clarity
4. **Accessibility** - WCAG 2.1 AA compliant

## Screen Mockups

### 1. Login (01-login.html)
- Centered card layout with space theme background
- Email/password authentication
- Social login options (Google, Microsoft, GitHub)
- MFA support indicator

### 2. Dashboard (02-dashboard.html)
- Mission cards in grid layout
- Statistics widgets (missions, spacecraft, delta-V)
- Quick filters (All, Active, Draft, Completed)
- Recent activity feed
- Side navigation

### 3. Mission Editor (03-mission-editor.html)
- Three-panel layout (tree, editor, properties)
- Mission metadata form
- Epoch configuration
- Force model settings
- Auto-save indicator

### 4. Spacecraft Builder (04-spacecraft-builder.html)
- Tabbed interface for properties
- Initial state vector input
- Hardware configuration list
- 3D spacecraft preview panel

### 5. Orbit Visualization (05-orbit-visualization.html)
- Full-screen 3D view with Earth
- Multiple orbit paths
- Spacecraft markers with labels
- Playback controls and timeline
- Floating info panel

### 6. Maneuver Planner (06-maneuver-planner.html)
- Maneuver card list
- Delta-V budget display
- Maneuver details editor
- Orbit preview (before/after)
- Optimization wizards

### 7. Propagation Control (07-propagation-control.html)
- Configuration panel (time, propagator, forces)
- Real-time status card
- Progress bar with estimates
- Console output
- Intermediate results

### 8. Time-Series Charts (08-charts.html)
- Parameter selection panel
- Line/area/scatter chart options
- Multi-spacecraft comparison
- Statistics panel
- Zoom and export controls

### 9. Ground Track Map (09-ground-track.html)
- 2D world map with ground tracks
- Spacecraft position markers
- Layer controls
- Playback timeline
- Coordinate display

### 10. Reporting (10-reporting.html)
- Report type selection
- Configuration options
- Live preview (PDF-like)
- Export options

### 11. Script Editor (11-script-editor.html)
- Monaco-style code editor
- Syntax highlighting for GMAT
- File browser
- Console output
- Line numbers and minimap

### 12. Settings (12-settings.html)
- Category navigation
- Theme selection
- Unit preferences
- Toggle switches
- Slider controls

### 13. User Profile (13-profile.html)
- Profile header with avatar
- Account statistics
- Security settings
- Connected accounts
- Active sessions

### 14. Notifications (14-notifications.html)
- Filter sidebar
- Notification list with grouping
- Unread indicators
- Toast notification example

## Viewing Mockups

### Option 1: Open in Browser
Simply open any HTML file in a modern web browser:
```bash
# Windows
start mockups/01-login.html

# macOS
open mockups/01-login.html

# Linux
xdg-open mockups/01-login.html
```

### Option 2: Live Server
Use VS Code Live Server extension or similar:
1. Install Live Server extension
2. Right-click on HTML file
3. Select "Open with Live Server"

## Generating Screenshots

### Prerequisites
```bash
npm install puppeteer
```

### Generate All Screenshots
```bash
node generate-screenshots.js
```

This will create PNG files in the `screenshots/` directory.

## Angular Material Implementation Notes

When implementing these designs in Angular:

### 1. Install Angular Material
```bash
ng add @angular/material
```

### 2. Choose Dark Theme
Select the "Custom" theme option and configure:
```scss
@use '@angular/material' as mat;

$ngmat-primary: mat.define-palette(mat.$blue-palette, 500);
$ngmat-accent: mat.define-palette(mat.$orange-palette, 500);
$ngmat-warn: mat.define-palette(mat.$red-palette, 500);

$ngmat-dark-theme: mat.define-dark-theme((
  color: (
    primary: $ngmat-primary,
    accent: $ngmat-accent,
    warn: $ngmat-warn,
  )
));

@include mat.all-component-themes($ngmat-dark-theme);
```

### 3. Component Mapping
| Mockup Element | Angular Material Component |
|----------------|---------------------------|
| Buttons | `mat-button`, `mat-raised-button`, `mat-fab` |
| Cards | `mat-card` |
| Forms | `mat-form-field`, `mat-input`, `mat-select` |
| Toolbar | `mat-toolbar` |
| Sidenav | `mat-sidenav` |
| Tabs | `mat-tab-group` |
| Lists | `mat-list`, `mat-nav-list` |
| Tables | `mat-table` |
| Dialogs | `mat-dialog` |
| Snackbar | `mat-snack-bar` |
| Progress | `mat-progress-bar`, `mat-spinner` |
| Toggles | `mat-slide-toggle` |
| Chips | `mat-chip` |

### 4. Icons
Use Material Icons (Outlined variant):
```html
<mat-icon>rocket_launch</mat-icon>
```

### 5. Fonts
Include in `index.html`:
```html
<link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&family=Roboto+Mono&display=swap" rel="stylesheet">
<link href="https://fonts.googleapis.com/icon?family=Material+Icons+Outlined" rel="stylesheet">
```

## Contributing

When updating designs:
1. Modify the HTML mockup file
2. Regenerate screenshots
3. Update this README if necessary
4. Commit changes with descriptive message

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-18 | Initial design system and mockups |
