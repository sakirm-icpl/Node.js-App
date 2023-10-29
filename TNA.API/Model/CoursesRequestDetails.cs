using System;

namespace TNA.API.Model
{
    public class CoursesRequestDetails : BaseModel
    {
        public int Id { get; set; }
        public int CourseRequestId { get; set; }
        public string Status { get; set; }
        public int StatusUpdatedBy { get; set; }
        public DateTime Date { get; set; }
        public string ReasonForRejection { get; set; }
        public int CourseID { get; set; }
        public bool IsNominate { get; set; }
        public int Role { get; set; }
    }
}
