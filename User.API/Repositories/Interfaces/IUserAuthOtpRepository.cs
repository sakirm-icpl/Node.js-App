using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    
    public interface IUserAuthOtpRepository : IRepository<UserAuthOtp>
    {
        void ChangeDbContext(string connectionString);
    }
}
