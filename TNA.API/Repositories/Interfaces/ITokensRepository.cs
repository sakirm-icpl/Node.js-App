using System.Threading.Tasks;

namespace TNA.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
