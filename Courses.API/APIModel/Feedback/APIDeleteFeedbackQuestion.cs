namespace Courses.API.APIModel.Feedback
{
    public class APIDeleteFeedbackQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string ErrMessage { get; set; }
        public int totalRecordInsert { get; set; }
        public int totalRecordRejected { get; set; }
    }
}
