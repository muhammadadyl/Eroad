using Confluent.Kafka;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Infrastructure.Consumers;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Eroad.FleetManagement.Query.Infrastructure.Consumers;
using Eroad.FleetManagement.Query.Infrastructure.DataAccess;
using Eroad.FleetManagement.Query.Infrastructure.Handlers;
using Eroad.FleetManagement.Query.Infrastructure.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using EventHandler = Eroad.FleetManagement.Query.Infrastructure.Handlers.EventHandler;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
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

// Configure Action to Configure DbContext
Action<DbContextOptionsBuilder> configureDbContext = (o => o.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
builder.Services.AddSingleton(new DatabaseContextFactory(configureDbContext));

// Create database and tables from code
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IEventHandler, EventHandler>();

// Configure Kafka Consumer
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, Eroad.FleetManagement.Query.Infrastructure.Consumers.EventConsumer>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHostedService<ConsumerHostedService>();
builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("fleet_management_query_health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<Eroad.FleetManagement.Query.API.Services.Grpc.DriverLookupGrpcService>();
app.MapGrpcService<Eroad.FleetManagement.Query.API.Services.Grpc.VehicleLookupGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
