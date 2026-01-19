# Mission Management Guide

Learn how to create, configure, organize, and collaborate on space missions in NGMAT.

## Table of Contents

- [Mission Basics](#mission-basics)
- [Creating Missions](#creating-missions)
- [Mission Configuration](#mission-configuration)
- [Mission Lifecycle](#mission-lifecycle)
- [Organizing Missions](#organizing-missions)
- [Sharing and Collaboration](#sharing-and-collaboration)
- [Mission Templates](#mission-templates)
- [Import and Export](#import-and-export)
- [Best Practices](#best-practices)

## Mission Basics

### What is a Mission?

A **mission** in NGMAT is a container for all elements of a space mission analysis:

- **Mission Metadata** - Name, description, type, and time period
- **Spacecraft** - One or more spacecraft with properties and states
- **Maneuvers** - Planned orbital maneuvers and burns
- **Propagation Settings** - Numerical integrators and force models
- **Coordinate Systems** - Reference frames used in the mission
- **Results** - Propagated trajectories and analysis data
- **Reports** - Generated documentation and exports

### Mission Types

NGMAT supports various mission types:

| Mission Type | Description | Typical Orbit Regimes |
|--------------|-------------|----------------------|
| **LEO** | Low Earth Orbit | 200-2000 km altitude |
| **GEO** | Geostationary Orbit | 35,786 km altitude |
| **MEO** | Medium Earth Orbit | 2,000-35,786 km altitude |
| **HEO** | Highly Elliptical Orbit | Variable perigee/apogee |
| **Lunar** | Moon missions | Cislunar and lunar orbit |
| **Interplanetary** | Deep space missions | Beyond Earth's SOI |
| **Custom** | User-defined | Any orbit regime |

## Creating Missions

### Create a New Mission

**Web Application:**
1. Click **"Missions"** in the sidebar
2. Click **"+ New Mission"** button
3. Fill in the mission creation form
4. Click **"Create"**

**Desktop Application:**
1. Select **File > New Mission** (or press `Ctrl+N`)
2. Complete the mission wizard
3. Click **"Finish"**

### Mission Creation Form

#### Required Fields

**Mission Name** (Required)
- Must be unique per user
- 3-100 characters
- Use descriptive names: "ISS Reboost 2026" instead of "Mission1"

**Mission Type** (Required)
- Select from predefined types or choose "Custom"
- Affects default settings and templates

**Start Epoch** (Required)
- Mission start date and time
- Format: YYYY-MM-DD HH:MM:SS UTC
- Example: `2026-01-18 12:00:00`

#### Optional Fields

**Description**
- Detailed mission description
- Supports markdown formatting
- Include mission objectives, constraints, and notes

**End Epoch**
- Mission end date and time
- Can be left empty and determined by propagation

**Tags**
- Categorize missions with tags
- Examples: "operational", "study", "training"

### Quick Create Options

Use templates for faster mission creation:

**Template 1: LEO Satellite**
- Circular orbit at 400 km
- 51.6° inclination
- Default spacecraft properties

**Template 2: GEO Communications Satellite**
- Geostationary orbit
- 0° inclination
- Default comsat properties

**Template 3: Lunar Transfer**
- Earth parking orbit
- Trans-lunar injection maneuver
- Lunar orbit insertion

## Mission Configuration

### Edit Mission Properties

1. Open the mission
2. Click **"Mission Settings"** or **"Edit"**
3. Modify desired properties
4. Click **"Save Changes"**

### Configurable Properties

#### General Settings

**Mission Name and Description**
- Update at any time
- Name must remain unique

**Time Settings**
- **Start Epoch** - Mission start time
- **End Epoch** - Mission end time
- **Time System** - UTC, TDB, TAI, etc.

#### Analysis Settings

**Coordinate System**
- Default coordinate system for mission
- Options: ECI J2000, ECEF, Moon-Centered, etc.

**Central Body**
- Primary gravitational body
- Options: Earth, Moon, Sun, Mars, etc.

**Units Preferences**
- Length: km, m, AU
- Mass: kg, lbm
- Time: s, min, hr, day
- Angle: rad, deg

#### Propagation Defaults

**Default Propagator**
- RK45, RK89, RK4, Adams-Bashforth

**Default Step Size**
- Typical: 60-300 seconds for LEO
- Larger steps for higher orbits

**Force Model Defaults**
- Gravity degree and order
- Drag model selection
- SRP on/off

## Mission Lifecycle

### Mission Status States

Missions progress through defined states:

```
Draft → Active → Completed → Archived
  ↓       ↓         ↓
  └───────┴─────────┴──────→ Deleted
```

**Draft**
- Initial state after creation
- Mission configuration in progress
- Can be freely modified

**Active**
- Mission is being actively worked on
- Propagation and analysis in progress
- Changes tracked in history

**Completed**
- Mission analysis finished
- Results finalized
- Read-only (can clone to modify)

**Archived**
- Long-term storage
- Not shown in default views
- Can be restored if needed

**Deleted**
- Soft-deleted (not permanently removed)
- Can be recovered within 30 days
- Permanently deleted after retention period

### Change Mission Status

1. Open the mission
2. Click **"Status"** dropdown
3. Select new status
4. Confirm the change

**Status Transition Rules:**
- Draft → Active (anytime)
- Active → Completed (when analysis done)
- Completed → Archived (for long-term storage)
- Any status → Deleted (soft delete)

## Organizing Missions

### Mission List Views

**All Missions** - View all your missions

**Recent** - Recently opened or modified missions

**Favorites** - Missions you've marked as favorites

**Shared with Me** - Missions others have shared with you

**By Status** - Filter by Draft, Active, Completed, Archived

**By Type** - Filter by mission type (LEO, GEO, Lunar, etc.)

### Search and Filter

**Search Box**
- Search by mission name or description
- Partial matches supported
- Case-insensitive

**Filters**
- **Status:** Draft, Active, Completed, Archived
- **Type:** LEO, GEO, Lunar, Interplanetary, etc.
- **Date Range:** Created or modified within range
- **Tags:** Filter by assigned tags

**Sort Options**
- Name (A-Z or Z-A)
- Created Date (newest or oldest first)
- Modified Date (most recent first)
- Status

### Folders and Organization

**Create Folders** (Desktop Application)
1. Right-click in Mission Explorer
2. Select **"New Folder"**
3. Enter folder name
4. Drag missions into folder

**Tags** (All Applications)
1. Open mission
2. Click **"Tags"** field
3. Type tag name and press Enter
4. Multiple tags supported

### Favorites

Mark important missions as favorites:
1. Hover over mission in list
2. Click the **star icon**
3. Mission appears in Favorites view

## Sharing and Collaboration

### Share a Mission

1. Open the mission
2. Click **"Share"** button
3. Enter collaborator's email address
4. Select permission level:
   - **View Only** - Can view but not modify
   - **Edit** - Can view and modify
   - **Admin** - Full control including sharing
5. Click **"Send Invitation"**

The collaborator receives an email with a link to access the mission.

### Manage Collaborators

**View Current Collaborators**
1. Open mission
2. Click **"Collaborators"** tab
3. See list of users with access

**Change Permissions**
1. Click on collaborator
2. Select new permission level
3. Click **"Update"**

**Revoke Access**
1. Click **"Remove"** next to collaborator name
2. Confirm removal
3. User loses immediate access

### Collaboration Best Practices

**Use Comments**
- Add comments to document changes
- Tag users with @mention for notifications

**Track Changes**
- Review activity log regularly
- See who made what changes and when

**Version Control**
- Clone mission before major changes
- Keep backup copies of important versions

**Communicate**
- Use mission description for objectives and notes
- Document assumptions and constraints

## Mission Templates

### Using Templates

Templates provide starting points for common mission types:

1. Click **"New Mission"**
2. Select **"From Template"**
3. Choose a template
4. Customize the mission
5. Click **"Create"**

### Built-in Templates

**LEO Satellite**
- Circular orbit at 400 km
- 51.6° inclination (ISS-like)
- Basic spacecraft properties

**GEO Communications**
- Geostationary orbit
- 0° inclination
- Comsat properties

**Sun-Synchronous Orbit**
- ~800 km altitude
- 98.2° inclination
- Earth observation configuration

**Lunar Mission**
- Earth parking orbit
- TLI maneuver
- Lunar orbit insertion

**Mars Transfer**
- Earth departure orbit
- Heliocentric transfer
- Mars arrival

### Create Custom Templates

Save frequently used configurations as templates:

1. Configure a mission completely
2. Click **"Mission"** menu
3. Select **"Save as Template"**
4. Enter template name and description
5. Select which elements to include:
   - Spacecraft configuration
   - Maneuver sequence
   - Propagation settings
   - Force models
6. Click **"Save Template"**

Templates are saved to your account and can be reused for new missions.

## Import and Export

### Export Mission

Export missions for backup, sharing, or external analysis:

1. Open the mission
2. Click **"Export"** button
3. Select export format:
   - **JSON** - Complete mission data in JSON format
   - **GMAT Script** - Compatible with NASA GMAT
   - **ZIP Archive** - All files and data
4. Choose export options
5. Click **"Export"**
6. Save file to desired location

### Import Mission

Import missions from files:

1. Click **"Import Mission"**
2. Select file to import:
   - NGMAT JSON (.json)
   - GMAT Script (.script)
   - ZIP Archive (.zip)
3. Review mission preview
4. Click **"Import"**
5. Mission is added to your mission list

### Supported Formats

**NGMAT JSON Format**
- Native format for NGMAT
- Complete mission data
- Recommended for backup and sharing

**GMAT Script Format**
- Compatible with NASA GMAT
- Best-effort import (not all features supported)
- Useful for migrating from GMAT

**TLE (Two-Line Elements)**
- Import spacecraft state from TLE
- See [Spacecraft Configuration Guide](spacecraft-configuration.md#importing-tle)

## Best Practices

### Naming Conventions

**Mission Names**
- Use descriptive, meaningful names
- Include mission purpose or objective
- Example: "Starlink-2024-Deployment" not "Mission1"

**Spacecraft Names**
- Include identifier or purpose
- Example: "CommSat-1A", "ISS-Progress-MS-18"

**Maneuver Names**
- Describe maneuver type and purpose
- Example: "Apogee-Raise-1", "Plane-Change-To-ISS"

### Documentation

**Write Clear Descriptions**
- Document mission objectives
- List key assumptions
- Note any constraints or requirements

**Add Comments**
- Comment on important decisions
- Explain non-obvious configurations
- Document sources of data

**Track Changes**
- Review activity log periodically
- Document major milestones

### Data Management

**Regular Backups**
- Export important missions regularly
- Keep backups in multiple locations
- Use version control for critical missions

**Clean Up**
- Archive completed missions
- Delete obsolete test missions
- Organize with folders and tags

**Naming Consistency**
- Use consistent naming scheme across missions
- Makes searching and filtering easier

### Collaboration

**Set Clear Permissions**
- Give minimum necessary access
- Use View Only when possible
- Reserve Admin for trusted collaborators

**Communicate Changes**
- Use comments to explain changes
- Notify team of major updates
- Document decision rationale

**Coordinate Work**
- Avoid simultaneous editing
- Use mission cloning for experiments
- Merge changes carefully

## Troubleshooting

### Common Issues

**Issue: Cannot delete a mission**
- **Cause:** Mission may be locked or in use
- **Solution:** Ensure mission is not open in another tab/window

**Issue: Mission not appearing in list**
- **Cause:** Filters may be hiding it
- **Solution:** Clear all filters and search

**Issue: Changes not saving**
- **Cause:** Network connectivity issues
- **Solution:** Check internet connection; changes auto-save when reconnected

**Issue: Cannot share mission**
- **Cause:** User does not have share permissions
- **Solution:** Contact mission owner to request Admin access

### Getting Help

For additional assistance:
- See [Troubleshooting and FAQ](troubleshooting-faq.md)
- Review [API Documentation](../README.md)
- Ask in [Community Forum](https://github.com/QuinntyneBrown/NGMAT/discussions)

---

**Next:** [Spacecraft Configuration Guide](spacecraft-configuration.md) - Learn to define and configure spacecraft properties.
