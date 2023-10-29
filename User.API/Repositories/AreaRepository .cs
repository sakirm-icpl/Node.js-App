//======================================
// <copyright file="AreaRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class AreaRepository : Repository<Area>, IAreaRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AreaRepository));
        private UserDbContext _db;
        public AreaRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetAreaNames()
        {
            var result = (from area in this._db.Area
                          where (area.IsDeleted == 0)
                          select area.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string areaName)
        {
            if (string.IsNullOrEmpty(areaName))
                return 0;
            var result = await (from area in this._db.Area.AsNoTracking()
                                where (area.IsDeleted == 0 && string.Equals(area.Name, areaName, StringComparison.CurrentCultureIgnoreCase))
                                select area.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;
            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from area in this._db.Area
                          orderby area.Id descending
                          select area.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }
        public async Task<string> GetAreaNameById(int? locationId)
        {
            var result = (from area in this._db.Area
                          where (area.IsDeleted == Record.NotDeleted && area.Id == locationId)
                          select area.Name);
            return await result.AsNoTracking().SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Area>> GetAllAreas(string search)
        {
            try
            {
                var result = (from area in this._db.Area
                              where (area.Name.StartsWith(search) && area.IsDeleted == Record.NotDeleted)
                              select new Area
                              {
                                  Name = area.Name,
                                  Id = area.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Area>> GetAreas()
        {
            try
            {
                var result = (from area in this._db.Area
                              where (area.IsDeleted == Record.NotDeleted)
                              select new Area
                              {
                                  Name = area.Name,
                                  Id = area.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

    }
}
