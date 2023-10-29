using MyCourse.API.APIModel;
using MyCourse.API.APIModel.DiscussionForum;
using MyCourse.API.Helper;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Repositories.Interfaces.DiscussionForum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.API.Model;
using log4net;
using MyCourse.API.Common;

namespace MyCourse.API.Repositories.DiscussionForum
{
    public class DiscussionForumRepository : Repository<Model.DiscussionForum>, IDiscussionForumRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DiscussionForumRepository));
        private CourseContext db;
        ICustomerConnectionStringRepository _customerConnection;
        private ICourseRepository _courseRepository;
        private IAccessibilityRule _accessibilityRule;
        private IConfiguration _configuration;
        public DiscussionForumRepository(CourseContext context, IAccessibilityRule accessibilityRule,
            ICustomerConnectionStringRepository customerConnection, IConfiguration configuration, ICourseRepository courseRepository) : base(context)
        {
            this.db = context;
            this._customerConnection = customerConnection;
            this._configuration = configuration;
            this._courseRepository = courseRepository;
            this._accessibilityRule = accessibilityRule;
        }

        public async Task<Action> SavePost(int? id, int? PostId, int CourseId, string SubjectText, string User, int ModifiedBy, string FilePath, string FileType, string organisationCode)
        {
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
                            cmd.CommandText = "DiscussionForum_Upsert";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.BigInt) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.BigInt) { Value = PostId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@SubjectText", SqlDbType.NVarChar) { Value = SubjectText });
                            cmd.Parameters.Add(new SqlParameter("@User", SqlDbType.NVarChar) { Value = User });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.BigInt) { Value = ModifiedBy });
                            cmd.Parameters.Add(new SqlParameter("@FilePath", SqlDbType.NVarChar) { Value = FilePath });
                            cmd.Parameters.Add(new SqlParameter("@FileType", SqlDbType.NVarChar) { Value = FileType });
                            await cmd.ExecuteNonQueryAsync();
                        }

                        #region "Send Bell Notifications"

                        List<DiscussionForumUsers> discussionForumUsers = await this.GetDiscussionForumUsers(Convert.ToInt32(PostId), Convert.ToInt32(CourseId), organisationCode);

                        var Title = dbContext.Course.Where(a => a.Id == CourseId).Select(a => a.Title).SingleOrDefault();
                        bool IsApplicableToAll = dbContext.Course.Where(a => a.Id == CourseId).Select(a => a.IsApplicableToAll).SingleOrDefault();
                        int notificationID = 0;

                        ApiNotification Notification = new ApiNotification();
                        Notification.Title = Title;
                        Notification.Message = this._configuration[Configuration.DiscussionBoardNotification].ToString();
                        Notification.Message = Notification.Message.Replace("{course}", Title);
                        Notification.Url = TlsUrl.DiscussionBoardPost + CourseId;
                        Notification.Type = Record.DiscussionBoard;
                        Notification.UserId = ModifiedBy;
                        Notification.CourseId = CourseId;
                        notificationID = await this._accessibilityRule.SendNotificationCourseApplicability(Notification, IsApplicableToAll);

                        DataTable dtUserIds = new DataTable();
                        dtUserIds.Columns.Add("UserIds");

                        foreach (var result in discussionForumUsers)
                        {
                            if (result.UserId != ModifiedBy)
                            {
                                dtUserIds.Rows.Add(result.UserId);
                            }
                        }
                        if (dtUserIds.Rows.Count > 0)
                            await this._accessibilityRule.SendDataForApplicableNotifications(notificationID, dtUserIds, ModifiedBy);
                        #endregion

                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }



        public async Task<List<ApiNotification>> GetDiscussionBoardCount(int CourseID)
        {
            List<ApiNotification> listUserApplicability = new List<ApiNotification>();

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
                            cmd.CommandText = "GetDiscussionBoardCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.NVarChar) { Value = CourseID });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    listUserApplicability.Add(new ApiNotification() { Title = row["Title"].ToString(), Id = Convert.ToInt32(row["Id"]) });
                                }
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
            }
            return listUserApplicability;
        }



        public async Task<List<DiscussionForumUsers>> GetDiscussionForumUsers(int PostId, int CourseId, string orgCode)
        {
            List<DiscussionForumUsers> users = new List<DiscussionForumUsers>();

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
                            cmd.CommandText = "DiscussionBoard_PushNotification";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = PostId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = CourseId });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    DiscussionForumUsers user = new DiscussionForumUsers();
                                    user.UserId = Convert.ToInt32(row["UserId"].ToString());
                                    user.UserName = row["UserName"].ToString();
                                    user.CourseId = Convert.ToInt32(row["CourseId"].ToString());
                                    user.CourseName = row["CourseName"].ToString();
                                    users.Add(user);
                                    reader.Dispose();
                                }
                                connection.Close();
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
            return users;
        }



        public bool Exists(int courseId)
        {
            if (this.db.DiscussionForum.Count(x => (x.CourseId == courseId)) > 0)
                return true;

            return false;
        }



        public async Task<int> GetCountDiscussionForum(bool? IsShowActiveRecords, int CourseId)
        {
            if (IsShowActiveRecords == false)
            {
                return await this.db.DiscussionForum.Where(b => (b.CourseId.Equals(CourseId))).CountAsync();
            }
            else
            {
                return await this.db.DiscussionForum.Where(b => (b.CourseId.Equals(CourseId) && b.IsDeleted == Record.NotDeleted)).CountAsync();
            }
        }



        public async Task<IEnumerable<APIDiscussionForum>> GetDiscussionForumByCourseId(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null)
        {
            try
            {
                List<APIDiscussionForum> result = new List<APIDiscussionForum>();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetDiscussionForum";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.BigInt) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@IsShowActiveRecords", SqlDbType.Bit) { Value = IsShowActiveRecords });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });


                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APIDiscussionForum data = new APIDiscussionForum();
                                data.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                data.PostThreadId = string.IsNullOrEmpty(row["PostThreadId"].ToString()) ? 0 : int.Parse(row["PostThreadId"].ToString());
                                data.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                data.PostParentId = string.IsNullOrEmpty(row["PostParentId"].ToString()) ? 0 : int.Parse(row["PostParentId"].ToString());
                                data.PostLevel = string.IsNullOrEmpty(row["PostLevel"].ToString()) ? 0 : int.Parse(row["PostLevel"].ToString());
                                data.SortOrder = string.IsNullOrEmpty(row["SortOrder"].ToString()) ? 0 : int.Parse(row["SortOrder"].ToString());
                                data.SubjectText = row["SubjectText"].ToString();
                                data.UseName = row["UseName"].ToString();
                                data.Date = string.IsNullOrEmpty(row["CreatedDate"].ToString()) ? DateTime.MinValue : DateTime.Parse(row["CreatedDate"].ToString());
                                data.UserProfilePicture = row["ProfilePicture"].ToString();
                                data.UserProfilePicture = string.IsNullOrEmpty(data.UserProfilePicture) ? null : string.Concat(this._configuration["ApiGatewayUrl"], data.UserProfilePicture);
                                data.IsDeleted = string.IsNullOrEmpty(row["IsDeleted"].ToString()) ? false : bool.Parse(row["IsDeleted"].ToString());
                                data.IsCommentAddedByUser = string.IsNullOrEmpty(row["IsCommentAddedByUser"].ToString()) ? false : bool.Parse(row["IsCommentAddedByUser"].ToString());
                                data.PrePostUsename = row["PrePostUsename"].ToString();
                                data.SubjectText = row["subjecttext"].ToString();

                                result.Add(data);
                            }
                            connection.Close();
                        }
                    }
                }
                List<APIDiscussionForum> DiscussionPosts = new List<APIDiscussionForum>();
                //Adding post  
                DiscussionPosts = result.Where(d => d.Id == d.PostThreadId && d.PostParentId == d.Id).ToList();


                return DiscussionPosts;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<IEnumerable<APIDiscussionForum>> GetAllDiscussionCommentsByParentId(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null)
        {
            try
            {
                List<APIDiscussionForum> result = new List<APIDiscussionForum>();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetDiscussionForum";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.BigInt) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@IsShowActiveRecords", SqlDbType.Bit) { Value = IsShowActiveRecords });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });


                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APIDiscussionForum data = new APIDiscussionForum();
                                data.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                data.PostThreadId = string.IsNullOrEmpty(row["PostThreadId"].ToString()) ? 0 : int.Parse(row["PostThreadId"].ToString());
                                data.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                data.PostParentId = string.IsNullOrEmpty(row["PostParentId"].ToString()) ? 0 : int.Parse(row["PostParentId"].ToString());
                                data.PostLevel = string.IsNullOrEmpty(row["PostLevel"].ToString()) ? 0 : int.Parse(row["PostLevel"].ToString());
                                data.SortOrder = string.IsNullOrEmpty(row["SortOrder"].ToString()) ? 0 : int.Parse(row["SortOrder"].ToString());
                                data.SubjectText = row["SubjectText"].ToString();
                                data.UseName = row["UseName"].ToString();
                                data.Date = string.IsNullOrEmpty(row["CreatedDate"].ToString()) ? DateTime.MinValue : DateTime.Parse(row["CreatedDate"].ToString());
                                data.UserProfilePicture = row["ProfilePicture"].ToString();
                                data.UserProfilePicture = string.IsNullOrEmpty(data.UserProfilePicture) ? null : string.Concat(this._configuration["ApiGatewayUrl"], data.UserProfilePicture);
                                data.IsDeleted = string.IsNullOrEmpty(row["IsDeleted"].ToString()) ? false : bool.Parse(row["IsDeleted"].ToString());
                                data.IsCommentAddedByUser = string.IsNullOrEmpty(row["IsCommentAddedByUser"].ToString()) ? false : bool.Parse(row["IsCommentAddedByUser"].ToString());
                                data.PrePostUsename = row["PrePostUsename"].ToString();
                                data.SubjectText = row["subjecttext"].ToString();
                                data.FilePath = row["FilePath"].ToString();
                                data.FileType = row["FileType"].ToString();
                                data.UserName = row["UserName"].ToString();
                                data.Gender = row["Gender"].ToString();
                                result.Add(data);
                            }
                            connection.Close();
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<IEnumerable<APIDiscussionForum>> GetAllDiscussion(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null)
        {
            List<APIDiscussionForum> result = new List<APIDiscussionForum>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetDiscussionForum";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = CourseId });
                        cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.BigInt) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@IsShowActiveRecords", SqlDbType.Bit) { Value = IsShowActiveRecords });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            APIDiscussionForum data = new APIDiscussionForum();
                            data.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            data.PostThreadId = string.IsNullOrEmpty(row["PostThreadId"].ToString()) ? 0 : int.Parse(row["PostThreadId"].ToString());
                            data.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                            data.PostParentId = string.IsNullOrEmpty(row["PostParentId"].ToString()) ? 0 : int.Parse(row["PostParentId"].ToString());
                            data.PostLevel = string.IsNullOrEmpty(row["PostLevel"].ToString()) ? 0 : int.Parse(row["PostLevel"].ToString());
                            data.SortOrder = string.IsNullOrEmpty(row["SortOrder"].ToString()) ? 0 : int.Parse(row["SortOrder"].ToString());
                            data.SubjectText = row["SubjectText"].ToString();
                            data.UseName = row["UseName"].ToString();
                            data.Date = string.IsNullOrEmpty(row["CreatedDate"].ToString()) ? DateTime.MinValue : DateTime.Parse(row["CreatedDate"].ToString());
                            data.UserProfilePicture = row["ProfilePicture"].ToString();
                            data.UserProfilePicture = string.IsNullOrEmpty(data.UserProfilePicture) ? null : string.Concat(this._configuration["ApiGatewayUrl"], data.UserProfilePicture);
                            data.IsDeleted = string.IsNullOrEmpty(row["IsDeleted"].ToString()) ? false : bool.Parse(row["IsDeleted"].ToString());
                            data.IsCommentAddedByUser = string.IsNullOrEmpty(row["IsCommentAddedByUser"].ToString()) ? false : bool.Parse(row["IsCommentAddedByUser"].ToString());
                            result.Add(data);
                        }
                        connection.Close();
                    }
                }
            }
            return result;

        }
        public async Task<bool> CheckInprogessValidation(int CourseId, int ModifiedBy, string Rolecode)
        {
            CourseCompletionStatus status = new CourseCompletionStatus();
            status = await this.db.CourseCompletionStatus.Where(r => r.CourseId == CourseId && r.UserId == ModifiedBy).FirstOrDefaultAsync();
            if (status == null && String.Compare(Rolecode, "CA") != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> GetParameterValue(string orgCode, string configName)
        {
            string configValue;
            var cache = new CacheManager.CacheManager();
            string cacheKeyConfig = CacheKeyNames.CONFIGURATION_VALUE + "_" + configName + "_" + orgCode;
            if (cache.IsAdded(cacheKeyConfig))
            {
                configValue = cache.Get<string>(cacheKeyConfig);
            }
            else
            {
                configValue = await _courseRepository.GetMasterConfigurableParameterValue(configName);
                _logger.Debug(string.Format("Adding value {0} for cache key {1}", configValue, cacheKeyConfig));
                configValue = string.IsNullOrEmpty(configValue) ? " " : configValue;
                cache.Add(cacheKeyConfig, configValue, System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
            }
            return configValue;
        }
    }
}

