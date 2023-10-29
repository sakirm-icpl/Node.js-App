using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseApplicability.API.APIModel
{
    public class APITotalRequest
    {
        public List<APIUserRequestedCourses> data { get; set; }
        public int TotalRecords { get; set; }

    }

    public class APIUserRequestedCourses
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }

    }

    public class GetSupervisorData
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string UserId { get; set; }
        public string searchText { get; set; }
        public string columnName { get; set; }


    }


}