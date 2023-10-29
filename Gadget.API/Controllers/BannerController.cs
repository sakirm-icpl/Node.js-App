using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.Common;
using Gadget.API.Helper;
using Gadget.API.Metadata;
using Gadget.API.Models;
using Gadget.API.Repositories;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Gadget.API.Common.AuthorizePermissions;
using static Gadget.API.Common.TokenPermissions;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/g/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class BannerController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BannerController));
        private readonly ITokensRepository _tokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IBannerRepository _bannerRepository;
        IAzureStorage _azurestorage;

        public IConfiguration _configuration { get; }
        public BannerController(IIdentityService identitySvc,
            ITokensRepository tokensRepository,
            IHttpContextAccessor httpContextAccessor,
            IBannerRepository bannerRepository,
            IAzureStorage azurestorage,
            IConfiguration config) : base(identitySvc)
        {
            this._azurestorage = azurestorage;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = config;
            this._tokensRepository = tokensRepository;
            this._bannerRepository = bannerRepository;
        }

        [HttpGet("{page:int}/{pageSize:int}/{status?}/{search?}")]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> GetAll(int page, int pageSize, bool? status = null, string search = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                return Ok(await _bannerRepository.GetAll(page, pageSize, status, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("count/{status?}/{search?}")]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> GetCount(bool? status = null, string search = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                return Ok(await _bannerRepository.Count(status, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{BannerId}")]
        public async Task<IActionResult> GetAuthoringMasterIdByModuleID(int BannerId)
        {
            try
            {
                return Ok(await _bannerRepository.GetBannerById(BannerId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetActiveBanners")]
        public async Task<IActionResult> GetActiveBanners()
        {
            try
            {
                return Ok(await _bannerRepository.GetActiveBanners());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ShowBannerOnLandingPage")]
        public async Task<IActionResult> ShowBannerOnLandingPage()
        {
            try
            {
                return Ok(await _bannerRepository.ShowBannerOnLandingPage());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> Post([FromBody] BannerPayload bannerPayload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else if (await this._bannerRepository.NameExists(bannerPayload.Name))
                {
                    return this.StatusCode(409, "Duplicate Entry! Name Already Exists");
                }
                else
                {
                    int TotalCount = await _bannerRepository.TotalCount();

                    Banner banner = new Banner();
                    banner.Name = bannerPayload.Name;
                    banner.BannerType = bannerPayload.BannerType;
                    banner.ThumbnailImage = bannerPayload.ThumbnailImage;
                    banner.Path = bannerPayload.Path;
                    banner.IsActive = bannerPayload.IsActive;
                    banner.BannerNumber = TotalCount+1;
                    banner.CreatedBy = UserId;
                    banner.CreatedDate = DateTime.UtcNow;
                    banner.IsDeleted = false;

                    await _bannerRepository.Add(banner);

                    int id = banner.Id;
                    return Ok(id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> Put(int id, [FromBody] Banner banner)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Banner bannerObj = await this._bannerRepository.Get(id);
                if (bannerObj == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else if (await this._bannerRepository.NameExists(banner.Name, bannerObj.Id))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                else
                {
                    bannerObj.Name = banner.Name;
                    bannerObj.BannerType = banner.BannerType;
                    bannerObj.ThumbnailImage = banner.ThumbnailImage;
                    bannerObj.Path = banner.Path;
                    bannerObj.IsActive = banner.IsActive;
                    bannerObj.ModifiedBy = UserId;
                    bannerObj.ModifiedDate = DateTime.UtcNow;
                    await _bannerRepository.Update(bannerObj);
                    return Ok(bannerObj.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int bannerId = Convert.ToInt32(id);
                Banner banner = await _bannerRepository.Get(bannerId);

                banner.IsDeleted = true;
                banner.ModifiedBy = UserId;
                banner.ModifiedDate = DateTime.UtcNow;
                await _bannerRepository.Update(banner);
                return Ok(Message.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("UpdateBannerSequence")]
        public async Task<IActionResult> BulkPermissionUpdate([FromBody] List<BannerPayload> banners)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                List<Banner> updatedData = await this._bannerRepository.UpdateBannerSequence(banners, UserId);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("ThumbnailUpload")]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> ProfilePictureUpload([FromBody] ThumbnailImage pictureProfile)

        {
            try
            {
                if (string.IsNullOrEmpty(pictureProfile.Base64String))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                string[] str = pictureProfile.Base64String.Split(',');
                var bytes = Convert.FromBase64String(str[1]);

                var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                    string fileDir = this._configuration["ApiGatewayWwwroot"];
                    fileDir = Path.Combine(fileDir, OrgCode, "banner", "thumbnail");
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }
                    string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                    if (bytes.Length > 0)
                    {
                        using (var stream = new FileStream(file, FileMode.Create))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Flush();
                        }
                    }

                    if (string.IsNullOrEmpty(file))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    file = file.Substring(file.LastIndexOf(OrgCode)).Replace(@"\", "/");
                    file = file.Replace(@"\""", "");
                    return Ok(file);
                }
                else
               {
                    BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, "thumbnail", "banner");
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            string filePath = res.Blob.Name.ToString();
                            return this.Ok(filePath.Replace(@"\", "/"));
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost] 
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.Banner)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string code = "banner";
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (fileUpload.ContentType.Contains(FileType.Image))
                        {
                            string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.png : request.Form[Record.FileType].ToString();

                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayWwwroot"];
                                fileDir = Path.Combine(fileDir, OrgCode, code, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
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
                           else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, fileType, code);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                             string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                        else if (fileUpload.ContentType.Contains(FileType.Video))
                        {
                            string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.mp4 : request.Form[Record.FileType].ToString();

                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {

                                string fileDir = this._configuration["ApiGatewayWwwroot"];
                                fileDir = Path.Combine(fileDir, OrgCode, code, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
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
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode,fileType, code);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                             string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                    }
                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }
    }
}
