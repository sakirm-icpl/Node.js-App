using System.Xml.Serialization;

namespace Courses.API.Common
{

    [XmlRoot(ElementName = "customdata")]
	public class Customdata
	{
		[XmlElement(ElementName = "bbb_preferred_camera_profile")]
		public string Bbb_preferred_camera_profile { get; set; }
	}

	[XmlRoot(ElementName = "attendee")]
	public class Attendee
	{ 
		[XmlElement(ElementName = "userID")]
		public string UserID { get; set; }
		[XmlElement(ElementName = "fullName")]
		public string FullName { get; set; }
		[XmlElement(ElementName = "role")]
		public string Role { get; set; }
		[XmlElement(ElementName = "isPresenter")]
		public string IsPresenter { get; set; }
		[XmlElement(ElementName = "isListeningOnly")]
		public string IsListeningOnly { get; set; }
		[XmlElement(ElementName = "hasJoinedVoice")]
		public string HasJoinedVoice { get; set; }
		[XmlElement(ElementName = "hasVideo")]
		public string HasVideo { get; set; }
		[XmlElement(ElementName = "clientType")]
		public string ClientType { get; set; }
		[XmlElement(ElementName = "customdata")]
		public Customdata Customdata { get; set; }
	}

	[XmlRoot(ElementName = "attendees")]
	public class Attendees
	{
		[XmlElement(ElementName = "attendee")]
		public Attendee Attendee { get; set; }
	}

	[XmlRoot(ElementName = "metadata")]
	public class BBBMetadata
	{
		[XmlElement(ElementName = "bn-userid")]
		public string Bnuserid { get; set; }
		[XmlElement(ElementName = "bn-meetingid")]
		public string Bnmeetingid { get; set; }
		[XmlElement(ElementName = "bn-priority")]
		public string Bnpriority { get; set; }
	}

	[XmlRoot(ElementName = "response")]
	public class BBBMeetingInfoResponse
	{
		[XmlElement(ElementName = "returncode")]
		public string Returncode { get; set; }
		[XmlElement(ElementName = "meetingName")]
		public string MeetingName { get; set; }
		[XmlElement(ElementName = "meetingID")]
		public string MeetingID { get; set; }
		[XmlElement(ElementName = "internalMeetingID")]
		public string InternalMeetingID { get; set; }
		[XmlElement(ElementName = "createTime")]
		public string CreateTime { get; set; }
		[XmlElement(ElementName = "createDate")]
		public string CreateDate { get; set; }
		[XmlElement(ElementName = "voiceBridge")]
		public string VoiceBridge { get; set; }
		[XmlElement(ElementName = "dialNumber")]
		public string DialNumber { get; set; }
		[XmlElement(ElementName = "attendeePW")]
		public string AttendeePW { get; set; }
		[XmlElement(ElementName = "moderatorPW")]
		public string ModeratorPW { get; set; }
		[XmlElement(ElementName = "running")]
		public string Running { get; set; }
		[XmlElement(ElementName = "duration")]
		public string Duration { get; set; }
		[XmlElement(ElementName = "hasUserJoined")]
		public string HasUserJoined { get; set; }
		[XmlElement(ElementName = "recording")]
		public string Recording { get; set; }
		[XmlElement(ElementName = "hasBeenForciblyEnded")]
		public string HasBeenForciblyEnded { get; set; }
		[XmlElement(ElementName = "startTime")]
		public string StartTime { get; set; }
		[XmlElement(ElementName = "endTime")]
		public string EndTime { get; set; }
		[XmlElement(ElementName = "participantCount")]
		public string ParticipantCount { get; set; }
		[XmlElement(ElementName = "listenerCount")]
		public string ListenerCount { get; set; }
		[XmlElement(ElementName = "voiceParticipantCount")]
		public string VoiceParticipantCount { get; set; }
		[XmlElement(ElementName = "videoCount")]
		public string VideoCount { get; set; }
		[XmlElement(ElementName = "maxUsers")]
		public string MaxUsers { get; set; }
		[XmlElement(ElementName = "moderatorCount")]
		public string ModeratorCount { get; set; }
		[XmlElement(ElementName = "attendees")]
		public Attendees Attendees { get; set; }
		[XmlElement(ElementName = "metadata")]
		public Metadata Metadata { get; set; }
		[XmlElement(ElementName = "isBreakout")]
		public string IsBreakout { get; set; }
	}
}
