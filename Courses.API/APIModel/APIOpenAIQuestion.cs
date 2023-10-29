namespace Courses.API.APIModel
{
    public class APIOpenAIQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public string Metadata { get; set; }
        public string Industry { get; set; }
        public string CourseCode { get; set; }
    }
}
