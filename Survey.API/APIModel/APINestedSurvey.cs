using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APINestedSurveyQuestions
    {
        [Key]
        public int QuestionId { get; set; }
        public int LcmsId { get; set; }
        public string? Question { get; set; }
        public int? IsRoot { get; set; }
    }

    public class APINestedSurveyOptions
    {
        [Key]
        public int Id { get; set; }
        public int? QuestionId { get; set; }
        public int? LcmsId { get; set; }
        public string? OptionText { get; set; }
        public int? NextQuestionId { get; set; }
    }

    public class SurveyOptionNested
    {
        [Key]
        public int Id { get; set; }
        public int? OptionId { get; set; }
        public int? LcmsId { get; set; }
        public int? NextQuestionId { get; set; }
    }

    public class APINestedSurveyResultDetail
    {
        public int? Id { get; set; }
        public string? Section { get; set; }
        public int SurveyQuestionId { get; set; }
        public int SurveyOptionId { get; set; }
        public int? SurveyResultId { get; set; }
        public string? SubjectiveAnswer { get; set; }
    }

    public class APINestedSurveyResult
    {
        public int? Id { get; set; }
        public int SurveyId { get; set; }
        public string? SurveyResultStatus { get; set; }
        public int? UserId { get; set; } 
        public APINestedSurveyResultDetail[] apiNestedSurveyResultDetail { get; set; }
    }
}
