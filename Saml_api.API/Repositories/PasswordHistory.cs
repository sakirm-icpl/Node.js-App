using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saml.API.Data;
using Saml.API.Helper;
using Saml.API.Models;
using Saml.API.Repositories.Interfaces;

namespace Saml.API.Repositories
{
    public class PasswordHistoryRepository : Repository<PasswordHistory>, IPasswordHistory
    {
        private UserDbContext _db;
        public PasswordHistoryRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }

        public async Task<bool> IsOldPassword(int userId, string password)
        {
            password = Helper.Security.EncryptSHA512(password);
            List<string> OldPassword = await this._db.PasswordHistory.Where(Passwords => Passwords.IsDeleted == Record.NotDeleted && Passwords.UserMasterId == userId)
                .OrderByDescending(Pass => Pass.Id).Select(Pass => Pass.Password).Take(5).ToListAsync();
            if (OldPassword.Contains(password))
                return true;
            return false;
        }

        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<PasswordHistory>();
        }
    }
}
