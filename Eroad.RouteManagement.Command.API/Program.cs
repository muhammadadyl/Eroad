using Confluent.Kafka;
using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;
using Eroad.CQRS.Core.Handlers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.RouteManagement.Command.API.Commands;
using Eroad.RouteManagement.Command.API.Commands.Route;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using Eroad.RouteManagement.Command.Infrastructure.Config;
using Eroad.RouteManagement.Command.Infrastructure.Handlers;
using Eroad.RouteManagement.Command.Infrastructure.Producers;
using Eroad.RouteManagement.Command.Infrastructure.Repositories;
using Eroad.RouteManagement.Command.Infrastructure.Stores;
using Eroad.RouteManagement.Common;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register BSON class maps for domain events
BsonClassMap.RegisterClassMap<DomainEvent>();
BsonClassMap.RegisterClassMap<RouteCreatedEvent>();
BsonClassMap.RegisterClassMap<RouteUpdatedEvent>();
BsonClassMap.RegisterClassMap<RouteStatusChangedEvent>();
BsonClassMap.RegisterClassMap<CheckpointAddedEvent>();
BsonClassMap.RegisterClassMap<CheckpointUpdatedEvent>();
BsonClassMap.RegisterClassMap<DriverAssignedToRouteEvent>();
BsonClassMap.RegisterClassMap<VehicleAssignedToRouteEvent>();

// Add services to the container.
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection(nameof(ProducerConfig)));
builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection(nameof(KafkaConfig)));
builder.Services.AddScoped<IEventStoreRepository, EventStoreRepository>();
builder.Services.AddScoped<IEventProducer, EventProducer>();
builder.Services.AddScoped<IEventStore, EventStore>();
builder.Services.AddScoped<IEventSourcingHandler<RouteAggregate>, RouteEventSourcingHandler>();
builder.Services.AddScoped<IRouteCommandHandler, RouteCommandHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
