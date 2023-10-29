using TNA.API.Common;
using TNA.API.Helper;
using TNA.API.Model;
//using TNA.API.Models;
using TNA.API.Repositories.Interfaces;
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
//using TNA.API.Model.ILT;
using TNA.API.ExternalIntegration.EdCast;
using TNA.API.Model.EdCastAPI;
using Microsoft.AspNetCore.Hosting;
//using Courses.API.Repositories.Interfaces.EdCast;
using static TNA.API.Model.CourseContext;
using TNA.API.APIModel;

namespace TNA.API.Repositories
{
    public class AccessibiltyRuleRepository : Repository<AccessibilityRule>, IAccessibilityRule
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
      //  private readonly IAccessibilityRuleRejectedRepository _accessibilityRuleRejectedRepository;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        private readonly ITLSHelper _tlsHelper;
        private readonly INotification _notification;
        private readonly IHostingEnvironment _hostingEnvironment;
        IEdCastTransactionDetails _edCastTransactionDetails;
        
        public AccessibiltyRuleRepository(CourseContext context, IConfiguration configuration, IHostingEnvironment hostingEnvironment, IEdCastTransactionDetails edCastTransactionDetails,/* IAccessibilityRuleRejectedRepository accessibilityRuleRejectedRepository,*/ ICustomerConnectionStringRepository customerConnectionRepository, ITLSHelper tlsHelper, INotification notification) : base(context)
        {
            _configuration = configuration;
            _db = context;
          //  _accessibilityRuleRejectedRepository = accessibilityRuleRejectedRepository;
            _customerConnectionRepository = customerConnectionRepository;
            _tlsHelper = tlsHelper;
            _notification = notification;
            _hostingEnvironment = hostingEnvironment;
            _edCastTransactionDetails = edCastTransactionDetails;
          
        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<AccessibilityRule>();
        }
        public async Task<bool> Exists(string name)
        {
            return false;
        }
        public async Task<List<object>> Get(int page, int pageSize, string search = null, string filter = null)
        {
            
            var Query = _db.AccessibilityRule.Join(_db.Course, r => r.CourseId, (p => p.Id), (r, p) => new { IsDeleted=r.IsDeleted, ModifiedBy=r.ModifiedBy,                
                    CreatedBy =r.CreatedBy,
                    Id=p.Id,
                    CourseId=r.CourseId,
                    Title = p.Title,
                    Code= p.Code,
                    IsActive=p.IsActive,
            }).Join(
            _db.UserMaster,
            capp => capp.ModifiedBy,
            user => user.Id,
            (capp, user) => new
            { 
                UserName = user.UserName,             
                CreatedBy = capp.CreatedBy,
                IsDeleted = capp.IsDeleted,
                ModifiedBy = capp.ModifiedBy,              
                Title = capp.Title,
                Id = capp.Id,               
                CourseId = capp.CourseId,                
                Code = capp.Code,
                IsActive = capp.IsActive,
            }).Join(
            _db.UserMasterDetails,
            capp => capp.CreatedBy,
            userd => userd.UserMasterId,
            (capp, userd) => new
            {
                CreatedBy = capp.CreatedBy,
                BusinessId = userd.BusinessId,
                GroupId = userd.GroupId,
                LocationId = userd.LocationId,
                AreaId = userd.AreaId,                
                IsDeleted = capp.IsDeleted,
                ModifiedBy = capp.ModifiedBy,
                Title = capp.Title,
                Id = capp.Id,
                CourseId = capp.CourseId,
                Code = capp.Code,
                IsActive = capp.IsActive,
            }
        ).Where(c => (c.IsDeleted == false) && (search == null || c.Title.StartsWith(search)) && c.IsDeleted == false) // && c.p.IsActive==true)
                        .GroupBy(od => new
                        {
                            od.Id,
                            od.CourseId,
                            od.Title,
                            od.Code,
                            od.IsActive
                        })
                     .OrderByDescending(a => a.Key.Id)
                       .Select(m => new APICourseAccessibilityRule
                       {
                           Id = m.Key.Id,
                           CourseId = m.Key.CourseId,
                           Title = m.Key.Title,
                           courseCode = m.Key.Code,
                           courseStatus = m.Key.IsActive
                       });

            if (page != -1)
                Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
            if (pageSize != -1)
                Query = Query.Take(Convert.ToInt32(pageSize));

            return await Query.ToListAsync<object>();
        }

