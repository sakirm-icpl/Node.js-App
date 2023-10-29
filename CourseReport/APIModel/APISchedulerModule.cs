using CourseReport.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using static CourseReport.API.Helper.MetaData.Record;

namespace CourseReport.API.APIModel
{
    public class APISchedulerModule : BaseReportModel
    {
        [Required]
       [RestrictFutureDtValidationAttribute("ToDate")]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
        public int? RegionID { get; set; }
        public int UserID { get; set; }
        public string ExportAs { get; set; }

    }
}
