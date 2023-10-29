using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using ILT.API.Model.ILT;
using ILT.API.Repositories.Interfaces;

namespace ILT.API.Repositories
{
    public class ScheduleVisibilityRuleRepository : Repository<ScheduleVisibilityRule>, IScheduleVisibilityRule
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ScheduleVisibilityRuleRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;     
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        private readonly ITLSHelper _tlsHelper;
        private readonly INotification _notification;

        public ScheduleVisibilityRuleRepository(CourseContext context, IConfiguration configuration,         
            ICustomerConnectionStringRepository customerConnectionRepository, 
            ITLSHelper tlsHelper, INotification notification) : base(context)
        {
            _configuration = configuration;
            _db = context;           
            _customerConnectionRepository = customerConnectionRepository;
            _tlsHelper = tlsHelper;
            _notification = notification;

        }

   
        public async Task<List<object>> Get(int page, int pageSize, string search = null, string filter = null)
        {

            var Query = _db.ScheduleVisibilityRule.Join(_db.ILTSchedule, r => r.ScheduleId, (p => p.ID), (r, p) => new { r, p })

                       .Where(c => (c.r.IsDeleted == false) && (search == null || c.p.ScheduleCode.StartsWith(search)) && c.p.IsDeleted == false) // && c.p.IsActive==true)
                        .GroupBy(od => new
                        {
                            od.p.ID,
                            od.r.ScheduleId,
                            od.p.ScheduleCode,
                           // od.p.Code,
                            od.p.IsActive
                        })
                     .OrderByDescending(a => a.Key.ID)
                       .Select(m => new APIScheduleRegiLimit
                       {
                           Id = m.Key.ID,
                           ScheduleId = m.Key.ScheduleId,
                           scheduleCode = m.Key.ScheduleCode,
                         //  courseCode = m.Key.Code,
                           courseStatus = m.Key.IsActive
                       });

            if (page != -1)
                Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
            if (pageSize != -1)
                Query = Query.Take(Convert.ToInt32(pageSize));

            return await Query.ToListAsync<object>();
        }

        

        public async Task<int> count(string search = null, string filter = null)
        {
            var Query = (from accessibiltyRule in _db.ScheduleVisibilityRule
                         join schedule in _db.ILTSchedule on accessibiltyRule.ScheduleId equals schedule.ID
                        
                         where accessibiltyRule.IsDeleted == false && (search == null || schedule.ScheduleCode.StartsWith(search)) && schedule.IsDeleted == false //&& course.IsActive == true
                         select new { accessibiltyRule.Id, accessibiltyRule.ScheduleId, schedule.ScheduleCode }

                         );


            return await Query.Select(r => r.ScheduleCode).Distinct().CountAsync();
        }

        public async Task<List<object>> GetSchedule(int page, int pageSize, string search = null, string filter = null)
        {
            try
            {

                var Query = (from c in _db.Course
                             join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
                             join m in _db.Module on ma.ModuleId equals m.Id
                             join s in _db.ILTSchedule on new { id = m.Id, cid = ma.CourseId } equals new { id = s.ModuleId, cid = s.CourseId }
                             join l in _db.TrainingPlace on s.PlaceID equals l.Id

                             where c.IsDeleted == false && m.ModuleType == "classroom"
                         //    && new DateTime(s.StartDate.Year, s.StartDate.Month, s.StartDate.Day, s.StartTime.Hours, s.StartTime.Minutes, s.StartTime.Seconds, s.StartTime.Milliseconds) > DateTime.UtcNow


                             select new APIGetAllSchedule
                             {
                                 Title = c.Title,
                                 ModuleNamne = m.Name,
                                 CourseCode = c.Code,
                                 ScheduleId = s.ID,
                                 Schedulecode = s.ScheduleCode,
                                 StartDate = Convert.ToString(s.StartDate.Date),
                                 Type = m.ModuleType,
                                 Venue = l.PlaceName,
                                 usercount = this._db.ScheduleVisibilityRule.Where(x => x.ScheduleId == s.ID && x.IsDeleted == false).Select(x => x.Id).Count() > 0 ? true : false,
                             });


                if (filter == "null")
                    filter = null;
                if (search == "null")
                    search = null;
                if (!string.IsNullOrEmpty(search))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (filter.ToLower().Equals("code"))
                            Query = Query.Where(r => r.Schedulecode.StartsWith(search));
                        if (filter.ToLower().Equals("title"))
                            Query = Query.Where(r => r.Title.Contains(search));
                        if (filter.ToLower().Equals("coursetype"))
                            Query = Query.Where(r => r.Type.Contains(search));
                        if (filter.ToLower().Equals("modulename"))
                            Query = Query.Where(r => r.ModuleNamne.StartsWith(search));

                    }
                    else
                    {
                        Query = Query.Where(r => r.Title.Contains(search) || r.Schedulecode.Contains(search) || r.ModuleNamne.Contains(search));
                    }
                }


                if (page != -1)
                    Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
                if (pageSize != -1)
                    Query = Query.Take(Convert.ToInt32(pageSize));

