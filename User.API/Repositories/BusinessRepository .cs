//======================================
// <copyright file="BusinessRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class BusinessRepository : Repository<Business>, IBusinessRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BusinessRepository));
        private UserDbContext _db;
        public BusinessRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetBusinessNames()
        {
            var result = (from business in this._db.Business
                          where (business.IsDeleted == 0)
                          select business.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string businessName)
        {
            if (string.IsNullOrEmpty(businessName))
                return 0;
            return await (from business in this._db.Business.AsNoTracking()
                          where (business.IsDeleted == 0 && string.Equals(business.Name, businessName, StringComparison.CurrentCultureIgnoreCase))
                          select business.Id).FirstOrDefaultAsync();

        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from business in this._db.Business
                          orderby business.Id descending
                          select business.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetBuisnessNameById(int? locationId)
        {
            var result = (from Buisness in this._db.Business
                          where (Buisness.IsDeleted == Record.NotDeleted && Buisness.Id == locationId)
                          select Buisness.Name);
            return await result.AsNoTracking().SingleOrDefaultAsync();
        }
        public async Task<IEnumerable<Business>> GetAllBusiness(string search)
        {
            try
            {
                var result = (from business in this._db.Business
                              where (business.Name.StartsWith(search) && business.IsDeleted == Record.NotDeleted)
                              select new Business
                              {
                                  Name = business.Name,
                                  Id = business.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<List<Business>> GetBusiness()
        {
            try
            {
                var result = (from business in this._db.Business
                              where (business.IsDeleted == Record.NotDeleted)
                              select new Business
                              {
                                  Name = business.Name,
                                  Id = business.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            var Query = _db.Business.Where(r => r.IsDeleted == Record.NotDeleted);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.Name.Contains(search) || (r.Code.Contains(search))));
            }
            return await Query.CountAsync();
        }

        public async Task<bool> Exists(string Name, string Code, int? businessid = null)
        {
            Code = Code.ToLower().Trim();
            Name = Name.ToLower().Trim();
            int Count = 0;

            if (businessid != null)
            {
                Count = await (from c in this._db.Business
                               where c.Id != businessid && c.IsDeleted == 0 && (c.Name.ToLower().Equals(Name) || c.Code.ToLower().Equals(Code))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this._db.Business
                               where c.IsDeleted == 0 && (c.Name.ToLower().Equals(Name) || c.Code.ToLower().Equals(Code))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;


        }

        public async Task<List<object>> GetAll(int page, int pageSize, string search = null)
        {
            var Query = _db.Business.Where(r => r.IsDeleted == Record.NotDeleted);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.Name.Contains(search) || (r.Code.Contains(search))));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<Business> businessList = await Query.ToListAsync();

            List<object> Result = new List<object>();
            foreach (Business business in businessList)
            {
                object businessobj = new
                {
                    business.Id,
                    business.Name,
                    business.Code
                };
                Result.Add(businessobj);
            }
            return Result;
        }
        public async Task<bool> IsDependacyExist(int BusinessId)
        {
            int Count = await (from usermasterdetails in _db.UserMasterDetails
                               join business in _db.Business on usermasterdetails.BusinessId equals business.Id
                               where (usermasterdetails.IsDeleted == false && business.Id == BusinessId)
                               select new { business.Id }).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }

    }
}
