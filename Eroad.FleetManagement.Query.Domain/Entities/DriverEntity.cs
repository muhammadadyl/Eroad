using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Eroad.FleetManagement.Query.Domain.Entities
{
    [Table("Driver", Schema = "dbo")]
    public class DriverEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string DriverLicense { get; set; }
        
        public string Status { get; set; }
    }
}
