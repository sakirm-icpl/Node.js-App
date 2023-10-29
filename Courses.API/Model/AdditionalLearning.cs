
using Courses.API.Model;
using System.ComponentModel.DataAnnotations;

namespace Course.API.Model
{
    public class AdditionalLearning : BaseModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        [MaxLength(100)]
        [Required]
        public string CourseTitle { get; set; }
        [MaxLength(100)]
        public string ContentId { get; set; }
        [MaxLength(100)]
        [Required]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string FileForUpload { get; set; }
        [MaxLength(100)]
        public string Foreword { get; set; }
        [MaxLength(100)]
        public string Author { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
    }
}
