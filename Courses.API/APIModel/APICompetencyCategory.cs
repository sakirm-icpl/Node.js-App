using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICompetencyCategory
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(8)]
        public string CategoryName { get; set; }
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

    }
}
