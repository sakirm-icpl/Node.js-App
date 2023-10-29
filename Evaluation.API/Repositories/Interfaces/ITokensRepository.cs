using System.Threading.Tasks;

namespace Evaluation.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
