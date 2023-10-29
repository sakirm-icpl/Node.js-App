using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APIUserwiseCourseStatusReport
    {
        public int? UserCategoryId { get; set; }
        public int? SalesOfficeId { get; set; }
        public int? ControllingOfficeId { get; set; }
        public string ExportAs { get; set; }
    }

    public class APIUserwiseCourseStatusReportResult
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ReportsToUserId { get; set; }
        public string ReportsToUserName { get; set; }
        public string UserCategory { get; set; }
        public string IsActive { get; set; }
        public string SalesOfficeName { get; set; }
        public string SalesArea { get; set; }
        public string ControllingOffice { get; set; }
        public string State { get; set; }
        public int Inprogress { get; set; }
        public int Completed { get; set; }
        public int Applicable { get; set; }
        public string LastLoggedInDate { get; set; }
    }
}
