using System.ComponentModel.DataAnnotations;

namespace Survey.API.Models
{
    public class SurveyQuestionRejected : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Options { get; set; }
        [MaxLength(2000)]
        public string Question { get; set; }
        [MaxLength(2000)]
        public string ActiveQuestion { get; set; }
        [MaxLength(2000)]
        public string ObjectiveQuestion { get; set; }
        [MaxLength(250)]
        public string EnterOption1 { get; set; }
        [MaxLength(250)]
        public string EnterOption2 { get; set; }
        [MaxLength(250)]
        public string EnterOption3 { get; set; }
        [MaxLength(250)]
        public string EnterOption4 { get; set; }
        [MaxLength(250)]
        public string EnterOption5 { get; set; }
        [MaxLength(250)]
        public string EnterOption6 { get; set; }
        [MaxLength(250)]
        public string EnterOption7 { get; set; }
        [MaxLength(250)]
        public string EnterOption8 { get; set; }
        [MaxLength(250)]
        public string EnterOption9 { get; set; }
        [MaxLength(250)]
        public string EnterOption10 { get; set; }

        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        public bool IsMultipleChoice { get; set; }

    }
}
