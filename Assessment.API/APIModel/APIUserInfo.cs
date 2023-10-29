using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assessment.API.APIModel
{
    public class APIUserInfo
    {
        public int UserID { get; set; }
        public string? UserIDName { get; set; }
        public string? UserName { get; set; }
        public string? email { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? city { get; set; }
        public string? country { get; set; }

    }

    public class APIAlisonCourseUserReport
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Release_date { get; set; }
        public string? fullname { get; set; }
        public string? headline { get; set; }
        public string? image { get; set; }
        public string? coursename { get; set; }
        public string? courselink { get; set; }
        public string? coursestate { get; set; }
        public DateTime firstaccess { get; set; }
        public DateTime lastaccess { get; set; }
        public string? totaltimespent { get; set; }
        public string? scores { get; set; }
        public string? fullname_en { get; set; }
        public string? shortName { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? email { get; set; }
        public string? courseValue { get; set; }
        public string? courseStatus { get; set; }
        public DateTime enrollment_date { get; set; }
        public string? mobilenumber { get; set; }
        public string? UserIDName { get; set; }

    }
    public class GetReportRecordCount
    {
        public int TotalRecordCount { get; set; }
    }

    public class ApiAlisonResponse
    {
        public int StatusCode { get; set; }
        public Object? ResponseObject { get; set; }
        public string? Description { get; set; }
    }

    public class ConnectionInfo
    {
        public string? ConnectionString { get; set; }
        public string? ClientCode { get; set; }
    }

}
