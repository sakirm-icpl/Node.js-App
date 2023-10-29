using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);

        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}
