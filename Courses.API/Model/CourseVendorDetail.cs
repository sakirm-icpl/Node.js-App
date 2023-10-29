using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class CourseVendorDetail : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Code { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        [MaxLength(10)]
        public string Type { get; set; }

    }
}
