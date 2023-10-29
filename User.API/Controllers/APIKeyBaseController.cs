
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper.Log_API_Count;
using User.API.Repositories.Interfaces;
using static User.API.Helper.EnumHelper;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/u/[controller]")]
    [ApiToken]
    public class HRMSBaseController :ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(HRMSBaseController));
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        public IHRMSService _hRMSService;        
        public IConfiguration _configuration { get; }

        public HRMSBaseController(ICustomerConnectionStringRepository conn,
            IHRMSService hRMSServic,
            IConfiguration configuration) 
        {
           this._customerConnectionStringRepository = conn;
            this._hRMSService = hRMSServic;
            this._configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("success");
        }
        [HttpPost("TVSUserMasterAPI")]
        public async Task<IActionResult> TVSUserMasterAPI([FromBody] APITVSUserData[] tvsData)
        {
            string OrgCode = "tvs"; // _configuration["TVSHEMSDB"];
            if (OrgCode is null)
                return  Ok(new APIHRMSResponse() { StatusCode = "ER", Success = false, StatusMessage = "Please check Database Code" });

             string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
             if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

            List<APITVSUserData> aPITVSUserDatas = tvsData.ToList();

            //string inputString = null;
            //using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            //{
            //    inputString = await reader.ReadToEndAsync();
            //}
            //var myDetails = JsonConvert.DeserializeObject<Hrmsesponse>(inputString);           
            aPITVSUserDatas.ForEach(u => u.IsActive= u.actionFlag.ToLower() == "deactivate" ? false : true);
            var result = await _hRMSService.MainHRMSProcess<APITVSUserData>(aPITVSUserDatas, OrgCode, ConnectionString);
            return Ok(result);
        }
       
    }
}