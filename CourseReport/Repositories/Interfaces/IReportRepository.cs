using CourseReport.API.APIModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
//using static CourseReport.API.Common.ResponseModels;

namespace CourseReport.API.Repositories.Interface
{
    public interface IReportRepository
    {
        //Task<IEnumerable<APICourseCompletionReport>> GetCourseCompletionReport(APICourseCompletion courseCompletion);
        //Task<IEnumerable<APINotStartedCourseReport>> GetNotStartedCourseReport(APICourseCompletion courseCompletion);
        //Task<int> SoctrainSACReportCount(APISoctrainCountSAC aPISoctrain);
        //Task<List<APISoctrainSACTrainingReport>> SoctrainTrainingSACReport(APISoctrainSACTrainingModule aPISoctrainTrainingSAC);
        //Task<int> SoctrainTrainingSACReportCount(APISoctrainSACTrainingCountModule aPISoctrainTraining);
        //Task<IEnumerable<APICourseExportReport>> GetExportCourseCompletionReport();
        //Task<IEnumerable<APIUserLoginStatistic>> GetUserLoginStatistic(APILoginStatisticsReportModule APILoginStatisticsReport);
        //Task<int> GetNotStartedCourseReportCount(APICourseCompletion courseCompletion);
        //Task<int> GetCourseCompletionReportCount(APICourseCompletion courseCompletion);


        //Task<IEnumerable<APIAssessmentResultSheetReport>> GetAssessmentResultSheet(APIAssessmentResultSheetModule assessmentResultSheet);
        //Task<IEnumerable<APIRecommondedTrainingReport>> GetTrainingProgram(APIRecommondedTraining assessmentResultSheet);
        //Task<IEnumerable<APIAssessmentResultSheetGraph>> GetAssessmentResultSheetGraph(APIAssessmentResultSheetGraphModule assessmentResultSheetGraph);

        //Task<IEnumerable<APITestResultReport>> GetTestResultReport(APITestResultReportModule testResultReportModule);
        //Task<IEnumerable<APITestResultQuestionWiseGraph>> GetTestResultQuestionWiseGraph(APITestResultQuestionWiseGraphModule TestResultQuestionWiseGraph);

        //Task<IEnumerable<APIUserLoginStatistic>> GetUserLoginStatisticReports(APILoginStatisticsReportModule APILoginStatisticsReport);


        //Task<IEnumerable<APIUserAssessmentSheet>> GetUserAssessmentSheet(APIAssessmentResultSheetModule assessmentResultSheet);
        //Task<IEnumerable<ApiAttendanceSummuryReport>> GetAttendenceSummuryReport(ApiAttendenceSummuryReportModule AttendenceSummuryReport);
        //Task<IEnumerable<ApiProcessEvaluationAuditReport>> GetProcessEvaluationAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<IEnumerable<ApiProcessEvaluationAuditReport>> GetCriticalAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<IEnumerable<ApiProcessEvaluationAuditReport>> GetNightAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<IEnumerable<ApiProcessEvaluationAuditReport>> GetOpsAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<IEnumerable<ApiProcessEvaluationSectionReport>> GetProcessEvaluationSectionWiseReport(APIProcessEvaluationSectionModule ProcessEvaluationSectionREport);
        //Task<IEnumerable<ApiProcessEvaluationStoreWiseReport>> GetProcessEvaluationStoreWiseReport(APIProcessEvaluationStoreModule ProcessEvaluationStoreReport);

