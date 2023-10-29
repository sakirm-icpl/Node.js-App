using Survey.API.APIModel;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface ILcmsRepository
    {
        Task<int> PostLcms(ApiLcms apiLcms);
        Task<int> UpdateLcms(ApiLcms apiLcms);
        Task<ApiLcms> GetLcms(int lcmsId);
    }
}
