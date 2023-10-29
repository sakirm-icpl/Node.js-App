using Courses.API.Validations;
using System.ComponentModel.DataAnnotations;
using Courses.API.Helper;

namespace Courses.API.APIModel
{
    public class APICourseTagged
    {
        public int Id { get; set; }
        [MinLength(2), MaxLength(8)]
        [Required]
        public string TagName { get; set; }
    }
}
