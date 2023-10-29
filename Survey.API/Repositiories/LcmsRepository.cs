using Survey.API.APIModel;
using Survey.API.Helper;
using Survey.API.Repositories.Interfaces;
using Survey.API.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Survey.API.Repositories
{
    public class LcmsRepository : ILcmsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identitySvc;
        public LcmsRepository(IConfiguration configuration, IIdentityService identitySvc)
        {
            this._configuration = configuration;
            this._identitySvc = identitySvc;
        }
        public async Task<int> PostLcms(ApiLcms apiLcms)
        {
            JObject oJsonObject = new JObject
            {
                { "description", apiLcms.Description },
                { "name", apiLcms.Name },
                { "contentType", apiLcms.ContentType },
                { "metaData", apiLcms.MetaData },
                { "isNested", apiLcms.IsNested }
            };
            string Url = this._configuration[Configuration.CourseApi];
            Url = Url + "Lcms/LcmsSurvey";
            string Token = this._identitySvc.GetToken();
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, Token);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                int LcmsId = JsonConvert.DeserializeObject<int>(result);
                return LcmsId;
            }
            else
            {
                string result = await response.Content.ReadAsStringAsync();
                if (result.ToLower().Contains("duplicate"))
                    return -1;
                return 0;
            }
        }
        public async Task<int> UpdateLcms(ApiLcms apiLcms)
        {
            JObject oJsonObject = new JObject
            {
                { "id", apiLcms.Id },
                { "description", apiLcms.Description },
                { "name", apiLcms.Name },
                { "contentType", apiLcms.ContentType },
                { "metaData", apiLcms.MetaData },
                { "isNested", apiLcms.IsNested }
            };
            string Url = this._configuration[Configuration.CourseApi];
            Url = Url + "Lcms/Survey/" + apiLcms.Id;
            string Token = this._identitySvc.GetToken();
            HttpResponseMessage response = await ApiHelper.CallPutAPI(Url, oJsonObject, Token);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return 1;
            }
            else
            {
                string result = await response.Content.ReadAsStringAsync();
                return 0;
            }
        }
        public async Task<ApiLcms> GetLcms(int lcmsId)
        {
            string Url = this._configuration[Configuration.CourseApi];
            Url = Url + "Lcms/" + lcmsId;
            string Token = this._identitySvc.GetToken();
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                ApiLcms apiLcms = JsonConvert.DeserializeObject<ApiLcms>(result);
                return apiLcms;
            }
            else
            {
                string result = await response.Content.ReadAsStringAsync();
                return null;
            }
        }
    }
}
