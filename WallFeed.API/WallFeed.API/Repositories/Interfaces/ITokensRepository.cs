using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);

        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}
