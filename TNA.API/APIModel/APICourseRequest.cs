using System;

namespace TNA.API.APIModel
{
    public class APICourseRequest
    {
        public int Id { get; set; }
        public int CourseRequestID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public int StatusUpdatedBy { get; set; }
        public string StatusApprovedBy { get; set; }
        public string RoleName { get; set; }
        public string ReasonForRejection { get; set; }
        public DateTime Date { get; set; }
        public int? IsApproved { get; set; }
        public string Department { get; set; }
        public bool IsRequestSendToLM { get; set; }
        public bool IsRequestSend { get; set; }
        public string OtherCourseName { get; set; }
        public string OtherCourseDescription { get; set; }
        public int Role { get; set; }
        public bool IsNominate { get; set; }
        public string CourseDescription { get; set; }
        public decimal? Rating { get; set; }
        public string Image { get; set; }
        public bool IsCatalogue { get; set; }
        public string NewStatus { get; set; }
    }
}
