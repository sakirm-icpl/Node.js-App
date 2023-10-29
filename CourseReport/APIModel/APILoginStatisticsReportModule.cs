using CourseReport.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

using static CourseReport.API.Helper.MetaData.Record;

namespace CourseReport.API.APIModel
{
    public class APILoginStatisticsReportModule : BaseReportModel
    {
        [Required]
        [RestrictFutureDtValidationAttribute("ToDate")]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { Filter.Web, Filter.Mobile, Filter.Both })]
        public string LoginFrom { get; set; }

        public string ExportAs { get; set; }


    }
}
