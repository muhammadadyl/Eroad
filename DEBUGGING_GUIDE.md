# Event Sourcing Pipeline Debugging Guide

## Problem Summary
When creating a delivery, the system throws: `gRPC RpcException: Status(StatusCode="Internal", Detail="An error occurred while retrieving the route")`

This indicates that **RouteManagement.Query.API cannot find the route** that was just created.

## Root Cause Analysis
The error likely originates from one of these points:

1. **Route is not being persisted to SQL Server** → Kafka consumer is not processing RouteCreatedEvent
2. **Route is in event store but not in query database** → EventConsumer is not mapping events to the read model
3. **Query API query is failing** → The RouteLookupGrpcService.GetRouteByIdAsync() cannot find the route

## Event Flow Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ 1. BFF Gateway HTTP Request (Create Route)                  │
│    POST /api/routes                                         │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. RouteManagement.Command.API                              │
│    CreateRouteCommand → CreateRouteCommandHandler           │
│    Creates domain aggregate & publishes RouteCreatedEvent   │
│    Stores in MongoDB Event Store                            │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Kafka Topic: route-management-events                     │
│    Publishes RouteCreatedEvent as JSON message              │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. RouteManagement.Query.API - EventConsumer                │
│    Subscribes to route-management-events topic              │
│    Deserializes JSON event message                          │
│    Invokes RouteCreatedEventHandler.HandleAsync()           │
│    Persists to SQL Server (dbo.Routes table)                │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. RouteManagement.Query.API - RouteLookupGrpcService       │
│    GetRouteByIdAsync(routeId)                               │
│    Queries SQL Server for route                             │
│    Returns route data or null if not found                  │
└─────────────────────────────────────────────────────────────┘
```

## Debugging Steps

### Step 1: Verify Command API Created the Route Event
**File:** `Eroad.RouteManagement.Command.API/Program.cs`

Check that:
- [ ] Event store is properly initialized (MongoDB connection)
- [ ] CreateRouteCommandHandler stores event correctly
- [ ] Kafka producer is configured

**To test:**
```
1. Create a route via HTTP API
2. Check MongoDB (eroad_event_store collection)
3. Look for RouteCreatedEvent with matching routeId
```

### Step 2: Verify Kafka Topic Exists and Event is Published
**Kafka Commands:**

```bash
# List topics
kafka-topics.sh --bootstrap-server localhost:9092 --list

# Check if route-management-events topic exists
# Should see: route-management-events

# Describe topic
kafka-topics.sh --bootstrap-server localhost:9092 --describe --topic route-management-events

# View messages in topic (last 10)
kafka-console-consumer.sh --bootstrap-server localhost:9092 \
  --topic route-management-events \
  --from-beginning \
  --max-messages 10
```

### Step 3: Verify EventConsumer is Running and Processing Events
**Log Output to Check:**

Look for these log messages in RouteManagement.Query.API:
- ✅ "EventConsumer started, subscribing to topic"
- ✅ "Received message on topic [route-management-events]"
- ✅ "Processing event: RouteCreatedEvent"
- ✅ "Successfully processed RouteCreatedEvent"

**If you see errors like:**
- ❌ "UnknownTopicOrPart" → Topic doesn't exist yet (should retry)
- ❌ "No handler found for event type" → EventHandler not registered
- ❌ "Failed to deserialize message" → EventJsonConverter issue
- ❌ "Database error while persisting" → SQL Server query issue

### Step 4: Verify Route Data in SQL Server
**Query to check if route exists:**

```sql
-- RouteManagement.Query Database
SELECT * FROM [RouteManagement].[dbo].[Routes]
WHERE [Id] = 'YOUR_ROUTE_ID'
```

Expected columns:
- Id (GUID)
- Name (nvarchar)
- Status (nvarchar)
- ScheduledStartTime (datetime2)
- ScheduledEndTime (datetime2)
- CreatedAt (datetime2)

If query returns 0 rows:
- Route was not persisted by EventConsumer
- Check EventConsumer logs for errors

### Step 5: Check if RouteLookupGrpcService is Querying Correctly
**File:** `Eroad.RouteManagement.Query.API/Services/RouteLookupGrpcService.cs`

The service should:
1. Query SQL Server for routes
2. Map to protobuf GetRouteByIdResponse
3. Return routes or empty collection

### Step 6: Test End-to-End in VS Code

**Terminal 1: Start all services**
```bash
# From workspace root
docker-compose up -d
```

**Terminal 2: Create a route**
```bash
# Call BFF Gateway to create route
curl -X POST http://localhost:5000/api/routes \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Route",
    "scheduledStartTime": "2024-01-15T10:00:00",
    "scheduledEndTime": "2024-01-15T18:00:00",
    "checkpoints": [
      {"sequence": 1, "location": "A"},
      {"sequence": 2, "location": "B"}
    ]
  }'
```

**Terminal 3: Monitor logs**
```bash
# Check Command API
docker logs -f eroad-routemanagement-command-api

# Check Query API
docker logs -f eroad-routemanagement-query-api

