using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Assessment.API.Helper
{

    public class CourseType
    {
        public const string Elearning = "elearning";
        public const string Classroom = "Classroom";
        public const string vilt = "vilt";
        public const string Assessment = "Certification";
        public const string Assessment_module = "Assessment";
        public const string Blended = "Blended";

    }
    public class contentType
    {
        public const string image = "image";
        public const string video = "video";
        public const string youtube = "youtube";
        public const string feedback = "feedback";
        public const string zip = "application/x-zip-compressed";
        public const string pdf = "application/pdf";
        public const string nonscorm = "nonscorm";
        public const string externallink = "nonscorm";
        public const string faq = "faq";
        public const string Scorm = "Scorm";
        public const string document = "document";
        public const string SCORM2004 = "SCORM2004";
        public const string assessment = "assessment";
        public const string audio = "audio";
        public const string memo = "memo";
        public const string SCORM12 = "SCORM1.2";
        public const string survey = "survey";
        public const string assignment = "assignment";
        public const string Authoring = "Authoring";
    }

    public class TrainerType
    {
        public const string Internal = "Internal";
        public const string External = "External";
        public const string Consultant = "Consultant";
    }
    public class FileType
    {
        public const string h5p = "application/octet-stream";
        public const string Pdf = "Pdf";
        public const string png = "png";
        public const string mp3 = "mp3";
        public const string mp4 = "mp4";
        public const string Video = "video";
        public const string Audio = "audio";
        public const string Image = "image";
        public const string Thumbnail = "Thumbnail";
        public const string AppZip = "application/zip";
        public const string AppXZipFile = "application/x-zip";
        public const string AppXZip = "application/x-zip-compressed";
        public const string Document = "document";
        public const string Zip = "zip";
        public const string Scorm = "scorm";
        public const string Youtube = "youtube";
        public static string Objective = "objective";
        public static string Subjective = "subjective";
        public static readonly string[] Doc = { "msword", "officedocument", "ms-word", "ms-excel", "ms-powerpoint", "pdf" , "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/msword"

        };
    }
    public class CommonValidation
    {
        public const string H5P = "H5P";
        public const string Course = "Course";
        public const string Survey = "Survey";
        public const string Boss = "boss";
        public const string Mini = "mini";
        public const string Normal = "normal";
        public const string Internal = "Internal";
        public const string External = "External";
        public const string Consultant = "Consultant";
        public const string Other = "Other";
        public const string SCORM = "SCORM";
        public const string nonSCORM = "nonSCORM";
        public const string vr = "vr";
        public const string xAPI = "xAPI";
        public const string cmi5 = "cmi5";
        public const string Document = "Document";
        public const string Video = "Video";
        public const string Kpoint = "kpoint";
        public const string Audio = "Audio";
        public const string YouTube = "YouTube";
        public const string externalLink = "externalLink";
        public const string vilt = "vilt";
        public const string Classroom = "Classroom";
        public const string Assessment = "Assessment";
        public const string Feedback = "Feedback";
        public const string AR = "AR";
        public const string VR = "VR";
        public const string memo = "memo";
        public const string Authoring = "Microlearning";
        public const string Assignment = "Assignment";
        public const string Objective = "Objective";
        public const string ObjectiveFeedback = "objective"; // For allowed small letters
        public const string Image = "Image";
        public const string ImageText = "ImageText";
        public const string TextAudio = "TextAudio";
        public const string TextVideo = "TextVideo";
        public const string subjective = "subjective";
        public const string MultipleSelection = "MultipleSelection";
        public const string SingleSelection = "SingleSelection";
        public const string emoji = "emoji";
        public const string Simple = "Simple";
        public const string Difficult = "Difficult";
        public const string Tough = "Tough";
        public const string Zero = "0";
        public const string One = "1";
        public const string Two = "2";
        public const string Three = "3";
        public const string Four = "4";
        public const string Five = "5";
        public const string Six = "6";
        public const string Seven = "7";
        public const string Eight = "8";
        public const string Nine = "9";
        public const string Ten = "10";
    }
    public class ContentType
    {
        public const string ExternalLink = "externallink";
        public const string Faq = "faq";
        public const string Survey = "survey";
    }

    public class CourseProgressStatus
    {
        public const string Completed = "completed";
        public const string Inprogress = "inprogress";
        public const string NotStarted = "notstarted";
    }

    public class SortBy
    {
        public const string Recently = "recently";
        public const string toprated = "toprated";
        public const string New = "new";
        public const string AZ = "a-z";
        public const string ZA = "z-a";
        public const string expiringsoon = "expiringsoon";
    }
    public static class Configuration
    {
        public static string UpdateMainTable = "UpdateMainTable";
        public static string ApiKey = "api_key";
        public static string ApiSecret = "api_secret";
        public static string DataType = "data_type";
        public static string Json = "JSON";
        public static string ApiUrl = "Notificationapi_url";
        public static string HostId = "host_id";
        public static string Topic = "topic";
        public static string Type = "type";
        public static string StartTime = "start_time";
        public static string Duration = "duration";
        public static string Id = "id";
        public static string UserActivate = "Use Activted OR Deactivated";
        public static string ApplicationUrl = "Application_url";
        public static string RegardName = "Regard_Name";
        public static string SupportMail = "Support_Mail";
        public static string NotificationApi = "NotificationApi";
        public static string CourseNotification = "CourseNotification";
        public static string DiscussionBoardNotification = "DiscussionBoardNotification";
        public static string MasterApi = "MasterApi";
        public static string CourseCompletionNotification = "CourseCompletionNotification";
        public static string AuditApi = "AuditApi";

    }
    public static class TlsUrl
    {
        public static string NotificationAPost = "myCourseModule/";
        public static string DiscussionBoardPost = "conversation/";
        public static string MyTeamPost = "myteam";
        public static string AssEvaluation = "evaluation-management";
    }
    public class APIHelper
    {
        public const string UserAPI = "UserAPI";
        public const string xAPIEndPoint = "xAPIEndPoint";
        public const string xAPIBasic = "xAPIBasic";

        public static async Task<HttpResponseMessage> CallGetAPI(string url, string? token = null)

        {
            using (var client = new HttpClient())
            {

                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                var response = client.GetAsync(url).Result;
                return response;
            }
        }
    }
    public class APICMI5Helper
    {
        public const string UserAPI = "UserAPI";

        public const string cmi5EndPoint = "cmi5EndPoint";
        public const string fetch = "cmi5fetch";
        public const string token = "cmi5token";

        public static async Task<HttpResponseMessage> CallGetAPI(string url, string? token = null)

        {
            using (var client = new HttpClient())
            {

                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                var response = client.GetAsync(url).Result;
                return response;
            }
        }
    }
    public static class AttendaceStatus
    {
        public static string Attended = "ATTD";
        public static string Withdrew = "WDLNCOSTAPPL";
        public static string Absent = "NOSHOWNCOSTAPPL";
        public static string Waiver = "WAIVER";
    }

    public static class UserRoles
    {
        public static string BA = "BA";
        public static string GA = "GA";
        public static string LA = "LA";
        public static string AA = "AA";
        public static string CA = "CA";
    }
}
