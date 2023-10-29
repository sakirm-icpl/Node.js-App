using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface ITokensRepository
    {
        Task<bool> UserTokenExists(string token);

        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}
