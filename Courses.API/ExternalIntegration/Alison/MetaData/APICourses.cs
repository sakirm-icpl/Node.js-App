using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalIntegration.MetaData
{
    public class APIAlisonCoursesDetails
    {
        public int id { get; set; }
        public int category { get; set; }
        public string categoryname { get; set; }
        public string categories { get; set; }
        public string release_date { get; set; }
        public string fullname { get; set; }
        public string headline { get; set; }
        public string image { get; set; }
        public string coursename { get; set; }
        public string courselink { get; set; }
        public string coursestate { get; set; }
        public string coursevalue { get; set; }
        public string firstaccess { get; set; }
        public string lastaccess { get; set; }
        public string totaltimespent { get; set; }
        public string scores { get; set; }
        public string fullname_en { get; set; }
        public string? fullname_ar { get; set; }
        public string fullname_fr { get; set; }
        public string summary_en { get; set; }
        public string? summary_ar { get; set; }
        public string summary_fr { get; set; }

        public string shortName { get; set; }
        public string courseStatus { get; set; }

        public string enrollment_date { get; set; }

    }

    public class APIAlisonCourses
    {
        public string coursename { get; set; }
        public string courselink { get; set; }
    }

    public class APIUserAlisonCourseDetails
    {
        public int UserID { get; set; }
        public string UserIDName { get; set; }
        public string emailId { get; set; }
        public List<APIAlisonCoursesDetails> aPICoursesDetails { get; set; }

    }

    public class APIAlisonCoursesApplicable
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class APIAlisonCategory
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
    }

    public class APIAlisonCourseCheck
    {
        public bool IsAlisonCourse { get; set; }
        public string CourseLink { get; set; }
    }

    public class APIAlisonProgressStatusCountData
    {
        public int InprogressCount { get; set; }
        public int CompletedCount { get; set; }
        public int NotStartedCount { get; set; }
        public double InprogressCountPercentage { get; set; }
        public double CompletedCountPercentage { get; set; }
        public double NotStartedCountPercentage { get; set; }
    }

}
