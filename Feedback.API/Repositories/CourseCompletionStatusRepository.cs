using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using log4net;
using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Services;
using Feedback.API.Common;
using Feedback.API.Models;
using Feedback.API.Helper;
using Feedback.API.APIModel;

namespace Feedback.API.Repositories
{
    public class CourseCompletionStatusRepository : Repository<CourseCompletionStatus>, ICourseCompletionStatusRepository
    {
        private FeedbackContext _db;
        ICourseRepository _courseRepository;
        
        private IConfiguration _configuration;
       
        IRewardsPointRepository _rewardsPointRepository;
       
        IEmail _email;
        ICustomerConnectionStringRepository _customerConnection;
        IIdentityService _identitySv;
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseCompletionStatusRepository));

        public CourseCompletionStatusRepository(FeedbackContext context,
            ICourseRepository courseRepository,
            
            IConfiguration configuration,
            IRewardsPointRepository rewardsPointRepository
           
           
            , IEmail email
            , IIdentityService identitySv
           
            , ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this._db = context;
            this._courseRepository = courseRepository;
           
            this._configuration = configuration;
           
            this._rewardsPointRepository = rewardsPointRepository;
            _email = email;
            _identitySv = identitySv;
            
            this._customerConnection = customerConnection;
        }

        public async Task<int> Post(CourseCompletionStatus courseCompletionStatus, string? OrgCode = null)
        {
            try
            {

                CourseCompletionStatus ExistingCourseCompletionStatus = await this.Get(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                String CourseStatus = await this.GetCourseCompletionStatus(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                if (ExistingCourseCompletionStatus == null)
                {
                    courseCompletionStatus.Status = CourseStatus;
                    courseCompletionStatus.CreatedDate = DateTime.Now;
                    courseCompletionStatus.ModifiedDate = DateTime.Now;
                    await this.Add(courseCompletionStatus);
                }
                else
                {
                    if (ExistingCourseCompletionStatus.Status == Status.Completed)
                        ExistingCourseCompletionStatus.Status = Status.Completed;
                    else
                    {
                        ExistingCourseCompletionStatus.Status = CourseStatus;
                        ExistingCourseCompletionStatus.ModifiedDate = DateTime.Now;
                    }
                    await this.Update(ExistingCourseCompletionStatus);
                }

                if (CourseStatus == Status.Completed)
                {

                    Model.Course Course = await _courseRepository.Get(courseCompletionStatus.CourseId);
                    await this._rewardsPointRepository.AddCourseReward(Course, courseCompletionStatus.UserId, OrgCode);
                    DateTime CourseAssignedDate = await this.GetCourseAssignedDate(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                    CourseCompletionStatus completion = await this.Get(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                    int DateDifference = (completion.ModifiedDate.Date - CourseAssignedDate.Date).Days;
                    if (DateDifference != 0 || DateDifference == 0 && Course.IsApplicableToAll == false)
                    {
                        await this._rewardsPointRepository.AddCourseCompletionBaseOnDate(Course, courseCompletionStatus.UserId, DateDifference, OrgCode);
                    }
                    else
                    {
                        DateTime CreatedDate = Course.CreatedDate;
                        CourseCompletionStatus completion1 = await this.Get(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                        int DateDifferenceForCreated = (completion1.ModifiedDate.Date - CreatedDate.Date).Days;
                        if (Course.IsApplicableToAll == true && DateDifferenceForCreated != 0 || DateDifference == 0)
                        {

                            await this._rewardsPointRepository.AddCourseCompletionBaseOnDate(Course, courseCompletionStatus.UserId, DateDifferenceForCreated, OrgCode);
                        }
                    }
                    string COURSE_COMPLETION_EMAIL = await this.GetBoolConfigurablevalue("COURSE_COMPLETION_EMAIL");

                    if (COURSE_COMPLETION_EMAIL.ToLower().ToString() == "yes")
                    {
                        APIUserDetails aPIUserDetails = await this.GetDetailsUserWise(courseCompletionStatus.UserId, courseCompletionStatus.CourseId);
                        if (aPIUserDetails != null)
                            Task.Run(() => _email.SendCourseCompletionStatusMail(Course.Title, courseCompletionStatus.UserId, aPIUserDetails, courseCompletionStatus.CourseId));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }

        public async Task<DateTime> GetCourseAssignedDate(int userId, int courseId)
        {
            DateTime? CourseAssignedDate = null;
            try
            {
                var connection = this._db.Database.GetDbConnection();
                var Course = await (from r in this._db.AccessibilityRule
                                    select new
                                    {
                                        CourseAssignedDate = FeedbackContext.GetCourseAssignedDateForRewardPoints(userId, courseId)
                                    }).FirstOrDefaultAsync();

                DateTime dateTime = Convert.ToDateTime(Course.CourseAssignedDate);
                return dateTime;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return Convert.ToDateTime(CourseAssignedDate);
        }

        public async Task<CourseCompletionStatus> Get(int userId, int courseId)
        {
            IQueryable<CourseCompletionStatus> Query = _db.CourseCompletionStatus;
            Query = Query.AsNoTracking().Where(r => r.UserId.Equals(userId) && r.CourseId == courseId);
            Query = Query.OrderByDescending(r => r.Id);
            return await Query.FirstOrDefaultAsync();
        }

        public async Task<string> GetBoolConfigurablevalue(string configurableparameter)
        {
            string ConfigurablevalueYN = "No";
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
                            cmd.CommandText = "GetConfigurableParameterValue";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = configurableparameter });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                ConfigurablevalueYN = string.IsNullOrEmpty(row["Value"].ToString()) ? null : row["Value"].ToString();
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
            return ConfigurablevalueYN;
        }

        public async Task<APIUserDetails> GetDetailsUserWise(int userid, int CourseId)
        {

            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                APIUserDetails apiUserDetails = null;
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetUserMasterInfoForMail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userid });
                    cmd.Parameters.Add(new SqlParameter("@courseid", SqlDbType.Int) { Value = CourseId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            apiUserDetails = new APIUserDetails
                            {
                                EmailId = Security.Decrypt(row["EmailId"].ToString()),
                                UserName = row["UserName"].ToString(),
                                CustomerCode = row["CustomerCode"].ToString(),
                            };
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
                return apiUserDetails;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        public async Task<string> GetCourseCompletionStatus(int userId, int courseId)
        {
            string CourseStatus = string.Empty;
            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "IsCourseCompleted";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    foreach (DataRow row in dt.Rows)
                    {
                        CourseStatus = row["CourseStatus"].ToString();
                    }
                    reader.Dispose();
                }
                connection.Close();
                return CourseStatus;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
        }
    }
}