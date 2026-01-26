using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.RouteManagement.Query.Domain.Entities
{
    [Table("Checkpoint", Schema = "dbo")]
    public class CheckpointEntity
    {
        [Key]
        public Guid RouteId { get; set; }
        
        [Key]
        public int Sequence { get; set; }
        
        public string Location { get; set; }
        
        public DateTime ExpectedTime { get; set; }
        
        [ForeignKey(nameof(RouteId))]
        [JsonIgnore]
        public virtual RouteEntity? Route { get; set; }
    }
}
