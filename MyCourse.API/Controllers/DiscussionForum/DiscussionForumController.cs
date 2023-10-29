using AspNet.Security.OAuth.Introspection;
using MyCourse.API.APIModel.DiscussionForum;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Helper.Metadata;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Repositories.Interfaces.DiscussionForum;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static MyCourse.API.Common.TokenPermissions;
using log4net;
using MyCourse.API.Helper.FileFormatValidation;
using MyCourse.API.Model.Log_API_Count;
using MyCourse.API.Repositories;

namespace MyCourse.API.Controllers.DiscussionForum
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class DiscussionForumController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DiscussionForumController));
        IDiscussionForumRepository _discussionForumRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        public IConfiguration _configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        ICourseRepository _courseRepository;

        public DiscussionForumController(IDiscussionForumRepository discussionForumRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository, IRewardsPointRepository rewardsPointRepository
            , IConfiguration configure, IHttpContextAccessor httpContextAccessor, ICourseRepository courseRepository) : base(_identitySvc)
        {
            _discussionForumRepository = discussionForumRepository;
            this._tokensRepository = tokensRepository;
            this._rewardsPointRepository = rewardsPointRepository;
            this._configuration = configure;
            this._httpContextAccessor = httpContextAccessor;
            _courseRepository = courseRepository;
        }

        [HttpGet("GetDiscussionForumByCourseId/{id}/{page:int}/{pageSize:int}/{IsShowActiveRecords}")]
        public async Task<IActionResult> GetDiscussionForumByCourseId(int id, int page, int pageSize, bool IsShowActiveRecords)
        {
            try
            {

                if (_discussionForumRepository.Exists(id))
                {
                    IEnumerable<APIDiscussionForum> courseDiscussion = await _discussionForumRepository.GetDiscussionForumByCourseId(id, UserId, IsShowActiveRecords, page, pageSize);
                    return Ok(courseDiscussion);
                }

                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                    });
            }
        }


        [HttpGet("GetAllDiscussionCommentsByParentId/{id}/{page:int}/{pageSize:int}/{IsShowActiveRecords}")]
        public async Task<IActionResult> GetAllDiscussionCommentsByParentId(int id, int page, int pageSize, bool IsShowActiveRecords)
        {
            try
            {
                IEnumerable<APIDiscussionForum> courseDiscussion = await _discussionForumRepository.GetAllDiscussionCommentsByParentId(id, UserId, IsShowActiveRecords, page, pageSize);
                return Ok(courseDiscussion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCountDiscussionForum/{id}/{IsShowActiveRecords}")]
        public async Task<IActionResult> CountDiscussionForum(bool? IsShowActiveRecords, int id)
        {
            try
            {
                var Count = await _discussionForumRepository.GetCountDiscussionForum(IsShowActiveRecords, id);
                return Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromForm] APIDiscussionForumPost aPIDiscussionForumPost)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {

                    string IsStartsWithAdmin = await _discussionForumRepository.GetParameterValue(OrganisationCode, "ADMIN_INITIATE_DISCUSSION");
                    if (IsStartsWithAdmin.Trim() == string.Empty)
                    {
                        // no results check course discussion initiate by any user  // default value no
                        // yes for IOCL - only admin can able to start discussion
                        IsStartsWithAdmin = "No";
                    }
                    string IsDiscussionWithCompleted = await _discussionForumRepository.GetParameterValue(OrganisationCode, "DISCUSSION_FOR_COMPLETED_COURSES");
                    if (IsDiscussionWithCompleted.Trim() == string.Empty)
                    {
                        // yes results check course staus as completed // default value yes
                        //no for ujjivan
                        IsDiscussionWithCompleted = "Yes";
                    }

                    //dont allow end user to create discussion thread                   
                    if (String.Compare(RoleCode, "CA") != 0 && aPIDiscussionForumPost.PostId == 0 && IsStartsWithAdmin.ToLower() == "yes")
                    {
                        return StatusCode(413, EnumHelper.GetEnumName(MessageType.NoAccess));
                    }

                    int? id = null;
                    int? PostId = aPIDiscussionForumPost.PostId;
                    int CourseId = aPIDiscussionForumPost.CourseId;
                    string SubjectText = aPIDiscussionForumPost.SubjectText;
                    string FilePath = null;
                    string FileTypee = null;
                    string User = UserName;
                    int ModifiedBy = UserId;
                    bool flag;
                    flag = await this._discussionForumRepository.CheckInprogessValidation(CourseId, ModifiedBy, RoleCode);
                    if (flag == true || IsDiscussionWithCompleted.ToLower() == "no")
                    {
                        if (aPIDiscussionForumPost.FileType != null && aPIDiscussionForumPost.FilePath != null)
                        {
                            var request = _httpContextAccessor.HttpContext.Request;
                            if (request.Form.Files.Count > 0)
                            {
                                int i = 0;
                                foreach (IFormFile uploadedFile in request.Form.Files)
                                {
                                    var supportedTypes = new[] { "jpeg", "gif", "bmp", "jpg", "png", "mp4", "webm", "ogg", "mpeg", "wav", "mp3", "ppt", "pptx", "xls", "pdf", "xlsx" };
                                    if (!FileValidation.IsValidExtension(uploadedFile, supportedTypes))
                                        return StatusCode(422, "Invalid File Format.");

                                    var sniffer = new Sniffer();                                    
                                    sniffer.Populate(FileTypes.DiscussioforumnFileTypes);   
                                    byte[] fileHead = FileValidation.ReadFileHead(uploadedFile);
                                    var results = sniffer.Match(fileHead);
                                    if (results.Count > 0)
                                    {
                                        i++;
                                    }
                                    if (i == 0)
                                    {
                                        return StatusCode(422, "Invalid File Format.");
                                    }
                                }
                                foreach (IFormFile uploadedFile in request.Form.Files)
                                {
                                    if (uploadedFile.Length <= 15000000)
                                    {
                                        if ((uploadedFile.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidLCMSVideo(uploadedFile)))
                                        {
                                            aPIDiscussionForumPost.FileType = FileType.Video;
                                            aPIDiscussionForumPost.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Video, OrganisationCode);

                                            FilePath = aPIDiscussionForumPost.FilePath;
                                            FileTypee = aPIDiscussionForumPost.FileType;
                                        }
                                        else if ((uploadedFile.ContentType.Contains(FileType.Audio)) && (FileValidation.IsValidLCMSAudio(uploadedFile)))
                                        {
                                            aPIDiscussionForumPost.FileType = FileType.Audio;
                                            aPIDiscussionForumPost.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Audio, OrganisationCode);

                                            FilePath = aPIDiscussionForumPost.FilePath;
                                            FileTypee = aPIDiscussionForumPost.FileType;
                                        }
                                        else if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                                        {
                                           
                                           
                                                aPIDiscussionForumPost.FileType = FileType.Image;
                                                aPIDiscussionForumPost.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Image, OrganisationCode);

                                                FilePath = aPIDiscussionForumPost.FilePath;
                                                FileTypee = aPIDiscussionForumPost.FileType;
                                            
                                        }
                                        else
                                        {
                                            int count = 0;
                                            foreach (string docType in FileType.Doc)
                                            {
                                                if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                                {
                                                    count++;    
                                                    aPIDiscussionForumPost.FileType = docType;
                                                    aPIDiscussionForumPost.FilePath = await this._courseRepository.SaveFile(uploadedFile, docType, OrganisationCode);

                                                    FilePath = aPIDiscussionForumPost.FilePath;
                                                    FileTypee = aPIDiscussionForumPost.FileType;
                                                }
                                            }
                                            if (count==0)
                                            {
                                                return StatusCode(422, "Invalid File Format.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return StatusCode(412, "File size is too large,please choose file upto 15mb.");
                                    }
                                }

                            }
                        }
                        string coursTitle = await _courseRepository.GetCourseNam(CourseId);
                        await this._discussionForumRepository.SavePost(id, PostId, CourseId, SubjectText, User, ModifiedBy, FilePath, FileTypee, OrganisationCode);
                        string Category = RewardPointCategory.Normal;
                        string Condition = RewardPointCategory.Replytoadiscussionthread;
                        await this._rewardsPointRepository.AddRewardDiscussionReply(UserId, CourseId, OrganisationCode, Category, Condition, coursTitle);
                        return Ok(aPIDiscussionForumPost);
                    }
                    return StatusCode(413, "User can't reply when course is 'Not Started', Please completing the course");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + " Message:" + ex.Message });
            }
        }



        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Model.DiscussionForum discussionForum = await _discussionForumRepository.Get(DecryptedId);

                if (UserId != discussionForum.CreatedBy && RoleCode != "CA")
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.NoAccess) });

                if (ModelState.IsValid && discussionForum != null)
                {
                    if (discussionForum.IsDeleted == false)
                    {
                        discussionForum.IsDeleted = true;
                    }
                    else if (discussionForum.IsDeleted == true)
                    {
                        discussionForum.IsDeleted = false;
                    }
                    await this._discussionForumRepository.Update(discussionForum);
                }

                if (discussionForum == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
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
                            string fileDir = this._configuration["ApiGatewayLXPFiles"];
                            fileDir = Path.Combine(fileDir, OrganisationCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
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

    }
}
