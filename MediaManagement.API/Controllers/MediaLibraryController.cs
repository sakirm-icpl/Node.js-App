// ======================================
// <copyright file="MediaLibraryController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using MediaManagement.API.APIModel;
using MediaManagement.API.Common;
using MediaManagement.API.Helper;
using MediaManagement.API.Models;
using MediaManagement.API.Repositories.Interfaces;
using MediaManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static MediaManagement.API.Common.AuthorizePermissions;
using static MediaManagement.API.Common.TokenPermissions;
using log4net;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using MediaManagement.API.Helper.Log_API_Count;

namespace MediaManagement.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class MediaLibraryController : IdentityController
    {
        
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MediaLibraryController));
        private IMediaLibraryRepository mediaLibraryRepository;
        private IAlbumRepository albumRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public MediaLibraryController( IMediaLibraryRepository mediaLibraryController, IAzureStorage azurestorage, IAlbumRepository albumController,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration confugu,
            IRewardsPointRepository rewardsPointRepository,
            IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.mediaLibraryRepository = mediaLibraryController;
            this.albumRepository = albumController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._identitySvc = identitySvc;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
            this._azurestorage = azurestorage;
           
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<MediaLibrary> mediaLibrary = await this.mediaLibraryRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIMediaLibrary>>(mediaLibrary));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Album")]
        public async Task<IActionResult> GetAlbum()
        {
            try
            {
                return Ok(await this.albumRepository.GetAlbum(UserId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Album/{id:int}")]
        public async Task<IActionResult> GetAlbumById(int id)
        {
            try
            {

                IEnumerable<MediaLibrary> mediaLibrary = await this.mediaLibraryRepository.GetAllMediaLibraryByAlbumId(id);
                if (mediaLibrary != null)
                    await this._rewardsPointRepository.AlbumReadRewardPoint(UserId, id, OrgCode);
                return Ok(Mapper.Map<List<APIMediaLibrary>>(mediaLibrary));
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                 return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTopOneMedia")]
        public async Task<IActionResult> GetTopOneMedia()
        {
            try
            {

                APIMediaLibrary mediaLibrary = await this.mediaLibraryRepository.GetTopOneMedia();
                return Ok(mediaLibrary);
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {

                List<APIMediaLibrary> mediaLibrary = await this.mediaLibraryRepository.GetAllMediaLibrary(UserId, UserRole, page, pageSize, search);
                return Ok(Mapper.Map<List<APIMediaLibrary>>(mediaLibrary));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int mediaLibrary = await this.mediaLibraryRepository.Count(UserId, UserRole, search);
                return Ok(mediaLibrary);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {

                MediaLibrary mediaLibrary = await this.mediaLibraryRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIMediaLibrary>(mediaLibrary));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("Exists/{search:minlength(0)?}")]
        public async Task<bool> Exists(string search)
        {
            try
            {
                return await this.mediaLibraryRepository.Exist(search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;   
            }

        }
        [HttpGet("Exists/ObjectTitle/{albumid?}/{search:minlength(0)?}")]
        public async Task<bool> ExistsTitle(int albumid, string search) 
        {
            try
            {
                return await this.mediaLibraryRepository.ExistTitle(albumid, search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;   
            }
        }
        //// GET: api/values
        //[HttpGet("Album/{category}")]
        //[Produces("application/json")]
        //public async Task<IActionResult> SearchAlbum(string category)
        //{

        //    return this.Ok(await this.albumRepository.Search(category));
        //}
        // GET: api/values
        [HttpGet("AlbumTyped/{album}")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchCategory(string album)
        {
            try
            {

                return this.Ok(await this.mediaLibraryRepository.SearchAlbum(UserId, album));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost("SaveMedia")]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> PostSaveMedia([FromBody]APIMediaLibraryBulk aPIMediaLibrary)
        {

            try
            {

                if (ModelState.IsValid)
                {

                    foreach (APIMediaLibraryMerge opt in aPIMediaLibrary.aPIMediaLibraryMerge)
                    {
                        MediaLibrary mediaLibrary = aPIMediaLibrary.MapAPIToMediaLibrary(aPIMediaLibrary, UserId);

                        mediaLibrary = this.mediaLibraryRepository.GetMediaLibraryObjectMerge(mediaLibrary, aPIMediaLibrary).Result;
                        mediaLibrary.Date = DateTime.UtcNow;
                        mediaLibrary.Album = aPIMediaLibrary.Album;
                        mediaLibrary.FileType = opt.FileType;
                        mediaLibrary.AlbumId = mediaLibrary.AlbumId;
                        mediaLibrary.ObjectId = (await this.mediaLibraryRepository.GetTotalMediaLibraryCount() + 1);
                        
                     //   bool result = await this.mediaLibraryRepository.ExistObjectTitle(opt.ObjectTitle);
                     //   if(result)
                     //       return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateTitle), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateTitle) });
                        mediaLibrary.ObjectTitle = opt.ObjectTitle;
                        mediaLibrary.FilePath = opt.FilePath;
                        mediaLibrary.Keywords = opt.Keywords;
                        mediaLibrary.LikesCount = aPIMediaLibrary.LikesCount;
                        mediaLibrary.IsDeleted = false;
                        mediaLibrary.ModifiedBy = UserId;
                        mediaLibrary.ModifiedDate = DateTime.UtcNow;
                        mediaLibrary.CreatedBy = UserId;
                        mediaLibrary.CreatedDate = DateTime.UtcNow;
                        mediaLibrary.Metadata = aPIMediaLibrary.Metadata;
                        await mediaLibraryRepository.Add(mediaLibrary);
                    }
                    return Ok(aPIMediaLibrary);


                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> Post([FromBody]APIMediaLibrary aPIMediaLibrary)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    MediaLibrary mediaLibrary = aPIMediaLibrary.MapAPIToMediaLibrary(aPIMediaLibrary, UserId);
                    mediaLibrary = this.mediaLibraryRepository.GetMediaLibraryObject(mediaLibrary, aPIMediaLibrary).Result;
                    mediaLibrary.Date = DateTime.UtcNow;
                    mediaLibrary.Album = aPIMediaLibrary.Album;
                    mediaLibrary.FileType = aPIMediaLibrary.FileType;
                    mediaLibrary.AlbumId = mediaLibrary.AlbumId;
                    mediaLibrary.ObjectId = (await this.mediaLibraryRepository.GetTotalMediaLibraryCount() + 1);
                    mediaLibrary.ObjectTitle = aPIMediaLibrary.ObjectTitle;
                    mediaLibrary.FilePath = aPIMediaLibrary.FilePath;
                    mediaLibrary.Keywords = aPIMediaLibrary.Keywords;
                    mediaLibrary.LikesCount = aPIMediaLibrary.LikesCount;
                    mediaLibrary.IsDeleted = false;
                    mediaLibrary.ModifiedBy = UserId;
                    mediaLibrary.ModifiedDate = DateTime.UtcNow;
                    mediaLibrary.CreatedBy = UserId;
                    mediaLibrary.CreatedDate = DateTime.UtcNow;
                    mediaLibrary.Metadata = aPIMediaLibrary.Metadata;
                    await mediaLibraryRepository.Add(mediaLibrary);
                    return Ok(mediaLibrary);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // POST api/<controller>
        [HttpGet("MediaLibraryRead/{id:int}")]
        public async Task<IActionResult> RewardPointread(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0790";
                string category = "Normal";
                int point = 1;
                int userId = UserId;
                await this.mediaLibraryRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();

            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpGet("MediaLibraryLike/{id:int}")]
        public async Task<IActionResult> RewardPointLike(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0790";
                string category = "Bonus";
                int point = 2;
                int userId = UserId;
                await this.mediaLibraryRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.MediaLibrary)]
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


                        // allowed .pdf,video,images


                        if (FileValidation.IsValidImageVideoPdf(fileUpload))
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
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks  + fileUpload.FileName);
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> Put(int id, [FromBody]APIMediaLibrary aPIMediaLibrary)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                MediaLibrary mediaLibrary = await this.mediaLibraryRepository.Get(s => s.IsDeleted == false && s.Id == id);
                string path = mediaLibrary.FilePath;
                if (mediaLibrary == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (mediaLibrary != null)
                {

                    mediaLibrary.Album = aPIMediaLibrary.Album;
                    mediaLibrary.FileType = aPIMediaLibrary.FileType;
                    mediaLibrary.AlbumId = aPIMediaLibrary.AlbumId;
                    mediaLibrary.ObjectTitle = aPIMediaLibrary.ObjectTitle;
                    mediaLibrary.FilePath = aPIMediaLibrary.FilePath;
                    mediaLibrary.Keywords = aPIMediaLibrary.Keywords;
                    mediaLibrary.LikesCount = aPIMediaLibrary.LikesCount;
                    mediaLibrary.ModifiedBy = UserId;
                    mediaLibrary.ModifiedDate = DateTime.UtcNow;
                    mediaLibrary.Metadata = aPIMediaLibrary.Metadata;
                    await this.mediaLibraryRepository.Update(mediaLibrary);

                    //Image File Delete
                    if ((path == aPIMediaLibrary.FilePath))
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
                }

                return Ok(mediaLibrary);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                MediaLibrary mediaLibrary = await this.mediaLibraryRepository.Get(DecryptedId);

                if (ModelState.IsValid && mediaLibrary != null)
                {
                    mediaLibrary.IsDeleted = true;
                    await this.mediaLibraryRepository.Update(mediaLibrary);
                }

                if (mediaLibrary == null)
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
        /// Search specific MediaLibrary.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<MediaLibrary> mediaLibrary = await this.mediaLibraryRepository.Search(q);
                return Ok(Mapper.Map<List<APIMediaLibrary>>(mediaLibrary));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
