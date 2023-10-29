using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class APIUserLearningReportModule
    {
        [Required]
        public int UserId { get; set; }

        public int? CourseId { get; set; }
        [Required]
        public int PageSize { get; set; }
        [Required]
        public int StartIndex { get; set; }
        public String Search { get; set; }
        public string ExportAs { get; set; }
    }
}
