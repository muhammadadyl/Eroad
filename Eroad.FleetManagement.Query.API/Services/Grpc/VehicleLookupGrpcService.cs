using Eroad.FleetManagement.Contracts;
using Eroad.FleetManagement.Query.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Eroad.FleetManagement.Query.API.Services.Grpc
{
    public class VehicleLookupGrpcService : VehicleLookup.VehicleLookupBase
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleLookupGrpcService> _logger;

        public VehicleLookupGrpcService(IVehicleRepository vehicleRepository, ILogger<VehicleLookupGrpcService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public override async Task<VehicleLookupResponse> GetVehicleById(GetVehicleByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Vehicle with ID {request.Id} not found"));
                }

                return new VehicleLookupResponse
                {
                    Message = "Successfully returned vehicle",
                    Vehicles = { MapToProto(vehicle) }
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle by ID: {VehicleId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the vehicle"));
            }
        }

        public override async Task<VehicleLookupResponse> GetAllVehicles(GetAllVehiclesRequest request, ServerCallContext context)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAllAsync();
                
                var response = new VehicleLookupResponse
                {
                    Message = $"Successfully returned {vehicles.Count} vehicle(s)"
                };
                response.Vehicles.AddRange(vehicles.Select(MapToProto));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vehicles");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving vehicles"));
            }
        }

        public override async Task<VehicleLookupResponse> GetVehiclesByDriver(GetVehiclesByDriverRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.DriverId, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var vehicles = await _vehicleRepository.GetByDriverIdAsync(driverId);
                
                var response = new VehicleLookupResponse
                {
                    Message = $"Successfully returned {vehicles.Count} vehicle(s)"
                };
                response.Vehicles.AddRange(vehicles.Select(MapToProto));
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles by driver: {DriverId}", request.DriverId);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving vehicles"));
            }
        }

        public override async Task<VehicleLookupResponse> GetVehiclesByStatus(GetVehiclesByStatusRequest request, ServerCallContext context)
        {
            try
            {
                var vehicles = await _vehicleRepository.GetByStatusAsync(request.Status);
                
                var response = new VehicleLookupResponse
                {
                    Message = $"Successfully returned {vehicles.Count} vehicle(s) with status '{request.Status}'"
                };
                response.Vehicles.AddRange(vehicles.Select(MapToProto));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles by status: {Status}", request.Status);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving vehicles"));
            }
        }

        private static VehicleEntity MapToProto(Domain.Entities.VehicleEntity vehicle)
        {
            var entity = new VehicleEntity
            {
                Id = vehicle.Id.ToString(),
                Registration = vehicle.Registration,
                VehicleType = vehicle.VehicleType,
                Status = vehicle.Status
            };

            if (vehicle.AssignedDriverId.HasValue)
            {
                entity.AssignedDriverId = vehicle.AssignedDriverId.Value.ToString();
            }

            return entity;
        }
    }
}
