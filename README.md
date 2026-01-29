# Eroad Delivery Management System

A microservices-based delivery management platform built with .NET 8, implementing Event Sourcing, CQRS, and Domain-Driven Design patterns.

##  Architecture Overview

### Technology Stack

#### Backend Services
- **.NET 8** - Application framework
- **gRPC** - Inter-service communication
- **MediatR** - CQRS command/query handling
- **Entity Framework Core** - ORM for query databases
- **Polly** - Service resilience and rate limiting

#### Data Storage
- **MongoDB** - Event store for Command side (Event Sourcing)
- **SQL Server** - Read models for Query side (CQRS)
- **Redis** - Distributed locking and caching

#### Messaging & Events
- **Apache Kafka** - Event streaming and pub/sub messaging
- **Event Sourcing** - Complete audit trail of all state changes

#### Containerization
- **Docker** - Container runtime
- **Docker Compose** - Multi-container orchestration

### Microservices

#### 1. Delivery Tracking Service
- **Command API** (Port 5001) - Manages delivery lifecycle
- **Query API** (Port 5002) - Delivery data queries
- **Events**: delivery-tracking-events

#### 2. Fleet Management Service  
- **Command API** (Port 5003) - Vehicle and driver management
- **Query API** (Port 5004) - Fleet data queries
- **Events**: fleet-management-events

#### 3. Route Management Service
- **Command API** (Port 5005) - Route planning and optimization
- **Query API** (Port 5006) - Route data queries  
- **Events**: route-management-events

#### 4. BFF Gateway (Port 5000)
- Backend for Frontend
- Aggregates data from multiple services
- Implements business logic orchestration
- Distributed locking for concurrent operations

##  Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/)

### Running with Docker Compose

#### 1. Start All Services (Infrastructure + APIs) __(Recommended)__
```powershell
docker-compose --profile init-services up -d
```

#### 2. View Logs
```powershell
docker-compose logs -f
```

#### 3. Stop Services
```powershell
docker-compose down
```

### Running Services Locally (Development)

#### 1. Start Infrastructure
```powershell
docker-compose --profile dev-services up -d
```

Note: Add kafka-kraft in your windows host file

#### 2. Run Individual Services

**Delivery Tracking Command API:**
```powershell
cd Eroad.DeliveryTracking.Command.API
dotnet restore
dotnet run
```

**Delivery Tracking Query API:**
```powershell
cd Eroad.DeliveryTracking.Query.API
dotnet restore
dotnet run
```

**BFF Gateway:**
```powershell
cd Eroad.BFF.Gateway
dotnet restore
dotnet run
```

Repeat for other services as needed.

##  Architecture Patterns

### CQRS (Command Query Responsibility Segregation)

**Command Side:**
- Handles write operations
- Uses Event Sourcing
- Stores events in MongoDB
- Publishes events to Kafka

**Query Side:**
- Handles read operations
- Consumes events from Kafka
- Updates SQL Server read models
- Optimized for queries

### Event Sourcing

All state changes are captured as immutable events:

```csharp
// Example: Delivery Status Change
public class DeliveryStatusChangedEvent : DomainEvent
{
    public Guid Id { get; set; }
    public DeliveryStatus OldStatus { get; set; }
    public DeliveryStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

**Benefits:**
- Complete audit trail
- Event replay capability
- Temporal queries
- Easy debugging

### Domain-Driven Design (DDD)

**Aggregates:**
- `DeliveryAggregate` - Delivery lifecycle management
- `RouteAggregate` - Route planning
- `VehicleAggregate` - Vehicle management
- `DriverAggregate` - Driver management

**Domain Events:**
- `DeliveryCreatedEvent`
- `DeliveryStatusChangedEvent`
- `DriverAssignedEvent`
- `VehicleAssignedEvent`
- `ProofOfDeliveryCapturedEvent`

### State Machine Pattern

Delivery status transitions are controlled by a state machine:

**Valid Transitions:**
- PickedUp -> InTransit, Failed
- InTransit -> OutForDelivery, Failed
- OutForDelivery -> Delivered, Failed
- Delivered  (Terminal state)
- Failed -> PickedUp (retry)

##  Inter-Service Communication

### gRPC Protocol Buffers

Services communicate using gRPC for high-performance, type-safe calls:

```protobuf
service DeliveryLookup {
  rpc GetAllDeliveries(GetAllDeliveriesRequest) returns (DeliveryLookupResponse);
  rpc GetActiveDeliveriesByDriver(GetActiveDeliveriesByDriverRequest) returns (DeliveryLookupResponse);
  rpc GetActiveDeliveriesByVehicle(GetActiveDeliveriesByVehicleRequest) returns (DeliveryLookupResponse);
}
```

### Kafka Event Topics

- `delivery-tracking-events` - Delivery lifecycle events
- `fleet-management-events` - Vehicle and driver events
- `route-management-events` - Route planning events

##  Key Features

### 1. Delivery Assignment Validation

Prevents double-booking of drivers and vehicles:

```csharp
var (isValid, error, conflictId, start, end) = 
    await validator.ValidateDriverAvailabilityAsync(
        driverId, scheduledStart, scheduledEnd);
