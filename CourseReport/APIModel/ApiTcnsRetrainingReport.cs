using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class ApiExportTcnsRetrainingReport
    {
        public string UserId { get; set; }
        public string CourseCode { get; set; }
        public string UserName { get; set; }
        public string CourseTitle { get; set; }
        public string CourseStartDate { get; set; }
        public string CourseCompletionDate { get; set; }
        public string CourseStatus { get; set; }
        public string RetrainingDate { get; set; }
        public string UserStatus { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string FunctionCode { get; set; }
        public string Group { get; set; }
        public string Region { get; set; }
        public string Score { get; set; }
    }

    public class APITcnsRetrainingReport
    {
        [Required]
        public int CourseId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ExportAs { get; set; }
    }
}
