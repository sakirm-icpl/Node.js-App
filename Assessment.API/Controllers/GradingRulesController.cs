using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using log4net;
using Assessment.API.Common;
using Assessment.API.Helper;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Repositories.Interfaces;
using static Assessment.API.Common.AuthorizePermissions;
using static Assessment.API.Common.TokenPermissions;

namespace Assessment.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired("Discontinued")]
    //Remove this controller
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class GradingRulesController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GradingRulesController));
        private IGradingRules _gardingRules;
        private string url;
        private IConfiguration _configuration;
        private readonly ITokensRepository _tokensRepository;
        public GradingRulesController(IGradingRules gardingRules, IConfiguration configuration, ITokensRepository tokensRepository)
        {
            this._gardingRules = gardingRules;
            this._configuration = configuration;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet]
        public async Task<IEnumerable<GradingRules>> Get()
        {
            try
            {
                return await this._gardingRules.GetAll(s => s.IsDeleted == false);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<APIGradingRules> gradingRules = await this._gardingRules.GetAllGradingRules(page, pageSize, search);
                return Ok(gradingRules);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this._gardingRules.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        public async Task<GradingRules> Get(int id)
        {
            try
            {
                return await this._gardingRules.Get(s => s.IsDeleted == false && s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GradingRules gradingRules)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    GradingRules grading = new GradingRules();
                    if (await this._gardingRules.Exist(gradingRules.Grade, gradingRules.CourseId, gradingRules.ModelId))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        grading.GradingRuleID = Convert.ToString(await this._gardingRules.GetTotalGradingRulesCount() + 1);
                        grading.CourseId = gradingRules.CourseId;
                        grading.ModelId = gradingRules.ModelId;
                        grading.Grade = gradingRules.Grade;
                        grading.ScorePercentage = gradingRules.ScorePercentage;
                        grading.CreatedDate = DateTime.UtcNow;
                        grading.ModifiedDate = DateTime.UtcNow;
                        grading.IsDeleted = false;
                        grading.ModifiedBy = 1;
                        grading.CreatedBy = 1;
                        await _gardingRules.Add(grading);
                        return Ok(grading);
                    }
                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] GradingRules gradingRules)
        {
            try
            {
                GradingRules grading = await this._gardingRules.Get(s => s.IsDeleted == false && s.Id == id);

                if (grading == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && grading != null)
                {
                    grading.ModifiedDate = DateTime.UtcNow;
                    grading.ModifiedBy = 1;
                    grading.CourseId = gradingRules.CourseId;
                    grading.ModelId = gradingRules.ModelId;
                    grading.Grade = gradingRules.Grade;
                    grading.ScorePercentage = gradingRules.ScorePercentage;
                    await this._gardingRules.Update(grading);
                }
                return this.Ok(grading);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                GradingRules graderule = await this._gardingRules.Get(DecryptedId);

                if (ModelState.IsValid && graderule != null)
                {
                    graderule.IsDeleted = true;
                    await this._gardingRules.Update(graderule);
                }

                if (graderule == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        /// <summary>
        /// Search specific SupportManagement.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IEnumerable<GradingRules>> Search(string q)
        {
            try
            {
                IEnumerable<GradingRules> result = await this._gardingRules.Search(q);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        private async Task<HttpResponseMessage> CallAPI(string url)
        {
            using (var client = new HttpClient())
            {

                string apiUrl = this.url;

                var response = await client.GetAsync(url);

                return response;
            }
        }

    }
}