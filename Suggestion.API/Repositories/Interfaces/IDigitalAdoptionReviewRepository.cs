using Suggestion.API.APIModel;

using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IDigitalAdoptionReviewList : IRepository<DigitalAdoptionReview>
    {
        Task<APIDigitalAdoptionReviewsListandCount> GetDigitalAdoptionReview(int page, int pageSize, string search);
        Task<APIDigitalAdoptionReviewDashBoard> DigitalAdoptionReviewDashboard(APIFilter filterData);
        Task<UserDigitalAdoptionReview> UserDigitalAdoption(int UserId);
        Task<APIResponse> ProcessImportFile(APIDARImport aPIDataMigration, int UserId, string OrgCode, string UserName);
    }
}