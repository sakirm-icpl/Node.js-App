using ILT.API.APIModel;
using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ILT.API.Repositories.Interfaces;
using System.Net.Http;
using ILT.API.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;

namespace ILT.API.Repositories
{
    public class OnlineWebinarRepository : IOnlineWebinarRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OnlineWebinarRepository));
        private CourseContext _db;
        private IConfiguration _configuration;
        private ICustomerConnectionStringRepository _customerConnection;

        
        public OnlineWebinarRepository(CourseContext context, IConfiguration Configuration, ICustomerConnectionStringRepository customerConnection)
        {
            _db = context;
            _configuration = Configuration;
            _customerConnection = customerConnection;
        }
        private IConfidentialClientApplication GetTeamsConfidentialClientApplication()
        {
            IConfidentialClientApplication app;
            try
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "TEAMS").FirstOrDefault();
                
                string authority = iltonlineSettingteams.TeamsAuthority;
                string ClientId = iltonlineSettingteams.ClientID;
                string ClientSecret = iltonlineSettingteams.ClientSecret;
                
                app = ConfidentialClientApplicationBuilder.Create(ClientId)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(new Uri(authority))
                        .Build();
                return app;
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<AuthenticationResult> GetTeamsToken()
        {
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
            IConfidentialClientApplication app = GetTeamsConfidentialClientApplication();
            AuthenticationResult results = null;
            try
            {
                results = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            
            return results;
        }

        public async Task<List<TeamsScheduleDetails>> PostTeamsMeeting(Teams teams, int userId)
        {
            List<TeamsScheduleDetails> teamsScheduleDetails = new List<TeamsScheduleDetails>();

            AuthenticationResult results = await GetTeamsToken();
            try
            {

                var objectID = results.UniqueId;
                
                teamsScheduleDetails = await CallTeamsEventCalendars(results.AccessToken, teams, userId, objectID, null);
              
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return teamsScheduleDetails;
        }

        public async Task<List<TeamsScheduleDetails>> CallTeamsEventCalendars(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId)
        {
            List<TeamsScheduleDetails> teamsScheduleDetailss = new List<TeamsScheduleDetails>();
            TeamsScheduleDetails teamsScheduleDetails = null;
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if(webinarMaster == null)
            {
                ConfigurableValues configurableValues = this._db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();
                UserWebinarMaster userWebinarMaster1 = new UserWebinarMaster();
                userWebinarMaster1.isDefault = 1;
                userWebinarMaster1.isDeleted = 0;
                userWebinarMaster1.TeamsEmail = teams.Username;
                userWebinarMaster1.WebinarID = configurableValues.ID;
                userWebinarMaster1.Username = null;
                userWebinarMaster1.CreatedBy = userId;
                userWebinarMaster1.CreatedDate = DateTime.Now;
                userWebinarMaster1.ModifiedDate = DateTime.Now;

                this._db.UserWebinarMasters.Add(userWebinarMaster1);
                this._db.SaveChanges();

                webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            }
            if (webinarMaster != null)
            {

                EventMeeting eventMeeting = new EventMeeting();
                Start start = new Start();
                End end = new End();

                try
                {
                    eventMeeting.start = new Start();
                    eventMeeting.end = new End();

                    DateTime startDate = new DateTime(teams.StartDate.Ticks);
                    DateTime endDate = new DateTime(teams.EndDate.Ticks);
                    if(teams.ScheduleID == 0)
                    {
                        for (int i = 0; i < teams.TrinerData.Count(); i++)
                        {
                            string email = Security.DecryptForUI(teams.TrinerData[i].EmailId);
                            email = Security.Encrypt(email);
                            UserMaster userMaster = this._db.UserMaster.Where(x => x.EmailId == email).FirstOrDefault();

                            if (userMaster != null)
                            {
                                var Query = (from ILTSchedule in this._db.ILTSchedule
                                             join ILTScheduleTrainerBindings in this._db.ILTScheduleTrainerBindings on ILTSchedule.ID equals ILTScheduleTrainerBindings.ScheduleID

                                             where
                                             (
                                             (ILTSchedule.StartTime >= TimeSpan.Parse(teams.StartTime) && ILTSchedule.StartTime <= TimeSpan.Parse(teams.EndTime))
                                             ||
                                             (ILTSchedule.EndTime >= TimeSpan.Parse(teams.StartTime) && ILTSchedule.EndTime <= TimeSpan.Parse(teams.EndTime))
                                             )
                                             &&
                                             (
                                             (ILTSchedule.StartDate >= startDate && ILTSchedule.StartDate <= endDate)
                                             ||
                                             (ILTSchedule.EndDate >= startDate && ILTSchedule.EndDate <= endDate)
                                             )
                                             &&
                                             ILTSchedule.IsActive == true && ILTSchedule.IsDeleted == false && teams.StartTime != teams.EndTime && ILTScheduleTrainerBindings.TrainerID == userMaster.Id
                                             select new ILTScheduleTrainerBindings
                                             {
                                                 TrainerName = ILTScheduleTrainerBindings.TrainerName,
                                                 ScheduleID = ILTScheduleTrainerBindings.ScheduleID
                                             });

                                if (Query.Count() > 0)
                                {
                                    return teamsScheduleDetailss;
                                }
                            }
                        }
                    }
                   
                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        teamsScheduleDetails = new TeamsScheduleDetails();
                        HolidayList holidayList = null;
                        if(teams.HolidayList != null)
                        {
                            holidayList = teams.HolidayList.Where(a => a.Date == date && a.IsHoliday == true).FirstOrDefault();
                        }
                        
                        if(holidayList == null)
                        {
                            string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", date);
                            string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", date);
                            var sdate = startdate1.Substring(0, 11);
                            var sdate1 = teams.StartTime;
                            var sdate2 = startdate1.Substring(17, 2);

                            var edate = enddate1.Substring(0, 11);
                            var edate1 = teams.EndTime;
                            var edate2 = enddate1.Substring(17, 2);

                            eventMeeting.subject = teams.CourseName;
                            eventMeeting.start.dateTime = sdate + sdate1 + ":" + sdate2;
                            eventMeeting.start.timeZone = "Asia/Kolkata";
                            eventMeeting.end.dateTime = edate + edate1 + ":" + edate2;
                            eventMeeting.end.timeZone = "Asia/Kolkata";
                            eventMeeting.IsOnlineMeeting = true;
                            eventMeeting.OnlineMeetingProvider = "teamsForBusiness";

                            eventMeeting.attendees = new Attendance[teams.TrinerData.Count()];
                            for (int i = 0; i < teams.TrinerData.Count(); i++)
                            {
                                eventMeeting.attendees[i] = new Attendance();
                                eventMeeting.attendees[i].emailAddress = new EmailAddress();
                                eventMeeting.attendees[i].emailAddress.address = Security.DecryptForUI(teams.TrinerData[i].EmailId);
                                eventMeeting.attendees[i].emailAddress.name = teams.TrinerData[i].EmailName;
                                eventMeeting.attendees[i].status = new Status1();
                                eventMeeting.attendees[i].type = "required";
                            }

                            JObject oJsonObject1 = new JObject();
                            oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));

                            var baseUrl = "https://graph.microsoft.com/v1.0/users/" + webinarMaster.TeamsEmail + "/calendar/events";
                            TeamsEventResponse res = await ApiHelper.CreateTeamsEvent(oJsonObject1, TeamsAccessToken, baseUrl);

                            if (res != null)
                            {

                                teamsScheduleDetails.CourseID = teams.CourseID;
                                teamsScheduleDetails.ScheduleID = teams.ScheduleID;
                                teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                                teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                                teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                                teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                                teamsScheduleDetails.IsActive = Record.Active;
                                teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                                teamsScheduleDetails.iCalUId = res.iCalUId;
                                teamsScheduleDetails.MeetingId = res.id;
                                teamsScheduleDetails.StartTime = eventMeeting.start.dateTime;
                                teamsScheduleDetails.EndTime = eventMeeting.end.dateTime;
                                if (res.onlineMeeting == null)
                                {
                                    teamsScheduleDetails.JoinUrl = res.webLink;
                                }
                                else
                                {
                                    teamsScheduleDetails.JoinUrl = res.onlineMeeting.joinUrl;
                                }

                                teamsScheduleDetails.UserWebinarId = webinarMaster.Id;


                                if (MeetingId == null)
                                {
                                    await this._db.TeamsScheduleDetails.AddAsync(teamsScheduleDetails);
                                    await this._db.SaveChangesAsync();
                                    teamsScheduleDetailss.Add(teamsScheduleDetails);
                                }

                            }
                        }  
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetailss;
        }
        public async Task<TeamsScheduleDetails> UpdateTeamsMeeting(int userId, Teams teams, UserWebinarMaster userWebinarMaster,int id, AuthenticationResult authenticationResult)
        {
            HolidayList holidayList = null;
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            if (teams.HolidayList != null)
            {
                holidayList = teams.HolidayList.Where(a => a.Date == teams.StartDate && a.IsHoliday == true).FirstOrDefault();
            }
            if(holidayList == null)
            {
                //AuthenticationResult results = await GetTeamsToken();
              
                ILTSchedule iLTSchedule = _db.ILTSchedule.Where(a => a.ID == teams.ScheduleID).FirstOrDefault();
                if (iLTSchedule != null)
                {
                    Model.Course course = _db.Course.Where(a => a.Id == iLTSchedule.CourseId).FirstOrDefault();
                    APIILTSchedular aPIILTSchedular1 = await GetByID(iLTSchedule.ID);
                    List<APIUserData> mailSendUser = new List<APIUserData>();

                    for (int i = 0; i < aPIILTSchedular1.TrainerList.Length; i++)
                    {
                        UserMaster userMaster = _db.UserMaster.Where(a => a.Id == aPIILTSchedular1.TrainerList[i].AcademyTrainerID).FirstOrDefault();
                        APIUserData aPIUserData = new APIUserData();
                        aPIUserData.emailId = Security.Decrypt(userMaster.EmailId);
                        aPIUserData.userName = userMaster.UserName;
                        mailSendUser.Add(aPIUserData);
                    }
                    List<APITrainingNomination> aPITrainingNomination = await GetNominateUserDetails(iLTSchedule.ID, course.Id, 1, 10000, "userName", null);


                    if (aPITrainingNomination != null)
                    {
                        for (int i = 0; i < aPITrainingNomination.Count; i++)
                        {
                            APIUserData aPIUserData = new APIUserData();
                            aPIUserData.emailId = aPITrainingNomination[i].EmailId;
                            aPIUserData.userName = aPITrainingNomination[i].UserName;
                            mailSendUser.Add(aPIUserData);
                        }
                    }

                    if (userWebinarMaster != null)
                    {

                        EventMeeting eventMeeting = new EventMeeting();
                        Start start = new Start();
                        End end = new End();

                        try
                        {
                            eventMeeting.start = new Start();
                            eventMeeting.end = new End();

                            string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.StartDate.Date);
                            string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.EndDate.Date);

                            var sdate = startdate1.Substring(0, 11);
                            var sdate1 = teams.StartTime;
                            var sdate2 = startdate1.Substring(17, 2);

                            var edate = enddate1.Substring(0, 11);
                            var edate1 = teams.EndTime;
                            var edate2 = enddate1.Substring(17, 2);

                            eventMeeting.subject = course.Title;
                            eventMeeting.start.dateTime = sdate + sdate1 + ":" + sdate2;
                            eventMeeting.start.timeZone = "Asia/Kolkata";
                            eventMeeting.end.dateTime = edate + edate1 + ":" + edate2;
                            eventMeeting.end.timeZone = "Asia/Kolkata";
                            eventMeeting.IsOnlineMeeting = true;
                            eventMeeting.OnlineMeetingProvider = "teamsForBusiness";
                            eventMeeting.attendees = new Attendance[mailSendUser.Count];
                            for (int i = 0; i < mailSendUser.Count; i++)
                            {
                                eventMeeting.attendees[i] = new Attendance();
                                eventMeeting.attendees[i].emailAddress = new EmailAddress();
                                eventMeeting.attendees[i].emailAddress.address = mailSendUser[i].emailId;
                                eventMeeting.attendees[i].emailAddress.name = mailSendUser[i].userName;
                                eventMeeting.attendees[i].status = new Status1();
                                eventMeeting.attendees[i].type = "required";
                            }

                            JObject oJsonObject1 = new JObject();
                            oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));
                            TeamsScheduleDetails teamsScheduleDetails1 = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID && a.ID == id).FirstOrDefault();
                            if(teamsScheduleDetails1 != null)
                            {
                                string baseUrl = "https://graph.microsoft.com/v1.0/users/" + userWebinarMaster.TeamsEmail + "/calendar/events/" + teamsScheduleDetails1.MeetingId;
                                HttpResponseMessage Response = await ApiHelper.CallPatchAPIForTeams(baseUrl, oJsonObject1, authenticationResult.AccessToken);
                                TeamsEventResponse TeamsResponce = null;
                                if (Response.IsSuccessStatusCode)
                                {
                                    var result = Response.Content.ReadAsStringAsync().Result;

                                    TeamsResponce = JsonConvert.DeserializeObject<TeamsEventResponse>(result);
                                }
                                if (TeamsResponce != null)
                                {

                                    teamsScheduleDetails.CourseID = iLTSchedule.CourseId;
                                    teamsScheduleDetails.ScheduleID = Convert.ToInt32(teamsScheduleDetails1.ScheduleID);
                                    teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                                    teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                                    teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                                    teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                                    teamsScheduleDetails.IsActive = Record.Active;
                                    teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                                    teamsScheduleDetails.iCalUId = TeamsResponce.iCalUId;
                                    teamsScheduleDetails.MeetingId = TeamsResponce.id;
                                    teamsScheduleDetails.StartTime = TeamsResponce.start.dateTime.ToString();
                                    teamsScheduleDetails.EndTime = TeamsResponce.start.dateTime.ToString();
                                    if (TeamsResponce.onlineMeeting == null)
                                    {
                                        teamsScheduleDetails.JoinUrl = TeamsResponce.webLink;
                                    }
                                    else
                                    {
                                        teamsScheduleDetails.JoinUrl = TeamsResponce.onlineMeeting.joinUrl;
                                    }

                                    teamsScheduleDetails.UserWebinarId = userWebinarMaster.Id;
                                }
                            }
                            else
                            {
                                var baseUrl = "https://graph.microsoft.com/v1.0/users/" + userWebinarMaster.TeamsEmail + "/calendar/events";
                                TeamsEventResponse res = await ApiHelper.CreateTeamsEvent(oJsonObject1, authenticationResult.AccessToken, baseUrl);

                                if (res != null)
                                {

                                    teamsScheduleDetails.CourseID = teams.CourseID;
                                    teamsScheduleDetails.ScheduleID = teams.ScheduleID;
                                    teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                                    teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                                    teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                                    teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                                    teamsScheduleDetails.IsActive = Record.Active;
                                    teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                                    teamsScheduleDetails.iCalUId = res.iCalUId;
                                    teamsScheduleDetails.MeetingId = res.id;
                                    teamsScheduleDetails.StartTime = eventMeeting.start.dateTime;
                                    teamsScheduleDetails.EndTime = eventMeeting.end.dateTime;
                                    if (res.onlineMeeting == null)
                                    {
                                        teamsScheduleDetails.JoinUrl = res.webLink;
                                    }
                                    else
                                    {
                                        teamsScheduleDetails.JoinUrl = res.onlineMeeting.joinUrl;
                                    }

                                    teamsScheduleDetails.UserWebinarId = userWebinarMaster.Id;
                                    await this._db.TeamsScheduleDetails.AddAsync(teamsScheduleDetails);
                                    await this._db.SaveChangesAsync();
                                    
                                }
                            }
                       
                        }

                        catch (Exception ex)

                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }

                }
            }
            else
            {
                string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.StartDate.Date);
                var sdate = startdate1.Substring(0, 11);
                var sdate1 = teams.StartTime;

                teamsScheduleDetails = this._db.TeamsScheduleDetails.Where(
                    a => a.ScheduleID == teams.ScheduleID && a.StartTime == sdate1
                    ).FirstOrDefault();
                if(teamsScheduleDetails != null)
                {
                    await cancleTeamsMeeting(userId, teams.ScheduleID, userWebinarMaster, teamsScheduleDetails);
                }
            }
           
            return teamsScheduleDetails;
        }
        public async Task<TeamsScheduleDetails> cancleTeamsMeeting(int userId,int scheduleID, UserWebinarMaster userWebinarMaster, TeamsScheduleDetails teamsScheduleDetails1)
        {
            AuthenticationResult results = await GetTeamsToken();
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            ILTSchedule iLTSchedule = _db.ILTSchedule.Where(a => a.ID == scheduleID).FirstOrDefault();
            if (iLTSchedule != null)
            {
                Model.Course course = _db.Course.Where(a => a.Id == iLTSchedule.CourseId).FirstOrDefault();
                APIILTSchedular aPIILTSchedular1 = await GetByID(iLTSchedule.ID);
                
                if (userWebinarMaster != null)
                {

                    try
                    {
                        string baseUrl = "https://graph.microsoft.com/v1.0/users/" + userWebinarMaster.TeamsEmail + "/calendar/events/" + teamsScheduleDetails1.MeetingId;
                        HttpResponseMessage Response = await ApiHelper.CallDeleteAPI(baseUrl,  results.AccessToken);
                        
                        if (Response.IsSuccessStatusCode)
                        {                           

                            teamsScheduleDetails.CourseID = iLTSchedule.CourseId;
                            teamsScheduleDetails.ScheduleID = Convert.ToInt32(teamsScheduleDetails1.ScheduleID);
                            teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                            teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                            teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                            teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                            teamsScheduleDetails.IsActive = Record.NotActive;
                            teamsScheduleDetails.IsDeleted = Record.Deleted;                            
                          
                            teamsScheduleDetails.UserWebinarId = userWebinarMaster.Id;
                        }
                    }

                    catch (Exception ex)

                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                    }
                }

            }
            return teamsScheduleDetails;
        }

        public async Task<APIILTSchedular> GetByID(int Id)

        {
            var Query = (from ILTSchedule in this._db.ILTSchedule
                         join TrainingPlace in this._db.TrainingPlace on ILTSchedule.PlaceID equals TrainingPlace.Id into tempPlace
                         from TrainingPlace in tempPlace.DefaultIfEmpty()
                         join AcademyAgency in this._db.AcademyAgencyMaster on ILTSchedule.AcademyAgencyID equals AcademyAgency.Id into tempAcademyAgency
                         from AcademyAgency in tempAcademyAgency.DefaultIfEmpty()
                         join ILTScheduleTrainerBindings in this._db.ILTScheduleTrainerBindings on ILTSchedule.ID equals ILTScheduleTrainerBindings.ScheduleID into tempILTScheduleTrainerBindings
                         from ILTScheduleTrainerBindings in tempILTScheduleTrainerBindings.DefaultIfEmpty()
                         join Module in this._db.Module on ILTSchedule.ModuleId equals Module.Id into tempModule
                         from Module in tempModule.DefaultIfEmpty()
                         join Courses in this._db.Course on ILTSchedule.CourseId equals Courses.Id into CourseGroup
                         from Course in CourseGroup.DefaultIfEmpty()
                         join Batch in this._db.ILTBatch on ILTSchedule.BatchId equals Batch.Id into BatchGroup
                         from Batch in BatchGroup.DefaultIfEmpty()
                         where ILTSchedule.ID == Id
                         select new APIILTSchedular
                         {
                             ID = ILTSchedule.ID,
                             ScheduleCode = ILTSchedule.ScheduleCode,
                             BatchId = ILTSchedule.BatchId,
                             BatchCode = Batch.BatchCode,
                             BatchName = Batch.BatchName,
                             ModuleId = ILTSchedule.ModuleId,
                             ModuleName = Module.Name,
                             StartDate = ILTSchedule.StartDate,
                             EndDate = ILTSchedule.EndDate,
                             StartTime = ILTSchedule.StartTime.ToString(@"hh\:mm"),
                             EndTime = ILTSchedule.EndTime.ToString(@"hh\:mm"),
                             RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                             IsActive = ILTSchedule.IsActive,
                             IsDeleted = ILTSchedule.IsDeleted,
                             TrainerType = ILTSchedule.TrainerType,
                             PlaceID = ILTSchedule.PlaceID,
                             PlaceName = TrainingPlace.PlaceName,
                             AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                             AcademyAgencyName = AcademyAgency.AcademyAgencyName,
                             TrainerDescription = ILTSchedule.TrainerDescription,
                             ScheduleType = ILTSchedule.ScheduleType,
                             ReasonForCancellation = ILTSchedule.ReasonForCancellation,
                             Status = ILTSchedule.IsActive,
                             EventLogo = ILTSchedule.EventLogo,
                             City = TrainingPlace.Cityname,
                             ContactNumber = TrainingPlace.ContactNumber,
                             ContactPersonName = TrainingPlace.ContactPerson,
                             PlaceType = TrainingPlace.PlaceType,
                             postalAddress = TrainingPlace.PostalAddress,
                             SeatCapacity = TrainingPlace.AccommodationCapacity,
                             Cost = ILTSchedule.Cost,
                             Currency = ILTSchedule.Currency,
                             CourseID = Course.Id,
                             CourseCode = Course.Code,
                             CourseName = Course.Title,
                             TrainerList = (from ILTScheduleTrainerBindings1 in this._db.ILTScheduleTrainerBindings
                                                //join usermater in _db.UserMaster on ILTScheduleTrainerBindings1.TrainerID equals usermater.Id
                                            where ILTScheduleTrainerBindings1.ScheduleID == ILTSchedule.ID
                                            select new TrainerList
                                            {
                                                AcademyTrainerID = ILTScheduleTrainerBindings1.TrainerID,
                                                AcademyTrainerName = ILTScheduleTrainerBindings1.TrainerName,
                                                TrainerType = ILTScheduleTrainerBindings1.TrainerType
                                            }).ToArray(),
                             HolidayList = (from ScheduleHolidayList in this._db.ScheduleHolidayDetails
                                            where ScheduleHolidayList.ReferenceID == ILTSchedule.ID
                                            select new HolidayList
                                            {
                                                IsHoliday = ScheduleHolidayList.IsHoliday,
                                                Reason = ScheduleHolidayList.Reason,
                                                Date = ScheduleHolidayList.Date
                                            }).ToArray(),
                             TopicList = (from ModuleTopicAssociation in this._db.ModuleTopicAssociation
                                          join TopicMaster in this._db.TopicMaster on ModuleTopicAssociation.TopicId equals TopicMaster.ID
                                          where ModuleTopicAssociation.ModuleId == ILTSchedule.ModuleId
                                          select new TopicList
                                          {
                                              TopicId = ModuleTopicAssociation.TopicId,
                                              TopicName = TopicMaster.TopicName
                                          }).ToArray(),
                             WebinarType = ILTSchedule.WebinarType,
                             Purpose = ILTSchedule.Purpose,
                             ScheduleCapacity = ILTSchedule.ScheduleCapacity
                         }).Distinct();

            APIILTSchedular aPIILTSchedular = Query.FirstOrDefault();
            if (aPIILTSchedular != null)
            {
                if (aPIILTSchedular.TrainerList != null)
                {
                    for (int i = 0; i < aPIILTSchedular.TrainerList.Length; i++)
                    {
                        UserMaster userMaster = _db.UserMaster.Where(a => a.Id == aPIILTSchedular.TrainerList[i].AcademyTrainerID).FirstOrDefault();
                        if (userMaster != null)
                        {
                            aPIILTSchedular.TrainerList[i].NameUserId = aPIILTSchedular.TrainerList[i].AcademyTrainerName + "-" + Security.Decrypt(userMaster.UserId) + " (" + aPIILTSchedular.TrainerList[i].TrainerType + ")";
                        }
                        if (aPIILTSchedular.TrainerList[i].TrainerType == "External")
                        {
                            aPIILTSchedular.TrainerList[i].NameUserId = aPIILTSchedular.TrainerList[i].AcademyTrainerName + " (" + aPIILTSchedular.TrainerList[i].TrainerType + ")";
                        }
                    }
                }
            }

            return aPIILTSchedular;
        }
        public async Task<List<APITrainingNomination>> GetNominateUserDetails(int id, int courseId, int page, int pageSize, string search = null, string searchText = null)
        {
            List<APITrainingNomination> TrainingNominationList = new List<APITrainingNomination>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllNominateUserDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APITrainingNomination TrainingNomination = new APITrainingNomination();
                                TrainingNomination.ID = string.IsNullOrEmpty(row["AutoGenerateUserID"].ToString()) ? 0 : int.Parse(row["AutoGenerateUserID"].ToString());
                                TrainingNomination.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                TrainingNomination.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : Security.Decrypt(row["UserId"].ToString());
                                TrainingNomination.UserName = row["UserName"].ToString();
                                TrainingNomination.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                TrainingNomination.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                TrainingNomination.Status = row["Status"].ToString();
                                TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());

                                TrainingNominationList.Add(TrainingNomination);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return TrainingNominationList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        #region GoogleMeet
        CalendarService GetCalendarService(string keyfilepath, string AdminAccountId)
        {
           
                string[] Scopes = {
           CalendarService.Scope.Calendar,
           CalendarService.Scope.CalendarEvents,
           CalendarService.Scope.CalendarEventsReadonly
         };

                GoogleCredential credential;
                using (var stream = new FileStream(keyfilepath, FileMode.Open, FileAccess.Read))
                {
                    // As we are using admin SDK, we need to still impersonate user who has admin access    
                    //  https://developers.google.com/admin-sdk/directory/v1/guides/delegation    
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes).CreateWithUser(AdminAccountId);
                }

                // Create Calendar API service.    
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Calendar Sample",
                });
                return service;
           
        }
        public async Task<GoogleMeetRessponce> CallGSuitEventCalendars(MeetDetails teams, int userId)
        {
            GoogleMeetRessponce resp = new GoogleMeetRessponce();
            GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "googlemeet").FirstOrDefault();
                if (iltonlineSettingteams != null)
                {
                    string AdminAccountId = iltonlineSettingteams.ClientID;
                    string ServiceJsonFilePath = iltonlineSettingteams.ClientSecret;
                    if (!string.IsNullOrEmpty(AdminAccountId) && !string.IsNullOrEmpty(ServiceJsonFilePath))
                    {
                        CalendarService _service = GetCalendarService(ServiceJsonFilePath, AdminAccountId);

                        string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.StartDate.Date);
                        string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.EndDate.Date);

                        var sdate = startdate1.Substring(0, 11);
                        var sdate1 = teams.StartTime;
                        var sdate2 = startdate1.Substring(17, 2);

                        var edate = enddate1.Substring(0, 11);
                        var edate1 = teams.EndTime;
                        var edate2 = enddate1.Substring(17, 2);

                        List<EventAttendee> attendes = new List<EventAttendee>();
                        if (teams.TrinerData != null)
                        {
                            if (teams.TrinerData.Length > 0)
                            {
                                foreach (TrainersData trainer in teams.TrinerData)
                                {

                                    EventAttendee a = new EventAttendee();
                                    a.Email = Security.DecryptForUI(trainer.EmailId);
                                    a.DisplayName = trainer.EmailName;
                                    attendes.Add(a);
                                }
                            }
                        }


                        Event newEvent = new Event()
                        {
                            Summary = teams.CourseName,
                            // Location = "800 Howard St., San Francisco, CA 94103",
                            Description = teams.CourseName,
                            Start = new EventDateTime()
                            {
                                DateTime = DateTime.Parse(sdate + sdate1 + ":" + sdate2),
                                TimeZone = "Asia/Kolkata",
                            },
                            End = new EventDateTime()
                            {
                                DateTime = DateTime.Parse(edate + edate1 + ":" + edate2),
                                TimeZone = "Asia/Kolkata",
                            },
                            ConferenceData = new ConferenceData()
                            {
                                CreateRequest = new CreateConferenceRequest()
                                {
                                    RequestId = new Guid().ToString(),
                                    ConferenceSolutionKey = new ConferenceSolutionKey()
                                    {
                                        Type = "hangoutsMeet",
                                    }
                                }
                            },
                            Attendees = attendes

                    };

                        String calendarId = webinarMaster.TeamsEmail;
                      
                        EventsResource.InsertRequest request = new EventsResource.InsertRequest(_service, newEvent, calendarId);
                        request.SendNotifications = true;
                        request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;
                        request.ConferenceDataVersion = 1;
                        Event res = request.Execute();

                        if (res.HangoutLink != null)
                        {

                            googleMeetDetails.CourseID = teams.CourseID;
                            googleMeetDetails.ScheduleID = 0;
                            googleMeetDetails.CreatedBy = Convert.ToInt32(userId);
                            googleMeetDetails.CreatedDate = DateTime.UtcNow;
                            googleMeetDetails.ModifiedBy = Convert.ToInt32(userId);
                            googleMeetDetails.ModifiedDate = DateTime.UtcNow;

                            googleMeetDetails.IsActive = Record.Active;
                            googleMeetDetails.IsDeleted = Record.NotDeleted;

                            googleMeetDetails.ICalUID = res.ICalUID;
                            googleMeetDetails.MeetingId = res.Id;
                            googleMeetDetails.StartTime = res.Start.DateTime.ToString();
                            googleMeetDetails.Status = res.Status;
                            googleMeetDetails.OrganizerEmail = res.Organizer.Email.ToString();
                            googleMeetDetails.HtmlLink = res.HtmlLink;
                            googleMeetDetails.HangoutLink = res.HangoutLink;
                            googleMeetDetails.UserWebinarId = webinarMaster.Id;
                            await this._db.GoogleMeetDetails.AddAsync(googleMeetDetails);
                            await this._db.SaveChangesAsync();

                            resp.Id = 200;
                            resp.Status = "Meeting created successfully";
                            resp.meetDetails = googleMeetDetails;
                        }
                        else
                        {
                            resp.Id = 103;
                            resp.Status = "Technical Issue, Please contact with admin";                           
                        }
                    }
                    else
                    {
                        resp.Id = 104;
                        resp.Status = "Google Meeting Host Details Not Found";
                    }
                }
                else
                {
                    resp.Id = 102;
                    resp.Status = "Google Meeting Host Details Not Found";
                }
            }
            else
            {
                resp.Id = 101;
                resp.Status = "Google Meeting Host Details Not Found";
            }

            _logger.Info("Create gsuit status online webinar repository" +resp.Status);
            _logger.Info("Create gsuit Id online webinar repository" + resp.Id);
            return resp;
        }

        public async Task<GoogleMeetRessponce> UpdateGSuitEventCalendars(UpdateGsuitMeeting teams, int userId)
        {
            GoogleMeetRessponce resp = new GoogleMeetRessponce();
            GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "googlemeet").FirstOrDefault();
                if (iltonlineSettingteams != null)
                {
                    string AdminAccountId = iltonlineSettingteams.ClientID;
                    string ServiceJsonFilePath = iltonlineSettingteams.ClientSecret;
                    if (!string.IsNullOrEmpty(AdminAccountId) && !string.IsNullOrEmpty(ServiceJsonFilePath))
                    {
                        CalendarService _service = GetCalendarService(ServiceJsonFilePath, AdminAccountId);

                        Event getEvent = new Event();
                        // Retrieve the event from the API
                        try
                        {
                            getEvent = _service.Events.Get(teams.Username, teams.eventId).Execute();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }


                        string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.StartDate.Date);
                        string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", teams.EndDate.Date);

                        var sdate = startdate1.Substring(0, 11);
                        var sdate1 = teams.StartTime;
                        var sdate2 = startdate1.Substring(17, 2);

                        var edate = enddate1.Substring(0, 11);
                        var edate1 = teams.EndTime;
                        var edate2 = enddate1.Substring(17, 2);

                        EventDateTime Starttime = new EventDateTime();
                          
                        Starttime.DateTime = DateTime.Parse(sdate + sdate1 + ":" + sdate2);
                        Starttime.TimeZone = "Asia/Kolkata";

                        EventDateTime Endtime = new EventDateTime();
                        Endtime.DateTime = DateTime.Parse(edate + edate1 + ":" + edate2);
                        Endtime.TimeZone = "Asia/Kolkata";

                      
                        getEvent.Start=Starttime;
                        getEvent.End=Endtime;

                        List<EventAttendee> attendes = new List<EventAttendee>();
                        if (teams.TrinerData != null)
                        {
                            if (teams.TrinerData.Length > 0)
                            {
                                foreach (TrainersData trainer in teams.TrinerData)
                                {

                                    EventAttendee a = new EventAttendee();
                                    a.Email = Security.DecryptForUI(trainer.EmailId);
                                    a.DisplayName = trainer.EmailName;
                                    attendes.Add(a);
                                }
                                getEvent.Attendees = attendes;
                            }
                        }
                        

                        String calendarId = webinarMaster.TeamsEmail;
                        EventsResource.UpdateRequest request = new EventsResource.UpdateRequest(_service,getEvent, calendarId, teams.eventId );
                        request.SendNotifications = true;
                        request.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
                        request.ConferenceDataVersion = 1;
                        Event res = request.Execute();
                        if (res.HtmlLink != null)
                        {
                            googleMeetDetails =await _db.GoogleMeetDetails.Where(a => a.ScheduleID == teams.ScheduleID).FirstOrDefaultAsync();
                            if (googleMeetDetails != null)
                            {
                                googleMeetDetails.CourseID = teams.CourseID;
                                googleMeetDetails.ScheduleID = teams.ScheduleID;
                                googleMeetDetails.ModifiedBy = Convert.ToInt32(userId);
                                googleMeetDetails.ModifiedDate = DateTime.UtcNow;

                                googleMeetDetails.IsActive = Record.Active;
                                googleMeetDetails.IsDeleted = Record.NotDeleted;

                                googleMeetDetails.ICalUID = res.ICalUID;
                                googleMeetDetails.MeetingId = res.Id;
                                googleMeetDetails.StartTime = res.Start.DateTime.ToString();
                                googleMeetDetails.Status = res.Status;
                                googleMeetDetails.OrganizerEmail = res.Organizer.Email.ToString();
                                googleMeetDetails.HtmlLink = res.HtmlLink;                                
                                googleMeetDetails.UserWebinarId = webinarMaster.Id;
                                this._db.GoogleMeetDetails.Update(googleMeetDetails);
                                await this._db.SaveChangesAsync();

                                resp.Id = 200;
                                resp.Status = "Meeting created successfully";
                                resp.meetDetails = googleMeetDetails;
                            }
                            else
                            {
                                resp.Id = 105;
                                resp.Status = "Can Not Update As data not present in googleMeetDetails";
                            }
                        }
                        else
                        {
                            resp.Id = 103;
                            resp.Status = "Technical Issue, Please contact with admin";
                        }
                    }
                    else
                    {
                        resp.Id = 104;
                        resp.Status = "Google Meeting Host Details Not Found";
                    }
                }
                else
                {
                    resp.Id = 102;
                    resp.Status = "Google Meeting Host Details Not Found";
                }
            }
            else
            {
                resp.Id = 101;
                resp.Status = "Google Meeting Host Details Not Found";
            }
            _logger.Info("Update gsuit status online webinar repository" + resp.Status);
            _logger.Info("Update gsuit Id online webinar repository" + resp.Id);
            return resp;
        }



        public async Task<GoogleMeetAttendees> GetGSuitEventCalendars(GoogleMeetDetails teams)
        {
            GoogleMeetAttendees resp = new GoogleMeetAttendees();
            GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.OrganizerEmail).FirstOrDefault();
            if (webinarMaster != null)
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "googlemeet").FirstOrDefault();
                if (iltonlineSettingteams != null)
                {
                    string AdminAccountId = iltonlineSettingteams.ClientID;
                    string ServiceJsonFilePath = iltonlineSettingteams.ClientSecret;
                    if (!string.IsNullOrEmpty(AdminAccountId) && !string.IsNullOrEmpty(ServiceJsonFilePath))
                    {
                        CalendarService _service = GetCalendarService(ServiceJsonFilePath, AdminAccountId);

                        Event getEvent = new Event();
                        List<EventAttendee> attendees = new List<EventAttendee>();
                        // Retrieve the event from the API
                        try
                        {
                            getEvent = _service.Events.Get(webinarMaster.TeamsEmail, teams.MeetingId).Execute();
                            if (getEvent != null)
                            {
                                foreach (EventAttendee item in getEvent.Attendees)
                                {
                                    EventAttendee att = new EventAttendee();
                                    att.Email =Security.EncryptForUI( item.Email);
                                    att.DisplayName = item.DisplayName;
                                    attendees.Add(att);
                                }
                                resp.Id = 200;
                                resp.Status = "Google Meeting Details Found";
                                resp.attendees = attendees.ToList();
                            }
                            else
                            {
                                resp.Id = 103;
                                resp.Status = "Google Meeting Details Not Found";
                                resp.attendees = attendees;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                       
                        
                    }
                    else
                    {
                        resp.Id = 104;
                        resp.Status = "Google Meeting Host Details Not Found";
                    }
                }
                else
                {
                    resp.Id = 102;
                    resp.Status = "Google Meeting Host Details Not Found";
                }
            }
            else
            {
                resp.Id = 101;
                resp.Status = "Google Meeting Host Details Not Found";
            }
            _logger.Info("Update gsuit status online webinar repository" + resp.Status);
            _logger.Info("Update gsuit Id online webinar repository" + resp.Id);
            return resp;
        }

        public async Task<GoogleMeetRessponce> cancleGsuitMeeting(int userId, UserWebinarMaster userWebinarMaster, GoogleMeetDetails meetdetails1)
        {
            GoogleMeetRessponce resp = new GoogleMeetRessponce();
            GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == userWebinarMaster.TeamsEmail).FirstOrDefault();
            if (webinarMaster != null)
            {
                ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "googlemeet").FirstOrDefault();
                if (iltonlineSettingteams != null)
                {
                    string AdminAccountId = iltonlineSettingteams.ClientID;
                    string ServiceJsonFilePath = iltonlineSettingteams.ClientSecret;
                    if (!string.IsNullOrEmpty(AdminAccountId) && !string.IsNullOrEmpty(ServiceJsonFilePath))
                    {
                        CalendarService _service = GetCalendarService(ServiceJsonFilePath, AdminAccountId);


                        // Retrieve the event from the API
                        try
                        {
                            _service.Events.Delete(userWebinarMaster.TeamsEmail, meetdetails1.MeetingId).Execute();

                            googleMeetDetails = await _db.GoogleMeetDetails.Where(a => a.ScheduleID == meetdetails1.ScheduleID).FirstOrDefaultAsync();
                            if (googleMeetDetails != null)
                            {

                                googleMeetDetails.ModifiedBy = Convert.ToInt32(userId);
                                googleMeetDetails.ModifiedDate = DateTime.UtcNow;

                                googleMeetDetails.IsActive = Record.NotActive;
                                googleMeetDetails.IsDeleted = Record.Deleted;

                                this._db.GoogleMeetDetails.Update(googleMeetDetails);
                                await this._db.SaveChangesAsync();

                                resp.Id = 200;
                                resp.Status = "Meeting created successfully";
                                resp.meetDetails = googleMeetDetails;
                            }
                            else
                            {
                                resp.Id = 105;
                                resp.Status = "Can Not Update As data not present in googleMeetDetails";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                    else
                    {
                        resp.Id = 104;
                        resp.Status = "Google Meeting Host Details Not Found";
                    }
                }
                else
                {
                    resp.Id = 102;
                    resp.Status = "Google Meeting Host Details Not Found";
                }
                }
                else
                {
                resp.Id = 101;
                resp.Status = "Google Meeting Host Details Not Found";
            }

            _logger.Info("cancle gsuit status online webinar repository" + resp.Status);
            _logger.Info("cancle gsuit Id online webinar repository" + resp.Id);
            return resp;
        }

        public async Task<EventAttendee[]> CallGSuitUpdateEventCalendars(UpdateGsuit teams, List<EventAttendee> gsuitattendees)
        {
            try
            {
                UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
                if (webinarMaster != null)
                {
                    ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "googlemeet").FirstOrDefault();
                    if (iltonlineSettingteams != null)
                    {
                        string AdminAccountId = iltonlineSettingteams.ClientID;
                        string ServiceJsonFilePath = iltonlineSettingteams.ClientSecret;
                        if (!string.IsNullOrEmpty(AdminAccountId) && !string.IsNullOrEmpty(ServiceJsonFilePath))
                        {
                            CalendarService _service = GetCalendarService(ServiceJsonFilePath, AdminAccountId);

                            Event getEvent = new Event();
                            List<EventAttendee> oldattendees = new List<EventAttendee>();                            
                            try
                            {
                                getEvent = _service.Events.Get(webinarMaster.TeamsEmail, teams.eventId).Execute();
                                if (getEvent != null)
                                {
                                    oldattendees = getEvent.Attendees.ToList();
                                }
                                else
                                {
                                    _logger.Info("Event not found for " + teams.eventId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            if (oldattendees.Count > 0)
                            {
                                foreach (EventAttendee item in gsuitattendees)
                                {
                                    oldattendees.Add(item);
                                }
                            }

                            //Event patche = new Event();
                            //patche.Attendees = oldattendees;
                            //Event geteventpatch = _service.Events.Patch(patche, webinarMaster.TeamsEmail, teams.eventId).Execute();
                            getEvent.Attendees = oldattendees;

                            EventsResource.UpdateRequest request = new EventsResource.UpdateRequest(_service, getEvent, webinarMaster.TeamsEmail, teams.eventId);
                            request.SendNotifications = true;
                            request.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
                            request.ConferenceDataVersion = 1;
                            Event res = request.Execute();

                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return gsuitattendees.ToArray();
        }
        #endregion
    }
}
