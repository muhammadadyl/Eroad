using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.DeliveryTracking.Query.Domain.Entities
{
    [Table("Incident", Schema = "dbo")]
    public class IncidentEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid DeliveryId { get; set; }
        
        public string Type { get; set; }
        
        public string Description { get; set; }
        
        public DateTime ReportedTimestamp { get; set; }
        
        public DateTime? ResolvedTimestamp { get; set; }
        
        public bool Resolved { get; set; }
        
        [ForeignKey(nameof(DeliveryId))]
        [JsonIgnore]
        public virtual DeliveryEntity? Delivery { get; set; }
    }
}
