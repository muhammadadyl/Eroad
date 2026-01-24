using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.FleetManagement.Query.Domain.Entities
{
    [Table("Vehicle", Schema = "dbo")]
    public class VehicleEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Registration { get; set; }
        
        public string VehicleType { get; set; }
        
        public string Status { get; set; }

        public Guid? AssignedDriverId { get; set; }
        
        [ForeignKey(nameof(AssignedDriverId))]
        [JsonIgnore]
        public virtual DriverEntity? AssignedDriver { get; set; }
    }
}
