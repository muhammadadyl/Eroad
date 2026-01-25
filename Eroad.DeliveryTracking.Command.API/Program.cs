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
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register BSON class maps for domain events
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
