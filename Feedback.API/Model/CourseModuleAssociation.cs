using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("CourseModuleAssociation", Schema = "Course")]

    public class CourseModuleAssociation
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public bool IsPreAssessment { get; set; }
        public int? PreAssessmentId { get; set; }
        public bool IsAssessment { get; set; }
        public int? AssessmentId { get; set; }
        public bool IsFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public int? SequenceNo { get; set; }
        public int? SectionId { get; set; }

        public bool Isdeleted { get; set; }

        public int? CompletionPeriodDays { get; set; }
    }
}
