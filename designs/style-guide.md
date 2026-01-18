# NGMAT Design System & Style Guide

## Overview
This document defines the visual design system for NGMAT (Next Generation Mission Analysis Tool), an Angular Material-based web application for aerospace mission planning and analysis.

---

## 1. Design Principles

### 1.1 Core Principles
- **Clarity**: Scientific data must be presented clearly and accurately
- **Efficiency**: Minimize clicks and cognitive load for complex operations
- **Consistency**: Use Material Design 3 patterns throughout
- **Accessibility**: WCAG 2.1 AA compliance
- **Responsiveness**: Desktop-first with mobile support

### 1.2 Target Users
- Aerospace engineers
- Mission planners
- Orbital mechanics specialists
- Students and researchers

---

## 2. Color System

### 2.1 Dark Theme Primary Palette
```scss
// Primary Colors
$primary-50: #e3f2fd;
$primary-100: #bbdefb;
$primary-200: #90caf9;
$primary-300: #64b5f6;
$primary-400: #42a5f5;
$primary-500: #2196f3;  // Primary
$primary-600: #1e88e5;
$primary-700: #1976d2;
$primary-800: #1565c0;
$primary-900: #0d47a1;

// Accent Colors
$accent-50: #fff3e0;
$accent-100: #ffe0b2;
$accent-200: #ffcc80;
$accent-300: #ffb74d;
$accent-400: #ffa726;
$accent-500: #ff9800;  // Accent
$accent-600: #fb8c00;
$accent-700: #f57c00;
$accent-800: #ef6c00;
$accent-900: #e65100;

// Warn Colors
$warn-500: #f44336;  // Red
$warn-600: #e53935;
$warn-700: #d32f2f;
```

### 2.2 Dark Theme Surface Colors
```scss
// Background
$surface-background: #121212;
$surface-card: #1e1e1e;
$surface-dialog: #2d2d2d;
$surface-elevated: #383838;
$surface-hover: rgba(255, 255, 255, 0.08);
$surface-selected: rgba(33, 150, 243, 0.16);

// Text
$text-primary: rgba(255, 255, 255, 0.87);
$text-secondary: rgba(255, 255, 255, 0.60);
$text-disabled: rgba(255, 255, 255, 0.38);
$text-hint: rgba(255, 255, 255, 0.38);
```

### 2.3 Status Colors
```scss
// Status Indicators
$status-success: #4caf50;
$status-info: #2196f3;
$status-warning: #ff9800;
$status-error: #f44336;
$status-draft: #9e9e9e;
$status-active: #4caf50;
$status-completed: #2196f3;
```

### 2.4 Visualization Colors
```scss
// Orbit Visualization
$orbit-primary: #00bcd4;    // Cyan - Primary orbit
$orbit-secondary: #ff9800;  // Orange - Secondary orbit
$orbit-target: #4caf50;     // Green - Target orbit
$orbit-maneuver: #f44336;   // Red - Maneuver points
$orbit-ground-track: #ffeb3b; // Yellow - Ground track

// Chart Colors
$chart-series-1: #2196f3;
$chart-series-2: #ff9800;
$chart-series-3: #4caf50;
$chart-series-4: #9c27b0;
$chart-series-5: #00bcd4;
$chart-series-6: #f44336;
```

---

## 3. Typography

### 3.1 Font Family
```scss
$font-family-primary: 'Roboto', sans-serif;
$font-family-monospace: 'Roboto Mono', monospace;  // For code/data
```

### 3.2 Type Scale
```scss
// Headlines
$headline-1: 96px / 1.167 / -1.5px;
$headline-2: 60px / 1.2 / -0.5px;
$headline-3: 48px / 1.167 / 0;
$headline-4: 34px / 1.235 / 0.25px;
$headline-5: 24px / 1.334 / 0;
$headline-6: 20px / 1.6 / 0.15px;

// Subtitles
$subtitle-1: 16px / 1.75 / 0.15px;
$subtitle-2: 14px / 1.57 / 0.1px;

// Body
$body-1: 16px / 1.5 / 0.5px;
$body-2: 14px / 1.43 / 0.25px;

// Button/Caption/Overline
$button: 14px / 1.75 / 1.25px / uppercase;
$caption: 12px / 1.66 / 0.4px;
$overline: 10px / 2.66 / 1.5px / uppercase;
```

