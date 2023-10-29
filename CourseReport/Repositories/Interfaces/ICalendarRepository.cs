using CourseReport.API.APIModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories.Interface
{
    public interface ICalendarRepository
    {
        Task<ApiCalendarCourseModuleReport> GetMyTrainingCalendarData(int userId, DateTime? fromDate = null, DateTime? toDate = null);

        //Task<List<ApiCalendarV2>> GetILTScheduleCalendarData(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);

        // Task<List<ApiCalendarV2>> GetILTScheduleCalendarDataForTrainer(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<ILTScheduleCalenderDataExport>> GetILTScheduleCalendarDataExport(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
        Task<List<ILTScheduleCalenderDataExport>> GetILTCalenderDataForTrainerExport(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<ApiCalendarV2ForTrainer>> GetILTScheduleCalendarDataForTrainer(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
        string GetWebinarLink(string ScheduleCode);
        Task<List<ApiCalendarV2ForTrainer>> GetILTScheduleCalendarData(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<ApiCalendarV2ForTrainer>> GetOrganizationCalendarData(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
