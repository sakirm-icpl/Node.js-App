using Courses.API.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APICourseCertificateAuthority
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserID { get; set; }
        
        public int? DesignationID { get; set; }
    }

    public class APICourseCertificateAuthorityDetails
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserID { get; set; }
        public int DesignationID { get; set; }
        [Required]
        public string AuthoristionName { get; set; }
        public string AuthoristionDesignationName { get; set; }
        public string CourseName { get; set; }
        public bool Active { get; set; }
        public int CourseCertificateId { get; set; }
        public string CreatedDate { get; set; }

    }

    public class CourseCertificateAuthority : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserID { get; set; }
        public int? DesignationID { get; set; }
    }

    public class APICourseCertificateExport
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AuthoristionName { get; set; }
        public string AuthoristionDesignationName { get; set; }
        public string CourseName { get; set; }
        public string CreatedDate { get; set; }

    }
}