### 3.3 Font Weights
```scss
$font-weight-light: 300;
$font-weight-regular: 400;
$font-weight-medium: 500;
$font-weight-bold: 700;
```

---

## 4. Spacing System

### 4.1 Base Unit
```scss
$spacing-unit: 8px;

$spacing-xs: 4px;   // 0.5x
$spacing-sm: 8px;   // 1x
$spacing-md: 16px;  // 2x
$spacing-lg: 24px;  // 3x
$spacing-xl: 32px;  // 4x
$spacing-xxl: 48px; // 6x
```

### 4.2 Layout Margins
```scss
$layout-margin-desktop: 24px;
$layout-margin-tablet: 16px;
$layout-margin-mobile: 16px;

$layout-gutter: 24px;
```

---

## 5. Elevation System

### 5.1 Material Elevation Levels
```scss
// Dark theme shadows use lighter overlays
$elevation-0: none;
$elevation-1: 0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24);
$elevation-2: 0 3px 6px rgba(0,0,0,0.15), 0 2px 4px rgba(0,0,0,0.12);
$elevation-4: 0 10px 20px rgba(0,0,0,0.15), 0 3px 6px rgba(0,0,0,0.10);
$elevation-8: 0 15px 25px rgba(0,0,0,0.15), 0 5px 10px rgba(0,0,0,0.05);
$elevation-16: 0 20px 40px rgba(0,0,0,0.2);
$elevation-24: 0 25px 50px rgba(0,0,0,0.25);
```

### 5.2 Component Elevations
| Component | Elevation |
|-----------|-----------|
| App Bar | 4 |
| Card | 1 |
| Dialog | 24 |
| Drawer | 16 |
| FAB | 6 (resting), 12 (pressed) |
| Menu | 8 |
| Snackbar | 6 |
| Tooltip | 4 |

---

## 6. Border Radius

```scss
$radius-none: 0;
$radius-xs: 2px;
$radius-sm: 4px;
$radius-md: 8px;
$radius-lg: 12px;
$radius-xl: 16px;
$radius-full: 9999px;

// Component-specific
$radius-button: 4px;
$radius-card: 8px;
$radius-chip: 16px;
$radius-dialog: 8px;
$radius-input: 4px;
```

---

## 7. Components

### 7.1 Buttons
```html
<!-- Primary Button -->
<button mat-raised-button color="primary">Primary Action</button>

<!-- Secondary Button -->
<button mat-stroked-button color="primary">Secondary</button>

<!-- Icon Button -->
<button mat-icon-button><mat-icon>add</mat-icon></button>

<!-- FAB -->
<button mat-fab color="accent"><mat-icon>add</mat-icon></button>
```

### 7.2 Cards
```html
<mat-card class="mission-card">
  <mat-card-header>
    <mat-card-title>Mission Name</mat-card-title>
    <mat-card-subtitle>Status Badge</mat-card-subtitle>
  </mat-card-header>
  <mat-card-content>
    <!-- Content -->
  </mat-card-content>
  <mat-card-actions>
    <!-- Actions -->
  </mat-card-actions>
</mat-card>
```

### 7.3 Form Fields
```html
<mat-form-field appearance="outline">
  <mat-label>Field Label</mat-label>
  <input matInput placeholder="Placeholder">
  <mat-hint>Helper text</mat-hint>
  <mat-error>Error message</mat-error>
</mat-form-field>
```

### 7.4 Navigation
```html
<!-- Side Navigation -->
<mat-sidenav-container>
  <mat-sidenav mode="side" opened>
    <mat-nav-list>
      <a mat-list-item routerLink="/dashboard">
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>
</mat-sidenav-container>
```

---

## 8. Icons

### 8.1 Icon Library
- Use Material Icons (Filled, Outlined, or Rounded)
- Prefer Outlined style for consistency
- Icon size: 24px (default), 18px (dense), 36px (large)

### 8.2 Custom Icons
Mission-specific icons for:
- Spacecraft
- Orbit
- Maneuver
- Propagation
- Ground Station

---

## 9. Layout Patterns

### 9.1 App Shell
```
+------------------------------------------+
| App Bar (Toolbar)                        |
+--------+--------------------------------+
| Side   |                                 |
| Nav    |     Main Content Area           |
| (240px)|                                 |
|        |                                 |
+--------+--------------------------------+
```

