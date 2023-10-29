using AspNet.Security.OAuth.Introspection;
using WallFeed.API.Helper;
using WallFeed.API.Common;
using WallFeed.API.Metadata;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using WallFeed.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static WallFeed.API.Common.TokenPermissions;
using WallFeed.API.APIModel;
using WallFeed.API.Helper.Log_API_Count;

namespace WallFeed.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class WallFeedController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WallFeedController));
        IFeedRepository _feedRepository;
        IFeedContentRepository _feedContentRepository;
        ISocialCheckHistoryRepository _socialCheckHistoryRepository;
        IFeedLikeRepository _feedLikeRepository;
        IFeedCommentsRepository _feedCommentsRepository;
        IFeedCommentsLikeRepository _feedCommentsLikeRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public WallFeedController(IHttpContextAccessor httpContextAccessor,
                                IFeedRepository feedRepository,
                                IFeedContentRepository feedContentRepository,
                                ISocialCheckHistoryRepository socialCheckHistoryRepository,
                                IFeedLikeRepository feedLikeRepository,
                                IFeedCommentsRepository feedCommentsRepository,
                                IFeedCommentsLikeRepository feedCommentsLikeRepository,
                                IIdentityService identitySvc) : base(identitySvc)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._feedRepository = feedRepository;
            this._feedContentRepository = feedContentRepository;
            this._socialCheckHistoryRepository = socialCheckHistoryRepository;
            this._feedLikeRepository = feedLikeRepository;
            this._feedCommentsRepository = feedCommentsRepository;
            this._feedCommentsLikeRepository = feedCommentsLikeRepository;

        }


        // GET: WallFeed
        [HttpGet]
        public async Task<IActionResult> GetFeed()
        {
            try
            {
                return Ok(await _feedRepository.GetFeedData(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // GET: WallFeed/NewPostToShow
        [HttpGet]
        [Route("NewPostToShow")]
        public async Task<IActionResult> NewPostToShow()
        {
            try
            {
                return Ok(await this._socialCheckHistoryRepository.NewPostToShow(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

     
        // POST: WallFeed
        [HttpPost]
        public async Task<IActionResult> PostFile([FromForm]string caption)
        {
            try
            {
                int FeedTableId=0;
                if (ModelState.IsValid)
                {
                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {
                        foreach (IFormFile uploadedFile in request.Form.Files)
                        {
                            if (uploadedFile.Length < 20971520)
                            {

                                if (FeedTableId == 0 && (uploadedFile.ContentType.Contains(FileType.Image)))
                                    FeedTableId = await RetureFeedId(caption, Constants.Image);
                                else if (FeedTableId == 0 && (uploadedFile.ContentType.Contains(FileType.Video)))
                                        FeedTableId = await RetureFeedId(caption, Constants.Video);
                                else
                                {
                                    foreach (string docType in FileType.Doc)
                                    {
                                        if ((uploadedFile.ContentType.Contains(docType)) && FeedTableId == 0)
                                        {
                                            FeedTableId = await RetureFeedId(caption, Constants.Document);
                                        }
                                    }
                                }


                                if ((uploadedFile.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidVideo(uploadedFile)))
                                {
                                    string location = await this._feedRepository.SaveVideo(uploadedFile);
                                    await AddContent(FeedTableId, location);

                                    return Ok();
                                }
                                else if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                                {
                                    string location = await this._feedRepository.SaveImage(uploadedFile);
                                    await AddContent(FeedTableId, location);

                                }
                                else
                                {
                                    foreach (string docType in FileType.Doc)
                                    {
                                        if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                        {
                                            string location = await this._feedRepository.SavePdf(uploadedFile);
                                            await AddContent(FeedTableId, location);

                                            return Ok();
                                        }
                                    }
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                                }
                            }
                            else
                            {
                                return StatusCode(412, "File size is too large. Allowed file limit is 20mb.");
                            }
                        }

                        return Ok();
                    }
                    else
                    {
                        if (FeedTableId == 0)
                            await RetureFeedId(caption, Constants.Text);

                        return Ok();
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // POST: WallFeed/FeedCheckTime
        [HttpPost]
        [Route("FeedCheckTime")]
        public async Task<IActionResult> PostFeedCheckTime()
        {
            try
            {
                SocialCheckHistory oldFeedCheckHistory = await this._socialCheckHistoryRepository.CheckForDuplicate(UserId);
                if (oldFeedCheckHistory != null)
                {
                    oldFeedCheckHistory.LastWallfeedCheckTime = DateTime.Now;
                    await this._socialCheckHistoryRepository.Update(oldFeedCheckHistory);
                }
                else
                {
                    SocialCheckHistory feedCheckHistory = new SocialCheckHistory();
                    feedCheckHistory.UserId = UserId;
                    feedCheckHistory.LastWallfeedCheckTime = DateTime.Now;
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


        // DELETE : WallFeed
        [HttpDelete]
        public async Task<IActionResult> DeleteFeed([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                List<Feed> feed = await _feedRepository.FeedValidation(DecryptedId);

                if (feed.Count == 0)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                List<APIContent> feedContent = await _feedContentRepository.GetContentByFeedTableId(DecryptedId);

                this._feedRepository.DeleteWallFeed(DecryptedId);
                this._feedRepository.DeleteImages(feedContent);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [Route("Like/{Id}")]
        public async Task<IActionResult> GetFeedLike(int Id)
        {
            try
            {
                return Ok(await this._feedLikeRepository.GetNumberOfLikes(Id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("Like/{FeedTableId}")]
        public async Task<IActionResult> PostFeedLike(int FeedTableId)
        {
            try
            {
                APILike likeResponse = new APILike();
                likeResponse.Id = FeedTableId;

                FeedLike feedLike = new FeedLike();
                feedLike.FeedTableId = FeedTableId;
                feedLike.UserId = UserId;
                feedLike.CreatedDate = DateTime.Now;
                feedLike.ModifiedDate = DateTime.Now;
                feedLike.CreatedBy = UserId;
                feedLike.ModifiedBy = UserId;

                List<FeedLike> like = await this._feedLikeRepository.ValidateLike(feedLike);
                if (like.Count == 0)
                {
                    await this._feedLikeRepository.Add(feedLike);
                    likeResponse.SelfLiked = true;
                    likeResponse.Likes = await this._feedLikeRepository.GetNumberOfLikes(likeResponse.Id);
                }
                else
                {
                    await this._feedLikeRepository.Remove(like[0]);
                    likeResponse.SelfLiked = false;
                    likeResponse.Likes = await this._feedLikeRepository.GetNumberOfLikes(likeResponse.Id);
                }
                    

                return Ok(likeResponse);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpGet]
        [Route("Comments/{FeedTableId}")]
        public async Task<IActionResult> GetFeedComments(int FeedTableId)
        {
            try
            {

                return Ok(await this._feedCommentsRepository.GetComments(FeedTableId,UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("Comments")]
        public async Task<IActionResult> PostFeedComments([FromBody]FeedComments commentsData)
        {
            try
            {
                APICommentNumber commentResponse = new APICommentNumber();
                FeedComments feedComments = new FeedComments();
                feedComments.Comment = commentsData.Comment;
                feedComments.FeedTableId = commentsData.FeedTableId;
                feedComments.UserId = UserId;
                feedComments.CreatedDate = DateTime.Now;
                feedComments.ModifiedDate = DateTime.Now;
                feedComments.CreatedBy = UserId;
                feedComments.ModifiedBy = UserId;

                await this._feedCommentsRepository.Add(feedComments);
                APIComment aPIComment = await this._feedCommentsRepository.GetSingleComment(feedComments.UserId, feedComments.CreatedDate);
                commentResponse.numberofcomments = await this._feedLikeRepository.GetNumberOfComments(aPIComment.FeedTableId);
                commentResponse.Id = aPIComment.Id;
                commentResponse.FeedTableId = aPIComment.FeedTableId;
                return Ok(commentResponse);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("CommentsLike/{FeedCommentsId}")]
        public async Task<IActionResult> PostCommentsLike(int FeedCommentsId)
        {
            try
            {
                APILike likeResponse = new APILike();
                likeResponse.Id = FeedCommentsId;

                FeedCommentsLikeTable feedCommentsLike = new FeedCommentsLikeTable();
                feedCommentsLike.FeedCommentsId = FeedCommentsId;
                feedCommentsLike.UserId = UserId;
                feedCommentsLike.CreatedDate = DateTime.Now;
                feedCommentsLike.ModifiedDate = DateTime.Now;
                feedCommentsLike.CreatedBy = UserId;
                feedCommentsLike.ModifiedBy = UserId;

                List<FeedCommentsLikeTable> commentslike = await this._feedCommentsLikeRepository.ValidateCommentsLike(feedCommentsLike);
                if (commentslike.Count == 0)
                {
                    await this._feedCommentsLikeRepository.Add(feedCommentsLike);
                    likeResponse.SelfLiked = true;
                    likeResponse.Likes = await this._feedCommentsLikeRepository.GetNumberOfCommentsLikes(likeResponse.Id);
                }
                else
                {
                    await this._feedCommentsLikeRepository.Remove(commentslike[0]);
                    likeResponse.SelfLiked = false;
                    likeResponse.Likes = await this._feedCommentsLikeRepository.GetNumberOfCommentsLikes(likeResponse.Id);
                }


                return Ok(likeResponse);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        //Function
        private async Task<int> RetureFeedId(string Caption,string Type)
        {
                Feed feed = new Feed(); 
                feed.UserId = UserId;
                feed.Caption = Caption;
                feed.Type = Type;
                feed.CreatedDate = DateTime.Now;
                feed.ModifiedDate = DateTime.Now;
                feed.CreatedBy = UserId;
                feed.ModifiedBy = UserId;

                DateTime feedTime = feed.CreatedDate;
                int feedUserId = feed.UserId;

                await this._feedRepository.Add(feed);
                List<Feed> feeds = this._feedRepository.GetFeedId(feedUserId, feedTime);

                return feeds[0].Id;
        }

        private async Task AddContent(int FeedTableId, string location)
        {
            FeedContent feedContent = new FeedContent();
            feedContent.FeedTableId = FeedTableId;
            feedContent.Location = location;
            feedContent.CreatedDate = DateTime.Now;
            feedContent.ModifiedDate = DateTime.Now;
            feedContent.CreatedBy = UserId;
            feedContent.ModifiedBy = UserId;

            await this._feedContentRepository.Add(feedContent);
        }

    }
}
