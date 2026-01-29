using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.BFF.Gateway.Application.Services;
using Eroad.BFF.Gateway.Application.Validators;
using Eroad.BFF.Gateway.Presentation.Middleware;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;
using StackExchange.Redis;
using Grpc.Core;
using System.Threading.RateLimiting;

// Enable insecure HTTP/2 for gRPC (required for non-TLS connections)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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

// Create a logger factory for startup logging
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Configure Polly v8 Resilience Pipeline for gRPC
var grpcResiliencePipeline = new ResiliencePipelineBuilder()
    // Retry policy with exponential backoff
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<RpcException>(ex =>
            ex.StatusCode == StatusCode.Unavailable ||
            ex.StatusCode == StatusCode.DeadlineExceeded ||
            ex.StatusCode == StatusCode.Internal),
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            logger.LogWarning("gRPC Retry {RetryAttempt} after {Delay}ms due to {Exception}",
                args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
            return ValueTask.CompletedTask;
        }
    })
    // Circuit breaker to prevent cascading failures
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(15),
        ShouldHandle = new PredicateBuilder().Handle<RpcException>(),
        OnOpened = args =>
        {
            logger.LogError("Circuit breaker OPENED due to {Exception}", args.Outcome.Exception?.Message);
            return ValueTask.CompletedTask;
        },
        OnClosed = args =>
        {
            logger.LogInformation("Circuit breaker CLOSED");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = args =>
        {
            logger.LogInformation("Circuit breaker HALF-OPENED");
            return ValueTask.CompletedTask;
        }
    })
    // Timeout policy
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(30),
        OnTimeout = args =>
        {
            logger.LogWarning("gRPC request timed out after 30 seconds");
            return ValueTask.CompletedTask;
        }
    })
    .Build();

// Register Delivery Tracking gRPC clients (Query)
builder.Services
    .AddGrpcClient<DeliveryLookup.DeliveryLookupClient>(options =>
    {
        options.Address = new Uri(deliveryTrackingBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
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
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
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
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

builder.Services
    .AddGrpcClient<VehicleLookup.VehicleLookupClient>(options =>
    {
        options.Address = new Uri(fleetManagementBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
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
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

builder.Services
    .AddGrpcClient<VehicleCommand.VehicleCommandClient>(options =>
    {
        options.Address = new Uri(fleetManagementCommandBaseUrl);
    })
    .ConfigureChannel(options =>
    {
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
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
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
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
        options.MaxRetryAttempts = 0; // Disable built-in retries, use Polly
        options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
    });

// Register the Polly resilience pipeline as a singleton
builder.Services.AddSingleton(grpcResiliencePipeline);

// Configure rate limiting using Polly
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter: 100 requests per 10 seconds per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetSlidingWindowLimiter(clientId, _ => 
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit exceeded for {ClientId}", 
            context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            message = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : 10
        }, cancellationToken);
    };
});

// Register Redis for distributed locking
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? throw new InvalidOperationException("Redis connection string not configured");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// Register distributed lock manager
builder.Services.AddSingleton<IDistributedLockManager, RedisLockManager>();

// Register assignment validator
builder.Services.AddScoped<DeliveryAssignmentValidator>();

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

// Make the implicit Program class accessible to the test project
public partial class Program { }
