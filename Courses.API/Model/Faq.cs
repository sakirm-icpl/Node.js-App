using Assessment.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class Faq : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int LcmsId { get; set; }
    }
}