```

**Validation Rules:**
- Checks active deliveries (PickedUp, InTransit, OutForDelivery)
- Detects time overlaps using interval logic
- Returns conflict details for resolution

### 2. Status Transition Validation

State machine enforces valid delivery workflow:

```csharp
//  Valid: PickedUp  InTransit
aggregate.UpdateDeliveryStatus(DeliveryStatus.PickedUp, DeliveryStatus.InTransit);

//  Invalid: PickedUp  Delivered (skips intermediate states)
aggregate.UpdateDeliveryStatus(DeliveryStatus.PickedUp, DeliveryStatus.Delivered);
// Throws: InvalidOperationException with valid transitions message
```

### 3. Distributed Locking

Redis-based locks prevent concurrent modifications:

```csharp
await using var lockHandle = await lockManager.AcquireLockAsync(
    deliveryId, TimeSpan.FromSeconds(30));
```

### 4. Event-Driven Integration

Services react to domain events asynchronously:

```csharp
// Command side publishes event
RaiseEvent(new DeliveryStatusChangedEvent(...));

// Query side consumes event
public Task Handle(DeliveryStatusChangedEvent @event)
{
    // Update read model
}
```

##  Project Structure

```
Eroad/
 Eroad.BFF.Gateway/                  # Backend for Frontend
    Application/
       Services/                   # Business logic orchestration
       Validators/                 # Assignment validation
    Presentation/
        Controllers/                # REST API endpoints

 Eroad.DeliveryTracking.Command.API/ # Delivery write operations
    Commands/                       # CQRS commands
    Services/Grpc/                  # gRPC service implementations

 Eroad.DeliveryTracking.Command.Domain/ # Delivery business logic
    Aggregates/                     # Domain aggregates
        DeliveryAggregate.cs        # State machine implementation

 Eroad.DeliveryTracking.Command.Infrastructure/ # Event sourcing infrastructure
    Handlers/                       # Event sourcing handlers
    Repositories/                   # Event store repository
    Stores/                         # MongoDB event store

 Eroad.DeliveryTracking.Query.API/   # Delivery read operations
    Queries/                        # CQRS queries
    Services/Grpc/                  # gRPC query services

 Eroad.DeliveryTracking.Query.Infrastructure/ # Query database access
    DataAccess/                     # EF Core DbContext
    Repositories/                   # Query repositories

 Eroad.DeliveryTracking.Contracts/   # gRPC proto definitions
    Protos/
        delivery_tracking_command.proto
        delivery_tracking_query.proto

 Eroad.DeliveryTracking.Common/      # Shared domain types
    DeliveryStatus.cs               # Status enumeration
    DomainEvent.cs                  # Base event types
    Incident.cs                     # Value objects

 Eroad.CQRS.Core/                    # Shared CQRS infrastructure
    Commands/                       # Base command interfaces
    Domain/                         # Aggregate base classes
    Events/                         # Event store models
    Handlers/                       # Event sourcing interfaces
    Infrastructure/                 # Event store implementation
    Producers/                      # Kafka producers
    Queries/                        # Base query interfaces

 docker-compose.yaml                 # Container orchestration
