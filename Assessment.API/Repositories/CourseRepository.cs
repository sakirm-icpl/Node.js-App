using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using log4net;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Models;
using Assessment.API.Common;
using Assessment.API.Helper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Assessment.API.Model.EdCastAPI;
using Assessment.API.ExternalIntegration.EdCast;
using static Assessment.API.Models.AssessmentContext;
using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces.EdCast;

namespace Assessment.API.Repositories
{
    public class CourseRepository : Repository<Assessment.API.Model.Course>, ICourseRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRepository));
        private AssessmentContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IDarwinboxConfiguration _darwinboxConfiguration;

        public CourseRepository(AssessmentContext context, ICustomerConnectionStringRepository customerConnection, IHttpContextAccessor httpContextAccessor, IDarwinboxConfiguration darwinboxConfiguration) : base(context)
        {
            this._db = context;
            _httpContextAccessor = httpContextAccessor;
            this._customerConnection = customerConnection;
            this._darwinboxConfiguration = darwinboxConfiguration;

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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return value;
        }


        public async Task<string> GetAssessmentConfigurationID(int? courseId, int? moduleId, string orgCode, bool isPreAssessment = false, bool isContentAssessment = false)
        {
            int? AssessmentSheetConfigID = 0;

            var cache = new CacheManager.CacheManager();
            string cacheKeyConfig = Constants.AssessmentSheetConfigurationId + "-" + orgCode.ToUpper() + Convert.ToString(courseId) + Convert.ToString(moduleId) + Convert.ToString(isPreAssessment) + Convert.ToString(isContentAssessment);
            if (cache.IsAdded(cacheKeyConfig.ToUpper()))
                AssessmentSheetConfigID = Convert.ToInt32(cache.Get<string>(cacheKeyConfig.ToUpper()));
            else
            {
                if (moduleId == 0)
                {
                    if (isPreAssessment)
                    {
                        AssessmentSheetConfigID =
                        await (from cours in _db.Course
                               join assessment in _db.Module on cours.PreAssessmentId equals assessment.Id
                               join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                               where (cours.Id == courseId && cours.IsDeleted == false && assessment.IsDeleted == false)
                               select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                    }
                    else
                    {
                        AssessmentSheetConfigID =
                            await (from cours in _db.Course
                                   join assessment in _db.Module on cours.AssessmentId equals assessment.Id
                                   join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                                   where (cours.Id == courseId && cours.IsDeleted == false && assessment.IsDeleted == false)
                                   select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                    }
                }
                else
                {
                    if (isPreAssessment)
                    {
                        AssessmentSheetConfigID =
                        await (from courseModule in _db.CourseModuleAssociation
                               join assessment in _db.Module on courseModule.PreAssessmentId equals assessment.Id
                               join module in _db.Module on courseModule.ModuleId equals module.Id
                               join course in _db.Course on courseModule.CourseId equals course.Id
                               join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                               where (course.Id == courseId && module.Id == moduleId && course.IsDeleted == false &&
                               module.IsDeleted == false && assessment.IsDeleted == false)
                               select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                    }
                    else if (isContentAssessment)
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
                                        cmd.CommandText = "GetAssessmentConfigIdByModuleIDAndCourseID";
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.BigInt) { Value = moduleId });
                                        cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Bit) { Value = courseId });

                                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                                        DataTable dt = new DataTable();
                                        dt.Load(reader);
                                        if (dt.Rows.Count > 0)
                                        {
                                            foreach (DataRow row in dt.Rows)
                                            {
                                                AssessmentSheetConfigID = Convert.ToInt32(row["AssessmentSheetConfigID"].ToString());
                                            }
                                        }
                                        reader.Dispose();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                            throw (ex);
                        }
                    }
                    else
                    {
                        AssessmentSheetConfigID =
                        await (from courseModule in _db.CourseModuleAssociation
                               join assessment in _db.Module on courseModule.AssessmentId equals assessment.Id
                               join module in _db.Module on courseModule.ModuleId equals module.Id
                               join course in _db.Course on courseModule.CourseId equals course.Id
                               join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                               where (course.Id == courseId && module.Id == moduleId && course.IsDeleted == false &&
                               module.IsDeleted == false && assessment.IsDeleted == false)
                               select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                    }
                }
                _logger.Debug("Adding config key :- " + cacheKeyConfig.ToUpper() + " Value :- " + Convert.ToString(AssessmentSheetConfigID));
                cache.Add(cacheKeyConfig.ToUpper(), Convert.ToString(AssessmentSheetConfigID), DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
            }
            return AssessmentSheetConfigID.ToString();
        }

        public async Task<string> GetManagerAssessmentConfigurationID(int? courseId, int? moduleId)
        {
            int? AssessmentSheetConfigID = 0;
            try
            {
                AssessmentSheetConfigID =
                await (from course in _db.Course
                       join assessment in _db.Module on course.ManagerEvaluationId equals assessment.Id
                       join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                       where (course.Id == courseId && course.IsDeleted == false && assessment.IsDeleted == false)
                       select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return AssessmentSheetConfigID.ToString();
        }

        public async Task<string> GetCourseNam(int? id)
        {
            return await (from c in _db.Course
                          where c.IsDeleted == false && c.Id == id
                          select c.Title).FirstOrDefaultAsync();
        }

        public async Task<DarwinboxTransactionDetails> PostCourseStatusToDarwinbox(int courseID, int userId, string status, string orgcode, string? connectionstring = null)
        {
            APIDarwinTransactionDetails objCourseResponce = new APIDarwinTransactionDetails();
            DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();
            if (!string.IsNullOrEmpty(connectionstring))
                ChangeDbContext(connectionstring);

            string ProvidedUserId = await _db.UserMaster.Where(a => a.Id == userId).Select(a => a.ProvidedUserId).FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(ProvidedUserId))
            {

                obj.Http_method = ConstantEdCast.HTTPMETHOD;
                obj.Payload = null;
                obj.Tran_Status = ConstantEdCast.Trans_Error;
                obj.ResponseMessage = "Invalid User provided";
                obj.CreatedDate = DateTime.UtcNow;
                obj.CreatedBy = userId;
                obj.RequestUrl = "Enroll";
                obj.External_Id = courseID.ToString();
                _db.DarwinboxTransactionDetails.Add(obj);
                await _db.SaveChangesAsync();
                return obj;
            }
            else
            {

                try
                {

                    string url = null;

                    DarwinboxConfiguration darwinconfiguration = new DarwinboxConfiguration();
                    var cache = new CacheManager.CacheManager();
                    string cacheKeyConfig = Constants.DarwinboxConfiguration + "-" + orgcode.ToUpper();
                    if (cache.IsAdded(cacheKeyConfig.ToUpper()))
                    {
                        darwinconfiguration = cache.Get<DarwinboxConfiguration>(cacheKeyConfig.ToUpper());
                    }
                    else
                    {
                        var data = await this._darwinboxConfiguration.GetAll();
                        darwinconfiguration = data.FirstOrDefault();
                        cache.Add(cacheKeyConfig.ToUpper(), darwinconfiguration, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                    }
                    if (darwinconfiguration == null)
                    {

                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = null;
                        obj.Tran_Status = objCourseResponce.status;
                        obj.ResponseMessage = "Please set Darwinbox configuration.";
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = courseID.ToString();
                        _db.DarwinboxTransactionDetails.Add(obj);
                        await _db.SaveChangesAsync();
                        return obj;
                    }


                    List<APIUpdateCourseStatusToDB> emp_Data = new List<APIUpdateCourseStatusToDB>();
                    if (status != "enroll")
                    {

                        int modulecount = await _db.CourseModuleAssociation.Where(a => a.CourseId == courseID).CountAsync();
                        int completedmodulecount = await _db.ModuleCompletionStatus.Where(a => a.CourseId == courseID && a.UserId == userId && a.Status == "completed").CountAsync();
                        string CompletionPer = "5";
                        CompletionPer = Convert.ToString((int)(completedmodulecount / modulecount) * 100);
                        var Query = (from courses in _db.Course
                                     join ccs in _db.CourseCompletionStatus on courses.Id equals ccs.CourseId
                                     join User in _db.UserMaster on ccs.UserId equals User.Id

                                     where courses.IsDeleted == false && courses.Id == courseID && ccs.UserId == userId
                                     select new APIUpdateCourseStatusToDB
                                     {
                                         employee_id = string.IsNullOrEmpty(User.ProvidedUserId) ? "" : Security.Decrypt(User.ProvidedUserId),
                                         activity_id = courses.Code,
                                         start_date = ccs.CreatedDate.ToString(),
                                         complete_date = status == "completed" ? ccs.ModifiedDate.ToString() : null,
                                         action = status == "completed" ? "complete" : "start",
                                         last_updated_on = ccs.ModifiedDate.ToString(),
                                         completion_percentage = CompletionPer
                                     }).AsNoTracking();
                        emp_Data = await Query.Distinct().ToListAsync();
                    }
                    else
                    {
                        var Query = (from courses in _db.Course
                                     where courses.IsDeleted == false && courses.Id == courseID
                                     select new APIUpdateCourseStatusToDB
                                     {
                                         employee_id = string.IsNullOrEmpty(ProvidedUserId) ? "" : Security.Decrypt(ProvidedUserId),
                                         activity_id = courses.Code,
                                         enrolled_on = Convert.ToString(DateTime.UtcNow),
                                         action = status

                                     }).AsNoTracking();
                        emp_Data = await Query.Distinct().ToListAsync();
                    }

                    if (emp_Data.Count() == 0)
                    {

                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = null;
                        obj.Tran_Status = ConstantEdCast.Trans_Error;
                        obj.ResponseMessage = "Employee or course not found.";
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = courseID.ToString();
                        _db.DarwinboxTransactionDetails.Add(obj);
                        await _db.SaveChangesAsync();
                        return obj;
                    }

                    APICourseStatusPostAPIkey aPICourseStatusPostAPIkey = new APICourseStatusPostAPIkey();
                    aPICourseStatusPostAPIkey.api_key = darwinconfiguration.Update_LA;
                    aPICourseStatusPostAPIkey.employees = emp_Data.ToArray();

                    JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPICourseStatusPostAPIkey));
                    url = darwinconfiguration.DarwinboxHost;
                    url = url + "/lmsapi/updateuserlearningactivities";
                    string Body = JsonConvert.SerializeObject(oJsonObject);
                    objCourseResponce = await ApiHelper.PostDarwinboxAPI(url, Body, darwinconfiguration.Username, darwinconfiguration.Password);

                    if (objCourseResponce != null)
                    {
                        if (objCourseResponce.status == "0")
                        {
                            obj.Http_method = ConstantEdCast.HTTPMETHOD;
                            obj.Payload = Body;
                            obj.Tran_Status = objCourseResponce.status;
                            obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.error) : objCourseResponce.message;
                            obj.CreatedDate = DateTime.UtcNow;
                            obj.CreatedBy = userId;
                            obj.RequestUrl = url;
                            obj.External_Id = courseID.ToString();
                            _db.DarwinboxTransactionDetails.Add(obj);
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            if (objCourseResponce.error.Count() == 0)
                            {
                                obj.Http_method = ConstantEdCast.HTTPMETHOD;
                                obj.Payload = Body;
                                obj.Tran_Status = objCourseResponce.status;
                                obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.error) : objCourseResponce.message;
                                obj.CreatedDate = DateTime.UtcNow;
                                obj.CreatedBy = userId;
                                obj.RequestUrl = url;
                                obj.External_Id = courseID.ToString();
                                _db.DarwinboxTransactionDetails.Add(obj);
                                await _db.SaveChangesAsync();

                            }
                            else
                            {


                                obj.Http_method = ConstantEdCast.HTTPMETHOD;
                                obj.Payload = JsonConvert.SerializeObject(emp_Data);
                                obj.Tran_Status = ConstantEdCast.Trans_Error;
                                obj.ResponseMessage = string.Join(",", objCourseResponce.error);
                                obj.CreatedDate = DateTime.UtcNow;
                                obj.CreatedBy = userId;
                                obj.RequestUrl = url;
                                obj.External_Id = courseID.ToString();
                                _db.DarwinboxTransactionDetails.Add(obj);
                                await _db.SaveChangesAsync();

                            }
                        }
                    }


                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return obj;
        }

        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<Assessment.API.Model.Course>();
        }

        public async Task<string> GetModuleName(int? id)
        {
            var Modulename = await (from c in _db.Module
                                    where c.IsDeleted == false && c.Id == id
                                    select c.Name).SingleOrDefaultAsync();
            return Modulename;
        }
    }

   
}