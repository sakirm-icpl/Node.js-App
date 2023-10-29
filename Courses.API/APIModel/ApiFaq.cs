using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class ApiFaq
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        public string Metadata { get; set; }
        public string Description { get; set; }
        public int LcmsId { get; set; }
    }
}
