namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public interface IDeliveryCommandHandler
    {
        Task HandleAsync(CreateDeliveryCommand command);
        Task HandleAsync(UpdateDeliveryStatusCommand command);
        Task HandleAsync(UpdateCurrentCheckpointCommand command);
        Task HandleAsync(ReportIncidentCommand command);
        Task HandleAsync(ResolveIncidentCommand command);
        Task HandleAsync(CaptureProofOfDeliveryCommand command);
    }
}