        public async Task<ApplicabilityTotalAPI> GetV2(APICourseApplicability obj,int userId,string userRole)
        {
            ApplicabilityTotalAPI applicabilityTotalAPI = new ApplicabilityTotalAPI();
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            IQueryable< APICourseAccessibilityRule> Query =  _db.AccessibilityRule.Join(_db.Course, r => r.CourseId, (p => p.Id), (r, p) => new {
                IsDeleted = r.IsDeleted,
                ModifiedBy = p.ModifiedBy,
                CreatedBy = p.CreatedBy,
                Id = p.Id,
                CourseId = r.CourseId,
                Title = p.Title,
                Code = p.Code,
                IsActive = p.IsActive,
            }).Join(
           
            _db.UserMasterDetails,
            capp => capp.CreatedBy,
            userd => userd.UserMasterId,
            (capp, userd) => new
            {
                CreatedBy = capp.CreatedBy,
                BusinessId = userd.BusinessId,
                GroupId = userd.GroupId,
                LocationId = userd.LocationId,
                AreaId = userd.AreaId,
                IsDeleted = capp.IsDeleted,
                ModifiedBy = capp.ModifiedBy,
                Title = capp.Title,
                Id = capp.Id,
                CourseId = capp.CourseId,
                Code = capp.Code,
                IsActive = capp.IsActive               
            }
        ).Where(c => (c.IsDeleted == false) && (obj.search == null || c.Title.StartsWith(obj.search)) && c.IsDeleted == false) // && c.p.IsActive==true)
                        .GroupBy(od => new
                        {
                            od.Id,
                            od.CourseId,
                            od.Title,
                            od.Code,
                            od.IsActive,
                            od.LocationId,
                            od.GroupId,
                            od.BusinessId,
                            od.AreaId,
                            od.ModifiedBy,
                            od.CreatedBy
                        })
                     .OrderByDescending(a => a.Key.Id)
                       .Select(m => new APICourseAccessibilityRule
                       {
                           Id = m.Key.Id,
                           CourseId = m.Key.CourseId,
                           Title = m.Key.Title,
                           courseCode = m.Key.Code,
                           courseStatus = m.Key.IsActive,
                           LocationId = m.Key.LocationId,   
                           BusinessId  = m.Key.BusinessId,
                           AreaId= m.Key.AreaId,
                           GroupId = m.Key.GroupId,
                           CreatedBy = m.Key.CreatedBy
                       });

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
          
            if (obj.showAllData == false && (userRole != UserRoles.CA))
            {
                Query = Query.Where(r => r.CreatedBy == userId);
            }

            applicabilityTotalAPI.TotalRecords = await Query.CountAsync();

            if (obj.page != -1)
                Query = Query.Skip((Convert.ToInt32(obj.page) - 1) * Convert.ToInt32(obj.pageSize));
            if (obj.pageSize != -1)
                Query = Query.Take(Convert.ToInt32(obj.pageSize));

            List<APICourseAccessibilityRule> ruledata = new List<APICourseAccessibilityRule>();
            ruledata = await Query.ToListAsync();
            applicabilityTotalAPI.Data = ruledata;
            return applicabilityTotalAPI;
        }
        public async Task<List<object>> GetCategoryData(int page, int pageSize, string search = null, string filter = null)
        {
            try
            {

                var Query = (from accessibiltyRule in _db.AccessibilityRule
                             join category in _db.Category on accessibiltyRule.CategoryId equals category.Id
                             join subcategory in _db.SubCategory on accessibiltyRule.SubCategoryId equals subcategory.Id
                             into sc
                             from SubCategory in sc.DefaultIfEmpty()
                             where accessibiltyRule.IsDeleted == false && (search == null || category.Name.StartsWith(search))
                             select new APICategoryAccessibilityRule
                             {
                                 CategoryCode = category.Code,
                                 CategoryId = category.Id,
                                 Name = category.Name,
                                 CategoryStatus = accessibiltyRule.IsActive,
                                 SubCategoryId = SubCategory.Id == null ? 0 : SubCategory.Id,
                                 SubCategoryCode = SubCategory.Code == null ? "-" : SubCategory.Code
                             }); ;
                Query = Query.Distinct().OrderByDescending(a => a.CategoryId);

                if (page != -1)
                    Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
                if (pageSize != -1)
                    Query = Query.Take(Convert.ToInt32(pageSize));


                return await Query.ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        public async Task<List<UserGroup>> GetAllUserGroups()
        {
            List<UserGroup> userGroups = new List<UserGroup>();

            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllUserGroups";
                            cmd.CommandType = CommandType.StoredProcedure;

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    UserGroup userGroup = new UserGroup();
                                    userGroup.Id = Convert.ToInt32(row["Id"].ToString());
                                    userGroup.GroupName = row["GroupName"].ToString();
                                    userGroup.UserMasterId = Convert.ToInt32(row["UserMasterId"].ToString());
                                    userGroup.RowGUID = row["RowGUID"].ToString();
                                    userGroup.CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString());
                                    userGroup.ModifiedBy = Convert.ToInt32(row["ModifiedBy"].ToString());
                                    userGroup.CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString());
                                    userGroup.ModifiedDate = Convert.ToDateTime(row["ModifiedDate"].ToString());
                                    userGroup.IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString());
                                    userGroup.IsActive = Convert.ToBoolean(row["IsActive"].ToString());
                                    userGroups.Add(userGroup);
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
            return userGroups;
        }

        public async Task<int> count(string search = null, string filter = null)
        {
            var Query = (from accessibiltyRule in _db.AccessibilityRule
                         join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                         join user in _db.UserMaster on accessibiltyRule.CreatedBy equals user.Id
                         join umd in _db.UserMasterDetails on user.Id equals umd.UserMasterId
                         where accessibiltyRule.IsDeleted == false && (search == null || course.Title.StartsWith(search)) && course.IsDeleted == false //&& course.IsActive == true
                         select new { accessibiltyRule.Id, accessibiltyRule.CourseId, course.Title }

                         );


            return await Query.Select(r => r.Title).Distinct().CountAsync();
        }

        public async Task<int> categorycount(string search = null, string filter = null)
        {
            var Query = (from accessibiltyRule in _db.AccessibilityRule
                         join category in _db.Category on accessibiltyRule.CategoryId equals category.Id
                         join subcategory in _db.SubCategory on accessibiltyRule.SubCategoryId equals subcategory.Id
                         into sc
                         from SubCategory in sc.DefaultIfEmpty()
                         where accessibiltyRule.IsDeleted == false && (search == null || category.Name.StartsWith(search))
                         select new APICategoryAccessibilityRule
                         {
                             CategoryCode = category.Code,
                             CategoryId = category.Id,
                             Name = category.Name,
                             CategoryStatus = accessibiltyRule.IsActive,
                             SubCategoryId = SubCategory.Id,
                             SubCategoryCode = string.IsNullOrEmpty(SubCategory.Code) == null ? "-" : SubCategory.Code
                         }); ;
            Query = Query.Distinct().OrderByDescending(a => a.CategoryId);   //into t1

            return await Query.CountAsync();
        }


        public async Task<AccessibilityRule> GetAccessibility(int id)
        {
            AccessibilityRule accessibilityRules = await (from accessibilityRule in _db.AccessibilityRule.AsNoTracking()
                                                          join c in this._db.Course on accessibilityRule.CourseId equals c.Id into course
                                                          from courses in course.DefaultIfEmpty()
                                                          where (accessibilityRule.Id == id)
                                                          select accessibilityRule).SingleOrDefaultAsync();
            return accessibilityRules;
        }

        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }

        public async Task<List<AccessibilityRules>> SelfEnroll(APIAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null)
        {
            string url = "";
            List<AccessibilityRules> Duplicates = new List<AccessibilityRules>();
            AccessibilityRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
            AccessibilityRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
            if (AndAccessibilityRules.Count() > 0)
            {

                foreach (AccessibilityRules accessibility in AndAccessibilityRules)
                {
                    AccessibilityRule accessibilityRules = new AccessibilityRule
                    {
                        CourseId = apiAccessibility.CourseId,
                        ConditionForRules = "and",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = userId

                    };
                    if (accessibility.AccessibilityRule.ToLower().Equals("userid"))
                        accessibilityRules.UserID = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("userteamid"))
                        accessibilityRules.UserTeamId = Convert.ToInt32(accessibility.ParameterValue);

                    if (await RuleExist(accessibilityRules))
                    {
                        Duplicates.Add(AndAccessibilityRules[0]);
                    }
                    else
                    {
                        await this.Add(accessibilityRules);
                        #region "Send Email Notifications" 
                        var configParameter = await GetMasterConfigurableParameterValue("SELFENROLL_EMAIL");

                        if (Convert.ToString(configParameter).ToLower() == "yes")
                        {
                           
                            url = _configuration[Configuration.NotificationApi];
                            url = url + "/CourseEnrollement";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                            oJsonObject.Add("OrganizationCode", orgnizationCode);
                            oJsonObject.Add("UserId", userId);
                            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;
                            
                        }
                        #endregion


                    }
                }
            }
            if (OrAccessibilityRules.Count() > 0)
            {
                foreach (AccessibilityRules accessibility in OrAccessibilityRules)
                {
                    AccessibilityRule accessibilityRule = new AccessibilityRule
                    {
                        CourseId = apiAccessibility.CourseId
                    };
                    if (!accessibility.Condition.Equals("null"))
                        accessibilityRule.ConditionForRules = "or";
                    accessibilityRule.CreatedDate = DateTime.UtcNow;
                    bool RecordExist = false;
                    string columnName = accessibility.AccessibilityRule.ToLower();
                    var Query = _db.AccessibilityRule.Where(a => a.CourseId == apiAccessibility.CourseId && a.IsDeleted == false);
                    switch (columnName)
                    {
                        case "userid":
                            accessibilityRule.UserID = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.UserID == accessibilityRule.UserID && x.IsDeleted == Record.NotDeleted);
                            break;
                    }
                    RecordExist = Query.Count() > 0 ? true : false;
                    if (!RecordExist)
                    {
                        await this.Add(accessibilityRule);

                        #region "Send Email Notifications" 
                       
                        var configParameter =await GetMasterConfigurableParameterValue("SELFENROLL_EMAIL");
                        if (Convert.ToString(configParameter).ToLower() == "yes")
                        {
                            url = _configuration[Configuration.NotificationApi];
                            url = url + "/CourseEnrollement";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", accessibilityRule.CourseId);
                            oJsonObject.Add("OrganizationCode", orgnizationCode);
                            oJsonObject.Add("UserId", userId);
                            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;
                            
                        }
                        #endregion
                    }
                    else
                    {
                        Duplicates.Add(accessibility);                        
                    }
                }
            }
            if (Duplicates.Count > 0)
                return Duplicates;
            return null;
        }

        public async Task<List<AccessibilityRules>> DbSelfEnroll(APIAccessibility apiAccessibility, int userId, string connectionString = null)
        {
            ChangeDbContext(connectionString);

            List<AccessibilityRules> Duplicates = new List<AccessibilityRules>();
                AccessibilityRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
                AccessibilityRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
               
                if (OrAccessibilityRules.Count() > 0)
                {
                    foreach (AccessibilityRules accessibility in OrAccessibilityRules)
                    {
                        AccessibilityRule accessibilityRule = new AccessibilityRule
                        {
                            CourseId = apiAccessibility.CourseId
                        };
                        if (!accessibility.Condition.Equals("null"))
                            accessibilityRule.ConditionForRules = "or";
                        accessibilityRule.CreatedDate = DateTime.UtcNow;
                        bool RecordExist = false;
                        string columnName = accessibility.AccessibilityRule.ToLower();
                        var Query = _db.AccessibilityRule.Where(a => a.CourseId == apiAccessibility.CourseId && a.IsDeleted == false);
                        switch (columnName)
                        {
                            case "userid":
                                accessibilityRule.UserID = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.UserID == accessibilityRule.UserID && x.IsDeleted == Record.NotDeleted);
                                break;
                        }
                        RecordExist = Query.Count() > 0 ? true : false;
                        if (!RecordExist)
                        {
                             await this.Add(accessibilityRule);
                        }
                        else
                        {
                            Duplicates.Add(accessibility);
                        }
                    }
                }
                if (Duplicates.Count > 0)
                    return Duplicates;
                return null;
           
        }

        public async Task<List<AccessibilityRules>> Post(APIAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null)
        {
            string url = "";
            string urlSMS = "";
            List<AccessibilityRules> Duplicates = new List<AccessibilityRules>();
            AccessibilityRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
            AccessibilityRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
            if (AndAccessibilityRules.Count() > 0)
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    AccessibilityRule accessibilityRules = new AccessibilityRule
                    {
                        CourseId = apiAccessibility.CourseId,
                        ConditionForRules = "and",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = userId,
                        ModifiedBy = userId,
                        IsActive = true

                    };
                    if (!string.IsNullOrEmpty(apiAccessibility.GroupTemplateId))
                    {
                        accessibilityRules.GroupTemplateId = Convert.ToInt32(apiAccessibility.GroupTemplateId);
                    }
                    else
                    {
                        accessibilityRules.GroupTemplateId = null;
                    }
                    if (apiAccessibility.EdCast_due_date!=null)
                    {
                        accessibilityRules.EdCast_due_date = apiAccessibility.EdCast_due_date;
                    }
                    else
                    {
                        accessibilityRules.EdCast_due_date = null;
                    }
                    if (apiAccessibility.EdCast_assigned_date != null)
                    {
                        accessibilityRules.EdCast_assigned_date = apiAccessibility.EdCast_assigned_date;
                    }
                    else
                    {
                        accessibilityRules.EdCast_assigned_date = null;
                    }

                    foreach (AccessibilityRules accessibility in AndAccessibilityRules)
                    {
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn1"))
                            accessibilityRules.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn2"))
                            accessibilityRules.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn3"))
                            accessibilityRules.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn4"))
                            accessibilityRules.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn5"))
                            accessibilityRules.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn6"))
                            accessibilityRules.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn7"))
                            accessibilityRules.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn8"))
                            accessibilityRules.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn9"))
                            accessibilityRules.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn10"))
                            accessibilityRules.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn11"))
                            accessibilityRules.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn12"))
                            accessibilityRules.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("area"))
                            accessibilityRules.Area = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("business"))
                            accessibilityRules.Business = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("emailid"))
                            accessibilityRules.EmailID = accessibility.ParameterValue;
                        if (accessibility.AccessibilityRule.ToLower().Equals("location"))
                            accessibilityRules.Location = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("group"))
                            accessibilityRules.Group = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("userid"))
                            accessibilityRules.UserID = Convert.ToInt32(accessibility.ParameterValue);
                        if (accessibility.AccessibilityRule.ToLower().Equals("mobilenumber"))
                            accessibilityRules.MobileNumber = accessibility.ParameterValue;
                        if (accessibility.AccessibilityRule.ToLower().Equals("dateofjoining"))
                        {
                            accessibilityRules.StartDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue).Date;
                            accessibilityRules.EndDateOfJoining = Convert.ToDateTime(accessibility.ParameterValue2).Date;
                        }
                        if (accessibility.AccessibilityRule.ToLower().Equals("userteamid"))
                            accessibilityRules.UserTeamId = Convert.ToInt32(accessibility.ParameterValue);
                    }

                    if (await RuleExist(accessibilityRules))
                    {
                        Duplicates.Add(AndAccessibilityRules[0]);
                    }
                    else
                    {
                        await this.Add(accessibilityRules);

                        if (accessibilityRules.UserID != null || accessibilityRules.EmailID != null)
                        {
                            var enable_Edcast = await GetMasterConfigurableParameterValue("Enable_Edcast");
                            _logger.Debug("Enable_Edcast :-" + enable_Edcast);

                            string LxpDetails = null;
                            LxpDetails = await _db.Course.Where(a => a.Id == (int)accessibilityRules.CourseId).Select(a => a.LxpDetails).FirstOrDefaultAsync();

                            if (Convert.ToString(enable_Edcast).ToLower() == "yes" && !string.IsNullOrEmpty(LxpDetails))
                            {
                                
                                APIEdcastDetailsToken result = await GetEdCastToken(LxpDetails);
                                if (result != null)
                                {
                                    string assignmentStatus = "assigned";
                                    if (accessibilityRules.UserID == null)
                                    {
                                        accessibilityRules.UserID = Convert.ToInt32(accessibilityRules.EmailID);
                                    }
                                    string assigneddate = accessibilityRules.EdCast_assigned_date.HasValue? accessibilityRules.EdCast_assigned_date.Value.ToString("yyyy-MM-dd"):null ;
                                    string duedate = accessibilityRules.EdCast_due_date.HasValue ? accessibilityRules.EdCast_due_date.Value.ToString("yyyy-MM-dd") : null;

                                    APIEdCastTransactionDetails obj = await CourseAssignment((int)accessibilityRules.CourseId, (int)accessibilityRules.UserID, assignmentStatus, assigneddate, duedate, result.access_token,LxpDetails);
                                }
                                else
                                {
                                    _logger.Debug("Token null from edcast");
                                }
                            }
                        }

                        #region "Send Email Notifications" 
                        url = _configuration[Configuration.NotificationApi];

                        url = url + "/CourseApplicability";
                        JObject oJsonObject = new JObject();
                        oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                        oJsonObject.Add("OrganizationCode", orgnizationCode);
                        HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;
                        #endregion

                        #region "Send Bell Notifications"

                        List<CourseApplicablityEmails> Emails = await this.GetCourseApplicabilityEmails(Convert.ToInt32(accessibilityRules.CourseId), orgnizationCode);

                        var Title = dbContext.Course.Where(a => a.Id == accessibilityRules.CourseId).Select(a => a.Title).SingleOrDefault();
                        bool IsApplicableToAll = dbContext.Course.Where(a => a.Id == accessibilityRules.CourseId).Select(a => a.IsApplicableToAll).SingleOrDefault();
                        int notificationID = 0;

                        List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId));

                        if (aPINotification.Count > 0)
                            notificationID = aPINotification.FirstOrDefault().Id;
                        else
                        {
                            ApiNotification Notification = new ApiNotification();
                            Notification.Title = Title;
                            Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                            Notification.Message = Notification.Message.Replace("{course}", Title);
                            if (accessibilityRules.CourseId != null)
                            {
                                Model.Course course = _db.Course.Where(a => a.Id == accessibilityRules.CourseId).FirstOrDefault();
                                if (course.Code.Contains("urn") || course.Code.Contains("SkillSoft") || course.Code.ToLower().Contains("zobble"))
                                {
                                    Notification.Url = course.CourseURL;
                                }
                                else
                                {
                                    Notification.Url = TlsUrl.NotificationAPost + accessibilityRules.CourseId;
                                }
                            }
                           // Notification.Url = TlsUrl.NotificationAPost + accessibilityRules.CourseId;
                            Notification.Type = Record.Course;
                            Notification.UserId = userId;
                            Notification.CourseId = accessibilityRules.CourseId;
                            notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll);
                        }
                        DataTable dtUserIds = new DataTable();
                        dtUserIds.Columns.Add("UserIds");

                        foreach (var result in Emails)
                        {
                            dtUserIds.Rows.Add(result.UserId);
                        }
                        if (dtUserIds.Rows.Count > 0)
                            await this.SendDataForApplicableNotifications(notificationID, dtUserIds, userId);
                        #endregion

                        #region "Send Push Notifications"
                        url = _configuration[Configuration.NotificationApi];
                        url += "/CourseApplicabilityPushNotification";
                        JObject Pushnotification = new JObject();
                        Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                        Pushnotification.Add("OrganizationCode", orgnizationCode);
                       
                        HttpResponseMessage responses1 = CallAPI(url, Pushnotification).Result;
                        #endregion

                        #region "Send SMS Notifications"
                        var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            urlSMS = _configuration[Configuration.NotificationApi];
                            urlSMS += "/CourseApplicabilitySMS";
                            JObject oJsonObjectSMS = new JObject();
                            oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                            oJsonObjectSMS.Add("OrganizationCode", orgnizationCode);
                            HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                        }
                        #endregion

                    }
                }
            }
            if (OrAccessibilityRules.Count() > 0)
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    foreach (AccessibilityRules accessibility in OrAccessibilityRules)
                    {
                        AccessibilityRule accessibilityRule = new AccessibilityRule
                        {
                            CourseId = apiAccessibility.CourseId
                        };

                        if (!string.IsNullOrEmpty(apiAccessibility.GroupTemplateId))
                        {
                            accessibilityRule.GroupTemplateId = Convert.ToInt32(apiAccessibility.GroupTemplateId);
                        }
                        else
                        {
                            accessibilityRule.GroupTemplateId = null;
                        }

                        if (!accessibility.Condition.Equals("null"))
                            accessibilityRule.ConditionForRules = "or";
                        accessibilityRule.CreatedDate = DateTime.UtcNow;
                        bool RecordExist = false;
                        string columnName = accessibility.AccessibilityRule.ToLower();
                        var Query = dbContext.AccessibilityRule.Where(a => a.CourseId == apiAccessibility.CourseId && a.IsDeleted == false);
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

                            case "userteamid":
                                accessibilityRule.UserTeamId = Convert.ToInt32(accessibility.ParameterValue);
                                Query = Query.Where(x => x.UserTeamId == accessibilityRule.UserTeamId && x.IsDeleted == Record.NotDeleted);
                                break;
                        }
                        RecordExist = Query.Count() > 0 ? true : false;
                        if (!RecordExist)
                        {
                            await this.Add(accessibilityRule);

                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicability";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", accessibilityRule.CourseId);
                            oJsonObject.Add("OrganizationCode", orgnizationCode);
                            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("CourseId", accessibilityRule.CourseId);
                            Pushnotification.Add("OrganizationCode", orgnizationCode);
                           
                            HttpResponseMessage responses1 = CallAPI(url, Pushnotification).Result;

                            int notificationUserId = Convert.ToInt32(accessibilityRule.UserID);

                            List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRule.CourseId));
                            if (aPINotification != null)
                            {
                                var Title = dbContext.Course.Where(a => a.Id == accessibilityRule.CourseId).Select(a => a.Title).SingleOrDefault();
                                bool IsApplicableToAll = dbContext.Course.Where(a => a.Id == accessibilityRule.CourseId).Select(a => a.IsApplicableToAll).SingleOrDefault();

                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = Title;
                                Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                Notification.Message = Notification.Message.Replace("{course}", Title);
                                Notification.Url = TlsUrl.NotificationAPost + accessibilityRule.CourseId;
                                Notification.Type = Record.Course;

                                int NotificationId = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll);

                                DataTable dtUserIds = new DataTable();
                                dtUserIds.Columns.Add("UserIds");
                                dtUserIds.Rows.Add(notificationUserId);
                                await this.SendDataForApplicableNotifications(NotificationId, dtUserIds, userId);

                            }

                            var SendSMSToUser = GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
                            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            {
                                urlSMS = _configuration[Configuration.NotificationApi];

                                urlSMS += "/CourseApplicabilitySMS";
                                JObject oJsonObjectSMS = new JObject();
                                oJsonObjectSMS.Add("CourseId", accessibilityRule.CourseId);
                                oJsonObjectSMS.Add("OrganizationCode", orgnizationCode);
                                HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                            }
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

        public async Task<List<AccessibilityRules>> PostCategory(APICategoryAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null)
        {

            List<AccessibilityRules> Duplicates = new List<AccessibilityRules>();
            AccessibilityRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
            AccessibilityRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
            if (AndAccessibilityRules.Count() > 0)
            {

                AccessibilityRule accessibilityRules = new AccessibilityRule
                {
                    CategoryId = apiAccessibility.CategoryId,
                    ConditionForRules = "and",
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedBy = userId

                };
                if (apiAccessibility.SubCategoryId != null)
                {
                    accessibilityRules.SubCategoryId = Convert.ToInt32(apiAccessibility.SubCategoryId);
                }
                else
                {
                    accessibilityRules.SubCategoryId = null;
                }

                foreach (AccessibilityRules accessibility in AndAccessibilityRules)
                {

                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn1"))
                        accessibilityRules.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn2"))
                        accessibilityRules.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn3"))
                        accessibilityRules.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn4"))
                        accessibilityRules.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn5"))
                        accessibilityRules.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn6"))
                        accessibilityRules.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn7"))
                        accessibilityRules.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn8"))
                        accessibilityRules.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn9"))
                        accessibilityRules.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn10"))
                        accessibilityRules.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn11"))
                        accessibilityRules.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("configurationcolumn12"))
                        accessibilityRules.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("area"))
                        accessibilityRules.Area = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("business"))
                        accessibilityRules.Business = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("emailid"))
                        accessibilityRules.EmailID = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower().Equals("location"))
                        accessibilityRules.Location = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("group"))
                        accessibilityRules.Group = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("userid"))
                        accessibilityRules.UserID = Convert.ToInt32(accessibility.ParameterValue);
                    if (accessibility.AccessibilityRule.ToLower().Equals("mobilenumber"))
                        accessibilityRules.MobileNumber = accessibility.ParameterValue;

                }

                if (await CategoryRuleExist(accessibilityRules))
                {
                    Duplicates.Add(AndAccessibilityRules[0]);
                }
                else
                {
                    await this.Add(accessibilityRules);
                }
            }
            if (OrAccessibilityRules.Count() > 0)
            {
                foreach (AccessibilityRules accessibility in OrAccessibilityRules)
                {
                    AccessibilityRule accessibilityRule = new AccessibilityRule
                    {
                        CategoryId = apiAccessibility.CategoryId,
                        ConditionForRules = "and",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = userId

                    };

                    if (apiAccessibility.SubCategoryId != null)
                    {
                        accessibilityRule.SubCategoryId = Convert.ToInt32(apiAccessibility.SubCategoryId);
                    }
                    else
                    {
                        accessibilityRule.SubCategoryId = null;
                    }

                    if (!accessibility.Condition.Equals("null"))
                        accessibilityRule.ConditionForRules = "or";
                    accessibilityRule.CreatedDate = DateTime.UtcNow;
                    bool RecordExist = false;
                    string columnName = accessibility.AccessibilityRule.ToLower();
                    var Query = _db.AccessibilityRule.Where(a => a.CategoryId == apiAccessibility.CategoryId && a.IsDeleted == false && a.SubCategoryId == apiAccessibility.SubCategoryId);
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
                    }
                    RecordExist = Query.Count() > 0 ? true : false;
                    if (!RecordExist)
                    {
                        await this.Add(accessibilityRule);


                    }
                    else
                    {
                        Duplicates.Add(accessibility);
                    }

                }
            }
            if (Duplicates.Count > 0)
                return Duplicates;
            return null;

        }
        public async Task<List<CourseApplicablityEmails>> GetCourseApplicabilityEmails(int CourseID, string orgCode)
        {
            List<CourseApplicablityEmails> emails = new List<CourseApplicablityEmails>();

            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetBellNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = CourseID });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    CourseApplicablityEmails EmailObject = new CourseApplicablityEmails();
                                    EmailObject.UserId = Convert.ToInt32(row["UserId"].ToString());
                                    emails.Add(EmailObject);
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
            return emails;
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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return value;
        }

        public async Task<AccessibilityRule> GetRuleByCourIdAndCondition(string condition, int courseId, int userId)
        {
            return await _db.AccessibilityRule.Where(r => r.ConditionForRules.ToLower().Equals(condition.ToLower()) && r.CourseId.Value == courseId && r.UserID == userId).SingleOrDefaultAsync();
        }
        public async Task<List<Object>> GetRules(int userId)
        {
            string url = _configuration[APIHelper.UserAPI];
            HttpResponseMessage response = APIHelper.CallGetAPI(url + userId).Result;
            Object User = null;
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                User = JsonConvert.DeserializeObject(result);
            }
            return null;
        }

        public async Task<List<string>> GetCourseName(int page, int pageSize, string search = null)
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


            if (page != -1)
                CourseNames = CourseNames.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                CourseNames = CourseNames.Take(pageSize);
            return await CourseNames.ToListAsync();
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

        public async Task<int> GetAccessibilityRulesCount(int courseId)
        {
            int Count = 0;
            Count = await (from accessibiltyRule in _db.AccessibilityRule
                           join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                           into c
                           from course in c.DefaultIfEmpty()
                           where accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                               accessibiltyRule.CourseId,
                               accessibiltyRule.Id,
                               accessibiltyRule.UserTeamId,
                               course.Title
                           }).CountAsync();

            if (Count == 0)
            {
                Count = await (from accessibiltyRule in _db.AccessibilityRule
                               join course in _db.Course on accessibiltyRule.CourseId equals course.Id

                               join applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate on accessibiltyRule.GroupTemplateId equals applicabilityGroupTemplate.Id
                               into d
                               from applicabilityGroupTemplate in d.DefaultIfEmpty()
                               where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0) && accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false
                               select new
                               {
                                   accessibiltyRule.CourseId,
                                   accessibiltyRule.Id,
                                   course.Title,
                                   accessibiltyRule.GroupTemplateId,
                                   applicabilityGroupTemplate.ApplicabilityGroupName
                               }).CountAsync();
            }
            return Count;
        }

        public async Task<int> GetCategoryAccessibilityRulesCount(int categoryid)
        {
            int Count = 0;
            Count = await (from accessibiltyRule in _db.AccessibilityRule
                           join category in _db.Category on accessibiltyRule.CategoryId equals category.Id
                           join subcategory in _db.SubCategory on accessibiltyRule.SubCategoryId equals subcategory.Id
                           into sc
                           from SubCategory in sc.DefaultIfEmpty()
                           where accessibiltyRule.CategoryId == categoryid && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                               accessibiltyRule.CourseId,
                               accessibiltyRule.Id,
                               category.Name,
                               accessibiltyRule.CategoryId,
                           }).CountAsync();

            if (Count == 0)
            {
                Count = await (from accessibiltyRule in _db.AccessibilityRule
                               join course in _db.Course on accessibiltyRule.CategoryId equals course.CategoryId

                               join applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate on accessibiltyRule.GroupTemplateId equals applicabilityGroupTemplate.Id
                               into d
                               from applicabilityGroupTemplate in d.DefaultIfEmpty()
                               where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0) && accessibiltyRule.CategoryId == categoryid && accessibiltyRule.IsDeleted == false
                               select new
                               {
                                   accessibiltyRule.CategoryId,
                                   accessibiltyRule.CourseId,
                                   accessibiltyRule.Id,
                                   course.Title,
                                   accessibiltyRule.GroupTemplateId,
                                   applicabilityGroupTemplate.ApplicabilityGroupName
                               }).CountAsync();
            }
            return Count;
        }
        public async Task<List<APIAccessibilityRules>> GetAccessibilityRules(int courseId, string orgnizationCode, string token, int Page, int PageSize)
        {
            var Result = await (from accessibiltyRule in _db.AccessibilityRule
                                join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                into c
                                from course in c.DefaultIfEmpty()
                                where accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                                    accessibiltyRule.CourseId,
                                    accessibiltyRule.Id,
                                    accessibiltyRule.UserTeamId,
                                    course.Title,
                                    accessibiltyRule.StartDateOfJoining,
                                    accessibiltyRule.EndDateOfJoining
                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var ResultForGroupApplicability = await (from accessibiltyRule in _db.AccessibilityRule
                                                     join course in _db.Course on accessibiltyRule.CourseId equals course.Id

                                                     join applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate on accessibiltyRule.GroupTemplateId equals applicabilityGroupTemplate.Id
                                                     into d
                                                     from applicabilityGroupTemplate in d.DefaultIfEmpty()
                                                     where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0) && accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false
                                                     select new
                                                     {
                                                         accessibiltyRule.CourseId,
                                                         accessibiltyRule.Id,
                                                         course.Title,
                                                         accessibiltyRule.GroupTemplateId,
                                                         applicabilityGroupTemplate.ApplicabilityGroupName
                                                     }).ToListAsync();
            List<APIAccessibilityRules> AccessibilityRules = new List<APIAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int CourseId = 0;
                int Id = 0;
                int i=0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("courseid"))
                        CourseId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("Title") &&
                        !rule.Name.Equals("CourseId") &&
                        !rule.Name.Equals("Id"))
                    {
                        if (string.Equals(rule.Name, "startdateofjoining" , StringComparison.CurrentCultureIgnoreCase))
                        {
                            var applicationDateFormat = await GetMasterConfigurableParameterValue("APPLICATION_DATE_FORMAT");
                            Rules RuleDoj = new Rules
                            {
                                AccessibilityParameter = "DateOfJoining",
                                AccessibilityValue =  Convert.ToDateTime(rule.GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
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
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
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
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
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
            foreach (APIAccessibilityRules AccessRule in AccessibilityRules)
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
                    if (!string.Equals(AccessRule.AccessibilityParameter1,"dateofjoining",StringComparison.CurrentCultureIgnoreCase))
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
                    else if (AccessRule.AccessibilityParameter1 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter1 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId1).FirstOrDefault();
                        AccessRule.AccessibilityValue1 = userTeams.TeamName;
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
                    else if (AccessRule.AccessibilityParameter2 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter2 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId2).FirstOrDefault();
                        AccessRule.AccessibilityValue2 = userTeams.TeamName;
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

                    CourseId = Int32.Parse(item.CourseId.ToString());

                    APIAccessibilityRules accessRule = new APIAccessibilityRules
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "Group Template Name",
                        AccessibilityValue1 = item.ApplicabilityGroupName,
                        AccessibilityValueId1 = Int32.Parse(item.GroupTemplateId.ToString()),
                        CourseId = CourseId
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }
            return AccessibilityRules;
        }

        public async Task<List<APICategoryAccessibilityRules>> GetCategoryAccessibilityRules(int CategoryId, string orgnizationCode, string token, int Page, int PageSize)
        {
            try
            {
                var Result = await (from accessibiltyRule in _db.AccessibilityRule
                                    join category in _db.Category on accessibiltyRule.CategoryId equals category.Id
                                    join subcategory in _db.SubCategory on accessibiltyRule.SubCategoryId equals subcategory.Id
                                    into sc
                                    from SubCategory in sc.DefaultIfEmpty()
                                    where accessibiltyRule.CategoryId == CategoryId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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

                                        accessibiltyRule.Id,
                                        category.Name,
                                        accessibiltyRule.CategoryId

                                    }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

                Result = Result.Distinct().ToList();
                List<APICategoryAccessibilityRules> AccessibilityRules = new List<APICategoryAccessibilityRules>();
                foreach (var AccessRule in Result)
                {
                    string Condition = AccessRule.ConditionForRules;
                    PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                    List<Rules> Rules = new List<Rules>();
                    int SubCategoryId = 0;
                    string CategoryCode = string.Empty;
                    string SubCategoryCode = string.Empty;
                    int Id = 0;
                    foreach (PropertyInfo rule in properties)
                    {
                        if (rule.Name.ToLower().Equals("categoryid"))
                            CategoryId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                        if (rule.Name.ToLower().Equals("subcategoryid"))
                            SubCategoryId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                        if (rule.Name.ToLower().Equals("Code"))
                            CategoryCode = rule.GetValue(AccessRule).ToString();
                        if (rule.Name.ToLower().Equals("SubCode"))
                            SubCategoryCode = Convert.ToString(rule.GetValue(AccessRule).ToString());

                        if (rule.Name.ToLower().Equals("id"))
                            Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                        if (rule.GetValue(AccessRule, null) != null &&
                            !rule.Name.Equals("ConditionForRules") &&
                            !rule.Name.Equals("Name") &&
                            !rule.Name.Equals("CategoryId") &&
                            !rule.Name.Equals("Id") &&
                            !rule.Name.Equals("SubCategoryId") && !rule.Name.Equals("CategoryCode")
                            && !rule.Name.Equals("SubCategoryCode"))
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
                    if (Rules.Count == 3)
                    {
                        AccessibilityRule acc = await this.GetAccessibilityRules1(Id);
                        SubCategory subCategory = await this.GetSubCategoryNameById(Convert.ToInt32(acc.SubCategoryId));
                        Category Category = await this.GetCategoryNameById(Convert.ToInt32(acc.SubCategoryId));
                        APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                        {
                            CategoryCode = Category == null ? "-" : Category.Code,
                            SubCategoryCode = subCategory == null ? "-" : subCategory.Code,
                            CategoryId = CategoryId,
                            SubCategoryId = subCategory == null ? 0 : subCategory.Id,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                            Condition1 = "and",
                            AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                            AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue),
                            Condition2 = "and",
                            AccessibilityParameter3 = Rules.ElementAt(2).AccessibilityParameter,
                            AccessibilityValueId3 = Int32.Parse(Rules.ElementAt(2).AccessibilityValue)
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    if (Rules.Count == 2)
                    {
                        AccessibilityRule acc = await this.GetAccessibilityRules1(Id);
                        SubCategory subCategory = await this.GetSubCategoryNameById(Convert.ToInt32(acc.SubCategoryId));
                        Category Category = await this.GetCategoryNameById(Convert.ToInt32(acc.SubCategoryId));

                        APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                        {
                            CategoryCode = Category == null ? "-" : Category.Code,
                            SubCategoryCode = subCategory == null ? "-" : subCategory.Code,
                            CategoryId = CategoryId,
                            SubCategoryId = subCategory == null ? 0 : subCategory.Id,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                            Condition1 = "and",
                            AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                            AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue),

                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    else if (Rules.Count == 1)
                    {
                        AccessibilityRule acc = await this.GetAccessibilityRules1(Id);
                        SubCategory subCategory = await this.GetSubCategoryNameById(Convert.ToInt32(acc.SubCategoryId));
                        Category Category = await this.GetCategoryNameById(Convert.ToInt32(acc.CategoryId));

                        APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                        {
                            CategoryCode = Category == null ? "-" : Category.Code,
                            SubCategoryCode = subCategory == null ? "-" : subCategory.Code,
                            CategoryId = CategoryId,
                            SubCategoryId = subCategory == null ? 0 : subCategory.Id,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue)
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
                foreach (APICategoryAccessibilityRules AccessRule in AccessibilityRules)
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
                            AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                        }
                    }
                    if (AccessRule.AccessibilityValueId3 != 0)
                    {
                        ColumnName = AccessRule.AccessibilityParameter3;
                        Value = AccessRule.AccessibilityValueId3;
                        response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            Title _Title = JsonConvert.DeserializeObject<Title>(result);
                            AccessRule.AccessibilityValue3 = _Title == null ? null : _Title.Name;
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
                        else
                        {
                            AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                        }
                        if (AccessRule.AccessibilityParameter3 == "UserID")
                        {
                            AccessRule.AccessibilityParameter3 = "UserID";
                        }
                        else if (AccessRule.AccessibilityParameter2 == "MobileNumber")
                        {
                            AccessRule.AccessibilityParameter3 = "MobileNumber";
                        }
                        else if (AccessRule.AccessibilityParameter3 == "EmailID")
                        {
                            AccessRule.AccessibilityParameter3 = "EmailID";
                        }
                        else
                        {
                            AccessRule.AccessibilityParameter3 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter3, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                        }
                    }
                }


                return AccessibilityRules;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<APIAccessibilityRules> GetAccessibilityRule(int ruleId, string orgnizationCode)
        {
            var AccessRule = await (from accessibiltyRule in _db.AccessibilityRule
                                    join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                    into c
                                    from course in c.DefaultIfEmpty()
                                    where accessibiltyRule.Id == ruleId && accessibiltyRule.IsDeleted == false
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
                                        accessibiltyRule.Location,
                                        accessibiltyRule.UserID,
                                        accessibiltyRule.ConditionForRules,
                                        accessibiltyRule.CourseId,
                                        accessibiltyRule.Id,
                                        accessibiltyRule.UserTeamId,
                                        course.Title
                                    }).SingleOrDefaultAsync();

            string Condition = AccessRule.ConditionForRules;
            PropertyInfo[] properties = AccessRule.GetType().GetProperties();
            List<Rules> Rules = new List<Rules>();
            int CourseId = 0;
            int Id = 0;
            foreach (PropertyInfo rule in properties)
            {

                if (rule.Name.ToLower().Equals("courseid"))
                    CourseId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                if (rule.Name.ToLower().Equals("id"))
                    Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                if (rule.GetValue(AccessRule, null) != null &&
                    !rule.Name.Equals("ConditionForRules") &&
                    !rule.Name.Equals("Title") &&
                    !rule.Name.Equals("CourseId") &&
                    !rule.Name.Equals("Id"))
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
            APIAccessibilityRules ApiRule = new APIAccessibilityRules();
            if (Rules.Count == 2)
            {

                ApiRule.CourseId = CourseId;
                ApiRule.Id = Id;
                ApiRule.AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter;
                ApiRule.AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue);
                ApiRule.Condition1 = "and";
                ApiRule.AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter;
                ApiRule.AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue);
            }
            else if (Rules.Count == 1)
            {
                ApiRule.CourseId = CourseId;
                ApiRule.Id = Id;
                ApiRule.AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter;
                ApiRule.AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue);
            }
            if (ApiRule != null)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = ApiRule.AccessibilityParameter1;
                int Value = ApiRule.AccessibilityValueId1;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    ApiRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }
                if (ApiRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = ApiRule.AccessibilityParameter2;
                    Value = ApiRule.AccessibilityValueId2;
                    response = APIHelper.CallGetAPI(UserUrl + NameById + "/" + ColumnName + "/" + Value).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        ApiRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
            }

            return ApiRule;
        }


        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int CourseId, string AccessibilityValue11, string AccessibilityValue22)
        {
            bool isvalid = true;

            if (_db.Course.Where(y => y.Id == CourseId && y.IsDeleted == false).Count() <= 0)
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
                            if (AccessibilityParameter1.ToLower() != "userteamid")
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
                            else
                            {
                                UserTeams userTeams = _db.UserTeams.Where(a => a.Id == Convert.ToInt32(AccessibilityValue1) && a.IsDeleted == false).FirstOrDefault();
                                if (userTeams != null)
                                {
                                    isvalid = true;
                                }
                                else
                                {
                                    isvalid = false;
                                }
                            }
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


        public async Task<bool> CheckValidDatacategory(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, string AccessibilityParameter3, string AccessibilityValue3, int CategoryId, int? SubCategoryId)
        {
            bool isvalid = true;

            if (_db.Category.Where(y => y.Id == CategoryId).Count() <= 0)
            {
                isvalid = false;
                return isvalid;

            }

            if ((_db.SubCategory.Where(y => y.Id == SubCategoryId).Count() <= 0) && SubCategoryId != null)
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
                            if (AccessibilityParameter1.ToLower() != "userteamid")
                            {
                                cmd.CommandText = "CheckCategoryValidDataForUserSetting";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter1", SqlDbType.VarChar) { Value = AccessibilityParameter1 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityValue1", SqlDbType.VarChar) { Value = AccessibilityValue1 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter2", SqlDbType.VarChar) { Value = AccessibilityParameter2 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityValue2", SqlDbType.VarChar) { Value = AccessibilityValue2 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter3", SqlDbType.VarChar) { Value = AccessibilityParameter3 });
                                cmd.Parameters.Add(new SqlParameter("@AccessibilityValue3", SqlDbType.VarChar) { Value = AccessibilityValue3 });
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
                            else
                            {
                                UserTeams userTeams = _db.UserTeams.Where(a => a.Id == Convert.ToInt32(AccessibilityValue1) && a.IsDeleted == false).FirstOrDefault();
                                if (userTeams != null)
                                {
                                    isvalid = true;
                                }
                                else
                                {
                                    isvalid = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return isvalid;
        }

        public async Task<bool> CheckValidDataForUserGroup(ApiAccebilityRuleUserGroup accebilityRuleUserGroup)
        {
            bool isvalid = true;

            if (_db.Course.Where(y => y.Id == accebilityRuleUserGroup.CourseId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            if (_db.UserGroup.Where(y => y.Id == accebilityRuleUserGroup.UserGroupId).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            return isvalid;
        }

        public async Task<int> UpdateRule(APIAccessibilityRules apiAccessibilityRules, int id)
        {
            AccessibilityRule ExistingAccessibilityRules = await this.Get(id);
            AccessibilityRule accessibilityRules = new AccessibilityRule
            {
                CreatedBy = ExistingAccessibilityRules.CreatedBy,
                CreatedDate = ExistingAccessibilityRules.CreatedDate,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = ExistingAccessibilityRules.CreatedBy,
                CourseId = ExistingAccessibilityRules.CourseId,
                Id = ExistingAccessibilityRules.Id,
                ConditionForRules = ExistingAccessibilityRules.ConditionForRules
            };

            if (accessibilityRules == null)
                return 0;
            accessibilityRules.ModifiedDate = DateTime.UtcNow;

            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn1"))
                accessibilityRules.ConfigurationColumn1 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn2"))
                accessibilityRules.ConfigurationColumn2 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn3"))
                accessibilityRules.ConfigurationColumn3 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn4"))
                accessibilityRules.ConfigurationColumn4 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn5"))
                accessibilityRules.ConfigurationColumn5 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn6"))
                accessibilityRules.ConfigurationColumn6 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn7"))
                accessibilityRules.ConfigurationColumn7 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn6"))
                accessibilityRules.ConfigurationColumn8 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn9"))
                accessibilityRules.ConfigurationColumn9 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn10"))
                accessibilityRules.ConfigurationColumn10 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn11"))
                accessibilityRules.ConfigurationColumn11 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("configurationcolumn12"))
                accessibilityRules.ConfigurationColumn12 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("area"))
                accessibilityRules.Area = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("business"))
                accessibilityRules.Business = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("emailid"))
                accessibilityRules.EmailID = apiAccessibilityRules.AccessibilityValue1;
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("location"))
                accessibilityRules.Location = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("group"))
                accessibilityRules.Group = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("userid"))
                accessibilityRules.UserID = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId1);
            if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("mobilenumber"))
                accessibilityRules.MobileNumber = apiAccessibilityRules.AccessibilityValue1;

            if (apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn1"))
                    accessibilityRules.ConfigurationColumn1 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn2"))
                    accessibilityRules.ConfigurationColumn2 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn3"))
                    accessibilityRules.ConfigurationColumn3 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn4"))
                    accessibilityRules.ConfigurationColumn4 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn5"))
                    accessibilityRules.ConfigurationColumn5 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn6"))
                    accessibilityRules.ConfigurationColumn6 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn7"))
                    accessibilityRules.ConfigurationColumn7 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn8"))
                    accessibilityRules.ConfigurationColumn8 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn9"))
                    accessibilityRules.ConfigurationColumn9 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn10"))
                    accessibilityRules.ConfigurationColumn10 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn11"))
                    accessibilityRules.ConfigurationColumn11 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("configurationcolumn12"))
                    accessibilityRules.ConfigurationColumn12 = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("area"))
                    accessibilityRules.Area = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("business"))
                    accessibilityRules.Business = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("emailid"))
                    accessibilityRules.EmailID = apiAccessibilityRules.AccessibilityValue2;
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("location"))
                    accessibilityRules.Location = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("group"))
                    accessibilityRules.Group = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("userid"))
                    accessibilityRules.UserID = Convert.ToInt32(apiAccessibilityRules.AccessibilityValueId2);
                if (apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("mobilenumber"))
                    accessibilityRules.MobileNumber = apiAccessibilityRules.AccessibilityValue2;
            }
            await this.Update(accessibilityRules);
            return 1;
        }

        public async Task<int> DeleteRule(int roleId)
        {
            AccessibilityRule accessibilityRules = await this.Get(roleId);
            if (accessibilityRules != null)
            {
                if (accessibilityRules.UserID != null || accessibilityRules.EmailID != null)
                {
                    var enable_Edcast = await GetMasterConfigurableParameterValue("Enable_Edcast");
                    _logger.Debug("Enable_Edcast :-" + enable_Edcast);
                    string LxpDetails = null;
                    LxpDetails = await _db.Course.Where(a => a.Id == (int)accessibilityRules.CourseId).Select(a => a.LxpDetails).FirstOrDefaultAsync();
                    _logger.Debug("LxpDetails :-" + LxpDetails);
                    if (Convert.ToString(enable_Edcast).ToLower() == "yes" && !string.IsNullOrEmpty(LxpDetails))
                    {
                        APIEdcastDetailsToken result = await GetEdCastToken(LxpDetails);
                        if (result != null)
                        {
                            string assignmentStatus = "withdraw";
                            if (accessibilityRules.UserID == null)
                            {
                                accessibilityRules.UserID = Convert.ToInt32(accessibilityRules.EmailID);
                            }
                            APIEdCastTransactionDetails obj = await CourseAssignment((int)accessibilityRules.CourseId, (int)accessibilityRules.UserID, assignmentStatus, null, null, result.access_token, LxpDetails);
                        }
                        else
                        {
                            _logger.Debug("Token null from edcast");
                        }
                    }
                }
                accessibilityRules.IsDeleted = true;
                await this.Update(accessibilityRules);
                return 1;
            }
            return 0;
        }

        public async Task<int> DeleteCategoryRule(int roleId)
        {
            AccessibilityRule accessibilityRules = await this.Get(roleId);
            if (accessibilityRules != null)
            {
                accessibilityRules.IsDeleted = true;
                await this.Update(accessibilityRules);
                return 1;
            }
            return 0;
        }
        public async Task<bool> RuleExist(AccessibilityRule accessibilityRule)
        {
            IQueryable<AccessibilityRule> Query = this._db.AccessibilityRule.Where(a => a.CourseId == accessibilityRule.CourseId && a.IsDeleted == Record.NotDeleted);

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

        public async Task<bool> CategoryRuleExist(AccessibilityRule accessibilityRule)
        {
            IQueryable<AccessibilityRule> Query = this._db.AccessibilityRule.Where(a => a.CategoryId == accessibilityRule.CategoryId && a.IsDeleted == Record.NotDeleted && a.SubCategoryId == accessibilityRule.SubCategoryId);

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

            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> GroupNameExists(string GroupName)
        {
            IQueryable<UserGroup> Query = this._db.UserGroup.Where(a => a.GroupName == GroupName && a.IsDeleted == Record.NotDeleted);

            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }

        //public async Task<ApiResponse> ProcessImportFile(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
        //{
        //    try
        //    {
        //        return await new AccessibilityRuleImport.ProcessFile().ProcessRecordsAsync(file, _customerConnectionRepository, userid, _configuration, orgcode);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;              
        //    }
        //}

        //public async Task<ApiResponse> ProcessGroupImportFile(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, string GroupName, IConfiguration _configuration, string orgcode)
        //{
        //    try
        //    {
        //        return await new AccessibilityRuleImport.ProcessFile().ProcessGroupRecordsAsync(file, _customerConnectionRepository, userid, GroupName, _configuration, orgcode);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public List<AccessibilityRule> GetRuleByUserTeams(int courseId)
        {
            List<AccessibilityRule> accessibilityRule = _db.AccessibilityRule.Where(a => a.CourseId == courseId && a.UserTeamId != null && a.IsDeleted == false).ToList();

            return accessibilityRule;
        }
        public List<CourseApplicableUser> GetUsersForUserTeam(int? Id)
        {
            List<CourseApplicableUser> listUserApplicability = new List<CourseApplicableUser>();

            var connection = this._db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetUserTeamApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("UserTeamId", SqlDbType.BigInt) { Value = Id });

                    DbDataReader reader = cmd.ExecuteReader();
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
                    cmd.CommandText = "GetCourseApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.BigInt) { Value = courseId });

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

        public async Task<List<CategoryApplicableUser>> GetCategoryApplicableUserList(int categoryid)
        {
            List<CategoryApplicableUser> listUserApplicability = new List<CategoryApplicableUser>();

            var connection = this._db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetCategoryApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.BigInt) { Value = categoryid });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            CategoryApplicableUser rule = new CategoryApplicableUser();
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
        public FileInfo GetApplicableUserListExcel(List<APIAccessibilityRules> aPIAccessibilityRules, List<CourseApplicableUser> courseApplicableUsers, string CourseName, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"CourseApplicableUser.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Course Applicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "Course Name";
                row++;
                worksheet.Cells[row, column].Value = CourseName;
                row++;
                column = 1;
                row++;
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                if (string.Equals(OrgCode,"sbil",StringComparison.CurrentCultureIgnoreCase))
                {
                    worksheet.Cells[row, column++].Value = "Accessibility Value11";
                    worksheet.Cells[row, column].Style.Font.Bold = true;
                }
                worksheet.Cells[row, column++].Value = "Additional Criteria";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                if (string.Equals(OrgCode,"sbil",StringComparison.CurrentCultureIgnoreCase))
                {
                    worksheet.Cells[row, column++].Value = "Accessibility Value22";
                    worksheet.Cells[row, column].Style.Font.Bold = true;
                }

                foreach (APIAccessibilityRules course in aPIAccessibilityRules)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                    if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    {
                        worksheet.Cells[row, column++].Value = course.AccessibilityValue11 == null ? "-" : course.AccessibilityValue11;
                    }
                    worksheet.Cells[row, column++].Value = course.Condition1 == null ? "-" : course.Condition1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter2 == null ? "-" : course.AccessibilityParameter2;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue2 == null ? "-" : course.AccessibilityValue2;
                    if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    {
                        worksheet.Cells[row, column++].Value = course.AccessibilityValue22 == null ? "-" : course.AccessibilityValue22;
                    }
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

        public FileInfo GetCategoryApplicableUserListExcel(List<APICategoryAccessibilityRules> aPIAccessibilityRules, List<CategoryApplicableUser> courseApplicableUsers, string CategoryName, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"CategoryApplicableUser.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SurveyApplicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "Category Name";
                row++;
                worksheet.Cells[row, column].Value = CategoryName;
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

                foreach (APICategoryAccessibilityRules course in aPIAccessibilityRules)
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

                foreach (CategoryApplicableUser courseApplicableUser in courseApplicableUsers)
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
        public async Task<List<APIAccessibilityRules>> GetAccessibilityRulesForExport(int courseId, string orgnizationCode, string token, string CourseName)
        {
            var Result = await (from accessibiltyRule in _db.AccessibilityRule
                                join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                into c
                                from course in c.DefaultIfEmpty()
                                where accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                                    accessibiltyRule.CourseId,
                                    accessibiltyRule.Id,
                                    course.Title,
                                    accessibiltyRule.StartDateOfJoining,
                                    accessibiltyRule.EndDateOfJoining
                                }).ToListAsync();

            var ResultForGroupApplicability = await (from accessibiltyRule in _db.AccessibilityRule
                                                     join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                                     join applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate on accessibiltyRule.GroupTemplateId equals applicabilityGroupTemplate.Id
                                                     into d
                                                     from applicabilityGroupTemplate in d.DefaultIfEmpty()
                                                     where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0) && accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false
                                                     select new
                                                     {
                                                         accessibiltyRule.CourseId,
                                                         accessibiltyRule.Id,
                                                         course.Title,
                                                         accessibiltyRule.GroupTemplateId,
                                                         applicabilityGroupTemplate.ApplicabilityGroupName
                                                     }).ToListAsync();

            var UserTeamsApplicability = await (from accessibiltyRule in _db.AccessibilityRule
                                                join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                                join userTeams in _db.UserTeams on accessibiltyRule.UserTeamId equals userTeams.Id
                                                into d
                                                from userTeams in d.DefaultIfEmpty()
                                                where (accessibiltyRule.UserTeamId != null && accessibiltyRule.UserTeamId != 0) && accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false
                                                select new
                                                {
                                                    accessibiltyRule.CourseId,
                                                    accessibiltyRule.Id,
                                                    course.Title,
                                                    accessibiltyRule.UserTeamId,
                                                    userTeams.TeamName
                                                }).ToListAsync();

            List<APIAccessibilityRules> AccessibilityRules = new List<APIAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int CourseId = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("courseid"))
                        CourseId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("Title") &&
                        !rule.Name.Equals("CourseId") &&
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
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
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
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
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
            foreach (APIAccessibilityRules AccessRule in AccessibilityRules)
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

                    CourseId = Int32.Parse(item.CourseId.ToString());

                    APIAccessibilityRules accessRule = new APIAccessibilityRules
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "Group Template Name",
                        AccessibilityValue1 = item.ApplicabilityGroupName,
                        AccessibilityValueId1 = Int32.Parse(item.GroupTemplateId.ToString()),
                        CourseId = CourseId
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }

            if (UserTeamsApplicability != null)
            {
                foreach (var item in UserTeamsApplicability)
                {
                    int CourseId = 0;

                    CourseId = Int32.Parse(item.CourseId.ToString());

                    APIAccessibilityRules accessRule = new APIAccessibilityRules
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "User Team",
                        AccessibilityValue1 = item.TeamName,
                        AccessibilityValueId1 = Int32.Parse(item.UserTeamId.ToString()),
                        CourseId = CourseId
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }

            return AccessibilityRules;
        }

        public async Task<string> GetCourseNames(int courseId)
        {
            var SurveyName = await (from c in _db.Course
                                    where c.IsDeleted == false && c.Id == courseId
                                    select c.Title).SingleOrDefaultAsync();
            return SurveyName;
        }

        public async Task<string> GetCategoryNames(int categoryId)
        {
            var SurveyName = await (from c in _db.Category
                                    where c.Id == categoryId
                                    select c.Name).SingleOrDefaultAsync();
            return SurveyName;
        }

        public async Task<Category> GetCategoryId(string categoryName)
        {
            var query = (from c in _db.Category
                         where c.Name == categoryName.ToLower()
                         select new Category
                         {
                             Id = c.Id,
                             Code = c.Code,
                             ImagePath = c.ImagePath,
                             Name = c.Name
                         }
                              );
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Category> GetCategoryNameById(int Id)
        {
            var query = (from c in _db.Category
                         where c.Id == Id
                         select new Category
                         {
                             Id = c.Id,
                             Code = c.Code,
                             ImagePath = c.ImagePath,
                             Name = c.Name
                         }
                              );
            return await query.FirstOrDefaultAsync();
        }

        public async Task<SubCategory> GetSubCategoryNameById(int Id)
        {
            var query = (from c in _db.SubCategory
                         where c.Id == Id
                         select new SubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             Name = c.Name
                         }
                              );
            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> GetAccessibilityRulesForCategory(int categoryId, int? subcategoryId, int? config2, int? config3, int? UserId, int? LanguageId)
        {
            try
            {
                int query = await (from c in _db.AccessibilityRule
                                   where (c.CategoryId == categoryId && c.SubCategoryId == subcategoryId && c.Location == config2 && c.ConfigurationColumn1 == config3 && c.UserID == UserId && c.ConfigurationColumn2 == LanguageId)
                                   select
                                        c.Id
                                            ).FirstOrDefaultAsync();
                if (query != 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        public async Task<bool> GetAccessibilityRules2(int categoryId, int? config2, int? config3, int? UserId)
        {
            try
            {
                if (config2 == 0)
                {
                    config2 = null;
                }
                if (config3 == 0)
                {
                    config3 = null;
                }
                if (UserId == 0)
                {
                    UserId = null;
                }
                int query = await (from c in _db.AccessibilityRule
                                   where (c.CategoryId == categoryId || (c.Location == config2 || c.Location == null) || (c.ConfigurationColumn1 == config3 || c.ConfigurationColumn1 == null) || (c.UserID == UserId || c.UserID == null))
                                   || (c.CategoryId == categoryId && c.Location == config2 && c.ConfigurationColumn1 == config3)
                                   select //new Category

                                        c.Id


                                  ).SingleOrDefaultAsync();
                if (query != 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        public async Task<AccessibilityRule> GetAccessibilityRules1(int Id)
        {
            var query = await (from c in _db.AccessibilityRule
                               where c.Id == Id
                               select new AccessibilityRule
                               {
                                   CategoryId = c.CategoryId,
                                   SubCategoryId = c.SubCategoryId
                               }).FirstOrDefaultAsync();


            return query;
        }
        public async Task<APISubCategory> GetSubCategoryId(string categoryName, int Id)
        {
            var query = (from c in _db.SubCategory
                         where c.Name == categoryName.ToLower() && c.CategoryId == Id
                         select new APISubCategory
                         {
                             CategoryId = c.CategoryId,
                             Code = c.Code,
                             Id = c.Id,
                             Name = c.Name
                         }
                            );

            return await query.FirstOrDefaultAsync();
        }
        public async Task<List<ApiNotification>> GetCountByCourseIdAndUserId(int Url)
        {
            List<ApiNotification> listUserApplicability = new List<ApiNotification>();

            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetCountByCourseIdAndUserId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.NVarChar) { Value = Url });

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

        public async Task<int> SendNotificationCourseApplicability(ApiNotification apiNotification, bool IsApplicabletoall)
        {
            int Id = 0;

            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "InsertNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@IsRead", SqlDbType.Int) { Value = apiNotification.IsRead });
                            cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar) { Value = apiNotification.Message });
                            cmd.Parameters.Add(new SqlParameter("@Url", SqlDbType.NVarChar) { Value = apiNotification.Url });
                            cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = apiNotification.Title });
                            cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = apiNotification.Type });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = apiNotification.UserId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = apiNotification.CourseId });

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
                                Id = Convert.ToInt32(row["Id"].ToString());
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
            return Id;
        }

        public async Task SendDataForApplicableNotifications(int notificationId, DataTable dtUserIds, int createdBy)
        {
            try
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (SqlCommand cmd = (SqlCommand)connection.CreateCommand())
                        {
                            cmd.CommandText = "ApplicableInsertNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.NVarChar) { Value = notificationId });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = createdBy });
                            cmd.Parameters.AddWithValue("@TVP_UserIDs", dtUserIds);

                            DbDataReader reader = await cmd.ExecuteReaderAsync();

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
        }

        public async Task<List<APICategoryAccessibilityRules>> GetCategoryAccessibilityRulesForExport(int CategoryId, string orgnizationCode, string token, string CategoryName)
        {
            var Result = await (from accessibiltyRule in _db.AccessibilityRule
                                join category in _db.Category on accessibiltyRule.CategoryId equals category.Id
                                where accessibiltyRule.CategoryId == CategoryId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                                    accessibiltyRule.CourseId,
                                    accessibiltyRule.Id,
                                    category.Name,
                                    accessibiltyRule.CategoryId
                                }).ToListAsync();


            List<APICategoryAccessibilityRules> AccessibilityRules = new List<APICategoryAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int CourseId = 0;
                int Id = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("categoryid"))
                        CategoryId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("Name") &&
                        !rule.Name.Equals("CategoryId") &&
                        !rule.Name.Equals("Id"))
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
                if (Rules.Count == 3)
                {
                    APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                    {
                        CategoryId = CategoryId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue),
                        Condition2 = "and",
                        AccessibilityParameter3 = Rules.ElementAt(2).AccessibilityParameter,
                        AccessibilityValueId3 = Int32.Parse(Rules.ElementAt(2).AccessibilityValue)
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                if (Rules.Count == 2)
                {
                    APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                    {
                        CategoryId = CategoryId,

                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue),

                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APICategoryAccessibilityRules ApiRule = new APICategoryAccessibilityRules
                    {
                        CategoryId = CategoryId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue)
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
            foreach (APICategoryAccessibilityRules AccessRule in AccessibilityRules)
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
                        AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                if (AccessRule.AccessibilityValueId3 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter3;
                    Value = AccessRule.AccessibilityValueId3;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        AccessRule.AccessibilityValue3 = _Title == null ? null : _Title.Name;
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
                    else
                    {
                        AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                    if (AccessRule.AccessibilityParameter3 == "UserID")
                    {
                        AccessRule.AccessibilityParameter3 = "UserID";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "MobileNumber")
                    {
                        AccessRule.AccessibilityParameter3 = "MobileNumber";
                    }
                    else if (AccessRule.AccessibilityParameter3 == "EmailID")
                    {
                        AccessRule.AccessibilityParameter3 = "EmailID";
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter3 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter3, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                }
            }


            return AccessibilityRules;
        }
        //public async Task<ApiResponse> ProcessImportCategory(FileInfo file, ICourseRepository _courseRepository, IAccessibilityRule _accessibilityRule, IAccessibilityRuleRejectedRepository _accessibilityRuleRejectedRepository, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
        //{
        //    ApiResponse Response = new ApiResponse();
        //    try
        //    {
        //        AccessibilityRuleImport.ProcessFile.Reset();
        //        bool resultMessage = await new AccessibilityRuleImport.ProcessFile().InitilizeAsync1(file);
        //        if (resultMessage == true)
        //        {
        //            Response = await new AccessibilityRuleImport.ProcessFile().ProcessRecordCategoryAsync(_accessibilityRule, _courseRepository, _accessibilityRuleRejectedRepository, _customerConnectionRepository, userid, _configuration, orgcode);
        //            AccessibilityRuleImport.ProcessFile.Reset();
        //            return Response;
        //        }
        //        else
        //        {
        //            Response.ResponseObject = Record.FileInvalid;
        //            AccessibilityRuleImport.ProcessFile.Reset();
        //            return Response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }
        //    return Response;
        //}
        public async Task<APIEdcastDetailsToken> GetEdCastToken(string LxpDetails = null)
        {
            APIEdcastDetailsToken objtoken = new APIEdcastDetailsToken();
            try
            {
                APIEdcastToken gettokendetails = new APIEdcastToken();


                string url = null;

                EdCastConfiguration edCastConfiguration = await _db.EdCastConfiguration.Where(a => a.LxpDetails == LxpDetails).FirstOrDefaultAsync();
                if (edCastConfiguration == null)
                {
                    EdCastTransactionDetails obj = new EdCastTransactionDetails();
                    obj.TransactionID = null;
                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
                    obj.Payload = null;
                    obj.Tran_Status = ConstantEdCast.Trans_Error;
                    obj.ResponseMessage = "Please set Edcast " + LxpDetails + " configuration.";
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.CreatedBy = 1;
                    obj.RequestUrl = url;
                    obj.External_Id = null;
                    await _edCastTransactionDetails.Add(obj);

                    return objtoken;
                }


                gettokendetails.client_id = edCastConfiguration.LmsClientID;
                gettokendetails.client_secret = edCastConfiguration.LmsClientSecrete;
                gettokendetails.grant_type = ConstantEdCast.grant_type;

                url = edCastConfiguration.LmsHost;
                url = url + "/api/oauth/token";

                JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(gettokendetails));

                objtoken = await ApiHelper.GetTokenForEdcastLMS(oJsonObject, url);
                if (objtoken != null)
                {
                    if (objtoken.access_token != null)
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = null;
                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = JsonConvert.SerializeObject(gettokendetails);
                        obj.Tran_Status = ConstantEdCast.Trans_Success;
                        obj.ResponseMessage = null;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = 1;
                        obj.RequestUrl = url;
                        obj.External_Id = null;
                        await _edCastTransactionDetails.Add(obj);
                    }
                    else
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = null;
                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = JsonConvert.SerializeObject(gettokendetails);
                        obj.Tran_Status = ConstantEdCast.Trans_Error;
                        obj.ResponseMessage = objtoken.error;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = 1;
                        obj.RequestUrl = url;
                        obj.External_Id = null;
                        await _edCastTransactionDetails.Add(obj);
                    }
                }


            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return objtoken;
        }


        public async Task<APIEdCastTransactionDetails> CourseAssignment(int courseID, int userId, string assignmentStatus, string assignedDate = null, string dueDate = null, string token = null, string LxpDetails = null)
        {
            APIEdCastTransactionDetails objCourseResponce = new APIEdCastTransactionDetails();
            try
            {
                string url = null;
                EdCastConfiguration edCastConfiguration = await _db.EdCastConfiguration.Where(a => a.LxpDetails == LxpDetails).FirstOrDefaultAsync();
                if (edCastConfiguration == null)
                {
                    EdCastTransactionDetails obj = new EdCastTransactionDetails();
                    obj.TransactionID = null;
                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
                    obj.Payload = null;
                    obj.Tran_Status = ConstantEdCast.Trans_Error;
                    obj.ResponseMessage = "Please set Edcast " + LxpDetails + " configuration.";
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.CreatedBy = 1;
                    obj.RequestUrl = url;
                    obj.External_Id = courseID.ToString();
                    await _edCastTransactionDetails.Add(obj);
                    objCourseResponce.error = obj.ResponseMessage;
                    objCourseResponce.message = obj.ResponseMessage;
                    objCourseResponce.data.id = courseID.ToString();
                    objCourseResponce.data.http_method = ConstantEdCast.HTTPMETHOD;
                    objCourseResponce.data.payload = null;
                    return objCourseResponce;
                }


                var Query = (from user in _db.UserMaster
                             where user.IsDeleted == false && user.Id == userId
                             select new APICourseAssignment
                             {
                                 external_id = Convert.ToString(courseID),
                                 email_id = Security.Decrypt(user.EmailId),
                                 status = assignmentStatus,
                                 assigned_date = Convert.ToString(assignedDate),
                                 due_date = Convert.ToString(dueDate)
                             }
                       );
                var course_Data = await Query.FirstOrDefaultAsync();
                JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(course_Data));
                url = edCastConfiguration.LmsHost;
                url = url + "/api/developer/v2/user_courses.json";
                string Body = JsonConvert.SerializeObject(oJsonObject);
                objCourseResponce = await ApiHelper.PostEdcastAPI(url, Body, token);

                if (objCourseResponce != null)
                {
                    if (objCourseResponce.error == null)
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = objCourseResponce.data.id;
                        obj.Http_method = objCourseResponce.data.http_method;
                        obj.Payload = JsonConvert.SerializeObject(course_Data);
                        obj.Tran_Status = objCourseResponce.data.status;
                        obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? objCourseResponce.error : objCourseResponce.message;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = objCourseResponce.data.payload.external_id;
                        await _edCastTransactionDetails.Add(obj);
                    }
                    else
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = null;
                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = JsonConvert.SerializeObject(course_Data);
                        obj.Tran_Status = ConstantEdCast.Trans_Error;
                        obj.ResponseMessage = objCourseResponce.error;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = null;
                        await _edCastTransactionDetails.Add(obj);

                    }
                }


            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return objCourseResponce;
        }


        public async Task<List<APIAccessibilityRules>> GetAccessibilityRulesV2(int courseId, string orgnizationCode, string token, int Page, int PageSize,int userId,string userRole)
        {
            var Result = await (from accessibiltyRule in _db.AccessibilityRule
                               // join User in _db.UserMaster on accessibiltyRule.CreatedBy equals User.Id
                                join course in _db.Course on accessibiltyRule.CourseId equals course.Id
                                into c
                                from course in c.DefaultIfEmpty()
                                where accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false && (accessibiltyRule.GroupTemplateId == null || accessibiltyRule.GroupTemplateId == 0)
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
                                    accessibiltyRule.CourseId,
                                    accessibiltyRule.Id,
                                    accessibiltyRule.UserTeamId,
                                    course.Title,
                                    accessibiltyRule.StartDateOfJoining,
                                    accessibiltyRule.EndDateOfJoining,
                                    course.Code,
                                    accessibiltyRule.CreatedBy
                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var ResultForGroupApplicability = await (from accessibiltyRule in _db.AccessibilityRule
                                                     join User in _db.UserMaster on accessibiltyRule.CreatedBy equals User.Id
                                                     join course in _db.Course on accessibiltyRule.CourseId equals course.Id

                                                     join applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate on accessibiltyRule.GroupTemplateId equals applicabilityGroupTemplate.Id
                                                     into d
                                                     from applicabilityGroupTemplate in d.DefaultIfEmpty()
                                                     where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0) && accessibiltyRule.CourseId == courseId && accessibiltyRule.IsDeleted == false
                                                     select new
                                                     {
                                                         accessibiltyRule.CourseId,
                                                         accessibiltyRule.Id,
                                                         course.Title,
                                                         accessibiltyRule.GroupTemplateId,
                                                         applicabilityGroupTemplate.ApplicabilityGroupName,
                                                         User.UserName,
                                                         accessibiltyRule.CreatedBy
                                                     }).ToListAsync();
            List<APIAccessibilityRules> AccessibilityRules = new List<APIAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int CourseId = 0;
                int Id = 0;
                int i = 0;
                string userName=null;
                int createdBy = 0;
                foreach (PropertyInfo rule in properties)
                {                   
                    if (rule.Name.ToLower().Equals("username"))
                        userName = rule.GetValue(AccessRule).ToString();
                    if (rule.Name.ToLower().Equals("createdby"))
                        createdBy = Int32.Parse(rule.GetValue(AccessRule).ToString());
                }
                    foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("courseid"))
                        CourseId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());                  

                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("Title") &&
                        !rule.Name.Equals("CourseId") &&
                        !rule.Name.Equals("Id")&&
                        !rule.Name.Equals("UserName")&&
                        !rule.Name.Equals("CreatedBy"))
                    {
                        if (string.Equals(rule.Name, "startdateofjoining", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var applicationDateFormat = await GetMasterConfigurableParameterValue("APPLICATION_DATE_FORMAT");
                            Rules RuleDoj = new Rules
                            {
                                AccessibilityParameter = "DateOfJoining",
                                AccessibilityValue = Convert.ToDateTime(rule.GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                AccessibilityValue2 = Convert.ToDateTime(properties[++i].GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                Condition = Condition,
                                CreatedBy = createdBy,
                                UserName = userName,
                            };
                            Rules.Add(RuleDoj);
                        }
                        else if ((!string.Equals(rule.Name, "enddateofjoining", StringComparison.CurrentCultureIgnoreCase)) && (!string.Equals(rule.Name, "code", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            Rules Rule = new Rules
                            {
                                AccessibilityParameter = rule.Name,
                                AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                                CreatedBy = createdBy,
                                UserName=userName,
                                Condition = Condition
                            };
                            Rules.Add(Rule);
                        }
                    }
                    i++;
                }

                if (Rules.Count == 2)
                {
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
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
                        UserName = Rules.ElementAt(0).UserName,
                        UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (Rules.ElementAt(0).CreatedBy == userId) ? true : false : true
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APIAccessibilityRules ApiRule = new APIAccessibilityRules
                    {
                        CourseId = CourseId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        AccessibilityValue1 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue : null,
                        AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                        UserName = Rules.ElementAt(0).UserName,
                        UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (Rules.ElementAt(0).CreatedBy == userId) ? true : false : true  
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
            foreach (APIAccessibilityRules AccessRule in AccessibilityRules)
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
                    else if (AccessRule.AccessibilityParameter1 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter1 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId1).FirstOrDefault();
                        AccessRule.AccessibilityValue1 = userTeams.TeamName;
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
                    else if (AccessRule.AccessibilityParameter2 == "UserTeamId")
                    {
                        AccessRule.AccessibilityParameter2 = "User Team";
                        UserTeams userTeams = _db.UserTeams.Where(a => a.Id == AccessRule.AccessibilityValueId2).FirstOrDefault();
                        AccessRule.AccessibilityValue2 = userTeams.TeamName;
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

                    CourseId = Int32.Parse(item.CourseId.ToString());

                    APIAccessibilityRules accessRule = new APIAccessibilityRules
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "Group Template Name",
                        AccessibilityValue1 = item.ApplicabilityGroupName,
                        AccessibilityValueId1 = Int32.Parse(item.GroupTemplateId.ToString()),
                        CourseId = CourseId,
                        UserName=item.UserName,
                        UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (item.CreatedBy == userId) ? true : false : true  
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }
            return AccessibilityRules;
        }


    }

}
