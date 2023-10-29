using Courses.API.APIModel;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IGamificationRepository
    {
        Task<ApiResponse> GetMissionCounts(int userId);
        Task<ApiResponse> GetDaywiseMissionCounts(int userId, int noOfDays, string missionType = null);
    }
}
