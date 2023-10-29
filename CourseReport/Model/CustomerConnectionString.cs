using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CourseReport.API.Model
{
    [Table("CustomerConnectionString", Schema = "Masters")]
    [NotMapped]
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
