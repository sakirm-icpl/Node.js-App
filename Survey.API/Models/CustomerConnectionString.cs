using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.API.Models
{
    [Table("CustomerConnectionString", Schema = "Masters")]
    public class CustomerConnectionString
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Code { get; set; }
        [Required]
        [MaxLength(100)]
        public string ConnectionString { get; set; }
    }
}
