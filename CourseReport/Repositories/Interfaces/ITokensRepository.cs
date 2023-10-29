using System.Threading.Tasks;


namespace CourseReport.API.Repositories.Interface
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);
    }
}
