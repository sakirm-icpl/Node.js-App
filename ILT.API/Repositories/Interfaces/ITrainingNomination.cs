using ILT.API.APIModel;
using ILT.API.Model;
using ILT.API.Model.ILT;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface ITrainingNomination : IRepository<TrainingNomination>
    {
        Task<APIILTSchedular> GetByID(int Id);
        Task<List<APIILTSchedular>> GetAllActiveSchedules(int page, int pageSize, string OrganisationCode, int UserId, string searchParameter = null, string searchText = null);
        Task<int> GetAllActiveSchedulesCount(string OrganisationCode, int UserId, string searchParameter = null, string searchText = null,bool showAllData=false);
        Task<ApiResponseILT> PostNominateUser(int id, int moduleId, int courseId, List<APIUserData> userID, int userId, string OrganisationCode, int BatchId);
        Task<ApiResponse> checkValidData(int ScheduleID, int ModuleID, int CourseID);
        Task<List<APITrainingNomination>> GetNominateUserDetails(int id, int courseId, int page, int pageSize, string search = null, string searchText = null);
        Task<int> GetNominateUserCount(int id, int courseId, string search = null, string searchText = null);
        Task<ApiResponse> DeleteNominateUsers(int moduleId, int courseId, int scheduleId, int userId, string orgcode);
        Task<ApiResponse> GetCourseName(string search = null);
        Task<ApiResponse> ProcessRecordsAsync(DataTable nominationImportDt, int userid, string OrganisationCode);
        Task<ApiResponse> ProcessImportFile(APITrainingNominationPath aPITrainingNominationPath, int UserId, string OrganisationCode);
        Task<List<SchedularTypeahead>> GetByModuleId(int ModuleId, int? CourseId, int UserId, string OrganisationCode);
        Task<List<APITrainingNomination>> GetUsersForNomination(int scheduleID, int courseId, int moduleId, int UserId, int page, int pageSize, string search = null, string searchText = null, string Type = null);
        Task<List<APITrainingNomination>> getAllUsersForSectionalAdmin(int scheduleID, int courseId, int moduleId, int UserId, int page, int pageSize, string OrganisationCode, string search = null, string searchText = null);
       
        Task<int> GetUsersCountForNomination(int scheduleID, int courseId, int moduleId, int UserId, string search = null, string searchText = null, string Type = null);
        Task<int> GetUsersCountForSectionalAdmin(int scheduleID, int courseId, int moduleId, int UserId, string OrganisationCode, string search = null, string searchText = null);
        Task<List<SchedularTypeahead>> GetScheduleByModuleId(int Id);
        Task<List<SchedularTypeahead>> GetScheduleByModuleId_AttendanceReport(int ModuleId, int? CourseId);

        Task<IEnumerable<APINominatedUsersForExport>> GetNominatedWiseReport(APINominatedUsersForExport trainingnominate);
        Task<IEnumerable<APITrainingNominationRejected>> GetAllTrainingNominationReject(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);
        void Delete();
        Task<FileInfo> ExportTrainingNominationReject(string search);
        Task<List<APIILTSchedular>> GetAllActiveSchedulesV2(ApiNominationGet apiNominationGet, string OrganisationCode, int UserId);
        Task<ApiResponse> GetRoleCourseName(int userId, string userRole, string search = null);
        Task<ApiResponse> GetRoleAllCourseName(int userId, string userRole, string search = null);
        Task<ApiResponse> GetScheduleTrainerCourses(int userId, string userRole, string search = null);
    }
}
