using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class UserOtpRpository : Repository<UserMasterOtp>, IUserOtpRepository
    {
        private UserDbContext _db;
        public UserOtpRpository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<UserMasterOtp>();
        }
    }
}
