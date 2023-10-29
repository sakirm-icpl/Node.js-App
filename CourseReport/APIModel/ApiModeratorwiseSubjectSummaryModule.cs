using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APIModeratorwiseSubjectSummaryModule: BaseReportModel
    {
        public string  Search { get; set; }
    }
    public class ApiModeratorwiseSubjectSummaryReport
    {
        public string Moderator { get; set; }
        public string Subject { get; set; }
        public int SessionToBeCreated { get; set; }
        public int NumberOfModulesToBeCreated { get; set; }
        public int NumberOfModulesCreated { get; set; }
        public int Percentage { get; set; }
        public int TotalRecordCount { get; set; }

    }
}
