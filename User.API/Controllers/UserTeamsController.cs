using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.TokenPermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/u/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class UserTeamsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserTeamsController));
        private readonly IIdentityService _identitySvc;
        private readonly IUserTeamsRepository _iUserTeamsRepository;
        private IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public IConfiguration _configuration { get; set; }

        public UserTeamsController(IIdentityService identityService, IUserTeamsRepository iUserTeamsRepository,
         IUserRepository userRepository,
        IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ICustomerConnectionStringRepository customerConnectionString) : base(identityService)
        {
            this._identitySvc = identityService;
            this._iUserTeamsRepository = iUserTeamsRepository;
            this._configuration = configuration;
            this._httpContextAccessor = httpContextAccessor;
            this._customerConnectionString = customerConnectionString;
            this._userRepository = userRepository;
        }

        #region User Teams
        [HttpPost("SaveUserTeams")]
        public async Task<IActionResult> PostUserTeams([FromBody] UserTeams userTeams)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                if (userTeams == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Data Not Found", Description = "Please enter data of user team" });
                }
                if (userTeams.TeamCode == null || string.IsNullOrEmpty(userTeams.TeamCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Team code Not Found", Description = "team code is invalid" });
                }
                if (userTeams.TeamName == null || string.IsNullOrEmpty(userTeams.TeamName))
                {
                    return BadRequest(new ResponseMessage { Message = "Team name Not Found", Description = "Team name is invalid" });
                }

                userTeams.CreatedDate = DateTime.Now;
                userTeams.ModifiedDate = DateTime.Now;
                userTeams.ModifiedBy = UserId;
                userTeams.CreatedBy = UserId;
                userTeams.NumberOfRules = 0;
                userTeams.NumberofMembers = 0;
                userTeams.IsDeleted = false;

                int Result = await this._iUserTeamsRepository.SaveUserTeams(userTeams);
                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "User Team save", Description = "User Team Saved Successfully" });
                }
                else if (Result == -4)
                {
                    return BadRequest(new ResponseMessage { Message = "Duplicate Data", Description = "Duplicate Team code " });
                }
                else if (Result == -5)
                {
                    return BadRequest(new ResponseMessage { Message = "Duplicate Data", Description = "Duplicate Team Name " });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("GetAllUserTeams")]
        public async Task<IActionResult> GetUserTeams([FromBody] UserTeamsPage userTeamsPage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                IEnumerable<UserTeams> userTeams = await _iUserTeamsRepository.GetAllUserTeams(userTeamsPage.Page, userTeamsPage.PageSize, userTeamsPage.Search, userTeamsPage.ColumnName, UserId);
                return Ok(userTeams);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("GetAllUserTeamsCount")]
        public async Task<IActionResult> GetUserTeamsCount([FromBody] UserTeamsPage userTeamsPage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                int Count = await _iUserTeamsRepository.GetAllUserTeamsCount(userTeamsPage.Page, userTeamsPage.PageSize, userTeamsPage.Search, userTeamsPage.ColumnName, UserId);
                return Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult DeleteUserTeams([FromQuery] string TeamCode)
        {
            try
            {
                if (TeamCode == null || string.IsNullOrEmpty(TeamCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Teams code is null" });
                }
                int Result = this._iUserTeamsRepository.DeleteUserTeams(TeamCode);

                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "Record Deleted Successfully" });
                }
                else if (Result == -2)
                {
                    return BadRequest(new ResponseMessage { Message = "TeamCode is not available", Description = "teams code is invalid" });
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = "TeamCode is null", Description = "teams code is null" });
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("UpdateUserTeams")]
        public async Task<IActionResult> UpdateUserTeams([FromBody] UserTeams userTeams)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                if (userTeams == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Data Not Found", Description = "Please enter data of user team" });
                }
                if (userTeams.TeamCode == null || string.IsNullOrEmpty(userTeams.TeamCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Team Code Not Found", Description = "Please check team code" });
                }
                if (userTeams.TeamName == null || string.IsNullOrEmpty(userTeams.TeamName))
                {
                    return BadRequest(new ResponseMessage { Message = "Team Name Not Found", Description = "Please check team name" });
                }
                UserTeams userTeams1 = _iUserTeamsRepository.GetUserTeamsByTeamsCode(userTeams.TeamCode);
                if (userTeams1 == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Invalid Teams Code", Description = "Please check team code" });
                }
                int Result = await _iUserTeamsRepository.UpdateUserTeams(userTeams, UserId);
                if (Result == -4)
                {
                    return BadRequest(new ResponseMessage { Message = "Duplicate Teams Name", Description = "Teams Name is invalid name should be unique" });
                }
                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "User Team Update", Description = "User Team Updated Successfully" });
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        #endregion
        [HttpPost("GetTeamMappingUserCount")]
        public async Task<IActionResult> GetTeamMappingUserCount([FromBody] UserTeamCount getTeamCount)
        {
            try
            {
                if (getTeamCount == null )
                {
                    return BadRequest(new ResponseMessage { Message = "Teams code is null" });
                }
                List<UserTeamApplicableUser> Result = this._userRepository.GetUsersForuserTeam(getTeamCount.TeamCode);
                if (Result != null)
                {
                    return Ok(Result.Count());
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        #region Mapping
        [HttpGet("TypeAheadForuserTeams/{search?}")]
        public async Task<IActionResult> GetTypeAheadForUserTeams(string search = null)
        {
            try
            {
                return Ok(await _iUserTeamsRepository.GetUserTeams(search));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpPost("SaveMapping")]
        public async Task<IActionResult> PostMappingCondition([FromBody] MappingParameters[] mappingParameters)
        {
            List<Mappingparameter> reject = new List<Mappingparameter>();
            if (mappingParameters == null)
            {
                return BadRequest(new ResponseMessage { Message = "Invalid Data" });
            }
            foreach (MappingParameters rule in mappingParameters)
            {
                bool isvalid = false;

                isvalid = await _iUserTeamsRepository.CheckValidData(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.UserTeamsId);

                if (!isvalid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
            foreach (MappingParameters mappingParameters1 in mappingParameters)
            {
                List<Mappingparameter> Result = await _iUserTeamsRepository.CheckmappingStatus(mappingParameters1, UserId);

                if (Result != null)
                {
                    foreach(Mappingparameter mapping in Result)
                    {
                        reject.Add(mapping);
                    }
                }
            }
            if (reject.Count != 0)
            {
                if(reject[0].AccessibilityParameter == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Invalid UserTeams" });
                }
                return (BadRequest(reject));
            }

            return Ok();
        }

        [HttpPost("GetRules")]
        public async Task<IActionResult> GetRules([FromBody] APIGetRulesFormapping objAPIGetRules)
        {
            try
            {
                return Ok(await _iUserTeamsRepository.GetAccessibilityRules(objAPIGetRules.userTeamsId,OrgCode,Token, objAPIGetRules.page, objAPIGetRules.pageSize));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpPost("GetRulesCount")]
        public async Task<IActionResult> GetRulesCount([FromBody] APIGetRulesFormapping objAPIGetRules)
        {
            try
            {
                return Ok(await _iUserTeamsRepository.GetAccessibilityRulesCount(objAPIGetRules.userTeamsId));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpDelete("DeleteRule")]
        public async Task<IActionResult> RuleDelete([FromQuery] string id)
        {
            try
            {
                int Id = Convert.ToInt32(id);
                int Result = await _iUserTeamsRepository.DeleteRule(Id);
                if (Result == 1)
                    return Ok();
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotAvailable), Description = EnumHelper.GetEnumDescription(MessageType.DataNotAvailable) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion


        #region "Import data"
        [HttpGet]
        [Route("UserTeamsImport")]
        public IActionResult UserTeamsImport()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "UserTeamsImport.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));

                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("UserTeamsImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "TeamCode*";
                    worksheet.Cells[1, 2].Value = "UserId*";
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save();
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {

                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();

                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string filename = fileUpload.FileName;
                            string[] fileaary = filename.Split('.');
                            string fileextention = fileaary[1].ToLower();
                            string filex = Record.XLSX;
                            if (fileextention != filex)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                            }
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrgCode, Record.Users);
                            fileDir = Path.Combine(fileDir, customerCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = ex.Message, Description = ex.InnerException.ToString() });
            }

        }
        [HttpPost]
        [Route("SaveUserData")]
        public ApiResponse SaveUserTeam([FromBody] UserTeamsImportPayload aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResponse() { StatusCode = 400, Description = "Invalid post request" };

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));

                return _iUserTeamsRepository.ProcessImportFile(file, _customerConnectionString, UserId, _configuration, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new ApiResponse() { StatusCode = 400, Description = "Error while importing User Team Mapping file. Please contact support." };
            }
        }

        [HttpPost]
        [Route("ExportUserTeamReport")]
        public IActionResult DownloadUserTeamReport([FromBody] APIAccessibilityRuleFilePath aPIFilePath)
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, aPIFilePath.Path));

                if (!file.Exists)
                    return BadRequest(new ResponseMessage { Message = "File does not exits." });

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

        [HttpPost("GetUserTeamUserList_Export")]
        public async Task<IActionResult> GetUserTeamUserList_Export([FromBody] APIGetRulesUserTeam aPIGetRulesUserTeam)
        {
            try
            {
                FileInfo ExcelFile;
                int Id = aPIGetRulesUserTeam.userTeamId;
                
                List<MappingParameters> accessibilityRules = new List<MappingParameters>();

                int count = await this._iUserTeamsRepository.GetAccessibilityRulesCount(aPIGetRulesUserTeam.userTeamId);
                if(count != 0)
                {
                    accessibilityRules = await this._iUserTeamsRepository.GetAccessibilityRules(aPIGetRulesUserTeam.userTeamId, OrgCode, Token, 1, count);

                }

                UserTeams userTeams = _iUserTeamsRepository.getUserById(aPIGetRulesUserTeam.userTeamId);

                List<UserTeamApplicableUser> UserListForUserTeam1 = this._userRepository.GetUsersForuserTeam(userTeams.TeamCode);


                ExcelFile = this._iUserTeamsRepository.GetApplicableUserListExcel(accessibilityRules, UserListForUserTeam1, userTeams.TeamName, OrgCode);

                var fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    fileData = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, ExcelFile.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
