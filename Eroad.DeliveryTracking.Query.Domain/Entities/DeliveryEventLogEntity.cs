using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eroad.DeliveryTracking.Query.Domain.Entities
{
    [Table("DeliveryEventLog", Schema = "dbo")]
    public class DeliveryEventLogEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid DeliveryId { get; set; }
        
        [Required]
        public string EventCategory { get; set; }
        
        [Required]
        public string EventType { get; set; }
        
        [Required]
        public string EventData { get; set; }
        
        public DateTime OccurredAt { get; set; }
    }
}
