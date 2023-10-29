using Gadget.API.APIModel;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IUseCasesList : IRepository<UseCase>
    {
        Task<APIUseCasesListandCount> GetUseCases(int page, int pageSize, string search);
    }
}