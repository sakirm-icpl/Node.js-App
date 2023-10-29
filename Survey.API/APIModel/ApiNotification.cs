using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class ApiNotification
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }

        public int? QuizId { get; set; }
        public int? SurveyId { get; set; }

    }
}
