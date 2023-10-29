using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.APIModel
{
    public class APINodalCourseRequests
    {
        public int Id { get; set; }
        public DateTime RequestedDate { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string FHName { get; set; }
        public string AadharNumber { get; set; }
        public bool? IsApprovedByNodal { get; set; }
        public string RequestStatus { get; set; }
        public bool IsPaymentDone { get; set; }
        public string PaymentStatus { get; set; }
        public string Reason { get; set; }
        public string CourseStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsAccepted { get; set; }
        public string OrganizationDetails { get; set; }
        public string OrganizationID { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APINodalCourseRequestGroupDetails
    {
        public int GroupId { get; set; }
        public string GroupCode { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int Requests { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }
    public class APINodalCourseRequestUserDetails
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupCode { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public DateTime RequestedDate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string FHName { get; set; }
        public string AadharNumber { get; set; }
        public bool? IsApprovedByNodal { get; set; }
        public string RequestStatus { get; set; }
        public bool IsPaymentDone { get; set; }
        public string PaymentStatus { get; set; }
        public string Reason { get; set; }
        public string CourseStatus { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsAccepted { get; set; }
        public string OrganizationID { get; set; }
        public string AadhaarPath { get; set; }
    }
    public class APINodalRequest
    {
        public int Id { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsApprovedByNodal { get; set; }
        public string Reason { get; set; }
    }
    public class APINodalRequestResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public List<APINodalRequestList> aPINodalRequestList { get; set; }
    }
    public class APINodalRequestList
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; }
        public int GA_Id { get; set; }
        public string GA_Name { get; set; }
        public string GA_EmailId { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public bool IsApprove { get; set; }
        public string Status { get; set; }
        public string PaymentUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string OrgCode { get; set; }
        public string RequestType { get; set; }
        public string GroupCode { get; set; }
        public string Reason { get; set; }
    }
    public class APICourseRequestDelete
    {
        public int Id { get; set; }
    }
    public class APISelfCourseRequest
    {
        public int CourseId { get; set; }
    }
    public class APISelfCourseRequestEmail
    {
        public string CourseTitle { get; set; }
        public int UserMasterId { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public string OrgCode { get; set; }
    }
    public static class NodalCourseStatus
    {
        public static readonly string Inprogress = "inprogress";
        public static readonly string Completed = "completed";
    }
    public class APIGroupCourseCompletion
    {
        public int CourseId { get; set; }
        public int GroupId { get; set; }
        public int UserIds { get; set; }
    }
    public class APINodalUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string ConfigurationColumn12 { get; set; }
        public string OrganizationCode { get; set; }
        public string AadharNumber { get; set; }
        public string FHName { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Response { get; set; }
        public int? ConfigurationColumn12Id { get; set; }
    }
    public class APINodalUserDetailsEmail
    {
        public int NodalUserId { get; set; }
        public string NodalUserName { get; set; }
        public string NodalEmailID { get; set; }
        public string NodalMobileNumber { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public string AirPort { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string OrgCode { get; set; }
        public string CourseTitle { get; set; }
    }
}
