using Evaluation.API.Models;
using Evaluation.API.APIModel;
using Evaluation.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evaluation.API.APIModel;
using Evaluation.API.APIModel;
using System.Text;
using Evaluation.API.Model;
using Evaluation.API.Models;

namespace Evaluation.API.Repositories.Interface
{
    public interface IProcessEvaluationQuestion : IRepository<ProcessEvaluationQuestion>
    {
        Task<IEnumerable<ProcessEvaluationQuestion>> GetAllQuestionPagination(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null);
        Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForEvaluation();
        Task<StringBuilder> GetTransactionID(apiGetTransactionID apigettransactionID, string LoginID);
        Task<StringBuilder> GetTransactionIDForOER(apiGetTransactionID apigettransactionID, string LoginID);
        Task<IEnumerable<APIProcessEvaluationManagement>> GetProcessManagement(string OrganisationCode, int UserId);
        Task<ApiResponse> PostProcessResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null);
        Task<APIPostProcessEvaluationDisplay> LastSubmitedProcessResult(APILastSubmitedResult aPIPostAssessmentResult, string organizationcode);
        Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForKitchenAudit();
        Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForOEREvaluation();
        Task<APIPostProcessEvaluationDisplay> LastSubmitedProcessResultKitchenAudit(APILastSubmitedResultKitchenAudit aPIPostAssessmentResult, string OrganisationCode);
        Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForEvaluationforChaiPoint(apiGetQuestionforChaipoint apiGetQuestionforChaipoint);
        Task<ApiResponse> PostProcessResultForChaipoint(APIPostProcessEvaluationResultForChaipoint aPIPostAssessmentResult, int UserId, string OrgCode = null);

        Task<ApiResponse> PostOERProcessResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null);
        Task<APIPostOERProcessEvaluationDisplay> LastSubmitedOERProcessResult(APILastSubmitedResult aPIPostAssessmentResult, string organizationcode);

        Task<IEnumerable<PMSEvaluationPointResponse>> GetQuestionForPMSEvaluation(string Section, int userId);
        Task<int> PMSEvaluationSumbit(int UserId, string ManagerId);

        Task PostPMSEvaluationResult(List<APIPMSEvaluationResult> apiPMSEvaluationResult, string OrganisationCode);
        Task<IEnumerable<PMSEvaluationResultPointResponse>> GetPendingPMSEvaluationById(int id);

        Task<IEnumerable<PendingPMSEvaluation>> GetPendingPMSEvaluation(int userId);
        Task PostPMSEvaluationResultManager(List<APIPMSEvaluationResultManager> apiPMSEvaluationResult);
        Task<bool> Exist(int UserId);

        #region Critical Audit Evaluation 
        Task<IEnumerable<APICriticalAuditQuestion>> GetQuestionForCriticalAuditEvaluation();
        Task<ApiResponse> PostProcessCriticalAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null);
        Task<APIPostProcessEvaluationDisplay> LastSubmittedCriticalAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode);
        #endregion


        #region Night Audit Evaluation 
        Task<IEnumerable<APINightAuditQuestion>> GetQuestionForNightAuditEvaluation();
        Task<ApiResponse> PostProcessNightAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null);
        Task<APIPostProcessEvaluationDisplay> LastSubmittedNightAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode);
        #endregion

        #region Ops Audit Evaluation 
        Task<IEnumerable<APIOpsAuditQuestion>> GetQuestionForOpsAuditEvaluation();
        Task<ApiResponse> PostProcessOpsAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null);
        Task<APIPostProcessEvaluationDisplay> LastSubmittedOpsAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode);
      //  Task<StringBuilder> GetTransactionID(apiGetQuestionforChaipoint apiGetQuestionforChaipoint, string loginId);
        #endregion
    }
}
