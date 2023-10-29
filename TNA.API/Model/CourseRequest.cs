using System;


namespace TNA.API.Model
{
    public class CourseRequest : BaseModel
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string OtherCourseName { get; set; }
        public string OtherCourseDescription { get; set; }
        public bool IsAccessGiven { get; set; }
        public bool IsRequestSendToBUHead { get; set; }
        public bool IsRequestSendToLM { get; set; }
        public bool IsRequestSendToHR { get; set; }
        public bool IsRequestSendFromHRTOBU { get; set; }
        public bool IsRequestSendFromTA { get; set; }
        public string NewStatus { get; set; }
        public int TNAYear { get; set; }
    }
}
