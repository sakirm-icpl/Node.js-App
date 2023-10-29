using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class ApiSurveyQuestionExist
    {
        [Required]
        public string Question { get; set; }
        [Required]
        public string Section { get; set; }
    }
}
