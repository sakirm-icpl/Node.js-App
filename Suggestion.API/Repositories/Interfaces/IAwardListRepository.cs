using Suggestion.API.APIModel;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IAwardList : IRepository<AwardList>
    {
        Task<APIAwardListsListandCount> GetAwardList(int page, int pageSize, string search);
    }
}