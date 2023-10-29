using System.Xml.Serialization;

namespace Courses.API.Common
{
    [XmlRoot(ElementName = "response")]
    public class BigBlueButtonResponse
    {
        [XmlElement(ElementName = "returncode")]
        public string Returncode { get; set; }
        [XmlElement(ElementName = "meetingID")]
        public string MeetingID { get; set; }
        [XmlElement(ElementName = "internalMeetingID")]
        public string InternalMeetingID { get; set; }
        [XmlElement(ElementName = "parentMeetingID")]
        public string ParentMeetingID { get; set; }
        [XmlElement(ElementName = "attendeePW")]
        public string AttendeePW { get; set; }
        [XmlElement(ElementName = "moderatorPW")]
        public string ModeratorPW { get; set; }
        [XmlElement(ElementName = "createTime")]
        public string CreateTime { get; set; }
        [XmlElement(ElementName = "voiceBridge")]
        public string VoiceBridge { get; set; }
        [XmlElement(ElementName = "dialNumber")]
        public string DialNumber { get; set; }
        [XmlElement(ElementName = "createDate")]
        public string CreateDate { get; set; }
        [XmlElement(ElementName = "hasUserJoined")]
        public string HasUserJoined { get; set; }
        [XmlElement(ElementName = "duration")]
        public string Duration { get; set; }
        [XmlElement(ElementName = "hasBeenForciblyEnded")]
        public string HasBeenForciblyEnded { get; set; }
        [XmlElement(ElementName = "messageKey")]
        public string MessageKey { get; set; }
        
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [XmlElement(ElementName = "running")]
        public string running { get; set; }
    }

}
