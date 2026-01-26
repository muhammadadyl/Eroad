using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.DeliveryTracking.Query.Domain.Entities
{
    [Table("Delivery", Schema = "dbo")]
    public class DeliveryEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid RouteId { get; set; }
        
        public Guid? DriverId { get; set; }
        
        public Guid? VehicleId { get; set; }
        
        public string Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? DeliveredAt { get; set; }
        
        public string? SignatureUrl { get; set; }
        
        public string? ReceiverName { get; set; }
        
        public string? CurrentCheckpoint { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<IncidentEntity> Incidents { get; set; } = new List<IncidentEntity>();

        [JsonIgnore]
        public virtual ICollection<DeliveryCheckpointEntity> Checkpoints { get; set; } = new List<DeliveryCheckpointEntity>();
    }
}