```

##  Configuration

### Connection Strings

**SQL Server (Query Databases):**
```json
{
  \"ConnectionStrings\": {
    \"SqlServer\": \"Server=localhost;Database=DeliveryTrackingQuery;User Id=sa;Password=\$\@P@ssw0rd02;TrustServerCertificate=true;\"
  }
}
```

**MongoDB (Event Store):**
```json
{
  \"MongoDbConfig\": {
    \"ConnectionString\": \"mongodb://localhost:27017\",
    \"Database\": \"deliverytracking\",
    \"Collection\": \"eventStore\"
  }
}
```

**Kafka (Event Streaming):**
```json
{
  \"ProducerConfig\": {
    \"BootstrapServers\": \"localhost:9092\"
  },
  \"ConsumerConfig\": {
    \"BootstrapServers\": \"localhost:9092\",
    \"GroupId\": \"delivery-query-consumer\",
    \"AutoOffsetReset\": \"Earliest\"
  }
}
```

**Redis (Distributed Locking):**
```json
{
  \"ConnectionStrings\": {
    \"Redis\": \"localhost:6379\"
  }
}
```

### Service Endpoints

```json
{
  \"ServiceEndpoints\": {
    \"DeliveryTrackingCommandBaseUrl\": \"http://localhost:5001\",
    \"DeliveryTrackingBaseUrl\": \"http://localhost:5002\",
    \"FleetManagementCommandBaseUrl\": \"http://localhost:5003\",
    \"FleetManagementBaseUrl\": \"http://localhost:5004\",
    \"RouteManagementCommandBaseUrl\": \"http://localhost:5005\",
    \"RouteManagementBaseUrl\": \"http://localhost:5006\"
  }
}
```

##  Testing

### Integration Tests

- Ensure infrastructure (Kafka, MongoDB, SQL Server, Redis) is running before executing integration tests. Start the basic infra with:

```powershell
docker-compose --profile init-services up -d
```

- Run the BFF integration test project:

```powershell
dotnet test Eroad.BFF.IntegrationTest
```

- Notes on eventual consistency: query/read models are updated asynchronously (via Kafka consumers). Integration tests that assert on read/query endpoints may need a retry/polling strategy (for example: poll the GET endpoint until a condition is met or a timeout elapses) to avoid flaky failures.

   Recommended pattern: poll the endpoint with a short retry interval (200-1000ms) and a reasonable timeout (10-30s) when asserting results from query APIs.


### Manual Testing with Kafka UI

Access Kafka UI at http://localhost:8080 to:
- View topics and messages
- Monitor consumer groups
- Inspect event payloads

### gRPC Testing

Use tools like:
- [grpcurl](https://github.com/fullstorydev/grpcurl)
- [BloomRPC](https://github.com/bloomrpc/bloomrpc)
- Postman (with gRPC support)

##  Monitoring & Debugging

### View Event Store (MongoDB)
```bash
docker exec -it mongo-container mongosh
use deliverytracking
db.eventStore.find().pretty()
```

### View Read Models (SQL Server)
```bash
docker exec -it sql-container /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '\$\@P@ssw0rd02'
SELECT * FROM Deliveries
GO
```

### View Kafka Messages
- Access Kafka UI: http://localhost:8080
- Browse topics
- View message payloads and headers

### Redis Commands
```bash
docker exec -it redis-container redis-cli
KEYS *
GET <key>
```

##  Security Considerations

- **gRPC Security**: Currently configured for development (HTTP/2 unencrypted)
- **Database Credentials**: Stored in appsettings.Development.json (excluded from source control)
- **Production**: Use environment variables and secrets management (Azure Key Vault, HashiCorp Vault)

##  Known Limitations & Future Enhancements

### Current Limitations
- No authentication/authorization implemented
- Single-instance deployment (no high availability)
- Basic error handling without retry policies
- Limited observability (no distributed tracing)

### Planned Enhancements
1. **Authentication & Authorization**
   - JWT-based authentication
   - Role-based access control (RBAC)
   - API key management

2. **Resilience**
   - Circuit breaker pattern (Polly)
   - Retry with exponential backoff
   - Bulkhead isolation

3. **Observability**
   - Distributed tracing (OpenTelemetry)
   - Centralized logging (ELK Stack)
   - Metrics & dashboards (Prometheus + Grafana)

4. **Performance**
   - Response caching
   - Database query optimization
   - Connection pooling

5. **Deployment**
   - Kubernetes manifests
   - Helm charts
   - CI/CD pipelines (GitHub Actions)

##  Team

**Development Team**: Eroad Engineering

##  Support

For issues and questions:
- Create an issue in the repository
- Contact the development team

---

**Built with  using .NET 8, Event Sourcing, CQRS, and Microservices Architecture**
