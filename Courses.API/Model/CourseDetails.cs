using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class Course_Details 
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public int? CourseOwnerID { get; set; }
        public int? CourseInstructorID { get; set; }

        public string? CourseType { get; set; }
        public bool? MobileNativeDeeplink { get; set; }
        public bool? IsRefresherMandatory { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
