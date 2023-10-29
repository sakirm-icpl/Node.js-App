//======================================
// <copyright file="JobAidRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using log4net;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class JobAidRepository : Repository<JobAid>, IJobAidRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobAidRepository));
        private UserDbContext _db;
        public JobAidRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<JobAid>> GetAllJobAidSetting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobAid in context.JobAid
                                  where jobAid.IsDeleted == Record.NotDeleted

                                  select new JobAid
                                  {
                                      Id = jobAid.Id,
                                      ContentId = jobAid.ContentId,
                                      Title = jobAid.Title,
                                      FileType = jobAid.FileType,
                                      Content = jobAid.Content,
                                      AdditionalDescription = jobAid.AdditionalDescription,
                                      KeywordForSearch = jobAid.KeywordForSearch,
                                      ModifiedBy = jobAid.ModifiedBy,
                                      ModifiedDate = jobAid.ModifiedDate
                                  });
                    if (!string.IsNullOrEmpty(search))
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            switch (columnName.ToLower())
                            {
                                case "contentid":
                                    result = result.Where(c => c.ContentId.StartsWith(search));
                                    break;
                                case "title":
                                    result = result.Where(c => c.Title.StartsWith(search));
                                    break;
                                case "additionaldescription":
                                    result = result.Where(c => c.AdditionalDescription.StartsWith(search));
                                    break;
                                case "keywordforsearch":
                                    result = result.Where(c => c.KeywordForSearch.StartsWith(search));
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
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobAid in context.JobAid
                                  join userMaster in context.UserMaster on jobAid.ModifiedBy equals userMaster.Id
                                  where jobAid.IsDeleted == Record.NotDeleted
                                  select new APIJobAid
                                  {
                                      Title = jobAid.Title
                                  });
                    if (!string.IsNullOrEmpty(search))

                        result = result.Where(c => c.Title.StartsWith(search));

                    return await result.AsNoTracking().CountAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        public async Task<bool> Exist(string title)
        {

            var count = await this._db.JobAid.Where(r => r.Title.ToLower() == title.ToLower() && (r.IsDeleted == Record.NotDeleted)).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<int> GetTotalJobAidCount()
        {
            return await this._db.JobAid.CountAsync();
        }

        public async Task<IEnumerable<JobAid>> Search(string q)
        {
            var result = (from jobAid in this._db.JobAid
                          where ((jobAid.ContentId.StartsWith(q) || Convert.ToString(jobAid.Title).StartsWith(q) || Convert.ToString(jobAid.AdditionalDescription).StartsWith(q)) && jobAid.IsDeleted == 0)
                          select jobAid).ToListAsync();
            return await result;
        }


        public async Task<IEnumerable<APIJobAid>> GetAllRecordJobAid()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobAid in context.JobAid
                                  join userMaster in context.UserMaster on jobAid.ModifiedBy equals userMaster.Id
                                  where jobAid.IsDeleted == Record.NotDeleted
                                  select new APIJobAid
                                  {
                                      Id = jobAid.Id,
                                      ContentId = jobAid.ContentId,
                                      Title = jobAid.Title,
                                      FileType = jobAid.FileType,
                                      AdditionalDescription = jobAid.AdditionalDescription,
                                      Content = jobAid.Content,

                                      UserName = userMaster.UserName,
                                      KeywordForSearch = jobAid.KeywordForSearch
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

        public async Task<IEnumerable<APIJobAid>> GetAllJobAid(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobAid in context.JobAid
                                  join userMaster in context.UserMaster on jobAid.ModifiedBy equals userMaster.Id
                                  where jobAid.IsDeleted == Record.NotDeleted
                                  select new APIJobAid
                                  {
                                      Id = jobAid.Id,
                                      ContentId = jobAid.ContentId,
                                      Title = jobAid.Title,
                                      FileType = jobAid.FileType,
                                      AdditionalDescription = jobAid.AdditionalDescription,
                                      Content = jobAid.Content,
                                      UserName = userMaster.UserName,
                                      KeywordForSearch = jobAid.KeywordForSearch
                                  });
                    if (!string.IsNullOrEmpty(search))
                        result = result.Where(c => c.Title.StartsWith(search));

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

        public async Task<IEnumerable<APIJobAid>> GetJobAid(int id)
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from jobAid in context.JobAid
                                  join userMaster in context.UserMaster on jobAid.ModifiedBy equals userMaster.Id
                                  where jobAid.IsDeleted == Record.NotDeleted && (jobAid.Id == id)
                                  select new APIJobAid
                                  {
                                      Id = jobAid.Id,
                                      ContentId = jobAid.ContentId,
                                      Title = jobAid.Title,
                                      FileType = jobAid.FileType,
                                      AdditionalDescription = jobAid.AdditionalDescription,
                                      Content = jobAid.Content,
                                      UserName = userMaster.UserName,
                                      KeywordForSearch = jobAid.KeywordForSearch
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
