using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class AssessmentHeader : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int CourseID { get; set; }

        public int ModuleID { get; set; }
        [Required]
        public int NumberOfQuestionMaintained { get; set; }

        public int NumberOfObjectiveQuestions { get; set; }

        public int NumberOfSubjectiveQuestions { get; set; }
        [Required]
        public int TotalMarkOfAllQuestions { get; set; }

        public int TotalMarksOfObjectiveQuestions { get; set; }

        public int TotalMarksOfSubjectiveQuestions { get; set; }

        public int NumberOfObjectiveQuestionsAsked { get; set; }

        public int NumberOfSubjectiveQuestionsAsked { get; set; }

        public bool RandomizedQuestion { get; set; }

        public bool AdaptiveAssessment { get; set; }

        public bool NegativeMarking { get; set; }

        public bool AllowSkipQuestion { get; set; }
        [Required]
        public int PassingScore { get; set; }

        public int NumberOfAttempts { get; set; }
        [Required]
        public bool IsPreAssessment { get; set; }
    }
}
