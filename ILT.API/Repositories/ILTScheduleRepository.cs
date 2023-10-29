using AutoMapper;
using ILT.API.APIModel;
using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Model.ILT;
using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static ILT.API.APIModel.APIGoToMeeting;
using ILT.API.Common;
using System.Net;
using log4net;
using Dapper;
using System.Globalization;
using OfficeOpenXml;
using ILT.API.Helper.Metadata;
using Azure.Identity;
using Azure.Core;
using Microsoft.Identity.Client;
using System.Security;
//using com.pakhee.common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ILT.API.ExternalIntegration.EdCast;
using ILT.API.Model.TNA;

namespace ILT.API.Repositories
{
    public class ILTScheduleRepository : Repository<ILTSchedule>, IILTSchedule
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTScheduleRepository));
        private CourseContext _db;
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        ICustomerConnectionStringRepository _customerConnection;
        INotification _notification;
        IEmail _email;
        IIdentityService _identitySv;
        ICourseRepository _courseRepository;
        ITLSHelper _tLSHelper;
        IOnlineWebinarRepository _onlineWebinarRepository;


        public ILTScheduleRepository(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment, CourseContext context,
                                     ICustomerConnectionStringRepository customerConnection, IEmail email, IIdentityService identitySv, IConfiguration configuration,
                                     INotification notification, IILTTrainingAttendance iILTTrainingAttendance,
                                     ICourseRepository courseRepository,
                                     ITLSHelper tLSHelper, IOnlineWebinarRepository onlineWebinarRepository) : base(context)
        {
            _db = context;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _notification = notification;
            this._identitySv = identitySv;
            _email = email;
            _customerConnection = customerConnection;
            _courseRepository = courseRepository;
            _tLSHelper = tLSHelper;
            _onlineWebinarRepository = onlineWebinarRepository;
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
                             ScheduleCapacity = ILTSchedule.ScheduleCapacity,
                             RequestApproval = ILTSchedule.RequestApproval
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
                        if (aPIILTSchedular.TrainerList[i].TrainerType == "Other")
                        {
                            aPIILTSchedular.TrainerList[i].NameUserId = aPIILTSchedular.TrainerList[i].AcademyTrainerName + " (" + aPIILTSchedular.TrainerList[i].TrainerType + ")";
                        }
                    }
                }
            }

            return aPIILTSchedular;
        }

        private static async Task<APIZoomDetailsToken> GetElibilityToken(HttpClient client)
        {
            string baseAddress = @"https://zoom.us/oauth/token";

            string grant_type = "client_credentials";
            string client_id = "xBZX9wnWRArQYf4veEfFA";
            string client_secret = "E9NA7nwVKVBDVhv2ACagl6Pk1XZYSVt2";

            var form = new Dictionary<string, string>
                {
                    {"grant_type", grant_type},
                    {"client_id", client_id},
                    {"client_secret", client_secret},
                };

            HttpResponseMessage tokenResponse = await client.PostAsync(baseAddress, new FormUrlEncodedContent(form));
            var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
            APIZoomDetailsToken tok = JsonConvert.DeserializeObject<APIZoomDetailsToken>(jsonContent);
            return tok;
        }

        public async Task<APIZoomDetailsToken> CreateZoomMeeting(string code)
        {
            APIZoomDetailsToken gettokendetails = new APIZoomDetailsToken();
            APIZoomDetailsToken tok = new APIZoomDetailsToken();
            try
            {
                gettokendetails = await GetAccessTokenForZoom(code);
                if (gettokendetails != null)
                {
                    APIZoomMeetingResponce aPIZoomMeetingResponce = await CallZoomMeeting(gettokendetails);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return gettokendetails;
        }



        public async Task<ILTOnlineSetting> GetZoomConfiguration()

        {
            ILTOnlineSetting config = new ILTOnlineSetting();
            try
            {
                config = _db.ILTOnlineSetting.Where(a => a.Type == "ZOOM").FirstOrDefault();
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return config;
        }

        private async Task<APIZoomDetailsToken> GetAccessTokenForZoom(string code = null)
        {
            APIZoomDetailsToken objtoken = new APIZoomDetailsToken();
            try
            {
                if (code != null)
                {
                    ILTOnlineSetting iltonlineSettingZoom = _db.ILTOnlineSetting.Where(a => a.Type == "ZOOM").FirstOrDefault();
                    if (iltonlineSettingZoom != null)
                    {
                        APIZoomDetailsGetToken gettokendetails = new APIZoomDetailsGetToken();
                        gettokendetails.client_id = iltonlineSettingZoom.ClientID;
                        gettokendetails.client_secret = iltonlineSettingZoom.ClientSecret;
                        gettokendetails.authorize_url = "https://zoom.us/oauth/authorize";
                        gettokendetails.access_token_url = "https://zoom.us/oauth/token";
                        gettokendetails.redirect_uri = iltonlineSettingZoom.RedirectUri; //"https://indianoilsampark.com/zoomverify/verifyzoom.html";
                        gettokendetails.authorization_method = "body";
                        gettokendetails.uri = "https://marketplace.zoom.us/docs/oauth/callback/success?code=" + code + "&state=";
                        JObject oJsonObject = new JObject();
                        oJsonObject = JObject.Parse(JsonConvert.SerializeObject(gettokendetails));

                        objtoken = await ApiHelper.GetTokenForZoomMeeting(oJsonObject);
                    }
                }
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return objtoken;
        }

        private async Task<APIZoomMeetingResponce> CallZoomMeeting(APIZoomDetailsToken obj)
        {
            string userid = "sysadmin@enthralltech.com";
            APIZoomCreate aPIZoomCreate = new APIZoomCreate();
            APIZoomCreateSettings aPIZoomCreateSettings = new APIZoomCreateSettings();
            APIZoomCreateRecurrence aPIZoomCreateRecurrence = new APIZoomCreateRecurrence();
            APIZoomMeetingResponce aPIZoomMeetingResponce = new APIZoomMeetingResponce();
            try
            {
                aPIZoomCreateSettings.host_video = true;
                aPIZoomCreateSettings.participant_video = true;
                aPIZoomCreateSettings.cn_meeting = false;
                aPIZoomCreateSettings.in_meeting = true;
                aPIZoomCreateSettings.join_before_host = true;
                aPIZoomCreateSettings.mute_upon_entry = false;
                aPIZoomCreateSettings.enforce_login = false;
                aPIZoomCreateSettings.enforce_login_domains = "https://indianoilsampark.com";

                aPIZoomCreateRecurrence.type = 1;
                aPIZoomCreateRecurrence.repeat_interval = 1;
                aPIZoomCreateRecurrence.weekly_days = 1;
                aPIZoomCreateRecurrence.monthly_week = 1;
                aPIZoomCreateRecurrence.monthly_week_day = 1;
                aPIZoomCreateRecurrence.end_times = 1;
                aPIZoomCreateRecurrence.end_date_time = "2020-09-30T13:00:00Z";

                aPIZoomCreate.topic = "Testmeetingfromlms_iocl";
                aPIZoomCreate.start_time = "2020-09-30T12:00:00Z";
                aPIZoomCreate.duration = 30;
                aPIZoomCreate.timezone = "Asia/Kolkata";
                aPIZoomCreate.password = "*Pass@12_";
                aPIZoomCreate.agenda = "Testmeeting";
                aPIZoomCreate.settings = aPIZoomCreateSettings;
                aPIZoomCreate.recurrence = aPIZoomCreateRecurrence;
                aPIZoomCreate.type = 2;

                JObject oJsonObject = new JObject();
                oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPIZoomCreate));

                aPIZoomMeetingResponce = await ApiHelper.GetMeetingResponce(oJsonObject, userid, obj.access_token);
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return aPIZoomMeetingResponce;
        }


        public async Task<APIZoomMeetingResponce> CallZoomMeetings(APIZoomDetailsToken obj, Teams aPIILTSchedular, int userId)
        {
            APIZoomMeetingResponce aPIZoomMeetingResponce = new APIZoomMeetingResponce();
            Zoom zoom = new Zoom();
            //ILTOnlineSetting iltonlineSettingZoom = _db.ILTOnlineSetting.Where(a => a.Type == "ZOOM").FirstOrDefault();
            if (aPIILTSchedular.Username != null)
            {
                //string userid = iltonlineSettingZoom.UserID;
                string userid = aPIILTSchedular.Username;

                APIZoomCreate aPIZoomCreate = new APIZoomCreate();
                APIZoomCreateSettings aPIZoomCreateSettings = new APIZoomCreateSettings();
                APIZoomCreateRecurrence aPIZoomCreateRecurrence = new APIZoomCreateRecurrence();

                try
                {
                    aPIZoomCreateSettings.host_video = false;
                    aPIZoomCreateSettings.participant_video = false;
                    aPIZoomCreateSettings.cn_meeting = false;
                    aPIZoomCreateSettings.in_meeting = true;
                    aPIZoomCreateSettings.join_before_host = false;
                    aPIZoomCreateSettings.approval_type = 2;
                    aPIZoomCreateSettings.mute_upon_entry = false;
                    aPIZoomCreateSettings.meeting_authentication = false;
                    aPIZoomCreateSettings.authentication_domains = "https://uat.gogetempowered.com";

                    aPIZoomCreateRecurrence.type = 1;
                    aPIZoomCreateRecurrence.repeat_interval = 1;
                    aPIZoomCreateRecurrence.weekly_days = 1;
                    aPIZoomCreateRecurrence.monthly_week = 1;
                    aPIZoomCreateRecurrence.monthly_week_day = 1;
                    aPIZoomCreateRecurrence.end_times = 1;
                    aPIZoomCreateRecurrence.end_date_time = string.Format("{0:yyyy-MM-ddThh:mm:ssZ}", aPIILTSchedular.EndDate.Add(TimeSpan.Parse(aPIILTSchedular.EndTime)));

                    aPIZoomCreate.topic = aPIILTSchedular.CourseName;

                    aPIZoomCreate.start_time = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", aPIILTSchedular.StartDate.Add(TimeSpan.Parse(aPIILTSchedular.StartTime)));
                    aPIZoomCreate.start_time = aPIZoomCreate.start_time.Substring(0, 11);
                    aPIZoomCreate.start_time = aPIZoomCreate.start_time + aPIILTSchedular.StartTime + ":" + "00";
                    TimeSpan diffDate = aPIILTSchedular.EndDate.Date.Add(TimeSpan.Parse(aPIILTSchedular.EndTime)).Subtract(aPIILTSchedular.StartDate.Date.Add(TimeSpan.Parse(aPIILTSchedular.StartTime)));
                    aPIZoomCreate.duration = Convert.ToInt32(diffDate.TotalMinutes);
                    //aPIZoomCreate.timezone = "Asia/Calcutta";//"UTC"; // "Asia /Kolkata";
                    aPIZoomCreate.agenda = aPIILTSchedular.CourseName;
                    aPIZoomCreate.settings = aPIZoomCreateSettings;
                    aPIZoomCreate.recurrence = aPIZoomCreateRecurrence;
                    aPIZoomCreate.type = 2;

                    JObject oJsonObject = new JObject();
                    oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPIZoomCreate));

                    aPIZoomMeetingResponce = await ApiHelper.GetMeetingResponce(oJsonObject, userid, obj.access_token);

                    if (aPIZoomMeetingResponce != null)
                    {
                        UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == userid).FirstOrDefault();
                        ZoomMeetingDetails oldobjzoom = this._db.ZoomMeetingDetails.Where(a => a.ScheduleID == aPIILTSchedular.ScheduleID).FirstOrDefault();
                        if (oldobjzoom == null)
                        {
                            ZoomMeetingDetails objzoom = new ZoomMeetingDetails();
                            objzoom.CourseID = aPIILTSchedular.CourseID;
                            objzoom.ScheduleID = Convert.ToInt32(aPIILTSchedular.ScheduleID);
                            objzoom.CreatedBy = Convert.ToInt32(userId);
                            objzoom.CreatedDate = DateTime.UtcNow;
                            objzoom.ModifiedBy = Convert.ToInt32(userId);
                            objzoom.ModifiedDate = DateTime.UtcNow;

                            objzoom.IsActive = Record.Active;
                            objzoom.IsDeleted = Record.NotDeleted;

                            objzoom.Host_id = aPIZoomMeetingResponce.host_id;
                            objzoom.Join_url = aPIZoomMeetingResponce.join_url;
                            objzoom.Start_time = aPIZoomMeetingResponce.start_time;
                            objzoom.Start_url = aPIZoomMeetingResponce.start_url;
                            objzoom.Timezone = aPIZoomMeetingResponce.timezone;
                            objzoom.Topic = aPIZoomMeetingResponce.topic;
                            objzoom.UniqueMeetingId = Convert.ToString(aPIZoomMeetingResponce.id);
                            objzoom.Uuid = aPIZoomMeetingResponce.uuid;
                            objzoom.Duration = aPIZoomMeetingResponce.duration.ToString();
                            objzoom.UserWebinarId = webinarMaster.Id;
                            await this._db.ZoomMeetingDetails.AddAsync(objzoom);
                            await this._db.SaveChangesAsync();
                        }
                        else
                        {
                            oldobjzoom.CourseID = aPIILTSchedular.CourseID;
                            oldobjzoom.ScheduleID = Convert.ToInt32(aPIILTSchedular.ScheduleID);
                            oldobjzoom.ModifiedBy = Convert.ToInt32(userId);
                            oldobjzoom.ModifiedDate = DateTime.UtcNow;

                            oldobjzoom.IsActive = Record.Active;
                            oldobjzoom.IsDeleted = Record.NotDeleted;

                            oldobjzoom.Host_id = aPIZoomMeetingResponce.host_id;
                            oldobjzoom.Join_url = aPIZoomMeetingResponce.join_url;
                            oldobjzoom.Start_time = aPIZoomMeetingResponce.start_time;
                            oldobjzoom.Start_url = aPIZoomMeetingResponce.start_url;
                            oldobjzoom.Timezone = aPIZoomMeetingResponce.timezone;
                            oldobjzoom.Topic = aPIZoomMeetingResponce.topic;
                            oldobjzoom.UniqueMeetingId = Convert.ToString(aPIZoomMeetingResponce.id);
                            oldobjzoom.Uuid = aPIZoomMeetingResponce.uuid;
                            oldobjzoom.Duration = aPIZoomMeetingResponce.duration.ToString();
                            oldobjzoom.UserWebinarId = webinarMaster.Id;
                            this._db.ZoomMeetingDetails.Update(oldobjzoom);
                            await this._db.SaveChangesAsync();
                        }
                    }
                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return aPIZoomMeetingResponce;
        }

        private async Task<List<TeamsScheduleDetails>> CallTeamsCalendars(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId)
        {
            TeamsResponse aPITeamsCreateResponse = new TeamsResponse();
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            List<TeamsScheduleDetails> teamsScheduleDetailsv2 = new List<TeamsScheduleDetails>();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                string userid = webinarMaster.Username;

                TeamsMeet aPITeamsRequest = new TeamsMeet();
                Participant participant = new Participant();
                Organizer organizer = new Organizer();
                Identity identity = new Identity();
                User user = new User();

                try
                {
                    aPITeamsRequest.startDateTime = string.Format("{0:yyyy-MM-ddThh:mm:00.000+05:30}", teams.StartDate.Date.Add(TimeSpan.Parse(teams.StartTime))); ;
                    aPITeamsRequest.endDateTime = string.Format("{0:yyyy-MM-ddThh:mm:00.000+05:30}", teams.EndDate.Date.Add(TimeSpan.Parse(teams.EndTime))); ;
                    aPITeamsRequest.subject = teams.CourseName;
                    aPITeamsRequest.lobbyBypassSettings = new lobbyBypassSettings();
                    aPITeamsRequest.lobbyBypassSettings.scope = "organizer";
                    if (MeetingId == null)
                    {
                        aPITeamsRequest.participants = new Participant();
                        aPITeamsRequest.participants.organizer = new Organizer();
                        aPITeamsRequest.participants.organizer.identity = new Identity();
                        aPITeamsRequest.participants.organizer.identity.user = new User();
                        aPITeamsRequest.participants.organizer.identity.user.id = objectID;
                    }

                    JObject oJsonObject = new JObject();
                    oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPITeamsRequest));

                    ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                    aPITeamsCreateResponse = await ApiHelper.CreateTeamsEventResponse(oJsonObject, objectID, TeamsAccessToken, configurableValues.BaseUrl, MeetingId);

                    if (aPITeamsCreateResponse != null)
                    {
                        teamsScheduleDetails.CourseID = teams.CourseID;
                        teamsScheduleDetails.ScheduleID = Convert.ToInt32(teams.ScheduleID);
                        teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                        teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                        teamsScheduleDetails.IsActive = Record.Active;
                        teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                        teamsScheduleDetails.iCalUId = aPITeamsCreateResponse.id;
                        teamsScheduleDetails.MeetingId = aPITeamsCreateResponse.id;
                        teamsScheduleDetails.StartTime = aPITeamsCreateResponse.startDateTime.ToString();
                        teamsScheduleDetails.EndTime = aPITeamsCreateResponse.endDateTime.ToString();
                        teamsScheduleDetails.JoinUrl = aPITeamsCreateResponse.joinUrl;
                        teamsScheduleDetails.UserWebinarId = webinarMaster.Id;

                        if (MeetingId == null)
                        {
                            await this._db.TeamsScheduleDetails.AddAsync(teamsScheduleDetails);
                            await this._db.SaveChangesAsync();
                        }
                        teamsScheduleDetailsv2.Add(teamsScheduleDetails);
                    }

                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetailsv2;
        }
        private async Task<TeamsScheduleDetails> CallTeamsCalendarsV2(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId)
        {
            TeamsResponse aPITeamsCreateResponse = new TeamsResponse();
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                string userid = webinarMaster.Username;

                TeamsMeet aPITeamsRequest = new TeamsMeet();
                Participant participant = new Participant();
                Organizer organizer = new Organizer();
                Identity identity = new Identity();
                User user = new User();

                try
                {
                    aPITeamsRequest.startDateTime = string.Format("{0:yyyy-MM-ddThh:mm:00.000+05:30}", teams.StartDate.Date.Add(TimeSpan.Parse(teams.StartTime))); ;
                    aPITeamsRequest.endDateTime = string.Format("{0:yyyy-MM-ddThh:mm:00.000+05:30}", teams.EndDate.Date.Add(TimeSpan.Parse(teams.EndTime))); ;
                    aPITeamsRequest.subject = teams.CourseName;
                    aPITeamsRequest.lobbyBypassSettings = new lobbyBypassSettings();
                    aPITeamsRequest.lobbyBypassSettings.scope = "organizer";
                    if (MeetingId == null)
                    {
                        aPITeamsRequest.participants = new Participant();
                        aPITeamsRequest.participants.organizer = new Organizer();
                        aPITeamsRequest.participants.organizer.identity = new Identity();
                        aPITeamsRequest.participants.organizer.identity.user = new User();
                        aPITeamsRequest.participants.organizer.identity.user.id = objectID;
                    }

                    JObject oJsonObject = new JObject();
                    oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPITeamsRequest));

                    ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                    aPITeamsCreateResponse = await ApiHelper.CreateTeamsEventResponse(oJsonObject, objectID, TeamsAccessToken, configurableValues.BaseUrl, MeetingId);

                    if (aPITeamsCreateResponse != null)
                    {

                        teamsScheduleDetails.CourseID = teams.CourseID;
                        teamsScheduleDetails.ScheduleID = Convert.ToInt32(teams.ScheduleID);
                        teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                        teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                        teamsScheduleDetails.IsActive = Record.Active;
                        teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                        teamsScheduleDetails.iCalUId = aPITeamsCreateResponse.id;
                        teamsScheduleDetails.MeetingId = aPITeamsCreateResponse.id;
                        teamsScheduleDetails.StartTime = aPITeamsCreateResponse.startDateTime.ToString();
                        teamsScheduleDetails.EndTime = aPITeamsCreateResponse.endDateTime.ToString();
                        teamsScheduleDetails.JoinUrl = aPITeamsCreateResponse.joinUrl;
                        teamsScheduleDetails.UserWebinarId = webinarMaster.Id;

                        if (MeetingId == null)
                        {
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
            return teamsScheduleDetails;
        }

        public async Task<List<ILTSchedule>> GetByModuleID(int id)
        {
            var Query = this._db.ILTSchedule
                .Where(ilt => ilt.IsActive == true && ilt.IsDeleted == Record.NotDeleted && ilt.ModuleId == id);

            return await Query.ToListAsync();
        }

        public async Task<List<APIILTSchedular>> GetAllActiveSchedules(int page, int pageSize, int UserId, string OrganisationCode, string schduleType, string search = null, string searchText = null)
        {
            List<APIILTSchedular> ScheduleList = new List<APIILTSchedular>();
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
                            cmd.CommandText = "GetScheduleDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTSchedular obj = new APIILTSchedular();

                                obj.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                obj.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                obj.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                obj.StartTime = row["StartTime"].ToString();
                                obj.EndTime = row["EndTime"].ToString();
                                obj.RegistrationEndDate = Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                obj.IsActive = Convert.ToBoolean(row["Status"].ToString());
                                obj.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                obj.TrainerType = row["TrainerType"].ToString();
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.ContactPersonName = row["ContactPersonName"].ToString();
                                obj.City = row["City"].ToString();
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                obj.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                obj.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                obj.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                obj.TrainerDescription = row["TrainerDescription"].ToString();
                                obj.ScheduleType = row["ScheduleType"].ToString();
                                obj.ReasonForCancellation = row["ReasonForCancellation"].ToString();
                                obj.Status = Convert.ToBoolean(row["Status"].ToString());
                                obj.EventLogo = row["EventLogo"].ToString();
                                obj.Cost = string.IsNullOrEmpty(row["Cost"].ToString()) ? 0 : float.Parse(row["Cost"].ToString());
                                obj.Currency = row["Currency"].ToString();
                                obj.CourseCode = row["CourseCode"].ToString();
                                obj.CourseName = row["CourseName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(Convert.ToString(row["CourseID"])) ? 0 : Convert.ToInt32(row["CourseID"].ToString());
                                obj.BatchId = string.IsNullOrEmpty(Convert.ToString(row["BatchId"])) ? 0 : Convert.ToInt32(row["BatchId"].ToString());
                                obj.BatchCode = string.IsNullOrEmpty(Convert.ToString(row["BatchCode"])) ? null : Convert.ToString(row["BatchCode"]);
                                obj.BatchName = string.IsNullOrEmpty(Convert.ToString(row["BatchName"])) ? null : Convert.ToString(row["BatchName"]);
                                ScheduleList.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ScheduleList;
        }

        public async Task<int> GetAllActiveSchedulesCount(int UserId, string OrganisationCode, string search = null, string searchText = null, bool showAllData = false)
        {
            int Count = 0;
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
                            cmd.CommandText = "GetScheduleDetailsCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = showAllData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return 0;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = int.Parse(row["TotalRecord"].ToString());
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public bool ValidateSQLInjection(APIILTSchedular aPIILTSchedular)
        {
            if (FileValidation.CheckForSQLInjection(aPIILTSchedular.AcademyAgencyName)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.PlaceName)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.SeatCapacity)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.City)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.postalAddress)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.ContactPersonName)
                || FileValidation.CheckForSQLInjection(aPIILTSchedular.ContactNumber))
                return true;
            else
                return false;
        }

        public async Task<ApiResponse> PostILT(APIILTSchedular aPIILTSchedular, int userId, string OrganisationCode)
        {
            ApiResponse Response = new ApiResponse();
            int ScheduleId = 0;
            List<TrainerListWithUserNameId> trainerListWithUserNameIds = new List<TrainerListWithUserNameId>();
            string type = null;
            for (int i = 0; i < aPIILTSchedular.TrainerList.Length; i++)
            {
                TrainerListWithUserNameId trainerListWithUserNameId = new TrainerListWithUserNameId();
                trainerListWithUserNameId.AcademyTrainerID = aPIILTSchedular.TrainerList[i].AcademyTrainerID;
                trainerListWithUserNameId.AcademyTrainerName = aPIILTSchedular.TrainerList[i].AcademyTrainerName;
                trainerListWithUserNameId.TrainerType = aPIILTSchedular.TrainerList[i].TrainerType;

                switch (aPIILTSchedular.TrainerList[i].TrainerType.ToLower())
                {
                    case "internal":
                        type = "Internal";
                        break;

                    case "external":
                        if (type != "Internal")
                        {
                            type = "External";
                        }
                        break;

                    case "Consultant":
                        if (type != "Internal" && type != "External")
                        {
                            type = "Consultant";
                        }
                        break;

                    case "other":
                        if (type != "Internal" && type != "External" && type != "Consultant")
                        {
                            type = "Other";
                        }

                        break;
                }

                trainerListWithUserNameIds.Add(trainerListWithUserNameId);

            }
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[ILTSchedule_Insert]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@ScheduleCode", SqlDbType.VarChar) { Value = aPIILTSchedular.ScheduleCode });
                        cmd.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.Int) { Value = aPIILTSchedular.BatchId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = aPIILTSchedular.ModuleId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleName", SqlDbType.VarChar) { Value = aPIILTSchedular.ModuleName });
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTSchedular.CourseID });
                        cmd.Parameters.Add(new SqlParameter("@CourseName", SqlDbType.VarChar) { Value = aPIILTSchedular.CourseName });
                        cmd.Parameters.Add(new SqlParameter("@TrainerType", SqlDbType.VarChar) { Value = type });
                        cmd.Parameters.Add(new SqlParameter("@AcademyAgencyId", SqlDbType.Int) { Value = aPIILTSchedular.AcademyAgencyID });
                        cmd.Parameters.Add(new SqlParameter("@AcademyAgencyName", SqlDbType.VarChar) { Value = aPIILTSchedular.AcademyAgencyName });
                        cmd.Parameters.Add(new SqlParameter("@AcademyTrainerId", SqlDbType.Int) { Value = aPIILTSchedular.AcademyTrainerID });
                        cmd.Parameters.Add(new SqlParameter("@AcademyTrainerName", SqlDbType.VarChar) { Value = aPIILTSchedular.AcademyTrainerName });
                        cmd.Parameters.Add(new SqlParameter("@AgencyTrainerName", SqlDbType.VarChar) { Value = aPIILTSchedular.AgencyTrainerName });
                        cmd.Parameters.Add(new SqlParameter("@TrainerDescription", SqlDbType.VarChar) { Value = aPIILTSchedular.TrainerDescription });
                        cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.VarChar) { Value = aPIILTSchedular.City });
                        cmd.Parameters.Add(new SqlParameter("@SeatCapacity", SqlDbType.VarChar) { Value = aPIILTSchedular.SeatCapacity });
                        cmd.Parameters.Add(new SqlParameter("@PostalAddress", SqlDbType.VarChar) { Value = aPIILTSchedular.postalAddress });
                        cmd.Parameters.Add(new SqlParameter("@ContactPersonName", SqlDbType.VarChar) { Value = aPIILTSchedular.ContactPersonName });
                        cmd.Parameters.Add(new SqlParameter("@ContactNumber", SqlDbType.VarChar) { Value = aPIILTSchedular.ContactNumber });
                        cmd.Parameters.Add(new SqlParameter("@PlaceID", SqlDbType.Int) { Value = aPIILTSchedular.PlaceID });
                        cmd.Parameters.Add(new SqlParameter("@PlaceName", SqlDbType.VarChar) { Value = aPIILTSchedular.PlaceName });
                        cmd.Parameters.Add(new SqlParameter("@PlaceType", SqlDbType.VarChar) { Value = aPIILTSchedular.PlaceType });
                        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = aPIILTSchedular.StartDate });
                        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = aPIILTSchedular.EndDate });
                        cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = aPIILTSchedular.StartTime });
                        cmd.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.Time) { Value = aPIILTSchedular.EndTime });
                        cmd.Parameters.Add(new SqlParameter("@RegistrationEndDate", SqlDbType.DateTime) { Value = aPIILTSchedular.RegistrationEndDate });
                        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true });
                        cmd.Parameters.Add(new SqlParameter("@EventLogo", SqlDbType.VarChar) { Value = aPIILTSchedular.EventLogo });
                        cmd.Parameters.Add(new SqlParameter("@Cost", SqlDbType.BigInt) { Value = aPIILTSchedular.Cost });
                        cmd.Parameters.Add(new SqlParameter("@Currency", SqlDbType.VarChar) { Value = aPIILTSchedular.Currency });
                        cmd.Parameters.Add(new SqlParameter("@WebinarType", SqlDbType.VarChar) { Value = aPIILTSchedular.WebinarType });
                        cmd.Parameters.Add(new SqlParameter("@TrainerList", SqlDbType.Structured) { Value = trainerListWithUserNameIds.ToList().ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@HolidayList", SqlDbType.Structured) { Value = aPIILTSchedular.HolidayList.ToList().ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@Purpose", SqlDbType.VarChar) { Value = aPIILTSchedular.Purpose });
                        cmd.Parameters.Add(new SqlParameter("@ScheduleCapacity", SqlDbType.Int) { Value = aPIILTSchedular.ScheduleCapacity });
                        cmd.Parameters.Add(new SqlParameter("@RequestApproval", SqlDbType.Bit) { Value = aPIILTSchedular.RequestApproval });

                        SqlParameter ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                        ResponseParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ResponseParam);
                        SqlParameter ScheduleIdParam = new SqlParameter("@ScheduleIdOutPut", SqlDbType.Int);
                        ScheduleIdParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ScheduleIdParam);
                        await cmd.ExecuteNonQueryAsync();
                        string ResponseValue = Convert.ToString(ResponseParam.Value);
                        if (ResponseValue == "Success")
                        {
                            ScheduleId = Convert.ToInt32(ScheduleIdParam.Value);
                        }
                        else
                        {
                            return new ApiResponse { Description = ResponseValue, StatusCode = 400 };
                        }
                    }
                    connection.Close();
                }
            }

            // ------ Add ons Webinar type code ----//          
            var ModuleType = _db.Module.Where(a => a.Id == aPIILTSchedular.ModuleId).Select(c => c.ModuleType).FirstOrDefault();
            if (ModuleType == "vilt")
            {
                aPIILTSchedular.ID = ScheduleId;
                bool val = await CreateWebinarLink(aPIILTSchedular, userId);
            }
            // ------ Add ons Webinar type code  end ----//

            //  bell and mail notification on schedule creation //
            string token = _identitySv.GetToken();
            bool Courseresult = await this.CourseEnrolledByTNA(aPIILTSchedular.CourseID);
            if (Courseresult == true)
            {
                //commented to resolve 502 error
                Task.Run(() => SendScheduleCreationBellNotification(aPIILTSchedular.CourseID, aPIILTSchedular.ID, token, OrganisationCode));
                Task.Run(() => _email.SendScheduleCreationEmailNotification(aPIILTSchedular.CourseID, OrganisationCode, aPIILTSchedular.ID));
            }
            else
            {
                if (OrganisationCode != "dwtc")
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                _logger.Debug("Sending Schedule Creation Notification to Trainers");
                                cmd.CommandText = "GetAllUsersForScheduleCreationMail";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@ILTTrainerList", SqlDbType.Structured) { Value = trainerListWithUserNameIds.ToList().ToDataTable() });
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    connection.Close();
                                }
                                int count = 0;
                                List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    string title = "Schedule Creation Notification";
                                    int UserIDToSend = Convert.ToInt32(row["ID"]);
                                    string Type = Record.Enrollment1;
                                    string Message = "New schedule created '{scheduleCode}' awaiting for your response.";
                                    Message = Message.Replace("{scheduleCode}", aPIILTSchedular.ScheduleCode);

                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = title;
                                    Notification.Message = Message;
                                    Notification.Url = TlsUrl.NotificationAPost + aPIILTSchedular.CourseID;// + '/' + aPIILTSchedular.ID;
                                    Notification.Type = Type;
                                    Notification.UserId = UserIDToSend;
                                    lstApiNotification.Add(Notification);
                                    count++;
                                    if (count % Constants.BATCH_SIZE == 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }
                                }
                                if (lstApiNotification.Count > 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }
                                reader.Dispose();
                                _logger.Debug("Sent Schedule Creation Notification to Trainers");
                            }
                            connection.Close();
                        }
                    }
                }
            }

            //  bell and mail notification on schedule creation //
            Response.Description = "success";
            Response.StatusCode = 200;
            return Response;
        }

        public async Task<ApiResponse> PutILT(APIILTSchedular aPIILTSchedular, int userId, string OrganisationCode)
        {
            ApiResponse Response = new ApiResponse();
            int ScheduleId = 0;
            string type = null;
            List<TrainerListWithUserNameId> trainerListWithUserNameIds = new List<TrainerListWithUserNameId>();
            ILTSchedule iLTSchedule = _db.ILTSchedule.Where(a => a.ID == aPIILTSchedular.ID).FirstOrDefault();
            for (int i = 0; i < aPIILTSchedular.TrainerList.Length; i++)
            {
                TrainerListWithUserNameId trainerListWithUserNameId = new TrainerListWithUserNameId();
                trainerListWithUserNameId.AcademyTrainerID = aPIILTSchedular.TrainerList[i].AcademyTrainerID;
                trainerListWithUserNameId.AcademyTrainerName = aPIILTSchedular.TrainerList[i].AcademyTrainerName;
                trainerListWithUserNameId.TrainerType = aPIILTSchedular.TrainerList[i].TrainerType;

                switch (aPIILTSchedular.TrainerList[i].TrainerType.ToLower())
                {
                    case "internal":
                        type = "Internal";
                        break;
                    case "external":
                        if (iLTSchedule.TrainerType != "internal")
                        {
                            type = "External";
                        }
                        break;
                    case "Consultant":
                        if (iLTSchedule.TrainerType != "Internal" && iLTSchedule.TrainerType != "External")
                        {
                            type = "Consultant";
                        }
                        break;
                    case "other":
                        if (iLTSchedule.TrainerType != "Internal" && iLTSchedule.TrainerType != "External" && iLTSchedule.TrainerType != "Consultant")
                        {
                            type = "Other";
                        }

                        break;
                }


                trainerListWithUserNameIds.Add(trainerListWithUserNameId);

            }
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[ILTSchedule_Insert]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.Int) { Value = aPIILTSchedular.ID });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@ScheduleCode", SqlDbType.VarChar) { Value = aPIILTSchedular.ScheduleCode });
                        cmd.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.Int) { Value = aPIILTSchedular.BatchId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = aPIILTSchedular.ModuleId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleName", SqlDbType.VarChar) { Value = aPIILTSchedular.ModuleName });
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTSchedular.CourseID });
                        cmd.Parameters.Add(new SqlParameter("@CourseName", SqlDbType.VarChar) { Value = aPIILTSchedular.CourseName });
                        cmd.Parameters.Add(new SqlParameter("@TrainerType", SqlDbType.VarChar) { Value = type });
                        cmd.Parameters.Add(new SqlParameter("@AcademyAgencyId", SqlDbType.Int) { Value = aPIILTSchedular.AcademyAgencyID });
                        cmd.Parameters.Add(new SqlParameter("@AcademyAgencyName", SqlDbType.VarChar) { Value = aPIILTSchedular.AcademyAgencyName });
                        cmd.Parameters.Add(new SqlParameter("@AcademyTrainerId", SqlDbType.Int) { Value = aPIILTSchedular.AcademyTrainerID });
                        cmd.Parameters.Add(new SqlParameter("@AcademyTrainerName", SqlDbType.VarChar) { Value = aPIILTSchedular.AcademyTrainerName });
                        cmd.Parameters.Add(new SqlParameter("@AgencyTrainerName", SqlDbType.VarChar) { Value = aPIILTSchedular.AgencyTrainerName });
                        cmd.Parameters.Add(new SqlParameter("@TrainerDescription", SqlDbType.VarChar) { Value = aPIILTSchedular.TrainerDescription });
                        cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.VarChar) { Value = aPIILTSchedular.City });
                        cmd.Parameters.Add(new SqlParameter("@SeatCapacity", SqlDbType.VarChar) { Value = aPIILTSchedular.SeatCapacity });
                        cmd.Parameters.Add(new SqlParameter("@PostalAddress", SqlDbType.VarChar) { Value = aPIILTSchedular.postalAddress });
                        cmd.Parameters.Add(new SqlParameter("@ContactPersonName", SqlDbType.VarChar) { Value = aPIILTSchedular.ContactPersonName });
                        cmd.Parameters.Add(new SqlParameter("@ContactNumber", SqlDbType.VarChar) { Value = aPIILTSchedular.ContactNumber });
                        cmd.Parameters.Add(new SqlParameter("@PlaceID", SqlDbType.Int) { Value = aPIILTSchedular.PlaceID });
                        cmd.Parameters.Add(new SqlParameter("@PlaceName", SqlDbType.VarChar) { Value = aPIILTSchedular.PlaceName });
                        cmd.Parameters.Add(new SqlParameter("@PlaceType", SqlDbType.VarChar) { Value = aPIILTSchedular.PlaceType });
                        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = aPIILTSchedular.StartDate });
                        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = aPIILTSchedular.EndDate });
                        cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = aPIILTSchedular.StartTime });
                        cmd.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.Time) { Value = aPIILTSchedular.EndTime });
                        cmd.Parameters.Add(new SqlParameter("@RegistrationEndDate", SqlDbType.DateTime) { Value = aPIILTSchedular.RegistrationEndDate });
                        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true });
                        cmd.Parameters.Add(new SqlParameter("@EventLogo", SqlDbType.VarChar) { Value = aPIILTSchedular.EventLogo });
                        cmd.Parameters.Add(new SqlParameter("@Cost", SqlDbType.BigInt) { Value = aPIILTSchedular.Cost });
                        cmd.Parameters.Add(new SqlParameter("@Currency", SqlDbType.VarChar) { Value = aPIILTSchedular.Currency });
                        cmd.Parameters.Add(new SqlParameter("@WebinarType", SqlDbType.VarChar) { Value = aPIILTSchedular.WebinarType });
                        cmd.Parameters.Add(new SqlParameter("@TrainerList", SqlDbType.Structured) { Value = trainerListWithUserNameIds.ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@HolidayList", SqlDbType.Structured) { Value = aPIILTSchedular.HolidayList.ToList().ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@Purpose", SqlDbType.VarChar) { Value = aPIILTSchedular.Purpose });
                        cmd.Parameters.Add(new SqlParameter("@ScheduleCapacity", SqlDbType.Int) { Value = aPIILTSchedular.ScheduleCapacity });
                        cmd.Parameters.Add(new SqlParameter("@RequestApproval", SqlDbType.Bit) { Value = aPIILTSchedular.RequestApproval });

                        SqlParameter ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                        ResponseParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ResponseParam);
                        SqlParameter ScheduleIdParam = new SqlParameter("@ScheduleIdOutPut", SqlDbType.Int);
                        ScheduleIdParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ScheduleIdParam);
                        await cmd.ExecuteNonQueryAsync();
                        string ResponseValue = Convert.ToString(ResponseParam.Value);
                        if (ResponseValue == "Success")
                        {
                            ScheduleId = Convert.ToInt32(ScheduleIdParam.Value);
                        }
                        else
                        {
                            return new ApiResponse { Description = ResponseValue, StatusCode = 400 };
                        }
                    }
                    connection.Close();
                }
            }

            // ------ Add ons Webinar type code ----//          
            var ModuleType = _db.Module.Where(a => a.Id == aPIILTSchedular.ModuleId).Select(c => c.ModuleType).FirstOrDefault();
            if (ModuleType == "vilt")
            {
                aPIILTSchedular.ID = ScheduleId;
                bool val = await CreateWebinarLink(aPIILTSchedular, userId);
            }
            // ------ Add ons Webinar type code  end ----//

            //  bell and mail notification on schedule creation //
            string token = _identitySv.GetToken();
            bool Courseresult = await this.CourseEnrolledByTNA(aPIILTSchedular.CourseID);
            if (Courseresult == true)
            {
                //commented to resolve 502 error
                Task.Run(() => SendScheduleCreationBellNotification(aPIILTSchedular.CourseID, aPIILTSchedular.ID, token, OrganisationCode));
                Task.Run(() => _email.SendScheduleCreationEmailNotification(aPIILTSchedular.CourseID, OrganisationCode, aPIILTSchedular.ID));
            }
            else
            {
                if (OrganisationCode != "dwtc")
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                _logger.Debug("Sending Schedule Creation Notification to Trainers");
                                cmd.CommandText = "GetAllUsersForScheduleCreationMail";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@ILTTrainerList", SqlDbType.Structured) { Value = trainerListWithUserNameIds.ToList().ToDataTable() });
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    connection.Close();
                                }
                                int count = 0;
                                List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    string title = "Schedule Creation Notification";
                                    int UserIDToSend = Convert.ToInt32(row["ID"]);
                                    string Type = Record.Enrollment1;
                                    string Message = "New schedule created '{scheduleCode}' awaiting for your response.";
                                    Message = Message.Replace("{scheduleCode}", aPIILTSchedular.ScheduleCode);

                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = title;
                                    Notification.Message = Message;
                                    Notification.Url = TlsUrl.NotificationAPost + aPIILTSchedular.CourseID; // + '/' + aPIILTSchedular.ID;
                                    Notification.Type = Type;
                                    Notification.UserId = UserIDToSend;
                                    lstApiNotification.Add(Notification);
                                    count++;
                                    if (count % Constants.BATCH_SIZE == 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }
                                }
                                if (lstApiNotification.Count > 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }
                                reader.Dispose();
                                _logger.Debug("Sent Schedule Creation Notification to Trainers");
                            }
                            connection.Close();
                        }
                    }
                }
            }

            //  bell and mail notification on schedule creation //
            Response.Description = "success";
            Response.StatusCode = 200;
            return Response;
        }

        public async Task<bool> CreateWebinarLink(APIILTSchedular aPIILTSchedular, int userId)
        {
            bool flag = true;
            try
            {
                string ReturnURL = null;
                if (aPIILTSchedular.WebinarType.ToLower() == "gotomeeting")
                {
                    ILTOnlineSetting iltonlineSetting = _db.ILTOnlineSetting.Where(a => a.Type == "GOTOMEETING").FirstOrDefault();
                    string responce = await ApiHelper.DirectLogin(iltonlineSetting);
                    string accessToken = responce.ToString();
                    ReturnURL = CreateGoToMeeting(aPIILTSchedular.CourseID, userId, aPIILTSchedular.StartDate.Add(TimeSpan.Parse(aPIILTSchedular.StartTime)), aPIILTSchedular.EndDate.Add(TimeSpan.Parse(aPIILTSchedular.EndTime)), accessToken, Convert.ToInt32(aPIILTSchedular.ID));
                }
                else if (aPIILTSchedular.WebinarType.ToLower() == "zoom")
                {
                    ZoomMeetingDetails zoomMeetingDetails = this._db.ZoomMeetingDetails.Where(a => a.Uuid == aPIILTSchedular.zoomScheduleDetails.uuid).FirstOrDefault();  //&& a.Start_url == aPIILTSchedular.zoomScheduleDetails.start_url for manual entry of link

                    if (zoomMeetingDetails != null)
                    {
                        zoomMeetingDetails.ScheduleID = aPIILTSchedular.ID;
                        zoomMeetingDetails.Start_url = aPIILTSchedular.zoomScheduleDetails.start_url;
                        zoomMeetingDetails.Join_url = aPIILTSchedular.zoomScheduleDetails.join_url;
                        this._db.ZoomMeetingDetails.Update(zoomMeetingDetails);
                        await this._db.SaveChangesAsync();
                    }

                }
                else if (aPIILTSchedular.WebinarType.ToLower() == "teams")
                {
                    try
                    {
                        ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ATPTLWCS").FirstOrDefault();
                        if (conferenceParameters != null)
                        {
                            if (conferenceParameters.Value.ToLower() == "no")
                            {
                                TeamsScheduleDetails teamsResponse = new TeamsScheduleDetails();
                                if (aPIILTSchedular != null)
                                {
                                    foreach (TeamsScheduleDetails teamsScheduleDetails in aPIILTSchedular.teamsScheduleDetails)
                                    {
                                        teamsResponse = this._db.TeamsScheduleDetails.Where(a => a.MeetingId == teamsScheduleDetails.MeetingId).FirstOrDefault();

                                        if (teamsResponse != null)
                                        {
                                            if (teamsResponse.ScheduleID == 0)
                                            {
                                                teamsResponse.ScheduleID = aPIILTSchedular.ID;
                                                teamsResponse.JoinUrl = teamsScheduleDetails.JoinUrl;
                                                this._db.TeamsScheduleDetails.Update(teamsResponse);
                                                await this._db.SaveChangesAsync();
                                            }
                                            else
                                            {
                                                teamsResponse.ModifiedBy = userId;
                                                teamsResponse.ModifiedDate = DateTime.Now;
                                                teamsResponse.MeetingId = teamsScheduleDetails.MeetingId;
                                                teamsResponse.JoinUrl = teamsScheduleDetails.JoinUrl;
                                                this._db.TeamsScheduleDetails.Update(teamsResponse);
                                                await this._db.SaveChangesAsync();
                                            }
                                        }
                                        else
                                        {
                                            _logger.Error("Teams Response data not found");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TeamsScheduleDetails teamsScheduleDetails1 = this._db.TeamsScheduleDetails.Where(a => a.ScheduleID == aPIILTSchedular.ID).FirstOrDefault();
                                if (teamsScheduleDetails1 == null)
                                {
                                    TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
                                    string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", aPIILTSchedular.StartDate);
                                    string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", aPIILTSchedular.EndDate);
                                    var sdate = startdate1.Substring(0, 11);
                                    var sdate1 = aPIILTSchedular.StartTime;
                                    var sdate2 = startdate1.Substring(17, 2);

                                    var edate = enddate1.Substring(0, 11);
                                    var edate1 = aPIILTSchedular.EndTime;
                                    var edate2 = enddate1.Substring(17, 2);

                                    teamsScheduleDetails.StartTime = sdate + sdate1 + ":" + sdate2;
                                    teamsScheduleDetails.EndTime = edate + edate1 + ":" + edate2;
                                    teamsScheduleDetails.IsDeleted = false;
                                    teamsScheduleDetails.CreatedBy = userId;
                                    teamsScheduleDetails.ModifiedBy = userId;
                                    teamsScheduleDetails.ScheduleID = aPIILTSchedular.ID;
                                    teamsScheduleDetails.CreatedDate = DateTime.Now;
                                    teamsScheduleDetails.ModifiedDate = DateTime.Now;
                                    teamsScheduleDetails.CourseID = aPIILTSchedular.CourseID;
                                    teamsScheduleDetails.MeetingId = "MeetingId";
                                    teamsScheduleDetails.IsActive = true;
                                    teamsScheduleDetails.UserWebinarId = 0;

                                    teamsScheduleDetails.JoinUrl = aPIILTSchedular.teamsScheduleDetails[0].JoinUrl;

                                    this._db.TeamsScheduleDetails.Add(teamsScheduleDetails);
                                    this._db.SaveChanges();
                                }
                                else
                                {
                                    string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", aPIILTSchedular.StartDate);
                                    string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", aPIILTSchedular.EndDate);
                                    var sdate = startdate1.Substring(0, 11);
                                    var sdate1 = aPIILTSchedular.StartTime;
                                    var sdate2 = startdate1.Substring(17, 2);

                                    var edate = enddate1.Substring(0, 11);
                                    var edate1 = aPIILTSchedular.EndTime;
                                    var edate2 = enddate1.Substring(17, 2);

                                    teamsScheduleDetails1.StartTime = sdate + sdate1 + ":" + sdate2;
                                    teamsScheduleDetails1.EndTime = edate + edate1 + ":" + edate2;
                                    teamsScheduleDetails1.ModifiedBy = userId;
                                    teamsScheduleDetails1.ModifiedDate = DateTime.Now;

                                    this._db.TeamsScheduleDetails.Update(teamsScheduleDetails1);
                                    this._db.SaveChanges();

                                }
                            }
                        }
                        else
                        {
                            TeamsScheduleDetails teamsResponse = new TeamsScheduleDetails();
                            if (aPIILTSchedular != null)
                            {
                                foreach (TeamsScheduleDetails teamsScheduleDetails in aPIILTSchedular.teamsScheduleDetails)
                                {
                                    teamsResponse = this._db.TeamsScheduleDetails.Where(a => a.MeetingId == teamsScheduleDetails.MeetingId).FirstOrDefault();

                                    if (teamsResponse != null)
                                    {
                                        if (teamsResponse.ScheduleID == 0)
                                        {
                                            teamsResponse.ScheduleID = aPIILTSchedular.ID;
                                            teamsResponse.JoinUrl = teamsScheduleDetails.JoinUrl;
                                            this._db.TeamsScheduleDetails.Update(teamsResponse);
                                            await this._db.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            teamsResponse.ModifiedBy = userId;
                                            teamsResponse.ModifiedDate = DateTime.Now;
                                            teamsResponse.MeetingId = teamsScheduleDetails.MeetingId;
                                            teamsResponse.JoinUrl = teamsScheduleDetails.JoinUrl;
                                            this._db.TeamsScheduleDetails.Update(teamsResponse);
                                            await this._db.SaveChangesAsync();
                                        }
                                    }
                                    else
                                    {
                                        _logger.Error("Teams Response data not found");
                                    }
                                }
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        flag = false;
                    }
                }
                else if (aPIILTSchedular.WebinarType.ToLower() == "bigbluebutton" || aPIILTSchedular.WebinarType.ToLower() == "aeracademy")
                {
                    try
                    {
                        BBBMeeting bbbMeeting = new BBBMeeting();
                        bbbMeeting.CourseId = Convert.ToInt32(aPIILTSchedular.CourseID);
                        bbbMeeting.ScheduleID = Convert.ToInt32(aPIILTSchedular.ID);
                        bbbMeeting.CreatedBy = userId;
                        bbbMeeting.CreatedAt = DateTime.UtcNow;
                        bbbMeeting.ModifiedBy = userId;
                        bbbMeeting.ModifiedAt = DateTime.UtcNow;
                        bbbMeeting.MeetingID = GetRandomMeetingID();
                        bbbMeeting.RecordID = bbbMeeting.MeetingID;

                        bbbMeeting.Record = aPIILTSchedular.Record;
                        bbbMeeting.AutoStartRecording = aPIILTSchedular.AutoStartRecording;
                        bbbMeeting.AllowStartStopRecording = aPIILTSchedular.AllowStartStopRecording;
                        bbbMeeting.MeetingTime = aPIILTSchedular.StartTime;
                        bbbMeeting.MeetingDate = aPIILTSchedular.StartDate;
                        bbbMeeting.Duration = GetMeetingDuration(aPIILTSchedular.StartTime, aPIILTSchedular.EndTime);
                        bbbMeeting.MeetingName = aPIILTSchedular.ModuleName;
                        bbbMeeting.ModuleID = aPIILTSchedular.ModuleId;
                        this._db.BBBMeeting.Add(bbbMeeting);
                        this._db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        flag = false;
                    }
                }
                else if (aPIILTSchedular.WebinarType.ToLower() == "googlemeet")
                {
                    try
                    {
                        GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();

                        googleMeetDetails = this._db.GoogleMeetDetails.Where(a => a.MeetingId == aPIILTSchedular.googleMeetDetails.MeetingId).FirstOrDefault();



                        if (googleMeetDetails != null)
                        {
                            if (googleMeetDetails.ScheduleID == 0)
                            {
                                googleMeetDetails.ScheduleID = aPIILTSchedular.ID;
                                googleMeetDetails.HangoutLink = aPIILTSchedular.googleMeetDetails.HangoutLink;
                                googleMeetDetails.HtmlLink = aPIILTSchedular.googleMeetDetails.HtmlLink;
                                googleMeetDetails.ICalUID = aPIILTSchedular.googleMeetDetails.ICalUID;
                                this._db.GoogleMeetDetails.Update(googleMeetDetails);
                                await this._db.SaveChangesAsync();
                            }
                            else
                            {
                                googleMeetDetails.ModifiedBy = userId;
                                googleMeetDetails.ModifiedDate = DateTime.Now;
                                googleMeetDetails.MeetingId = aPIILTSchedular.googleMeetDetails.MeetingId;
                                googleMeetDetails.HangoutLink = aPIILTSchedular.googleMeetDetails.HangoutLink;
                                googleMeetDetails.HtmlLink = aPIILTSchedular.googleMeetDetails.HtmlLink;
                                googleMeetDetails.ICalUID = aPIILTSchedular.googleMeetDetails.ICalUID;
                                googleMeetDetails.StartTime = aPIILTSchedular.googleMeetDetails.StartTime;
                                googleMeetDetails.Status = aPIILTSchedular.googleMeetDetails.Status;
                                this._db.GoogleMeetDetails.Update(googleMeetDetails);
                                await this._db.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        flag = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                flag = false;
            }
            return flag;
        }

        private int GetMeetingDuration(string startTime, string endTime)
        {
            try
            {
                int hourMinutes = (DateTime.Parse(endTime, System.Globalization.CultureInfo.CurrentCulture).Hour - DateTime.Parse(startTime, System.Globalization.CultureInfo.CurrentCulture).Hour) * 60;
                hourMinutes = hourMinutes + (DateTime.Parse(endTime, System.Globalization.CultureInfo.CurrentCulture).Minute - DateTime.Parse(startTime, System.Globalization.CultureInfo.CurrentCulture).Minute);
                return hourMinutes;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }

        private string GetRandomMeetingID()
        {
            return (Convert.ToString(DateTime.Now) + new Random().Next(1, 100000)).Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace("/", "_").Replace("AM", "").Replace("PM", "_");
        }

        public string CreateGoToMeeting(int? courseID, int userId, DateTime starttime, DateTime endtime, string accessToken, int? scheduleID)
        {
            string JoinUrl = string.Empty;

            try
            {
                GoToMeetingDetails gotomeeting = _db.GoToMeetingDetails.Where(a => a.ScheduleID == scheduleID).FirstOrDefault();
                string subject = _db.Course.Where(a => a.Id == courseID).Select(c => c.Title).FirstOrDefault();

                if (gotomeeting != null)
                {
                    JoinUrl = gotomeeting.JoinURL.ToString();
                }
                if (string.IsNullOrEmpty(JoinUrl))
                {
                    JObject oJsonObject = new JObject();
                    oJsonObject.Add("subject", subject);
                    oJsonObject.Add("starttime", string.Format("{0:yyyy-MM-ddThh:mm:ssZ}", starttime));
                    oJsonObject.Add("endtime", string.Format("{0:yyyy-MM-ddThh:mm:ssZ}", endtime));
                    oJsonObject.Add("passwordrequired", false);
                    oJsonObject.Add("conferencecallinfo", "conferencecallinfo");
                    oJsonObject.Add("timezonekey", "GST (UTC+4)");
                    oJsonObject.Add("meetingtype", "immediate");

                    MeetingResponce obj = ApiHelper.CreateMeeting(accessToken, oJsonObject);

                    if (obj.joinURL != "")
                    {
                        JoinUrl = obj.joinURL;
                        string StartMeetingURL = "https://api.getgo.com/G2M/rest/meetings/" + obj.meetingid + "/start";
                        string ReturnURLNew = ApiHelper.StartMeeting(accessToken, StartMeetingURL);
                        JoinUrl = ReturnURLNew;

                        GoToMeetingDetails objgotomeeting = new GoToMeetingDetails();
                        objgotomeeting.CourseID = Convert.ToInt32(courseID);
                        objgotomeeting.ScheduleID = Convert.ToInt32(scheduleID);
                        objgotomeeting.JoinURL = obj.joinURL;
                        objgotomeeting.StartMeetingURL = JoinUrl;
                        objgotomeeting.UniqueMeetingId = Convert.ToInt32(obj.meetingid);
                        objgotomeeting.ConferenceCallInfo = obj.conferenceCallInfo;
                        objgotomeeting.MaxParticipants = Convert.ToInt32(obj.maxParticipants);
                        objgotomeeting.IsActive = true;
                        objgotomeeting.IsDeleted = false;
                        objgotomeeting.CreatedBy = userId;
                        objgotomeeting.CreatedDate = DateTime.UtcNow;
                        objgotomeeting.ModifiedBy = userId;
                        objgotomeeting.ModifiedDate = DateTime.UtcNow;

                        this._db.GoToMeetingDetails.Add(objgotomeeting);
                        this._db.SaveChanges();
                    }

                    else
                    {
                        JoinUrl = "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return JoinUrl;
        }

        public async Task<List<TypeAhead>> GetAcademyTypeAhead(string trainerType, string search = null)
        {
            try
            {
                var Query = this._db.AcademyAgencyMaster.Where(a => a.IsActive == true && a.IsDeleted == false && (a.TrainerType.ToLower() == trainerType.ToLower()));
                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(a => a.AcademyAgencyName.StartsWith(search.ToLower()));
                }
                return await Query.Select(a => new TypeAhead
                {
                    Id = a.Id,
                    Title = a.AcademyAgencyName
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<TypeAhead>> GetAcademyData(string trainerType)
        {
            try
            {
                var Query = this._db.AcademyAgencyMaster.Where(a => a.IsActive == true && a.IsDeleted == false && (a.TrainerType.ToLower() == trainerType.ToLower()));

                return await Query.Select(a => new TypeAhead
                {
                    Id = a.Id,
                    Title = a.AcademyAgencyName
                }).OrderBy(a => a.Title).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<ScheduleCode> GetScheduleCode(int UserId)
        {
            int? ScheduleId = _db.ScheduleCode.Max(u => (int?)u.Id);
            if (ScheduleId != null)
            {
                ScheduleCode scheduleCode = await _db.ScheduleCode.Where(f => f.Id == ScheduleId).FirstOrDefaultAsync();
                if (scheduleCode != null)
                {
                    if (scheduleCode.IsDeleted == true && scheduleCode.UserId == UserId)
                    {
                        string scCode = "SC" + Convert.ToString(scheduleCode.Id);
                        ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(f => f.ScheduleCode == scCode).FirstOrDefaultAsync();
                        if (iLTSchedule != null)
                        {
                            ScheduleCode scheduleCode1 = new ScheduleCode();
                            scheduleCode1.IsDeleted = false;
                            scheduleCode1.UserId = UserId;
                            _db.ScheduleCode.Add(scheduleCode1);
                            await _db.SaveChangesAsync();
                            return scheduleCode1;
                        }
                        else
                        {
                            return scheduleCode;
                        }
                    }
                    else
                    {
                        ScheduleCode ScheduleCode = new ScheduleCode();
                        ScheduleCode.IsDeleted = false;
                        ScheduleCode.UserId = UserId;
                        _db.ScheduleCode.Add(ScheduleCode);
                        await _db.SaveChangesAsync();
                        return ScheduleCode;
                    }
                }
                else
                {
                    ScheduleCode ScheduleCode = new ScheduleCode();
                    ScheduleCode.IsDeleted = false;
                    ScheduleCode.UserId = UserId;
                    _db.ScheduleCode.Add(ScheduleCode);
                    await _db.SaveChangesAsync();
                    return ScheduleCode;
                }
            }
            else
            {
                ScheduleCode ScheduleCode = new ScheduleCode();
                ScheduleCode.IsDeleted = false;
                ScheduleCode.UserId = UserId;
                _db.ScheduleCode.Add(ScheduleCode);
                await _db.SaveChangesAsync();
                return ScheduleCode;
            }

            /*ScheduleCode ScheduleCode = new ScheduleCode();
            _db.ScheduleCode.Add(ScheduleCode);
            await _db.SaveChangesAsync();
            return ScheduleCode;*/
        }

        public async Task CancelScheduleCode(APIScheduleCode aPIScheduleCode, int UserId)
        {
            string Code = aPIScheduleCode.ScheduleCode.Replace("SC", "");
            int ScheduleId = Convert.ToInt32(Code);
            ScheduleCode scheduleCode = await _db.ScheduleCode.Where(f => f.Id == ScheduleId).FirstOrDefaultAsync();
            scheduleCode.IsDeleted = true;
            scheduleCode.UserId = UserId;
            _db.ScheduleCode.Update(scheduleCode);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckModuleName(string moduleName)
        {
            Module Name = new Module();
            Name = await this._db.Module.Where(m => m.ModuleType == "Classroom" && m.Name == moduleName && m.IsActive == Record.Active && m.IsDeleted == Record.NotActive).FirstOrDefaultAsync();
            if (Name == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> CourseEnrolledByTNA(int? courseId)
        {
            CourseRequest courseRequest = await this._db.CourseRequest.Where(m => m.CourseID == courseId && m.IsActive == Record.Active && m.IsDeleted == Record.NotActive && m.Status == "Enrolled").FirstOrDefaultAsync();
            if (courseRequest == null)
            {
                return false;
            }
            return true;
        }

        public async Task<int> CancellationSchedule(int scheduleId, string reason, string OrganisationCode)
        {
            ILTSchedule newILTSchedule = new ILTSchedule();
            ILTSchedule ILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == scheduleId && a.IsActive == Record.Active && a.IsDeleted == false).FirstOrDefaultAsync();
            if (ILTSchedule != null)
            {
                ILTSchedule.IsDeleted = true;
                ILTSchedule.IsActive = false;
                ILTSchedule.ScheduleType = "Cancelled";
                ILTSchedule.ReasonForCancellation = reason;
                await this.Update(ILTSchedule);

                List<TrainingNomination> trainingNominations = await _db.TrainingNomination.Where(s => s.ScheduleID == scheduleId && s.IsActive == true && s.IsDeleted == false
                                                                    && s.IsActiveNomination == true).ToListAsync();
                foreach (TrainingNomination item in trainingNominations)
                    item.IsActiveNomination = false;

                _db.TrainingNomination.UpdateRange(trainingNominations);
                await _db.SaveChangesAsync();

                var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");
                bool IsSendSMSToUser = false;
                string urlSMS = string.Empty;
                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    IsSendSMSToUser = true;
                    urlSMS = _configuration[Configuration.NotificationApi];
                }

                string token = _identitySv.GetToken();
                List<EnrollmentSchedular> enrollmentSchedulars = await GetNotificationForUsersOnScheduleCancellation(OrganisationCode, scheduleId);

                int count = 0;
                List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                List<APINominateUserSMS> SMSList = new List<APINominateUserSMS>();
                foreach (EnrollmentSchedular obj in enrollmentSchedulars)
                {
                    int UserIDToSend = Convert.ToInt32(obj.UserId);
                    string Message = "The Schedule '{ScheduleCode}' having module name '{ModuleName}' and start date {StartDate} has been cancelled for reason {Reason}.";
                    Message = Message.Replace("{ScheduleCode}", obj.ScheduleCode);
                    Message = Message.Replace("{ModuleName}", obj.ModuleName);
                    Message = Message.Replace("{StartDate}", obj.StartDate.ToString("dd MMM, yyyy"));
                    Message = Message.Replace("{Reason}", obj.Reason);

                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = "Schedule cancellation";
                    Notification.Message = Message;
                    Notification.Url = TlsUrl.NotificationAPost + obj.CourseId;
                    Notification.Type = Record.Enrollment1;
                    Notification.UserId = obj.UserId;
                    lstApiNotification.Add(Notification);
                    count++;
                    if (count % Constants.BATCH_SIZE == 0)
                    {
                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                        lstApiNotification.Clear();
                    }

                    //Prepare SMS List                
                    if (IsSendSMSToUser)
                    {
                        APINominateUserSMS objSMS = new APINominateUserSMS();
                        objSMS.CourseTitle = obj.CourseTitle;
                        objSMS.UserName = obj.UserName;
                        objSMS.StartDate = obj.StartDate;
                        objSMS.MobileNumber = !string.IsNullOrEmpty(obj.MobileNumber) ? Security.Decrypt(obj.MobileNumber) : null;
                        objSMS.organizationCode = OrganisationCode;
                        objSMS.UserID = obj.UserId;
                        SMSList.Add(objSMS);
                    }
                }
                if (lstApiNotification.Count > 0)
                {
                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                    lstApiNotification.Clear();
                }

                Task.Run(() => _email.SendScheduleCancellationEmail(OrganisationCode, scheduleId));

                if (string.Equals(OrganisationCode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (IsSendSMSToUser)
                        await this._email.SendScheduleCancellationNotificationSMS(SMSList);
                }

                // ------ Add ons Webinar type code ----//          
                var ModuleType = _db.Module.Where(a => a.Id == ILTSchedule.ModuleId).Select(c => c.ModuleType).FirstOrDefault();
                if (ModuleType == "vilt")
                {
                    if (ILTSchedule.WebinarType.ToLower() == "zoom")
                    {
                        ZoomMeetingDetails zoomMeetingDetails = this._db.ZoomMeetingDetails.Where(a => a.ScheduleID == ILTSchedule.ID).FirstOrDefault();  //&& a.Start_url == aPIILTSchedular.zoomScheduleDetails.start_url for manual entry of link

                        if (zoomMeetingDetails != null)
                        {
                            APIZoomDetailsToken obj = new APIZoomDetailsToken();
                            string Token = this.ZoomToken();
                            obj.access_token = Token;
                            int i = await ApiHelper.DeleteZoomMeetingResponce(zoomMeetingDetails.UniqueMeetingId, Token);
                        }
                    }
                    else if (ILTSchedule.WebinarType.ToLower() == "teams")
                    {
                        TeamsScheduleDetails teamdetails = await this.CancleTeamsMeeting(ILTSchedule.ID, ILTSchedule.ModifiedBy, OrganisationCode);
                    }
                    else if (ILTSchedule.WebinarType.ToLower() == "googlemeet")
                    {
                        GoogleMeetRessponce teamdetails = await this.CancleGsuitMeeting(ILTSchedule.ID, ILTSchedule.ModifiedBy, OrganisationCode);
                    }
                }
                // ------ Add ons Webinar type code  end ----//

                return 1;
            }
            return 0;
        }
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
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

        public async Task<List<EnrollmentSchedular>> GetNotificationForUsersOnScheduleCancellation(string orgCode, int scheduleId)
        {
            List<EnrollmentSchedular> Emails = new List<EnrollmentSchedular>();
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@OrgCode", orgCode);
                        parameters.Add("@ScheduleId", scheduleId);

                        var Result = await SqlMapper.QueryAsync<EnrollmentSchedular>((SqlConnection)connection, "[dbo].[GetUserDataForCancelledSchedule]", parameters, null, null, CommandType.StoredProcedure);
                        Emails = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function GetNotificationForUsersOnScheduleCancellation :-" + Utilities.GetDetailedException(ex));
                throw;
            }
            return Emails;
        }

        public async Task<string> SaveImage(IFormFile uploadedFile, int UserId)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string Path = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            Path = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, Record.ILTLogo);
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            fileName = System.IO.Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
            fileName = string.Concat(fileName.Split(' '));

            Path = System.IO.Path.Combine(Path, fileName);
            Path = string.Concat(Path.Split(' '));
            await SaveFile(uploadedFile, Path);

            string DomainName = request.Host.ToString();
            if (request.IsHttps)
                DomainName = string.Concat("https://", DomainName);
            else
                DomainName = string.Concat("http://", DomainName);
            string FPath = new System.Uri(Path).AbsoluteUri;
            string filePathLogo = string.Concat(string.Concat(DomainName, '/'), FPath.Substring(FPath.LastIndexOf(Record.ILTLogo)));
            return filePathLogo;
        }

        public async Task<bool> SaveFile(IFormFile uploadedFile, string filePath)
        {
            try
            {
                using (var fs = new FileStream(Path.Combine(filePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        public async Task<int> SendScheduleCreationBellNotification(int? courseId, int scheduleid, string token, string organizationcode)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Message = "One of your assigned courses, '{scheduleCode}' has been scheduled. Please visit the “My Assigned Courses” section and enroll.";
            Notification.Url = TlsUrl.NotificationAPost + courseId;
            await this._notification.SendScheduleCreationNotification(Notification, token, organizationcode);
            return 1;
        }

        public async Task<int> ScheduleRequestNotificationTo_Common(int courseId, int ScheduleId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            Notification.Url = TlsUrl.NotificationAPost + courseId;
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;

        }

        public async Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> Notification, string token)
        {
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;

        }

        public async Task<bool> AddCalendarData(string summary, string location, DateTime startDate, DateTime endDate, string eamilID)
        {

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                          new ClientSecrets
                          {
                              //appsetting
                              ClientId = this._configuration["ClientId"],
                              ClientSecret = this._configuration["ClientSecret"],
                          },
                          new[] { CalendarService.Scope.Calendar }, "user", CancellationToken.None).Result;

            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });

            Event myEvent = new Event
            {
                Summary = summary,
                Location = location,
                Start = new EventDateTime()
                {

                    DateTime = startDate,
                    TimeZone = "America/Los_Angeles"
                },
                End = new EventDateTime()
                {
                    DateTime = endDate,
                    TimeZone = "America/Los_Angeles"
                },
                Recurrence = new String[] { "RRULE:FREQ=WEEKLY;BYDAY=MO" },
                Attendees = new List<EventAttendee>() { new EventAttendee() { Email = eamilID } }
            };

            Event recurringEvent = service.Events.Insert(myEvent, "primary").Execute();
            return true;
        }


        public async Task<bool> AddCalendarEvent(string summary, string location, DateTime startDate, DateTime endDate, string eamilID)

        {
            UserCredential credential;
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, "credentials.json");

            using (var stream =
                        new FileStream(sWebRootFolder, FileMode.Open, FileAccess.ReadWrite))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });

            Event myEvent = new Event
            {
                Summary = summary,
                Location = location,
                Start = new EventDateTime()
                {

                    DateTime = startDate,
                    TimeZone = "America/Los_Angeles"
                },
                End = new EventDateTime()
                {
                    DateTime = endDate,
                    TimeZone = "America/Los_Angeles"
                },
                Recurrence = new String[] { "RRULE:FREQ=WEEKLY;BYDAY=MO" },
                Attendees = new List<EventAttendee>() { new EventAttendee() { Email = eamilID } }
            };

            Event recurringEvent = service.Events.Insert(myEvent, "primary").Execute();
            return true;
        }

        public async Task<List<APIILTSchedular>> GetAllSchedules()
        {
            var Query = (from ILTSchedule in this._db.ILTSchedule
                         join TrainingPlace in this._db.TrainingPlace on ILTSchedule.PlaceID equals TrainingPlace.Id into tempPlace
                         from TrainingPlace in tempPlace.DefaultIfEmpty()
                         join Module in this._db.Module on ILTSchedule.ModuleId equals Module.Id into tempModule
                         from Module in tempModule.DefaultIfEmpty()
                         join AcademyAgency in this._db.AcademyAgencyMaster on ILTSchedule.AcademyAgencyID equals AcademyAgency.Id into tempAcademyAgency
                         from AcademyAgency in tempAcademyAgency.DefaultIfEmpty()
                         join TrainerName in this._db.ILTScheduleTrainerBindings on ILTSchedule.ID equals TrainerName.ScheduleID into tempAcademyTrainer
                         from TrainerName in tempAcademyTrainer.DefaultIfEmpty()
                         join batch in this._db.ILTBatch on ILTSchedule.BatchId equals batch.Id into tmpBatch
                         from batch in tmpBatch.DefaultIfEmpty()
                         select new APIILTSchedular
                         {
                             ID = ILTSchedule.ID,
                             ScheduleCode = ILTSchedule.ScheduleCode,
                             ModuleId = ILTSchedule.ModuleId,
                             StartDate = ILTSchedule.StartDate,
                             EndDate = ILTSchedule.EndDate,
                             StartTime = ILTSchedule.StartTime.ToString(),
                             EndTime = ILTSchedule.EndTime.ToString(),
                             RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                             IsActive = ILTSchedule.IsActive,
                             PlaceID = ILTSchedule.PlaceID,
                             TrainerType = TrainerName.TrainerType,
                             PlaceName = TrainingPlace.PlaceName,
                             ModuleName = Module.Name,
                             City = TrainingPlace.Cityname,
                             ContactPersonName = TrainingPlace.ContactPerson,
                             AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                             AcademyAgencyName = AcademyAgency.AcademyAgencyName,
                             AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                             AcademyTrainerName = TrainerName.TrainerName,
                             TrainerDescription = ILTSchedule.TrainerDescription,
                             ScheduleType = ILTSchedule.ScheduleType,
                             ReasonForCancellation = ILTSchedule.ReasonForCancellation,
                             Status = ILTSchedule.IsActive,
                             EventLogo = ILTSchedule.EventLogo,
                             Cost = ILTSchedule.Cost,
                             Currency = ILTSchedule.Currency,
                             BatchId = batch.Id,
                             BatchCode = batch.BatchCode,
                             BatchName = batch.BatchName
                         });

            Query = Query.OrderByDescending(schedule => schedule.ID);

            return await Query.ToListAsync();
        }

        public async Task<List<APIILTSchedularExport>> GetAllSchedulesForExport(int UserId, string OrganizationCode, bool showAllData = false)
        {
            List<APIILTSchedularExport> apiIILTSchedularList = new List<APIILTSchedularExport>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        APIILTSchedularExport apiIILTSchedular = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetOrganizationILTSchedules";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add(new SqlParameter("@OrganizationCode", SqlDbType.NVarChar) { Value = OrganizationCode });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = showAllData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    apiIILTSchedular = new APIILTSchedularExport
                                    {
                                        ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString()),
                                        CourseName = row["Title"].ToString(),
                                        CourseCode = row["Code"].ToString(),
                                        ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString()),
                                        ScheduleCode = row["ScheduleCode"].ToString(),
                                        PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString()),
                                        ConductedBy = row["ConductedBy"].ToString(),
                                        TrainerType = row["TrainerType"].ToString(),
                                        PlaceName = row["PlaceName"].ToString(),
                                        ModuleName = row["ModuleName"].ToString(),
                                        StartDate = Convert.ToDateTime(row["StartDate"]),
                                        EndDate = Convert.ToDateTime(row["EndtDate"]),
                                        StartTime = (row["StartTime"].ToString()),
                                        EndTime = (row["EndTime"].ToString()),
                                        City = row["City"].ToString(),
                                        ContactPersonName = row["ContactPersonName"].ToString(),
                                        AcademyTrainerID = Security.Decrypt(row["AcademyTrainerID"].ToString()),
                                        AcademyTrainerName = row["AcademyTrainerName"].ToString(),
                                        TrainerDescription = row["TrainerDescription"].ToString(),
                                        ScheduleType = row["ScheduleType"].ToString(),
                                        ReasonForCancellation = row["ReasonForCancellation"].ToString(),
                                        Cost = string.IsNullOrEmpty(row["Cost"].ToString()) ? 0 : int.Parse(row["Cost"].ToString()),
                                        Currency = row["Currency"].ToString(),
                                        Region = row["Region"].ToString(),
                                        TopicName = row["TopicName"].ToString(),
                                        DepartmentOfTrainer = row["DepartmentOfTrainer"].ToString(),
                                        SubFunctionOfTrainer = row["SubFunctionOfTrainer"].ToString(),
                                        BatchId = string.IsNullOrEmpty(Convert.ToString(row["BatchId"])) ? 0 : Convert.ToInt32(row["BatchId"].ToString()),
                                        BatchCode = string.IsNullOrEmpty(Convert.ToString(row["BatchCode"])) ? null : Convert.ToString(row["BatchCode"]),
                                        BatchName = string.IsNullOrEmpty(Convert.ToString(row["BatchName"])) ? null : Convert.ToString(row["BatchName"]),
                                        NominationCount = row["NominationCount"].ToString(),
                                        LastModified = row["LastModified"].ToString()

                                    };
                                    apiIILTSchedularList.Add(apiIILTSchedular);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return apiIILTSchedularList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool> AddUserAsTrainer(APIUserAsTrainer aPIUserAsTrainer, int UserId)
        {
            try
            {
                ILTScheduleTrainerBindings iLTScheduleTrainerBindings = new ILTScheduleTrainerBindings();
                ILTScheduleTrainerBindings iLTScheduleTrainerBindingsPost = new ILTScheduleTrainerBindings();

                iLTScheduleTrainerBindings = await this._db.ILTScheduleTrainerBindings.Where(a => a.TrainerID == Convert.ToInt32(aPIUserAsTrainer.TrainerId)).FirstOrDefaultAsync();
                if (iLTScheduleTrainerBindings == null)
                {
                    iLTScheduleTrainerBindingsPost.ScheduleID = 0;
                    iLTScheduleTrainerBindingsPost.TrainerID = Convert.ToInt32(aPIUserAsTrainer.TrainerId);
                    iLTScheduleTrainerBindingsPost.TrainerName = aPIUserAsTrainer.UserName;
                    iLTScheduleTrainerBindingsPost.TrainerType = "Internal";
                    iLTScheduleTrainerBindingsPost.IsActive = true;
                    iLTScheduleTrainerBindingsPost.IsDeleted = false;
                    iLTScheduleTrainerBindingsPost.CreatedBy = UserId;
                    iLTScheduleTrainerBindingsPost.CreatedDate = DateTime.UtcNow;
                    iLTScheduleTrainerBindingsPost.ModifiedBy = UserId;
                    iLTScheduleTrainerBindingsPost.ModifiedDate = DateTime.UtcNow;

                    this._db.ILTScheduleTrainerBindings.Add(iLTScheduleTrainerBindingsPost);
                    await this._db.SaveChangesAsync();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APIUserAsTrainer>> GetUserAsTrainer(APIGetUserAsTrainer aPIGetUserAsTrainer)
        {
            List<APIUserAsTrainer> aPIUserAs = new List<APIUserAsTrainer>();
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
                            cmd.CommandText = "GetUserAsTrainer";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = aPIGetUserAsTrainer.search });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = aPIGetUserAsTrainer.page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = aPIGetUserAsTrainer.pageSize });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);


                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    APIUserAsTrainer aPI = new APIUserAsTrainer();
                                    aPI.TrainerId = row["TrainerID"].ToString();
                                    aPI.TrainerUserId = Security.Decrypt(row["UserId"].ToString());
                                    aPI.UserName = row["TrainerName"].ToString();
                                    aPIUserAs.Add(aPI);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return aPIUserAs;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetUserAsTrainerCount(APIGetUserAsTrainer aPIGetUserAsTrainer)
        {
            int Count = 0;
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
                            cmd.CommandText = "GetUserAsTrainerCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = aPIGetUserAsTrainer.search });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    Count = Convert.ToInt32(row["COUNT"].ToString());
                                }
                            }

                            reader.Dispose();
                        }
                        connection.Close();
                        return Count;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<TeamsAccessToken> addUpdateTeamsAccessToken(TeamsAccessToken teamsAccessToken)
        {
            try
            {
                TeamsAccessToken existingvalue = _db.TeamsAccessToken.FirstOrDefault();
                if (existingvalue != null)
                {
                    existingvalue.TeamsToken = teamsAccessToken.TeamsToken;
                    _db.TeamsAccessToken.Update(existingvalue);
                    await this._db.SaveChangesAsync();
                    return teamsAccessToken;
                }
                else
                {
                    _db.TeamsAccessToken.Add(teamsAccessToken);
                    await this._db.SaveChangesAsync();
                    return teamsAccessToken;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<Response> StartBigBlueMeeting(string meetingID, int userID, string userName)
        {
            BigBlueButtonResponse bigBlueButtonResponse;
            string meetingURL;
            string meetingResponse;
            try
            {
                var bbbMeetings = (from bbbMeeting in _db.BBBMeeting
                                   where bbbMeeting.MeetingID == meetingID && bbbMeeting.IsMeetingCreated == false
                                   select bbbMeeting).FirstOrDefault();
                if (bbbMeetings == null)
                    return new Response { StatusCode = HttpStatusCode.OK, Message = "Meeting does not exist with the given meeting id." };
                if (bbbMeetings.IsMeetingCreated == true)
                    return new Response { StatusCode = HttpStatusCode.OK, Message = "Meeting with the given meeting ID is already created in the past." };

                if (bbbMeetings.MeetingDate.Date == DateTime.Now.Date)
                {

                    int elapsedMinutesInDay = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalMinutes);
                    int meetingDuration = Convert.ToInt32(DateTime.Parse(bbbMeetings.MeetingTime).TimeOfDay.TotalMinutes + bbbMeetings.Duration);

                    if (meetingDuration >= elapsedMinutesInDay)
                    {
                        BBBMeeting meeting = new BBBMeeting();
                        meeting.AllowStartStopRecording = bbbMeetings.AllowStartStopRecording;
                        meeting.AutoStartRecording = bbbMeetings.AutoStartRecording;
                        meeting.Record = bbbMeetings.Record;
                        meeting.MeetingName = bbbMeetings.MeetingName;
                        meeting.RecordID = bbbMeetings.RecordID;
                        meeting.Duration = bbbMeetings.Duration;
                        meeting.MeetingID = bbbMeetings.MeetingID;

                        // Check if meeting is already running. If yes, then join that meeting.
                        meetingURL = new BigBlueButtonUtilities(_configuration).GetMeetingRunningURL(meeting);
                        meetingResponse = await new HttpHelpers().MakaHTTPGetCallAsync(meetingURL);
                        bigBlueButtonResponse = new BigBlueButtonUtilities(_configuration).FromXml<BigBlueButtonResponse>(meetingResponse);
                        if (bigBlueButtonResponse.Returncode.ToUpper() == "SUCCESS" && bigBlueButtonResponse.running.ToUpper() == "TRUE")
                        {
                            if (bbbMeetings.CreatedBy == userID)
                                return new Response { StatusCode = HttpStatusCode.OK, JoinMeetingUrl = new BigBlueButtonUtilities(_configuration).GetJoinMeetingURL(meeting, userName, true) };
                            else
                                return new Response { StatusCode = HttpStatusCode.OK, JoinMeetingUrl = new BigBlueButtonUtilities(_configuration).GetJoinMeetingURL(meeting, userName, false) };
                        }
                        else if (bbbMeetings.CreatedBy != userID)
                        {
                            return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = "The moderator has not yet started the meeting. Please try after some time." };
                        }

                        if (bbbMeetings.CreatedBy == userID)
                        {
                            //This user is the meeting creater and now the meeting should be created with the UserName and JoinURL should be returned

                            meetingURL = new BigBlueButtonUtilities(_configuration).GetCreateMeetingURL(meeting, userName);
                            meetingResponse = await new HttpHelpers().MakaHTTPGetCallAsync(meetingURL);
                            bigBlueButtonResponse = new BigBlueButtonUtilities(_configuration).FromXml<BigBlueButtonResponse>(meetingResponse);
                            if (bigBlueButtonResponse.Returncode.ToUpper() == "SUCCESS")
                                return new Response { StatusCode = HttpStatusCode.OK, JoinMeetingUrl = new BigBlueButtonUtilities(_configuration).GetJoinMeetingURL(meeting, userName, true) };
                            else
                                return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = bigBlueButtonResponse.Message };
                        }
                    }
                    else
                        return new Response { StatusCode = HttpStatusCode.OK, Message = "Either the meeting has finished or its scheduled for a later time." };
                }
                return new Response { StatusCode = HttpStatusCode.OK, Message = "The meeting is scheduled for a different date" };


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = "Some error occured while creating the meeting. Please try again." };
            }
        }

        public async Task<Response> EndBigBlueMeeting(string meetingID, int userID, string userName)
        {

            var bbbMeetings = (from bbbMeeting in _db.BBBMeeting
                               where bbbMeeting.MeetingID == meetingID
                               select bbbMeeting).FirstOrDefault();
            if (bbbMeetings == null)
                return new Response { StatusCode = HttpStatusCode.OK, Message = "Meeting does not exist with the given meeting id." };

            if (bbbMeetings.CreatedBy == userID)
            {
                //This user is the meeting creater and now the meeting should be created with the UserName and JoinURL should be returned
                BBBMeeting meeting = new BBBMeeting();
                meeting.MeetingID = bbbMeetings.MeetingID;
                string endmeetingURL = new BigBlueButtonUtilities(_configuration).GetEndMeetingURL(meeting);
                string meetingResponse = await new HttpHelpers().MakaHTTPGetCallAsync(endmeetingURL);
                BigBlueButtonResponse bigBlueButtonResponse = new BigBlueButtonUtilities(_configuration).FromXml<BigBlueButtonResponse>(meetingResponse);
                if (bigBlueButtonResponse.Returncode.ToUpper() == "SUCCESS")
                    return new Response { StatusCode = HttpStatusCode.OK, Message = "Meeting ended successfully." };
                else
                    return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = bigBlueButtonResponse.Message };
            }
            else
                return new Response { StatusCode = HttpStatusCode.OK, Message = "Meeting can be ended only by the meeting creater." };

        }


        //public async Task<Response> MarkBigBlueMeetingAttendance(string meetingID)
        //{
        //    try
        //    {
        //        string endmeetingURL = new BigBlueButtonUtilities(_configuration).GetMeetingInfoURL(meetingID);
        //        string meetingResponse = await new HttpHelpers().MakaHTTPGetCallAsync(endmeetingURL);
        //        BBBMeetingInfoResponse bigBlueButtonResponse = new BigBlueButtonUtilities(_configuration).FromXml<BBBMeetingInfoResponse>(meetingResponse);

        //        if (bigBlueButtonResponse.Returncode.ToUpper() == "SUCCESS")
        //        {
        //            foreach (var item in (IEnumerable<Attendees>)bigBlueButtonResponse.Attendees)
        //            { }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }
        //    return new Response { StatusCode = HttpStatusCode.OK, Message = "Attendance marked successfully." };
        //}
        public async Task<byte[]> ExportImportFormat(string OrgCode)
        {
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = FileName.ILTScheduleImportFormat;
            string DomainName = this._configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrgCode);
            string scheduleCodeEditable = await _courseRepository.GetConfigurationValueAsync("ASCFE", OrgCode);
            string EnableScheduleSeatCapacity = await _courseRepository.GetConfigurationValueAsync("SCHEDULE_SEATCAPACITY", OrgCode);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILTSchedule");
                if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                {
                    if (scheduleCodeEditable.ToLower() == "yes")
                    {
                        worksheet.Cells[1, 1].Value = "Schedule Code*";
                        worksheet.Cells[1, 2].Value = "Course Code*";
                        worksheet.Cells[1, 3].Value = "Batch Code*";
                        worksheet.Cells[1, 4].Value = "Module Name*";
                        worksheet.Cells[1, 5].Value = "Start Date*";
                        worksheet.Cells[1, 6].Value = "End Date*";
                        worksheet.Cells[1, 7].Value = "Start Time*";
                        worksheet.Cells[1, 8].Value = "End Time*";
                        worksheet.Cells[1, 9].Value = "Registration End Date*";
                        worksheet.Cells[1, 10].Value = "Trainer Type*";
                        worksheet.Cells[1, 11].Value = "Trainer Name*";
                        worksheet.Cells[1, 12].Value = "Training Place Type";
                        worksheet.Cells[1, 13].Value = "Academy Agency Name*";
                        worksheet.Cells[1, 14].Value = "Training Place Name";
                        worksheet.Cells[1, 15].Value = "Seat Capacity";
                        worksheet.Cells[1, 16].Value = "City";
                        worksheet.Cells[1, 17].Value = "Venue";
                        worksheet.Cells[1, 18].Value = "Coordinator Name";
                        worksheet.Cells[1, 19].Value = "Contact Number";
                        worksheet.Cells[1, 20].Value = "Currency";
                        worksheet.Cells[1, 21].Value = "Cost";
                        worksheet.Cells[1, 22].Value = "Vilt Type";
                        worksheet.Cells[1, 23].Value = "Vilt Credentials";
                        if (EnableScheduleSeatCapacity.ToLower() == "yes")
                        {
                            worksheet.Cells[1, 24].Value = "ScheduleCapacity";
                            using (var rngitems = worksheet.Cells["V1:V1"])//Applying Css for header
                            {
                                rngitems.Style.Font.Bold = true;
                            }
                        }
                        using (var rngitems = worksheet.Cells["A1:K1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }
                        using (var rngitems = worksheet.Cells["M1:M1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }

                        worksheet.Cells["E1:E2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["H1:H2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["I1:I2000"].Style.Numberformat.Format = "@";
                    }
                    else
                    {
                        worksheet.Cells[1, 1].Value = "Course Code*";
                        worksheet.Cells[1, 2].Value = "Batch Code*";
                        worksheet.Cells[1, 3].Value = "Module Name*";
                        worksheet.Cells[1, 4].Value = "Start Date*";
                        worksheet.Cells[1, 5].Value = "End Date*";
                        worksheet.Cells[1, 6].Value = "Start Time*";
                        worksheet.Cells[1, 7].Value = "End Time*";
                        worksheet.Cells[1, 8].Value = "Registration End Date*";
                        worksheet.Cells[1, 9].Value = "Trainer Type*";
                        worksheet.Cells[1, 10].Value = "Trainer Name*";
                        worksheet.Cells[1, 11].Value = "Training Place Type";
                        worksheet.Cells[1, 12].Value = "Academy Agency Name*";
                        worksheet.Cells[1, 13].Value = "Training Place Name";
                        worksheet.Cells[1, 14].Value = "Seat Capacity";
                        worksheet.Cells[1, 15].Value = "City";
                        worksheet.Cells[1, 16].Value = "Venue";
                        worksheet.Cells[1, 17].Value = "Coordinator Name";
                        worksheet.Cells[1, 18].Value = "Contact Number";
                        worksheet.Cells[1, 19].Value = "Currency";
                        worksheet.Cells[1, 20].Value = "Cost";
                        worksheet.Cells[1, 21].Value = "Vilt Type";
                        worksheet.Cells[1, 22].Value = "Vilt Credentials";
                        if (EnableScheduleSeatCapacity.ToLower() == "yes")
                        {
                            worksheet.Cells[1, 23].Value = "ScheduleCapacity";
                            using (var rngitems = worksheet.Cells["U1:U1"])//Applying Css for header
                            {
                                rngitems.Style.Font.Bold = true;
                            }
                        }
                        using (var rngitems = worksheet.Cells["A1:J1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }
                        using (var rngitems = worksheet.Cells["L1:L1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }

                        worksheet.Cells["D1:D2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["E1:E2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["H1:H2000"].Style.Numberformat.Format = "@";
                    }
                }
                else
                {
                    if (scheduleCodeEditable.ToLower() == "yes")
                    {
                        worksheet.Cells[1, 1].Value = "Schedule Code*";
                        worksheet.Cells[1, 2].Value = "Course Code*";
                        worksheet.Cells[1, 3].Value = "Module Name*";
                        worksheet.Cells[1, 4].Value = "Start Date*";
                        worksheet.Cells[1, 5].Value = "End Date*";
                        worksheet.Cells[1, 6].Value = "Start Time*";
                        worksheet.Cells[1, 7].Value = "End Time*";
                        worksheet.Cells[1, 8].Value = "Registration End Date*";
                        worksheet.Cells[1, 9].Value = "Trainer Type*";
                        worksheet.Cells[1, 10].Value = "Trainer Name*";
                        worksheet.Cells[1, 11].Value = "Training Place Type";
                        worksheet.Cells[1, 12].Value = "Academy Agency Name*";
                        worksheet.Cells[1, 13].Value = "Training Place Name";
                        worksheet.Cells[1, 14].Value = "Seat Capacity";
                        worksheet.Cells[1, 15].Value = "City";
                        worksheet.Cells[1, 16].Value = "Venue";
                        worksheet.Cells[1, 17].Value = "CoordinatorName";
                        worksheet.Cells[1, 18].Value = "ContactNumber";
                        worksheet.Cells[1, 19].Value = "Currency";
                        worksheet.Cells[1, 20].Value = "Cost";
                        worksheet.Cells[1, 21].Value = "Vilt Type";
                        worksheet.Cells[1, 22].Value = "Vilt Credentials";
                        if (EnableScheduleSeatCapacity.ToLower() == "yes")
                        {
                            worksheet.Cells[1, 23].Value = "ScheduleCapacity";
                            using (var rngitems = worksheet.Cells["U1:U1"])//Applying Css for header
                            {
                                rngitems.Style.Font.Bold = true;
                            }
                        }
                        using (var rngitems = worksheet.Cells["A1:J1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }
                        using (var rngitems = worksheet.Cells["L1:L1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }

                        worksheet.Cells["C1:C2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["D1:D2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["E1:E2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";
                    }
                    else
                    {
                        worksheet.Cells[1, 1].Value = "Course Code*";
                        worksheet.Cells[1, 2].Value = "Module Name*";
                        worksheet.Cells[1, 3].Value = "Start Date*";
                        worksheet.Cells[1, 4].Value = "End Date*";
                        worksheet.Cells[1, 5].Value = "Start Time*";
                        worksheet.Cells[1, 6].Value = "End Time*";
                        worksheet.Cells[1, 7].Value = "Registration End Date*";
                        worksheet.Cells[1, 8].Value = "Trainer Type*";
                        worksheet.Cells[1, 9].Value = "Trainer Name*";
                        worksheet.Cells[1, 10].Value = "Training Place Type";
                        worksheet.Cells[1, 11].Value = "Academy Agency Name*";
                        worksheet.Cells[1, 12].Value = "Training Place Name";
                        worksheet.Cells[1, 13].Value = "Seat Capacity";
                        worksheet.Cells[1, 14].Value = "City";
                        worksheet.Cells[1, 15].Value = "Venue";
                        worksheet.Cells[1, 16].Value = "CoordinatorName";
                        worksheet.Cells[1, 17].Value = "ContactNumber";
                        worksheet.Cells[1, 18].Value = "Currency";
                        worksheet.Cells[1, 19].Value = "Cost";
                        worksheet.Cells[1, 20].Value = "Vilt Type";
                        worksheet.Cells[1, 21].Value = "Vilt Credentials";
                        if (EnableScheduleSeatCapacity.ToLower() == "yes")
                        {
                            worksheet.Cells[1, 22].Value = "ScheduleCapacity";
                            using (var rngitems = worksheet.Cells["T1:T1"])//Applying Css for header
                            {
                                rngitems.Style.Font.Bold = true;
                            }
                        }
                        using (var rngitems = worksheet.Cells["A1:I1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }
                        using (var rngitems = worksheet.Cells["K1:K1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;
                        }

                        worksheet.Cells["D1:D2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["E1:E2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";
                        worksheet.Cells["H1:H2000"].Style.Numberformat.Format = "@";
                    }
                }
                package.Save();
            }
            var Fs = file.OpenRead();
            byte[] fileData = null;
            using (BinaryReader binaryReader = new BinaryReader(Fs))
            {
                fileData = binaryReader.ReadBytes((int)Fs.Length);
            }
            return fileData;
        }
        public async Task<ApiResponse> ProcessImportFile(APIILTScheduleImport aPIILTScheduleImport, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIILTScheduleImport.Path;

                DataTable scheduledt = ReadFile(filepath);
                if (scheduledt == null || scheduledt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrgCode);
                string scheduleCodeEditable = await _courseRepository.GetConfigurationValueAsync("ASCFE", OrgCode);
                string EnableScheduleSeatCapacity = await _courseRepository.GetConfigurationValueAsync("SCHEDULE_SEATCAPACITY", OrgCode);


                List<string> importcolumns = GetImportColumns(batchwiseNomination, scheduleCodeEditable, EnableScheduleSeatCapacity).Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(scheduledt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(scheduledt, UserId, OrgCode, batchwiseNomination, scheduleCodeEditable, EnableScheduleSeatCapacity);
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
        public async Task<bool> ValidateFileColumnHeaders(DataTable scheduledt, List<string> importColumns)
        {
            if (scheduledt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < scheduledt.Columns.Count; i++)
            {
                string col = scheduledt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                scheduledt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(scheduledt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }
        private List<KeyValuePair<string, int>> GetImportColumns(string batchwiseNomination, string scheduleCodeEditable, string EnableScheduleSeatCapacity, bool allcolumns = false)
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.CourseCode, 100));
            if (allcolumns || (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes"))
            {
                columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.BatchCode, 100));
            }
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.ModuleName, 600));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.StartDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.EndDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.StartTime, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.EndTime, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.RegistrationEndDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.TrainerType, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.TrainerName, 600));
            if (allcolumns)
                columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.TrainerNameEncrypted, 2000));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.TrainingPlaceType, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.AcademyAgencyName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.TrainingPlaceName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.SeatCapacity, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.City, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.Venue, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.CoordinatorName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.ContactNumber, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.Currency, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.Cost, 50));

            if (allcolumns || (!string.IsNullOrEmpty(EnableScheduleSeatCapacity) && EnableScheduleSeatCapacity.ToLower() == "yes"))
            {
                columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.ScheduleCapacity, 50));
            }
            if (allcolumns || (!string.IsNullOrEmpty(scheduleCodeEditable) && scheduleCodeEditable.ToLower() == "yes"))
            {
                columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.ScheduleCode, 100));
            }
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.vilt, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleImportColumns.viltcredential, 100));
            return columns;
        }
        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                    pck.Load(stream);
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    if (!dt.Columns.Contains(firstRowCell.Text))
                        dt.Columns.Add(firstRowCell.Text.Trim());
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    if (!string.IsNullOrEmpty(Convert.ToString(wsRow.ElementAtOrDefault(0))))
                    {
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            if (!string.IsNullOrEmpty(cell.Text))
                                row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    else
                        break;
                }
            }
            //check for empty rows
            DataTable validDt = new DataTable();
            validDt = dt.Clone();
            foreach (DataRow dataRow in dt.Rows)
            {
                bool IsEmpty = true;
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow[dataColumn])))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty)
                    validDt.ImportRow(dataRow);
            }

            return validDt;
        }
        public async Task<ApiResponse> ProcessRecordsAsync(DataTable scheduledt, int userId, string OrgCode, string batchwiseNomination, string scheduleCodeEditable, string EnableScheduleSeatCapacity)
        {
            ApiResponse response = new ApiResponse();
            var applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);

            if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "no" && !scheduledt.Columns.Contains("BatchCode"))
                scheduledt.Columns.Add("BatchCode", typeof(string));
            if (!string.IsNullOrEmpty(scheduleCodeEditable) && scheduleCodeEditable.ToLower() == "no" && !scheduledt.Columns.Contains("ScheduleCode"))
                scheduledt.Columns.Add("ScheduleCode", typeof(string));
            if (!string.IsNullOrEmpty(EnableScheduleSeatCapacity) && EnableScheduleSeatCapacity.ToLower() == "no" && !scheduledt.Columns.Contains("ScheduleCapacity"))
                scheduledt.Columns.Add("ScheduleCapacity", typeof(string));
            scheduledt.Columns.Add("TrainerNameEncrypted", typeof(string));
            scheduledt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = scheduledt.Columns;
            List<string> importcolumns = GetImportColumns(batchwiseNomination, scheduleCodeEditable, EnableScheduleSeatCapacity, true).Select(c => c.Key).ToList(); ;
            foreach (string column in importcolumns)
            {
                scheduledt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<APIILTScheduleRejected> aPIILTScheduleRejectedlist = new List<APIILTScheduleRejected>();
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns(batchwiseNomination, scheduleCodeEditable, EnableScheduleSeatCapacity, true);
            DataTable finalDt = scheduledt.Clone();
            if (scheduledt != null && scheduledt.Rows.Count > 0)
            {
                foreach (DataRow dataRow in scheduledt.Rows)
                {
                    APIILTScheduleRejected aPIILTScheduleRejected = new APIILTScheduleRejected();

                    bool IsError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "StartDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartDate = Convert.ToString(dataRow[column]);
                                string outStartDate = ValidateDate(StartDate, applicationDateFormat);
                                if (outStartDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outStartDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Date required.";
                            }
                        }
                        else if (string.Compare(column, "EndDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndDate = Convert.ToString(dataRow[column]);
                                string outEndDate = ValidateDate(EndDate, applicationDateFormat);
                                if (outEndDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outEndDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Date required.";
                            }
                        }
                        else if (string.Compare(column, "RegistrationEndDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndDate = Convert.ToString(dataRow[column]);
                                string outEndDate = ValidateDate(EndDate, applicationDateFormat);
                                if (outEndDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Registration End Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outEndDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Registration End Date required.";
                            }
                        }
                        else if (string.Compare(column, "StartTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartTime = Convert.ToString(dataRow[column]);
                                string outStartTime = ValidateTime(StartTime);
                                if (outStartTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Time is not in hh:mm format.";
                                }
                                else
                                    dataRow[column] = outStartTime;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Time required.";
                            }
                        }
                        else if (string.Compare(column, "EndTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndTime = Convert.ToString(dataRow[column]);
                                string outEndTime = ValidateTime(EndTime);
                                if (outEndTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Time is not in hh:mm format.";
                                }
                                else
                                    dataRow[column] = outEndTime;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Time required.";
                            }
                        }
                        else if (string.Compare(column, "SeatCapacity", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                int seatcapacity;
                                bool flag = int.TryParse(Convert.ToString(dataRow[column]), out seatcapacity);
                                if (flag)
                                    dataRow[column] = seatcapacity;
                                else
                                {
                                    IsError = true;
                                    errorMsg = "Invalid Seat Capacity.";
                                }
                            }
                        }
                        else if (string.Compare(column, "Cost", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                int cost;
                                bool flag = int.TryParse(Convert.ToString(dataRow[column]), out cost);
                                if (flag)
                                    dataRow[column] = cost;
                                else
                                {
                                    IsError = true;
                                    errorMsg = "Invalid Cost.";
                                }
                            }
                        }
                        else if (string.Compare(column, "TrainerName", true) == 0)
                            dataRow["TrainerNameEncrypted"] = Security.Encrypt(Convert.ToString(dataRow[column]));

                        if (!DBNull.Value.Equals(dataRow[column]))
                        {
                            int columnlength = columnlengths.Where(c => c.Key == column).Select(len => len.Value).FirstOrDefault();
                            if (columnlength < Convert.ToString(dataRow[column]).Length)
                            {
                                IsError = true;
                                errorMsg = "Invalid data in " + column + ". Must be less than equal to " + Convert.ToString(columnlength) + " characters.";
                                break;
                            }
                        }
                        if (IsError)
                            break;
                    }
                    if (IsError)
                    {
                        aPIILTScheduleRejected.CourseCode = dataRow["CourseCode"] != null ? Convert.ToString(dataRow["CourseCode"]) : null;
                        aPIILTScheduleRejected.BatchCode = dataRow["BatchCode"] != null ? Convert.ToString(dataRow["BatchCode"]) : null;
                        aPIILTScheduleRejected.ModuleName = dataRow["ModuleName"] != null ? Convert.ToString(dataRow["ModuleName"]) : null;
                        aPIILTScheduleRejected.StartDate = dataRow["StartDate"] != null ? Convert.ToString(dataRow["StartDate"]) : null;
                        aPIILTScheduleRejected.EndDate = dataRow["EndDate"] != null ? Convert.ToString(dataRow["EndDate"]) : null;
                        aPIILTScheduleRejected.StartTime = dataRow["StartTime"] != null ? Convert.ToString(dataRow["StartTime"]) : null;
                        aPIILTScheduleRejected.EndTime = dataRow["EndTime"] != null ? Convert.ToString(dataRow["EndTime"]) : null;
                        aPIILTScheduleRejected.RegistrationEndDate = dataRow["RegistrationEndDate"] != null ? Convert.ToString(dataRow["RegistrationEndDate"]) : null;
                        aPIILTScheduleRejected.TrainerType = dataRow["TrainerType"] != null ? Convert.ToString(dataRow["TrainerType"]) : null;
                        aPIILTScheduleRejected.TrainerName = dataRow["TrainerName"] != null ? Convert.ToString(dataRow["TrainerName"]) : null;
                        aPIILTScheduleRejected.TrainingPlaceType = dataRow["TrainingPlaceType"] != null ? Convert.ToString(dataRow["TrainingPlaceType"]) : null;
                        aPIILTScheduleRejected.AcademyAgencyName = dataRow["AcademyAgencyName"] != null ? Convert.ToString(dataRow["AcademyAgencyName"]) : null;
                        aPIILTScheduleRejected.TrainingPlaceName = dataRow["TrainingPlaceName"] != null ? Convert.ToString(dataRow["TrainingPlaceName"]) : null;
                        aPIILTScheduleRejected.SeatCapacity = dataRow["SeatCapacity"] != null ? Convert.ToString(dataRow["SeatCapacity"]) : null;
                        aPIILTScheduleRejected.City = dataRow["City"] != null ? Convert.ToString(dataRow["City"]) : null;
                        aPIILTScheduleRejected.Venue = dataRow["Venue"] != null ? Convert.ToString(dataRow["Venue"]) : null;
                        aPIILTScheduleRejected.CoordinatorName = dataRow["CoordinatorName"] != null ? Convert.ToString(dataRow["CoordinatorName"]) : null;
                        aPIILTScheduleRejected.ContactNumber = dataRow["ContactNumber"] != null ? Convert.ToString(dataRow["ContactNumber"]) : null;
                        aPIILTScheduleRejected.Currency = dataRow["Currency"] != null ? Convert.ToString(dataRow["Currency"]) : null;
                        aPIILTScheduleRejected.Cost = dataRow["Cost"] != null ? Convert.ToString(dataRow["Cost"]) : null;
                        aPIILTScheduleRejected.ScheduleCode = dataRow["ScheduleCode"] != null ? Convert.ToString(dataRow["ScheduleCode"]) : null;
                        aPIILTScheduleRejected.ScheduleCapacity = dataRow["ScheduleCapacity"] != null ? Convert.ToString(dataRow["ScheduleCapacity"]) : null;
                        aPIILTScheduleRejected.Status = Record.Rejected;
                        aPIILTScheduleRejected.ErrorMessage = errorMsg;
                        dataRow["ErrorMessage"] = aPIILTScheduleRejected.ErrorMessage;
                        aPIILTScheduleRejectedlist.Add(aPIILTScheduleRejected);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                        if (finalDt.Columns.Contains("ViltType"))
                        {
                            finalDt.Columns.Remove("ViltType");
                        }
                        if (finalDt.Columns.Contains("ViltCredentials"))
                        {
                            finalDt.Columns.Remove("ViltCredentials");
                        }
                    }
                }

                try
                {
                    DataTable dtResult = new DataTable();
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[ILTSchedule_BulkImport]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@ILTScheduleBulkImportType", SqlDbType.Structured) { Value = finalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }
                        connection.Close();
                    }
                    List<ILTScheduleRejected> iLTScheduleRejecteds = Mapper.Map<List<ILTScheduleRejected>>(aPIILTScheduleRejectedlist);
                    _db.ILTScheduleRejected.AddRange(iLTScheduleRejecteds);
                    await _db.SaveChangesAsync();
                    aPIILTScheduleRejectedlist.AddRange(dtResult.ConvertToList<APIILTScheduleRejected>());
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            try
            {
                foreach (APIILTScheduleRejected aPIILTScheduleRejected1 in aPIILTScheduleRejectedlist)
                {
                    if (aPIILTScheduleRejected1 != null)
                    {
                        if (aPIILTScheduleRejected1.ErrorMessage == null && aPIILTScheduleRejected1.CourseType.ToLower() == "vilt")
                        {
                            foreach (DataRow dataRow in scheduledt.Rows)
                            {
                                string code = dataRow["CourseCode"].ToString();
                                string module = dataRow["ModuleName"].ToString();
                                string startdate = Convert.ToDateTime(dataRow["StartDate"]).ToString("yyyy-MM-dd");
                                string enddate = Convert.ToDateTime(dataRow["EndDate"]).ToString("yyyy-MM-dd");
                                TimeSpan starttime = TimeSpan.Parse(dataRow["StartTime"].ToString());
                                TimeSpan endtime = TimeSpan.Parse(dataRow["EndTime"].ToString());

                                if (code == aPIILTScheduleRejected1.CourseCode &&
                                    module == aPIILTScheduleRejected1.ModuleName &&
                                    startdate == aPIILTScheduleRejected1.StartDate.Substring(0, 10) &&
                                    enddate == aPIILTScheduleRejected1.EndDate.Substring(0, 10)
                                    )
                                {

                                    Model.Course course = _db.Course.Where(a => a.Code == dataRow["CourseCode"].ToString()).FirstOrDefault();
                                    Module module1 = this._db.Module.Where(a => a.Name == module).FirstOrDefault();
                                    if (course != null && module1 != null)
                                    {
                                        ILTSchedule iLTSchedule = this._db.ILTSchedule.Where(a =>
                                        a.StartDate == Convert.ToDateTime(startdate) &&
                                        a.EndDate == Convert.ToDateTime(enddate) &&
                                        a.StartTime == starttime &&
                                        a.EndTime == endtime &&
                                        a.CourseId == course.Id &&
                                        a.ModuleId == module1.Id
                                        ).FirstOrDefault();

                                        Teams teams = new Teams();
                                        teams.StartDate = Convert.ToDateTime(dataRow["StartDate"].ToString());
                                        teams.EndDate = Convert.ToDateTime(dataRow["EndDate"].ToString());
                                        teams.CourseID = course.Id;
                                        teams.StartTime = Convert.ToString(dataRow["StartTime"]);
                                        teams.EndTime = Convert.ToString(dataRow["EndTime"]);
                                        teams.Username = Convert.ToString(dataRow["ViltCredentials"]);

                                        var Query =
                                            (from
                                             ilt in this._db.ILTSchedule
                                             join
                                             trainer in this._db.ILTScheduleTrainerBindings on ilt.ID equals trainer.ScheduleID
                                             join usermaster in this._db.UserMaster on
                                             trainer.TrainerID equals usermaster.Id
                                             where ilt.ID == iLTSchedule.ID
                                             select new UserMaster
                                             {
                                                 EmailId = usermaster.EmailId,
                                                 UserName = usermaster.UserName
                                             }
                                             );

                                        UserMaster userMaster = Query.FirstOrDefault();
                                        TrainersData trainersData = new TrainersData();
                                        trainersData.EmailName = userMaster.UserName;
                                        userMaster.EmailId = Security.Decrypt(userMaster.EmailId);
                                        userMaster.EmailId = Security.EncryptForUI(userMaster.EmailId);
                                        trainersData.EmailId = userMaster.EmailId;
                                        teams.TrinerData = new List<TrainersData>();
                                        teams.TrinerData.Add(trainersData);
                                        teams.CourseName = course.Title;

                                        if (iLTSchedule != null && dataRow["ViltType"].ToString().ToLower() == "teams")
                                        {

                                            teams.ScheduleID = iLTSchedule.ID;
                                            List<TeamsScheduleDetails> teamsScheduleDetails = await PostTeamsMeeting(teams, userId, OrgCode);
                                            if (teamsScheduleDetails != null)
                                            {

                                                iLTSchedule.WebinarType = "TEAMS";

                                                this._db.ILTSchedule.Update(iLTSchedule);
                                                this._db.SaveChanges();
                                            }
                                        }
                                        else if (iLTSchedule != null && dataRow["ViltType"].ToString().ToLower() == "zoom")
                                        {
                                            try
                                            {
                                                APIZoomDetailsToken obj = new APIZoomDetailsToken();
                                                string Token = this.ZoomToken();
                                                obj.access_token = Token;
                                                APIZoomMeetingResponce aPIZoomMeetingResponce = await CallZoomMeetings(obj, teams, userId);

                                                if (aPIZoomMeetingResponce != null)
                                                {

                                                    iLTSchedule.WebinarType = "zoom";

                                                    this._db.ILTSchedule.Update(iLTSchedule);
                                                    this._db.SaveChanges();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.Error(Utilities.GetDetailedException(ex));
                                            }
                                        }

                                    }

                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            int totalRecordInsert = aPIILTScheduleRejectedlist.Where(x => x.ErrorMessage == null).Count();
            int totalRecordRejected = aPIILTScheduleRejectedlist.Where(x => x.ErrorMessage != null).Count();
            string resultstring = "Total records inserted: " + totalRecordInsert + ", rejected: " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIILTScheduleRejectedlist = aPIILTScheduleRejectedlist.Where(x => x.ErrorMessage != null).ToList() };
            return response;
        }

        public string ValidateDate(string InputDate, string validDateFormat)
        {
            string outputDate = null;
            try
            {
                DateTime result;
                result = DateTime.ParseExact(InputDate, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                outputDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputDate;
        }
        public string ValidateTime(string InputTime)
        {
            string outputTime = null;
            try
            {
                DateTime dt;
                if (DateTime.TryParseExact(InputTime, "HH:mm", CultureInfo.InvariantCulture,
                                                              DateTimeStyles.None, out dt))
                {
                    outputTime = dt.TimeOfDay.ToString(@"hh\:mm");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputTime;
        }
        public async Task<List<APIILTScheduleRejected>> GetScheduleRejected()
        {
            try
            {
                IEnumerable<ILTScheduleRejected> iLTScheduleRejected = await _db.ILTScheduleRejected.ToListAsync();
                List<APIILTScheduleRejected> aPIILTScheduleRejected = Mapper.Map<List<APIILTScheduleRejected>>(iLTScheduleRejected);

                return aPIILTScheduleRejected;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<FileInfo> ExportILTScheduleReject(string OrgCode)
        {
            IEnumerable<APIILTScheduleRejected> aPIILTScheduleRejected = await GetScheduleRejected();
            FileInfo fileInfo = await GetILTScheduleReject(aPIILTScheduleRejected, OrgCode);
            return fileInfo;
        }

        private async Task<FileInfo> GetILTScheduleReject(IEnumerable<APIILTScheduleRejected> aPIILTScheduleRejecteds, string OrgCode)
        {
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();
            string applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
            string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrgCode);
            List<string> ExportHeader = GetILTScheduleRejectHeader(batchwiseNomination);
            ExportData.Add(RowNumber, ExportHeader);

            foreach (APIILTScheduleRejected aPIILTScheduleRejected in aPIILTScheduleRejecteds)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetILTScheduleRejectRowData(aPIILTScheduleRejected, applicationDateFormat, batchwiseNomination);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ILTScheduleRejected, ExportData);
            return fileInfo;
        }

        private List<string> GetILTScheduleRejectHeader(string batchwiseNomination)
        {
            List<string> ExportHeader = new List<string>();
            ExportHeader.Add(HeaderName.CourseName);
            if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                ExportHeader.Add(HeaderName.BatchCode);
            ExportHeader.AddRange(new List<string>()
            {
                HeaderName.ModuleName,
                HeaderName.StartDate,
                HeaderName.EndDate,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.RegistrationEndDate,
                HeaderName.TrainerType,
                HeaderName.TrainerName,
                HeaderName.TrainingPlaceType,
                HeaderName.AcademyAgencyName,
                HeaderName.TrainingPlaceName,
                HeaderName.SeatCapacity,
                HeaderName.City,
                HeaderName.Venue,
                HeaderName.CoordinatorName,
                HeaderName.ContactNumber,
                HeaderName.Currency,
                HeaderName.Cost,
                HeaderName.Status,
                HeaderName.ScheduleCode,
                HeaderName.ErrorMessage
            });
            return ExportHeader;
        }

        private List<string> GetILTScheduleRejectRowData(APIILTScheduleRejected aPIILTScheduleRejected, string applicationDateFormat, string batchwiseNomination)
        {
            List<string> ExportData = new List<string>();
            ExportData.Add(aPIILTScheduleRejected.CourseCode);
            if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                ExportData.Add(aPIILTScheduleRejected.BatchCode);
            ExportData.AddRange(new List<string>()
            {
                aPIILTScheduleRejected.ModuleName,
                !string.IsNullOrEmpty(aPIILTScheduleRejected.StartDate) ? Convert.ToDateTime(aPIILTScheduleRejected.StartDate).ToString(applicationDateFormat) : null,
                !string.IsNullOrEmpty(aPIILTScheduleRejected.EndDate) ? Convert.ToDateTime(aPIILTScheduleRejected.EndDate).ToString(applicationDateFormat) : null,
                TimeSpan.Parse(aPIILTScheduleRejected.StartTime).ToString(@"hh\:mm"),
                TimeSpan.Parse(aPIILTScheduleRejected.EndTime).ToString(@"hh\:mm"),
                !string.IsNullOrEmpty(aPIILTScheduleRejected.RegistrationEndDate) ? Convert.ToDateTime(aPIILTScheduleRejected.RegistrationEndDate).ToString(applicationDateFormat) : null,
                aPIILTScheduleRejected.TrainerType,
                aPIILTScheduleRejected.TrainerName,
                aPIILTScheduleRejected.TrainingPlaceType,
                aPIILTScheduleRejected.AcademyAgencyName,
                aPIILTScheduleRejected.TrainingPlaceName,
                aPIILTScheduleRejected.SeatCapacity,
                aPIILTScheduleRejected.City,
                aPIILTScheduleRejected.Venue,
                aPIILTScheduleRejected.CoordinatorName,
                aPIILTScheduleRejected.ContactNumber,
                aPIILTScheduleRejected.Currency,
                aPIILTScheduleRejected.Cost,
                aPIILTScheduleRejected.Status,
                aPIILTScheduleRejected.ScheduleCode,
                aPIILTScheduleRejected.ErrorMessage,
            });
            return ExportData;
        }
        public async Task<List<TeamsScheduleDetails>> PostTeamsMeeting(Teams teams, int userId, string organisationCode)
        {
            List<TeamsScheduleDetails> teamsScheduleDetails = new List<TeamsScheduleDetails>();
            ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
            if (withoutPassword == null)
            {
                return teamsScheduleDetails;
            }

            if (withoutPassword.Value.ToLower() == "yes")
            {
                teamsScheduleDetails = await _onlineWebinarRepository.PostTeamsMeeting(teams, userId);
            }
            else
            {
                IPublicClientApplication app = GetTeamsAuthentication();
                AuthenticationResult results = await GetTeamsToken(app, teams.Username, 0);
                try
                {

                    var objectID = results.UniqueId;
                    ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ONLINE_LOGIN").FirstOrDefault();
                    if (conferenceParameters.Value.ToLower() == "yes")
                    {
                        teamsScheduleDetails = await CallTeamsEventCalendars(results.AccessToken, teams, userId, objectID, null);
                    }
                    else
                    {
                        teamsScheduleDetails = await CallTeamsCalendars(results.AccessToken, teams, userId, objectID, null);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetails;
        }
        public async Task<TeamsScheduleDetails> UpdateTeamsMeeting(UpdateTeams teams, int userId, string organisationCode, int id, AuthenticationResult authenticationResult,HolidayList[] HolidayList)
        {
            ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
            UserWebinarMaster userWebinarMaster = this._db.UserWebinarMasters.Where(a => a.Id == teams.userWebinarId).FirstOrDefault();

            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            if (withoutPassword.Value.ToLower() == "yes")
            {
                var objectID = userWebinarMaster.TeamsEmail;
                Teams teams1 = new Teams();
                teams1.CourseID = teams.CourseID;
                teams1.CourseName = teams.CourseName;
                teams1.EndDate = teams.EndDate;
                teams1.EndTime = teams.EndTime;
                teams1.ScheduleID = teams.ScheduleID;
                teams1.StartDate = teams.StartDate;
                teams1.StartTime = teams.StartTime;
                teams1.Username = userWebinarMaster.TeamsEmail;
                teams1.HolidayList = HolidayList;
                teamsScheduleDetails = await _onlineWebinarRepository.UpdateTeamsMeeting(userId, teams1, userWebinarMaster, id, authenticationResult);
            }
            else
            {
                IPublicClientApplication app = GetTeamsAuthentication();

                AuthenticationResult results = await GetTeamsToken(app, userWebinarMaster.TeamsEmail, 0);
                try
                {

                    var objectID = results.UniqueId;
                    Teams teams1 = new Teams();
                    teams1.CourseID = teams.CourseID;
                    teams1.CourseName = teams.CourseName;
                    teams1.EndDate = teams.EndDate;
                    teams1.EndTime = teams.EndTime;
                    teams1.ScheduleID = teams.ScheduleID;
                    teams1.StartDate = teams.StartDate;
                    teams1.StartTime = teams.StartTime;
                    teams1.Username = userWebinarMaster.TeamsEmail;
                    ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ONLINE_LOGIN").FirstOrDefault();
                    if (conferenceParameters.Value.ToLower() == "yes")
                    {
                        teamsScheduleDetails = await CallTeamsEventCalendarsV2(results.AccessToken, teams1, userId, objectID, teams.MeetingId);
                    }
                    else
                    {
                        teamsScheduleDetails = await CallTeamsCalendarsV2(results.AccessToken, teams1, userId, objectID, teams.MeetingId);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetails;
        }
        public async Task<List<TeamsScheduleDetails>> CancelRemoveDatesSchedule(List<UpdateTeams> teams,int userid)
        {
            if(teams.Count() > 0)
            {
                List<TeamsScheduleDetails> teamsScheduleDetails3 = this._db.TeamsScheduleDetails.Where(a => a.ScheduleID == teams[0].ScheduleID
                && a.IsDeleted == false && a.IsActive == true
                ).ToList();
                foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetails3)
                {
                    UpdateTeams updateTeams = teams.Where(a => a.MeetingId == teamsScheduleDetails.MeetingId || (a.id == -1 && a.MeetingId == "meetingid")).FirstOrDefault();
                    if (updateTeams == null)
                    {
                        UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                        TeamsScheduleDetails teamsScheduleDetails1 = await _onlineWebinarRepository.cancleTeamsMeeting(userid, teamsScheduleDetails.ScheduleID, webinarMaster, teamsScheduleDetails);
                        teamsScheduleDetails.IsDeleted = teamsScheduleDetails1.IsDeleted;
                        teamsScheduleDetails.IsActive = teamsScheduleDetails1.IsActive;

                        this._db.TeamsScheduleDetails.Update(teamsScheduleDetails);
                        this._db.SaveChanges();
                    }
                }
                teamsScheduleDetails3 = this._db.TeamsScheduleDetails.Where(a => a.ScheduleID == teams[0].ScheduleID
             && a.IsDeleted == false && a.IsActive == true
             ).ToList();
                return teamsScheduleDetails3;
            }
            return null;
        }
    public async Task<TeamsScheduleDetails> CancleTeamsMeeting(int ScheduleID, int userId, string organisationCode)
        {
            ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            List<TeamsScheduleDetails> teamsScheduleDetails1 = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == ScheduleID).ToList();
            if (teamsScheduleDetails1 != null)
            {
                foreach(TeamsScheduleDetails teamsScheduleDetails2 in teamsScheduleDetails1)
                {
                    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails2.UserWebinarId).FirstOrDefault();

                    if (withoutPassword.Value.ToLower() == "yes")
                    {

                        teamsScheduleDetails = await _onlineWebinarRepository.cancleTeamsMeeting(userId, ScheduleID, userWebinarMaster, teamsScheduleDetails2);
                    }
                    else
                    {
                        IPublicClientApplication app = GetTeamsAuthentication();

                        AuthenticationResult results = await GetTeamsToken(app, userWebinarMaster.TeamsEmail, 0);
                        try
                        {
                            ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ONLINE_LOGIN").FirstOrDefault();
                            if (conferenceParameters.Value.ToLower() == "yes")
                            {
                                teamsScheduleDetails = await CallTeamsCancleEventCalendars(results.AccessToken, ScheduleID, userId, teamsScheduleDetails2, userWebinarMaster);
                            }


                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
            }
            return teamsScheduleDetails;
        }

        public async Task<GoogleMeetRessponce> CancleGsuitMeeting(int ScheduleID, int userId, string organisationCode)
        {

            GoogleMeetRessponce resp = new GoogleMeetRessponce();
            GoogleMeetDetails meetScheduleDetails1 = _db.GoogleMeetDetails.Where(a => a.ScheduleID == ScheduleID).FirstOrDefault();
            if (meetScheduleDetails1 != null)
            {
                UserWebinarMaster userWebinarMaster = this._db.UserWebinarMasters.Where(a => a.Id == meetScheduleDetails1.UserWebinarId).FirstOrDefault();

                if (userWebinarMaster != null)
                {

                    resp = await _onlineWebinarRepository.cancleGsuitMeeting(userId,  userWebinarMaster, meetScheduleDetails1);
                }
               
            }
            return resp;
        }

        public async Task<GoogleMeetRessponce> PostGsuitMeet(MeetDetails teams, int userId, string organisationCode)
        {
            GoogleMeetRessponce googleMeetDetails = new GoogleMeetRessponce();
            try
            {
                googleMeetDetails = await _onlineWebinarRepository.CallGSuitEventCalendars(teams, userId);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            
            return googleMeetDetails;
        }
        public async Task<GoogleMeetRessponce> UpdateGsuitMeet(UpdateGsuitMeeting meet, int userId, string organisationCode)
        {
            GoogleMeetRessponce googleMeetDetails = new GoogleMeetRessponce();
            try
            {
                googleMeetDetails = await _onlineWebinarRepository.UpdateGSuitEventCalendars(meet, userId);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return googleMeetDetails;
        }
        public IPublicClientApplication GetTeamsAuthentication()
        {
            ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();
            string authority = configurableValues.Authority;
            IPublicClientApplication app;
            app = PublicClientApplicationBuilder.Create(configurableValues.ConfigurableParameter1)
                  .WithAuthority(authority)
                  .Build();
            return app;
        }
        public async Task<AuthenticationResult> GetTeamsToken(IPublicClientApplication app,string Username,int meeting)
        { 
            AuthenticationResult results = null;
            string[] scopes = new string[] { "https://graph.microsoft.com/Calendars.ReadWrite",
                                                             "https://graph.microsoft.com/OnlineMeetings.ReadWrite",
                                                             "https://graph.microsoft.com/openid",
                                                             "https://graph.microsoft.com/profile",
                                                             "https://graph.microsoft.com/User.Read",
                                                              "https://graph.microsoft.com/email",
                                                             "https://graph.microsoft.com/OnlineMeetings.Read",
                                                             "https://graph.microsoft.com/OnlineMeetingArtifact.Read.All"
                                            };
            var securePassword = new SecureString();
            if (meeting == 0)
            {
                var Query = await (from UserWebinarMasters in this._db.UserWebinarMasters
                        where UserWebinarMasters.isDeleted == 0 && UserWebinarMasters.TeamsEmail == Username
                        select new { UserWebinarMasters.Username, UserWebinarMasters.Password }).FirstOrDefaultAsync();
                var Password = Security.Decrypt(Query.Password);
                foreach (char c in Password)
                    securePassword.AppendChar(c);

                try
                {
                    results = await app.AcquireTokenByUsernamePassword(scopes, Username, securePassword).ExecuteAsync();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                    return null;
                }
            }
            else
            {
                var Query = await (from UserWebinarMasters in this._db.UserWebinarMasters
                where UserWebinarMasters.TeamsEmail == Username
                select new { UserWebinarMasters.Username, UserWebinarMasters.Password }).FirstOrDefaultAsync();
                
                var Password = Security.Decrypt(Query.Password);
                foreach (char c in Password)
                    securePassword.AppendChar(c);
                try
                {
                    results = await app.AcquireTokenByUsernamePassword(scopes, Username, securePassword).ExecuteAsync();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                    return null;
                }
            }
            return results;
        }
        public async Task<HttpResponseMessage> GetTeamsMeeting(string JoinUrl)
        {
            try
            {
                
                var Query = await (from TeamsScheduleDetails in this._db.TeamsScheduleDetails
                                   where TeamsScheduleDetails.JoinUrl == JoinUrl 
                                   select new { TeamsScheduleDetails.MeetingId,TeamsScheduleDetails.UserWebinarId }).Distinct().ToListAsync();

                TeamsScheduleDetails teamsScheduleDetails = _db.TeamsScheduleDetails.Where(a => a.JoinUrl == JoinUrl).FirstOrDefault();
                UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();

                HttpResponseMessage response = await CallGetAPIForTeam(teamsScheduleDetails, webinarMaster);
                return response;
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
            }
        }
        private async Task<List<TeamsScheduleDetails>> CallTeamsEventCalendars(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId)
        {
            
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            List<TeamsScheduleDetails> teamsScheduleDetailsv2 = new List<TeamsScheduleDetails>();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                string userid = webinarMaster.Username;

                EventMeeting eventMeeting = new EventMeeting();
                Start start = new Start();
                End end = new End();

                try
                {
                    eventMeeting.start = new Start();
                    eventMeeting.end = new End();
                    DateTime startDate = new DateTime(teams.StartDate.Ticks);
                    DateTime endDate = new DateTime(teams.EndDate.Ticks);

                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        teamsScheduleDetails = new TeamsScheduleDetails();

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
                        eventMeeting.attendees = new Attendance[1];
                        eventMeeting.attendees[0] = new Attendance();
                        eventMeeting.attendees[0].emailAddress = new EmailAddress();
                        eventMeeting.attendees[0].status = new Status1();
                        eventMeeting.attendees[0].type = "required";


                        JObject oJsonObject1 = new JObject();
                        oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));

                        ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                        var baseUrl = "https://graph.microsoft.com/v1.0/me/events";
                        TeamsEventResponse res = await ApiHelper.CreateTeamsEvent(oJsonObject1, TeamsAccessToken, baseUrl);

                        if (res != null)
                        {

                            teamsScheduleDetails.CourseID = teams.CourseID;
                            teamsScheduleDetails.ScheduleID = 0;
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
                            }
                            teamsScheduleDetailsv2.Add(teamsScheduleDetails);
                        }
                        teamsScheduleDetailsv2.Add(teamsScheduleDetails);
                    }

                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetailsv2;
        }
        private async Task<TeamsScheduleDetails> CallTeamsEventCalendarsV2(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId)
        {

            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.TeamsEmail == teams.Username).FirstOrDefault();
            if (webinarMaster != null)
            {
                string userid = webinarMaster.Username;

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

                    eventMeeting.subject = teams.CourseName;
                    eventMeeting.start.dateTime = sdate + sdate1 + ":" + sdate2;
                    eventMeeting.start.timeZone = "Asia/Kolkata";
                    eventMeeting.end.dateTime = edate + edate1 + ":" + edate2;
                    eventMeeting.end.timeZone = "Asia/Kolkata";
                    eventMeeting.IsOnlineMeeting = true;
                    eventMeeting.OnlineMeetingProvider = "teamsForBusiness";
                    eventMeeting.attendees = new Attendance[1];
                    eventMeeting.attendees[0] = new Attendance();
                    eventMeeting.attendees[0].emailAddress = new EmailAddress();
                    eventMeeting.attendees[0].status = new Status1();
                    eventMeeting.attendees[0].type = "required";


                    JObject oJsonObject1 = new JObject();
                    oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));

                    ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                    var baseUrl = "https://graph.microsoft.com/v1.0/me/events";
                    TeamsEventResponse res = await ApiHelper.CreateTeamsEvent(oJsonObject1, TeamsAccessToken, baseUrl);

                    if (res != null)
                    {

                        teamsScheduleDetails.CourseID = teams.CourseID;
                        teamsScheduleDetails.ScheduleID = 0;
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
                        }

                    }
                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetails;
        }

        private async Task<TeamsScheduleDetails> CallTeamsCancleEventCalendars(string TeamsAccessToken, int scheduleId,  int userId, TeamsScheduleDetails teamsScheduleDetails1, UserWebinarMaster webinarMaster)
        {

            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
           
            if (webinarMaster != null)
            {
                
                try
                {

                    ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                    var baseUrl = "https://graph.microsoft.com/v1.0/me/events/"+ teamsScheduleDetails1.MeetingId;
                    HttpResponseMessage Response = await ApiHelper.CallDeleteAPI(baseUrl, TeamsAccessToken );

                    if (Response.IsSuccessStatusCode)
                    {
                      
                        teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                        teamsScheduleDetails.IsActive = Record.NotActive;
                        teamsScheduleDetails.IsDeleted = Record.Deleted;

                       
                        teamsScheduleDetails.UserWebinarId = webinarMaster.Id;


                        if (teamsScheduleDetails1.MeetingId == null)
                        {
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
            return teamsScheduleDetails;
        }

        public async Task<HttpResponseMessage> CallGetAPIForTeam(TeamsScheduleDetails teamsScheduleDetails, UserWebinarMaster webinarMaster)
        {
            
            ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();
            IPublicClientApplication app = GetTeamsAuthentication();
            AuthenticationResult results = await GetTeamsToken(app, webinarMaster.TeamsEmail,1);
            string api = configurableValues.BaseUrl + results.UniqueId + "/onlineMeetings/"+ teamsScheduleDetails.MeetingId + "/attendanceReports";
            HttpResponseMessage aPITeamsCreateResponse = await ApiHelper.CallGetAPI(api, results.AccessToken);
            return aPITeamsCreateResponse;
        }
        public async Task<List<TeamsScheduleDetailsV2>> GetTeamsDetails(string ScheduleId)
        {
            ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ScheduleCode == ScheduleId && a.WebinarType == "TEAMS").FirstOrDefaultAsync();
            List<TeamsScheduleDetails> aPITeamsScheduleDetails = new List<TeamsScheduleDetails>();
            List<TeamsScheduleDetailsV2> teamsScheduleDetailsV3 = new List<TeamsScheduleDetailsV2>();
            TeamsScheduleDetailsV2 teamsScheduleDetailsV2 = new TeamsScheduleDetailsV2();
            if (iLTSchedule != null)
            {
                aPITeamsScheduleDetails = await _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToListAsync();

               
                
                if (aPITeamsScheduleDetails != null)
                {
                    foreach(TeamsScheduleDetails teamsScheduleDetails in aPITeamsScheduleDetails)
                    {
                        teamsScheduleDetailsV2 = Mapper.Map<TeamsScheduleDetailsV2>(teamsScheduleDetails);
                        UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetailsV2.UserWebinarId).FirstOrDefault();

                        if (userWebinarMaster != null)
                        {
                            teamsScheduleDetailsV2.Username = userWebinarMaster.TeamsEmail;
                        }
                        teamsScheduleDetailsV3.Add(teamsScheduleDetailsV2);
                    }
                   
                }
                
                return teamsScheduleDetailsV3;
            }
            return teamsScheduleDetailsV3;
        }
        public async Task<List<ILTSchedule>> GetTeamsMeetingsDetails(string ScheduleId)
        {
            int module = int.Parse(ScheduleId);
            List<ILTSchedule> iLTSchedule = await _db.ILTSchedule.Where(a => a.ModuleId == module && a.WebinarType == "TEAMS").ToListAsync();

            return iLTSchedule;
        }
        public async Task<List<ILTSchedule>> GetWebinarMeetingsDetails(int ScheduleId)
        {
            List<ILTSchedule> iLTSchedule = await _db.ILTSchedule.Where(a => a.ModuleId == ScheduleId && (a.WebinarType == "TEAMS" || a.WebinarType == "zoom")).ToListAsync();

            return iLTSchedule;
        }
        public string GetMeetingScheduleEmail(int? TeamsUserId)
        {
            UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == TeamsUserId).FirstOrDefault();
            if (userWebinarMaster == null)
            {
                return null;
            }
            return userWebinarMaster.TeamsEmail;
        }
        public async Task<MeetingsReportData> GetMeetingDetails(string MeetingId,string Username,string Code)
        {
            ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
            AuthenticationResult results = null;
            if (withoutPassword.Value.ToLower() == "yes")
            {
                results = await _onlineWebinarRepository.GetTeamsToken();
            }
            else
            {
                IPublicClientApplication app = GetTeamsAuthentication();
                results = await GetTeamsToken(app, Username, 1);
            }
                
            if(results == null)
            {
                return null;
            }
            ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "teams").FirstOrDefault();
            ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();
            string url = configurableValues.BaseUrl + results.UniqueId + "/onlineMeetings/" + MeetingId + "/attendanceReports";
            HttpResponseMessage aPITeamsCreateResponse = await ApiHelper.CallGetAPI(url, results.AccessToken);
            var result = aPITeamsCreateResponse.Content.ReadAsStringAsync().Result;

            TeamsMeeting TeamsResponce = JsonConvert.DeserializeObject<TeamsMeeting>(result);
            MeetingsReportData meetingsReportData = new MeetingsReportData();
            meetingsReportData.ReportId = new string[TeamsResponce.value.Length];
            meetingsReportData.totalParticipantCount = new int[TeamsResponce.value.Length];
            for(int i = 0;i < TeamsResponce.value.Length; i++)
            {
                meetingsReportData.ReportId[i] = TeamsResponce.value[i].id;
                meetingsReportData.totalParticipantCount[i] = TeamsResponce.value[i].totalParticipantCount;
            }
            meetingsReportData.MeetingId = MeetingId;
            meetingsReportData.Code = Code;
            return meetingsReportData;
        }
        public async Task<Meeting> GetTeamsMeetingReportForId(string meetingId,string reportId,string Username)
        {
            IPublicClientApplication app = GetTeamsAuthentication();
            AuthenticationResult results = await GetTeamsToken(app, Username, 1);
            ILTOnlineSetting iltonlineSettingteams = _db.ILTOnlineSetting.Where(a => a.Type.ToLower() == "teams").FirstOrDefault();
            ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();
            string url = configurableValues.BaseUrl + results.UniqueId + "/onlineMeetings/" + meetingId + "/attendanceReports/" + reportId + "?$expand=attendanceRecords";
            HttpResponseMessage aPITeamsCreateResponse = await ApiHelper.CallGetAPI(url, results.AccessToken);
            var result = aPITeamsCreateResponse.Content.ReadAsStringAsync().Result;
            Meeting TeamsResponce = JsonConvert.DeserializeObject<Meeting>(result);

            return TeamsResponce;
        }
        public FileInfo ExportTeamsMeetingReportForId(ExportTeamsMeetingReport[] exportTeamsMeetingReports,string ExportAs)
        {
            FileInfo File = null;
            if(exportTeamsMeetingReports == null)
            {
                return File;
            }
            File = GetTeamsExcelReport(exportTeamsMeetingReports,ExportAs);
            return File;
        }
        private FileInfo GetTeamsExcelReport(ExportTeamsMeetingReport[] exportTeamsMeetingReport,string ExportAs)
        {
            String ExcelNameCSV = "TeamsDetailReport.csv";
            String ExcelNameXLSX = "TeamsDetailReport.xlsx";
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();

            List<string> ExportHeader = GetTeamsReportHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (ExportTeamsMeetingReport exportTeamsMeetingReport1 in exportTeamsMeetingReport)
            {
                List<string> DataRow = new List<string>();
                DataRow = TeamsReportRow(exportTeamsMeetingReport1);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }
            if (ExportAs == "csv")
            { 
                DataTable ForToCsv = this._tLSHelper.ToDataTableUserLearningReport<ExportTeamsMeetingReport>(exportTeamsMeetingReport);
                FileInfo csvFile = _tLSHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo fileInfo = _tLSHelper.GenerateExcelFile(ExcelNameXLSX, ExportData);
                return fileInfo;
            }
        }
        private List<string> GetTeamsReportHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                "DisplayName",
                "EmailId",
                "Role",
                "MeetingStartDateTime",
                "JoinDateTime",
                "MeetingEndDateTime",
                "LeaveDateTime",

            };
            return ExportHeader;
        }
        private List<string> TeamsReportRow(ExportTeamsMeetingReport exportTeamsMeetingReport)
        {

            List<string> TeamsRow = new List<string>
            {
                exportTeamsMeetingReport.displayName,
                exportTeamsMeetingReport.emailAddress,
                exportTeamsMeetingReport.role,
                exportTeamsMeetingReport.meetingStartDateTime,
                exportTeamsMeetingReport.joinDateTime,
                exportTeamsMeetingReport.meetingEndDateTime,
                exportTeamsMeetingReport.leaveDateTime,
            };

            return TeamsRow;
        }
        public string ZoomToken()
        {
            try
            {
                ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "zoom").FirstOrDefault();
                if (configurableValues == null)
                {
                    return null;
                }
                // Token will be good for 20 minutes
                DateTime Expiry = DateTime.UtcNow.AddMinutes(20);

                string ApiKey = configurableValues.ConfigurableParameter1;
                string ApiSecret = configurableValues.Authority;

                int ts = (int)(Expiry - new DateTime(1970, 1, 1)).TotalSeconds;

                // Create Security key  using private key above:
                var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiSecret));

                // length should be >256b
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //Finally create a Token
                var header = new JwtHeader(credentials);

                //Zoom Required Payload
                var payload = new JwtPayload
                {
                    { "iss", ApiKey},
                    { "exp", ts },
                };

                var secToken = new JwtSecurityToken(header, payload);
                var handler = new JwtSecurityTokenHandler();

                // Token to String so you can use it in your client
                var tokenString = handler.WriteToken(secToken);

                return tokenString;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public async Task<ZoomMeetingDetailsV2> GetZoomDetails(string ScheduleId)
        {
            int code = int.Parse(ScheduleId);
            ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ID == code && a.WebinarType == "zoom").FirstOrDefaultAsync();
            ZoomMeetingDetails aPIZoomScheduleDetails = new ZoomMeetingDetails();
            ZoomMeetingDetailsV2 zoomMeetingDetailsV2 = new ZoomMeetingDetailsV2();
            if (iLTSchedule != null)
            {
                aPIZoomScheduleDetails = await _db.ZoomMeetingDetails.Where(a => a.ScheduleID == iLTSchedule.ID).FirstOrDefaultAsync();

                if(aPIZoomScheduleDetails != null)
                {
                    zoomMeetingDetailsV2 = Mapper.Map<ZoomMeetingDetailsV2>(aPIZoomScheduleDetails);
                }
                UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == aPIZoomScheduleDetails.UserWebinarId).FirstOrDefault();
                if (userWebinarMaster != null)
                {
                    zoomMeetingDetailsV2.Username = userWebinarMaster.TeamsEmail;
                }
                return zoomMeetingDetailsV2;
            }
            return zoomMeetingDetailsV2;
        }
        public async Task<int> UpdateZoomMeeting(UpdateZoom zoom, int userId)
        {
            APIZoomMeetingResponce aPIZoomMeetingResponce = new APIZoomMeetingResponce();


            UserWebinarMaster userWebinarMaster = this._db.UserWebinarMasters.Where(a => a.Id == zoom.userWebinarId).FirstOrDefault();
            string Token = ZoomToken();

            APIZoomCreate aPIZoomCreate = new APIZoomCreate();
            APIZoomCreateSettings aPIZoomCreateSettings = new APIZoomCreateSettings();
            APIZoomCreateRecurrence aPIZoomCreateRecurrence = new APIZoomCreateRecurrence();
            try
            {
                aPIZoomCreateSettings.host_video = false;
                aPIZoomCreateSettings.participant_video = false;
                aPIZoomCreateSettings.cn_meeting = false;
                aPIZoomCreateSettings.in_meeting = true;
                aPIZoomCreateSettings.join_before_host = false;
                aPIZoomCreateSettings.approval_type = 2;
                aPIZoomCreateSettings.mute_upon_entry = false;
                aPIZoomCreateSettings.meeting_authentication = false;
                aPIZoomCreateSettings.authentication_domains = "https://uat.gogetempowered.com";

                aPIZoomCreateRecurrence.type = 1;
                aPIZoomCreateRecurrence.repeat_interval = 1;
                aPIZoomCreateRecurrence.weekly_days = 1;
                aPIZoomCreateRecurrence.monthly_week = 1;
                aPIZoomCreateRecurrence.monthly_week_day = 1;
                aPIZoomCreateRecurrence.end_times = 1;
                aPIZoomCreateRecurrence.end_date_time = string.Format("{0:yyyy-MM-ddThh:mm:ssZ}", zoom.EndDate.Add(TimeSpan.Parse(zoom.EndTime)));
                aPIZoomCreateRecurrence.end_date_time = aPIZoomCreateRecurrence.end_date_time.Substring(0, 11);
                aPIZoomCreateRecurrence.end_date_time = aPIZoomCreateRecurrence.end_date_time + zoom.EndTime + ":" + "00";

                aPIZoomCreate.topic = zoom.CourseName;

                aPIZoomCreate.start_time = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", zoom.StartDate.Add(TimeSpan.Parse(zoom.StartTime)));
                aPIZoomCreate.start_time = aPIZoomCreate.start_time.Substring(0, 11);
                aPIZoomCreate.start_time = aPIZoomCreate.start_time + zoom.StartTime + ":" + "00";
                TimeSpan diffDate = zoom.EndDate.Date.Add(TimeSpan.Parse(zoom.EndTime)).Subtract(zoom.StartDate.Date.Add(TimeSpan.Parse(zoom.StartTime)));
                aPIZoomCreate.duration = Convert.ToInt32(diffDate.TotalMinutes);
                aPIZoomCreate.agenda = zoom.CourseName;
                aPIZoomCreate.settings = aPIZoomCreateSettings;
                aPIZoomCreate.recurrence = aPIZoomCreateRecurrence;
                aPIZoomCreate.type = 2;

                JObject oJsonObject = new JObject();
                oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPIZoomCreate));

                int response = await ApiHelper.UpdateZoomMeetingResponce(oJsonObject, userWebinarMaster.TeamsEmail, zoom.MeetingId, Token);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return -1;
        }
        public async Task<ZoomMeetingParticipants> GetZoomMeetingParticipants(long UniqueZoomMeetingId)
        {
            if(UniqueZoomMeetingId == 0)
            {
                return null;
            }
            string Token = ZoomToken();
            if(Token == null)
            {
                return null;
            }
            string url = "https://api.zoom.us/v2/report/meetings/" + UniqueZoomMeetingId + "/participants";
            HttpResponseMessage httpResponseMessage = await ApiHelper.CallGetAPI(url, Token);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
            ZoomMeetingParticipants zoomMeetingParticipants  = JsonConvert.DeserializeObject<ZoomMeetingParticipants>(result);
            return zoomMeetingParticipants;

        }
        public async Task<ZoomMeetingDetailsForReport> ZoomMeetingDetailsForReport(long UniqueZoomMeetingId)
        {
            if (UniqueZoomMeetingId == 0)
            {
                return null;
            }
            string Token = ZoomToken();
            if (Token == null)
            {
                return null;
            }
            string url = "https://api.zoom.us/v2/past_meetings/" + UniqueZoomMeetingId;
            HttpResponseMessage httpResponseMessage = await ApiHelper.CallGetAPI(url, Token);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;
            ZoomMeetingDetailsForReport zoomMeetingDetailsForReport = JsonConvert.DeserializeObject<ZoomMeetingDetailsForReport>(result);
            return zoomMeetingDetailsForReport;
        }
        public FileInfo ExportZoomMeetingReportForId(ZoomReport zoomReport, string ExportAs)
        {
            FileInfo File = null;
            if (zoomReport == null)
            {
                return File;
            }
            File = GetZoomExcelReport(zoomReport, ExportAs);
            return File;
        }
        private FileInfo GetZoomExcelReport(ZoomReport zoomReport, string ExportAs)
        {
            String ExcelNameCSV = "ZoomDetailReport.csv";
            String ExcelNameXLSX = "ZoomDetailReport.xlsx";
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();

            List<string> ExportHeader = GetZoomReportHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (ZoomParticipants zoomParticipants in zoomReport.zoomMeetingParticipants.participants)
            {
                List<string> DataRow = new List<string>();
                DataRow = ZoomReportRow(zoomParticipants,zoomReport.ZoomMeetingDetailsForReport);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }
            if (ExportAs == "csv")
            {

                DataTable ForToCsv = this._tLSHelper.ToDataTableUserLearningReportZoom(zoomReport.zoomMeetingParticipants.participants,zoomReport.ZoomMeetingDetailsForReport);
                FileInfo csvFile = _tLSHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo fileInfo = _tLSHelper.GenerateExcelFile(ExcelNameXLSX, ExportData);
                return fileInfo;
            }
        }
        private List<string> GetZoomReportHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                "Name",
                "EmailId",
                "MeetingStartDateTime",
                "JoinDateTime",
                "MeetingEndDateTime",
                "LeaveDateTime",
                "Status"
            };
            return ExportHeader;
        }
        private List<string> ZoomReportRow(ZoomParticipants zoomParticipants, ZoomMeetingDetailsForReport zoomMeetingDetailsForReport)
        {

            List<string> TeamsRow = new List<string>
            {
                zoomParticipants.name,
                zoomParticipants.user_email,
                zoomMeetingDetailsForReport.start_time,
                zoomParticipants.join_time,
                zoomMeetingDetailsForReport.end_time,
                zoomParticipants.leave_time,
                zoomParticipants.status
            };

            return TeamsRow;
        }

        public async Task<List<APIILTSchedular>> GetAllActiveSchedulesV2(ApiScheduleGet apiScheduleGet, int UserId, string OrganisationCode)
        {
            List<APIILTSchedular> ScheduleList = new List<APIILTSchedular>();
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
                            cmd.CommandText = "GetScheduleDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = apiScheduleGet.page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = apiScheduleGet.pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = apiScheduleGet.search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = apiScheduleGet.searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = apiScheduleGet.showAllData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTSchedular obj = new APIILTSchedular();

                                obj.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                obj.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                obj.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                obj.StartTime = row["StartTime"].ToString();
                                obj.EndTime = row["EndTime"].ToString();
                                obj.RegistrationEndDate = Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                obj.IsActive = Convert.ToBoolean(row["Status"].ToString());
                                obj.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                obj.TrainerType = row["TrainerType"].ToString();
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.ContactPersonName = row["ContactPersonName"].ToString();
                                obj.City = row["City"].ToString();
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                obj.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                obj.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                obj.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                obj.TrainerDescription = row["TrainerDescription"].ToString();
                                obj.ScheduleType = row["ScheduleType"].ToString();
                                obj.ReasonForCancellation = row["ReasonForCancellation"].ToString();
                                obj.Status = Convert.ToBoolean(row["Status"].ToString());
                                obj.EventLogo = row["EventLogo"].ToString();
                                obj.Cost = string.IsNullOrEmpty(row["Cost"].ToString()) ? 0 : float.Parse(row["Cost"].ToString());
                                obj.Currency = row["Currency"].ToString();
                                obj.CourseCode = row["CourseCode"].ToString();
                                obj.CourseName = row["CourseName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(Convert.ToString(row["CourseID"])) ? 0 : Convert.ToInt32(row["CourseID"].ToString());
                                obj.BatchId = string.IsNullOrEmpty(Convert.ToString(row["BatchId"])) ? 0 : Convert.ToInt32(row["BatchId"].ToString());
                                obj.BatchCode = string.IsNullOrEmpty(Convert.ToString(row["BatchCode"])) ? null : Convert.ToString(row["BatchCode"]);
                                obj.BatchName = string.IsNullOrEmpty(Convert.ToString(row["BatchName"])) ? null : Convert.ToString(row["BatchName"]);
                                obj.UserName = string.IsNullOrEmpty(Convert.ToString(row["UserName"])) ? null : Convert.ToString(row["UserName"]);  ;
                                obj.UserCreated = Convert.ToBoolean(row["UserCreated"]);
                                ScheduleList.Add(obj);
                            }

                          


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ScheduleList;
        }

        public async Task<GoogleMeetDetailsV2> GetGsuitDetails(int ScheduleId)
        {
           
            ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ID == ScheduleId && a.WebinarType.ToLower() == "googlemeet").FirstOrDefaultAsync();
            GoogleMeetDetails aPIGoogleMeetDetails = new GoogleMeetDetails();
            GoogleMeetDetailsV2 googleMeetDetailsV2 = new GoogleMeetDetailsV2();
            if (iLTSchedule != null)
            {
                aPIGoogleMeetDetails = await _db.GoogleMeetDetails.Where(a => a.ScheduleID == iLTSchedule.ID).FirstOrDefaultAsync();

                if (aPIGoogleMeetDetails != null)
                {
                    googleMeetDetailsV2 = Mapper.Map<GoogleMeetDetailsV2>(aPIGoogleMeetDetails);
                }
                else
                    return null;
                UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == aPIGoogleMeetDetails.UserWebinarId).FirstOrDefault();
                if (userWebinarMaster != null)
                {
                    googleMeetDetailsV2.Username = userWebinarMaster.TeamsEmail;
                }
                return googleMeetDetailsV2;
            }
            return googleMeetDetailsV2;
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return results;
        }
    }
    
}
