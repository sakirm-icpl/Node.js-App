// ======================================
// <copyright file="InterestingArticlesController.cs" company="Enthralltech Pvt. Ltd.">
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
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Publication.API.Common.AuthorizePermissions;
using static Publication.API.Common.TokenPermissions;
using log4net;
using Publication.API.Helper.Log_API_Count;

namespace Publication.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class InterestingArticlesController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InterestingArticlesController));
        private IInterestingArticlesRepository interestingArticlesRepository;
        private IArticleCategoryRepository articleCategoryRepository;
        private readonly IIdentityService _identitySvc;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        private readonly ITokensRepository _tokensRepository;
        ISocialCheckHistoryRepository _socialCheckHistoryRepository;
        public InterestingArticlesController(IInterestingArticlesRepository interestingArticlesController,
            IArticleCategoryRepository articleCategoryController,
            IRewardsPointRepository rewardsPointRepository,
            IIdentityService identitySvc,
            ITokensRepository tokensRepository,
            ISocialCheckHistoryRepository socialCheckHistoryRepository) : base(identitySvc)
        {
            this.interestingArticlesRepository = interestingArticlesController;
            this.articleCategoryRepository = articleCategoryController;
            this._identitySvc = identitySvc;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
            this._socialCheckHistoryRepository = socialCheckHistoryRepository;
        }

        // GET: api/<controller>
        [HttpGet]
        //[PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> Get()
        {
            try
            {

                List<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIInterestingArticles>>(interestingArticles));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET: api/values
        [HttpGet("ArticleCategory/{category}")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchCategory(string category)
        {
            try
            {
                return this.Ok(await this.articleCategoryRepository.Search(category));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ArticleCategoryGetAll")]
        public async Task<IActionResult> GetArticles()
        {
            try
            {
                return Ok(await this.articleCategoryRepository.GetArticles(UserId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ArticleCategoryCreatedBy/{category:minlength(0)?}")]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> GetCategoryCreatedBy(string category, int userId)
        {
            try
            { 
                return Ok(await this.articleCategoryRepository.GetCategoryCreatedBy(category, UserId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ArticleCategoryById/{id:int}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            try
            {
                IEnumerable<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.GetAllInterestingArticlesByCategoryId(id, UserId);
                int userId = UserId;
                await this._rewardsPointRepository.InterestingArticalReadRewardPoint(id, userId, OrgCode);
                return Ok(Mapper.Map<List<APIInterestingArticles>>(interestingArticles));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("ArticleSearch/{id:int}/{search:minlength(0)?}")]
        public async Task<IActionResult> GetArticleBySearch(int id, string search)
        {
            try
            {

                IEnumerable<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.GetAllInterestingArticlesBySearch(id, search);
                return Ok(Mapper.Map<List<APIInterestingArticles>>(interestingArticles));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {

                IEnumerable<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.GetAllInterestingArticles(UserId, UserRole, page, pageSize, search);
                return Ok(Mapper.Map<List<APIInterestingArticles>>(interestingArticles));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {

                int interestingArticles = await this.interestingArticlesRepository.Count(UserId, UserRole, search);
                return Ok(interestingArticles);
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

                InterestingArticles interestingArticles = await this.interestingArticlesRepository.Get(s => s.IsDeleted == false && s.Id == id);
                int userId = UserId;
                //await this._rewardsPointRepository.InterestingArticalRewardPoint(id, userId, OrgCode);
                return Ok(Mapper.Map<APIInterestingArticles>(interestingArticles));
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
                return await this.interestingArticlesRepository.Exist(search);
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;  
            }
        }

        // GET: InterestingArticles/NewArticleToShow
        [HttpGet]
        [Route("NewArticleToShow")]
        public async Task<IActionResult> NewArticleToShow()
        {
            try
            {
                return Ok(await this._socialCheckHistoryRepository.NewArticleToShow(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST: InterestingArticles/ArticleCheckTime
        [HttpPost]
        [Route("ArticleCheckTime")]
        public async Task<IActionResult> NewsCheckTime()
        {
            try
            {
                SocialCheckHistory oldFeedCheckHistory = await this._socialCheckHistoryRepository.CheckForDuplicate(UserId);
                if (oldFeedCheckHistory != null)
                {
                    oldFeedCheckHistory.LastArticleCheckTime = DateTime.UtcNow;
                    await this._socialCheckHistoryRepository.Update(oldFeedCheckHistory);
                }
                else
                {
                    SocialCheckHistory feedCheckHistory = new SocialCheckHistory();
                    feedCheckHistory.UserId = UserId;
                    feedCheckHistory.LastArticleCheckTime = DateTime.UtcNow;
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

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> Post([FromBody]APIInterestingArticles aPIInterestingArticles)
        {

            try
            {
                //aPIInterestingArticles.CreatedBy = UserId;
                // bool validValue = false;
                if (ModelState.IsValid)
                {
                    InterestingArticles interestingArticles = aPIInterestingArticles.MapAPIToInterestingArticles(aPIInterestingArticles);
                    interestingArticles = this.interestingArticlesRepository.GetInterestingArticlesObject(interestingArticles, aPIInterestingArticles,UserId).Result;
                    interestingArticles.Date = DateTime.Now;
                    interestingArticles.CategoryId = interestingArticles.CategoryId;
                    interestingArticles.Category = aPIInterestingArticles.Category;
                    interestingArticles.ShowToAll = aPIInterestingArticles.ShowToAll;
                    interestingArticles.Article = aPIInterestingArticles.Article;
                    interestingArticles.ArticleDescription = aPIInterestingArticles.ArticleDescription;
                    interestingArticles.ValidityDate = aPIInterestingArticles.ValidityDate;
                    interestingArticles.Status = aPIInterestingArticles.Status;
                    interestingArticles.IsDeleted = false;
                    interestingArticles.ModifiedBy = UserId;
                    interestingArticles.ModifiedDate = DateTime.Now;
                    interestingArticles.CreatedBy = UserId;
                    interestingArticles.CreatedDate = DateTime.Now;
                    await interestingArticlesRepository.Add(interestingArticles);
                    return Ok(interestingArticles);

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
        [HttpGet("InterestingArticlesRead/{id:int}")]
        public async Task<IActionResult> PostCounter(int id)
        {

            try
            {
                await _rewardsPointRepository.InterestingArticalReadRewardPoint(UserId, id, OrgCode);
                return Ok();
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // POST api/<controller>
        [HttpGet("InterestingArticlesLike/{id:int}")]
        public async Task<IActionResult> PostCounterLike(int id)
        {

            try
            {
                await _rewardsPointRepository.InterestingArticalLikeDislikeRewardPoint(UserId, id);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> Put(int id, [FromBody]APIInterestingArticles aPIInterestingArticles)
        {


            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                InterestingArticles interestingArticles = await this.interestingArticlesRepository.Get(s => s.IsDeleted == false && s.Id == id);
                if (interestingArticles == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (interestingArticles != null)
                {
                    interestingArticles.Date = aPIInterestingArticles.Date;
                    interestingArticles.CategoryId = aPIInterestingArticles.CategoryId;
                    interestingArticles.Category = aPIInterestingArticles.Category;
                    interestingArticles.Article = aPIInterestingArticles.Article;
                    interestingArticles.ShowToAll = aPIInterestingArticles.ShowToAll;
                    interestingArticles.ArticleDescription = aPIInterestingArticles.ArticleDescription;
                    interestingArticles.ValidityDate = aPIInterestingArticles.ValidityDate;
                    interestingArticles.Status = aPIInterestingArticles.Status;
                    interestingArticles.ModifiedBy = UserId;
                    interestingArticles.ModifiedDate = DateTime.UtcNow;
                    await this.interestingArticlesRepository.Update(interestingArticles);
                }
                return Ok(interestingArticles);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.InterestingArticles)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                InterestingArticles interestingArticles = await this.interestingArticlesRepository.Get(DecryptedId);

                if (ModelState.IsValid && interestingArticles != null)
                {
                    interestingArticles.IsDeleted = true;
                    await this.interestingArticlesRepository.Update(interestingArticles);
                }

                if (interestingArticles == null)
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
        /// Search specific InterestingArticles.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<InterestingArticles> interestingArticles = await this.interestingArticlesRepository.Search(q);
                return Ok(Mapper.Map<List<APIInterestingArticles>>(interestingArticles));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
