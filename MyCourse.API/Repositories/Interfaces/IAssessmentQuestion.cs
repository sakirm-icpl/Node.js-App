//using Assessment.API.APIModel;
using MyCourse.API.Model;
using MyCourse.API.APIModel;
//using MyCourse.API.APIModel.Assessment;
//using MyCourse.API.APIModel.Refactored;
using MyCourse.API.Common;
//using MyCourse.API.Model.Assessment;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.APIModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interface
{
    public interface IAssessmentQuestion : IRepository<AssessmentQuestion>
    {
        //Task<IEnumerable<APIGetQuestionMaster>> GetAllQuestionMaster(int page, int pageSize, int UserId, string userRole, bool showAllData = false, string search = null, string columnName = null, bool? isMemoQuestions = null);
        //Task<APIAssessmentQuestion> GetAssessmentQuestionByID(int QuestionID);
        //Task<IEnumerable<APIAssessmentQuestion>> GetAssessmentQuestion(int ConfigureId,string organizationcode);
        //Task<int> Count(string search = null, string columnName = null);
        //bool QuestionExists(string Question);
        //Task<IEnumerable<APIAssessmentQuestionConfiguration>> GetAssessmentQuestionByConfigurationId(int ConfigureId);
        //Task<APIAssessmentMaster> GetAssessmentHeader(int ConfigurationID, int CourseID, int ModuleID, int userId, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false,string orgCode="");
        //Task<bool> AssessmentStatus(int CourseId, int? ModuleId, int userId, bool isPreAssessment = false, bool isContentAssessment = false);
        //int GetTotalMark(int assessmentSheetConfigID, string OrgCode);
        //int GetObtainedMarks(int? referenceQuestionID, int? optionAnswerId);
        //int GetMultipleObtainedMarks(int? referenceQuestionID, int? optionAnswerId);
        //int GetMultipleObtainedMarksCount(int? referenceQuestionID);
        Task<bool> Exist(string changedName);
        //Task<string> ProcessImportFile(FileInfo file, IAssessmentQuestion _assessmentQuestion, IAssessmentQuestionRejectedRepository _assessmentQuestionRejected, IAsessmentQuestionOption _asessmentQuestionOption, int userid, string OrganisationCode);
        //Task<Message> DeleteQuestion(int questionId);
        //Task<IEnumerable<APIGetQuestionMaster>> GetAllActiveQuestion(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null);
        //Task<int> ActiveQustionsCount(string search = null, string columnName = null, bool? isMemoQuestions = null);
        //Task<List<APIAssessmentQuestion>> PostQuestion(List<APIAssessmentQuestion> aPIAssessmentsQuestion, int UserId, string OrganisationCode);
        //Task<Message> UpdateAssessmentQuestion(int id, APIAssessmentQuestion apiAssessmentQuestion, int UserId);
        //Task<IEnumerable<APIAdaptiveAssessmentQuestion>> GetAdaptiveAssessment(int courseId);
        //Task<int> GetMultipleAnwersMarks(int? QuestionID, List<int?> selectedOptionId);

        //List<QuestionAnswerAssessement> GetMultipleAnwersMarksList(List<int?> QuestionID, List<int?> selectedOptionId);
        //Task<APIAssessmentMaster> GetAdaptiveAssessmentHeader(int courseId, int userId);
        //Task<IEnumerable<APIGetQuestionMaster>> GetAllQuestionPagination(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null);
        //Task<IEnumerable<APIAssessmentReview>> GetQuestionForReview(APIStartAssessment aPIStartAssessment, int UserId);
        //Task<ApiResponse> MultiDeleteAssessmentQuestion(APIDeleteAssessmentQuestion[] apideletemultipleque);
        //Task<bool> IsContentCompleted(int userId, int courseId, int? moduleId, bool isPreassessment);
        //int GetMarksForCut(int? referenceQuestionID);
        //Task<bool> ExistQuestionOption(APIAssessmentQuestion objAPIAssessmentQuestion);
        //Task<bool> ExistQuestionOptionUpdate(APIAssessmentQuestion objAPIAssessmentQuestion, int id);
        //Task<List<AssessmentQuestionRejected>> GetAllAssessmentQuestionRejected();
        //Task<List<APIJobRole>> GetCompetencySkill(int QuestionID);
        //Task<IEnumerable<APISubjectiveAQReview>> GetReviewQuestion(int ConfigureId);
        //Task<IEnumerable<APIAssessmentDataForReview>> GetAssessmentForReview(int page, int pageSize, string search = null, string columnName = null);
        //Task<int> ReviewCountCount(string search = null, string columnName = null);
        //Task<APITotalAssessmentQuestion> GetAllQuestionPaginationV2(int page, int pageSize, int userId, string userRole, bool showAllData = false, string search = null, string columnName = null, bool? isMemoQuestions = null);
        //Task<APIFQCourse> courseCodeExists(string coursecode);
        //int SaveAssessmentPhotos(APIassessmentPhotos aPIassessmentPhotos, int Userid);
        //Task<CameraPhotosResponse> GetCameraPhotos(GetAPIassessmentPhotos getAPIassessmentPhotos, int Userid);
        //Task<APITotalCourseForEvaluation> GetCoursesForCameraEvaluation(int page, int pageSize, int userId);
        //Task<int> SaveStatusForCourseEvaluation(CameraPhotosStatusForCourseEvaluation cameraPhotosStatusForCourseEvaluation, int UserId, int ManagerId);
        //Task<bool> AssessmentCompletionStatus(int courseId, int moduleId, int userId, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false);
    }
}
