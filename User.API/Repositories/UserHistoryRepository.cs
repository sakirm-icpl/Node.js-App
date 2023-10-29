//======================================
// <copyright file="UserHistoryRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class UserHistoryRepository : Repository<UserHistory>, IUserHistoryRepository
    {
        private UserDbContext _db;
        public UserHistoryRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<APIUserHistory> GetuserHistory(int userId)
        {
            UserHistory history = await this._db.UserHistory.Where(u => u.RowId == userId).OrderByDescending(s => s.Id).FirstOrDefaultAsync();
            APIUserHistory apiUserHistory = new APIUserHistory();
            if (history != null)
            {
                apiUserHistory.After = JsonConvert.DeserializeObject(history.After);
                apiUserHistory.Before = JsonConvert.DeserializeObject(history.Before);
                apiUserHistory.Created = history.Created;
                apiUserHistory.Id = history.Id;
                apiUserHistory.RowId = history.RowId;
            }

            return apiUserHistory;
        }

    }
}
