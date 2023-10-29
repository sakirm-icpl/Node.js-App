using MyCourse.API.APIModel;
using MyCourse.API.Repositories.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IDataMigration
    {
        Task<ApiResponse> ProcessImportFile(FileInfo file, IDataMigration _dataMigration, int UserId, string OrgCode);

        Task<ApiResponse> ProcessAssessmentImportFile(FileInfo file, IDataMigration _dataMigration, int UserId);
        Task<ApiResponse> ProcessCourseModuleImportFile(FileInfo file, IDataMigration _dataMigration, int UserId);
        Task<ApiResponse> ILTScheduleImportFile(APIDataMigrationFilePath aPIILTScheduleImport, int UserId, string OrgCode);
        Task<ApiResponse> TrainingRecommondationImportFile(APIDataMigrationFilePath aPIILTScheduleImport, int UserId, string OrgCode);
        Task<ApiResponse> ProcessImportFile_Competency(FileInfo file, IDataMigration _dataMigration, int UserId);
        Task<byte[]> ExportImportFormat(string OrgCode);
        Task<byte[]> TrainingReommendationFormat(string OrgCode);
    }

}
