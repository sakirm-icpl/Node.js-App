using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;

namespace User.API.Repositories.Interfaces
{
    public interface IRewardsPoint
    {
        Task<int> AddLoginRewardPoints(int userId);
        Task<IEnumerable<APIRanking>> GetTopRanking(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string OrgCode = null, string configuredColumnValue = null, string isinstitute = "false");
        Task<APILeaderBoardData> GetLeaderBoardData(string UserID);
        Task<int> JobAidReadRewardPoint(int userId, int jobAidId);
        Task<int> JobResposibilityReadRewardPoint(int userId);
        Task<int> KeyAreaSettingsReadRewardPoint(int userId);
        Task<int> AddDailyLoginRewardPoints(int userId);
        Task<int> GetSevenDayCount(int userId);
        Task<int> AddSevenDayPoints(int userId);
        Task<APIRewardPointCount> GetHouseRewardPointCount();
        Task<IEnumerable<APIRanking>> GetMyRanking(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string isinstitute = "false");
        Task<int> AddFirstTimeLoginRewardPoints(int userId);
        Task<IEnumerable<APIRankingExport>> GetTopRankingForExport(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string OrgCode = null, string configuredColumnValue = null, string isinstitute = "false");

        Task<int> FirstTimeLoggedInLogin(int UserId);
        Task<int> DailyLoggedInLogin(int UserId);
        Task<int> AddKeySetting(int userId, string Title);
        Task<int> AddJobAid(int userId, string Title);
        Task<List<APIRewardLeaderBoard>> GetDatewiseLeaderBoardData(APIRewardLeaderBoardDate APIRewardPointsSummery, int UserId);
    }
}
