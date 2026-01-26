using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.DeliveryTracking.Query.Domain.Entities
{
    [Table("DeliveryCheckpoint", Schema = "dbo")]
    public class DeliveryCheckpointEntity
    {
        [Key]
        public Guid DeliveryId { get; set; }

        [Key]
        public int Sequence { get; set; }

        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime ReachedAt { get; set; }

        [ForeignKey(nameof(DeliveryId))]
        [JsonIgnore]
        public virtual DeliveryEntity? Delivery { get; set; }
    }
}
