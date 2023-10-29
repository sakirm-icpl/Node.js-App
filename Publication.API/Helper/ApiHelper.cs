using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Publication.API.Helper;

namespace Publication.API.Helper
{
    public static class ApiHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ApiHelper));
        public static async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject, string token = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }
        public static async Task<HttpResponseMessage> CallGetAPI(string url, string token = null)
        {
            using (HttpClient client = new HttpClient())
            {

                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                HttpResponseMessage response = await client.GetAsync(url);
                return response;
            }

        }
        public static async Task<HttpResponseMessage> CallPutAPI(string url, JObject oJsonObject, string token = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }

        public static async Task<HttpResponseMessage> CallDeleteAPI(string url, string token = null)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    if (token != null)
                    {
                        token = token.Replace("Bearer ", "");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string apiUrl = url;

                    var response = await client.DeleteAsync(url);

                    return response;
                }
                catch (Exception ex)
                {
                     _logger.Error( Utilities.GetDetailedException(ex));
                }
                return null;

            }
        }

        public class APIHelper
        {
            public const string UserAPI = "UserAPI";
            public const string xAPIEndPoint = "xAPIEndPoint";
            public const string xAPIBasic = "xAPIBasic";
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
            public static async Task<HttpResponseMessage> CallGetAPI(string url, string token = null)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
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
}
