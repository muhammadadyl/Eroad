# Eroad BFF Gateway - Clean Architecture

This Backend-For-Frontend (BFF) Gateway follows Clean Architecture principles to ensure maintainability, testability, and separation of concerns. It provides a unified API for frontend applications by aggregating data from multiple microservices.

## Architecture Overview

```
Eroad.BFF.Gateway/
├── Application/                    # Application Layer
│   ├── DTOs/                      # Data Transfer Objects
│   │   ├── DeliveryEventLogDto.cs
│   │   ├── EventManagementView.cs
│   │   ├── FleetManagementView.cs
│   │   ├── LiveTrackingView.cs
│   │   ├── RouteManagementView.cs
│   │   └── SharedModels.cs        # Shared DTOs (DriverInfo, VehicleInfo, etc.)
│   ├── Interfaces/                # Service interfaces
│   │   ├── IDeliveryTrackingService.cs
│   │   ├── IFleetManagementService.cs
│   │   └── IRouteManagementService.cs
│   ├── Services/                  # Application Services
│   │   ├── DeliveryTrackingService.cs
│   │   ├── FleetManagementService.cs
│   │   └── RouteManagementService.cs
│   └── Validators/                # Business validation
│       └── DeliveryAssignmentValidator.cs
│
├── Presentation/                   # Presentation Layer
│   ├── Controllers/               # REST API Controllers
│   │   ├── DeliveryManagementController.cs
│   │   ├── FleetManagementController.cs
│   │   └── RouteManagementController.cs
│   └── Middleware/
│       └── ExceptionHandlingMiddleware.cs
│
├── Program.cs                     # Application entry point
└── appsettings.json              # Configuration
```

## Layer Responsibilities

### Application Layer (`Application/`)
- **DTOs**: Data Transfer Objects for request/response models
- **Interfaces**: Service contracts following Dependency Inversion Principle
- **Services**: Business logic orchestration, data aggregation from multiple microservices
- **Validators**: Business rule validation (e.g., delivery assignment validation with distributed locking)

### Presentation Layer (`Presentation/`)
- **Controllers**: HTTP request handling, routing, validation
- **Middleware**: Cross-cutting concerns (exception handling, logging, etc.)

## Resilience & Performance Features

### 🔄 Circuit Breaker (Polly v8)
- **Failure Ratio**: 50% failures trigger circuit open
- **Break Duration**: 15 seconds
- **Sampling Window**: 30 seconds
- **Minimum Throughput**: 10 requests before evaluation
- **States**: Closed → Open → Half-Open → Closed
- Prevents cascading failures to downstream services

### 🔁 Retry Policy (Polly v8)
- **Max Retry Attempts**: 3
- **Backoff Strategy**: Exponential with jitter
- **Initial Delay**: 1 second
- **Retries on**: `Unavailable`, `DeadlineExceeded`, `Internal` gRPC status codes
- Automatic retry with exponential backoff for transient failures

### ⏱️ Timeout Policy
- **Timeout**: 30 seconds per gRPC request
- Prevents hanging requests and resource exhaustion

### 🚦 Rate Limiting
- **Global Limiter**: 100 requests per 10 seconds per IP
- **Algorithm**: Sliding window with 5 segments
- **Queue**: 10 requests can queue when limit reached
- **Response**: 429 Too Many Requests with `retryAfter` metadata
- Protects against DoS and excessive usage

### 🔐 Distributed Locking (Redis)
- **Purpose**: Prevent concurrent assignment conflicts
- **Use Case**: Driver/vehicle assignment validation
- **Implementation**: `RedisLockManager` with atomic operations
- Ensures data consistency across multiple BFF instances

### 🌐 CORS (Cross-Origin Resource Sharing)
- **Development**: Allows localhost origins (3000, 4200, 5173) with all methods
- **Production**: Configurable whitelist with restricted methods (GET, POST, PATCH)
- **Credentials**: Enabled for authenticated requests
- **Custom Headers**: Exposes `X-Correlation-Id` and `X-Request-Id` for tracing

