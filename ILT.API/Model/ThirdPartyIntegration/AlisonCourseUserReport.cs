using System;

namespace ILT.API.Model.ThirdPartyIntegration
{
    public class AlisonCourseUserReport
    {
            public int Id { get; set; }
            public int? AlisonCourseID { get; set; }
            public int? CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string Release_date { get; set; }
            public string fullname { get; set; }
            public string headline { get; set; }
            public string image { get; set; }
            public string coursename { get; set; }
            public string courselink { get; set; }
            public string coursestate { get; set; }
            public string coursevalue { get; set; }
            public DateTime firstaccess { get; set; }
            public DateTime lastaccess { get; set; }
            public string totaltimespent { get; set; }
            public string scores { get; set; }
            public string fullname_en { get; set; }
            public string shortName { get; set; }
            public DateTime CreatedDate { get; set; }
            public int CreatedBy { get; set; }
            public int UserID { get; set; }

    }
}
