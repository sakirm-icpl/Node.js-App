using System;

namespace CourseReport.API.APIModel
{
    public class APICalendarCourseReport
    {
        public int Id { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsPreAssessment { get; set; }
        public string CourseStatus { get; set; }
        public string AssessmentStatus { get; set; }
        public string PreAssessmentStatus { get; set; }
        public string FeedbackStatus { get; set; }
        public string CourseTitle { get; set; }
        public DateTime CourseStartDate { get; set; }
        public DateTime CourseEndDate { get; set; }
        public DateTime AssessmentSubmitDate { get; set; }
        public DateTime PreAssessmentSubmitDate { get; set; }
        public DateTime FeedbcaskSubmitDate { get; set; }
    }
}
