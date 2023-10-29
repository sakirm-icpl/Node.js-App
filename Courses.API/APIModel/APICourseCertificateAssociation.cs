using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APICourseCertificateAssociation
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string CertificateImageName { get; set; }
        public DateTime Date { get; set; }
     
    }
}
