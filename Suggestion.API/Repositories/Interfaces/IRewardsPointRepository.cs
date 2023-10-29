using System;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IRewardsPointRepository
    {
        Task<int> RewardPointSave(string functionCode, string category, int referenceId, int userId, int? point = null, string description = null);
        Task<int> AddSurveySubmitReward(int userId, int surveyId, int IsFirstSurvey,string SurveySubject, DateTime CreatedDate, string OrgCode);
        Task<int> AlbumReadRewardPoint(int userId, int albumId,string OrgCode);
        Task<int> PublicationReadRewardPoint(int userId, int publicationId,string OrgCode);
        Task<int> InterestingArticalReadRewardPoint(int userId, int articalId,string OrgCode);
        Task<int> InterestingArticalLikeDislikeRewardPoint(int userId, int articalId);
        Task<int> PollsResponseRewardPoint(int userId, int pollId,string Question, string OrgCode);
        Task<int> QuizAttemptedRewardPoint(int userId, int quizId, bool IsFullMarks,string QuizTitle, DateTime CreatedDate, string OrgCode);
        Task<int> MySuggestionSubmitRewardPoint(int userId,int referenceId, string ContextualAreaofBusiness, string OrgCode);
        Task<int> AddNewsUpdateReadReward(int id, int userId,string SubHead ,string OrgCode);
        Task<int> SuggestionManagementRewardPoint(int userId, int suggestionId,string Suggestion, string OrgCode);
        Task<int> MySuggestionRewardPoint(int userId, int suggestionId,string DetailedDescription, string OrgCode);
        Task<string> RewardPointsDescription(string Category, string Condition, string FunctionCode);
        Task<int> InterestingArticalRewardPoint(int id, int userId, string OrgCode);
    }
}
