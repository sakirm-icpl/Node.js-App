using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class SubjectiveAssessmentStatus
    {
        public int Id { get; set; }
        public int AssessmentResultID { get; set; }
        public int HeaderID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public int CheckerID { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
    }
}
