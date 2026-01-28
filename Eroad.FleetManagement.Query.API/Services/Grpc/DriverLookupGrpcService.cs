using Eroad.FleetManagement.Contracts;
using Eroad.FleetManagement.Query.API.Queries;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Services.Grpc
{
    public class DriverLookupGrpcService : DriverLookup.DriverLookupBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DriverLookupGrpcService> _logger;

        public DriverLookupGrpcService(IMediator mediator, ILogger<DriverLookupGrpcService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<DriverResponse> GetDriverById(GetDriverByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var drivers = await _mediator.Send(new FindDriverByIdQuery { Id = driverId }, context.CancellationToken);
                if (drivers == null || drivers.Count == 0)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Driver with ID {request.Id} not found"));
                }

                return new DriverResponse
                {
                    Message = "Successfully returned driver",
                    Driver = MapToProto(drivers.FirstOrDefault())
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver by ID: {DriverId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the driver"));
            }
        }

        public override async Task<DriverLookupResponse> GetAllDrivers(GetAllDriversRequest request, ServerCallContext context)
        {
            try
            {
                var drivers = await _mediator.Send(new FindAllDriversQuery(), context.CancellationToken);
                
                var response = new DriverLookupResponse
                {
                    Message = $"Successfully returned {drivers.Count} driver(s)"
                };
                response.Drivers.AddRange(drivers.Select(MapToProto));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all drivers");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving drivers"));
            }
        }

        public override async Task<DriverLookupResponse> GetDriversByStatus(GetDriversByStatusRequest request, ServerCallContext context)
        {
            try
            {
                var drivers = await _mediator.Send(new FindDriversByStatusQuery { Status = request.Status }, context.CancellationToken);
                
                var response = new DriverLookupResponse
                {
                    Message = $"Successfully returned {drivers.Count} driver(s) with status '{request.Status}'"
                };
                response.Drivers.AddRange(drivers.Select(MapToProto));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers by status: {Status}", request.Status);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving drivers"));
            }
        }

        private static DriverEntity MapToProto(Domain.Entities.DriverEntity driver)
        {
            return new DriverEntity
            {
                Id = driver.Id.ToString(),
                Name = driver.Name,
                DriverLicense = driver.DriverLicense,
                Status = driver.Status
            };
        }
    }
}
