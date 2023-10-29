using CourseReport.API.APIModel;
using CourseReport.API.APIModel;
using CourseReport.API.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CourseReport.API.Service.Interface
{
    public interface ICourseReportService
    {
        Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule,string OrgCode);
        Task<FileInfo> ExportCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule,string OrgCode, List<APIUserSetting> userSetting);
        Task<IEnumerable<APIUserLearningReport>> GetUserLearningReport(APIUserLearningReportModule UserLearningReportModule,int UserID,string OrgCode);
        Task<FileInfo> ExportUserLearningReport(APIUserLearningReportModule UserLearningReportModule, int UserID, string OrgCode);
        Task<IEnumerable<APICourseRatingReport>> CourseRatingReport(APICourseRatingReport aPICourseRatingReport);
        Task<int> GetCourseRatingReportCount(APICourseRatingReport aPICourseRatingReport);
        Task<FileInfo> ExportCourseRatingReport(APICourseRatingReport aPICourseRatingReport, string OrgCode);
        Task<IEnumerable<ApiModeratorwiseSubjectSummaryReport>> GetModeratorwiseSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryReport);
        Task<FileInfo> ExportModeratorwiseSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule);
        Task<IEnumerable<ApiModeratorwiseSubjectDeailsReport>> GetModeratorwiseSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule);
        Task<FileInfo> ExportModeratorwiseSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule);
        Task<FileInfo> ExportUserwiseCourseStatusReport(APIUserwiseCourseStatusReport aPIUserwiseCourseStatusReport);
        Task<FileInfo> GetTcnsRetrainingReport(APITcnsRetrainingReport tcnsRetrainingReport, string orgCode);

        Task<int> PostCourseWiseCompletionReport(APIPostCourseWiseCompletionReport data, int UserId);
        Task<string> UpdateCourseWiseCompletionReport(APIUpdateCourseWiseCompletionReport data, int UserId);
        Task<ExportCourseCompletionDetailReport> UpdateonDownloadReport(int Id);

        Task<CourseWiseCompletionReports> GetCourseWiseCompletionReports(int page,int pageSize,int UserId);
    }
}
