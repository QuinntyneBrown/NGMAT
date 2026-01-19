# NGMAT

A .NET 8.0 implementation of the General Mission Analysis Tool (GMAT) - a space mission analysis and design platform built with microservices architecture.

## Overview

NGMAT provides comprehensive tools for space mission planning, orbit propagation, maneuver optimization, and spacecraft analysis. The system is designed using Clean Architecture principles with a distributed microservices approach.

## Architecture

The solution follows Clean Architecture with three layers per service:
- **Core** - Domain entities, interfaces, and business logic
- **Infrastructure** - Data access, external integrations, and implementations
- **Api** - REST API endpoints with OpenAPI/Swagger documentation

## Services

| Service | Description |
|---------|-------------|
| **Identity** | Authentication and authorization |
| **EventStore** | Event sourcing and persistence |
| **CalculationEngine** | Mathematical computations and algorithms |
| **CoordinateSystem** | Coordinate frame transformations and conversions |
| **Ephemeris** | Celestial body position and velocity data |
| **ForceModel** | Gravitational and non-gravitational force models |
| **Propagation** | Orbit propagation and trajectory simulation |
| **MissionManagement** | Mission planning and lifecycle management |
| **Spacecraft** | Spacecraft modeling and configuration |
| **Maneuver** | Orbital maneuver planning and execution |
| **Optimization** | Trajectory and mission optimization algorithms |
| **Visualization** | Data visualization and plotting |
| **Reporting** | Report generation and export |
| **ScriptExecution** | Script-based automation and execution |
| **Notification** | Event notifications and alerts |
| **ApiGateway** | Unified API entry point |

## Applications

- **Web Interface** - Angular-based web application for interactive mission analysis

## Shared Libraries

- **Shared** - Common utilities and base classes
- **Shared.Messaging.Abstractions** - Event bus interfaces and contracts
- **Shared.Messaging.UdpMulticast** - UDP multicast messaging implementation

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or VS Code with C# extension

## Getting Started

```bash
# Clone the repository
git clone <repository-url>
cd NGMAT

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Project Structure

```
NGMAT/
├── src/
│   ├── ApiGateway/              # API Gateway service
│   ├── Workspace/               # Web interface
│   │   └── projects/
│   │       └── ui/              # Angular web application
│   ├── Shared/                  # Shared libraries
│   │   ├── Shared.Messaging.Abstractions/
│   │   └── Shared.Messaging.UdpMulticast/
│   └── [ServiceName]/           # Domain services
│       ├── [ServiceName].Core/
│       ├── [ServiceName].Infrastructure/
│       └── [ServiceName].Api/
└── tests/
    └── [ServiceName].Tests/     # Unit and integration tests
```

## License

See [LICENSE](LICENSE) for details.
