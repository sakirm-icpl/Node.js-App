using System;
using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class PostAssessmentResult : CommonFields
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public int? ModuleId { get; set; }
        public int NoOfAttempts { get; set; }
        public double MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public decimal AssessmentPercentage { get; set; }
        [MaxLength(30)]
        public string AssessmentResult { get; set; }
        public int TotalNoQuestions { get; set; }
        [MaxLength(30)]
        public string PostAssessmentStatus { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsContentAssessment { get; set; }
        public bool IsAdaptiveAssessment { get; set; }
        public DateTime? AssessmentStartTime { get; set; }
        public DateTime? AssessmentEndTime { get; set; }
        public bool? IsReviewedBySME { get; set; }
        public bool IsManagerEvaluation { get; set; }
    }  
}
