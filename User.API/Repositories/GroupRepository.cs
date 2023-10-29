//======================================
// <copyright file="GroupRepository.cs" company="Enthralltech Pvt. Ltd.">
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
using User.API.Models;
using log4net;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupRepository));
        private UserDbContext _db;
        public GroupRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<string>> GetGroupNames()
        {
            var result = (from groups in this._db.Group
                          where (groups.IsDeleted == 0)
                          select groups.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return 0;
            var result = await (from groups in this._db.Group.AsNoTracking()
                                where (groups.IsDeleted == 0 && string.Equals(groups.Name, groupName, StringComparison.CurrentCultureIgnoreCase))
                                select groups.Id).FirstOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from groups in this._db.Group
                          orderby groups.Id descending
                          select groups.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetGroupNameById(int? groupId)
        {
            var result = (from Group in this._db.Group
                          where (Group.IsDeleted == Record.NotDeleted && Group.Id == groupId)
                          select Group.Name);
            return await result.AsNoTracking().SingleOrDefaultAsync();
        }
        public async Task<IEnumerable<Group>> GetAllGroups(string search)
        {
            try
            {
                var result = (from Group in this._db.Group
                              where (Group.Name.StartsWith(search) && Group.IsDeleted == Record.NotDeleted)
                              select new Group
                              {
                                  Name = Group.Name,
                                  Id = Group.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<List<Group>> GetGroups()
        {
            try
            {
                var result = (from groups in this._db.Group
                              where (groups.IsDeleted == Record.NotDeleted)
                              select new Group
                              {
                                  Name = groups.Name,
                                  Id = groups.Id

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
