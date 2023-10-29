using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class UserPrefferedCourseLanguage : BaseModel
    {
        public int Id { get; set; }
        public int? UserID { get; set; }       
        [Required]
        public int? CourseId { get; set; }
        [Required]
        public string LanguageCode { get; set; }
        public int ModuleId { get; set; }
    }
}
