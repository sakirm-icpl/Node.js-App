using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CourseReport.API.Helper.MetaData
{

    public class APIHelper
    {
        public const string UserAPI = "UserAPI";
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
                HttpResponseMessage response = client.GetAsync(url).Result;
                return response;
            }
        }
    }
    public class Title
    {
        [MaxLength(150)]
        public string Name { get; set; }
    }

}
