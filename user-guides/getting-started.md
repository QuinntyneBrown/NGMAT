# Getting Started with NGMAT

This guide will help you install, set up, and start using NGMAT for space mission analysis.

## Table of Contents

- [Installation](#installation)
- [Accessing NGMAT](#accessing-ngmat)
- [User Interface Overview](#user-interface-overview)
- [Creating Your First Mission](#creating-your-first-mission)
- [Basic Navigation](#basic-navigation)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Next Steps](#next-steps)

## Installation

### Web Application

The NGMAT web application requires no installation. Simply:

1. Open your web browser (Chrome, Firefox, Edge, or Safari)
2. Navigate to the NGMAT web application URL (provided by your administrator)
3. Create an account or log in with existing credentials

**Minimum Requirements:**
- Modern web browser with JavaScript enabled
- Internet connection
- Screen resolution of 1024x768 or higher

### Desktop Application

For the full-featured desktop application:

1. **Download the Installer**
   - Visit the NGMAT Releases page on your deployment's repository
   - Download the latest installer for your operating system:
     - Windows: `NGMAT-Setup-x.x.x.msi`
     - macOS: `NGMAT-x.x.x.dmg` (if available)
     - Linux: `NGMAT-x.x.x.AppImage` (if available)

2. **Install NGMAT**
   - **Windows:** Double-click the `.msi` file and follow the installation wizard
   - **macOS:** Open the `.dmg` file and drag NGMAT to Applications
   - **Linux:** Make the `.AppImage` executable and run it

3. **Verify Installation**
   - Launch NGMAT from your Start Menu (Windows) or Applications (macOS)
   - The application should start and show the welcome screen

### Developer Setup

For developers who want to run NGMAT from source:

```bash
# Clone the repository (replace with your repository URL)
git clone <your-ngmat-repository-url>
cd NGMAT

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests to verify setup
dotnet test

# Run a specific service (example: Mission Management API)
cd src/MissionManagement/MissionManagement.Api
dotnet run
```

For detailed developer instructions, see the [Developer Setup Guide](../README.md#getting-started).

## Accessing NGMAT

### Creating an Account

1. Navigate to the NGMAT application (web or desktop)
2. Click **"Sign Up"** or **"Create Account"**
3. Enter your email address
4. Choose a strong password (minimum 8 characters, including letters and numbers)
5. Click **"Register"**
6. Check your email for a verification link
7. Click the verification link to activate your account

### Logging In

1. Open NGMAT
2. Enter your email address
3. Enter your password
4. Click **"Login"**

**Tips:**
- Check "Remember Me" to stay logged in (web application only)
- Enable Two-Factor Authentication (2FA) in settings for added security
- Use "Forgot Password?" if you need to reset your password

## User Interface Overview

### Web Application Interface

The NGMAT web interface consists of several key areas:

```
┌─────────────────────────────────────────────────────────┐
│  Header: Logo | Navigation | User Menu | Notifications  │
├─────────────┬───────────────────────────────────────────┤
│             │                                           │
│  Sidebar    │         Main Content Area                │
│             │                                           │
│  - Home     │  Dashboard, Editor, Visualizations, etc. │
│  - Missions │                                           │
│  - Reports  │                                           │
│  - Scripts  │                                           │
│  - Help     │                                           │
│             │                                           │
├─────────────┴───────────────────────────────────────────┤
│  Footer: Status | Version | Documentation Links         │
└─────────────────────────────────────────────────────────┘
```

#### Key UI Components

**Header:**
- **Navigation Menu** - Quick access to main sections
- **User Menu** - Profile, settings, logout
- **Notifications** - Real-time alerts and updates
- **Search** - Find missions, spacecraft, and documentation

**Sidebar:**
- **Home** - Dashboard with mission overview
- **Missions** - List and manage all missions
- **Reports** - Generate and view reports
- **Scripts** - Script editor and library
- **Settings** - User preferences and configuration
- **Help** - Documentation and support

**Main Content Area:**
- Dynamic content based on current section
- Forms, tables, charts, and 3D visualizations
- Context-specific toolbars and actions

**Status Bar:**
- Current operation status
- Connection status
- Application version

### Desktop Application Interface

The desktop application provides a more advanced interface:

```
┌─────────────────────────────────────────────────────────┐
│  Menu Bar: File | Edit | View | Mission | Tools | Help  │
├─────────────┬───────────────────────┬───────────────────┤
│             │                       │                   │
│  Explorer   │   Editor Tabs         │  Properties Panel │
│  Panel      │   (Multi-tab)         │                   │
│             │                       │  - Spacecraft     │
│  - Missions │   ┌─────────────────┐ │  - Maneuver       │
│    └─Mission│   │ Mission Config  │ │  - Propagator     │
│      ├─S/C  │   └─────────────────┘ │  - Force Model    │
│      └─Man. │                       │                   │
│             │                       │                   │
├─────────────┴───────────────────────┴───────────────────┤
│  Console / Output                                       │
│  > Propagation completed successfully...                │
└─────────────────────────────────────────────────────────┘
```

#### Desktop-Specific Features

- **Docking Panels** - Rearrange panels to suit your workflow
- **Multi-Tab Editor** - Work on multiple missions simultaneously
- **Console Output** - View detailed operation logs
- **Offline Mode** - Work without internet connection
- **High-Performance 3D** - Advanced visualization with GPU acceleration

## Creating Your First Mission

Let's create a simple Low Earth Orbit (LEO) satellite mission:

### Step 1: Create a New Mission

1. Click **"Missions"** in the sidebar (or File > New Mission)
2. Click the **"+ New Mission"** button
3. Fill in the mission details:
   - **Name:** "My First LEO Mission"
   - **Description:** "A simple LEO satellite mission"
   - **Mission Type:** Low Earth Orbit (LEO)
   - **Start Epoch:** (Use default or enter: 2026-01-18 12:00:00 UTC)
4. Click **"Create Mission"**

### Step 2: Add a Spacecraft

1. In the mission view, click **"+ Add Spacecraft"**
2. Enter spacecraft details:
   - **Name:** "LEO Sat 1"
   - **Dry Mass:** 500 kg
   - **Fuel Mass:** 100 kg
   - **Drag Coefficient (Cd):** 2.2
   - **Drag Area:** 10 m²
   - **SRP Area:** 8 m²
   - **Reflectivity:** 1.8
3. Click **"Next"** to set initial state

### Step 3: Set Initial Orbit

You can specify the initial orbit using Keplerian elements:

```
Semi-major Axis (a):     7000 km
Eccentricity (e):        0.001
Inclination (i):         51.6°
RAAN (Ω):               0°
Argument of Perigee (ω): 0°
True Anomaly (ν):        0°
Epoch:                   2026-01-18 12:00:00 UTC
Coordinate System:       ECI J2000
```

Or use a state vector if you prefer:
```
Position X: 3000 km
Position Y: 5000 km
Position Z: 3000 km
Velocity X: -5.0 km/s
Velocity Y: 3.0 km/s
Velocity Z: 4.0 km/s
```

4. Click **"Create Spacecraft"**

### Step 4: Propagate the Orbit

1. Click **"Propagate"** in the mission toolbar
2. Configure propagation settings:
   - **End Epoch:** 1 day later (2026-01-19 12:00:00 UTC)
   - **Step Size:** 60 seconds
   - **Propagator:** RK45 (Runge-Kutta-Fehlberg)
   - **Force Models:** 
     - ☑ Earth Gravity (Degree 4, Order 4)
     - ☑ Atmospheric Drag
     - ☑ Solar Radiation Pressure
3. Click **"Run Propagation"**
4. Wait for propagation to complete (progress bar will show status)

### Step 5: Visualize Results

Once propagation completes:

1. Click **"Visualize"** tab
2. Select **"3D Orbit View"**
3. Use mouse to rotate, zoom, and pan:
   - **Left-click + drag** - Rotate view
   - **Right-click + drag** - Pan view
   - **Scroll wheel** - Zoom in/out
4. Click **"Play"** to animate the orbit
5. View the ground track on the map panel

### Step 6: Review Data

1. Click **"Results"** tab
2. View state vectors table (position and velocity over time)
3. Click **"Charts"** to see:
   - Altitude vs. Time
   - Velocity vs. Time
   - Orbital elements vs. Time
4. Export data if needed (CSV, JSON formats available)

Congratulations! You've created and analyzed your first mission in NGMAT.

## Basic Navigation

### Web Application Navigation

**Mouse Navigation:**
- **Click** - Select items, buttons, and links
- **Double-click** - Open missions or spacecraft
- **Right-click** - Context menus (where applicable)
- **Scroll** - Navigate long lists and pages

**Keyboard Navigation:**
- **Tab** - Move between form fields
- **Enter** - Submit forms or confirm actions
- **Esc** - Close dialogs or cancel operations
- **Arrow keys** - Navigate lists and tables

### Desktop Application Navigation

In addition to web navigation:

**Window Management:**
- **Ctrl+Tab** - Switch between open tabs
- **Ctrl+W** - Close current tab
- **F11** - Toggle fullscreen mode

**Panel Management:**
- **Drag panel headers** - Rearrange panels
- **Double-click panel header** - Float/dock panel
- **Right-click panel header** - Panel options

## Keyboard Shortcuts

### Global Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New Mission |
| `Ctrl+O` | Open Mission |
| `Ctrl+S` | Save Mission |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+W` | Close Tab/Window |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `Ctrl+F` | Find/Search |
| `F1` | Help |
| `F5` | Refresh |

### Mission Editor Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+N` | New Spacecraft |
| `Ctrl+Shift+M` | New Maneuver |
| `Ctrl+P` | Run Propagation |
| `Ctrl+R` | Generate Report |
| `Ctrl+E` | Export Data |

### Visualization Shortcuts

| Shortcut | Action |
|----------|--------|
| `Space` | Play/Pause Animation |
| `Arrow Left` | Step Backward |
| `Arrow Right` | Step Forward |
| `Home` | Reset View |
| `+/-` | Zoom In/Out |

### Script Editor Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Enter` | Run Script |
| `Ctrl+/` | Toggle Comment |
| `Ctrl+D` | Duplicate Line |
| `Ctrl+L` | Delete Line |
| `Ctrl+Space` | Auto-complete |

## Next Steps

Now that you're familiar with NGMAT basics, explore these topics:

### Essential Skills
1. [**Mission Management**](mission-management.md) - Learn to organize and manage multiple missions
2. [**Spacecraft Configuration**](spacecraft-configuration.md) - Master spacecraft setup and configuration
3. [**Orbit Propagation**](orbit-propagation.md) - Understand propagation options and force models

### Common Tasks
4. [**Maneuver Planning**](maneuver-planning.md) - Plan orbital maneuvers and transfers
5. [**Visualization**](visualization.md) - Create stunning visualizations of your missions
6. [**Reporting**](reporting-export.md) - Generate professional mission reports

### Advanced Features
7. [**Scripting**](scripting-automation.md) - Automate workflows with scripts
8. [**Optimization**](advanced-topics.md#optimization) - Optimize mission parameters
9. [**Batch Processing**](advanced-topics.md#batch-processing) - Process multiple scenarios

## Getting Help

If you encounter issues:

1. **Check Documentation** - Review relevant user guide sections
2. **Search FAQ** - See [Troubleshooting and FAQ](troubleshooting-faq.md)
3. **View Tutorials** - Look for video tutorials in the Help menu
4. **Ask Community** - Post questions in GitHub Discussions
5. **Report Issues** - Submit bug reports via GitHub Issues

## Additional Resources

- [**Video Tutorials**](https://github.com/QuinntyneBrown/NGMAT/wiki/tutorials) - Step-by-step video guides
- [**Example Missions**](https://github.com/QuinntyneBrown/NGMAT/tree/main/examples) - Sample missions to learn from
- [**API Documentation**](../README.md) - For developers and advanced users
- [**Community Forum**](https://github.com/QuinntyneBrown/NGMAT/discussions) - Connect with other users

---

**Next:** [Mission Management Guide](mission-management.md) - Learn to create, organize, and collaborate on missions.
