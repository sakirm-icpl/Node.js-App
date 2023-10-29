using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class APIExportAllCoursesCompletionReport
    {
        public int Id { get; set; }
        [Required]
        public string ConfiguredColumnName { get; set; }
        [Required]
        public string ChangedColumnName { get; set; }


    }
}
