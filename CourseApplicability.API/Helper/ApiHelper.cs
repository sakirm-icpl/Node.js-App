using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json.Serialization;
using System.Threading;

namespace CourseApplicability.API.Helper
{
    public static class ApiHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ApiHelper));
      
        public static async Task<HttpResponseMessage> CallPostAPI(string url, string body, string token = null)
        {
            try
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
                    var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
    }
}

