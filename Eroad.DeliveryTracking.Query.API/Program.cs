using Confluent.Kafka;
using Eroad.CQRS.Core.Consumers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.DeliveryTracking.Query.API.Queries;
using Eroad.DeliveryTracking.Query.API.Services.Grpc;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Eroad.DeliveryTracking.Query.Infrastructure.Consumers;
using Eroad.DeliveryTracking.Query.Infrastructure.Converters;
using Eroad.DeliveryTracking.Query.Infrastructure.DataAccess;
using Eroad.DeliveryTracking.Query.Infrastructure.Dispatchers;
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

// Register event handler
builder.Services.AddScoped<IEventHandler, EventHandler>();

// Register query handler
builder.Services.AddScoped<IQueryHandler, QueryHandler>();

// Register Query Handler
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();

// Register query dispatchers
var deliveryQueryDispatcher = new DeliveryQueryDispatcher();
deliveryQueryDispatcher.RegisterHandler<FindAllDeliveriesQuery>(queryHandler.HandleAsync);
deliveryQueryDispatcher.RegisterHandler<FindDeliveryByIdQuery>(queryHandler.HandleAsync);
deliveryQueryDispatcher.RegisterHandler<FindDeliveriesByStatusQuery>(queryHandler.HandleAsync);
deliveryQueryDispatcher.RegisterHandler<FindDeliveriesByRouteIdQuery>(queryHandler.HandleAsync);

var incidentQueryDispatcher = new IncidentQueryDispatcher();
incidentQueryDispatcher.RegisterHandler<FindIncidentsByDeliveryIdQuery>(queryHandler.HandleAsync);
incidentQueryDispatcher.RegisterHandler<FindAllUnresolvedIncidentsQuery>(queryHandler.HandleAsync);

builder.Services.AddSingleton<IQueryDispatcher<DeliveryEntity>>(deliveryQueryDispatcher);
builder.Services.AddSingleton<IQueryDispatcher<IncidentEntity>>(incidentQueryDispatcher);

// Configure JSON serializer with circular reference handling
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register Kafka consumer as hosted service
builder.Services.AddHostedService<ConsumerHostedService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("delivery_tracking_query", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Commented out for HTTP gRPC support

app.UseAuthorization();

app.MapControllers();
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
