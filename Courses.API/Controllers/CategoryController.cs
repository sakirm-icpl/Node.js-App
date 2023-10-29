using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using Courses.API.Model.Log_API_Count;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CategoryController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CategoryController));
        ICategoryRepository _categoryRepository;
        private readonly ITokensRepository _tokensRepository;
        public CategoryController(ICategoryRepository categoryRepository, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _categoryRepository = categoryRepository;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<Category> Category = new List<Category>();


                var cache = new CacheManager.CacheManager();

                string cacheKeyCategory = OrganisationCode + "_" + Constants.COMPETENCY_CATEGORY;
                if (cache.IsAdded(cacheKeyCategory))
                {
                    Category = cache.Get<List<Category>>(cacheKeyCategory);
                    return Ok(Mapper.Map<List<Category>>(Category));
                }
                else
                {
                    var Categories = await _categoryRepository.GetAllCategoriesBySequenceNo();
                    cache.Add<IEnumerable<Category>>(cacheKeyCategory, Categories);
                    if (Categories == null)
                        return NoContent();
                    return Ok(Categories);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getApplicable")]
        [Produces("application/json")]
        public async Task<IActionResult> getApplicable()
        {
            try
            {
                IEnumerable<Category> Category = new List<Category>();
                var Categories = await _categoryRepository.GetAllApplicableCategories(UserId);

                if (Categories == null)
                    return NoContent();
                return Ok(Categories);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getApplicableSubCategories/{CategoryId}")]
        [Produces("application/json")]
        public async Task<IActionResult> getApplicableSubcategories(int CategoryId)
        {
            try
            {
                IEnumerable<SubCategory> Category = new List<SubCategory>();
                var Categories = await _categoryRepository.GetAllApplicableSubCategories(UserId, CategoryId);

                if (Categories == null)
                    return NoContent();
                return Ok(Categories);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("GetCategoryTypeAhead/{search?}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string search = null)
        {
            try
            {
                List<Category> course = await _categoryRepository.GetCategoryTypeAhead(search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetSubCategoryTypeAhead/{CategoryId}/{search?}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetSubCategory(int CategoryId, string search = null)
        {
            try
            {
                List<SubCategory> course = await _categoryRepository.GetSubCategoryTypeAhead(CategoryId, search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (await _categoryRepository.Get(id) == null)
                {
                    return NotFound();
                }

                var category = await _categoryRepository.Get(id);
                return Ok(Mapper.Map<APICourseCategory>(category));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [Produces(typeof(List<APICourseCategory>))]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var category = await _categoryRepository.Get(page, pageSize, search);
                return Ok(Mapper.Map<List<APICourseCategory>>(category));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("count/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> GetCount(string search = null)
        {
            try
            {
                int count = await _categoryRepository.count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> Post([FromBody] APICourseCategory cat)
        {
            try
            {
                var cache = new CacheManager.CacheManager();

                string cacheKeyCategory = OrganisationCode + "_" + Constants.COMPETENCY_CATEGORY;
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (await _categoryRepository.Exists(null, cat.Code, cat.Name))
                    return StatusCode(409, "Duplicate");
                Category course_cat = Mapper.Map<Category>(cat);
                course_cat.CreatedDate = DateTime.UtcNow;
                course_cat.ModifiedDate = DateTime.UtcNow;
                course_cat.SequenceNo = await _categoryRepository.GetSequenceNo();
                await _categoryRepository.Add(course_cat);
                if (cache.IsAdded(cacheKeyCategory))
                {
                    cache.Remove(cacheKeyCategory);
                    List<Category> cat1 = await _categoryRepository.GetallCategories();
                    cache.Add<IEnumerable<Category>>(cacheKeyCategory, cat1);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("{id}")]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APICourseCategory cat)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var cache = new CacheManager.CacheManager();
                string cacheKeyCategory = OrganisationCode + "_" + Constants.COMPETENCY_CATEGORY;
                Category course_cat = await _categoryRepository.Get(id);
                if (course_cat == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (await _categoryRepository.Exists(id, cat.Code, cat.Name))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                course_cat.Code = cat.Code;
                course_cat.Id = id;
                course_cat.Name = cat.Name;
                course_cat.ImagePath = cat.ImagePath;
                course_cat.ModifiedDate = DateTime.UtcNow;
                await _categoryRepository.Update(course_cat);
                if (cache.IsAdded(cacheKeyCategory))
                {
                    cache.Remove(cacheKeyCategory);
                    List<Category> cat1 = await _categoryRepository.GetallCategories();
                    cache.Add<IEnumerable<Category>>(cacheKeyCategory, cat1);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UpdateCategories")]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> UpdateCategories([FromBody] List<APICategoryDTO> apiCategories)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var Result = await _categoryRepository.UpdateCategories(apiCategories);
                if (Result == Message.InvalidModel)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.categorymanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                var cache = new CacheManager.CacheManager();

                string cacheKeyCategory = OrganisationCode + "_" + Constants.COMPETENCY_CATEGORY;
                Category cat = await _categoryRepository.Get(DecryptedId);
                if (cat == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _categoryRepository.IsDependacyExist(DecryptedId))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                await _categoryRepository.Remove(cat);
                if (cache.IsAdded(cacheKeyCategory))
                {
                    cache.Remove(cacheKeyCategory);
                    List<Category> cat1 = await _categoryRepository.GetallCategories();
                    cache.Add<IEnumerable<Category>>(cacheKeyCategory, cat1);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCategories/{id}")]
        public async Task<IActionResult> GetCategories(int id)
        {
            var category = await _categoryRepository.GetCategoies(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpGet("GetTnaCategories")]
        public async Task<IActionResult> GetTnaCategories()
        {
            var category = await _categoryRepository.GetTnaCategories();
            return Ok(category);
        }
    }
}
