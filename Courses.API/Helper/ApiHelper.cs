using Courses.API.APIModel;
using Courses.API.Model.ILT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Courses.API.APIModel.ILT.APIGoToMeeting;
using Courses.API.Helper;
using log4net;
using Courses.API.APIModel.ILT;
using Courses.API.ExternalIntegration.EdCast;
using Courses.API.Model;
using System.Collections.Generic;
using Courses.API.Model.ThirdPartyIntegration;
using Microsoft.Extensions.Configuration;
using Courses.API.APIModel.ThirdPartyIntegration;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json.Serialization;
using System.Threading;

namespace Courses.API.Helper
{
    public static class ApiHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ApiHelper));
        public static async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject, string token = null)
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

                    var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }

        }
        public static async Task<HttpResponseMessage> CallPatchAPIForTeams(string url, JObject oJsonObject, string token = null)
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

                    var response = await client.PatchAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }

        }

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

        public static async Task<HttpResponseMessage> CallPutAPI(string url, JObject oJsonObject, string token = null)
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

                    var response = await client.PutAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;

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
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }

        }


        #region GoToMeeting
        public static HttpResponseMessage CallgotoPostAPI(string url, string jsonObject, string token = null)
        {
            using (var client = new HttpClient())
            {

                if (token != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                var response = client.PostAsync(url, new StringContent(jsonObject, Encoding.UTF8, "application/json")).Result;
                return response;
            }

        }
        public static HttpResponseMessage CallgotoGetAPI(string url, string token = null)
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
        public static async Task<string> DirectLogin(ILTOnlineSetting iltonlineSetting)
        {
            try
            {
                string url = "https://api.getgo.com/oauth/access_token?grant_type=password&user_id=" + iltonlineSetting.UserID + "&password=" + iltonlineSetting.Password + "&client_id=" + iltonlineSetting.ClientID + "";

                //  string url = "https://api.getgo.com/oauth/access_token?grant_type=password&user_id=lms@nlicgulf.com&password=Nlic$2018&client_id=DFZP60TqjGSEXOLw5dDDGEBoWxOf7Vw9";
                HttpResponseMessage Response = await ApiHelper.CallGetAPI(url);
                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    TokenResponce ConfigurableParameters = JsonConvert.DeserializeObject<TokenResponce>(result);

                    return ConfigurableParameters.access_token;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return null;
        }
        public static MeetingResponce CreateMeeting(string access_token, JObject inputJson)
        {
            try
            {
                string url = "https://api.getgo.com/G2M/rest/meetings";
                HttpResponseMessage Response = CallgotoPostAPI(url, JsonConvert.SerializeObject(inputJson), access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    MeetingResponce[] ConfigurableParameters = JsonConvert.DeserializeObject<MeetingResponce[]>(result);

                    return ConfigurableParameters[0];
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public static string StartMeeting(string access_token, string StartMeetingURL)
        {

            string URL = StartMeetingURL;
            try
            {
                HttpResponseMessage Response = CallgotoGetAPI(URL, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    TokenResponce ConfigurableParameters = JsonConvert.DeserializeObject<TokenResponce>(result);

                    return ConfigurableParameters.hostURL;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public static async Task<MeetingAttendees[]> GetAttendeesByMeeting(string access_token, string StartMeetingURL)
        {
            try
            {
                string URL = StartMeetingURL;
                HttpResponseMessage Response = await CallGetAPI(URL, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    MeetingAttendees[] ConfigurableParameters = JsonConvert.DeserializeObject<MeetingAttendees[]>(result);

                    return ConfigurableParameters;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        #endregion


        #region ZoomMeeting
        public async static Task<APIZoomDetailsToken> GetTokenForZoomMeeting(JObject inputJson, string access_token = null)
        {
            try
            {
                string url = "https://next-api.stoplight.io/oauth_token_capture";

                HttpResponseMessage Response = await ApiHelper.CallAPI(url, inputJson, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    APIZoomDetailsToken ConfigurableParameters = JsonConvert.DeserializeObject<APIZoomDetailsToken>(result);

                    return ConfigurableParameters;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async static Task<APIZoomMeetingResponce> GetMeetingResponce(JObject inputJson, string userid, string access_token = null)
        {
            try
            {
                string url = "https://api.zoom.us/v2/users/" + userid + "/meetings";

                HttpResponseMessage Response = await ApiHelper.CallAPI(url, inputJson, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    APIZoomMeetingResponce ConfigurableParameters = JsonConvert.DeserializeObject<APIZoomMeetingResponce>(result);

                    return ConfigurableParameters;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async static Task<int> UpdateZoomMeetingResponce(JObject inputJson, string userid, string MeetingId, string access_token = null)
        {
            try
            {
                string url = null;
                HttpResponseMessage Response = null;

                url = "https://api.zoom.us/v2/meetings/" + MeetingId;
                Response = await ApiHelper.CallPatchAPIForTeams(url, inputJson, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    return 204;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return -1;
        }
        public async static Task<int> DeleteZoomMeetingResponce( string MeetingId, string access_token = null)
        {
            try
            {
                string url = null;
                HttpResponseMessage Response = null;

                url = "https://api.zoom.us/v2/users/meetings/" + MeetingId;
                Response = await ApiHelper.CallDeleteAPI(url, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    return 204;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return -1;
        }

        #endregion


        #region TeamsMeeting
        public async static Task<TeamsResponse> CreateTeamsEventResponse(JObject inputJson, string userid, string access_token, string baseUrl,string MeetingId)
        {
            try
            {
                //string url = "https://graph.microsoft.com/v1.0/" + userid + "/calendar/events";
                string url = null;
                HttpResponseMessage Response = null;
                if (MeetingId == null)
                {
                    url = baseUrl + userid + "/onlineMeetings";
                    Response = await ApiHelper.CallAPI(url, inputJson, access_token);
                }
                else
                {
                    url = baseUrl + userid + "/onlineMeetings/" + MeetingId;
                    Response = await ApiHelper.CallPatchAPIForTeams(url, inputJson, access_token);
                }

                 

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;

                    TeamsResponse TeamsResponce = JsonConvert.DeserializeObject<TeamsResponse>(result);

                    return TeamsResponce;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async static Task<TeamsEventResponse> CreateTeamsEvent(JObject inputJson, string access_token, string baseUrl)
        {
            try
            {

                HttpResponseMessage Response = null;
               
                Response = await ApiHelper.CallAPI(baseUrl, inputJson, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;

                    TeamsEventResponse TeamsResponce = JsonConvert.DeserializeObject<TeamsEventResponse>(result);

                    return TeamsResponce;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public static async Task<HttpResponseMessage> CallPatchAPI(string url, string oJsonObject, string token = null)
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

                    HttpContent httpContent = new StringContent(oJsonObject, Encoding.UTF8, "application/json");

                    var response = await client.PatchAsync(url, httpContent);
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;

            }

        }


        #endregion


        public static async Task<APIxAPICompletionDetails> checkxAPICompletion(string url, string oJsonObject, string token = null)
        {
            using (var client = new HttpClient())
            {
                try
                {

                    if (token != null)
                    {
                        token = token.Replace("Basic ", "");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
                        client.DefaultRequestHeaders.Add("X-Experience-API-Version", "1.0.2");
                    }
                    client.DefaultRequestHeaders.Accept.Clear();
                    var Responce = await client.GetAsync(url);
                    if (Responce.IsSuccessStatusCode)
                    {
                        var result = Responce.Content.ReadAsStringAsync().Result;
                        APIxAPICompletionDetails xAPIDetails = JsonConvert.DeserializeObject<APIxAPICompletionDetails>(result);

                        return xAPIDetails;
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;

            }

        }

        #region Edcast
        public async static Task<APIEdcastDetailsToken> GetTokenForEdcastLMS(JObject inputJson, string url=null, string access_token = null)
        {
            try
            {                 

                HttpResponseMessage Response = await ApiHelper.CallAPI(url, inputJson, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    APIEdcastDetailsToken ConfigurableParameters = JsonConvert.DeserializeObject<APIEdcastDetailsToken>(result);

                    return ConfigurableParameters;
                }
                else 
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    APIEdcastDetailsToken ConfigurableParameters = JsonConvert.DeserializeObject<APIEdcastDetailsToken>(result);

                    return ConfigurableParameters;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public static async Task<APIEdCastTransactionDetails> PostEdcastAPI(string url, string body, string token = null)
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
                    var Response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));

                    if (Response.IsSuccessStatusCode)
                    {
                        var result = Response.Content.ReadAsStringAsync().Result;
                        APIEdCastTransactionDetails edCastTransactionDetails = JsonConvert.DeserializeObject<APIEdCastTransactionDetails>(result);

                        return edCastTransactionDetails;
                    }
                    else {
                        var result = Response.Content.ReadAsStringAsync().Result;
                        APIEdCastTransactionDetails edCastTransactionDetails = JsonConvert.DeserializeObject<APIEdCastTransactionDetails>(result);

                        return edCastTransactionDetails;
                    }
                    
                    
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public static async Task<APIDarwinTransactionDetails> PostDarwinboxAPI(string url, string body, string username, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    string apiUrl = url;
                    var Response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));

                    if (Response.IsSuccessStatusCode)
                    {
                        var result = Response.Content.ReadAsStringAsync().Result;
                        APIDarwinTransactionDetails edCastTransactionDetails = JsonConvert.DeserializeObject<APIDarwinTransactionDetails>(result);

                        return edCastTransactionDetails;
                    }
                    else
                    {
                        var result = Response.Content.ReadAsStringAsync().Result;
                        APIDarwinTransactionDetails edCastTransactionDetails = JsonConvert.DeserializeObject<APIDarwinTransactionDetails>(result);

                        return edCastTransactionDetails;
                    }


                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        #endregion

        public static string CallTokenAPI(string url, string client_id, string organization_id, string client_secret, string organization_key)
        {
            using (var client = new HttpClient())
            {

                var data = new[]
                {
                    new KeyValuePair<string, string>("client_id", client_id),
                    new KeyValuePair<string, string>("organization_id", organization_id),
                    new KeyValuePair<string, string>("client_secret", client_secret),
                    new KeyValuePair<string, string>("organization_key", organization_key)
                };
                client.DefaultRequestHeaders.Add("grant_type", "client_credentials");

                try
                {
                    string apiUrl = url;
                    client.Timeout = TimeSpan.FromHours(1);

                    var response = Task.Run(() => client.PostAsync(url, new FormUrlEncodedContent(data)));

                    response.Wait();
                    var result = response.Result.Content.ReadAsStringAsync().Result;
                    AlisonToken alisonToken = JsonConvert.DeserializeObject<AlisonToken>(result);
                    return alisonToken.access_token;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return null;
            }

        }

        #region Alison
        public static RegisterData CallAlisonRegisterAPI(string url, string email, string firstname, string lastname, string city, string country, string token)
        {
            using (var client = new HttpClient())
            {
                var data = new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("firstname", firstname),
                    new KeyValuePair<string, string>("city", city),
                    new KeyValuePair<string, string>("country", country),
                    new KeyValuePair<string, string>("lastname", lastname == null ? firstname : lastname),
                };
                client.DefaultRequestHeaders.Add("grant_type", "client_credentials");
                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                try
                {
                    string apiUrl = url;
                    client.Timeout = TimeSpan.FromHours(1);

                    var response = Task.Run(() => client.PostAsync(url, new FormUrlEncodedContent(data)));

                    response.Wait();
                    var result = response.Result.Content.ReadAsStringAsync().Result;
                    RegisterData alisonToken = JsonConvert.DeserializeObject<RegisterData>(result);
                    return alisonToken;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return null;
            }

        }

        public static RegisterData CallAlisonRegisterAPI(string url, string email, string token)
        {
            using (var client = new HttpClient())
            {

                var data = new[]
                {
                    new KeyValuePair<string, string>("email", email),

                };
                client.DefaultRequestHeaders.Add("grant_type", "client_credentials");
                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                try
                {
                    string apiUrl = url;
                    client.Timeout = TimeSpan.FromHours(1);

                    var response = Task.Run(() => client.PostAsync(url, new FormUrlEncodedContent(data)));

                    response.Wait();
                    var result = response.Result.Content.ReadAsStringAsync().Result;
                    RegisterData registerData = JsonConvert.DeserializeObject<RegisterData>(result);
                    return registerData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return null;
            }

        }
        #endregion

        public static async Task<HttpResponseMessage> CallKPointAPI(AuthenticationKpoint authenticationKpoint, string fileName,string name, IFormFile formFile)
        {

            using (var client = new HttpClient())
            {
                using (var multipartFormContent = new MultipartFormDataContent())
                {

                    string key = authenticationKpoint.key + fileName;
                    multipartFormContent.Add(new StringContent(key), name: "key");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.bucket), name: "bucket");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.X_amz_signature), name: "X-amz-signature");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.X_amz_credential), name: "X-amz-credential");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.X_amz_algorithm), name: "X-amz-algorithm");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.X_amz_date), name: "X-amz-date");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.X_amz_expires), name: "X-amz-expires");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.acl), name: "acl");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.x_amz_server_side_encryption), name: "x-amz-server-side-encryption");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.Content_Type), name: "Content-Type");
                    multipartFormContent.Add(new StringContent(authenticationKpoint.parameters.policy), name: "policy");
                    multipartFormContent.Add(new StringContent(fileName), name: "Filename");
                    multipartFormContent.Add(new StringContent(name), name: "name");

                    try
                    {
                        var data = new MemoryStream();
                        formFile.CopyTo(data);
                        data.Seek(0, SeekOrigin.Begin);
                        var imageContent = new StreamContent(data);
                       
                        multipartFormContent.Add(imageContent,name: "file");

                        client.Timeout = TimeSpan.FromHours(1);
                        //Send it
                        var response = client.PostAsync("https://media-upload.zencite.com/", multipartFormContent);

                        return response.Result;

                       
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex);
                    }

                    return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
                   
                }

            }

        }
        public static HttpResponseMessage CallPostKPointAPI(string upload_url,string kapsule_name,string description,string URL)
        {
            var data = new[]
              {
                    new KeyValuePair<string, string>("upload_url", upload_url),
                    new KeyValuePair<string, string>("kapsule_name", kapsule_name),
                    new KeyValuePair<string, string>("description", description),
                    new KeyValuePair<string, string>("visibility", "USERS"),
                    new KeyValuePair<string, string>("published_flag", "true"),
                    new KeyValuePair<string, string>("topics", "api,rest,video")
                };
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromHours(1);
                //Send it
                var response = Task.Run(() => client.PostAsync(URL, new FormUrlEncodedContent(data)));
                response.Wait();
                return response.Result;
            }
        }
    }
}
