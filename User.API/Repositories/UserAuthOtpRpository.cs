using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class UserAuthOtpRpository : Repository<UserAuthOtp>, IUserAuthOtpRepository
    {
        private UserDbContext _db;
        public UserAuthOtpRpository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<UserAuthOtp>();
        }
    }
}
