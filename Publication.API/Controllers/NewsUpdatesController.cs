// ======================================
// <copyright file="NewsUpdatesController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Publication.API.APIModel;
using Publication.API.Common;
using Publication.API.Helper;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Publication.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Publication.API.Common.AuthorizePermissions;
using static Publication.API.Common.TokenPermissions;
using log4net;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Publication.API.Helper.Log_API_Count;

namespace Publication.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class NewsUpdatesController : IdentityController
    {
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NewsUpdatesController));
        private INewsUpdatesRepository newsUpdatesRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        ISocialCheckHistoryRepository _socialCheckHistoryRepository;
        public NewsUpdatesController(INewsUpdatesRepository newsUpdatesController,
            IHttpContextAccessor httpContextAccessor,
            IAzureStorage azurestorage,
            IWebHostEnvironment environment,
            IConfiguration confugu,
            IIdentityService identitySvc,
            ITokensRepository tokensRepository,
            IRewardsPointRepository rewardsPointRepository,
            ISocialCheckHistoryRepository socialCheckHistoryRepository) : base(identitySvc)
        {
            this.newsUpdatesRepository = newsUpdatesController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
            this._rewardsPointRepository = rewardsPointRepository;
            this._socialCheckHistoryRepository = socialCheckHistoryRepository;
            this._azurestorage = azurestorage;
        }

        // GET: api/<controller>
        [HttpGet]
        //  [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(Mapper.Map<List<APINewsUpdates>>(await this.newsUpdatesRepository.GetAllApplicableNews(UserId)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.NewsUpdates)]

        public async Task<IActionResult> Get(int page, [Required, Range(1, 6)] int pageSize, string search = null)
        {
            try
            {
                IEnumerable<NewsUpdates> newsUpdates = await this.newsUpdatesRepository.GetAllNewsUpdates(page, pageSize, search);
                return Ok(Mapper.Map<List<APINewsUpdates>>(newsUpdates));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int newsUpdates = await this.newsUpdatesRepository.Count(search);
                return Ok(newsUpdates);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCount")]
        public async Task<IActionResult> GetTotalCount()
        {
            try
            {
                int newsUpdates = await this.newsUpdatesRepository.GetCount();
                return Ok(newsUpdates);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        //[PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                NewsUpdates newsUpdates = await this.newsUpdatesRepository.Get(s => s.IsDeleted == false && s.Id == id);
                await this._rewardsPointRepository.AddNewsUpdateReadReward(id, UserId, newsUpdates.SubHead, OrgCode);

                return Ok(Mapper.Map<APINewsUpdates>(newsUpdates));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> Post([FromBody] APINewsUpdates aPINewsUpdates)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    DateTime PublishDate = new DateTime();
                    PublishDate = aPINewsUpdates.PublishDate;
                    PublishDate = Convert.ToDateTime(PublishDate.ToString("dd MMMM yyyy"));

                    DateTime TodaysDate = new DateTime();
                    TodaysDate = DateTime.Now;
                    TodaysDate = Convert.ToDateTime(TodaysDate.ToString("dd MMMM yyyy"));
                    DateTime ValidityDate = new DateTime();
                    ValidityDate = aPINewsUpdates.ValidityDate;
                    ValidityDate = Convert.ToDateTime(TodaysDate.ToString("dd MMMM yyyy"));


                    if (ValidityDate >= PublishDate)
                    {
                        if (PublishDate >= TodaysDate)
                        {
                            NewsUpdates newsUpdates = new NewsUpdates
                            {
                                Date = aPINewsUpdates.Date.Date,
                                PublishDate = aPINewsUpdates.PublishDate.Date,
                                ValidityDate = aPINewsUpdates.ValidityDate.Date,
                                Headline = aPINewsUpdates.Headline,
                                SubHead = aPINewsUpdates.SubHead,
                                ClicksCount = aPINewsUpdates.ClicksCount,
                                ImagePath = aPINewsUpdates.ImagePath,
                                VideoPath = aPINewsUpdates.VideoPath,
                                DetailDescription = aPINewsUpdates.DetailDescription,
                                Source = aPINewsUpdates.Source,
                                IsDeleted = false,
                                ModifiedBy = UserId,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = UserId,
                                CreatedDate = DateTime.UtcNow
                            };
                            await newsUpdatesRepository.Add(newsUpdates);
                            return Ok(newsUpdates);
                        }
                        else
                            return BadRequest(new ResponseMessage { Description = "Publish date must be greater than Current date" });
                    }
                    else
                        return BadRequest(new ResponseMessage { Description = "Validity date must be greater than Publish date" });
                }

                else
                    return this.BadRequest(this.ModelState);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // POST api/<controller>
        [HttpGet("NewsUpdatesRead/{id:int}")]
        public async Task<IActionResult> PostCounter(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0780";
                string category = "Normal";
                int point = 1;
                int userId = UserId;
                await this.newsUpdatesRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();



            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // GET: WallFeed/NewNewsToShow
        [HttpGet]
        [Route("NewNewsToShow")]
        public async Task<IActionResult> NewNewsToShow()
        {
            try
            {
                return Ok(await this._socialCheckHistoryRepository.NewNewsToShow(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST: NewsUpdates/NewsCheckTime
        [HttpPost]
        [Route("NewsCheckTime")]
        public async Task<IActionResult> NewsCheckTime()
        {
            try
            {
                SocialCheckHistory oldFeedCheckHistory = await this._socialCheckHistoryRepository.CheckForDuplicate(UserId);
                if (oldFeedCheckHistory != null)
                {
                    oldFeedCheckHistory.LastNewsCheckTime = DateTime.Now;
                    await this._socialCheckHistoryRepository.Update(oldFeedCheckHistory);
                }
                else
                {
                    SocialCheckHistory feedCheckHistory = new SocialCheckHistory();
                    feedCheckHistory.UserId = UserId;
                    feedCheckHistory.LastNewsCheckTime = DateTime.Now;
                    await this._socialCheckHistoryRepository.Add(feedCheckHistory);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {

                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.MP4 : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }


                        if (FileValidation.IsValidVideo(fileUpload))
                        {
                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {

                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode, FileType);
                                // fileDir = Path.Combine(fileDir, , FileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName  );
                                using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
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
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, FileType);

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
                       
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostImageUpload")]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> PostImageUpload()
        {
            try
            {
                string FileType = null;
                string file = null;

                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string ImageType = string.IsNullOrEmpty(request.Form[Record.ImageType]) ? Record.ImageType : request.Form[Record.ImageType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        
                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode, ImageType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName );
                                using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
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
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, ImageType);

                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
                                    return null;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                                return null;
                            }

                        
                        return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                    }
                }
            } 
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
            return null;
        }
    

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> Put(int id, [FromBody]APINewsUpdates aPINewsUpdates)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (aPINewsUpdates.ValidityDate >= aPINewsUpdates.PublishDate)
                {
     
                        NewsUpdates newsUpdates = await this.newsUpdatesRepository.Get(s => s.IsDeleted == false && s.Id == id);
                        string path = newsUpdates.ImagePath;
                        string videoPath = newsUpdates.VideoPath;
                        if (newsUpdates == null)
                        {
                            return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                        }
                        if (newsUpdates != null)
                        {

                            newsUpdates.Date = aPINewsUpdates.Date.Date;
                            newsUpdates.PublishDate = aPINewsUpdates.PublishDate.Date;
                            newsUpdates.ValidityDate = aPINewsUpdates.ValidityDate.Date;
                            newsUpdates.Headline = aPINewsUpdates.Headline;
                            newsUpdates.SubHead = aPINewsUpdates.SubHead;
                            newsUpdates.ClicksCount = aPINewsUpdates.ClicksCount;
                            newsUpdates.ImagePath = aPINewsUpdates.ImagePath;
                            newsUpdates.VideoPath = aPINewsUpdates.VideoPath;
                            newsUpdates.DetailDescription = aPINewsUpdates.DetailDescription;
                            newsUpdates.Source = aPINewsUpdates.Source;
                            newsUpdates.ModifiedBy = UserId;
                            newsUpdates.ModifiedDate = DateTime.UtcNow;
                            await this.newsUpdatesRepository.Update(newsUpdates);

                            //Image File Delete
                            if ((path == aPINewsUpdates.ImagePath))
                            {
                            }
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                string sWebRootFolder = this._configuration["ApiGatewayLXPFiles"];
                                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "Social");
                                // path = path.Replace("/", @"\");
                                string[] pathRemove = path.Split("/");
                                string filename = pathRemove[3];
                                string remainpath;
                                remainpath = @"\" + pathRemove[1] + @"\" + pathRemove[2] + @"\";
                                sb = sb.Append(sWebRootFolder);
                                sb = sb.Append(remainpath);
                                string finalpath = sb.ToString();
                                FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                                if (file.Exists)
                                {
                                    file.Delete();
                                }
                            }
                            //Video File Delete
                            if ((videoPath == aPINewsUpdates.VideoPath))
                            {
                            }
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                string sWebRootFolder = this._configuration["ApiGatewayLXPFiles"];
                                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "Social");
                                string[] pathRemove = videoPath.Split("/");
                                string filename = pathRemove[3];
                                string remainpath;
                                remainpath = @"\" + pathRemove[1] + @"\" + pathRemove[2] + @"\";
                                sb = sb.Append(sWebRootFolder);
                                sb = sb.Append(remainpath);
                                string finalpath = sb.ToString();
                                FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                                if (file.Exists)
                                {
                                    file.Delete();
                                }
                            }
                        }

                        return Ok(newsUpdates);
                }
                else
                    return BadRequest(new ResponseMessage { Description = "Validity date must be greater than Publish date" });
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.NewsUpdates)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                NewsUpdates newsUpdates = await this.newsUpdatesRepository.Get(DecryptedId);

                if (ModelState.IsValid && newsUpdates != null)
                {
                    newsUpdates.IsDeleted = true;
                    await this.newsUpdatesRepository.Update(newsUpdates);
                }

                if (newsUpdates == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        /// <summary>
        /// Search specific News.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<NewsUpdates> newsUpdates = await this.newsUpdatesRepository.Search(q);
                return Ok(Mapper.Map<List<APINewsUpdates>>(newsUpdates));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