# Check Kafka for events
kafka-console-consumer.sh --bootstrap-server localhost:9092 \
  --topic route-management-events \
  --from-beginning
```

**Terminal 4: Query the route database**
```bash
# Use SQL Server client
sqlcmd -S localhost -U sa -P YourPassword -d RouteManagement \
  -Q "SELECT * FROM dbo.Routes WHERE Id = 'YOUR_ROUTE_ID'"
```

**Terminal 5: Create delivery**
```bash
curl -X POST http://localhost:5000/api/deliveries \
  -H "Content-Type: application/json" \
  -d '{
    "routeId": "YOUR_ROUTE_ID",
    "recipientName": "John Doe",
    "recipientAddress": "123 Main St"
  }'
```

## Common Issues and Fixes

### Issue: Kafka Topic Does Not Exist
**Symptom:** `UnknownTopicOrPart` error in logs

**Fix Applied:** EventConsumer now has retry logic (5 retries × 2 seconds exponential backoff)

**Files:** All EventConsumer.cs files

### Issue: EventHandler Not Awaited
**Symptom:** Events processed but data not saved to database

**Fix Applied:** Changed async Task handlers to be properly awaited:
```csharp
// Before: var task = (Task)handlerMethod.Invoke(...); // ❌ Not awaited
// After: var task = (Task)handlerMethod.Invoke(...); await task; // ✅ Properly awaited
```

**Files:** 
- `Eroad.RouteManagement.Query.Infrastructure/Consumers/EventConsumer.cs`
- `Eroad.FleetManagement.Query.Infrastructure/Consumers/EventConsumer.cs`
- `Eroad.DeliveryTracking.Query.Infrastructure/Consumers/EventConsumer.cs`

### Issue: Service Scope Disposed Before Consumer Runs
**Symptom:** Database access errors in consumer

**Fix Applied:** Changed ConsumerHostedService to maintain persistent task and scope

**Files:**
- `Eroad.RouteManagement.Query.Infrastructure/Consumers/ConsumerHostedService.cs`
- `Eroad.FleetManagement.Query.Infrastructure/Consumers/ConsumerHostedService.cs`
- `Eroad.DeliveryTracking.Query.Infrastructure/Consumers/ConsumerHostedService.cs`

### Issue: Route Not Found in Query API
**Symptom:** BFF Gateway gets `null` when querying for route

**Root Causes:**
1. **Consumer never ran** → Check if ConsumerHostedService started
2. **Event never reached Kafka** → Check if Kafka producer in Command API is configured
3. **Event deserialization failed** → Check EventJsonConverter
4. **Handler exception** → Look for errors in Query API logs
5. **Database query failed** → Check SQL Server is accessible

**Debug Steps:**
```
1. Create route (note the route ID)
2. Check MongoDB: route event exists? ✓ or ✗
3. Check Kafka: event in topic? ✓ or ✗
4. Check SQL Server: route in table? ✓ or ✗
5. Check Query API logs: any errors? ✓ or ✗
6. Call GetRouteByIdAsync(routeId): returns data? ✓ or ✗
```

## Key Files and Their Responsibilities

| File | Responsibility | Status |
|------|---|---|
| `RouteManagement.Command.Api/Program.cs` | Register command handler, Kafka producer | ✅ |
| `RouteManagement.Command.Api/Handlers/CreateRouteCommandHandler.cs` | Handle command, create aggregate, publish event | ✅ |
| `RouteManagement.Query.Api/Program.cs` | Register consumer, logging, SQL Server | ✅ |
| `RouteManagement.Query.Infrastructure/Consumers/EventConsumer.cs` | Kafka consumption, event deserialization, handler invocation | ✅ Fixed |
| `RouteManagement.Query.Infrastructure/Consumers/ConsumerHostedService.cs` | Start/stop consumer, manage service scope | ✅ Fixed |
| `RouteManagement.Query.Infrastructure/Handlers/RouteCreatedEventHandler.cs` | Save route to SQL Server | ✅ |
| `RouteManagement.Query.Api/Services/RouteLookupGrpcService.cs` | Query SQL Server, return route data | ✅ |
| `BFF.Gateway/Application/Services/DeliveryTrackingService.cs` | Call Query APIs via gRPC | ✅ Enhanced with error handling |

## Next Actions

If the route is still not found:

1. **Add detailed logging** to EventConsumer to track each step:
   ```csharp
   _logger.LogInformation("About to invoke handler for event type {EventType}", eventType);
   ```

2. **Verify EventJsonConverter** is properly deserializing RouteCreatedEvent

3. **Check if RouteCreatedEventHandler is throwing exceptions** (check logs)

4. **Manually test the consumer** by publishing a test event to Kafka

5. **Inspect the SQL Server database** directly to verify table schema and permissions

## Summary

The enhanced error handling in DeliveryTrackingService will now show:
- gRPC Status.Detail with specific error information
- Which operation failed (driver lookup, delivery lookup, route lookup)
- Better stack traces for debugging

Use the steps above to identify where in the pipeline the route data is not flowing, then we can fix that specific component.
