using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel { 
    public class APICourseRatingReport
    {
        [Required]
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string UserId { get; set; }
        public string UseName { get; set; }
        public string ReviewRating { get; set; }
        public string ReviewText { get; set; }
        public string ModifiedDate { get; set; }
        public int PageSize { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }
        public string ExportAs { get; set; }
    }
}
