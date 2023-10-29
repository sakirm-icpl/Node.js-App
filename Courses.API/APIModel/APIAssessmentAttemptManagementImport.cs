using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APIAssessmentAttemptManagementImport
    {
        public string UserId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public int ModuleID { get; set; }
        public string AdditionalAttempts { get; set; }
        public int? AssessmentId { get; set; }
        public int? CourseId { get; set; }
        public string ErrMessage { get; set; }
    }
    public class APIAssessmentAttemptManagementPath
    {
        public string Path { get; set; }
    }
}

