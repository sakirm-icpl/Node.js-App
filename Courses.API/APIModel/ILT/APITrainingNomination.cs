using System.Collections.Generic;

namespace Courses.API.APIModel.ILT
{
    public class APITrainingNomination
    {
        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public int? ReferenceRequestID { get; set; }
        public string RequestCode { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Status { get; set; }
        public bool? IsPresent { get; set; }
        public int? ModuleID { get; set; }
        public int? CourseID { get; set; }
        public string TrainingRequestStatus { get; set; }
        public string AttendanceStatus { get; set; }
        public bool? NoticePeriod { get; set; }
        public string OverAllStatus { get; set; }
        public string AttendanceDate { get; set; }
    }

    public class APITrainingNominationWaiting
    {
        public int ID { get; set; }
        public int ScheduleID { get; set; }       
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }             
        public int? ModuleID { get; set; }
        public int? CourseID { get; set; }
        public string TrainingRequestStatus { get; set; }
        public string RequestDate { get; set; }

    }
    public class APINominationWaitingData
    {
        public int TotalRecords { get; set; }
        public List<APITrainingNominationWaiting> waitingdata { get; set; }
    }
    }
