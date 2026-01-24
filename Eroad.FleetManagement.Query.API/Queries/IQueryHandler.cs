using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public interface IQueryHandler
    {
        Task<List<DriverEntity>> HandleAsync(FindAllDriversQuery query);
        Task<List<DriverEntity>> HandleAsync(FindDriverByIdQuery query);
        Task<List<DriverEntity>> HandleAsync(FindDriversByStatusQuery query);
        Task<List<VehicleEntity>> HandleAsync(FindAllVehiclesQuery query);
        Task<List<VehicleEntity>> HandleAsync(FindVehicleByIdQuery query);
        Task<List<VehicleEntity>> HandleAsync(FindVehicleByDriverIdQuery query);
        Task<List<VehicleEntity>> HandleAsync(FindVehiclesByStatusQuery query);
    }
}
