using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.CourseRating
{
    public class CourseRating : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
        public decimal Average { get; set; }
    }
}
