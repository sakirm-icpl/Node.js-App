using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserOtpHistoryRepository : IRepository<UserMasterOtpHistory>
    {
        void ChangeDbContext(string connectionString);
    }
   
}
