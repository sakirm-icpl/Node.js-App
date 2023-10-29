using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Courses.API.Helper;

namespace Courses.API.Helper
{
    public class PushNotificationLogic
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PushNotificationLogic));
        public static async Task<bool> SendMessage()
        {
            string serverKey = "AAAAc8x-gQo:APA91bFzd-xJbk3JdzqyuC0F-yaQgeViGqijcINodUT-BrmVLjTvDy9M3nQEEdZPWTVOsOjQYV12ryy89op5QPGzrk1cuGo2sNeYuJXgpBF0GsreE-eFMPGI_xQBGSWUWPCbrGG_v3M1";
            bool returnflag = true;
            try
            {
                var result = "-1";
                var webAddr = "https://fcm.googleapis.com/fcm/send";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + serverKey);
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"to\": \"Your device token\",\"data\": {\"message\": \"This is a Firebase Cloud Messaging Topic Message!\",}}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                return returnflag;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return returnflag;
        }
        public static async Task<bool> SendPushNotificationMultiple(string[] deviceTokens, Notification notification)
        {
            bool returnflag = true;
            FirebaseNotificationModelResponce _FirebaseNotificationModelResponce = new FirebaseNotificationModelResponce();
            string title = notification.title;
            string body = notification.body;
            string CourseCode = notification.CourseCode;
            int CourseId = notification.CourseId;

            var messageInformation = new PushMessage()
            {
                notification = new Notification()
                {
                    title = title,
                    body = body,
                    CourseCode = CourseCode,
                    CourseId = CourseId
                },
                registration_ids = deviceTokens
            };
            //Object to JSON STRUCTURE => using Newtonsoft.Json;
            string jsonMessage = JsonConvert.SerializeObject(messageInformation);

            // Create request to Firebase API
            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            request.Headers.TryAddWithoutValidation("Authorization", "key =" + "AAAAc8x-gQo:APA91bFzd-xJbk3JdzqyuC0F-yaQgeViGqijcINodUT-BrmVLjTvDy9M3nQEEdZPWTVOsOjQYV12ryy89op5QPGzrk1cuGo2sNeYuJXgpBF0GsreE-eFMPGI_xQBGSWUWPCbrGG_v3M1");
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                result = await client.SendAsync(request);
                var username = await result.Content.ReadAsStringAsync();
                _FirebaseNotificationModelResponce = JsonConvert.DeserializeObject<FirebaseNotificationModelResponce>(username);
            }

            return returnflag;
        }



        public static async Task<bool> SendPushNotificationSingle(FirebaseNotificationModel firebaseModel)
        {
            bool returnflag = true;
            FirebaseNotificationModelResponce _FirebaseNotificationModelResponce = new FirebaseNotificationModelResponce();
            HttpRequestMessage httpRequest = null;
            HttpClient httpClient = null;

            var authorizationKey = string.Format("key={0}", "AAAAc8x-gQo:APA91bFzd-xJbk3JdzqyuC0F-yaQgeViGqijcINodUT-BrmVLjTvDy9M3nQEEdZPWTVOsOjQYV12ryy89op5QPGzrk1cuGo2sNeYuJXgpBF0GsreE-eFMPGI_xQBGSWUWPCbrGG_v3M1");
            var jsonBody = JsonConvert.SerializeObject(firebaseModel);

            try
            {
                httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");

                httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationKey);
                httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage result;
                using (var client = new HttpClient())
                {
                    result = await client.SendAsync(httpRequest);
                    var username = await result.Content.ReadAsStringAsync();
                    _FirebaseNotificationModelResponce = JsonConvert.DeserializeObject<FirebaseNotificationModelResponce>(username);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            finally
            {
                httpRequest.Dispose();
                httpClient.Dispose();

            }
            if (_FirebaseNotificationModelResponce.success == 1)
            {
                return returnflag;
            }
            else
            {
                return false;
            }
        }
    }

    public class FirebaseNotificationModelResponce
    {

        public string multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public results[] results { get; set; }
    }

    public class results
    {
        string message_id { get; set; }
    }


    public class FirebaseNotificationModel
    {
        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }

        [JsonProperty(PropertyName = "notification")]
        public Notification Notification { get; set; }
    }

    public class PushMessage
    {

        public string[] registration_ids { get; set; }
        public Notification notification { get; set; }

    }
    public class Notification
    {
        public string body { get; set; }
        public string title { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
    }
}
