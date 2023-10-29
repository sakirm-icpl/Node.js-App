//======================================
// <copyright file="UserMasterRejectedRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class UserMasterRejectedRepository : Repository<UserMasterRejected>, IUserMasterRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserMasterRejectedRepository));
        private UserDbContext _db;
        public UserMasterRejectedRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<List<APIUserMasterRejected>> GetAllRecord()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from userMasterRejected in context.UserMasterRejected

                                  select new APIUserMasterRejected
                                  {
                                      Id = userMasterRejected.Id,
                                      CustomerCode = userMasterRejected.CustomerCode,
                                      SerialNumber = userMasterRejected.SerialNumber,
                                      UserId = userMasterRejected.UserId,
                                      EmailId = userMasterRejected.EmailId,
                                      UserName = userMasterRejected.UserName

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

        public void delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [User].[UserMasterRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

        }

        public async Task<IEnumerable<UserMasterRejected>> GetAllUserReject(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.UserMasterRejected> Query = this._db.UserMasterRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.UserName.StartsWith(search) || Convert.ToString(v.UserId).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                return await Query.ToListAsync();
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
                return await this._db.UserMasterRejected.Where(r => r.UserName.Contains(search) || Convert.ToString(r.UserId).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.UserMasterRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<List<APIUserReject>> GetAllUsersRejected()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from userMasterRejected in context.UserMasterRejected

                                  select new APIUserReject
                                  {
                                      UserId = userMasterRejected.UserId,
                                      UserName = userMasterRejected.UserName,
                                      EmailId = userMasterRejected.EmailId,
                                      MobileNumber = userMasterRejected.MobileNumber,
                                      ModifiedDate = userMasterRejected.ModifiedDate.ToString("MMM dd, yyyy"),
                                      ErrorMessage = userMasterRejected.ErrorMessage
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
