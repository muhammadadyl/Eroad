using Eroad.CQRS.Core.Events;

namespace Eroad.FleetManagement.Common
{
    public record VehicleAddedEvent(
        string Registration,
        string VehicleType,
        VehicleStatus VehicleStatus = VehicleStatus.Available
        ) : DomainEvent(nameof(VehicleAddedEvent));


    public record VehicleUpdatedEvent(
        string Registration,
        string VehicleType
        ) : DomainEvent(nameof(VehicleUpdatedEvent));

    public record VehicleStatusChangedEvent(
        VehicleStatus OldStatus,
        VehicleStatus NewStatus,
        string Reason) : DomainEvent(nameof(VehicleStatusChangedEvent));

    public record DriverAddedEvent(
        string Name,
        string DriverLicence,
        DriverStatus DriverStatus = DriverStatus.Available
        ) : DomainEvent(nameof(DriverAddedEvent));

    public record DriverUpdatedEvent(
        string DriverLicence
        ) : DomainEvent(nameof(DriverUpdatedEvent));

    public record DriverStatusChangedEvent(
        DriverStatus OldStatus,
        DriverStatus NewStatus
        ) : DomainEvent(nameof(DriverStatusChangedEvent));
}
