namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class DriverStatusListResponse
    {
        public string Message { get; set; }
        public required List<StatusInfo> Statuses { get; set; }
    }
}
