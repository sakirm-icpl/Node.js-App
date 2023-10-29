namespace Courses.API.APIModel.Assessment
{
    public class APIAssessmentReview
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string AnswerAsImages { get; set; }
        public string QuestionType { get; set; }
        public string QuestionStyle { get; set; }
        public string Section { get; set; }
        public string ContentType { get; set; }
        public string ContentPath { get; set; }
        public string Metadata { get; set; }
        public bool Status { get; set; }
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public string OptionType { get; set; }
        public string OptionContentType { get; set; }
        public string OptionContentPath { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string SelectedAnswer { get; set; }

    }
}
