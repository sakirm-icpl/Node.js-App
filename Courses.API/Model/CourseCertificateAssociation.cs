using System;

namespace Courses.API.Model
{
    public class CourseCertificateAssociation
    {
        public int Id { get; set; }

        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string CertificateImageName { get; set; }

        public DateTime Date { get; set; }
    }
    public class APICourseCertificateTypeHead
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
