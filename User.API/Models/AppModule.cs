using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.API.Models
{
    [Table("AppModule", Schema = "Masters")]
    public class AppModule : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string ModuleCode { get; set; }
        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; }
        [MaxLength(70)]
        public string ChangeModuleName { get; set; }
    }
}
