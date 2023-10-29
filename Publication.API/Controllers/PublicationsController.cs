// ======================================
// <copyright file="PublicationsController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Publication.API.APIModel;
using Publication.API.Common;
using Publication.API.Helper;
using Publication.API.Metadata;
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static Publication.API.Common.AuthorizePermissions;
using static Publication.API.Common.TokenPermissions;
using static System.Net.WebRequestMethods;
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
    public class PublicationsController : IdentityController
    {
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PublicationsController));
        private IPublicationsRepository publicationsRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        private readonly ITokensRepository _tokensRepository;
        public PublicationsController(IPublicationsRepository publicationsController,
            IAzureStorage azurestorage,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IRewardsPointRepository rewardsPointRepository,
            IConfiguration confugu, IIdentityService identitySvc,
            ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.publicationsRepository = publicationsController;
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
        //[PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<Publications> publications = await this.publicationsRepository.GetAllPublications(UserId);
                return Ok(Mapper.Map<List<APIPublications>>(publications));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetSocialFile")]
       // [PermissionRequired(Permissions.Publications)]
        public IActionResult GetFile([FromBody]APIFileInfo socialfile)
        {
            //var file =  Path.Combine("E:/development/Services/Source/Services/Gadget/Gadget.API",
            //             "LXPFiles", socialfile); 
            try
            {
                if (!ModelState.IsValid || socialfile.socialfile == null)
                {
                    return BadRequest(ModelState);
                }

                if (Path.GetExtension(socialfile.socialfile) == ".mp4")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "video/mp4");
                }
                else if (Path.GetExtension(socialfile.socialfile) == ".avi")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "video/x-msvideo");
                }
                else if (Path.GetExtension(socialfile.socialfile) == ".mp3")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "audio/mp3");
                }
                else if (Path.GetExtension(socialfile.socialfile) == ".jpeg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/jpeg");
                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".jpg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/jpeg");

                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".bmp")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/bmp");

                }
                else if (Path.GetExtension(socialfile.socialfile) == ".png")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/png");

                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".doc")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/msword");
                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".docx")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".xlsx" || Path.GetExtension(socialfile.socialfile).ToLower() == ".xls")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
                else
                {
                    var res = socialfile.socialfile;
                    return this.Ok(res);
                    //return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/pdf");
                    //return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/pdf");
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }


            //return PhysicalFile(" C:/Publish/ApiGateway/LXPFiles/" + socialfile.socialfile, "application/pdf");
            //return PhysicalFile("E:/development/Services/Source/Services/Gadget/Gadget.API/LXPFiles" + socialfile.socialfile, "application/pdf");
        }
       


    [HttpPost("GetPublicationImage")]
       
        public IActionResult GetPublicationImage([FromBody]APIFileInfo socialfile)
        {
            try
            {
                var file = Path.Combine(this._configuration["ApiGatewayLXPFiles"],
                     socialfile.socialfile);

                //Check whether file is exists or not at particular location
                bool isFileExists = System.IO.File.Exists(file);

                if (isFileExists)
                {
                    if (Path.GetExtension(socialfile.socialfile) == ".jpeg")
                    {
                        return PhysicalFile(file, "image/jpeg");
                    }
                    else if (Path.GetExtension(socialfile.socialfile) == ".jpg")
                    {
                        return PhysicalFile(file, "image/jpeg");
                    }
                    else if (file == ".png")
                    {
                        return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/png");
                    }
                    else
                    {//IF Image Not Found Return Default Image
                        return PhysicalFile(this._configuration["DefaultProfilePicture"], "image/png");
                    }

                }
                else
                {   //IF Image Not Found Return Default Image
                    return PhysicalFile(this._configuration["DefaultProfilePicture"], "image/png");
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }






        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<Publications> publications = await this.publicationsRepository.GetAllPublications(page, pageSize, search);
                return Ok(Mapper.Map<List<APIPublications>>(publications));
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        //[AllowAnonymous]
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int publications = await this.publicationsRepository.Count(search);
                return Ok(publications);
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Publications publications = await this.publicationsRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIPublications>(publications));
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET: api/values
        [HttpGet("PublicationsTyped/{title}")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchCategory(string title)
        {
            try
            {
                return this.Ok(await this.publicationsRepository.SearchTitle(title));
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Exists/{search:minlength(0)?}")]
        public async Task <bool> Exists(string search)
        {
            try
            {
                return this.publicationsRepository.Exist(search);
            }
            catch(Exception ex)
            {
                throw ex;  
            }
        }

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Post([FromForm] APIPublications aPIPublications)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string file = null;
                    string FileType = null;
                    HttpRequest request = _httpContextAccessor.HttpContext.Request;
                    string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                    //string publication = string.IsNullOrEmpty(request.Form[Record.Publication]) ? Record.Pdf : request.Form[Record.Publication].ToString();
                    //string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.DocumentType : request.Form[Record.FileType].ToString();
                    if (request.Form.Files.Count > 0)
                    {
                        foreach (IFormFile fileUpload in request.Form.Files)
                        {
                            if (fileUpload.Length < 0 || fileUpload == null)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                            }

                            if (FileValidation.IsValidPdf(fileUpload))
                            {

                                var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                               {
                                    string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                    fileDir = Path.Combine(fileDir, OrgCode, "Social");
                                    fileDir = Path.Combine(fileDir, "pdf");
                                    if (!Directory.Exists(fileDir))
                                    {
                                        Directory.CreateDirectory(fileDir);
                                    }
                                    file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
                                    using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                                    {
                                        await fileUpload.CopyToAsync(fs);
                                    }
                                    if (String.IsNullOrEmpty(file))
                                    {
                                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                    }
                                    file = file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/");
                                }
                                else
                                {
                                    try
                                    {
                                        BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, "pdf","Social");

                                        if (res != null)
                                        {
                                            if (res.Error == false)
                                            {
                                                file = res.Blob.Name.ToString();
                                                file = "/" + file;
                                              // return Ok("/" + file.Replace(@"\", "/"));
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
                            //else if (FileValidation.IsValidLCMSDocument(fileUpload))
                            //{
                            //    string filename = fileUpload.FileName;
                            //    string[] fileaary = filename.Split('.');
                            //    string fileextention = fileaary[1].ToLower();
                            //    //string filex = Record.Docx;
                            //    if (fileextention == "docx")
                            //        FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Docx : request.Form[Record.FileType].ToString();
                            //    else
                            //        FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Doc : request.Form[Record.FileType].ToString();

                            //    if (fileextention != "docx" && fileextention != "doc")
                            //    {
                            //        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                            //    }
                            //    string fileDir = this._configuration["ApiGatewayWwwroot"];
                            //    fileDir = Path.Combine(fileDir, OrgCode, "Social");
                            //    fileDir = Path.Combine(fileDir,FileType);
                            //    if (!Directory.Exists(fileDir))
                            //    {
                            //        Directory.CreateDirectory(fileDir);
                            //    }
                            //    file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            //    using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                            //    {
                            //        await fileUpload.CopyToAsync(fs);
                            //    }
                            //    if (String.IsNullOrEmpty(file))
                            //    {
                            //        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            //    } 
                            //}
                            else
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                            }
                           
                                Publications publications = new Publications
                                {
                                    Date = aPIPublications.Date,
                                    Publication = aPIPublications.Publication,
                                    VolumeNumber = (await this.publicationsRepository.GetTotalPublicationCount() + 1),
                                    PublishedDate = aPIPublications.PublishedDate,
                                    Icon = aPIPublications.Icon,
                                    File = file,
                                    ClicksCount = aPIPublications.ClicksCount,
                                    RatingCount = aPIPublications.RatingCount,
                                    AverageRating = aPIPublications.AverageRating,
                                    IsDeleted = false,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow,
                                    CreatedBy = UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    Metadata = aPIPublications.Metadata,
                                };
                                await publicationsRepository.Add(publications);
                                return Ok(publications);
                        //}
                        //return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }
                      
                    }
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // POST api/<controller>
        [HttpGet("PublicationsRead/{id:int}")]
        public async Task<IActionResult> PublicationsRead(int id)
        {

            try
            {
                await this._rewardsPointRepository.PublicationReadRewardPoint(UserId, id,OrgCode);
                return Ok();
            }
            catch (Exception)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // POST api/<controller>
        [HttpGet("PublicationsLike/{id:int}")]
        public async Task<IActionResult> PublicationsLike(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0770";
                string category = "Special";
                int point = 3;
                int userId = UserId;
                await this.publicationsRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();
            }
            catch (Exception)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                string FileType = null;
                string file = null;
                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                //string publication = string.IsNullOrEmpty(request.Form[Record.Publication]) ? Record.Pdf : request.Form[Record.Publication].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }


                        if (FileValidation.IsValidPdf(fileUpload))
                        {
                            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string publication = string.IsNullOrEmpty(request.Form[Record.Publication]) ? Record.Pdf : request.Form[Record.Publication].ToString();
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode, "Social");
                                fileDir = Path.Combine(fileDir, "pdf");
                                //fileDir = Path.Combine(fileDir, publication);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + Record.Pdf);
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
                                             file = res.Blob.Name.ToString();

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
                        //else if (FileValidation.IsValidLCMSDocument(fileUpload))
                        //{
                        //    string filename = fileUpload.FileName;
                        //    string[] fileaary = filename.Split('.');
                        //    string fileextention = fileaary[1].ToLower();
                        //    //string filex = Record.Docx;
                        //    if (fileextention == "docx")
                        //        FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Docx : request.Form[Record.FileType].ToString();
                        //    else
                        //        FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Doc : request.Form[Record.FileType].ToString();

                        //    if (fileextention != "docx" && fileextention != "doc")
                        //    {
                        //        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                        //    }
                        //    string fileDir = this._configuration["ApiGatewayWwwroot"];
                        //    fileDir = Path.Combine(fileDir, OrgCode, "Social");
                        //    fileDir = Path.Combine(fileDir,FileType);
                        //    if (!Directory.Exists(fileDir))
                        //    {
                        //        Directory.CreateDirectory(fileDir);
                        //    }
                        //    file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                        //    using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                        //    {
                        //        await fileUpload.CopyToAsync(fs);
                        //    }
                        //    if (String.IsNullOrEmpty(file))
                        //    {
                        //        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                        //    }
                        //    return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                        //}
                        else
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                    }


                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        //[HttpPost]
        //[Route("PostImageUpload")]
        //public async Task<IActionResult> PostImageUpload()
        //{
        //    try
        //    {
        //        var request = _httpContextAccessor.HttpContext.Request;
        //        string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.PageType].ToString();
        //        string publication = string.IsNullOrEmpty(request.Form[Record.Publication]) ? Record.png : request.Form[Record.ImageType].ToString();
        //        if (request.Form.Files.Count > 0)
        //        {
        //            foreach (IFormFile fileUpload in request.Form.Files)
        //            {
        //                if (fileUpload.Length < 0 || fileUpload == null)
        //                {
        //                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
        //                }
        //                string fileDir = Path.Combine(hostingEnvironment.WebRootPath, customerCode, publication);
        //                if (!Directory.Exists(fileDir))
        //                {
        //                    Directory.CreateDirectory(fileDir);
        //                }
        //                string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + Record.png);
        //                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
        //                {
        //                    await fileUpload.CopyToAsync(fs);
        //                }
        //                if (String.IsNullOrEmpty(file))
        //                {
        //                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
        //                }
        //                return Ok(file.Substring(file.LastIndexOf("\\" + customerCode)).Replace(@"\", "/"));
        //            }

        //        }
        //        return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        //    }
        //}

        [HttpPost]
        [Route("PostFileIconUpload")]
        public async Task<IActionResult> ProfilePictureUpload([FromBody] Picture pictureProfile)
        {
            try
            {

                if (string.IsNullOrEmpty(pictureProfile.Base64String))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                string[] str = pictureProfile.Base64String.Split(',');
                byte[] bytes = Convert.FromBase64String(str[1]);

                 var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                        string fileDir = this._configuration["ApiGatewayLXPFiles"];
                    fileDir = Path.Combine(fileDir, OrgCode, "Social", "Images");
                    // fileDir = Path.Combine(fileDir, pictureProfile.CustomerCode);
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }
                    string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                    if (bytes.Length > 0)
                    {
                        using (FileStream stream = new FileStream(file, FileMode.Create))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Flush();
                        }
                    }

                    if (string.IsNullOrEmpty(file))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    return this.Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                }
                else
                {
                    BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode,"Images", "Social");
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
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
       // [AllowAnonymous]
       // [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Put(int id, [FromBody]APIPublications aPIPublications)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Publications publications = await this.publicationsRepository.Get(s => s.IsDeleted == false &&  s.Id == id);
                if (publications == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                string path = publications.File;
                string iconPath = publications.Icon;
               
                if (publications != null)
                {
                    publications.Date = aPIPublications.Date;
                    publications.Publication = aPIPublications.Publication;
                    publications.VolumeNumber = aPIPublications.VolumeNumber;
                    publications.PublishedDate = aPIPublications.PublishedDate;
                    publications.Icon = aPIPublications.Icon;
                    publications.File = aPIPublications.File;
                    publications.ClicksCount = aPIPublications.ClicksCount;
                    publications.RatingCount = aPIPublications.RatingCount;
                    publications.AverageRating = aPIPublications.AverageRating;
                    publications.ModifiedBy = UserId;
                    publications.ModifiedDate = DateTime.UtcNow;
                    publications.Metadata = aPIPublications.Metadata;
                    await this.publicationsRepository.Update(publications);

                    //Image File Delete
                    if ((path == aPIPublications.File))
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
                        string finalpath = sb.ToString();
                        FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    //Video File Delete
                    if ((iconPath == aPIPublications.Icon))
                    {
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        string sWebRootFolder = this._configuration["ApiGatewayLXPFiles"];
                        sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "Social");
                        string[] pathRemove = iconPath.Split("/");
                        string filename = @"\" + pathRemove[0]; //+ @"\" + pathRemove[4] + @"\";
                        string remainpath;
                        remainpath = @"\" + pathRemove[0];// + @"\" + pathRemove[2] + @"\";
                        sb = sb.Append(sWebRootFolder);
                        string finalpath = sb.ToString();
                        FileInfo file = new FileInfo(Path.Combine(finalpath, filename));
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                }

                return Ok(publications);

            }
            catch (Exception ex)
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.Publications)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Publications publications = await this.publicationsRepository.Get(DecryptedId);

                if (ModelState.IsValid && publications != null)
                {
                    publications.IsDeleted = true;
                    await this.publicationsRepository.Update(publications);
                }

                if (publications == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        /// <summary>
        /// Search specific Publications.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<Publications> announcements = await this.publicationsRepository.Search(q);
                return Ok(Mapper.Map<List<APIPublications>>(announcements));
            }
            catch
            {
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
