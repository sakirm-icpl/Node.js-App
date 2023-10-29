using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.Model
{
    public class ILTTeams
    {
    }
    public class meeting
    {
        public string id { get; set; }
        public int totalParticipantCount { get; set; }
        public string meetingStartDateTime { get; set; }
        public string meetingEndDateTime { get; set; }
    }
    public class BaseModel
    {
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

    }
    [Table("ILTSchedule", Schema = "Course")]
    public class ILTSchedule : BaseModel
    {
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public int CourseId { get; set; }
        public int? BatchId { get; set; }
        public int ModuleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public int PlaceID { get; set; }
        public string TrainerType { get; set; }
        public int? AcademyAgencyID { get; set; }
        public int? AcademyTrainerID { get; set; }
        public string AcademyTrainerName { get; set; }
        public string AgencyTrainerName { get; set; }
        public string TrainerDescription { get; set; }
        public string ScheduleType { get; set; }
        public string ReasonForCancellation { get; set; }
        public string EventLogo { get; set; }
        public float Cost { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        public string WebinarType { get; set; }
    }
    [Table("ZoomMeetingDetails", Schema = "Course")]
    public class ZoomMeetingDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string Join_url { get; set; }
        public string Start_url { get; set; }
        public string UniqueMeetingId { get; set; }
        public string Host_id { get; set; }
        public string Start_time { get; set; }
        public string Topic { get; set; }
        public string Uuid { get; set; }
        public string Timezone { get; set; }
        public string Duration { get; set; }
        public int UserWebinarId { get; set; }

    }
    [Table("TeamsScheduleDetails", Schema = "Course")]
    public class TeamsScheduleDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string MeetingId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string iCalUId { get; set; }
        public string? JoinUrl { get; set; }
        public int? UserWebinarId { get; set; }
    }
}
