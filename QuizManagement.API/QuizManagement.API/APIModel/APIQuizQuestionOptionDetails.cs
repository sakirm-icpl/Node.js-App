namespace QuizManagement.API.APIModel
{
    public class APIQuizQuestionOptionDetails
    {
        public int QuizID { get; set; }
        public int QuizQuestionID { get; set; }
        public string QuizQuestionText { get; set; }
        public int QuizOptionID { get; set; }
        public string QuizOptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string SelectedAnswer { get; set; }
    }
}
