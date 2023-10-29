using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
