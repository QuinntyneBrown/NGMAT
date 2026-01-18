# API Gateway

The API Gateway serves as the single entry point for all external client requests to the NGMAT microservices architecture. It handles routing, authentication, rate limiting, caching, and other cross-cutting concerns.

## Features

### ✅ Implemented

- **Request Routing** - Routes requests to 15 backend microservices based on URL path patterns
- **JWT Authentication** - Validates JWT tokens and enforces authentication for protected endpoints
- **Rate Limiting** - Implements per-IP rate limiting to prevent abuse (100 req/min, 1000 req/hour)
- **Response Caching** - In-memory response caching for improved performance
- **Response Compression** - Automatic gzip/brotli compression for responses
- **CORS** - Configurable Cross-Origin Resource Sharing policies
- **Health Checks** - `/health` endpoint for service health monitoring
- **Swagger Documentation** - Interactive API documentation at `/swagger`

### 🚧 Planned

- Redis distributed caching
- OpenTelemetry tracing
- Advanced metrics collection
- Request/response transformation
- Circuit breaker patterns

## Routes

The gateway routes requests to the following services:

| Route Pattern | Target Service | Port |
|--------------|----------------|------|
| `/v1/identity/**` | Identity Service | 5001 |
| `/v1/missions/**` | Mission Management Service | 5002 |
| `/v1/spacecraft/**` | Spacecraft Service | 5003 |
| `/v1/propagation/**` | Propagation Service | 5004 |
| `/v1/forcemodel/**` | Force Model Service | 5005 |
| `/v1/maneuvers/**` | Maneuver Service | 5006 |
| `/v1/coordinates/**` | Coordinate System Service | 5007 |
| `/v1/ephemeris/**` | Ephemeris Service | 5008 |
| `/v1/optimization/**` | Optimization Service | 5009 |
| `/v1/calc/**` | Calculation Engine Service | 5010 |
| `/v1/visualization/**` | Visualization Service | 5011 |
| `/v1/reports/**` | Reporting Service | 5012 |
| `/v1/scripts/**` | Script Execution Service | 5013 |
| `/v1/events/**` | Event Store Service | 5014 |
| `/v1/notifications/**` | Notification Service | 5015 |

## Configuration

### JWT Authentication

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-min-32-characters",
    "Issuer": "ngmat-api-gateway",
    "Audience": "ngmat-clients"
  }
}
```

For production, use environment variables or Azure Key Vault to store the secret key.

### Rate Limiting

Default rate limits are configured in `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ]
  }
}
```

### Backend Service URLs

Update the cluster destinations in `appsettings.json` to point to your backend services:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5001"
          }
        }
      }
    }
  }
}
```

## Running the Gateway

### Development

```bash
cd src/ApiGateway
dotnet run
```

The gateway will start on `https://localhost:5000` by default.

### Production

```bash
dotnet publish -c Release
cd bin/Release/net8.0/publish
dotnet ApiGateway.dll
```

## Testing

Run the unit tests:

```bash
cd tests/ApiGateway.Tests
dotnet test
```

## API Documentation

Once the gateway is running, access the Swagger UI at:

```
https://localhost:5000/swagger
```

## Health Checks

Check the gateway health status:

```bash
curl https://localhost:5000/health
```

## Authentication

To authenticate requests, include a JWT token in the Authorization header:

```bash
curl -H "Authorization: Bearer <your-jwt-token>" \
     https://localhost:5000/v1/missions/123
```

## Rate Limiting

When rate limits are exceeded, the gateway returns a `429 Too Many Requests` response with a `Retry-After` header indicating when to retry.

## Technology Stack

- **YARP** (Yet Another Reverse Proxy) - Microsoft's reverse proxy library
- **ASP.NET Core 8.0** - Web framework
- **JWT Bearer Authentication** - Token-based authentication
- **AspNetCoreRateLimit** - Rate limiting middleware
- **Swagger/OpenAPI** - API documentation

## See Also

- [REQUIREMENTS.md](REQUIREMENTS.md) - Detailed requirements
- [ROADMAP.md](ROADMAP.md) - Implementation roadmap
- [Architecture Overview](../../README.md) - Overall system architecture
