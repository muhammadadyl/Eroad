using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.RouteManagement.Query.Domain.Entities
{
    [Table("Route", Schema = "dbo")]
    public class RouteEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Origin { get; set; }
        
        public string Destination { get; set; }
        
        public string Status { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? UpdatedDate { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<CheckpointEntity> Checkpoints { get; set; }
    }
}
