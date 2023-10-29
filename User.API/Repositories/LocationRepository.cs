//======================================
// <copyright file="LocationRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LocationRepository));
        private UserDbContext _db;
        public LocationRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetLocationNames()
        {
            var result = (from Location in this._db.Location
                          where (Location.IsDeleted == 0)
                          select Location.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string locationName)
        {
            if (string.IsNullOrEmpty(locationName))
                return 0;
            return await (from Location in this._db.Location
                          where (Location.IsDeleted == 0 && string.Equals(Location.Name, locationName, StringComparison.CurrentCultureIgnoreCase))
                          select Location.Id).FirstOrDefaultAsync();
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from Location in this._db.Location
                          orderby Location.Id descending
                          select Location.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetLocationNameById(int? locationId)
        {
            var result = (from Location in this._db.Location
                          where (Location.IsDeleted == Record.NotDeleted && Location.Id == locationId)
                          select Location.Name);
            return await result.AsNoTracking().SingleOrDefaultAsync();
        }
        public async Task<IEnumerable<Location>> GetAllLocations(string search)
        {
            try
            {
                var result = (from location in this._db.Location
                              where (location.Name.StartsWith(search) && location.IsDeleted == Record.NotDeleted)
                              select new Location
                              {
                                  Name = location.Name,
                                  Id = location.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Location>> GetLocations()
        {
            try
            {
                var result = (from location in this._db.Location
                              where (location.IsDeleted == Record.NotDeleted)
                              select new Location
                              {
                                  Name = location.Name,
                                  Id = location.Id

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
