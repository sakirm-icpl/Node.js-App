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

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class SubCategoryController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SubCategoryController));
        ISubCategoryRepository _subCategoryRepository;
        private readonly ITokensRepository _tokensRepository;
        public SubCategoryController(ISubCategoryRepository subCategoryRepository, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _subCategoryRepository = subCategoryRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {

                if (_subCategoryRepository.Count() == 0)
                {
                    return NotFound();
                }
                var subCategory = await _subCategoryRepository.GetAll();
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                if (await _subCategoryRepository.Get(id) == null)
                {
                    return NotFound();
                }
                var subCategory = await _subCategoryRepository.GetSubCategoty(id);
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSubCategoryByCategory/{category:int}")]
        [Produces(typeof(List<APICourseSubCategory>))]
        public async Task<IActionResult> GetCategory(int category)
        {
            try
            {


                var subCategory = await _subCategoryRepository.GetCategory(category);
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("GetAllSubCategoryByCategory/{category?}")]
        [Produces(typeof(List<APICourseSubCategory>))]
        public async Task<IActionResult> GetAllCategory(int? category = null)
        {
            try
            {


                var subCategory = await _subCategoryRepository.GetAllSubcategories(category);
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}/{categoryId?}")]
        [Produces(typeof(List<APICourseSubCategory>))]
        public async Task<IActionResult> Get(int? page, int? pageSize, string search = null, int? categoryId = null)
        {
            try
            {
                if (page == null || pageSize == null)
                {
                    page = -1;
                    pageSize = -1;
                }

                var subCategory = await _subCategoryRepository.Get(page.Value, pageSize.Value, search, categoryId);
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("count/{search:minlength(0)?}/{categoryId?}")]
        [PermissionRequired(Permissions.subcategorymanage)]
        public async Task<IActionResult> GetCount(string search = null, int? categoryId = null)
        {
            try
            {
                int count = await _subCategoryRepository.count(search, categoryId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.subcategorymanage)]
        public async Task<IActionResult> Post([FromBody] APICourseSubCategory subCat)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    if (await _subCategoryRepository.Exists(null, subCat.CategoryId, subCat.Code, subCat.Name))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    else
                    {
                        SubCategory course_SubCat = Mapper.Map<SubCategory>(subCat);
                        course_SubCat.CreatedDate = DateTime.UtcNow;
                        course_SubCat.ModifiedDate = DateTime.UtcNow;
                        course_SubCat.SequenceNo = await _subCategoryRepository.GetSequenceNo();
                        await _subCategoryRepository.Add(course_SubCat);
                    }

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
        [PermissionRequired(Permissions.subcategorymanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APICourseSubCategory subCat)
        {
            try
            {
                SubCategory course_subCat = await _subCategoryRepository.Get(id);
                if (course_subCat == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _subCategoryRepository.Exists(id, subCat.CategoryId, subCat.Code, subCat.Name))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                if (course_subCat.Code != subCat.Code || course_subCat.CategoryId != subCat.CategoryId || course_subCat.Id != id)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });


                course_subCat.Code = subCat.Code;
                course_subCat.Id = id;
                course_subCat.CategoryId = subCat.CategoryId;
                course_subCat.Name = subCat.Name;
                course_subCat.ModifiedDate = DateTime.UtcNow;
                course_subCat.SequenceNo = subCat.SequenceNo;
                await _subCategoryRepository.Update(course_subCat);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.subcategorymanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                SubCategory subCategory = await _subCategoryRepository.Get(DecryptedId);

                if (subCategory == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                if (await _subCategoryRepository.IsDependecyExists(DecryptedId) == true)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });

                await _subCategoryRepository.Remove(subCategory);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UpdateSubCategories")]
        [PermissionRequired(Permissions.subcategorymanage)]
        public async Task<IActionResult> UpdateCategories([FromBody] List<APISubCategoryDTO> apisubcategories)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var Result = await _subCategoryRepository.UpdateSubCategories(apisubcategories);
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
    }
}
