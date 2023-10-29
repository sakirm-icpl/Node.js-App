using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Feedback.API.Common
{
    public class BigBlueButton
    {
        [XmlElement(ElementName = "returncode")]
        public string? Returncode { get; set; }
        [XmlElement(ElementName = "meetingID")]
        public string? MeetingID { get; set; }
        [XmlElement(ElementName = "internalMeetingID")]
        public string? InternalMeetingID { get; set; }
        [XmlElement(ElementName = "parentMeetingID")]
        public string? ParentMeetingID { get; set; }
        [XmlElement(ElementName = "attendeePW")]
        public string? AttendeePW { get; set; }
        [XmlElement(ElementName = "moderatorPW")]
        public string? ModeratorPW { get; set; }
        [XmlElement(ElementName = "createTime")]
        public string? CreateTime { get; set; }
        [XmlElement(ElementName = "voiceBridge")]
        public string? VoiceBridge { get; set; }
        [XmlElement(ElementName = "dialNumber")]
        public string? DialNumber { get; set; }
        [XmlElement(ElementName = "createDate")]
        public string? CreateDate { get; set; }
        [XmlElement(ElementName = "hasUserJoined")]
        public string? HasUserJoined { get; set; }
        [XmlElement(ElementName = "duration")]
        public string? Duration { get; set; }
        [XmlElement(ElementName = "hasBeenForciblyEnded")]
        public string? HasBeenForciblyEnded { get; set; }
        [XmlElement(ElementName = "messageKey")]
        public string? MessageKey { get; set; }
        [XmlElement(ElementName = "message")]
        public string? Message { get; set; }
    }

    public class BBBMeeting
    {
        public bool Record { get; set; }
        [MaxLength(25)]
        public string? MeetingID { get; set; }
        [MaxLength(25)]
        public string? RecordID { get; set; }
        [MaxLength(25)]
        public string? MeetingName { get; set; }
        [MaxLength(1000)]
        public string? CreateMeetingURL { get; set; }
        public string? MeetingTime { get; set; }
        public bool AutoStartRecording { get; set; }
        public bool AllowStartStopRecording { get; set; }

        public int Duration { get; set; }
        public DateTime MeetingDate { get; set; }

        public bool MeetingCancelled { get; set; } = false;
        public bool IsMeetingCreated { get; set; } = false;
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int CourseId { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }

    }

    public class BBMeetingDetails
    {
        public string? MeetingID { get; set; }
        public string? MeetingName { get; set; }
        public string? MeetingTime { get; set; }
        public int Duration { get; set; }
        public DateTime MeetingDate { get; set; }
        public int CourseId { get; set; }
        public bool MeetingExpired { get; set; }

    }
}
