namespace Eroad.RouteManagement.Query.API.DTOs
{
    public class StatusLookupResponse
    {
        public string Message { get; set; }
        public required List<StatusInfo> Statuses { get; set; }
    }
}