### 9.2 Dashboard Grid
- Use CSS Grid or Flexbox
- Card-based layout
- 3 columns (desktop), 2 columns (tablet), 1 column (mobile)

### 9.3 Editor Layout
- Split panels for form/preview
- Collapsible property panels
- Toolbar at top

---

## 10. Responsive Breakpoints

```scss
$breakpoint-xs: 0;
$breakpoint-sm: 600px;
$breakpoint-md: 960px;
$breakpoint-lg: 1280px;
$breakpoint-xl: 1920px;

// Angular CDK Breakpoints
@media (max-width: 599.98px) { /* Handset portrait */ }
@media (min-width: 600px) and (max-width: 959.98px) { /* Tablet portrait */ }
@media (min-width: 960px) and (max-width: 1279.98px) { /* Tablet landscape */ }
@media (min-width: 1280px) { /* Desktop */ }
```

---

## 11. Animation Guidelines

### 11.1 Timing
```scss
$duration-short: 150ms;
$duration-standard: 300ms;
$duration-long: 500ms;

$easing-standard: cubic-bezier(0.4, 0.0, 0.2, 1);
$easing-decelerate: cubic-bezier(0.0, 0.0, 0.2, 1);
$easing-accelerate: cubic-bezier(0.4, 0.0, 1, 1);
```

### 11.2 Transitions
- Use Angular animations for route transitions
- Mat-ripple for touch feedback
- Smooth transitions for panel expand/collapse

---

## 12. Data Display

### 12.1 Tables
```html
<table mat-table [dataSource]="data">
  <ng-container matColumnDef="name">
    <th mat-header-cell *matHeaderCellDef>Name</th>
    <td mat-cell *matCellDef="let row">{{row.name}}</td>
  </ng-container>
</table>
```

### 12.2 Numerical Data
- Use monospace font for numerical values
- Right-align numbers in tables
- Display units consistently
- Use locale-specific formatting

### 12.3 Scientific Notation
```
1.234567e+06  // For large numbers
1.234e-09     // For small numbers
```

---

## 13. Accessibility

### 13.1 Requirements
- Color contrast ratio: 4.5:1 (normal text), 3:1 (large text)
- Keyboard navigation support
- ARIA labels for interactive elements
- Focus indicators
- Screen reader announcements

### 13.2 Focus States
```scss
.focused {
  outline: 2px solid $primary-500;
  outline-offset: 2px;
}
```

---

## 14. Screen Specifications

### 14.1 Login Screen
- Centered card layout
- Logo at top
- Email/password fields
- Social login buttons
- Links: Forgot password, Register

### 14.2 Dashboard
- Grid of mission cards
- Quick actions toolbar
- Search and filter bar
- Statistics widgets

### 14.3 Mission Editor
- Form in main area
- Save/Cancel actions
- Validation feedback

### 14.4 Spacecraft Builder
- Tabbed interface
- Property panels
- 3D preview sidebar

### 14.5 Orbit Visualization
- Full-screen 3D canvas
- Floating controls overlay
- Time slider at bottom
- Info panel (collapsible)

### 14.6 Maneuver Planner
- Timeline view
- Maneuver list
- Delta-V budget display
- Optimization wizards

### 14.7 Charts View
- Chart area
- Parameter selector
- Export options
- Time range controls

### 14.8 Ground Track Map
- Full map view
- Layer controls
- Track animation controls

### 14.9 Script Editor
- Monaco editor
- Output console
- File browser sidebar

### 14.10 Settings
- Categorized settings
- Toggle switches
- Save confirmation

---

## 15. File Naming Convention

### 15.1 Design Assets
```
designs/
├── style-guide.md
├── mockups/
│   ├── 01-login.html
│   ├── 02-dashboard.html
│   ├── 03-mission-editor.html
│   └── ...
└── screenshots/
    ├── 01-login.png
    ├── 02-dashboard.png
    └── ...
```

### 15.2 Component Files (BEM)
```
.mission-card { }
.mission-card__header { }
.mission-card__title { }
.mission-card__actions { }
.mission-card--active { }
.mission-card--draft { }
```

---

## 16. Theme Configuration

### 16.1 Angular Material Theme
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
  ),
  typography: mat.define-typography-config(),
  density: 0,
));

@include mat.all-component-themes($ngmat-dark-theme);
```

---

## 17. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-18 | Initial design system |

