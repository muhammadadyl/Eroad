using Confluent.Kafka;
using Eroad.CQRS.Core.Consumers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.RouteManagement.Query.API.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;
using Eroad.RouteManagement.Query.Infrastructure.Consumers;
using Eroad.RouteManagement.Query.Infrastructure.DataAccess;
using Eroad.RouteManagement.Query.Infrastructure.Dispatchers;
using Eroad.RouteManagement.Query.Infrastructure.Handlers;
using Eroad.RouteManagement.Query.Infrastructure.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using EventHandler = Eroad.RouteManagement.Query.Infrastructure.Handlers.EventHandler;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Configure Action to Configure DbContext
Action<DbContextOptionsBuilder> configureDbContext = (o => o.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
builder.Services.AddSingleton(new DatabaseContextFactory(configureDbContext));

// Create database and tables from code
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<ICheckpointRepository, CheckpointRepository>();
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
builder.Services.AddScoped<IEventHandler, EventHandler>();
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, EventConsumer>();

// Register Query Handler
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();

var routeDispatcher = new RouteQueryDispatcher();
routeDispatcher.RegisterHandler<FindAllRoutesQuery>(queryHandler.HandleAsync);
routeDispatcher.RegisterHandler<FindRouteByIdQuery>(queryHandler.HandleAsync);
routeDispatcher.RegisterHandler<FindRoutesByStatusQuery>(queryHandler.HandleAsync);
routeDispatcher.RegisterHandler<FindRoutesByDriverIdQuery>(queryHandler.HandleAsync);
routeDispatcher.RegisterHandler<FindRoutesByVehicleIdQuery>(queryHandler.HandleAsync);

var checkpointDispatcher = new CheckpointQueryDispatcher();
checkpointDispatcher.RegisterHandler<FindCheckpointsByRouteIdQuery>(queryHandler.HandleAsync);

// Register Query Dispatcher
builder.Services.AddSingleton<IQueryDispatcher<RouteEntity>>(routeDispatcher);
builder.Services.AddSingleton<IQueryDispatcher<CheckpointEntity>>(checkpointDispatcher);

builder.Services.AddHostedService<ConsumerHostedService>();
builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("route_management_query_health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<Eroad.RouteManagement.Query.API.Services.Grpc.RouteLookupGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
