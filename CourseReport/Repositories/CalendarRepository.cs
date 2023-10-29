using Dapper;
using Microsoft.EntityFrameworkCore;
using CourseReport.API.APIModel;
using CourseReport.API.Data;
using CourseReport.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using CourseReport.API.Helper;
using System.Data.Common;
using CourseReport.API.Model;

namespace CourseReport.API.Repositories
{
    public class CalendarRepository : ICalendarRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CalendarRepository));

        protected ReportDbContext _db;
        protected ICustomerConnectionStringRepository _customerConnectionRepository;
        private readonly List<TimeZoneList> _tzList;
        public CalendarRepository(ReportDbContext context, ICustomerConnectionStringRepository customerConnectionRepository)
        {
            this._db = context;
            this._customerConnectionRepository = customerConnectionRepository;
            // get system time zone
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            _tzList = tzs.Select(tz => new TimeZoneList
            {
                Text = tz.DisplayName,
                Value = tz.Id
            }).ToList();
        }
        public async Task<ApiCalendarCourseModuleReport> GetMyTrainingCalendarData(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {

            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    SqlMapper.GridReader Result = await SqlMapper.QueryMultipleAsync((SqlConnection)connection, "[dbo].[GetMyTrainingCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                    ApiCalendarCourseModuleReport CalendarReport = new ApiCalendarCourseModuleReport
                    {
                        Courses = Result.Read<APICalendarCourseReport>().ToList(),
                        Modules = Result.Read<APICalendarModuleReport>().ToList()
                    };
                    connection.Close();
                    return CalendarReport;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }


        public async Task<List<ApiCalendarV2ForTrainer>> GetILTScheduleCalendarData(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    List<ApiCalendarV2ForTrainer> apiCalender = new List<ApiCalendarV2ForTrainer>();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    parameters.Add("@OrgCode", OrgCode);
                    string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = await SqlMapper.QueryAsync<ApiCalendarV3>((SqlConnection)connection, "[dbo].[GetMyBatchCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        List<ApiCalendarV3> ApiCalendarV3List = result.ToList();
                        connection.Close();

                        List<ApiCalendarV2ForTrainer> apiCalendarV2List = ApiCalendarV3List.GroupBy(x => new
                        {
                            x.CourseId,
                            x.CourseCode,
                            x.CourseTitle,
                            x.BatchId,
                            x.BatchCode,
                            x.BatchName,
                            x.BatchStartDate,
                            x.BatchEndDate,
                            x.BatchStartTime,
                            x.BatchEndTime,
                            x.ModuleType,
                            x.ModuleId,
                            x.ScheduleId,
                            x.FromTimeZone,
                            x.ToTimeZone
                        }).Select(x => x.FirstOrDefault()).Select(cal => new ApiCalendarV2ForTrainer
                        {
                            CourseId = cal.CourseId,
                            Start = cal.BatchStartDate,
                            End = cal.BatchEndDate,
                            StartTimes = cal.BatchStartTime.ToString(@"hh\:mm"),
                            EndTimes = cal.BatchEndTime.ToString(@"hh\:mm"),
                            Title = cal.BatchName + " (" + cal.CourseTitle + ")",
                            CourseCode = cal.CourseCode,
                            CourseName = cal.CourseTitle,
                            BatchId = cal.BatchId,
                            BatchCode = cal.BatchCode,
                            BatchName = cal.BatchName,
                            FromTimeZone = cal.FromTimeZone,
                            ToTimeZone = cal.ToTimeZone,
                            ModuleType = cal.ModuleType,
                            ScheduleId = cal.ScheduleId,
                            ModuleId = cal.ModuleId
                        }).ToList();

                        foreach (ApiCalendarV2ForTrainer calendarobj in apiCalendarV2List)
                        {
                            List<APICalendarSchedules> aPICalendarSchedulesList = ApiCalendarV3List.Where(x => x.BatchId == calendarobj.BatchId)
                                                                                                .Select(sch => new APICalendarSchedules
                                                                                                {
                                                                                                    CourseCode = sch.CourseCode,
                                                                                                    CourseName = sch.CourseTitle,
                                                                                                    ModuleName = sch.ModuleName,
                                                                                                    ScheduleCode = sch.ScheduleCode,
                                                                                                    Start = sch.ScheduleStartDate.ToString("yyyy-MM-dd"),
                                                                                                    End = sch.ScheduleEndDate.ToString("yyyy-MM-dd"),
                                                                                                    StartTime = sch.ScheduleStartTime.ToString(@"hh\:mm"),
                                                                                                    EndTime = sch.ScheduleEndTime.ToString(@"hh\:mm"),
                                                                                                    Currency = sch.Cost.ToString() + " " + sch.Currency,
                                                                                                    PlaceName = sch.PlaceName + ", " + sch.Cityname + "," + sch.PostalAddress,
                                                                                                    Title = sch.ScheduleCode,
                                                                                                    FromTimeZone = sch.FromTimeZone,
                                                                                                    ToTimeZone = sch.ToTimeZone,
                                                                                                    ModuleType = sch.ModuleType,
                                                                                                    ScheduleId = sch.ScheduleId,
                                                                                                    ModuleId = sch.ModuleId
                                                                                                }).ToList();
                            calendarobj.aPICalendarSchedulesList = aPICalendarSchedulesList;
                            apiCalender.Add(calendarobj);
                        }
                    }
                    else
                    {
                        IEnumerable<ILTScheduleCalenderData> result = await SqlMapper.QueryAsync<ILTScheduleCalenderData>((SqlConnection)connection, "[dbo].[GetMyILTScheduleCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        List<ILTScheduleCalenderData> ILTScheduleCalenderData = new List<ILTScheduleCalenderData>();
                        ILTScheduleCalenderData = result.ToList();
                        connection.Close();
                        foreach (ILTScheduleCalenderData iltSchedule in ILTScheduleCalenderData)
                        {
                            ApiCalendarV2ForTrainer calender = new ApiCalendarV2ForTrainer
                            {
                                CourseId = iltSchedule.Id,
                                CourseCode = iltSchedule.CourseCode,
                                CourseName = iltSchedule.CourseTitle,
                                ModuleName = iltSchedule.ModuleName,
                                ScheduleCode = iltSchedule.ScheduleCode,
                                Start = iltSchedule.StartDate,
                                End = iltSchedule.EndDate,
                                Title = iltSchedule.CourseTitle,
                                StartTimes = iltSchedule.StartTime.ToString(@"hh\:mm"),
                                EndTimes = iltSchedule.EndTime.ToString(@"hh\:mm"),
                                Currency = iltSchedule.Cost.ToString() + " " + iltSchedule.Currency,
                                PlaceName = iltSchedule.PlaceName + ", " + iltSchedule.Cityname + "," + iltSchedule.PostalAddress,
                                ModuleType = iltSchedule.ModuleType,
                                ScheduleId = iltSchedule.ScheduleID,
                                ModuleId = iltSchedule.ModuleID,
                                FromTimeZone = iltSchedule.FromTimeZone,
                                ToTimeZone = iltSchedule.ToTimeZone
                            };
                            apiCalender.Add(calender);
                        }

                        foreach (ApiCalendarV2ForTrainer item in apiCalender)
                        {

                            if (!string.IsNullOrEmpty(item.FromTimeZone) && !string.IsNullOrEmpty(item.ToTimeZone))
                            {
                                if (item.FromTimeZone != item.ToTimeZone)
                                {
                                    string from = null, to = null;
                                    if (_tzList.Where(a => a.Text == item.FromTimeZone).FirstOrDefault() != null)
                                        from = _tzList.Where(a => a.Text == item.FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (_tzList.Where(a => a.Text == item.ToTimeZone).FirstOrDefault() != null)
                                        to = _tzList.Where(a => a.Text == item.ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();


                                    //item.Tz_StartDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.Start + TimeSpan.Parse(item.StartTimes)), item.FromTimeZone), item.ToTimeZone);
                                    //item.Tz_EndDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.End + TimeSpan.Parse(item.EndTimes)), item.FromTimeZone), item.ToTimeZone);
                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        item.Start = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.Start), from), to)));
                                        item.End = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.End), from), to)));
                                        item.StartTimes = TimeSpan.Parse(Convert.ToString(item.Start.TimeOfDay)).ToString(@"hh\:mm");
                                        item.EndTimes = TimeSpan.Parse(Convert.ToString(item.End.TimeOfDay)).ToString(@"hh\:mm");

                                    }
                                }
                            }
                        }
                    }


                    return apiCalender;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }

        public async Task<List<ApiCalendarV2ForTrainer>> GetOrganizationCalendarData(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    List<ApiCalendarV2ForTrainer> apiCalender = new List<ApiCalendarV2ForTrainer>();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    parameters.Add("@OrgCode", OrgCode);
                    string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = await SqlMapper.QueryAsync<ApiCalendarV3>((SqlConnection)connection, "[dbo].[GetBatchOrganizationCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        List<ApiCalendarV3> ApiCalendarV3List = result.ToList();
                        connection.Close();

                        List<ApiCalendarV2ForTrainer> apiCalendarV2List = ApiCalendarV3List.GroupBy(x => new
                        {
                            x.CourseId,
                            x.CourseCode,
                            x.CourseTitle,
                            x.BatchId,
                            x.BatchCode,
                            x.BatchName,
                            x.BatchStartDate,
                            x.BatchEndDate,
                            x.BatchStartTime,
                            x.BatchEndTime,
                            x.ModuleType,
                            x.ModuleId,
                            x.ScheduleId,
                            x.FromTimeZone,
                            x.ToTimeZone,
                            x.IsShowInCatalogue
                        }).Select(x => x.FirstOrDefault()).Select(cal => new ApiCalendarV2ForTrainer
                        {
                            CourseId = cal.CourseId,
                            Start = cal.BatchStartDate,
                            End = cal.BatchEndDate,
                            StartTimes = cal.BatchStartTime.ToString(@"hh\:mm"),
                            EndTimes = cal.BatchEndTime.ToString(@"hh\:mm"),
                            Title = cal.BatchName + " (" + cal.CourseTitle + ")",
                            CourseCode = cal.CourseCode,
                            CourseName = cal.CourseTitle,
                            BatchId = cal.BatchId,
                            BatchCode = cal.BatchCode,
                            BatchName = cal.BatchName,
                            FromTimeZone = cal.FromTimeZone,
                            ToTimeZone = cal.ToTimeZone,
                            ModuleType = cal.ModuleType,
                            ScheduleId = cal.ScheduleId,
                            ModuleId = cal.ModuleId,
                            IsShowInCatalogue = cal.IsShowInCatalogue
                        }).ToList();

                        foreach (ApiCalendarV2ForTrainer calendarobj in apiCalendarV2List)
                        {
                            List<APICalendarSchedules> aPICalendarSchedulesList = ApiCalendarV3List.Where(x => x.BatchId == calendarobj.BatchId)
                                                                                                .Select(sch => new APICalendarSchedules
                                                                                                {
                                                                                                    CourseCode = sch.CourseCode,
                                                                                                    CourseName = sch.CourseTitle,
                                                                                                    ModuleName = sch.ModuleName,
                                                                                                    ScheduleCode = sch.ScheduleCode,
                                                                                                    Start = sch.ScheduleStartDate.ToString("yyyy-MM-dd"),
                                                                                                    End = sch.ScheduleEndDate.ToString("yyyy-MM-dd"),
                                                                                                    StartTime = sch.ScheduleStartTime.ToString(@"hh\:mm"),
                                                                                                    EndTime = sch.ScheduleEndTime.ToString(@"hh\:mm"),
                                                                                                    Currency = sch.Cost.ToString() + " " + sch.Currency,
                                                                                                    PlaceName = sch.PlaceName + ", " + sch.Cityname + "," + sch.PostalAddress,
                                                                                                    Title = sch.ScheduleCode,
                                                                                                    FromTimeZone = sch.FromTimeZone,
                                                                                                    ToTimeZone = sch.ToTimeZone,
                                                                                                    ModuleType = sch.ModuleType,
                                                                                                    ScheduleId = sch.ScheduleId,
                                                                                                    ModuleId = sch.ModuleId,
                                                                                                    IsShowInCatalogue = sch.IsShowInCatalogue
                                                                                                }).ToList();
                            calendarobj.aPICalendarSchedulesList = aPICalendarSchedulesList;
                            apiCalender.Add(calendarobj);
                        }
                    }
                    else
                    {
                        IEnumerable<ILTScheduleCalenderData> result = await SqlMapper.QueryAsync<ILTScheduleCalenderData>((SqlConnection)connection, "[dbo].[GetOrganizationILTScheduleCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        List<ILTScheduleCalenderData> ILTScheduleCalenderData = new List<ILTScheduleCalenderData>();
                        ILTScheduleCalenderData = result.ToList();
                        connection.Close();
                        foreach (ILTScheduleCalenderData iltSchedule in ILTScheduleCalenderData)
                        {
                            ApiCalendarV2ForTrainer calender = new ApiCalendarV2ForTrainer
                            {
                                CourseId = iltSchedule.Id,
                                CourseCode = iltSchedule.CourseCode,
                                CourseName = iltSchedule.CourseTitle,
                                ModuleName = iltSchedule.ModuleName,
                                ScheduleCode = iltSchedule.ScheduleCode,
                                Start = iltSchedule.StartDate,
                                End = iltSchedule.EndDate,
                                Title = iltSchedule.CourseTitle,
                                StartTimes = iltSchedule.StartTime.ToString(@"hh\:mm"),
                                EndTimes = iltSchedule.EndTime.ToString(@"hh\:mm"),
                                Currency = !String.IsNullOrEmpty(iltSchedule.Currency) ? iltSchedule.Cost.ToString() + " " + iltSchedule.Currency : iltSchedule.Cost.ToString(),
                                PlaceName = iltSchedule.PlaceName + ", " + iltSchedule.Cityname + "," + iltSchedule.PostalAddress,
                                ModuleType = iltSchedule.ModuleType,
                                ScheduleId = iltSchedule.ScheduleID,
                                ModuleId = iltSchedule.ModuleID,
                                FromTimeZone = iltSchedule.FromTimeZone,
                                ToTimeZone = !String.IsNullOrEmpty(iltSchedule.ToTimeZone) ? iltSchedule.ToTimeZone : String.Empty,
                                IsShowInCatalogue = iltSchedule.IsShowInCatalogue
                            };
                            apiCalender.Add(calender);
                        }

                        foreach (ApiCalendarV2ForTrainer item in apiCalender)
                        {

                            if (!string.IsNullOrEmpty(item.FromTimeZone) && !string.IsNullOrEmpty(item.ToTimeZone))
                            {
                                if (item.FromTimeZone != item.ToTimeZone)
                                {
                                    string from = null, to = null;
                                    if (_tzList.Where(a => a.Text == item.FromTimeZone).FirstOrDefault() != null)
                                        from = _tzList.Where(a => a.Text == item.FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (_tzList.Where(a => a.Text == item.ToTimeZone).FirstOrDefault() != null)
                                        to = _tzList.Where(a => a.Text == item.ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();


                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        item.Start = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.Start), from), to)));
                                        item.End = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.End), from), to)));
                                        item.StartTimes = TimeSpan.Parse(Convert.ToString(item.Start.TimeOfDay)).ToString(@"hh\:mm");
                                        item.EndTimes = TimeSpan.Parse(Convert.ToString(item.End.TimeOfDay)).ToString(@"hh\:mm");

                                    }
                                }
                            }
                        }
                    }


                    return apiCalender;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }

        public async Task<List<ILTScheduleCalenderDataExport>> GetILTScheduleCalendarDataExport(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                List<ILTScheduleCalenderDataExport> iLTScheduleCalenderDataExports = new List<ILTScheduleCalenderDataExport>();
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    List<ApiCalendarV2> apiCalender = new List<ApiCalendarV2>();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    parameters.Add("@OrgCode", OrgCode);
                    string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = await SqlMapper.QueryAsync<ILTScheduleCalenderDataExport>((SqlConnection)connection, "[dbo].[GetMyBatchCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        iLTScheduleCalenderDataExports.AddRange(result.ToList());
                        connection.Close();
                    }
                    else
                    {
                        var result = await SqlMapper.QueryAsync<ILTScheduleCalenderDataExport>((SqlConnection)connection, "[dbo].[GetMyILTScheduleCalendarData]", parameters, null, null, CommandType.StoredProcedure);
                        iLTScheduleCalenderDataExports.AddRange(result.ToList());
                        connection.Close();
                        foreach (ILTScheduleCalenderDataExport item in iLTScheduleCalenderDataExports)
                        {

                            if (!string.IsNullOrEmpty(item.FromTimeZone) && !string.IsNullOrEmpty(item.ToTimeZone))
                            {
                                if (item.FromTimeZone != item.ToTimeZone)
                                {
                                    string from = null, to = null;
                                    if (_tzList.Where(a => a.Text == item.FromTimeZone).FirstOrDefault() != null)
                                        from = _tzList.Where(a => a.Text == item.FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (_tzList.Where(a => a.Text == item.ToTimeZone).FirstOrDefault() != null)
                                        to = _tzList.Where(a => a.Text == item.ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        item.StartDate = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.StartDate), from), to)));
                                        item.EndDate = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.EndDate), from), to)));
                                        item.StartTime = TimeSpan.Parse(Convert.ToString(item.StartDate.TimeOfDay));
                                        item.EndTime = TimeSpan.Parse(Convert.ToString(item.EndDate.TimeOfDay));

                                    }

                                }
                            }

                        }
                    }



                    return iLTScheduleCalenderDataExports;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
        public async Task<List<ILTScheduleCalenderDataExport>> GetILTCalenderDataForTrainerExport(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                List<ILTScheduleCalenderDataExport> iLTScheduleCalenderDataExports = new List<ILTScheduleCalenderDataExport>();
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    List<ApiCalendarV2> apiCalender = new List<ApiCalendarV2>();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    parameters.Add("@OrgCode", OrgCode);
                    string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var resulttrainer = await SqlMapper.QueryAsync<ILTScheduleCalenderDataExport>((SqlConnection)connection, "[dbo].[GetMyILTBatchCalendarDataForTrainer]", parameters, null, null, CommandType.StoredProcedure);
                        iLTScheduleCalenderDataExports.AddRange(resulttrainer.ToList());
                        connection.Close();
                    }
                    else
                    {
                        var resulttrainer = await SqlMapper.QueryAsync<ILTScheduleCalenderDataExport>((SqlConnection)connection, "[dbo].[GetMyILTScheduleCalendarDataForTrainer]", parameters, null, null, CommandType.StoredProcedure);
                        iLTScheduleCalenderDataExports.AddRange(resulttrainer.ToList());


                        foreach (ILTScheduleCalenderDataExport item in iLTScheduleCalenderDataExports)
                        {

                            if (!string.IsNullOrEmpty(item.FromTimeZone) && !string.IsNullOrEmpty(item.ToTimeZone))
                            {
                                if (item.FromTimeZone != item.ToTimeZone)
                                {
                                    string from = null, to = null;
                                    if (_tzList.Where(a => a.Text == item.FromTimeZone).FirstOrDefault() != null)
                                        from = _tzList.Where(a => a.Text == item.FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (_tzList.Where(a => a.Text == item.ToTimeZone).FirstOrDefault() != null)
                                        to = _tzList.Where(a => a.Text == item.ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();


                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        item.StartDate = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.StartDate), from), to)));
                                        item.EndDate = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.EndDate), from), to)));
                                        item.StartTime = TimeSpan.Parse(Convert.ToString(item.StartDate.TimeOfDay));
                                        item.EndTime = TimeSpan.Parse(Convert.ToString(item.EndDate.TimeOfDay));

                                    }

                                }
                            }
                        }
                        connection.Close();
                    }
                    return iLTScheduleCalenderDataExports;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
        public async Task<List<ApiCalendarV2ForTrainer>> GetILTScheduleCalendarDataForTrainer(int userId, string OrgCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                using (ReportDbContext dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    List<ApiCalendarV2ForTrainer> apiCalender = new List<ApiCalendarV2ForTrainer>();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@FromDate", fromDate);
                    parameters.Add("@ToDate", toDate);
                    parameters.Add("@OrgCode", OrgCode);
                    string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var result = await SqlMapper.QueryAsync<ApiCalendarV3>((SqlConnection)connection, "[dbo].[GetMyILTBatchCalendarDataForTrainer]", parameters, null, null, CommandType.StoredProcedure);
                        List<ApiCalendarV3> ApiCalendarV3List = result.ToList();
                        connection.Close();

                        List<ApiCalendarV2ForTrainer> apiCalendarV2List = ApiCalendarV3List.GroupBy(x => new
                        {
                            x.CourseId,
                            x.CourseCode,
                            x.CourseTitle,
                            x.BatchId,
                            x.BatchCode,
                            x.BatchName,
                            x.BatchStartDate,
                            x.BatchEndDate,
                            x.BatchStartTime,
                            x.BatchEndTime,
                            x.FromTimeZone,
                            x.ToTimeZone,
                            x.ModuleType
                        }).Select(x => x.FirstOrDefault()).Select(cal => new ApiCalendarV2ForTrainer
                        {
                            CourseId = cal.CourseId,
                            Start = cal.BatchStartDate,
                            End = cal.BatchEndDate,
                            StartTimes = cal.BatchStartTime.ToString(@"hh\:mm"),
                            EndTimes = cal.BatchEndTime.ToString(@"hh\:mm"),
                            Title = cal.BatchName + " (" + cal.CourseTitle + ")",
                            CourseCode = cal.CourseCode,
                            CourseName = cal.CourseTitle,
                            BatchId = cal.BatchId,
                            BatchCode = cal.BatchCode,
                            BatchName = cal.BatchName,
                            FromTimeZone = cal.FromTimeZone,
                            ToTimeZone = cal.ToTimeZone,
                            ModuleType = cal.ModuleType
                        }).ToList();

                        foreach (ApiCalendarV2ForTrainer calendarobj in apiCalendarV2List)
                        {
                            List<APICalendarSchedules> aPICalendarSchedulesList = ApiCalendarV3List.Where(x => x.BatchId == calendarobj.BatchId)
                                                                                                .Select(sch => new APICalendarSchedules
                                                                                                {
                                                                                                    CourseCode = sch.CourseCode,
                                                                                                    CourseName = sch.CourseTitle,
                                                                                                    ModuleName = sch.ModuleName,
                                                                                                    ScheduleCode = sch.ScheduleCode,
                                                                                                    Start = sch.ScheduleStartDate.ToString(),
                                                                                                    End = sch.ScheduleEndDate.ToString(),
                                                                                                    StartTime = sch.ScheduleStartTime.ToString(@"hh\:mm"),
                                                                                                    EndTime = sch.ScheduleEndTime.ToString(@"hh\:mm"),
                                                                                                    Currency = sch.Cost.ToString() + " " + sch.Currency,
                                                                                                    PlaceName = sch.PlaceName + ", " + sch.Cityname + "," + sch.PostalAddress,
                                                                                                    Title = sch.ScheduleCode,
                                                                                                    FromTimeZone = sch.FromTimeZone,
                                                                                                    ToTimeZone = sch.ToTimeZone,
                                                                                                    ModuleType = sch.ModuleType
                                                                                                }).ToList();
                            calendarobj.aPICalendarSchedulesList = aPICalendarSchedulesList;
                            apiCalender.Add(calendarobj);
                        }
                    }
                    else
                    {
                        IEnumerable<ILTScheduleCalenderData> result = await SqlMapper.QueryAsync<ILTScheduleCalenderData>((SqlConnection)connection, "[dbo].[GetMyILTScheduleCalendarDataForTrainer]", parameters, null, null, CommandType.StoredProcedure);
                        List<ILTScheduleCalenderData> ILTScheduleCalenderData = new List<ILTScheduleCalenderData>();
                        ILTScheduleCalenderData = result.ToList();
                        connection.Close();
                        foreach (ILTScheduleCalenderData iltSchedule in ILTScheduleCalenderData)
                        {
                            ApiCalendarV2ForTrainer calender = new ApiCalendarV2ForTrainer
                            {
                                CourseId = iltSchedule.Id,
                                CourseCode = iltSchedule.CourseCode,
                                CourseName = iltSchedule.CourseTitle,
                                ModuleName = iltSchedule.ModuleName,
                                ModuleType = iltSchedule.ModuleType,
                                ScheduleCode = iltSchedule.ScheduleCode,
                                Start = iltSchedule.StartDate,
                                End = iltSchedule.EndDate,
                                Title = iltSchedule.CourseTitle,
                                StartTimes = iltSchedule.StartTime.ToString(@"hh\:mm"),
                                EndTimes = iltSchedule.EndTime.ToString(@"hh\:mm"),
                                Currency = iltSchedule.Cost.ToString() + " " + iltSchedule.Currency,
                                PlaceName = iltSchedule.PlaceName + ", " + iltSchedule.Cityname + "," + iltSchedule.PostalAddress,
                                FromTimeZone = iltSchedule.FromTimeZone,
                                ToTimeZone = iltSchedule.ToTimeZone
                            };
                            apiCalender.Add(calender);
                        }
                        foreach (ApiCalendarV2ForTrainer item in apiCalender)
                        {

                            if (!string.IsNullOrEmpty(item.FromTimeZone) && !string.IsNullOrEmpty(item.ToTimeZone))
                            {
                                if (item.FromTimeZone != item.ToTimeZone)
                                {
                                    string from = null, to = null;
                                    if (_tzList.Where(a => a.Text == item.FromTimeZone).FirstOrDefault() != null)
                                        from = _tzList.Where(a => a.Text == item.FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();

                                    if (_tzList.Where(a => a.Text == item.ToTimeZone).FirstOrDefault() != null)
                                        to = _tzList.Where(a => a.Text == item.ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();


                                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                                    {
                                        item.Start = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.Start), from), to)));
                                        item.End = Convert.ToDateTime(Convert.ToString(TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.End), from), to)));
                                        item.StartTimes = TimeSpan.Parse(Convert.ToString(item.Start.TimeOfDay)).ToString(@"hh\:mm");
                                        item.EndTimes = TimeSpan.Parse(Convert.ToString(item.End.TimeOfDay)).ToString(@"hh\:mm");

                                    }
                                }
                            }
                        }
                    }


                    return apiCalender;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnectionRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }
        public string GetWebinarLink(string ScheduleCode)
        {
            ILTSchedule iLTSchedule = _db.ILTSchedule.Where(a => a.ScheduleCode == ScheduleCode && a.IsDeleted == false).FirstOrDefault();

            if (iLTSchedule.WebinarType != null)
            {
                if (iLTSchedule.WebinarType.ToLower() == "teams")
                {
                    TeamsScheduleDetails teamsScheduleDetails = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID && a.IsDeleted == false).FirstOrDefault();
                    if (teamsScheduleDetails != null)
                        return teamsScheduleDetails.JoinUrl;
                }
                if (iLTSchedule.WebinarType.ToLower() == "zoom")
                {
                    ZoomMeetingDetails zoomMeetingDetails = _db.ZoomMeetingDetails.Where(a => a.ScheduleID == iLTSchedule.ID && a.IsDeleted == false).FirstOrDefault();
                    if (zoomMeetingDetails != null)
                        return zoomMeetingDetails.Join_url;
                }
                return null;
            }
            return null;
        }
    }
}