        //Task<IEnumerable<ApiProcessEvaluationAuditReport>> GetPartnerKitchenAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport);
        //Task<DataTable> GetPartnerKitchenReport(APIProcessEvaluationAuditModule objReport);
        //Task<DataTable> GetExportKitchenAuditSectionReport(APIProcessEvaluationSectionModule objReport);
        //Task<IEnumerable<ApiProcessEvaluationSectionReport>> GetKitchenAuditSectionWiseReport(APIProcessEvaluationSectionModule ProcessEvaluationSectionReport);
        //Task<DataTable> GetProcessEvaluationReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<DataTable> ExportGetCriticalAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<DataTable> ExportGetNightAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<DataTable> ExportGetOpsAuditReport(APIProcessEvaluationAuditModule ProcessEvaluationAuditReport, string OrgCode);
        //Task<DataTable> GetExportKitchenAuditStoreReport(APIProcessEvaluationStoreModule objReport);
        //Task<IEnumerable<ApiProcessEvaluationStoreWiseReport>> GetKitchenAuditStoreWiseReport(APIProcessEvaluationStoreModule ProcessEvaluationStoreReport);
        //Task<DataTable> GetExportEvaluationSectionReport(APIProcessEvaluationSectionModule ProcessEvaluationSectionReport);
        //Task<DataTable> GetExportEvaluationStoreReport(APIProcessEvaluationStoreModule ProcessEvaluationStoreReport);
        //Task<IEnumerable<APIOpinionPollReport>> GetOpinionPollReport(APIOpinionPollReportModule OpinionPollReport);
        //Task<APICourseProgressDetailsReport> GetCourseCompletionReportDetails(APICourseCompletionDetails courseCompletion);
        //Task<IEnumerable<APIAllCourseSummaryReportModule>> GetAllCourseSummaryReport();
        //Task<IEnumerable<APIExportUserwiseCoursecompletionreportModule>> ExportUserwiseCoursecompletionreport();
        //Task<IEnumerable<APIExportGetAllUserActivityReportModule>> ExportAllUserActivityReport();
        //Task<IEnumerable<APIManagerEvaluationReport>> ExportManagerEvaluationReport();
        //Task<List<APIReportProfabAccessibility>> ExportProfabAccessibilityReport(APIReportTLS reportTLSModule);
        //Task<List<APIReportProfabAccessibilityCount>> ExportProfabAccessibilityCountReport(APIReportTLS reportTLSModule);
        //Task<List<APIWorkDiary>> ExportWorkDiaryReport(APIReportTLS reportTLSModule);
        //Task<List<APISoctrainSACReport>> SoctrainSACReport(APISoctrainSAC aPISoctrainSAC);


        //Task<List<APICourseWiseReportTataSky>> ExportCourseWiseReportTataSky();
        //Task<List<APIAssessmentResultsDetails>> PostAssessmentResultsDetailsReport(APIPostAssessmentResultsDetails aPIPost);
        //Task<int> PostAssessmentResultsDetailsReportCount(APIPostAssessmentResultsDetails aPIPost);
        //Task<IEnumerable<ApiProcessEvaluationSectionReportChaiPoint>> GetProcessEvaluationSectionWiseReportChaiPoint(APIProcessEvaluationSectionModule ProcessEvaluationSectionReport);

        //Task<IEnumerable<ProcessEvaluationAuditDetailsWithImagePaths>> GetProcessEvaluationAuditDetailsWithFilePaths(int EvalResultId);
        //Task<IEnumerable<ProcessEvaluationAuditDetailsWithImagePaths>> GetCriticalAuditDetailsWithFilePaths(int EvalResultId);
        //Task<IEnumerable<ProcessEvaluationAuditDetailsWithImagePaths>> GetNightAuditDetailsWithFilePaths(int EvalResultId);
        //Task<IEnumerable<ProcessEvaluationAuditDetailsWithImagePaths>> GetOpsAuditDetailsWithFilePaths(int EvalResultId);
        //Task<APIDevPlanTotalReport> GetDevPlanReport(APIDevPlanCompletion DevPlanCompletion);
        //Task<List<APIDevPlanDetailsReport>> GetDevPlanReportDetails(APIDevelopementPlanDetails courseCompletion);
        //string GetConfigurableParameter(string code);
        //List<UserSettings> GetuserSettingsData();
        //List<UserSettings> GetValidateUserSettingsData();
        //Task<APIManagerEvaluationData> GetManagerEvaluation(APIAssessmentResultSheetModule assessmentResultSheet);
        //APIResponse<ManagerEvaluationSummaryAndDetailReport> GetManagerEvaluationSummaryAndDetailReport(ApiManagerEvaluationSummaryAndDetailReport apiManagerEvaluationSummaryAndDetailReport);
    }
}