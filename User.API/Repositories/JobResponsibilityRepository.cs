//======================================
// <copyright file="RoleResponsibilityRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class JobResponsibilityRepository : Repository<JobResponsibility>, IJobResponsibilityRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobResponsibilityRepository));
        private UserDbContext _db;
        public JobResponsibilityRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<JobResponsibility>> GetAllRoleResponsibility(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from roleResponsibility in context.JobResponsibility
                                  where roleResponsibility.IsDeleted == Record.NotDeleted
                                  select new JobResponsibility
                                  {
                                      Id = roleResponsibility.Id,
                                      UserId = roleResponsibility.UserId,
                                      ResponsibileUserId = roleResponsibility.ResponsibileUserId,
                                      ModifiedBy = roleResponsibility.ModifiedBy,
                                      ModifiedDate = roleResponsibility.ModifiedDate
                                  });
                    if (!string.IsNullOrEmpty(search))
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            switch (columnName.ToLower())
                            {
                                case "userid":
                                    result = result.Where(c => Convert.ToString(c.UserId).StartsWith(search));
                                    break;
                                case "responsibileUserid":
                                    result = result.Where(c => Convert.ToString(c.ResponsibileUserId).StartsWith(search));
                                    break;
                            }
                        }

                    if (page != -1)
                    {
                        result = result.Skip((page - 1) * pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    if (pageSize != -1)
                    {
                        result = result.Take(pageSize);
                        result = result.OrderByDescending(v => v.Id);
                    }

                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.JobResponsibility.Where(r => (Convert.ToString(r.UserId).StartsWith(search) || Convert.ToString(r.ResponsibileUserId).StartsWith(search)) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.JobResponsibility.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<bool> Exist(string jobDescription)
        {

            var count = await this._db.JobResponsibility.Where(r => string.Equals(Convert.ToString(r.UserId), jobDescription, StringComparison.CurrentCultureIgnoreCase)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<int> GetTotalRoleResponsibilityCount()
        {
            return await this._db.JobResponsibility.CountAsync();
        }

        public async Task<IEnumerable<JobResponsibility>> Search(string q)
        {
            var result = (from roleResponsi in this._db.JobResponsibility
                          where ((Convert.ToString(roleResponsi.UserId).StartsWith(q) || Convert.ToString(roleResponsi.UserId).StartsWith(q) || Convert.ToString(roleResponsi.Id).StartsWith(q) || Convert.ToString(roleResponsi.ResponsibileUserId).StartsWith(q)) && roleResponsi.IsDeleted == 0)
                          select roleResponsi).ToListAsync();
            return await result;
        }

        public async Task<IEnumerable<APIJobResponsibility>> GetAllJobResponsibility()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.UserId equals userMaster.Id
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId
                                  where jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,
                                      UserName = userMaster.UserName,
                                      User = userMaster.UserId,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,
                                      JobResponsibilityDetailId = jobResponsibilityDetail.Id,
                                      JobResponsibilityId = jobResponsibilityDetail.JobResponsibilityId
                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIJobResponsibility>> GetAllRecordJobResponsibility(int page, int pageSize, string search = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.UserId equals userMaster.Id
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId
                                  where jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,
                                      UserName = userMaster.UserName,
                                      User = userMaster.UserId,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,
                                      JobResponsibilityDetailId = jobResponsibilityDetail.Id,
                                      JobResponsibilityId = jobResponsibilityDetail.JobResponsibilityId
                                  });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => a.UserName.StartsWith(search) || a.User.StartsWith(search) || Convert.ToString(a.UserId).StartsWith(search));
                    }
                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);

                    if (pageSize != -1)
                        result = result.Take(pageSize);

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;


        }

        public async Task<IEnumerable<APIJobResponsibility>> GetKeyRoleResponsibility(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.UserId equals userMaster.Id
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId
                                  where (jobResponsibility.Id == id) && jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,
                                      UserName = userMaster.UserName,
                                      User = userMaster.UserId,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,
                                      JobResponsibilityDetailId = jobResponsibilityDetail.Id,
                                      JobResponsibilityId = jobResponsibilityDetail.JobResponsibilityId
                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<APIJobResponsibility> JobResponsibilityUser(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  where (jobResponsibility.Id == Convert.ToInt32(id)) && jobResponsibility.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,

                                  });
                    return await result.SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIJobResponsibility>> GetKeyRoleResponsibilityByUserId(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.UserId equals userMaster.Id
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId
                                  where (jobResponsibility.ResponsibileUserId == id) && jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,
                                      UserName = userMaster.UserName,
                                      User = userMaster.UserId,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,
                                      JobResponsibilityDetailId = jobResponsibilityDetail.Id,
                                      JobResponsibilityId = jobResponsibilityDetail.JobResponsibilityId
                                  });
                    return await result.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APIJobResponsibility>> GetAllRecordJobResponsibilityDescription(int userid)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.ModifiedBy equals userMaster.Id
                                  join userMasterD in context.UserMaster on jobResponsibility.ModifiedBy equals userMasterD.Id
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId
                                  where (jobResponsibilityDetail.IsDeleted == Record.NotDeleted && jobResponsibility.UserId == userid)
                                  select new APIJobResponsibility
                                  {
                                      Id = jobResponsibility.Id,
                                      UserId = jobResponsibility.UserId,
                                      ResponsibileUserId = jobResponsibility.ResponsibileUserId,
                                      UserName = userMaster.UserName,
                                      User = userMasterD.UserName,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,
                                      JobResponsibilityDetailId = jobResponsibilityDetail.Id,
                                      JobResponsibilityId = jobResponsibilityDetail.JobResponsibilityId,
                                      Date = jobResponsibilityDetail.ModifiedDate
                                  });


                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;


        }
    }
}
