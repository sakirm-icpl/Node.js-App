using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class AssignmentDetailsRejected : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string CourseCode { get; set; }
        [MaxLength(250)]
        public string AssignmentName { get; set; }
        [MaxLength(250)]
        public string UserId { get; set; }
        [MaxLength(250)]
        public string Status { get; set; }
        [MaxLength(250)]
        public string Remark { get; set; }
        [MaxLength(250)]     
        public string ErrorMessage { get; set; }
    }
}
