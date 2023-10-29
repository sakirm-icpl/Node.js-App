using CourseReport.API.APIModel;
using CourseReport.API.APIModel;
using CourseReport.API.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories.Interface
{
    public interface ICourseReportRepository
    {
        Task<string> GetConfigurableParameterValue(string Code);
        Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode);
        Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReportExport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode);
        Task<IEnumerable<APIUserLearningReport>> GetUserLearningReport(APIUserLearningReportModule UserLearningReportModule,int UserID, string OrgCode);
        Task<IEnumerable<APICourseRatingReport>> GetCourseRatingReport(APICourseRatingReport aPICourseRatingReport);
        Task<int> GetCourseRatingReportCount(APICourseRatingReport aPICourseRatingReport);
        Task<IEnumerable<ApiModeratorwiseSubjectSummaryReport>> GetModeratorSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule);
        Task<DataTable> GetSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule);
        Task<IEnumerable<ApiModeratorwiseSubjectDeailsReport>> GetModeratorSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule);
        Task<DataTable> GetSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule objReport);
        Task<IEnumerable<APIUserwiseCourseStatusReportResult>> GetUserwiseCourseStatusReport(APIUserwiseCourseStatusReport aPIUserwiseCourseStatusReport);
        Task<IEnumerable<ApiExportTcnsRetrainingReport>> GetTcnsRetrainingReport(APITcnsRetrainingReport tcnsRetrainingReport);

        Task<int> PostCourseWiseCompletionReport(APIPostCourseWiseCompletionReport data, int UserId);
        Task<CourseWiseCompletionReports> GetCourseWiseCompletionReports(int page,int pageSize,int UserId);
        Task<string> UpdateCourseWiseCompletionReport(APIUpdateCourseWiseCompletionReport data, int UserId);
        Task<ExportCourseCompletionDetailReport> UpdateonDownloadReport(int Id);
    }
}
