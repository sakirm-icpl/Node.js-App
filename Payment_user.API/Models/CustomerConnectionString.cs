using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payment.API.Models
{
    [Table("CustomerConnectionString", Schema = "Masters")]
    public class CustomerConnectionString
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Code { get; set; }
        [Required]
        [MaxLength(20)]
        public string DatabaseCode { get; set; }
        [Required]
        [MaxLength(200)]
        public string ConnectionString { get; set; }
        [Required]
        [MaxLength(50)]
        public string Theme { get; set; }
        [Required]
        [MaxLength(100)]
        public string LogoName { get; set; }
    }
}
