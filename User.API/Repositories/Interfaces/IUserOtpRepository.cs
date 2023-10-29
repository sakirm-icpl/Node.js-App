using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserOtpRepository : IRepository<UserMasterOtp>
    {
        void ChangeDbContext(string connectionString);
    }
}
