using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Courses.API.Common
{
    public class HttpHelpers
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(HttpHelpers));

        public async Task<string> MakaHTTPGetCallAsync(string URL)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage res = await client.GetAsync(URL))
            {
                using (HttpContent content = res.Content)
                {
                    return await content.ReadAsStringAsync();
                }
            }
        }

        public async Task<HttpResponseMessage> MakaHTTPPostCallAsync(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }
    }
}
