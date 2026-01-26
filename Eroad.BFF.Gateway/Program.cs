using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.BFF.Gateway.Application.Services;
using Eroad.BFF.Gateway.Presentation.Middleware;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;
using Grpc.Net.ClientFactory;
using Polly;

// Enable insecure HTTP/2 for gRPC (required for non-TLS connections)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get service endpoint URLs from configuration
var serviceEndpoints = builder.Configuration.GetSection("ServiceEndpoints");
var deliveryTrackingBaseUrl = serviceEndpoints["DeliveryTrackingBaseUrl"] 
    ?? throw new InvalidOperationException("DeliveryTrackingBaseUrl not configured");
var deliveryTrackingCommandBaseUrl = serviceEndpoints["DeliveryTrackingCommandBaseUrl"]
    ?? throw new InvalidOperationException("DeliveryTrackingCommandBaseUrl not configured");
var fleetManagementBaseUrl = serviceEndpoints["FleetManagementBaseUrl"]
    ?? throw new InvalidOperationException("FleetManagementBaseUrl not configured");
var fleetManagementCommandBaseUrl = serviceEndpoints["FleetManagementCommandBaseUrl"]
    ?? throw new InvalidOperationException("FleetManagementCommandBaseUrl not configured");
var routeManagementBaseUrl = serviceEndpoints["RouteManagementBaseUrl"]
    ?? throw new InvalidOperationException("RouteManagementBaseUrl not configured");
var routeManagementCommandBaseUrl = serviceEndpoints["RouteManagementCommandBaseUrl"]
    ?? throw new InvalidOperationException("RouteManagementCommandBaseUrl not configured");

// Define Polly retry policy for gRPC
var retryPolicy = Policy
    .Handle<Grpc.Core.RpcException>(ex => 
        ex.StatusCode == Grpc.Core.StatusCode.Unavailable ||
        ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
        });

// Register Delivery Tracking gRPC clients (Query)
builder.Services
    .AddGrpcClient<DeliveryLookup.DeliveryLookupClient>(options =>
    {
        options.Address = new Uri(deliveryTrackingBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register Delivery Tracking gRPC clients (Command)
builder.Services
    .AddGrpcClient<DeliveryCommand.DeliveryCommandClient>(options =>
    {
        options.Address = new Uri(deliveryTrackingCommandBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register Fleet Management gRPC clients (Query)
builder.Services
    .AddGrpcClient<DriverLookup.DriverLookupClient>(options =>
    {
        options.Address = new Uri(fleetManagementBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

builder.Services
    .AddGrpcClient<VehicleLookup.VehicleLookupClient>(options =>
    {
        options.Address = new Uri(fleetManagementBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register Fleet Management gRPC clients (Command)
builder.Services
    .AddGrpcClient<DriverCommand.DriverCommandClient>(options =>
    {
        options.Address = new Uri(fleetManagementCommandBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

builder.Services
    .AddGrpcClient<VehicleCommand.VehicleCommandClient>(options =>
    {
        options.Address = new Uri(fleetManagementCommandBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register Route Management gRPC clients (Query)
builder.Services
    .AddGrpcClient<RouteLookup.RouteLookupClient>(options =>
    {
        options.Address = new Uri(routeManagementBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register Route Management gRPC clients (Command)
builder.Services
    .AddGrpcClient<RouteCommand.RouteCommandClient>(options =>
    {
        options.Address = new Uri(routeManagementCommandBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 3;
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register application services with interfaces for dependency injection
builder.Services.AddScoped<IDeliveryTrackingService, DeliveryTrackingService>();
builder.Services.AddScoped<IFleetManagementService, FleetManagementService>();
builder.Services.AddScoped<IRouteManagementService, RouteManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
