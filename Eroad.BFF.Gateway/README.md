# Eroad BFF Gateway - Clean Architecture

This Backend-For-Frontend (BFF) Gateway follows Clean Architecture principles to ensure maintainability, testability, and separation of concerns.

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
│   │   ├── IEventManagementService.cs
│   │   ├── IFleetManagementService.cs
│   │   ├── ILiveTrackingService.cs
│   │   └── IRouteManagementService.cs
│   └── Services/                  # Application Services (Aggregators)
│       ├── EventManagementAggregator.cs
│       ├── FleetManagementAggregator.cs
│       ├── LiveTrackingAggregator.cs
│       └── RouteManagementAggregator.cs
│
├── Presentation/                   # Presentation Layer
│   ├── Controllers/               # REST API Controllers
│   │   ├── DeliveryManagementController.cs
│   │   ├── EventManagementController.cs
│   │   ├── FleetManagementController.cs
│   │   └── RouteManagementController.cs
│   └── Middleware/
│       └── ExceptionHandlingMiddleware.cs
│
├── Infrastructure/                 # Infrastructure Layer (for future use)
│   └── Services/                  # External service implementations
│
├── Program.cs                     # Application entry point
└── appsettings.json              # Configuration
```

## Layer Responsibilities

### Application Layer (`Application/`)
- **DTOs**: Data Transfer Objects for request/response models
- **Interfaces**: Service contracts following Dependency Inversion Principle
- **Services**: Business logic orchestration, data aggregation from multiple microservices

### Presentation Layer (`Presentation/`)
- **Controllers**: HTTP request handling, routing, validation (depend on interfaces, not concrete implementations)
- **Middleware**: Cross-cutting concerns (exception handling, logging, etc.)

### Infrastructure Layer (`Infrastructure/`)
- **Services**: External service implementations (gRPC clients, caching, etc.)
- Currently gRPC clients are registered in Program.cs

## Key Principles

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Rule**: Dependencies point inward (Presentation → Application → Infrastructure)
3. **Independence**: Business logic is independent of frameworks and UI
4. **Testability**: Layers can be tested independently

## Services (Aggregators)

Aggregators combine data from multiple microservices:

- **ILiveTrackingService** (implemented by LiveTrackingAggregator): Aggregates active delivery tracking data
- **IRouteManagementService** (implemented by RouteManagementAggregator): Provides route overview and details
- **IFleetManagementService** (implemented by FleetManagementAggregator): Aggregates fleet and driver information
- **IEventManagementService** (implemented by EventManagementAggregator): Handles delivery event timeline

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

### Event Management
- `GET /api/events/active-incidents` - Get active incidents
- `GET /api/events/delivery/{id}/timeline` - Get delivery timeline

## Configuration

Service endpoints are configured in `appsettings.json`:
- DeliveryTrackingBaseUrl / DeliveryTrackingCommandBaseUrl
- FleetManagementBaseUrl / FleetManagementCommandBaseUrl
- RouteManagementBaseUrl / RouteManagementCommandBaseUrl

## Middleware

- **ExceptionHandlingMiddleware**: Global exception handling with consistent error responses

## Future Enhancements

1. Move gRPC client registration to Infrastructure layer
2. Add service interfaces to Application.Interfaces
3. Implement caching strategies
4. Add request/response logging
5. Implement API versioning
6. Add health checks
