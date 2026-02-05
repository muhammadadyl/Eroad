using Confluent.Kafka;
using Eroad.CQRS.Core.Config;
using Eroad.CQRS.Core.Events;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Infrastructure.Producers;
using Eroad.CQRS.Core.Infrastructure.Repositories;
using Eroad.CQRS.Core.Infrastructure.Stores;
using Eroad.CQRS.Core.Producers;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using Eroad.RouteManagement.Command.Infrastructure.Handlers;
using Eroad.RouteManagement.Common;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Register BSON class maps for domain events
BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
BsonClassMap.RegisterClassMap<DomainEvent>();
BsonClassMap.RegisterClassMap<RouteCreatedEvent>();
BsonClassMap.RegisterClassMap<RouteUpdatedEvent>();
BsonClassMap.RegisterClassMap<RouteStatusChangedEvent>();
BsonClassMap.RegisterClassMap<CheckpointAddedEvent>();
BsonClassMap.RegisterClassMap<RouteScheduledEndTimeUpdatedEvent>();

// Add services to the container.
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection(nameof(KafkaConfig)));
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventProducer, EventProducer>();
builder.Services.AddScoped<IEventStore, EventStore>();
builder.Services.AddScoped<IEventSourcingHandler<RouteAggregate>, RouteEventSourcingHandler>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("route_management_command_health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<Eroad.RouteManagement.Command.API.Services.Grpc.RouteCommandGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
