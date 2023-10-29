//======================================
// <copyright file="HrmsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using log4net;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class HRMSRepository : Repository<HRMS>, IHRMSRepository
    {

        private UserDbContext _db;
        public HRMSRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<HRMS>> GetAllHrms(int page, int pageSize, string search = null)
        {
            IQueryable<HRMS> query = this._db.Hrms;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.ChangedName.StartsWith(search) && r.IsDeleted == Record.NotDeleted);
            }
            else
            {
                query = query.Where(r => r.IsDeleted == Record.NotDeleted);
            }
            if (page != -1)
                query = query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                query = query.Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.Hrms.Where(r => r.ChangedName.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.Hrms.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<string>> GetColumnNames(string search = null)
        {
            var columns = this._db.Hrms.Where(r => r.IsDeleted == Record.NotDeleted).Select(h => h.ColumnName);
            return await columns.ToListAsync();
        }
        public async Task<bool> IsColumnExist(string columnName = null)
        {
            int count = await this._db.Hrms.Where(r => r.IsDeleted == Record.NotDeleted && string.Equals(r.ColumnName, columnName, StringComparison.CurrentCultureIgnoreCase)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
    }
}
