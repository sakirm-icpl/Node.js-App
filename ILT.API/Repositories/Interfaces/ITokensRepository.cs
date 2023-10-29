using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
