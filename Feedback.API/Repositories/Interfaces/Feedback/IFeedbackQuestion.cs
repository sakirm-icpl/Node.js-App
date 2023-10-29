using Feedback.API.APIModel;
using Feedback.API.APIModel.Feedback;
using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackQuestion : IRepository<FeedbackQuestion>
    {
        bool Exists(string name);
        Task<List<CourseFeedbackAPI>> Get(int page, int pageSize, int UserId, string RoleCode, bool showAllData = false, string? search = null, string? filter = null);
        int Count(string? search = null, string? filter = null);

        Task<List<CourseFeedbackAPI>> GetLCMS(int page, int pageSize, string? isEmoji = null, string? search = null, string? filter = null);
        int LCMSCount(string? search = null, string? isEmoji = null, string? filter = null);

        Task<CourseFeedbackAPI> GetFeedback(int questionId);
        Task<IEnumerable<CourseFeedbackAPI>> GetFeedbackByCourseId(int CourseId);
        bool QuestionExists(string Question, int? id = null);
        Task<string> ProcessImportFile(FileInfo file, IFeedbackQuestion _feedbackQuestion, IFeedbackQuestionRejectedRepository _feedbackQuestionRejectedRepository, IFeedbackOption _feedbackOption, int UserId);
        Task<int> DeleteQuestion(int[] QuestionsId, int UserId);
        Task<IEnumerable<CourseFeedbackAPI>> GetFeedbackByConfigurationId(int configurationId, string OrgnisationCode);
        Task<IEnumerable<CourseFeedbackAnsAPI>> GetFeedbackAnsByConfigurationId(int configurationId, int ModuleId, int courseId, int UserId, string OrgnisationCode);
        int ActiveQuestionCount(string? search = null, string? columnName = null, string? isEmoji = null);
        Task<List<CourseFeedbackAPI>> GetActiveFeedbackQuestion(int page, int pageSize, string? isEmoji = null, string? search = null, string? filter = null);
        Task<List<CourseFeedbackAPI>> GetPagination(int page, int pageSize, string? search = null, string? filter = null);
        Task<APIFQCourse> courseCodeExists(string coursecode);
        Task<ApiResponse> MultiDeleteFeedbackQuestion(APIDeleteFeedbackQuestion[] apideletemultipleque);
        Task<UserFeedbackQueTotalAPI> GetPaginationV2(int page, int pageSize, int userId, string userRole, bool showAllData = false, string? search = null, string? filter = null, bool? isEmoji = null);
    }
}
