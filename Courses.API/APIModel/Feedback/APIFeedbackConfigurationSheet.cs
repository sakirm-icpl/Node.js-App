namespace Feedback.API.APIModel
{
    public class APIFeedbackConfigurationSheet
    {
        public int FeedbackConfiID { get; set; }
        public bool IsEmoji { get; set; }
        public APIFeedbackConfiguration[] aPIFeedbackConfiguration { get; set; }

    }

    public class APIFeedbackConfiguration
    {
        public int FeedbackId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
    }
}
