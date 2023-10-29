using System.ComponentModel.DataAnnotations;


namespace Assessment.API.Models
{
    public class AssessmentQuestionRejected : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Section { get; set; }
        [MaxLength(200)]
        public string LearnerInstruction { get; set; }
        [MaxLength(2000)]
        public string QuestionText { get; set; }
        [MaxLength(250)]
        public string DifficultyLevel { get; set; }
        [MaxLength(250)]
        public string Time { get; set; }
        [MaxLength(500)]
        public string ModelAnswer { get; set; }
        [MaxLength(200)]
        public string MediaFile { get; set; }
        [MaxLength(250)]
        public string AnswerAsImages { get; set; }
        [MaxLength(250)]
        public string Marks { get; set; }
        [MaxLength(250)]
        public string Status { get; set; }
        [MaxLength(250)]
        public string QuestionStyle { get; set; }
        [MaxLength(250)]
        public string QuestionType { get; set; }
        [MaxLength(250)]
        public string Metadata { get; set; }
        [MaxLength(250)]
        public string AnswerOptions { get; set; }
        [MaxLength(250)]
        public string AnswerOption1 { get; set; }
        [MaxLength(250)]
        public string AnswerOption2 { get; set; }
        [MaxLength(250)]
        public string AnswerOption3 { get; set; }
        [MaxLength(250)]
        public string AnswerOption4 { get; set; }
        [MaxLength(250)]
        public string AnswerOption5 { get; set; }
        [MaxLength(250)]
        public string CorrectAnswer1 { get; set; }
        [MaxLength(250)]
        public string CorrectAnswer2 { get; set; }
        [MaxLength(250)]
        public string CorrectAnswer3 { get; set; }
        [MaxLength(250)]
        public string CorrectAnswer4 { get; set; }
        [MaxLength(250)]
        public string CorrectAnswer5 { get; set; }
        public string CourseCode { get; set; }
        [MaxLength(500)]
        public string ErrorMessage { get; set; }
    }
}
