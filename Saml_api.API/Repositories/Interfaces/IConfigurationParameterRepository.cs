using System.Threading.Tasks;

namespace Saml.API.Repositories.Interfaces
{
    public interface IConfigurationParameterRepository
    {
        Task<bool> IsEmailConfigured();
    }
}
