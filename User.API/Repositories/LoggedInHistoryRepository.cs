using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class LoggedInHistoryRepository : Repository<LoggedInHistory>, ILoggedInHistoryRepository
    {
        private UserDbContext _db;
        public LoggedInHistoryRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<LoggedInHistory> GetLatestRecord(int userId)
        {
            LoggedInHistory loggedInHistory = await this._db.LoggedInHistory.Where(record => record.UserMasterId == userId && record.LogOutTime == null).OrderByDescending(record => record.Id).FirstOrDefaultAsync();
            return loggedInHistory;
        }

        public async Task<int> AlreadyLoggedInForDay(int userId)
        {
            DateTime datetimeNow = DateTime.Now;

            return await this._db.LoggedInHistory.Where(s => s.UserMasterId == userId && s.LoggedInTime.Date == datetimeNow.Date && s.LoggedInTime.Month == datetimeNow.Month && s.LoggedInTime.Year == datetimeNow.Year).CountAsync();
        }

        public async Task<int> FirstTimeLoggedIn(int userId)
        {
            return await this._db.LoggedInHistory.Where(s => s.UserMasterId == userId).CountAsync();
        }

    }
}
