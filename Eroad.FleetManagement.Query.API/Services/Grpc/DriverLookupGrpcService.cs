using Eroad.FleetManagement.Contracts;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Eroad.FleetManagement.Query.API.Services.Grpc
{
    public class DriverLookupGrpcService : DriverLookup.DriverLookupBase
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ILogger<DriverLookupGrpcService> _logger;

        public DriverLookupGrpcService(IDriverRepository driverRepository, ILogger<DriverLookupGrpcService> logger)
        {
            _driverRepository = driverRepository;
            _logger = logger;
        }

        public override async Task<DriverLookupResponse> GetDriverById(GetDriverByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var driver = await _driverRepository.GetByIdAsync(driverId);
                if (driver == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Driver with ID {request.Id} not found"));
                }

                return new DriverLookupResponse
                {
                    Message = "Successfully returned driver",
                    Drivers = { MapToProto(driver) }
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
                var drivers = await _driverRepository.GetAllAsync();
                
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
                var drivers = await _driverRepository.GetByStatusAsync(request.Status);
                
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
            var entity = new DriverEntity
            {
                Id = driver.Id.ToString(),
                Name = driver.Name,
                DriverLicense = driver.DriverLicense,
                Status = driver.Status
            };

            if (driver.AssignedVehicleId.HasValue)
            {
                entity.AssignedVehicleId = driver.AssignedVehicleId.Value.ToString();
            }

            return entity;
        }
    }
}
