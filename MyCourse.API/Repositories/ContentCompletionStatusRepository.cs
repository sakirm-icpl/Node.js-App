using MyCourse.API.Common;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.API.Helper;
using log4net;
//using MyCourse.API.ExternalIntegration.EdCast;
//using MyCourse.API.Model.EdCastAPI;
using Microsoft.AspNetCore.Mvc;
using MyCourse.API.APIModel;
using System.Net.Http;
using Azure;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MyCourse.API.Repositories
{
    public class ContentCompletionStatusRepository : Repository<ContentCompletionStatus>, IContentCompletionStatus
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContentCompletionStatusRepository));
        private CourseContext _db;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        private readonly IModuleCompletionStatusRepository _moduleCompletionStatus;
        ICustomerConnectionStringRepository _customerConnection;
        ICourseRepository _courseRepository;
        private readonly ILCMSRepository _lCMSRepository;
        public ContentCompletionStatusRepository(CourseContext context,
            ICourseCompletionStatusRepository courseCompletionStatusRepository,
            ICourseModuleAssociationRepository courseModuleAssociationRepository,
            ICustomerConnectionStringRepository customerConnection,
            IModuleCompletionStatusRepository moduleCompletionStatus,
            ICourseRepository courseRepository,
            ILCMSRepository lCMSRepository) : base(context)
        {
            _db = context;
            _courseModuleAssociationRepository = courseModuleAssociationRepository;
            this._customerConnection = customerConnection;
            _moduleCompletionStatus = moduleCompletionStatus;
            _courseRepository = courseRepository;
            _lCMSRepository = lCMSRepository;
        }
        public async Task<ContentCompletionStatus> Get(int userId, int courseId, int moduleId)
        {
            IQueryable<ContentCompletionStatus> Query = _db.ContentCompletionStatus;
            Query = Query.Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId);
            return await Query.FirstOrDefaultAsync();
        }
        public async Task<bool> IsContentStarted(int userId, int courseId, int moduleId)
        {
            int Count = await _db.ContentCompletionStatus.Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId).CountAsync();
            if (Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<int> PostCompletion(ContentCompletionStatus contentCompletionStatus, string CourseType = null, string Token = null)
        {

            ContentCompletionStatus ExistingContent = await this.Get(contentCompletionStatus.UserId, contentCompletionStatus.CourseId, contentCompletionStatus.ModuleId);
            if (ExistingContent != null)
            {
                ExistingContent.Location = contentCompletionStatus.Location;
                if (ExistingContent.Status == Status.Completed)
                    ExistingContent.Status = Status.Completed;
                else
                    ExistingContent.Status = contentCompletionStatus.Status;
                ExistingContent.ModifiedDate = DateTime.UtcNow;
                await this.Update(ExistingContent);
            }
            else
            {
                contentCompletionStatus.CreatedDate = DateTime.UtcNow;
                await this.Add(contentCompletionStatus);
            }
            ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
            ModuleStatus.UserId = contentCompletionStatus.UserId;
            ModuleStatus.CourseId = contentCompletionStatus.CourseId;
            ModuleStatus.ModuleId = contentCompletionStatus.ModuleId;
            ModuleStatus.CreatedDate = DateTime.UtcNow;
            ModuleStatus.ModifiedDate = DateTime.UtcNow;
            ModuleStatus.Status = Status.InProgress;
            if (contentCompletionStatus.Status == Status.Completed)
            {
                bool AssesmentExist = await _courseModuleAssociationRepository.IsAssementExist(contentCompletionStatus.CourseId, contentCompletionStatus.ModuleId);
                bool FeedbackExist = await _courseModuleAssociationRepository.IsFeedbackExist(contentCompletionStatus.CourseId, contentCompletionStatus.ModuleId);
                if (!AssesmentExist && !FeedbackExist && contentCompletionStatus.Status == Status.Completed)
                    ModuleStatus.Status = Status.Completed;
            }
            await _moduleCompletionStatus.Post(ModuleStatus, CourseType, Token);
            return 1;
        }

        public async Task<int> Post(ContentCompletionStatus contentCompletionStatus, string CourseType = null, string Token = null, string OrgCode = null)
        {
            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection

            string CourseStatusFromSP = string.Empty;
            string ModuleStatusFromSP = string.Empty;
            string ContentStatusFromSP = string.Empty;

            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {

                    //<Added by Gaurav for optimization>
                    cmd.CommandText = "ContentModuleCourseCompletion";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = contentCompletionStatus.CourseId });
                    cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = contentCompletionStatus.ModuleId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = contentCompletionStatus.UserId });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = contentCompletionStatus.Status });
                    cmd.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar) { Value = contentCompletionStatus.Location });
                    cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.NVarChar) { Value = contentCompletionStatus.ScheduleId });
                    cmd.Parameters.Add(new SqlParameter("@IsUserConsent", SqlDbType.Bit) { Value = contentCompletionStatus.IsUserConsent });
                    
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    foreach (DataRow row in dt.Rows)
                    {
                        CourseStatusFromSP = row["CourseStatus"].ToString();
                        ModuleStatusFromSP = row["CourseStatus"].ToString();
                        ContentStatusFromSP = row["CourseStatus"].ToString();
                    }
                    reader.Dispose();
                    //</Added by Gaurav for optimization>
                }
                connection.Close();

                var DarwinboxPost = await _courseRepository.GetMasterConfigurableParameterValue("Darwinbox_Post");
                _logger.Debug("Darwinbox_Post :-" + DarwinboxPost);
                if (Convert.ToString(DarwinboxPost).ToLower() == "yes")
                {
                    string courseStatus = "inprogress";
                    courseStatus = await this._db.CourseCompletionStatus.Where(c => c.CourseId == contentCompletionStatus.CourseId && c.UserId == contentCompletionStatus.UserId).Select(c => c.Status).FirstOrDefaultAsync();
                    if (courseStatus != null)
                    {
                        if (courseStatus != "completed")
                        {
                            DarwinboxTransactionDetails obj = await _courseRepository.PostCourseStatusToDarwinbox(contentCompletionStatus.CourseId, contentCompletionStatus.UserId, CourseStatusFromSP, OrgCode);
                        }
                    }
                }

                if (contentCompletionStatus.Status == Status.Completed)
                {
                    ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
                    ModuleStatus.UserId = contentCompletionStatus.UserId;
                    ModuleStatus.CourseId = contentCompletionStatus.CourseId;
                    ModuleStatus.ModuleId = contentCompletionStatus.ModuleId;
                    ModuleStatus.CreatedDate = DateTime.UtcNow;
                    ModuleStatus.ModifiedDate = DateTime.UtcNow;
                    ModuleStatus.Status = Status.InProgress;
                    if (contentCompletionStatus.Status == Status.Completed)
                    {
                        ModuleStatus.Status = ModuleStatusFromSP;
                    }
                    await _moduleCompletionStatus.PostCompletion(ModuleStatus, CourseType, Token, OrgCode, CourseStatusFromSP);
                }
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
            return 1;
        }
        public async Task<bool> IsContentCompleted(int userId, int courseId, int moduleId)
        {
            if (await this._db.ContentCompletionStatus.Where(c => c.CourseId == courseId && c.UserId == userId && c.ModuleId == moduleId && c.Status.Equals(Status.Completed)).CountAsync() > 0)
                return true;
            return false;
        }
        public async Task<string> GetStatus(int courseId, int moduleId, int userId)
        {
            string Stat = await this._db.ContentCompletionStatus.Where(c => c.CourseId == courseId && c.ModuleId == moduleId && c.UserId == userId).Select(m => m.Status).SingleOrDefaultAsync();
            if (Stat != null)
                return Stat;
            else
                return Status.NotStarted;
        }
        public async Task<KpointStatus> SaveKpointStatus(APIContentCompletionStatusForKpoint aPIContentCompletionStatusForKpoint,int Userid)
        {
            string token = _lCMSRepository.KPointToken(Userid);
            string url = "https://enthralltech.zencite.com/api/v3/users/me/viewerships?type=summary&scope=video&include.video="+ aPIContentCompletionStatusForKpoint.gccid+ "&xt="+ token;
            try
            {
                HttpResponseMessage httpResponseMessage = await APIHelper.CallGetAPI(url);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadAsStringAsync();
                    _logger.Error(result.Length+"\t"+result);
                    if (result.Length == 2)
                    {
                        return null;
                    }
                    KpointStatus[] kpointStatus = JsonConvert.DeserializeObject<KpointStatus[]>(result);
                    return kpointStatus[0];
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
           
            return null;
        }
        public async Task<List<KPointReportV2>> getKpointReport(APIForKpointReport aPIForKpointReport, int Userid)
        {
            string token = _lCMSRepository.KPointTokenForAdmin();

            CourseModuleAssociation courseModuleAssociation = _db.CourseModuleAssociation.Where(
                a => a.CourseId == aPIForKpointReport.CourseId
                ).FirstOrDefault();
            Module module = _db.Module.Where(a => a.Id == courseModuleAssociation.ModuleId).FirstOrDefault();
            LCMS lCMS = _db.LCMS.Where(a => a.Id == module.LCMSId).FirstOrDefault();

            string url = "https://enthralltech.zencite.com/api/v3/videos/" + lCMS.ExternalLCMSId + "/viewership?from=0&to=0&max=100&details=false&by=user&xt=" + token;
            try
            {
                HttpResponseMessage httpResponseMessage = await APIHelper.CallGetAPI(url);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    List<KPointReportV2> kPointReportV2s = new List<KPointReportV2>();
                    var result = await httpResponseMessage.Content.ReadAsStringAsync();

                    if (result.Length == 2)
                    {
                        return null;
                    }
                    KPointReport[] kpointStatus = JsonConvert.DeserializeObject<KPointReport[]>(result);
                    foreach (KPointReport kPointReport in kpointStatus)
                    {
                        KPointReportV2 kPointReportV2 = new KPointReportV2();
                        
                        kPointReportV2.view_count = kPointReport.view_count;
                        kPointReportV2.viewed_duration = kPointReport.viewed_duration;
                        kPointReportV2.percentage_viewed = kPointReport.percentage_viewed;
                        if(kPointReport.quizscore != null)
                        {
                            if(kPointReport.quizscore.list.Length != 0)
                            {
                                kPointReportV2.title = kPointReport.quizscore.list[0].title;
                                kPointReportV2.attempts = kPointReport.quizscore.list[0].attempts;
                                kPointReportV2.percentage = kPointReport.quizscore.list[0].percentage;
                            }
                            
                        }
                        string email = Security.Encrypt(kPointReport.email);
                        UserMaster userMaster = _db.UserMaster.Where(a => a.EmailId == email).FirstOrDefault();
                        if (userMaster != null)
                        {
                            kPointReportV2.UserName = Security.Decrypt(userMaster.UserName);
                            kPointReportV2.UserId = Security.Decrypt(userMaster.UserId);
                        }
                        MyCourse.API.Model.Course course = _db.Course.Where(a => a.Id == aPIForKpointReport.CourseId).FirstOrDefault();
                        if(course != null)
                        {
                            kPointReportV2.CourseName = course.Title;
                        }
                        kPointReportV2s.Add(kPointReportV2);
                    }
                    return kPointReportV2s;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }

            return null;
        }
    }
}
