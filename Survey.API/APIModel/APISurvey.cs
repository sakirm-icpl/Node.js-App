using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class APISurvey
    {

        public int Options { get; set; }
        public string Question { get; set; }
        public string ActiveQuestion { get; set; }
        public string ObjectiveQuestion { get; set; }
        public string EnterOption1 { get; set; }
        public string EnterOption2 { get; set; }
        public string EnterOption3 { get; set; }
        public string EnterOption4 { get; set; }
        public string EnterOption5 { get; set; }
        public string EnterOption6 { get; set; }
        public string EnterOption7 { get; set; }
        public string EnterOption8 { get; set; }
        public string EnterOption9 { get; set; }
        public string EnterOption10 { get; set; }

        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        public int Id { get; set; }

    }
    public class APISurveyFilePath
    {
        public string Path { get; set; }
    }

}
