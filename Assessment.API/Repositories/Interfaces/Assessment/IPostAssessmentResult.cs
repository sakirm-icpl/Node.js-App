using Assessment.API.APIModel;
using Assessment.API.APIModel.Assessment;
using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interface
{
    public interface IPostAssessmentResult : IRepository<PostAssessmentResult>
    {
        Task<IEnumerable<APIPostAssessmentSubjectiveResult>> GetPostAssessmentResultById(int AssesmentResultId);
        Task<int> GetNoOfAttempts(int UserId, int CourseId, int? ModuleID, bool isPreassessment, bool isContentAssessment);
        string GetAssessmentStatus(int UserId, int CourseId, int? ModuleID);
        Task AddModuleCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null);
        Task<ApiResponse> PostAssessmentQuestion(APIPostAssessmentQuestionResult aPIPostAssessmentResult, int UserId, string? OrgCode = null);
        Task AddCompleteionStatusForAdaptiveCourse(int UserId, int CourseId);
        Task AddCompleteionStatusForAdaptiveCourseModule(int UserId, int CourseId, int ModuleId);
        Task<APIAssessmentPostResults> PostAdaptiveAssessment(APIPostAdaptiveAssessment apiPostAdaptiveAssessment, int userId, string OrganisationCode);
        Task<ApiResponse> StartAssessment(APIStartAssessment apiStartAssessment, int userId, string OrganisationCode);
        Task<PostAssessmentResult> CheckForAssessmentCompleted(APIPostAssessmentQuestionResult aPIPostAssessmentResult, int UserId);
        Task<PostAssessmentResult> CheckForAssessmentCompletedByUser(APIStartAssessment aPIPostAssessmentResult, int UserId);
        Task<ApiResponse> PostManagerEvaluation(APIPostManagerEvaluationResult aPIPostAssessmentResult, string OrgCode);
        Task<ApiResponse> PostSubjectiveAssessmentReview(APIPostSubjectiveReview aPIPostAssessmentResult, int UserId, string? OrgCode = null);
        Task<List<APITrainingReommendationNeeds>> GetLatestAssessmentSubmitted(int UserId);
    }

}
