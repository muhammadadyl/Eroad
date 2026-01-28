using Confluent.Kafka;
using Eroad.CQRS.Core.Consumers;
using Eroad.DeliveryTracking.Query.API.Services.Grpc;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.Consumers;
using Eroad.DeliveryTracking.Query.Infrastructure.Converters;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Eroad.DeliveryTracking.Query.Infrastructure.Handlers;
using Eroad.DeliveryTracking.Query.Infrastructure.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using EventHandler = Eroad.DeliveryTracking.Query.Infrastructure.Handlers.EventHandler;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Add Logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

/// Configure Action to Configure DbContext
Action<DbContextOptionsBuilder> configureDbContext = (o => o.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
builder.Services.AddSingleton(new DatabaseContextFactory(configureDbContext));

// Create database and tables from code
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

// Create custom JSON serializer options for Kafka consumer
var jsonOptions = new System.Text.Json.JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
jsonOptions.Converters.Add(new EventJsonConverter());

// Register Kafka consumer configuration
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, EventConsumer>();

// Register repositories
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
builder.Services.AddScoped<IDeliveryCheckpointRepository, DeliveryCheckpointRepository>();
builder.Services.AddScoped<IDeliveryEventLogRepository, DeliveryEventLogRepository>();

// Register event handler
builder.Services.AddScoped<IEventHandler, EventHandler>();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register Kafka consumer as hosted service
builder.Services.AddHostedService<ConsumerHostedService>();
builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("delivery_tracking_query", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<DeliveryLookupGrpcService>();
app.MapGrpcService<IncidentLookupGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// Create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    context.Database.EnsureCreated();
}

app.Run();
