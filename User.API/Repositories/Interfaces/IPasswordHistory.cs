using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IPasswordHistory : IRepository<PasswordHistory>
    {
        Task<bool> IsOldPassword(int userId, string password);
        void ChangeDbContext(string connectionString);
    }
}
