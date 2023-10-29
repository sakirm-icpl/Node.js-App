using CourseReport.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using static CourseReport.API.Helper.MetaData.Record;


namespace CourseReport.API.APIModel
{
    public class APICourseWiseCompletionModule : BaseReportModel
    {
        [Required]
        public int? CourseId { get; set; }
        //[Required]
        //[RestrictFutureDtValidationAttribute("ToDate")]
        public DateTime? FromDate { get; set; }
        //[Required]
        public DateTime? ToDate { get; set; }
        public string Search { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { Filter.Active, Filter.Inactive, Filter.nullval })]
        public string UserStatus { get; set; }
        public int UserId { get; set; }
        public int? moduleId { get; set; }
        public string ExportAs { get; set; }
        public int? Id { get; set; }
    }
    public class APIReportDownload 
    {
        [Required]       
        public int Id { get; set; }
    }
}
