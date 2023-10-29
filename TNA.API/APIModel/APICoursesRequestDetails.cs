using System;

namespace TNA.API.APIModel
{
    public class APICoursesRequestDetails
    {
        public int Id { get; set; }
        public int CourseRequestId { get; set; }
        public string Status { get; set; }
        public int StatusUpdatedBy { get; set; }
        public DateTime Date { get; set; }
        public string ReasonForRejection { get; set; }
        public int CourseID { get; set; }
    }

    public class APITNAPostAcceptReject
    {
        public int[] Id { get; set; }
        public int isAccept { get; set; }
    }
}
