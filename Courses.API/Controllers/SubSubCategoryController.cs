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
    [Route("api/v1/c/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()] 
    public class SubSubCategoryController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SubSubCategoryController));
        ISubSubCategoryRepository _subSubCategoryRepository;
        private readonly ITokensRepository _tokensRepository;
        public SubSubCategoryController(ISubSubCategoryRepository subSubCategoryRepository, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _subSubCategoryRepository = subSubCategoryRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {

                if (_subSubCategoryRepository.Count() == 0)
                {
                    return NotFound();
                }
                var subSubCategory = await _subSubCategoryRepository.GetAll();
                return Ok(Mapper.Map<List<APICourseSubSubCategory>>(subSubCategory));
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
                if (await _subSubCategoryRepository.Get(id) == null)
                {
                    return NotFound();
                }
                var subCategory = await _subSubCategoryRepository.GetSubSubCategoty(id);
                return Ok(Mapper.Map<List<APICourseSubCategory>>(subCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSubSubCategoryBySubCategory/{subcategory:int}")]
        [Produces(typeof(List<APICourseSubSubCategory>))]
        public async Task<IActionResult> GetCategory(int subcategory)
        {
            try
            {


                var subSubCategory = await _subSubCategoryRepository.GetSubCategory(subcategory);
                return Ok(Mapper.Map<List<APICourseSubSubCategory>>(subSubCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("GetAllSubSubCategoryBySubCategory/{subcategory?}")]
        [Produces(typeof(List<APICourseSubSubCategory>))]
        public async Task<IActionResult> GetAllCategory(int? category = null)
        {
            try
            {


                var subSubCategory = await _subSubCategoryRepository.GetAllSubSubcategories(category);
                return Ok(Mapper.Map<List<APICourseSubSubCategory>>(subSubCategory));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}/{categoryId?}/{subcategoryId?}")]
        [Produces(typeof(List<APICourseSubSubCategory>))]
        public async Task<IActionResult> Get(int? page, int? pageSize, string search = null, int? categoryId = null,int? subcategoryId = null)
        {
            try
            {
                if (page == null || pageSize == null)
                {
                    page = -1;
                    pageSize = -1;
                }

                var subSubCategory = await _subSubCategoryRepository.Get(page.Value, pageSize.Value, search, categoryId,subcategoryId);
                var subSubCategoryList = Mapper.Map<List<APICourseSubSubCategory>>(subSubCategory);
                return Ok(subSubCategoryList);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Unexpected), Description = EnumHelper.GetEnumDescription(MessageType.Unexpected) });
            }
        }

        [HttpGet("count/{search:minlength(0)?}/{categoryId?}/{subcategoryId?}")]
        //[PermissionRequired(Permissions.subsubcategorymanage)]
        public async Task<IActionResult> GetCount(string search = null, int? categoryId = null, int? subcategoryId = null)
        {
            try
            {
                int count = await _subSubCategoryRepository.count(search, categoryId, subcategoryId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
       // [PermissionRequired(Permissions.subsubcategorymanage)]
        public async Task<IActionResult> Post([FromBody] APICourseSubSubCategory subCat)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    if (await _subSubCategoryRepository.Exists(null, subCat.SubCategoryId, subCat.Code, subCat.Name))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    else
                    {
                        SubSubCategory course_SubCat = Mapper.Map<SubSubCategory>(subCat);
                        course_SubCat.CreatedDate = DateTime.UtcNow;
                        course_SubCat.ModifiedDate = DateTime.UtcNow;
                        course_SubCat.SequenceNo = await _subSubCategoryRepository.GetSequenceNo();
                        course_SubCat.IsExternalSubSubCategory = false;
                        await _subSubCategoryRepository.Add(course_SubCat);
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
       // [PermissionRequired(Permissions.subsubcategorymanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APICourseSubSubCategory subCat)
        {
            try
            {
                SubSubCategory course_subCat = await _subSubCategoryRepository.Get(id);
                if (course_subCat == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _subSubCategoryRepository.Exists(id, subCat.SubCategoryId, subCat.Code, subCat.Name))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                if (course_subCat.Code != subCat.Code || course_subCat.SubCategoryId != subCat.SubCategoryId || course_subCat.Id != id)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });


                course_subCat.Code = subCat.Code;
                course_subCat.Id = id;
                course_subCat.SubCategoryId = subCat.SubCategoryId;
                course_subCat.Name = subCat.Name;
                course_subCat.ModifiedDate = DateTime.UtcNow;
                course_subCat.SequenceNo = subCat.SequenceNo;
                await _subSubCategoryRepository.Update(course_subCat);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        //[PermissionRequired(Permissions.subsubcategorymanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                SubSubCategory subSubCategory = await _subSubCategoryRepository.Get(DecryptedId);

                if (subSubCategory == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                if (await _subSubCategoryRepository.IsDependecyExists(DecryptedId) == true)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });

                await _subSubCategoryRepository.Remove(subSubCategory);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UpdateSubSubCategories")]
      //  [PermissionRequired(Permissions.subsubcategorymanage)]
        public async Task<IActionResult> UpdateSubCategories([FromBody] List<APISubSubCategoryDTO> apisubsubcategories)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var Result = await _subSubCategoryRepository.UpdateSubSubCategories(apisubsubcategories);
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
