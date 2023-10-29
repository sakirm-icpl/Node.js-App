using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CourseApplicability.API.Helper
{
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
    public class APIHelper
    {
        public const string UserAPI = "UserAPI";
        public const string xAPIEndPoint = "xAPIEndPoint";
        public const string xAPIBasic = "xAPIBasic";

        public static async Task<HttpResponseMessage> CallGetAPI(string url, string token = null)

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
}