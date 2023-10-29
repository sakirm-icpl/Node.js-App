using log4net;
using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Models;
using Assessment.API.Common;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using Assessment.API.Model.EdCastAPI;

namespace Assessment.API.Repositories
{
    public class ContentCompletionStatusRepository : Repository<ContentCompletionStatus>, IContentCompletionStatus
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContentCompletionStatusRepository));
        private AssessmentContext _db;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        private readonly IModuleCompletionStatusRepository _moduleCompletionStatus;
        ICustomerConnectionStringRepository _customerConnection;
        ICourseRepository _courseRepository;
        private readonly ILCMSRepository _lCMSRepository;
        public ContentCompletionStatusRepository(AssessmentContext context,
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

        public async Task<int> Post(ContentCompletionStatus contentCompletionStatus, string? CourseType = null, string? Token = null, string? OrgCode = null)
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

        public async Task<int> PostCompletion(ContentCompletionStatus contentCompletionStatus, string? CourseType = null, string? Token = null)
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

        public async Task<ContentCompletionStatus> Get(int userId, int courseId, int moduleId)
        {
            IQueryable<ContentCompletionStatus> Query = _db.ContentCompletionStatus;
            Query = Query.Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId);
            return await Query.FirstOrDefaultAsync();
        }

    }
}