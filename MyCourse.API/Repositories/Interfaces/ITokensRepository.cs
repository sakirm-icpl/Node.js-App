using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
