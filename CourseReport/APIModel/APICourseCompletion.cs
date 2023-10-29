using CourseReport.API.Validation;
using System.ComponentModel.DataAnnotations;
using static CourseReport.API.Helper.MetaData.Record;

namespace CourseReport.API.APIModel
{
    public class APICourseCompletion : BaseReportModel
    {
        [Required]
        public int CourseId { get; set; }
        public string search { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { Filter.InProgress, Filter.Completed, Filter.NotStarted })]
        public string status { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { Filter.Active, Filter.Inactive, Filter.nullval })]
        public string UserStatus { get; set; }
        public int UserId { get; set; }
        public string ExportAs { get; set; }
    }

    public class APICourseCompletionDetails 
    {
        [Required]
        public int CourseId { get; set; }     
        public int UserId { get; set; }
    }
    public class APIDevelopementPlanDetails
    {
        [Required]
        public int DevelopementPlanId { get; set; }
        public int UserId { get; set; }
    }
    public class APIDevPlanCompletion : BaseReportModel
    {
        [Required]
        public int DevelopementPlanId { get; set; }
        public string search { get; set; }
       
        [CommonValidationAttribute(AllowValue = new string[] { Filter.InProgress, Filter.Completed, Filter.NotStarted })]
        public string status { get; set; }
        public int UserId { get; set; }
        public string ExportAs { get; set; }
    }
}
