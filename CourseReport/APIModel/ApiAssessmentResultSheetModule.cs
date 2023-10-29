using CourseReport.API.Validation;
using System.ComponentModel.DataAnnotations;
using static CourseReport.API.Helper.MetaData.Record;



namespace CourseReport.API.APIModel
{
    public class APIAssessmentResultSheetModule : BaseReportModel
    {
        [Required]
        public int CourseId { get; set; }
   
        public int? UserId { get; set; }
        public string Search { get; set; }
        public string ModuleId { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { Filter.Passed, Filter.Failed, Filter.All })]
        public string Status { get; set; }
        public int? LoginUserId { get; set; }
        public string Region { get; set; }
        public string ExportAs { get; set; }
    }

    public class APIRecommondedTraining
    {
        [Required]
        public string Year { get; set; }
        public int? UserId { get; set; }       
        public string ExportAs { get; set; }
    }
    public class APICourseDashboardData : BaseReportModel
    {
        public int? UserId { get; set; }
        public int? CompletionPer { get; set; }
    }

    public class APICourseDashboardDataValues
    {
        public object data { get; set; }
    }
}
