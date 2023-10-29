//======================================
// <copyright file="JobResponsibilityDetailRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class JobResponsibilityDetailRepository : Repository<JobResponsibilityDetail>, IJobResponsibilityDetailRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobResponsibilityDetailRepository));
        private UserDbContext _db;
        public JobResponsibilityDetailRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<int> Count(string search = null)
        {

            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibility in context.JobResponsibility
                                  join userMaster in context.UserMaster on jobResponsibility.UserId equals userMaster.Id into ps
                                  from userMaster in ps.DefaultIfEmpty()
                                  join jobResponsibilityDetail in context.JobResponsibilityDetail on jobResponsibility.Id equals jobResponsibilityDetail.JobResponsibilityId into ps2
                                  from jobResponsibilityDetail in ps2.DefaultIfEmpty()
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

                    return await result.CountAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;

        }

        public async Task<IEnumerable<JobResponsibilityDetail>> GetAllJobResponsibilityDetail(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from JobResponsibilityDetail in context.JobResponsibilityDetail
                                  where JobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new JobResponsibilityDetail
                                  {
                                      Id = JobResponsibilityDetail.Id,
                                      JobResponsibilityId = JobResponsibilityDetail.JobResponsibilityId,
                                      JobDescription = JobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = JobResponsibilityDetail.AdditionalDescription,
                                      ModifiedBy = JobResponsibilityDetail.ModifiedBy,
                                      ModifiedDate = JobResponsibilityDetail.ModifiedDate
                                  });
                    if (!string.IsNullOrEmpty(search))
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            switch (columnName.ToLower())
                            {
                                case "jobdescription":
                                    result = result.Where(c => Convert.ToString(c.JobDescription).StartsWith(search));
                                    break;
                                case "additionaldescription":
                                    result = result.Where(c => Convert.ToString(c.AdditionalDescription).StartsWith(search));
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
        public async Task<IEnumerable<JobResponsibilityDetail>> Search(string q)
        {
            var result = (from jobResponsibilityDetail in this._db.JobResponsibilityDetail
                          where ((Convert.ToString(jobResponsibilityDetail.JobDescription).StartsWith(q) || Convert.ToString(jobResponsibilityDetail.JobResponsibilityId).StartsWith(q) || Convert.ToString(jobResponsibilityDetail.Id).StartsWith(q) || Convert.ToString(jobResponsibilityDetail.JobResponsibilityId).StartsWith(q)) && jobResponsibilityDetail.IsDeleted == 0)
                          select jobResponsibilityDetail).ToListAsync();
            return await result;
        }
        public bool Exist(int id, string search)
        {
            if (this._db.JobResponsibilityDetail.Count(x => x.JobDescription == search && x.JobResponsibilityId == id) > 0)
                return true;
            return false;
        }
        public async Task<IEnumerable<APIJobResponsibilityDetail>> GetAllJobResponsibilityDetailRecord()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibilityDetail in context.JobResponsibilityDetail
                                  join userMaster in context.UserMaster on jobResponsibilityDetail.ModifiedBy equals userMaster.Id
                                  where jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibilityDetail
                                  {
                                      Id = jobResponsibilityDetail.Id,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,

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
        public async Task<bool> ExistJob(int id, string search)
        {
            var count = await this._db.JobResponsibilityDetail.Where(p => String.Equals(p.JobDescription, search, StringComparison.CurrentCultureIgnoreCase) && Convert.ToString(p.JobResponsibilityId).Contains(Convert.ToString(id)) && p.IsDeleted == Record.NotDeleted).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<IEnumerable<APIJobResponsibilityDetail>> GetAllRoleResponsibility(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobResponsibilityDetail in context.JobResponsibilityDetail
                                  where (jobResponsibilityDetail.JobResponsibilityId == id) && jobResponsibilityDetail.IsDeleted == Record.NotDeleted
                                  select new APIJobResponsibilityDetail
                                  {
                                      Id = jobResponsibilityDetail.Id,
                                      JobDescription = jobResponsibilityDetail.JobDescription,
                                      AdditionalDescription = jobResponsibilityDetail.AdditionalDescription,

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
    }
}
