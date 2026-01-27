# Driver/Vehicle Assignment Validation Implementation

## Overview
Implemented distributed lock-based validation to prevent double-booking of drivers and vehicles during delivery assignment. The solution uses Redis distributed locks combined with time overlap detection to ensure no concurrent assignments conflict.

## Implementation Summary

### 1. Proto File Updates
**File**: `Eroad.RouteManagement.Contracts/Protos/route_management_query.proto`
- Added `scheduled_start_time` (field 8) and `scheduled_end_time` (field 9) to `RouteEntity` message
- These fields enable time-based validation for assignment conflicts

### 2. Repository Extensions
**Files**:
- `Eroad.DeliveryTracking.Query.Domain/Repositories/IDeliveryRepository.cs`
- `Eroad.DeliveryTracking.Query.Infrastructure/Repositories/DeliveryRepository.cs`

**New Methods**:
```csharp
Task<List<DeliveryEntity>> GetActiveDeliveriesByDriverAsync(Guid driverId);
Task<List<DeliveryEntity>> GetActiveDeliveriesByVehicleAsync(Guid vehicleId);
```

**Active Statuses**: PickedUp, InTransit, OutForDelivery

### 3. Distributed Lock Manager
**Files**:
- `Eroad.BFF.Gateway/Application/Interfaces/IDistributedLockManager.cs`
- `Eroad.BFF.Gateway/Application/Services/RedisLockManager.cs`

**Features**:
- Redis-based distributed locking with automatic expiration
- Owner validation using Lua script to prevent unauthorized releases
- Lock timeout: 10 seconds (configurable)
- Lock keys: `driver-assignment:{driverId}` and `vehicle-assignment:{vehicleId}`

### 4. Assignment Validator
**File**: `Eroad.BFF.Gateway/Application/Validators/DeliveryAssignmentValidator.cs`

**Methods**:
- `ValidateDriverAvailabilityAsync(Guid driverId, DateTime scheduledStart, DateTime scheduledEnd)`
- `ValidateVehicleAvailabilityAsync(Guid vehicleId, DateTime scheduledStart, DateTime scheduledEnd)`

**Validation Logic**:
1. Query active deliveries for driver/vehicle
2. Fetch route scheduled times via gRPC
3. Check time overlap: `!(newEnd <= existingStart || newStart >= existingEnd)`
4. Return validation result with conflict details

### 5. Service Updates
**File**: `Eroad.BFF.Gateway/Application/Services/DeliveryTrackingService.cs`

**Updated Methods**:
- `CreateDeliveryAsync()` - Validates driver/vehicle availability before creation
- `AssignDriverAsync()` - Validates driver availability before assignment
- `AssignVehicleAsync()` - Validates vehicle availability before assignment

**Validation Flow**:
1. Fetch route scheduled times
2. Acquire distributed lock (10s timeout)
3. Validate availability within lock
4. Perform assignment if valid
5. Release lock in finally block

### 6. Controller Updates
**File**: `Eroad.BFF.Gateway/Presentation/Controllers/DeliveryManagementController.cs`

**Error Handling**:
- BadRequest (400): Assignment conflicts (time overlaps)
- NotFound (404): Entity doesn't exist (route/driver/vehicle/delivery)

**Detection**: Checks if error message contains "already assigned" or "during"

### 7. Dependency Injection
**File**: `Eroad.BFF.Gateway/Program.cs`

**New Registrations**:
```csharp
// Redis for distributed locking
builder.Services.AddSingleton<IConnectionMultiplexer>(...)
builder.Services.AddSingleton<IDistributedLockManager, RedisLockManager>()

// Database context for delivery repository
builder.Services.AddDbContext<DatabaseContext>(...)
builder.Services.AddSingleton(new DatabaseContextFactory(...))
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>()

// Assignment validator
builder.Services.AddScoped<DeliveryAssignmentValidator>()
```

### 8. Configuration
**Files**:
- `Eroad.BFF.Gateway/appsettings.json`
- `Eroad.BFF.Gateway/appsettings.Development.json`

**New Connection Strings**:
```json
"ConnectionStrings": {
  "Redis": "localhost:6379",
  "DeliveryTrackingDb": "Server=localhost;Database=DeliveryTrackingDb;Trusted_Connection=true;TrustServerCertificate=True;"
}
```

### 9. Package Dependencies
**File**: `Eroad.BFF.Gateway/Eroad.BFF.Gateway.csproj`

**New Packages**:
- `StackExchange.Redis` (v2.7.10)
- `Microsoft.EntityFrameworkCore` (v8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (v8.0.0)

**New Project References**:
- `Eroad.DeliveryTracking.Query.Domain`
- `Eroad.DeliveryTracking.Query.Infrastructure`

## Architecture Pattern: Distributed Lock + Optimistic Locking Hybrid

### Why This Approach?
1. **Distributed Lock**: Prevents race conditions during validation (~100-500ms hold time)
2. **Optimistic Locking**: Event store provides fallback concurrency control
3. **Short Lock Duration**: Minimal performance impact, reduces contention
4. **Cross-Service Coordination**: Prevents double-booking across microservices

### Lock Lifecycle
```
1. Acquire lock → 2. Validate → 3. Assign → 4. Release lock (finally)
                     ↓ Invalid
                  Throw error (lock released in finally)
```

## Error Messages
- **Driver Conflict**: "Driver {driverId} already assigned to delivery {id} during {start} - {end}"
- **Vehicle Conflict**: "Vehicle {vehicleId} already assigned to delivery {id} during {start} - {end}"
- **Lock Failure**: "Unable to validate driver/vehicle {id} assignment. Please try again."

## Testing Considerations
1. **Redis Required**: Ensure Redis is running on `localhost:6379`
2. **Database Access**: BFF Gateway needs read access to DeliveryTrackingDb
3. **Proto Regeneration**: Run `dotnet build` on RouteManagement.Contracts to regenerate gRPC clients
4. **Concurrent Testing**: Use parallel requests to test lock effectiveness

## Performance Characteristics
- **Lock Hold Time**: 100-500ms (validation only)
- **Lock Timeout**: 10 seconds (prevents deadlocks)
- **Retry Strategy**: Client should retry on lock acquisition failure
- **Network Calls**: 1-3 gRPC calls per validation (depending on active deliveries)

## Next Steps
1. Restore NuGet packages: `dotnet restore`
2. Build solution: `dotnet build`
3. Start Redis: `docker run -p 6379:6379 redis`
4. Run BFF Gateway: `dotnet run --project Eroad.BFF.Gateway`
5. Test assignment conflicts using .http files
