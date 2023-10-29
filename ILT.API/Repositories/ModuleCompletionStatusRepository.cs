
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
namespace ILT.API.Repositories
{
    public class ModuleCompletionStatusRepository : Repository<ModuleCompletionStatus>, IModuleCompletionStatusRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleCompletionStatusRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
       // private readonly ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        ICourseRepository _courseRepository;
        ICustomerConnectionStringRepository _customerConnection;

        public ModuleCompletionStatusRepository(CourseContext context, IConfiguration configuration,
            ICourseModuleAssociationRepository courseModuleAssociationRepository,
         //   ICourseCompletionStatusRepository courseCompletionStatusRepository,
            ICourseRepository courseRepository,
            ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._configuration = configuration;
          //  _courseCompletionStatusRepository = courseCompletionStatusRepository;
            _courseModuleAssociationRepository = courseModuleAssociationRepository;
            _courseRepository = courseRepository;
            this._customerConnection = customerConnection;

        }
        public async Task<ModuleCompletionStatus> Get(int userId, int courseId, int moduleId)
        {
            IQueryable<ModuleCompletionStatus> Query = _db.ModuleCompletionStatus;
            Query = Query.Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId);
            return await Query.SingleOrDefaultAsync();
        }


        //public async Task<int> Post(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string Token = null, string Orgcode = null)
        //{
        //    try
        //    {
        //        ModuleCompletionStatus ExistingModule = await this.Get(moduleCompletionStatus.UserId, moduleCompletionStatus.CourseId, moduleCompletionStatus.ModuleId);
        //        if (ExistingModule != null)
        //        {
        //            if (ExistingModule.Status == "completed")
        //                ExistingModule.Status = "completed";
        //            else
        //            {
        //                ExistingModule.Status = moduleCompletionStatus.Status;
        //                ExistingModule.ModifiedDate = DateTime.UtcNow;
        //            }
        //            await this.Update(ExistingModule);
        //        }
        //        else
        //        {
        //            moduleCompletionStatus.CreatedDate = DateTime.UtcNow;
        //            await this.Add(moduleCompletionStatus);
        //        }

        //        string CourseTypeCompletion = await this._db.Course.Where(a => a.Id == moduleCompletionStatus.CourseId).Select(a => a.CourseType).FirstOrDefaultAsync();

        //        if (CourseTypeCompletion.ToLower() != "classroom")
        //        {
        //            CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
        //            courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
        //            courseCompletionStatus.UserId = moduleCompletionStatus.UserId;

        //            await _courseCompletionStatusRepository.Post(courseCompletionStatus, Orgcode);
        //        }
        //        else if (CourseTypeCompletion.ToLower() == "classroom")
        //        {
        //            //--------- Get Configurable Count --------------//
        //            string isDELINKING_ILT = null;
        //            try
        //            {
        //                using (var dbContext = this._customerConnection.GetDbContext())
        //                {
        //                    using (var connection = dbContext.Database.GetDbConnection())
        //                    {
        //                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                            connection.Open();
        //                        using (var cmd = connection.CreateCommand())
        //                        {
        //                            cmd.CommandText = "GetConfigurableParameterValue";
        //                            cmd.CommandType = CommandType.StoredProcedure;
        //                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "DELINKING_ILT" });
        //                            DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                            DataTable dt = new DataTable();
        //                            dt.Load(reader);
        //                            if (dt.Rows.Count <= 0)
        //                            {
        //                                reader.Dispose();
        //                                connection.Close();
        //                           }
        //                            foreach (DataRow row in dt.Rows)
        //                            {
        //                                isDELINKING_ILT = (row["Value"].ToString());
        //                            }
        //                            reader.Dispose();
        //                        }
        //                        connection.Close();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error(Utilities.GetDetailedException(ex));
        //                throw ex;
        //            }
        //            //--------- Get Configurable Count --------------//

        //            if (isDELINKING_ILT.ToString().ToLower() == "no")
        //            {
        //                CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
        //                courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
        //                courseCompletionStatus.UserId = moduleCompletionStatus.UserId;

        //                await _courseCompletionStatusRepository.Post(courseCompletionStatus, Orgcode);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }

        //    return 1;
        //}


        public async Task<int> PostFeedbackCompletion(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string Token = null, string Orgcode = null)
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

        //public async Task<int> PostCompletion(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string Token = null, string OrgCode = null,
        //                                                                                                                                string CourseStatusFromSP = null)
        //{
        //    var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
        //    try
        //    {

        //        // <Added by Gaurav for optimization>
        //        if (moduleCompletionStatus.Status == Status.Completed && CourseStatusFromSP == Status.Completed)
        //        {
        //            CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
        //            courseCompletionStatus.CourseId = moduleCompletionStatus.CourseId;
        //            courseCompletionStatus.UserId = moduleCompletionStatus.UserId;
        //            courseCompletionStatus.Status = CourseStatusFromSP;

        //            await _courseCompletionStatusRepository.PostCompletion(courseCompletionStatus, OrgCode);
        //        }
        //        // </Added by Gaurav for optimization>

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        if (connection.State == ConnectionState.Open)
        //            connection.Close();
        //        throw ex;
        //    }
        //    return 1;
        //}


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
                throw ex;
            }
            return ConfigurablevalueYN;
        }
        public async Task<string> CheckDelinkingValue(string token)
        {
            JObject oJsonObject = new JObject();
            string Url = this._configuration[Configuration.MasterApi];
            Url += "/ConfigurableParameters/DELINKING_ILT";
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ApiConfigurableParameters enrollmentType = JsonConvert.DeserializeObject<ApiConfigurableParameters>(result);

                return enrollmentType.Value;
            }
            return "";
        }

        public async Task<bool> Exist(int userId, int courseId, int moduleId)
        {
            if (await this._db.ModuleCompletionStatus.Where(c => c.CourseId == courseId && c.UserId == userId && c.ModuleId == moduleId && c.Status.Equals(Status.Completed)).CountAsync() > 0)
                return true;
            return false;
        }
        public async Task<string> GetStatus(int courseId, int moduleId, int userId)
        {
            string Stat = await this._db.ModuleCompletionStatus.Where(c => c.CourseId == courseId && c.ModuleId == moduleId && c.UserId == userId).Select(m => m.Status).SingleOrDefaultAsync();
            if (Stat != null)
                return Stat;
            else
                return Status.NotStarted;
        }
        public async Task<bool> IsAllModulesCompleted(int courseId, int moduleId, int userId)
        {
            var Course = await (from mc in this._db.ModuleCompletionStatus
                                select new
                                {
                                    CompletedModuleCount = this._db.ModuleCompletionStatus.Where(c => c.CourseId == courseId
                                    && c.ModuleId == moduleId && c.Status.Equals(Status.Completed) && c.UserId == userId).Count(),
                                    TotalModuleCount = this._db.CourseModuleAssociation.Where(c => c.CourseId == courseId).Count()
                                }).FirstOrDefaultAsync();
            if (Course.CompletedModuleCount == Course.TotalModuleCount && Course.CompletedModuleCount != 0)
                return true;
            return false;
        }

        public async Task<List<APIModuleStatus>> GetModuleStatus(int userId, int courseId, int page, int pageSize, string status = null)
        {
            List<APIModuleStatus> moduleStatusList = new List<APIModuleStatus>();
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
                            cmd.CommandText = "GetModuleStatus";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
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
                                if ((row["TrainingType"].ToString() == "Classroom") && (row["ScheduleCode"].ToString() == ""))
                                {
                                    APIModuleStatus aPIModuleStatus = new APIModuleStatus();
                                    aPIModuleStatus.Moduleid = int.Parse(row["Moduleid"].ToString());
                                    aPIModuleStatus.ModuleName = row["ModuleName"].ToString();
                                    aPIModuleStatus.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                    aPIModuleStatus.CompletedDate = string.IsNullOrEmpty(row["CompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CompletedDate"].ToString());
                                    aPIModuleStatus.Status = row["Status"].ToString();
                                    aPIModuleStatus.ModuleType = row["ModuleType"].ToString();
                                    aPIModuleStatus.TrainingType = row["TrainingType"].ToString();
                                    aPIModuleStatus.AssessmentPercentage = "";
                                    aPIModuleStatus.ScheduleCode = "";
                                    aPIModuleStatus.TrainingPlace = "";
                                    aPIModuleStatus.TrainerName = "";
                                    aPIModuleStatus.AcademyAgencyName = "";
                                    aPIModuleStatus.assLevel = "";
                                    aPIModuleStatus.assStatus = "";
                                    moduleStatusList.Add(aPIModuleStatus);
                                }
                                else if ((row["TrainingType"].ToString() == "Classroom") && (!string.IsNullOrEmpty(row["ScheduleCode"].ToString())))
                                {
                                    APIModuleStatus aPIModuleStatus = new APIModuleStatus();
                                    aPIModuleStatus.Moduleid = int.Parse(row["Moduleid"].ToString());
                                    aPIModuleStatus.ModuleName = row["ModuleName"].ToString();
                                    aPIModuleStatus.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                    aPIModuleStatus.CompletedDate = string.IsNullOrEmpty(row["CompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CompletedDate"].ToString());
                                    aPIModuleStatus.Status = row["Status"].ToString();
                                    aPIModuleStatus.ModuleType = row["ModuleType"].ToString();
                                    aPIModuleStatus.TrainingType = row["TrainingType"].ToString();
                                    aPIModuleStatus.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? null : row["AssessmentPercentage"].ToString();
                                    aPIModuleStatus.ScheduleCode = row["ScheduleCode"].ToString();
                                    aPIModuleStatus.TrainingPlace = row["TrainingPlace"].ToString();
                                    aPIModuleStatus.TrainerName = row["TrainerName"].ToString();
                                    aPIModuleStatus.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                    aPIModuleStatus.ScheduleStartDate = string.IsNullOrEmpty(row["ScheduleStartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["ScheduleStartDate"].ToString());
                                    aPIModuleStatus.EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString());
                                    aPIModuleStatus.AssessmentResult = string.IsNullOrEmpty(row["AssessmentResult"].ToString()) ? null : row["AssessmentResult"].ToString();
                                    aPIModuleStatus.assLevel = row["assLevel"].ToString();
                                    aPIModuleStatus.assStatus = row["assStatus"].ToString();
                                    moduleStatusList.Add(aPIModuleStatus);
                                }
                                else
                                {
                                    APIModuleStatus aPIModuleStatus = new APIModuleStatus();
                                    aPIModuleStatus.Moduleid = int.Parse(row["Moduleid"].ToString());
                                    aPIModuleStatus.ModuleName = row["ModuleName"].ToString();
                                    aPIModuleStatus.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                    aPIModuleStatus.CompletedDate = string.IsNullOrEmpty(row["CompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CompletedDate"].ToString());
                                    aPIModuleStatus.Status = row["Status"].ToString();
                                    aPIModuleStatus.ModuleType = row["ModuleType"].ToString();
                                    aPIModuleStatus.TrainingType = row["TrainingType"].ToString();
                                    aPIModuleStatus.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? null : row["AssessmentPercentage"].ToString();
                                    aPIModuleStatus.AssessmentResult = string.IsNullOrEmpty(row["AssessmentResult"].ToString()) ? null : row["AssessmentResult"].ToString();
                                    aPIModuleStatus.assLevel = row["assLevel"].ToString();
                                    aPIModuleStatus.assStatus = row["assStatus"].ToString();
                                    moduleStatusList.Add(aPIModuleStatus);
                                }
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return moduleStatusList;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetModuleCount(int userId, int courseId, string status = null)
        {
            return await (from moduleCompletion in this._db.ModuleCompletionStatus
                          join module in this._db.Module on moduleCompletion.ModuleId equals module.Id
                          where (moduleCompletion.UserId == userId && moduleCompletion.CourseId == courseId
                          && (status == null || moduleCompletion.Status.ToLower() == status.ToLower()))
                          select new { module.Id }).CountAsync();
        }
    }
}
