using System;
using System.Collections.Generic;

namespace Courses.API.APIModel
{
    public class ApiCourseCompletionDetails
    {
        public int Id { get; set; }
        public string CourseStatus { get; set; }
        public string Title { get; set; }
        public float TotalMarks { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string AssessmentResult { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string CertificateImageName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public string AuthorityName  { get; set; }
        public DateTime? CourseStartDate { get; set; }
        public string Grade { get; set; }
        public DateTime? CourseEndDate { get; set; }
        public string RollNumber { get; set; }
        public string CourseCode { get; set; }
        public string IssuedBy { get; set; }
        public string AuthorisedBy { get; set; }
        public string Area { get; set; }
        public string GroupName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int UsId { get; set; }
        public bool StatusFromImage { get; set; }
        
    }

    public class CourseCertificateData
    {
        public List<ApiCourseCompletionDetails> data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CourseCertificateDataFinal
    {
        public List<APICertificateDownlodResult> data { get; set; }
        public int TotalRecords { get; set; }
    }
    public class APICertificateDownlodResult
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string UserId { get; set; }
        public int UsId { get; set; }
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

    public class APIDevelopementPlanResult
    {
        public int DevPlanId { get; set; }
        public string DevelopementPlan { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string CompletionDate { get; set; }
      
        public string CertificateImageName { get; set; }
        public string EmployeeCode { get; set; }
       
    }
    public class CertificateBulk
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }

    }
    public class CertificateStatus
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
       
    }

}
