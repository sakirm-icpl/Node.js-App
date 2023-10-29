namespace TNA.API.Model
{
    public class CourseScheduleEnrollmentRequestDetails : BaseModel
    {
        public int Id { get; set; }
        public int CourseScheduleEnrollmentRequestID { get; set; }
        public string Status { get; set; }
        public int StatusUpdatedBy { get; set; }
        public string Comment { get; set; }
        public int ApprovedLevel { get; set; }
        public bool IsNominated { get; set; }

    }
}
