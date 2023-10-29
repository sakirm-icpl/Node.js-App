using System.Threading.Tasks;
using Saml.API.Models;

namespace Saml.API.Repositories.Interfaces
{
    public interface IPasswordHistory : IRepository<PasswordHistory>
    {
        Task<bool> IsOldPassword(int userId, string password);
        void ChangeDbContext(string connectionString);
    }
}
