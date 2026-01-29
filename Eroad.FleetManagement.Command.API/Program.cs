using Confluent.Kafka;
using Eroad.CQRS.Core.Config;
using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;
using Eroad.CQRS.Core.Handlers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Infrastructure.Producers;
using Eroad.CQRS.Core.Infrastructure.Stores;
using Eroad.CQRS.Core.Producers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using Eroad.FleetManagement.Command.Infrastructure.Handlers;
using Eroad.FleetManagement.Common;
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
MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
BsonClassMap.RegisterClassMap<DomainEvent>();
BsonClassMap.RegisterClassMap<VehicleAddedEvent>();
BsonClassMap.RegisterClassMap<VehicleUpdatedEvent>();
BsonClassMap.RegisterClassMap<VehicleStatusChangedEvent>();
BsonClassMap.RegisterClassMap<DriverAddedEvent>();
BsonClassMap.RegisterClassMap<DriverUpdatedEvent>();
BsonClassMap.RegisterClassMap<DriverStatusChangedEvent>();

// Add services to the container.
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection(nameof(KafkaConfig)));
builder.Services.AddScoped<IEventStoreRepository, Eroad.CQRS.Core.Infrastructure.Repositories.EventStoreRepository>();
builder.Services.AddScoped<IEventProducer, EventProducer>();
builder.Services.AddScoped<IEventStore, EventStore>();
builder.Services.AddScoped<IEventSourcingHandler<VehicleAggregate>, VehicleEventSourcingHandler>();
builder.Services.AddScoped<IEventSourcingHandler<DriverAggregate>, DriverEventSourcingHandler>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("fleet_management_command_health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<Eroad.FleetManagement.Command.API.Services.Grpc.VehicleCommandGrpcService>();
app.MapGrpcService<Eroad.FleetManagement.Command.API.Services.Grpc.DriverCommandGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
