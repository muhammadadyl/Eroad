using Confluent.Kafka;
using Eroad.CQRS.Core.Consumers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Query.API.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Eroad.FleetManagement.Query.Infrastructure.Consumers;
using Eroad.FleetManagement.Query.Infrastructure.DataAccess;
using Eroad.FleetManagement.Query.Infrastructure.Dispatchers;
using Eroad.FleetManagement.Query.Infrastructure.Handlers;
using Eroad.FleetManagement.Query.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using EventHandler = Eroad.FleetManagement.Query.Infrastructure.Handlers.EventHandler;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
builder.Services.AddScoped<IEventHandler, EventHandler>();
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, EventConsumer>();

// Register Query Handler
var queryHandler = builder.Services.BuildServiceProvider().GetRequiredService<IQueryHandler>();

var driverDispatcher = new DriverQueryDispatcher();
driverDispatcher.RegisterHandler<FindAllDriversQuery>(queryHandler.HandleAsync);
driverDispatcher.RegisterHandler<FindDriverByIdQuery>(queryHandler.HandleAsync);
driverDispatcher.RegisterHandler<FindDriversByStatusQuery>(queryHandler.HandleAsync);

var vehicleDispatcher = new VehicleQueryDispatcher();
vehicleDispatcher.RegisterHandler<FindAllVehiclesQuery>(queryHandler.HandleAsync);
vehicleDispatcher.RegisterHandler<FindVehicleByIdQuery>(queryHandler.HandleAsync);
vehicleDispatcher.RegisterHandler<FindVehicleByDriverIdQuery>(queryHandler.HandleAsync);
vehicleDispatcher.RegisterHandler<FindVehiclesByStatusQuery>(queryHandler.HandleAsync);

// Register Query Dispatcher
builder.Services.AddSingleton<IQueryDispatcher<DriverEntity>>(driverDispatcher);
builder.Services.AddSingleton<IQueryDispatcher<VehicleEntity>>(vehicleDispatcher);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references during serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHostedService<ConsumerHostedService>();
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
