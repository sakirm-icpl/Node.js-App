using System;

namespace Courses.API.APIModel.TNA
{
    public class APIScheduleEnrollmentRequest
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int StatusUpdatedBy { get; set; }
        public string StatusApprovedBy { get; set; }
        public string Comment { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
    }
}
