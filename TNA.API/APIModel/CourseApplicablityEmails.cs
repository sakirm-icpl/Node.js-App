using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TNA.API.APIModel
{
    public class CourseApplicablityEmails
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string LM_EmailID { get; set; }
        public string LA_EmailID { get; set; }
        public string TrainingName { get; set; }
        public string CourseTitle { get; set; }
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
    }
}
