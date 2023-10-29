using log4net;
using System.Data.Common;
using System.Data;
using Feedback.API.Models;
using Courses.API.Repositories.Interfaces;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Feedback.API.Model;
using System.Data.SqlClient;

namespace Courses.API.Repositories
{
    public class ModuleCompletionStatusRepository : Repository<ModuleCompletionStatus>, IModuleCompletionStatusRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleCompletionStatusRepository));
        private FeedbackContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        
        ICourseRepository _courseRepository;
        ICustomerConnectionStringRepository _customerConnection;

        public ModuleCompletionStatusRepository(FeedbackContext context, IConfiguration configuration,
            
            ICourseCompletionStatusRepository courseCompletionStatusRepository,
            ICourseRepository courseRepository,
            ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._configuration = configuration;
            _courseCompletionStatusRepository = courseCompletionStatusRepository;
           
            _courseRepository = courseRepository;
            this._customerConnection = customerConnection;

        }


        public async Task<int> Post(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string? Token = null, string? Orgcode = null)
        {
            try
            {
                ModuleCompletionStatus ExistingModule = await this.Get(moduleCompletionStatus.UserId, moduleCompletionStatus.CourseId, moduleCompletionStatus.ModuleId);
                if (ExistingModule != null)
                {
                    if (ExistingModule.Status == "completed")
                        ExistingModule.Status = "completed";
                    else
                    {
                        ExistingModule.Status = moduleCompletionStatus.Status;
                        ExistingModule.ModifiedDate = DateTime.UtcNow;
                    }
                    await this.Update(ExistingModule);
                }
                else
                {
                    moduleCompletionStatus.CreatedDate = DateTime.UtcNow;
                    await this.Add(moduleCompletionStatus);
                }

                string CourseTypeCompletion = await this._db.Course.Where(a => a.Id == moduleCompletionStatus.CourseId).Select(a => a.CourseType).FirstOrDefaultAsync();

                if (CourseTypeCompletion.ToLower() != "classroom")
                {
                    CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                    courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
                    courseCompletionStatus.UserId = moduleCompletionStatus.UserId;

                    await _courseCompletionStatusRepository.Post(courseCompletionStatus, Orgcode);
                }
                else if (CourseTypeCompletion.ToLower() == "classroom")
                {
                    //--------- Get Configurable Count --------------//
                    string isDELINKING_ILT = null;
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
                                    cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "DELINKING_ILT" });
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
                                        isDELINKING_ILT = (row["Value"].ToString());
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
                    //--------- Get Configurable Count --------------//

                    if (isDELINKING_ILT.ToString().ToLower() == "no")
                    {
                        CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                        courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
                        courseCompletionStatus.UserId = moduleCompletionStatus.UserId;

                        await _courseCompletionStatusRepository.Post(courseCompletionStatus, Orgcode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return 1;
        }

        public async Task<int> PostFeedbackCompletion(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string? Token = null, string? Orgcode = null)
        {
            try
            {
                ModuleCompletionStatus ExistingModule = await this.Get(moduleCompletionStatus.UserId, moduleCompletionStatus.CourseId, moduleCompletionStatus.ModuleId);
                if (ExistingModule != null)
                {
                    if (ExistingModule.Status == "completed")
                        ExistingModule.Status = "completed";
                    else
                    {
                        ExistingModule.Status = moduleCompletionStatus.Status;
                        ExistingModule.ModifiedDate = DateTime.UtcNow;
                    }
                    await this.Update(ExistingModule);
                }
                else
                {
                    moduleCompletionStatus.CreatedDate = DateTime.UtcNow;
                    await this.Add(moduleCompletionStatus);
                }

                //string CourseTypeCompletion = await this._db.Course.Where(a => a.Id == moduleCompletionStatus.CourseId).Select(a => a.CourseType).FirstOrDefaultAsync();

                //if (CourseTypeCompletion.ToLower() != "classroom")
                //{
                //    CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                //    courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
                //    courseCompletionStatus.UserId = moduleCompletionStatus.UserId;

                //    await _courseCompletionStatusRepository.Post(courseCompletionStatus, Orgcode);
                //}

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return 1;
        }


        public async Task<ModuleCompletionStatus> Get(int userId, int courseId, int moduleId)
        {
            IQueryable<ModuleCompletionStatus> Query = _db.ModuleCompletionStatus;
            Query = Query.Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId);
            return await Query.SingleOrDefaultAsync();
        }
    }
}