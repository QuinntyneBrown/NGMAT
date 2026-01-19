# NGMAT User Guide

Welcome to the NGMAT (General Mission Analysis Tool) User Guide. This comprehensive documentation will help you get started with NGMAT and guide you through all aspects of space mission analysis and design.

## About NGMAT

NGMAT is a .NET 8.0 implementation of NASA's General Mission Analysis Tool (GMAT) - a space mission analysis and design platform built with modern microservices architecture. It provides comprehensive tools for:

- **Space mission planning** - Design and analyze missions from LEO to deep space
- **Orbit propagation** - Simulate spacecraft trajectories with high precision
- **Maneuver optimization** - Plan and optimize orbital maneuvers
- **Spacecraft modeling** - Configure spacecraft hardware and properties
- **Visualization** - Visualize orbits, ground tracks, and mission data
- **Reporting** - Generate comprehensive mission reports and data exports

## Target Audience

This user guide is designed for:

- **Mission Analysts** - Planning and analyzing space missions
- **Aerospace Engineers** - Designing spacecraft trajectories and maneuvers
- **Students** - Learning orbital mechanics and mission design
- **Researchers** - Conducting space mission studies
- **Educators** - Teaching space mission analysis concepts

## System Requirements

### Web Application
- Modern web browser (Chrome, Firefox, Edge, Safari)
- Internet connection
- Minimum screen resolution: 1024x768

### Desktop Application
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime or later
- 4 GB RAM minimum (8 GB recommended)
- 2 GB available disk space
- Graphics card with OpenGL 4.5+ support

### Developer Setup
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Docker (optional, for containerized deployment)

## User Guide Structure

This user guide is organized into the following sections:

### 1. [Getting Started](getting-started.md)
Learn how to set up and access NGMAT, create your first mission, and navigate the user interface.

### 2. [Mission Management](mission-management.md)
Create, configure, and manage your space missions. Learn about mission lifecycle, collaboration, and organization.

### 3. [Spacecraft Configuration](spacecraft-configuration.md)
Define spacecraft properties, configure hardware, set initial states, and manage fuel budgets.

### 4. [Orbit Propagation](orbit-propagation.md)
Propagate orbits using various numerical integrators, configure force models, and analyze trajectory data.

### 5. [Maneuver Planning](maneuver-planning.md)
Plan impulsive and finite burns, optimize transfers, and analyze delta-V budgets.

### 6. [Coordinate Systems](coordinate-systems.md)
Work with different reference frames, perform coordinate transformations, and convert between state representations.

### 7. [Visualization](visualization.md)
Visualize orbits in 3D, plot ground tracks, generate time-series charts, and analyze mission data visually.

### 8. [Reporting and Export](reporting-export.md)
Generate mission reports, export data in various formats, and create documentation for your missions.

### 9. [Scripting and Automation](scripting-automation.md)
Use GMAT-compatible scripts to automate mission analysis and create reusable workflows.

### 10. [Advanced Topics](advanced-topics.md)
Learn about optimization algorithms, event detection, batch processing, and advanced features.

### 11. [Troubleshooting and FAQ](troubleshooting-faq.md)
Find solutions to common issues and answers to frequently asked questions.

## Quick Links

### Essential Guides
- [Installation and Setup](getting-started.md#installation)
- [Create Your First Mission](getting-started.md#creating-your-first-mission)
- [Basic Orbit Propagation](orbit-propagation.md#basic-propagation)
- [Planning a Hohmann Transfer](maneuver-planning.md#hohmann-transfer)

### Common Tasks
- [Import TLE Data](spacecraft-configuration.md#importing-tle)
- [Export Mission Data](reporting-export.md#exporting-data)
- [Share a Mission](mission-management.md#sharing-missions)
- [Generate Reports](reporting-export.md#generating-reports)

### Reference Materials
- [Coordinate System Reference](coordinate-systems.md#reference)
- [API Documentation](../README.md#services)
- [Script Command Reference](scripting-automation.md#command-reference)
- [Keyboard Shortcuts](getting-started.md#keyboard-shortcuts)

## Getting Help

If you need assistance:

1. **Check the FAQ** - [Troubleshooting and FAQ](troubleshooting-faq.md)
2. **Review Documentation** - Browse relevant sections of this user guide
3. **Search Issues** - Check the [GitHub Issues](https://github.com/QuinntyneBrown/NGMAT/issues)
4. **Ask Questions** - Create a new GitHub issue with the `question` label
5. **Community Support** - Join discussions in the repository

## Contributing to Documentation

This documentation is open source and welcomes contributions. If you find errors, have suggestions, or want to add content:

1. Fork the repository
2. Make your changes in the `user-guides/` directory
3. Submit a pull request with a clear description
4. Follow the [documentation style guide](../docs/coding-guidelines.md)

## Additional Resources

- **Technical Documentation** - [docs/](../docs/)
- **API Reference** - See individual service README files in [src/](../src/)
- **Requirements Specification** - [docs/requirements.md](../docs/requirements.md)
- **Architecture Overview** - [README.md](../README.md#architecture)
- **Coding Guidelines** - [docs/coding-guidelines.md](../docs/coding-guidelines.md)

## Version Information

- **User Guide Version:** 1.0
- **NGMAT Version:** Compatible with NGMAT v0.1.0+
- **Last Updated:** January 2026

## Feedback

Your feedback helps improve this documentation. Please report issues, suggest improvements, or share your experience using NGMAT through GitHub Issues.

---

**Ready to get started?** Begin with the [Getting Started Guide](getting-started.md) to set up NGMAT and create your first mission!
