using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APINodalCourse
    {
        public string OrgCode { get; set; }
        public int CourseId { get; set; }
    }
    public class APINodalCourses
    {
        public string OrgCode { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string SearchText { get; set; }
    }
    public class APITtGrCourses
    {
        [Required]
        public string OrgCode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        [Required]
        public string AccessKey { get; set; }
        public string Search { get; set; }
        public string SearchText { get; set; }
    }
    public class APINodalCoursesGroupAdmin
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string SearchText { get; set; }
    }
    public class APIGetVendor
    {
        [Required]
        public string Vendor_Type { get; set; }
      
    }
}
