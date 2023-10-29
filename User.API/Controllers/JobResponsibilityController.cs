//======================================
// <copyright file="RoleResponsibilityController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================


using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;
using static User.API.Common.TokenPermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.JobRoleResponsbilities)]
    public class JobResponsibilityController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobResponsibilityController));
        private IJobResponsibilityRepository roleResponsibilityRepository;
        private IJobResponsibilityDetailRepository roleResponsibilityDetailRepository;
        private IRewardsPoint _rewardsPoint;
        private readonly ITokensRepository _tokensRepository;

        public JobResponsibilityController(IJobResponsibilityRepository roleResponsibilityController,
            IJobResponsibilityDetailRepository roleResponsibilityDetailController,
            IRewardsPoint rewardsPoint,
            ITokensRepository tokensRepository,
            IIdentityService identitySvc) : base(identitySvc)
        {
            this.roleResponsibilityRepository = roleResponsibilityController;
            this.roleResponsibilityDetailRepository = roleResponsibilityDetailController;
            this._rewardsPoint = rewardsPoint;
            this._tokensRepository = tokensRepository;
        }
    
        [HttpGet]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetAllJobResponsibility();
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("JobResponsibilityDetail")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IEnumerable<JobResponsibilityDetail>> GetJobResponsibilityDetail()
        {
            try
            {
                return await this.roleResponsibilityDetailRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet("JobResponsibilityDetailRecord")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> GetJobResponsibilityDetailRecord()
        {
            try
            {
                IEnumerable<APIJobResponsibilityDetail> roleResponsibility = await this.roleResponsibilityDetailRepository.GetAllJobResponsibilityDetailRecord();
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetAllRecordJobResponsibility(page, pageSize, search);
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

     
        [HttpGet("GetJobDescription")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobDescription()
        {
            try
            {

                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetAllRecordJobResponsibilityDescription(UserId);
                if (roleResponsibility != null)
                {
                    await this._rewardsPoint.JobResposibilityReadRewardPoint(UserId);
                }
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
       
        [HttpGet("GetJobDescriptionCount")]
        public async Task<IActionResult> GetJobDescriptionCount()
        {
            try
            {

                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetAllRecordJobResponsibilityDescription(UserId);
                int count = 0;
                count = roleResponsibility.Count();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("JobResponsibilityDetail/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> GetJobResponsibilityDetail(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                IEnumerable<JobResponsibilityDetail> jobResponsibilityDetail = await this.roleResponsibilityDetailRepository.GetAllJobResponsibilityDetail(page, pageSize, search);
                return Ok(jobResponsibilityDetail);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var count = await this.roleResponsibilityRepository.Count(search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("JobResponsibilityDetail/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> GetCountJobResponsibilityDetail(string search)
        {
            try
            {
                var count = await this.roleResponsibilityDetailRepository.Count(search);
                return this.Ok(count);
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
                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetKeyRoleResponsibility(id);
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("JobResponsibilityDetail/{id}")]
        public async Task<JobResponsibilityDetail> GetJobResponsibilityDetail(int id)
        {
            try
            {
                return await this.roleResponsibilityDetailRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet("Exists/{id:int}/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public bool Exists(int id, string search)
        {
            try
            {
                return this.roleResponsibilityDetailRepository.Exist(id, search);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        [HttpGet("JobResponsibilityDetail/Exists/id/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<bool> JobResponsibility(int id, string search)
        {
            try
            {
                return await this.roleResponsibilityDetailRepository.ExistJob(id, search);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


       [HttpGet("JobResponsibilityUserRole/{id:int}")]
        public async Task<IActionResult> GetJobResponsibility(int id)
        {
            try
            {
                IEnumerable<APIJobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.GetKeyRoleResponsibilityByUserId(id);
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
      
        [HttpPost]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Post([FromBody] APIJobResponsibilityMerge aPIJobResponsibilityMerge)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    JobResponsibility roleResponsib = new JobResponsibility
                    {
                        UserId = aPIJobResponsibilityMerge.UserId,
                        ResponsibileUserId = aPIJobResponsibilityMerge.ResponsibileUserId,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await this.roleResponsibilityRepository.Add(roleResponsib);

                    List<JobResponsibilityDetail> jobResponsibilityDetails = new List<JobResponsibilityDetail>();
                    foreach (APIJobResponsibilityDetail opt in aPIJobResponsibilityMerge.APIJobResponsibilityDetails)
                    {
                        JobResponsibilityDetail jobResponsibilityDetail = new JobResponsibilityDetail
                        {
                            JobResponsibilityId = roleResponsib.Id,
                            JobDescription = opt.JobDescription,
                            AdditionalDescription = opt.AdditionalDescription,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        jobResponsibilityDetails.Add(jobResponsibilityDetail);
                    }
                    await roleResponsibilityDetailRepository.AddRange(jobResponsibilityDetails);



                    return this.Ok(roleResponsib);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Put(int id, [FromBody] APIJobResponsibilityMerge aPIJobResponsibilityMerge)
        {
            try
            {
                JobResponsibility jobResponsibility = await roleResponsibilityRepository.Get(id);
                if (jobResponsibility == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                APIJobResponsibilityMerge aPIJobResponsibilityMerges = new APIJobResponsibilityMerge();
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    jobResponsibility.UserId = aPIJobResponsibilityMerge.UserId;
                    jobResponsibility.ResponsibileUserId = aPIJobResponsibilityMerge.ResponsibileUserId;
                    jobResponsibility.CreatedBy = UserId;
                    jobResponsibility.CreatedDate = DateTime.UtcNow;
                    jobResponsibility.ModifiedBy = UserId;
                    jobResponsibility.ModifiedDate = DateTime.UtcNow;

                    await roleResponsibilityRepository.Update(jobResponsibility);
                    List<JobResponsibilityDetail> jobResponsibilityDetails = new List<JobResponsibilityDetail>();
                    foreach (APIJobResponsibilityDetail opt in aPIJobResponsibilityMerge.APIJobResponsibilityDetails)
                    {
                        if (opt.Id.HasValue)
                        {
                            JobResponsibilityDetail jobResponsibilityDetail = await roleResponsibilityDetailRepository.Get(opt.Id.Value);
                            if (jobResponsibilityDetail != null)
                            {
                                jobResponsibilityDetail.JobDescription = opt.JobDescription;
                                jobResponsibilityDetail.JobResponsibilityId = jobResponsibility.Id;
                                jobResponsibilityDetail.AdditionalDescription = opt.AdditionalDescription;
                                jobResponsibilityDetail.ModifiedBy = UserId;
                                jobResponsibilityDetail.ModifiedDate = DateTime.UtcNow;
                                jobResponsibilityDetails.Add(jobResponsibilityDetail);
                            }
                        }
                    }
                    await roleResponsibilityDetailRepository.UpdateRange(jobResponsibilityDetails);


                }
                return Ok(aPIJobResponsibilityMerges);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

     
        [HttpPost("JobResponsibilityDetail/{id}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> PutJobResponsibilityDetail(int id, [FromBody] APIJobResponsibilityDetail aPIJobResponsibilityDetail)
        {
            try
            {

                JobResponsibilityDetail jobResponsibilityDetail = await this.roleResponsibilityDetailRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == id);
                if (jobResponsibilityDetail == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && jobResponsibilityDetail != null)
                {
                    jobResponsibilityDetail.JobResponsibilityId = Convert.ToInt32(aPIJobResponsibilityDetail.JobResponsibilityId);
                    jobResponsibilityDetail.JobDescription = aPIJobResponsibilityDetail.JobDescription;
                    jobResponsibilityDetail.AdditionalDescription = aPIJobResponsibilityDetail.AdditionalDescription;
                    jobResponsibilityDetail.ModifiedBy = UserId;
                    jobResponsibilityDetail.ModifiedDate = DateTime.UtcNow;
                    await this.roleResponsibilityDetailRepository.Update(jobResponsibilityDetail);
                }

                return Ok(aPIJobResponsibilityDetail);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

      
        [HttpDelete]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                JobResponsibility roleResponsibility = await this.roleResponsibilityRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == DecryptedId);
                if (roleResponsibility == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                roleResponsibility.IsDeleted = Record.Deleted;
                await this.roleResponsibilityRepository.Update(roleResponsibility);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
      
        [HttpDelete("JobResponsibilityDetail")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> JobResponsibilityDetailDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                JobResponsibilityDetail jobResponsibilityDetail = await this.roleResponsibilityDetailRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == DecryptedId);
                if (jobResponsibilityDetail == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                jobResponsibilityDetail.IsDeleted = Record.Deleted;
                await this.roleResponsibilityDetailRepository.Update(jobResponsibilityDetail);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Search/{q}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<JobResponsibility> roleResponsibility = await this.roleResponsibilityRepository.Search(q);
                return this.Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("JobResponsibilityDetail/Search/{q}")]
        [PermissionRequired(Permissions.JobRoleResponsbilities)]
        public async Task<IActionResult> JobResponsibilityDetailSearch(string q)
        {
            try
            {
                IEnumerable<JobResponsibilityDetail> roleResponsibilityDetail = await this.roleResponsibilityDetailRepository.Search(q);
                return this.Ok(roleResponsibilityDetail);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
