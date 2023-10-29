using ILT.API.APIModel;
using ILT.API.Model.ILT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IILTTrainingAttendance : IRepository<ILTTrainingAttendance>
    {
        Task<List<SchedularTypeahead>> GetByModuleId(int ModuleId, int? CourseId, int UserId, string OrganisationCode);
        Task<List<APITrainingAttendanceForAll>> GetAllDetails(int page, int pageSize,int userId, string search = null, string searchText = null, bool showAllData = false);
        Task<int> GetAllDetailsCount(int userId, string search = null, string searchText = null, bool showAllData = false);
        Task<ApiResponseILT> UpdateILTAttendance(List<APIILTTrainingAttendance> aPIILTTrainingAttendance, int UserId, string LoginId, string OrganisationCode, string UserName, string UserRole);
        Task<ApiResponseILT> PostUserAttendance(List<APIILTTrainingAttendance> aPIILTTrainingAttendance, int userId, string OrganisationCode, string LoginId, string UserName, string UserRole);
        Task<List<APITrainingNomination>> GetAllUsersForAttendance(int scheduleID, int courseID, int page, int pageSize, string search, string searchText);
        Task<int> GetUsersCountForAttendance(int scheduleID, int courseID, string search, string searchText);
        Task<ApiResponse> PostUserAttendance(APIILTTrainingAttendance aPIILTTrainingAttendance, int userId);
        Task<List<APIILTScheduleDetails>> GetCourseAndSchedule(int page, int pageSize, int userid);
        Task<List<APITrainingNomination>> GetWebinarUsersForAttendance(int scheduleID, int courseID, int page, int pageSize, string search, string searchText);
        Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName);
        Task<bool> GetRegeneratedOTP(APIILTTrainingAttendance aPIILTTrainingAttendance, string OrganisationCode);
        Task<IEnumerable<APIAttendance>> GetAttendanceWiseReport(APIAttendance trainingnominated);
        Task<List<APIDetailsOfUserAfterAttendance>> GetDetailsForUserAttendance(APIGetDetailsForUserAttendance objUserDetails);
        Task<int> CheckForValidDate(APIILTTrainingAttendance obj);
        Task<string> GetBoolConfigurablevalue(string configurableparameter);
        bool CheckForHolidayAttendance(APIILTTrainingAttendance aPIILTTrainingAttendance);
        Task<List<APIUserAttendanceDetails>> GetAttendanceUserDetails(int ModuleId, int ScheduleId, int CourseId, int UserId, DateTime AttendanceDate);
        Task<int> CheckMultipleOTP(APIILTTrainingAttendance aPIILTTrainingAttendance);
        Task<ILTSchedule> ReadSchedulestartdaate(APIILTTrainingAttendance aPIILTTrainingAttendance);
        Task <bool> CheckForValidTrainer(APIILTTrainingAttendance aPIILTTrainingAttendance , int UserId ,string RoleCode);
        Task<string> GetApplicationDateFormat(string OrgCode);
        Task<List<APIILTCourseDetails>> GetILTCourses(int page, int pageSize, int userid);
        Task<List<ScheduleList>> GetILTScheduleDetails(int page, int pageSize, int userid, int ModuleId, int CourseId);
    }
}
