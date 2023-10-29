//======================================
// <copyright file="EnumHelper.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Saml.API.Helper
{
    public class Api
    {
        public static async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }
        public static async Task<HttpResponseMessage> CallPostAPI(string url, string body)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                return response;
            }
        }
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

                var response = await client.GetAsync(url);

                return response;
            }

        }
        public static async Task<HttpResponseMessage> CallPostAPI(string url, JObject oJsonObject, string token = null)
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
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }
        public static async Task<HttpResponseMessage> CallPostAPICsl(string url, JObject oJsonObject, string token = null)
        {
            using (var client = new HttpClient())
            {

                if (token != null)
                {
                    token = token.Replace("Bearer  ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }
    }
}
