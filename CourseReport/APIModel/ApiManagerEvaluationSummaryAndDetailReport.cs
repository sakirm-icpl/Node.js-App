using CourseReport.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class ApiManagerEvaluationSummaryAndDetailReport
    {
        public int CourseId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int? UserID { get; set; }
        public string Search { get; set; }
        public string Status { get; set; }
        public string ExportAs { get; set; }
    }
    public class ManagerEvaluationSummaryAndDetailReport
    {
        public string managername { get; set; }
        public int reportsToCount { get; set; }
        public int completion { get; set; }
        public int pending { get; set; }
        public int percentage { get; set; }
    }
}
