using Assessment.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model
{
    [Table("AuthoringMaster", Schema = "Course")]

    public class AuthoringMaster : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(500)]
        public string? Skills { get; set; }

        [Required]
        public string? Description { get; set; }
        public int LCMSId { get; set; }
        public string? ModuleType { get; set; }
        public string? MetaData { get; set; }
    }
}
