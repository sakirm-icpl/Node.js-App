using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class TokenBlacklistRepository : Repository<TokenBlacklist>, ITokenBlacklistRepository
    {
        private UserDbContext _db;
        public TokenBlacklistRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
    }
}
