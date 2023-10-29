using CourseReport.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APIProcessEvaluationAuditModule :  BaseReportModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string search { get; set; }
        public string ExportAs { get; set; }
        public string AuditorName { get; set; }
        public string StoreName { get; set; }
        public string RestaurantManagerName { get; set; }


    }
    public class ApiProcessEvaluationAuditReport
    {
        public int ID { get; set; }
        public string Month { get; set; }
        public string StoreName { get; set; }
        public int TotalRecordCount { get; set; }
        public string AuditorName { get; set; }
        public int Score { get; set; }
        public string EvaluationDate { get; set; }


    }

    public class ProcessEvaluationAuditDetailsWithImagePaths
    {
        public int EvalResultID { get; set; }
        public string QuestionText { get; set; }
        public string OptionText { get; set; }
        public string FilePath1 { get; set; }
        public string FilePath2 { get; set; }
        public string FilePath3 { get; set; }
        public string FilePath4 { get; set; }
        public string FilePath5 { get; set; }
        public string FilePath6 { get; set; }
        public string FilePath7 { get; set; }
        public string FilePath8 { get; set; }
        public string FilePath9 { get; set; }
        public string FilePath10 { get; set; }
        public string ImprovementAnswer { get; set; }

    }
}