                return await Query.ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }


        public async Task<APITotalGetAllSchedule> GetSchedules(ApiGetSchedules apiGetSchedules, int userId, string userRole)
        {
            try
            {
                DateTime datecurrent = DateTime.UtcNow;
                string date = datecurrent.ToString("dd-MM-yyyy");
                string format = "dd-MM-yyyy";
                DateTime parsedDate = DateTime.ParseExact(date, format, null);
                APITotalGetAllSchedule aPITotalGetAllSchedule = new APITotalGetAllSchedule();
                UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
                
                var Query = (from c in _db.Course
                             join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
                             join m in _db.Module on ma.ModuleId equals m.Id
                             join s in _db.ILTSchedule on new { id = m.Id, cid = ma.CourseId } equals new { id = s.ModuleId, cid = s.CourseId }
                             join l in _db.TrainingPlace on s.PlaceID equals l.Id
                             into TrainingPlaceLeftJoin
                             from place in TrainingPlaceLeftJoin.DefaultIfEmpty()
                             join user in _db.UserMaster on s.ModifiedBy equals user.Id
                             join umd in _db.UserMasterDetails on s.CreatedBy equals umd.UserMasterId
                             where c.IsDeleted == false &&  
                              ((DateTime.Compare(s.StartDate.Date, parsedDate.Date) >= 0) && (TimeSpan.Compare(s.StartTime, parsedDate.TimeOfDay) >= 0))
                             //    && new DateTime(s.StartDate.Year, s.StartDate.Month, s.StartDate.Day, s.StartTime.Hours, s.StartTime.Minutes, s.StartTime.Seconds, s.StartTime.Milliseconds) > DateTime.UtcNow


                             select new APIGetAllSchedulewithusers
                             {                                 
                                 Title = c.Title,
                                 ModuleNamne = m.Name,
                                 CourseCode = c.Code,
                                 ScheduleId = s.ID,
                                 Schedulecode = s.ScheduleCode,
                                 StartDate = Convert.ToString(s.StartDate.Date),
                                 Type = m.ModuleType,
                                 Venue = place.PlaceName,
                                 usercount = this._db.ScheduleVisibilityRule.Where(x => x.ScheduleId == s.ID && x.IsDeleted == false).Select(x => x.Id).Count() > 0 ? true : false,
                                 UserName = user.UserName,
                                 AreaId = umd.AreaId,
                                 LocationId = umd.LocationId,
                                 GroupId = umd.GroupId,
                                 BusinessId = umd.BusinessId,
                                 CreatedBy = s.CreatedBy,
                                 UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (s.CreatedBy == userId) ? true : false : true
                             });


                if (apiGetSchedules.filter == "null")
                    apiGetSchedules.filter = null;
                if (apiGetSchedules.search == "null")
                    apiGetSchedules.search = null;
                if (!string.IsNullOrEmpty(apiGetSchedules.search))
                {
                    if (!string.IsNullOrEmpty(apiGetSchedules.filter))
                    {
                        if (apiGetSchedules.filter.ToLower().Equals("code"))
                            Query = Query.Where(r => r.Schedulecode.StartsWith(apiGetSchedules.search));
                        if (apiGetSchedules.filter.ToLower().Equals("title"))
                            Query = Query.Where(r => r.Title.Contains(apiGetSchedules.search));
                        if (apiGetSchedules.filter.ToLower().Equals("coursetype"))
                            Query = Query.Where(r => r.Type.Contains(apiGetSchedules.search));
                        if (apiGetSchedules.filter.ToLower().Equals("modulename"))
                            Query = Query.Where(r => r.ModuleNamne.StartsWith(apiGetSchedules.search));

                    }
                    else
                    {
                        Query = Query.Where(r => r.Title.Contains(apiGetSchedules.search) || r.Schedulecode.Contains(apiGetSchedules.search) || r.ModuleNamne.Contains(apiGetSchedules.search));
                    }
                }
                if (userRole == UserRoles.BA)
                {
                    Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
                }
                if (userRole == UserRoles.GA)
                {
                    Query = Query.Where(r => r.GroupId == userdetails.GroupId);
                }
                if (userRole == UserRoles.LA)
                {
                    Query = Query.Where(r => r.LocationId == userdetails.LocationId);
                }
                if (userRole == UserRoles.AA)
                {
                    Query = Query.Where(r => r.AreaId == userdetails.AreaId);
                }
                if (apiGetSchedules.showAllData == false && (userRole != UserRoles.CA))
                {
                    Query = Query.Where(r => r.CreatedBy == userId);
                }
                Query = Query.OrderByDescending(a => a.ScheduleId);
                aPITotalGetAllSchedule.TotalRecords = await Query.CountAsync();

                if (apiGetSchedules.page != -1)
                    Query = Query.Skip((Convert.ToInt32(apiGetSchedules.page) - 1) * Convert.ToInt32(apiGetSchedules.pageSize));
                if (apiGetSchedules.pageSize != -1)
                    Query = Query.Take(Convert.ToInt32(apiGetSchedules.pageSize));

                DateTime dateTime = DateTime.Now;
                List<APIGetAllSchedulewithusers> aPIGetAllSchedulewithusers = await Query.ToListAsync();
                aPITotalGetAllSchedule.Data = aPIGetAllSchedulewithusers;


                return aPITotalGetAllSchedule;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }

        public async Task<int> Schedulecount(string search = null, string filter = null)
        {
            var Query = (from c in _db.Course
                         join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
                         join m in _db.Module on ma.ModuleId equals m.Id
                         join s in _db.ILTSchedule on new { id = m.Id, cid = ma.CourseId } equals new { id = s.ModuleId, cid = s.CourseId }
                         
                         
                         where c.IsDeleted == false && m.ModuleType == "classroom"
                         //&& new DateTime(s.StartDate.Year, s.StartDate.Month, s.StartDate.Day,s.StartTime.Hours,s.StartTime.Minutes,s.StartTime.Seconds,s.StartTime.Milliseconds) > DateTime.UtcNow
                         select new APIGetAllSchedule
                         {
                             Title = c.Title,
                             ModuleNamne = m.Name,
                             CourseCode = c.Code,
                             ScheduleId = s.ID,
                             Schedulecode = s.ScheduleCode,
                             StartDate = Convert.ToString(s.StartDate.Date),
                             Type = m.ModuleType,                             
                             usercount = true
                         });


            if (filter == "null")
                filter = null;
            if (search == "null")
                search = null;
            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (filter.ToLower().Equals("code"))
                        Query = Query.Where(r => r.Schedulecode.StartsWith(search));
                    if (filter.ToLower().Equals("title"))
                        Query = Query.Where(r => r.Title.Contains(search));
                    if (filter.ToLower().Equals("coursetype"))
                        Query = Query.Where(r => r.Type.Contains(search));
                    if (filter.ToLower().Equals("modulename"))
                        Query = Query.Where(r => r.ModuleNamne.StartsWith(search));

                }
                else
                {
                    Query = Query.Where(r => r.Title.Contains(search) || r.Schedulecode.Contains(search) || r.ModuleNamne.Contains(search));
                }
            }


            return await Query.Select(r => r.ScheduleId).Distinct().CountAsync();
        }

        public async Task<int> DeleteRule(int roleId)
        {
            ScheduleVisibilityRule scheduleVisibility = await this.Get(roleId);
            if (scheduleVisibility != null)
            {
                scheduleVisibility.IsDeleted = true;
                await this.Update(scheduleVisibility);
                return 1;
            }
            return 0;
        }

        public async Task<List<visibilityRules>> Post(APIScheduleVisibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null)
        {
            
            List<visibilityRules> Duplicates = new List<visibilityRules>();
            visibilityRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
            visibilityRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
            if (AndAccessibilityRules.Count() > 0)
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    ScheduleVisibilityRule scheduleRules = new ScheduleVisibilityRule
                    {
                        ScheduleId = apiAccessibility.ScheduleId,
                        ConditionForRules = "and",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = userId,
                        ModifiedBy = userId,
                        IsActive = true

                    };
                    if (!string.IsNullOrEmpty(apiAccessibility.UserTeamId))
                    {
                        scheduleRules.UserTeamId = Convert.ToInt32(apiAccessibility.UserTeamId);
                    }
                    else
                    {
                        scheduleRules.UserTeamId = null;
                    }

                    foreach (visibilityRules accessibility in AndAccessibilityRules)
                    {
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn1"))
                            scheduleRules.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn2"))
                            scheduleRules.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn3"))
                            scheduleRules.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn4"))
                            scheduleRules.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn5"))
                            scheduleRules.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn6"))
                            scheduleRules.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn7"))
                            scheduleRules.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn8"))
                            scheduleRules.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn9"))
                            scheduleRules.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn10"))
                            scheduleRules.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn11"))
                            scheduleRules.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn12"))
                            scheduleRules.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn13"))
                            scheduleRules.ConfigurationColumn13 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn14"))
                            scheduleRules.ConfigurationColumn14 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn15"))
                            scheduleRules.ConfigurationColumn15 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("area"))
                            scheduleRules.Area = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("business"))
                            scheduleRules.Business = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("emailid"))
                            scheduleRules.EmailID = accessibility.ParameterValue;
                        if (accessibility.AccessibilityRule.ToLower().Equals("location"))
                            scheduleRules.Location = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("group"))
                            scheduleRules.Group = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("userid"))
                            scheduleRules.UserID = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("mobilenumber"))
                            scheduleRules.MobileNumber = accessibility.ParameterValue;
                        if (accessibility.AccessibilityRule.ToLower().Equals("dateofjoining"))
                        {
                            scheduleRules.StartDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue).Date;
                            scheduleRules.EndDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue2).Date;
                        }

                    }

                    if (await RuleExist(scheduleRules))
                    {
                        Duplicates.Add(AndAccessibilityRules[0]);
                    }
                    else
                    {
                        await this.Add(scheduleRules);
                        #region notification
                        //#region "Send Email Notifications"
                        //url = _configuration[Configuration.NotificationApi];

                        //url = url + "/CourseApplicability";
                        //JObject oJsonObject = new JObject();
                        //oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                        //oJsonObject.Add("OrganizationCode", orgnizationCode);
                        //HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;
                        //#endregion

                        //#region "Send Bell Notifications"

                        //List<CourseApplicablityEmails> Emails = await this.GetCourseApplicabilityEmails(Convert.ToInt32(accessibilityRules.CourseId), orgnizationCode);

                        //var Title = dbContext.Course.Where(a => a.Id == accessibilityRules.CourseId).Select(a => a.Title).SingleOrDefault();
                        //bool IsApplicableToAll = dbContext.Course.Where(a => a.Id == accessibilityRules.CourseId).Select(a => a.IsApplicableToAll).SingleOrDefault();
                        //int notificationID = 0;

                        //List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId));

                        //if (aPINotification.Count > 0)
                        //    notificationID = aPINotification.FirstOrDefault().Id;
                        //else
                        //{
                        //    ApiNotification Notification = new ApiNotification();
                        //    Notification.Title = Title;
                        //    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                        //    Notification.Message = Notification.Message.Replace("{course}", Title);
                        //    Notification.Url = TlsUrl.NotificationAPost + accessibilityRules.CourseId;
                        //    Notification.Type = Record.Course;
                        //    Notification.UserId = userId;
                        //    Notification.CourseId = accessibilityRules.CourseId;
                        //    notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll);
                        //}
                        //DataTable dtUserIds = new DataTable();
                        //dtUserIds.Columns.Add("UserIds");

                        //foreach (var result in Emails)
                        //{
                        //    dtUserIds.Rows.Add(result.UserId);
                        //}
                        //if (dtUserIds.Rows.Count > 0)
                        //    await this.SendDataForApplicableNotifications(notificationID, dtUserIds, userId);
                        //#endregion

                        //#region "Send Push Notifications"
                        //url = _configuration[Configuration.NotificationApi];
                        //url += "/CourseApplicabilityPushNotification";
                        //JObject Pushnotification = new JObject();
                        //Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                        //Pushnotification.Add("OrganizationCode", orgnizationCode);
                        //HttpResponseMessage responses1 = CallAPI(url, Pushnotification).Result;
                        //#endregion

                        //#region "Send SMS Notifications"
                        //var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
                        //if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        //{
                        //    urlSMS = _configuration[Configuration.NotificationApi];
                        //    urlSMS += "/CourseApplicabilitySMS";
                        //    JObject oJsonObjectSMS = new JObject();
                        //    oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                        //    oJsonObjectSMS.Add("OrganizationCode", orgnizationCode);
                        //    HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                        //}
                        //#endregion
                        #endregion

                    }
                }
            }
            if (OrAccessibilityRules.Count() > 0)
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    foreach (visibilityRules accessibility in OrAccessibilityRules)
                    {
                        ScheduleVisibilityRule accessibilityRule = new ScheduleVisibilityRule
                        {
                            ScheduleId = apiAccessibility.ScheduleId
                        };

                        if (!string.IsNullOrEmpty(apiAccessibility.UserTeamId))
                        {
                            accessibilityRule.UserTeamId = Convert.ToInt32(apiAccessibility.UserTeamId);
                        }
                        else
                        {
                            accessibilityRule.UserTeamId = null;
                        }

                        if (!accessibility.Condition.Equals("null"))
                            accessibilityRule.ConditionForRules = "or";
                        accessibilityRule.CreatedDate = DateTime.UtcNow;
                        bool RecordExist = false;
                        string columnName = accessibility.AccessibilityRule.ToLower();
                        var Query = dbContext.ScheduleVisibilityRule.Where(a => a.ScheduleId == apiAccessibility.ScheduleId && a.IsDeleted == false);
                        switch (columnName)
                        {
                            case "configurationcolumn1":
                                accessibilityRule.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn1 == accessibilityRule.ConfigurationColumn1);
                                break;
                            case "configurationcolumn2":

                                accessibilityRule.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn2 == accessibilityRule.ConfigurationColumn2);
                                break;
                            case "configurationcolumn3":
                                accessibilityRule.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn3 == accessibilityRule.ConfigurationColumn3);
                                break;
                            case "configurationcolumn4":
                                accessibilityRule.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn4 == accessibilityRule.ConfigurationColumn4);
                                break;
                            case "configurationcolumn5":
                                accessibilityRule.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn5 == accessibilityRule.ConfigurationColumn5);
                                break;
                            case "configurationcolumn6":
                                accessibilityRule.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn6 == accessibilityRule.ConfigurationColumn6);
                                break;
                            case "configurationcolumn7":
                                accessibilityRule.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn7 == accessibilityRule.ConfigurationColumn7);
                                break;
                            case "configurationcolumn8":
                                accessibilityRule.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn8 == accessibilityRule.ConfigurationColumn8);
                                break;
                            case "configurationcolumn9":
                                accessibilityRule.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn9 == accessibilityRule.ConfigurationColumn9);
                                break;
                            case "configurationcolumn10":
                                accessibilityRule.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn10 == accessibilityRule.ConfigurationColumn10);
                                break;
                            case "configurationcolumn11":
                                accessibilityRule.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn11 == accessibilityRule.ConfigurationColumn11);
                                break;
                            case "configurationcolumn12":
                                accessibilityRule.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.ConfigurationColumn12 == accessibilityRule.ConfigurationColumn12);
                                break;
                            case "area":
                                accessibilityRule.Area = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.Area == accessibilityRule.Area);
                                break;
                            case "business":
                                accessibilityRule.Business = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.Business == accessibilityRule.Business);
                                break;
                            case "emailid":
                                accessibilityRule.EmailID = accessibility.ParameterValue;
                                Query = Query.Where(x => x.EmailID == accessibilityRule.EmailID);
                                break;
                            case "location":
                                accessibilityRule.Location = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.Location == accessibilityRule.Location);
                                break;
                            case "group":
                                accessibilityRule.Group = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.Group == accessibilityRule.Group && x.IsDeleted == Record.NotDeleted);
                                break;
                            case "userid":
                                accessibilityRule.UserID = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.UserID == accessibilityRule.UserID && x.IsDeleted == Record.NotDeleted);
                                break;
                            case "mobilenumber":
                                accessibilityRule.MobileNumber = accessibility.ParameterValue;
                                Query = Query.Where(x => x.MobileNumber == accessibilityRule.MobileNumber && x.IsDeleted == Record.NotDeleted);
                                break;
                            case "dateofjoining":
                                accessibilityRule.StartDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue).Date;
                                accessibilityRule.EndDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue2).Date;
                                Query = Query.Where(x => x.StartDateOfJoining <= accessibilityRule.StartDateOfJoining && x.EndDateOfJoining >= accessibilityRule.EndDateOfJoining && x.IsDeleted == Record.NotDeleted);
                                break;
                        }
                        RecordExist = Query.Count() > 0 ? true : false;
                        if (!RecordExist)
                        {
                            await this.Add(accessibilityRule);
                            #region notification
                            //url = _configuration[Configuration.NotificationApi];
                            //url += "/CourseApplicability";
                            //JObject oJsonObject = new JObject();
                            //oJsonObject.Add("CourseId", accessibilityRule.CourseId);
                            //oJsonObject.Add("OrganizationCode", orgnizationCode);
                            //HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                            //url = _configuration[Configuration.NotificationApi];
                            //url += "/CourseApplicabilityPushNotification";
                            //JObject Pushnotification = new JObject();
                            //Pushnotification.Add("CourseId", accessibilityRule.CourseId);
                            //Pushnotification.Add("OrganizationCode", orgnizationCode);
                            //HttpResponseMessage responses1 = CallAPI(url, Pushnotification).Result;

                            //int notificationUserId = Convert.ToInt32(accessibilityRule.UserID);

                            //List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRule.CourseId));
                            //if (aPINotification != null)
                            //{
                            //    var Title = dbContext.Course.Where(a => a.Id == accessibilityRule.CourseId).Select(a => a.Title).SingleOrDefault();
                            //    bool IsApplicableToAll = dbContext.Course.Where(a => a.Id == accessibilityRule.CourseId).Select(a => a.IsApplicableToAll).SingleOrDefault();

                            //    ApiNotification Notification = new ApiNotification();
                            //    Notification.Title = Title;
                            //    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                            //    Notification.Message = Notification.Message.Replace("{course}", Title);
                            //    Notification.Url = TlsUrl.NotificationAPost + accessibilityRule.CourseId;
                            //    Notification.Type = Record.Course;

                            //    int NotificationId = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll);

                            //    DataTable dtUserIds = new DataTable();
                            //    dtUserIds.Columns.Add("UserIds");
                            //    dtUserIds.Rows.Add(notificationUserId);
                            //    await this.SendDataForApplicableNotifications(NotificationId, dtUserIds, userId);

                            //}

                            //var SendSMSToUser = GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
                            //if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            //{
                            //    urlSMS = _configuration[Configuration.NotificationApi];

                            //    urlSMS += "/CourseApplicabilitySMS";
                            //    JObject oJsonObjectSMS = new JObject();
                            //    oJsonObjectSMS.Add("CourseId", accessibilityRule.CourseId);
                            //    oJsonObjectSMS.Add("OrganizationCode", orgnizationCode);
                            //    HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                            //}
                            #endregion
                        }
                        else
                        {
                            Duplicates.Add(accessibility);
                        }
                    }
                }
            }
            if (Duplicates.Count > 0)
                return Duplicates;
            return null;

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

      
        public async Task<int> CourseCount(string search = null)
        {
            var CourseNames = (from accessibiltyRule in _db.AccessibilityRule
                               join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                               into c
                               from course in c.DefaultIfEmpty()
                               where accessibiltyRule.IsDeleted == false && course.IsDeleted == false
                               select new { course.Title } into t1
                               group t1 by new { t1.Title } into result
                               select result.FirstOrDefault().Title);
            if (!string.IsNullOrEmpty(search))
            {
                CourseNames = CourseNames.Where(c => c.Contains(search));
            }
            return await CourseNames.CountAsync();
        }

        public async Task<int> GetAccessibilityRulesCount(int scheduleId)
        {
            int Count = 0;
            int TeamCount = 0;
            Count = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                           join schedule in _db.ILTSchedule on accessibiltyRule.ScheduleId equals schedule.ID
                         
                           where accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.UserTeamId == null || accessibiltyRule.UserTeamId == 0)
                           select new
                           {
                               accessibiltyRule.ConfigurationColumn1,
                               accessibiltyRule.ConfigurationColumn2,
                               accessibiltyRule.ConfigurationColumn3,
                               accessibiltyRule.ConfigurationColumn4,
                               accessibiltyRule.ConfigurationColumn5,
                               accessibiltyRule.ConfigurationColumn6,
                               accessibiltyRule.ConfigurationColumn7,
                               accessibiltyRule.ConfigurationColumn8,
                               accessibiltyRule.ConfigurationColumn9,
                               accessibiltyRule.ConfigurationColumn10,
                               accessibiltyRule.ConfigurationColumn11,
                               accessibiltyRule.ConfigurationColumn12,
                               accessibiltyRule.Area,
                               accessibiltyRule.Business,
                               accessibiltyRule.EmailID,
                               accessibiltyRule.MobileNumber,
                               accessibiltyRule.Location,
                               accessibiltyRule.Group,
                               accessibiltyRule.UserID,
                               accessibiltyRule.ConditionForRules,
                               accessibiltyRule.ScheduleId,
                               accessibiltyRule.Id,
                               schedule.ScheduleCode
                           }).CountAsync();

            TeamCount = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                               join course in _db.ILTSchedule on accessibiltyRule.ScheduleId equals course.ID

                               join applicabilityGroupTemplate in _db.UserTeamsMapping on accessibiltyRule.UserTeamId equals applicabilityGroupTemplate.UserTeamId
                               into d
                               from applicabilityGroupTemplate in d.DefaultIfEmpty()

                               join userteam in _db.UserTeams on applicabilityGroupTemplate.UserTeamId equals userteam.Id
                             
                               where (accessibiltyRule.UserTeamId != null && accessibiltyRule.UserTeamId != 0) && accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false
                               select new
                               {
                                   accessibiltyRule.ScheduleId,
                                   accessibiltyRule.Id,
                                   course.ScheduleCode,
                                   accessibiltyRule.UserTeamId,
                                   userteam.TeamName
                               }).Distinct().CountAsync();
            
            return Count+ TeamCount;
        }

      
        public async Task<APIScheduleVisibilityRulesTotal> GetAccessibilityRules(int scheduleId, string orgnizationCode, string token, int Page, int PageSize,int userId, string userRole,bool showAllData)
        {
            try
            {
                APITotalGetAllSchedule aPITotalGetAllSchedule = new APITotalGetAllSchedule();
                UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
                var Result = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                                    join schedule in _db.ILTSchedule on accessibiltyRule.ScheduleId equals schedule.ID
                                    join user in _db.UserMaster on accessibiltyRule.ModifiedBy equals user.Id
                                    join umd in _db.UserMasterDetails on accessibiltyRule.CreatedBy equals umd.UserMasterId

                                    where accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.UserTeamId == null || accessibiltyRule.UserTeamId == 0)
                                    select new
                                    {
                                        accessibiltyRule.ConfigurationColumn1,
                                        accessibiltyRule.ConfigurationColumn2,
                                        accessibiltyRule.ConfigurationColumn3,
                                        accessibiltyRule.ConfigurationColumn4,
                                        accessibiltyRule.ConfigurationColumn5,
                                        accessibiltyRule.ConfigurationColumn6,
                                        accessibiltyRule.ConfigurationColumn7,
                                        accessibiltyRule.ConfigurationColumn8,
                                        accessibiltyRule.ConfigurationColumn9,
                                        accessibiltyRule.ConfigurationColumn10,
                                        accessibiltyRule.ConfigurationColumn11,
                                        accessibiltyRule.ConfigurationColumn12,
                                        accessibiltyRule.Area,
                                        accessibiltyRule.Business,
                                        accessibiltyRule.EmailID,
                                        accessibiltyRule.MobileNumber,
                                        accessibiltyRule.Location,
                                        accessibiltyRule.Group,
                                        accessibiltyRule.UserID,
                                        accessibiltyRule.ConditionForRules,
                                        accessibiltyRule.ScheduleId,
                                        accessibiltyRule.Id,
                                        schedule.ScheduleCode,
                                        accessibiltyRule.StartDateOfJoining,
                                        accessibiltyRule.EndDateOfJoining,

                                        UserName = user.UserName,
                                        AreaId = umd.AreaId,
                                        LocationId = umd.LocationId,
                                        GroupId = umd.GroupId,
                                        BusinessId = umd.BusinessId,
                                        CreatedBy = accessibiltyRule.CreatedBy,
                                        UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (accessibiltyRule.CreatedBy == userId) ? true : false : true

                                    }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

                var ResultForGroupApplicability = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                                                         join course in _db.ILTSchedule on accessibiltyRule.ScheduleId equals course.ID

                                                         join applicabilityGroupTemplate in _db.UserTeamsMapping on accessibiltyRule.UserTeamId equals applicabilityGroupTemplate.UserTeamId
                                                         into d
                                                         from applicabilityGroupTemplate in d.DefaultIfEmpty()

                                                         join userteam in _db.UserTeams on applicabilityGroupTemplate.UserTeamId equals userteam.Id
                                                         join user in _db.UserMaster on accessibiltyRule.ModifiedBy equals user.Id
                                                         join umd in _db.UserMasterDetails on accessibiltyRule.CreatedBy equals umd.UserMasterId

                                                         where (accessibiltyRule.UserTeamId != null && accessibiltyRule.UserTeamId != 0) && accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false
                                                         select new
                                                         {
                                                             accessibiltyRule.ScheduleId,
                                                             accessibiltyRule.Id,
                                                             course.ScheduleCode,
                                                             accessibiltyRule.UserTeamId,
                                                             userteam.TeamName,

                                                             UserName = user.UserName,
                                                             AreaId = umd.AreaId,
                                                             LocationId = umd.LocationId,
                                                             GroupId = umd.GroupId,
                                                             BusinessId = umd.BusinessId,
                                                             CreatedBy = accessibiltyRule.CreatedBy,
                                                             UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (accessibiltyRule.CreatedBy == userId) ? true : false : true

                                                         }).Distinct().ToListAsync();
                List<APIScheduleVisibilityRulesWithUserdata> AccessibilityRules = new List<APIScheduleVisibilityRulesWithUserdata>();
                       

                foreach (var AccessRule in Result)
                {
                    string Condition = AccessRule.ConditionForRules;
                    PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                    List<Rules> Rules = new List<Rules>();
                    int ScheduleId = 0;
                    int Id = 0;
                    int i = 0;
                    foreach (PropertyInfo rule in properties)
                    {
                        if (rule.Name.ToLower().Equals("scheduleid"))
                            ScheduleId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                        if (rule.Name.ToLower().Equals("id"))
                            Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                        if (rule.GetValue(AccessRule, null) != null &&
                            !rule.Name.Equals("ConditionForRules") &&
                            !rule.Name.Equals("ScheduleCode") &&
                            !rule.Name.Equals("ScheduleId") &&
                            !rule.Name.Equals("Id") &&
                            !rule.Name.Equals("UserName") &&
                            !rule.Name.Equals("AreaId") &&
                            !rule.Name.Equals("LocationId") &&
                            !rule.Name.Equals("GroupId") &&
                            !rule.Name.Equals("BusinessId") &&
                            !rule.Name.Equals("CreatedBy") &&
                            !rule.Name.Equals("UserCreated"))
                        {
                            if (string.Equals(rule.Name, "startdateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var applicationDateFormat = await GetMasterConfigurableParameterValue("APPLICATION_DATE_FORMAT");
                                Rules RuleDoj = new Rules
                                {
                                    AccessibilityParameter = "DateOfJoining",
                                    AccessibilityValue = Convert.ToDateTime(rule.GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                    AccessibilityValue2 = Convert.ToDateTime(properties[++i].GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                    Condition = Condition
                                };
                                Rules.Add(RuleDoj);
                            }
                            else if (!string.Equals(rule.Name, "enddateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Rules Rule = new Rules
                                {
                                    AccessibilityParameter = rule.Name,
                                    AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                                    Condition = Condition
                                };
                                Rules.Add(Rule);
                            }
                        }
                        i++;
                    }
                    if (Rules.Count == 2)
                    {
                        APIScheduleVisibilityRulesWithUserdata ApiRule = new APIScheduleVisibilityRulesWithUserdata
                        {
                            ScheduleId = scheduleId,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                            AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                            AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                            Condition1 = "and",
                            AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                            AccessibilityValueId2 = !string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(1).AccessibilityValue) : 0,
                            AccessibilityValue2 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue : null,
                            AccessibilityValue22 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue2 : null,
                            UserName = AccessRule.UserName,
                            UserCreated = AccessRule.UserCreated,
                            AreaId = AccessRule.AreaId,
                            LocationId = AccessRule.LocationId,
                            GroupId = AccessRule.GroupId,
                            BusinessId = AccessRule.BusinessId,
                            CreatedBy = AccessRule.CreatedBy,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    else if (Rules.Count == 1)
                    {
                        APIScheduleVisibilityRulesWithUserdata ApiRule = new APIScheduleVisibilityRulesWithUserdata
                        {
                            ScheduleId = scheduleId,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                            AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                            AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                            UserName = AccessRule.UserName,
                            UserCreated = AccessRule.UserCreated,
                            AreaId = AccessRule.AreaId,
                            LocationId = AccessRule.LocationId,
                            GroupId = AccessRule.GroupId,
                            BusinessId = AccessRule.BusinessId,
                            CreatedBy = AccessRule.CreatedBy,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                }
                string UserUrls = _configuration[APIHelper.UserAPI];
                string settings = "setting/1/20/";
                UserUrls += settings;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
                List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
                }
                foreach (APIScheduleVisibilityRulesWithUserdata AccessRule in AccessibilityRules)
                {
                    string UserUrl = _configuration[APIHelper.UserAPI];
                    string NameById = "GetNameById";
                    string ColumnName = AccessRule.AccessibilityParameter1;
                    int Value = AccessRule.AccessibilityValueId1;
                    string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                    response = await APIHelper.CallGetAPI(Apiurl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                    }
                    if (AccessRule.AccessibilityValueId2 != 0)
                    {
                        ColumnName = AccessRule.AccessibilityParameter2;
                        Value = AccessRule.AccessibilityValueId2;
                        response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            Title _Title = JsonConvert.DeserializeObject<Title>(result);
                            if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                                AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                        }
                    }
                    if (ConfiguredColumns.Count > 0)
                    {
                        if (AccessRule.AccessibilityParameter1 == "UserID")
                        {
                            AccessRule.AccessibilityParameter1 = "UserID";
                        }
                        else if (AccessRule.AccessibilityParameter1 == "MobileNumber")
                        {
                            AccessRule.AccessibilityParameter1 = "MobileNumber";
                        }
                        else if (AccessRule.AccessibilityParameter1 == "EmailID")
                        {
                            AccessRule.AccessibilityParameter1 = "EmailID";
                        }
                        else if (AccessRule.AccessibilityParameter1 == "DateOfJoining")
                        {
                            AccessRule.AccessibilityParameter1 = "Date Of Joining";
                        }
                        else
                        {
                            AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                        }
                        if (AccessRule.AccessibilityParameter2 == "UserID")
                        {
                            AccessRule.AccessibilityParameter2 = "UserID";
                        }
                        else if (AccessRule.AccessibilityParameter2 == "MobileNumber")
                        {
                            AccessRule.AccessibilityParameter2 = "MobileNumber";
                        }
                        else if (AccessRule.AccessibilityParameter2 == "EmailID")
                        {
                            AccessRule.AccessibilityParameter2 = "EmailID";
                        }
                        else if (AccessRule.AccessibilityParameter2 == "DateOfJoining")
                        {
                            AccessRule.AccessibilityParameter2 = "Date Of Joining";
                        }
                        else
                        {
                            AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                        }
                    }
                }

                if (ResultForGroupApplicability != null)
                {
                    foreach (var item in ResultForGroupApplicability)
                    {
                        int CourseId = 0;

                        CourseId = Int32.Parse(item.ScheduleId.ToString());

                        APIScheduleVisibilityRulesWithUserdata accessRule = new APIScheduleVisibilityRulesWithUserdata
                        {
                            Id = item.Id,
                            AccessibilityParameter1 = "User Team Name",
                            AccessibilityValue1 = item.TeamName,
                            AccessibilityValueId1 = Int32.Parse(item.UserTeamId.ToString()),
                            ScheduleId = CourseId,

                            UserName = item.UserName,
                            UserCreated = item.UserCreated,
                            AreaId = item.AreaId,
                            LocationId = item.LocationId,
                            GroupId = item.GroupId,
                            BusinessId = item.BusinessId,
                            CreatedBy = item.CreatedBy,
                        };
                        AccessibilityRules.Add(accessRule);
                    }
                }
                APIScheduleVisibilityRulesTotal aPIScheduleVisibilityRulesTotal = new APIScheduleVisibilityRulesTotal();
                aPIScheduleVisibilityRulesTotal.Data = AccessibilityRules;
                aPIScheduleVisibilityRulesTotal.TotalRecords = AccessibilityRules.Count();
                return aPIScheduleVisibilityRulesTotal;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

    
        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int scheduleId, string AccessibilityValue11, string AccessibilityValue22)
        {
            bool isvalid = true;

            if (_db.ILTSchedule.Where(y => y.ID == scheduleId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            try
            {
                using (var dbContext = _customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "CheckValidDataForUserSetting";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter1", SqlDbType.VarChar) { Value = AccessibilityParameter1 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityValue1", SqlDbType.VarChar) { Value = AccessibilityValue1 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter2", SqlDbType.VarChar) { Value = AccessibilityParameter2 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityValue2", SqlDbType.VarChar) { Value = AccessibilityValue2 });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                isvalid = Boolean.Parse(dt.Rows[0]["IsValid"].ToString());
                            }
                            reader.Dispose();
                            connection.Close();
                        }
                    }
                }

                if (string.Equals(AccessibilityParameter1,"dateofjoining",StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(AccessibilityValue1) && !string.IsNullOrEmpty(AccessibilityValue11))
                    {
                        if (DateTime.Compare(Convert.ToDateTime(AccessibilityValue1), Convert.ToDateTime(AccessibilityValue11)) > 0)
                        {
                            isvalid = false;
                            return isvalid;
                        }
                    }
                    else
                    {
                        isvalid = false;
                        return isvalid;
                    }
                }
                if (string.Equals(AccessibilityParameter2, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(AccessibilityValue2) && !string.IsNullOrEmpty(AccessibilityValue22))
                    {
                        if (DateTime.Compare(Convert.ToDateTime(AccessibilityValue2), Convert.ToDateTime(AccessibilityValue22)) > 0)
                        {
                            isvalid = false;
                            return isvalid;
                        }
                    }
                    else
                    {
                        isvalid = false;
                        return isvalid;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return isvalid;
        }


    
        public async Task<bool> RuleExist(ScheduleVisibilityRule accessibilityRule)
        {
            IQueryable<ScheduleVisibilityRule> Query = this._db.ScheduleVisibilityRule.Where(a => a.ScheduleId == accessibilityRule.ScheduleId && a.IsDeleted == Record.NotDeleted);

            if (accessibilityRule.Area != null)
                Query = Query.Where(a => a.Area == accessibilityRule.Area);
            if (accessibilityRule.Business != null)
                Query = Query.Where(a => a.Business == accessibilityRule.Business);
            if (accessibilityRule.ConfigurationColumn1 != null)
                Query = Query.Where(a => a.ConfigurationColumn1 == accessibilityRule.ConfigurationColumn1);
            if (accessibilityRule.ConfigurationColumn2 != null)
                Query = Query.Where(a => a.ConfigurationColumn2 == accessibilityRule.ConfigurationColumn2);
            if (accessibilityRule.ConfigurationColumn3 != null)
                Query = Query.Where(a => a.ConfigurationColumn3 == accessibilityRule.ConfigurationColumn3);
            if (accessibilityRule.ConfigurationColumn4 != null)
                Query = Query.Where(a => a.ConfigurationColumn4 == accessibilityRule.ConfigurationColumn4);
            if (accessibilityRule.ConfigurationColumn5 != null)
                Query = Query.Where(a => a.ConfigurationColumn5 == accessibilityRule.ConfigurationColumn5);
            if (accessibilityRule.ConfigurationColumn6 != null)
                Query = Query.Where(a => a.ConfigurationColumn6 == accessibilityRule.ConfigurationColumn6);
            if (accessibilityRule.ConfigurationColumn7 != null)
                Query = Query.Where(a => a.ConfigurationColumn7 == accessibilityRule.ConfigurationColumn7);
            if (accessibilityRule.ConfigurationColumn8 != null)
                Query = Query.Where(a => a.ConfigurationColumn8 == accessibilityRule.ConfigurationColumn8);
            if (accessibilityRule.ConfigurationColumn9 != null)
                Query = Query.Where(a => a.ConfigurationColumn9 == accessibilityRule.ConfigurationColumn9);
            if (accessibilityRule.ConfigurationColumn10 != null)
                Query = Query.Where(a => a.ConfigurationColumn10 == accessibilityRule.ConfigurationColumn10);
            if (accessibilityRule.ConfigurationColumn11 != null)
                Query = Query.Where(a => a.ConfigurationColumn11 == accessibilityRule.ConfigurationColumn11);
            if (accessibilityRule.ConfigurationColumn12 != null)
                Query = Query.Where(a => a.ConfigurationColumn12 == accessibilityRule.ConfigurationColumn12);
            if (accessibilityRule.ConfigurationColumn13 != null)
                Query = Query.Where(a => a.ConfigurationColumn13 == accessibilityRule.ConfigurationColumn13);
            if (accessibilityRule.ConfigurationColumn14 != null)
                Query = Query.Where(a => a.ConfigurationColumn14 == accessibilityRule.ConfigurationColumn14);
            if (accessibilityRule.ConfigurationColumn15 != null)
                Query = Query.Where(a => a.ConfigurationColumn15 == accessibilityRule.ConfigurationColumn15);
            if (accessibilityRule.MobileNumber != null)
                Query = Query.Where(a => a.MobileNumber == accessibilityRule.MobileNumber);
            if (accessibilityRule.EmailID != null)
                Query = Query.Where(a => a.EmailID == accessibilityRule.EmailID);
            if (accessibilityRule.Location != null)
                Query = Query.Where(a => a.Location == accessibilityRule.Location);
            if (accessibilityRule.Group != null)
                Query = Query.Where(a => a.Group == accessibilityRule.Group);
            if (accessibilityRule.UserID != null)
                Query = Query.Where(a => a.UserID == accessibilityRule.UserID);
            if (accessibilityRule.StartDateOfJoining != null && accessibilityRule.EndDateOfJoining != null)
                Query = Query.Where(a => a.StartDateOfJoining <= accessibilityRule.StartDateOfJoining && a.EndDateOfJoining >= accessibilityRule.EndDateOfJoining);
            if (accessibilityRule.UserTeamId != null)
                Query = Query.Where(a => a.UserTeamId == accessibilityRule.UserTeamId);
            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }

       
        public async Task<List<CourseApplicableUser>> GetCourseApplicableUserList(int courseId)
        {
            List<CourseApplicableUser> listUserApplicability = new List<CourseApplicableUser>();

            var connection = this._db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetScheduleVisibilityUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.BigInt) { Value = courseId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            CourseApplicableUser rule = new CourseApplicableUser();
                            rule.UserID = Security.Decrypt(row["UserID"].ToString());
                            rule.UserName = row["UserName"].ToString();
                            listUserApplicability.Add(rule);
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return listUserApplicability;
        }

    
        public FileInfo GetApplicableUserListExcel(List<APIScheduleVisibilityRules> aPIAccessibilityRules, List<CourseApplicableUser> courseApplicableUsers, string CourseName, string ModuleNAme, string ScheduleCode, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"ScheduleRegistraionLimit.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Schedule Registraion Limit");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "Course Name";
                row++;
                worksheet.Cells[row, column].Value = CourseName;
                row++;
                worksheet.Cells[row, column].Value = "Module Name";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                row++;
                worksheet.Cells[row, column].Value = ModuleNAme;
                row++;
                worksheet.Cells[row, column].Value = "Schedule Code";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                row++;
                worksheet.Cells[row, column].Value = ScheduleCode;
                row++;
                column = 1;
                row++;
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
             
                worksheet.Cells[row, column++].Value = "Additional Criteria";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
              

                foreach (APIScheduleVisibilityRules course in aPIAccessibilityRules)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                   
                    worksheet.Cells[row, column++].Value = course.Condition1 == null ? "-" : course.Condition1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter2 == null ? "-" : course.AccessibilityParameter2;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue2 == null ? "-" : course.AccessibilityValue2;
                    
                }
                row++;
                row++;

                worksheet.Cells[row, 1].Value = "Applicable Users";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                row++;
                worksheet.Cells[row, 1].Value = "UserId";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = "UserName";
                worksheet.Cells[row, 2].Style.Font.Bold = true;

                foreach (CourseApplicableUser courseApplicableUser in courseApplicableUsers)
                {
                    row++; column = 1;
                    worksheet.Cells[row, column++].Value = courseApplicableUser.UserID == null ? "-" : courseApplicableUser.UserID;
                    worksheet.Cells[row, column++].Value = courseApplicableUser.UserName == null ? "-" : courseApplicableUser.UserName;

                }

                using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;

                }

                package.Save(); //Save the workbook.

            }
            return file;

        }

  
        public async Task<List<APIScheduleVisibilityRules>> GetAccessibilityRulesForExport(int scheduleId, string orgnizationCode, string token, string CourseName)
        {
            var Result = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                                join schedule in _db.ILTSchedule on accessibiltyRule.ScheduleId equals schedule.ID
                               
                                where accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.UserTeamId == null || accessibiltyRule.UserTeamId == 0)
                                select new
                                {
                                    accessibiltyRule.ConfigurationColumn1,
                                    accessibiltyRule.ConfigurationColumn2,
                                    accessibiltyRule.ConfigurationColumn3,
                                    accessibiltyRule.ConfigurationColumn4,
                                    accessibiltyRule.ConfigurationColumn5,
                                    accessibiltyRule.ConfigurationColumn6,
                                    accessibiltyRule.ConfigurationColumn7,
                                    accessibiltyRule.ConfigurationColumn8,
                                    accessibiltyRule.ConfigurationColumn9,
                                    accessibiltyRule.ConfigurationColumn10,
                                    accessibiltyRule.ConfigurationColumn11,
                                    accessibiltyRule.ConfigurationColumn12,
                                    accessibiltyRule.Area,
                                    accessibiltyRule.Business,
                                    accessibiltyRule.EmailID,
                                    accessibiltyRule.MobileNumber,
                                    accessibiltyRule.Location,
                                    accessibiltyRule.Group,
                                    accessibiltyRule.UserID,
                                    accessibiltyRule.ConditionForRules,
                                    accessibiltyRule.ScheduleId,
                                    accessibiltyRule.Id,
                                    schedule.ScheduleCode,
                                    accessibiltyRule.StartDateOfJoining,
                                    accessibiltyRule.EndDateOfJoining
                                }).ToListAsync();

            var ResultForGroupApplicability = await (from accessibiltyRule in _db.ScheduleVisibilityRule
                                                     join course in _db.ILTSchedule on accessibiltyRule.ScheduleId equals course.ID

                                                     join applicabilityGroupTemplate in _db.UserTeamsMapping on accessibiltyRule.UserTeamId equals applicabilityGroupTemplate.UserTeamId
                                                     into d
                                                     from applicabilityGroupTemplate in d.DefaultIfEmpty()

                                                     join userteam in _db.UserTeams on applicabilityGroupTemplate.UserTeamId equals userteam.Id
                                                   into e
                                                     from ut in e.DefaultIfEmpty()
                                                     where (accessibiltyRule.UserTeamId != null && accessibiltyRule.UserTeamId != 0) && accessibiltyRule.ScheduleId == scheduleId && accessibiltyRule.IsDeleted == false
                                                     select new
                                                     {
                                                         accessibiltyRule.ScheduleId,
                                                         accessibiltyRule.Id,
                                                         course.ScheduleCode,
                                                         accessibiltyRule.UserTeamId,
                                                         ut.TeamName
                                                     }).ToListAsync();
            List<APIScheduleVisibilityRules> AccessibilityRules = new List<APIScheduleVisibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int ScheduleId = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("scheduleid"))
                        ScheduleId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("ScheduleCode") &&
                        !rule.Name.Equals("ScheduleId") &&
                        !rule.Name.Equals("Id"))
                    {
                        if (string.Equals(rule.Name, "startdateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var applicationDateFormat = await GetMasterConfigurableParameterValue("APPLICATION_DATE_FORMAT");
                            Rules RuleDoj = new Rules
                            {
                                AccessibilityParameter = "DateOfJoining",
                                AccessibilityValue = Convert.ToDateTime(rule.GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                AccessibilityValue2 = Convert.ToDateTime(properties[++i].GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                Condition = Condition
                            };
                            Rules.Add(RuleDoj);
                        }
                        else if (!string.Equals(rule.Name, "enddateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Rules Rule = new Rules
                            {
                                AccessibilityParameter = rule.Name,
                                AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                                Condition = Condition
                            };
                            Rules.Add(Rule);
                        }
                    }
                    i++;
                }
                if (Rules.Count == 2)
                {
                    APIScheduleVisibilityRules ApiRule = new APIScheduleVisibilityRules
                    {
                        ScheduleId = scheduleId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                        AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = !string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(1).AccessibilityValue) : 0,
                        AccessibilityValue2 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue : null,
                        AccessibilityValue22 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue2 : null,
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APIScheduleVisibilityRules ApiRule = new APIScheduleVisibilityRules
                    {
                        ScheduleId = scheduleId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                        AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                    };
                    AccessibilityRules.Add(ApiRule);
                }
            }
            string UserUrls = _configuration[APIHelper.UserAPI];
            string settings = "setting/1/20/";
            UserUrls += settings;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
            }
            foreach (APIScheduleVisibilityRules AccessRule in AccessibilityRules)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                response = await APIHelper.CallGetAPI(Apiurl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }
                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        if (!string.Equals(AccessRule.AccessibilityParameter1, "dateofjoining", StringComparison.CurrentCultureIgnoreCase))
                            AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                if (ConfiguredColumns.Count > 0)
                {
                    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count()>0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter1;
                    AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count()>0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter2;
                }
            }

            if (ResultForGroupApplicability != null)
            {
                foreach (var item in ResultForGroupApplicability)
                {
                    int CourseId = 0;

                    CourseId = Int32.Parse(item.ScheduleId.ToString());

                    APIScheduleVisibilityRules accessRule = new APIScheduleVisibilityRules
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "User Team Name",
                        AccessibilityValue1 = item.TeamName,
                        AccessibilityValueId1 = Int32.Parse(item.UserTeamId.ToString()),
                        ScheduleId = CourseId
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }
            return AccessibilityRules;
        }

        public async Task<APIGetScheduleDetails> GetCourseModuleScheduleNames(int scheduleid)
        {
            APIGetScheduleDetails obj = new APIGetScheduleDetails();
            try { 
            var SurveyName = await (from c in _db.Course
                                    join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
                                    join m in _db.Module on ma.ModuleId equals m.Id
                                    join s in _db.ILTSchedule on new { id = m.Id, cid = ma.CourseId } equals new { id = s.ModuleId, cid = s.CourseId }
                                    where c.IsDeleted == false && s.ID == scheduleid
                                    select new APIGetScheduleDetails
                                    {
                                        CourseName = c.Title,
                                        ModuleName = m.Name,
                                       ScheduleCode=s.ScheduleCode
                                    }).SingleOrDefaultAsync();
            return SurveyName;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return obj;
        }

       
    }

}