### 💓 Health Checks
- **Endpoint**: `/health`
- **Status**: Returns 200 OK if service is running
- **Purpose**: Kubernetes liveness/readiness probes, monitoring

## Key Principles

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Rule**: Dependencies point inward (Presentation → Application → Infrastructure)
3. **Independence**: Business logic is independent of frameworks and UI
4. **Testability**: Layers can be tested independently

## Services (Aggregators)

Services orchestrate data from multiple microservices:

- **IDeliveryTrackingService** → `DeliveryTrackingService`: Manages delivery lifecycle and tracking data
- **IRouteManagementService** → `RouteManagementService`: Provides route overview and details
- **IFleetManagementService** → `FleetManagementService`: Aggregates fleet and driver information

All controllers depend on interfaces, following the **Dependency Inversion Principle** for better testability and loose coupling.

## API Endpoints

### Delivery Management
- `GET /api/deliveries/live-tracking` - Get active deliveries
- `GET /api/deliveries/{id}/completed-summary` - Get delivery timeline
- `POST /api/deliveries` - Create delivery
- `PATCH /api/deliveries/{id}/status` - Update delivery status
- `PATCH /api/deliveries/{id}/checkpoint` - Update checkpoint
- `POST /api/deliveries/{id}/incidents` - Report incident
- `PATCH /api/deliveries/{id}/incidents/{incidentId}/resolve` - Resolve incident
- `POST /api/deliveries/{id}/proof-of-delivery` - Capture proof
- `PATCH /api/deliveries/{id}/assign-driver` - Assign driver
- `PATCH /api/deliveries/{id}/assign-vehicle` - Assign vehicle
- `GET /api/deliveries/{id}/event-logs` - Get event logs

### Route Management
- `GET /api/routes/overview` - Get all routes
- `GET /api/routes/{id}` - Get route details
- `POST /api/routes` - Create route
- `POST /api/routes/{id}/checkpoints` - Add checkpoint
- `PATCH /api/routes/{id}/checkpoints/{sequence}` - Update checkpoint

### Fleet Management
- `GET /api/fleet/overview` - Get fleet overview
- `GET /api/fleet/drivers` - Get all drivers
- `GET /api/fleet/vehicles` - Get all vehicles
- `POST /api/fleet/drivers` - Create driver
- `POST /api/fleet/vehicles` - Create vehicle

## Configuration

### Service Endpoints (`appsettings.json`)

```json
{
  "ServiceEndpoints": {
    "DeliveryTrackingBaseUrl": "http://localhost:5001",
    "DeliveryTrackingCommandBaseUrl": "http://localhost:5002",
    "FleetManagementBaseUrl": "http://localhost:5003",
    "FleetManagementCommandBaseUrl": "http://localhost:5004",
    "RouteManagementBaseUrl": "http://localhost:5005",
    "RouteManagementCommandBaseUrl": "http://localhost:5006"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "CorsOrigins": [
    "http://localhost:3000",
    "http://localhost:4200",
    "http://localhost:5173"
  ]
}
```

## Middleware

- **ExceptionHandlingMiddleware**: Global exception handling with consistent error responses

## Future Enhancements

1. ✅ ~~Move gRPC client registration to Infrastructure layer~~
2. ✅ ~~Add service interfaces to Application.Interfaces~~
3. ✅ ~~Implement rate limiting~~
4. ✅ ~~Add circuit breaker pattern for resilience~~
5. ✅ ~~Add health checks~~
6. ✅ ~~Implement CORS~~
7. ✅ ~~Add distributed locking (Redis)~~
8. Add distributed caching (Redis) for frequently accessed data
9. Implement request/response logging with correlation IDs
10. Add API versioning (v1, v2)
11. Add authentication and authorization
12. Implement event-driven notifications (SignalR/WebSockets)
