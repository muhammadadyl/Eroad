using Confluent.Kafka;
using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;
using Eroad.CQRS.Core.Handlers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.DeliveryTracking.Command.API.Commands.Delivery;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using Eroad.DeliveryTracking.Command.Infrastructure.Config;
using Eroad.DeliveryTracking.Command.Infrastructure.Handlers;
using Eroad.DeliveryTracking.Command.Infrastructure.Producers;
using Eroad.DeliveryTracking.Command.Infrastructure.Repositories;
using Eroad.DeliveryTracking.Command.Infrastructure.Stores;
using Eroad.DeliveryTracking.Common;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Register BSON class maps for domain events
MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
BsonClassMap.RegisterClassMap<DomainEvent>();
BsonClassMap.RegisterClassMap<DeliveryCreatedEvent>();
BsonClassMap.RegisterClassMap<DeliveryStatusChangedEvent>();
BsonClassMap.RegisterClassMap<CheckpointReachedEvent>();
BsonClassMap.RegisterClassMap<IncidentReportedEvent>();
BsonClassMap.RegisterClassMap<IncidentResolvedEvent>();
BsonClassMap.RegisterClassMap<ProofOfDeliveryCapturedEvent>();

// Add services to the container.
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection(nameof(KafkaConfig)));
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventProducer, EventProducer>();
builder.Services.AddScoped<IEventStore, EventStore>();
builder.Services.AddScoped<IEventSourcingHandler<DeliveryAggregate>, DeliveryEventSourcingHandler>();
builder.Services.AddScoped<IDeliveryCommandHandler, DeliveryCommandHandler>();

builder.Services.AddHealthChecks();

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks()
    .AddCheck("delivery_tracking_command_health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

app.MapHealthChecks("/health");

// Map gRPC services
app.MapGrpcService<Eroad.DeliveryTracking.Command.API.Services.Grpc.DeliveryCommandGrpcService>();
app.MapGrpcHealthChecksService();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
