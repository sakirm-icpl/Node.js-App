using System;
using System.Collections.Generic;
namespace CourseApplicability.API.APIModel
{
    public class APICertificateTemplatesResult
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string CompletionDate { get; set; }
        public float Percentage { get; set; }
        public string AssessmentResult { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string CertificateImageName { get; set; }
        public string EmployeeCode { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public string AuthorityName { get; set; }
        public string CourseStartDate { get; set; }
        public string CourseEndDate { get; set; }
        public string Grade { get; set; }
        public string RollNumber { get; set; }
        public string CourseCode { get; set; }
        public string SearialNumber { get; set; }
        public string CertificatePath { get; set; }
        public string CertificateFile { get; set; }
        public string IssuedBy { get; set; }
        public string AuthorisedBy { get; set; }
        public string Area { get; set; }
        public string GroupName { get; set; }

    }

}