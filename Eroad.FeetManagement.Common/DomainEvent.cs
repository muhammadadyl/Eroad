using CQRS.Core.Events;

namespace Eroad.FeetManagement.Common
{
    public record DriverAssignedEvent(
        Guid DriverId,
        Guid VehicleId) : DomainEvent(nameof(DriverAssignedEvent));

    public record VehicleAddedEvent(
        string Registration,
        string VehicleType,
        VehicleStatus VehicleStatus = VehicleStatus.Available
        ) : DomainEvent(nameof(VehicleAddedEvent));


    public record VehicleUpdatedEvent(
        Guid VehicleId,
        string Registration,
        string VehicleType
        ) : DomainEvent(nameof(VehicleUpdatedEvent));

    public record VehicleStatusChangedEvent(
        Guid VehicleId,
        VehicleStatus OldStatus,
        VehicleStatus NewStatus,
        string Reason) : DomainEvent(nameof(VehicleStatusChangedEvent));

    public record DriverAddedEvent(
        string Name,
        string DriverLicence,
        DriverStatus DriverStatus = DriverStatus.Available
        ) : DomainEvent(nameof(DriverAddedEvent));

    public record DriverUpdatedEvent(
        Guid DriverId,
        string DriverLicence
        ) : DomainEvent(nameof(DriverUpdatedEvent));

    public record DriverStatusChangedEvent(
        Guid DriverId,
        DriverStatus OldStatus,
        DriverStatus NewStatus
        ) : DomainEvent(nameof(DriverStatusChangedEvent));
}
