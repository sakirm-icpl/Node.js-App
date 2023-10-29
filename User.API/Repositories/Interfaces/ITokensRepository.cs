using System.Threading.Tasks;

namespace User.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<int> CheckUserToken(int userId, string token);
        Task<bool> UserTokenExists(string token);
        Task<int> AddFCMToken(int userId, string fcmToken, bool IsIOS = false);
        Task<int> DeleteFCMToken(int userId, string fcmToken);
    }


}
