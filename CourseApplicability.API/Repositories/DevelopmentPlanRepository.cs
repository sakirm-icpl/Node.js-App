using CourseApplicability.API.APIModel;
using CourseApplicability.API.Helper;
using CourseApplicability.API.Model;
using CourseApplicability.API.Models;
using CourseApplicability.API.Repositories.Interfaces;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories
{
    public class DevelopmentPlanRepository : IDevelopmentPlanRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DevelopmentPlanRepository));
        private CoursesApplicabilityContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        ICourseRepository _courseRepository;
        IConfiguration _configuration;
        private IAccessibilityRule _accessibilityRule;
        public DevelopmentPlanRepository(CoursesApplicabilityContext db, IConfiguration configuration, ICustomerConnectionStringRepository customerConnection, ICourseRepository courseRepository, IAccessibilityRule accessibilityRule)
        {
            _db = db;
            _customerConnection = customerConnection;
            _courseRepository = courseRepository;
            _configuration = configuration;
            _accessibilityRule = accessibilityRule;
        }

        public async Task<List<APIDevelopmentPlanType>> GetDevelopmentPlanAccessibility(string search)
        {
            var data = (from c in _db.DevelopmentPlanForCourse

                        where (c.DevelopmentName.Contains(search) && c.IsDeleted == false && c.Status == true)

                        select new APIDevelopmentPlanType
                        {
                            Id = c.Id,
                            DevelopmentCode = c.DevelopmentCode,
                            DevelopmentName = c.DevelopmentName
                        });
            return await data.OrderByDescending(c => c.Id).ToListAsync();
        }

        public async Task<List<Mappingparameter>> CheckmappingStatus(MappingParameters mappingParameters, int UserId)
        {
            List<Mappingparameter> rejectMappingParameter = new List<Mappingparameter>();
            Mappingparameter RejectMapping = new Mappingparameter();
            if (mappingParameters == null)
            {
                return null;
            }
            else
            {

                DevelopmentPlanForCourse developmentPlanForCourse = _db.DevelopmentPlanForCourse.Where(a => a.Id == mappingParameters.DevelopmentPlanid && a.IsDeleted == false).FirstOrDefault();
                List<DevelopmentPlanApplicableUser> aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                UserTeams userTeams = new UserTeams();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (developmentPlanForCourse != null)
                {
                    var Mapping = _db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == developmentPlanForCourse.Id);
                    UserDevelopmentPlanMapping userDevelopmentPlanMapping = new UserDevelopmentPlanMapping();

                    switch (mappingParameters.AccessibilityParameter1.ToLower())
                    {

                        case "configurationcolumn1":
                            userDevelopmentPlanMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn2":

                            userDevelopmentPlanMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn3":

                            userDevelopmentPlanMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn4":

                            userDevelopmentPlanMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn5":

                            userDevelopmentPlanMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn6":

                            userDevelopmentPlanMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn7":

                            userDevelopmentPlanMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn8":

                            userDevelopmentPlanMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn9":

                            userDevelopmentPlanMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn10":

                            userDevelopmentPlanMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn11":

                            userDevelopmentPlanMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn12":

                            userDevelopmentPlanMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn13":

                            userDevelopmentPlanMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn14":

                            userDevelopmentPlanMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn15":

                            userDevelopmentPlanMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "area":

                            userDevelopmentPlanMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "business":

                            userDevelopmentPlanMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "emailid":

                            userDevelopmentPlanMapping.EmailID = mappingParameters.AccessibilityValue1;
                            break;
                        case "location":

                            userDevelopmentPlanMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "group":

                            userDevelopmentPlanMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "userid":

                            userDevelopmentPlanMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue1);

                            break;
                        case "mobilenumber":

                            userDevelopmentPlanMapping.MobileNumber = mappingParameters.AccessibilityValue1;

                            break;

                        case "userteamid":

                            userDevelopmentPlanMapping.UserTeamId = Convert.ToInt32(mappingParameters.AccessibilityValue1);

                            break;

                    }




                    if (mappingParameters.AccessibilityParameter2 == null)
                    {

                        userDevelopmentPlanMapping.ConditionForRules = mappingParameters.condition1;
                        userDevelopmentPlanMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                        userDevelopmentPlanMapping.CreatedBy = UserId;
                        userDevelopmentPlanMapping.ModifiedBy = UserId;
                        userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
                        userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userDevelopmentPlanMapping);

                        if (!Result)
                        {
                            this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
                            this._db.SaveChanges();

                            developmentPlanForCourse.NumberOfRules++;
                            developmentPlanForCourse.ModifiedBy = UserId;
                            developmentPlanForCourse.ModifiedDate = DateTime.Now;
                            aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(developmentPlanForCourse.Id);
                            developmentPlanForCourse.NumberofMembers = aPIUserMasterDetails.Count();
                            this._db.DevelopmentPlanForCourse.Update(developmentPlanForCourse);
                            this._db.SaveChanges();


                            aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();

                            if (developmentPlanForCourse != null)
                            {
                                _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                            }
                        }
                        else
                        {
                            RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                            RejectMapping.AccessibilityParameter1 = mappingParameters.AccessibilityParameter1;
                            RejectMapping.ParameterValue1 = mappingParameters.AccessibilityValue1;
                            rejectMappingParameter.Add(RejectMapping);
                        }

                        if (developmentPlanForCourse != null)
                        {
                            _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                        }

                    }

                    if (mappingParameters.AccessibilityParameter2 != null)
                    {
                        switch (mappingParameters.AccessibilityParameter2.ToLower())
                        {
                            case "configurationcolumn1":
                                userDevelopmentPlanMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn2":
                                userDevelopmentPlanMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn3":
                                userDevelopmentPlanMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn4":
                                userDevelopmentPlanMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn5":
                                userDevelopmentPlanMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn6":
                                userDevelopmentPlanMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn7":
                                userDevelopmentPlanMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn8":
                                userDevelopmentPlanMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn9":
                                userDevelopmentPlanMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn10":
                                userDevelopmentPlanMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn11":
                                userDevelopmentPlanMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn12":
                                userDevelopmentPlanMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn13":

                                userDevelopmentPlanMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn14":

                                userDevelopmentPlanMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn15":

                                userDevelopmentPlanMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "area":
                                userDevelopmentPlanMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "business":
                                userDevelopmentPlanMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "emailid":
                                userDevelopmentPlanMapping.EmailID = mappingParameters.AccessibilityValue2;
                                break;

                            case "location":
                                userDevelopmentPlanMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "group":
                                userDevelopmentPlanMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "userid":
                                userDevelopmentPlanMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "mobilenumber":
                                userDevelopmentPlanMapping.MobileNumber = mappingParameters.AccessibilityValue2;
                                break;

                            case "userteamid":

                                userDevelopmentPlanMapping.UserTeamId = Convert.ToInt32(mappingParameters.AccessibilityValue2);

                                break;
                        }


                        userDevelopmentPlanMapping.ConditionForRules = mappingParameters.condition1;
                        userDevelopmentPlanMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                        userDevelopmentPlanMapping.CreatedBy = UserId;
                        userDevelopmentPlanMapping.ModifiedBy = UserId;
                        userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
                        userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userDevelopmentPlanMapping);

                        if (!Result)
                        {

                            this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
                            this._db.SaveChanges();

                            developmentPlanForCourse.NumberOfRules++;
                            developmentPlanForCourse.ModifiedBy = UserId;
                            developmentPlanForCourse.ModifiedDate = DateTime.Now;

                            aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(developmentPlanForCourse.Id);
                            developmentPlanForCourse.NumberofMembers = aPIUserMasterDetails.Count();

                            this._db.DevelopmentPlanForCourse.Update(developmentPlanForCourse);
                            this._db.SaveChanges();

                            aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                        }
                        else
                        {
                            RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                            RejectMapping.AccessibilityParameter1 = mappingParameters.AccessibilityParameter1;
                            RejectMapping.ParameterValue1 = mappingParameters.AccessibilityValue1;
                            RejectMapping.AccessibilityParameter2 = mappingParameters.AccessibilityParameter2;
                            RejectMapping.ParameterValue2 = mappingParameters.AccessibilityValue2;

                            rejectMappingParameter.Add(RejectMapping);
                        }
                    }
                    else
                    {
                        if (rejectMappingParameter.Count() != 0)
                        {
                            return rejectMappingParameter;
                        }
                        return null;
                    }


                    if (developmentPlanForCourse != null)
                    {
                        _db.Entry(developmentPlanForCourse).State = EntityState.Detached;
                    }

                    return rejectMappingParameter;
                }
                else
                {
                    RejectMapping.DevelopmentPlanid = mappingParameters.DevelopmentPlanid;
                    rejectMappingParameter.Add(RejectMapping);

                    return rejectMappingParameter;
                }
            }
        }

        public async Task<bool> RuleExist(UserDevelopmentPlanMapping accessibilityRule)
        {
            IQueryable<UserDevelopmentPlanMapping> Query = this._db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == accessibilityRule.DevelopmentPlanid && a.IsDeleted == false);

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
            if (accessibilityRule.ConfigurationColumn13 != null)
                Query = Query.Where(a => a.ConfigurationColumn13 == accessibilityRule.ConfigurationColumn13);
            if (accessibilityRule.ConfigurationColumn14 != null)
                Query = Query.Where(a => a.ConfigurationColumn14 == accessibilityRule.ConfigurationColumn14);
            if (accessibilityRule.ConfigurationColumn15 != null)
                Query = Query.Where(a => a.ConfigurationColumn15 == accessibilityRule.ConfigurationColumn15);
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
            if (accessibilityRule.UserTeamId != null)
                Query = Query.Where(a => a.UserTeamId == accessibilityRule.UserTeamId);


            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }

        public async Task<List<DevelopmentPlanApplicableUser>> GetDevelopmentPlanApplicableUserList(int DevelopmentPlanId)
        {
            List<DevelopmentPlanApplicableUser> listUserApplicability = new List<DevelopmentPlanApplicableUser>();
            List<DevelopmentPlanApplicableUser> UserList1 = new List<DevelopmentPlanApplicableUser>();
            var connection = this._db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetDevelopmentPlanApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@DevelopmentPlanID", SqlDbType.BigInt) { Value = DevelopmentPlanId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            DevelopmentPlanApplicableUser rule = new DevelopmentPlanApplicableUser();
                            rule.UserID = Security.Decrypt(row["UserID"].ToString());
                            rule.UserName = row["UserName"].ToString();
                            listUserApplicability.Add(rule);
                        }
                    }
                    List<UserDevelopmentPlanMapping> accessibilityRule = GetRuleByUserTeams(DevelopmentPlanId);
                    List<DevelopmentPlanApplicableUser> UserListForUserTeam = new List<DevelopmentPlanApplicableUser>();


                    if (accessibilityRule != null)
                    {
                        foreach (UserDevelopmentPlanMapping accessibilityRule1 in accessibilityRule)
                        {
                            List<CourseApplicableUser> UserListForUserTeam1 = this._accessibilityRule.GetUsersForUserTeam(accessibilityRule1.UserTeamId);
                            foreach (CourseApplicableUser courseApplicableUser1 in UserListForUserTeam1)
                            {
                                DevelopmentPlanApplicableUser developmentPlanApplicableUser = new DevelopmentPlanApplicableUser();
                                developmentPlanApplicableUser.UserID = courseApplicableUser1.UserID;
                                developmentPlanApplicableUser.UserName = courseApplicableUser1.UserName;
                                UserListForUserTeam.Add(developmentPlanApplicableUser);
                            }
                        }

                    }
                    listUserApplicability.AddRange(UserListForUserTeam);

                    UserList1 = listUserApplicability.GroupBy(p => new { p.UserID, p.UserName })
                    .Select(g => g.First())
                    .ToList();
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return UserList1;
        }

        public List<UserDevelopmentPlanMapping> GetRuleByUserTeams(int developmentPlanid)
        {
            List<UserDevelopmentPlanMapping> accessibilityRule = _db.UserDevelopmentPlanMapping.Where(a => a.DevelopmentPlanid == developmentPlanid && a.UserTeamId != null && a.IsDeleted == false).ToList();

            return accessibilityRule;
        }

        public async Task<List<MappingParameters>> GetAccessibilityRules(int DevelopmentPlanId, string orgnizationCode, string token, int Page, int PageSize)
        {
            var Result = await (from UserDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                join DevelopmentPlanForCourse in _db.DevelopmentPlanForCourse on UserDevelopmentPlanMapping.DevelopmentPlanid equals DevelopmentPlanForCourse.Id
                                into c
                                from DevelopmentPlanForCourse in c.DefaultIfEmpty()
                                where UserDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && UserDevelopmentPlanMapping.IsDeleted == false
                                select new
                                {
                                    UserDevelopmentPlanMapping.ConfigurationColumn1,
                                    UserDevelopmentPlanMapping.ConfigurationColumn2,
                                    UserDevelopmentPlanMapping.ConfigurationColumn3,
                                    UserDevelopmentPlanMapping.ConfigurationColumn4,
                                    UserDevelopmentPlanMapping.ConfigurationColumn5,
                                    UserDevelopmentPlanMapping.ConfigurationColumn6,
                                    UserDevelopmentPlanMapping.ConfigurationColumn7,
                                    UserDevelopmentPlanMapping.ConfigurationColumn8,
                                    UserDevelopmentPlanMapping.ConfigurationColumn9,
                                    UserDevelopmentPlanMapping.ConfigurationColumn10,
                                    UserDevelopmentPlanMapping.ConfigurationColumn11,
                                    UserDevelopmentPlanMapping.ConfigurationColumn12,
                                    UserDevelopmentPlanMapping.ConfigurationColumn13,
                                    UserDevelopmentPlanMapping.ConfigurationColumn14,
                                    UserDevelopmentPlanMapping.ConfigurationColumn15,
                                    UserDevelopmentPlanMapping.Area,
                                    UserDevelopmentPlanMapping.Business,
                                    UserDevelopmentPlanMapping.EmailID,
                                    UserDevelopmentPlanMapping.MobileNumber,
                                    UserDevelopmentPlanMapping.Location,
                                    UserDevelopmentPlanMapping.Group,
                                    UserDevelopmentPlanMapping.UserID,
                                    UserDevelopmentPlanMapping.ConditionForRules,
                                    UserDevelopmentPlanMapping.UserTeamId,
                                    UserDevelopmentPlanMapping.Id,
                                    UserDevelopmentPlanMapping.DevelopmentPlanid


                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            List<MappingParameters> AccessibilityRules = new List<MappingParameters>();

            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<RulesForDevelopment> Rules = new List<RulesForDevelopment>();
                int DevelopmentPlanid1 = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("developmentplanid"))
                        DevelopmentPlanid1 = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("DevelopmentPlanid") &&
                        !rule.Name.Equals("Id"))
                    {

                        RulesForDevelopment Rule = new RulesForDevelopment
                        {
                            AccessibilityParameter = rule.Name,
                            AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                            Condition = Condition,
                            Id = Id
                        };
                        Rules.Add(Rule);

                    }
                    i++;
                }
                if (Rules.Count == 2)
                {
                    MappingParameters ApiRule = new MappingParameters
                    {
                        Id = Id,
                        DevelopmentPlanid = DevelopmentPlanid1,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,
                        condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValue2 = Rules.ElementAt(1).AccessibilityValue,
                        AccessibilityValueId2 = !string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(1).AccessibilityValue) : 0,


                    };

                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    MappingParameters ApiRule = new MappingParameters
                    {
                        DevelopmentPlanid = DevelopmentPlanid1,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,

                        AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                        AccessibilityValueId1 = !string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Int32.Parse(Rules.ElementAt(0).AccessibilityValue) : 0,


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
            foreach (MappingParameters AccessRule in AccessibilityRules)
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
            return AccessibilityRules;
        }

        public async Task<int> GetAccessibilityRulesCount(int DevelopmentPlanId)
        {
            int Count = 0;
            Count = await (from UserDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                           join DevelopmentPlanForCourse in _db.DevelopmentPlanForCourse on UserDevelopmentPlanMapping.DevelopmentPlanid equals DevelopmentPlanForCourse.Id
                           into c
                           from DevelopmentPlanForCourse in c.DefaultIfEmpty()
                           where UserDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && UserDevelopmentPlanMapping.IsDeleted == false
                           select new
                           {
                               UserDevelopmentPlanMapping.ConfigurationColumn1,
                               UserDevelopmentPlanMapping.ConfigurationColumn2,
                               UserDevelopmentPlanMapping.ConfigurationColumn3,
                               UserDevelopmentPlanMapping.ConfigurationColumn4,
                               UserDevelopmentPlanMapping.ConfigurationColumn5,
                               UserDevelopmentPlanMapping.ConfigurationColumn6,
                               UserDevelopmentPlanMapping.ConfigurationColumn7,
                               UserDevelopmentPlanMapping.ConfigurationColumn8,
                               UserDevelopmentPlanMapping.ConfigurationColumn9,
                               UserDevelopmentPlanMapping.ConfigurationColumn10,
                               UserDevelopmentPlanMapping.ConfigurationColumn11,
                               UserDevelopmentPlanMapping.ConfigurationColumn12,
                               UserDevelopmentPlanMapping.ConfigurationColumn13,
                               UserDevelopmentPlanMapping.ConfigurationColumn14,
                               UserDevelopmentPlanMapping.ConfigurationColumn15,
                               UserDevelopmentPlanMapping.Area,
                               UserDevelopmentPlanMapping.Business,
                               UserDevelopmentPlanMapping.EmailID,
                               UserDevelopmentPlanMapping.MobileNumber,
                               UserDevelopmentPlanMapping.Location,
                               UserDevelopmentPlanMapping.Group,
                               UserDevelopmentPlanMapping.UserID,
                               UserDevelopmentPlanMapping.ConditionForRules,
                               UserDevelopmentPlanMapping.UserTeamId,
                               UserDevelopmentPlanMapping.Id

                           }).CountAsync();
            return Count;
        }

        public async Task<int> DeleteRule(int roleId)
        {
            UserDevelopmentPlanMapping accessibilityRule = _db.UserDevelopmentPlanMapping.Where(a => a.Id == roleId && a.IsDeleted == false).FirstOrDefault();

            if (accessibilityRule != null)
            {
                DevelopmentPlanForCourse userTeams = _db.DevelopmentPlanForCourse.Where(a => a.Id == accessibilityRule.DevelopmentPlanid && a.IsDeleted == false).FirstOrDefault();

                accessibilityRule.IsDeleted = true;
                _db.UserDevelopmentPlanMapping.Update(accessibilityRule);
                await _db.SaveChangesAsync();

                List<DevelopmentPlanApplicableUser> aPIUserMasterDetails = new List<DevelopmentPlanApplicableUser>();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (userTeams != null)
                {
                    if (userTeams.DevelopmentCode != null)
                    {

                        aPIUserMasterDetails = await GetDevelopmentPlanApplicableUserList(userTeams.Id);
                        userTeams.NumberofMembers = aPIUserMasterDetails.Count();
                        userTeams.NumberOfRules = userTeams.NumberOfRules - 1;
                        _db.DevelopmentPlanForCourse.Update(userTeams);
                        await _db.SaveChangesAsync();
                        return 1;
                    }
                }


                _db.DevelopmentPlanForCourse.Update(userTeams);
                await _db.SaveChangesAsync();
                return 1;
            }
            return 0;
        }

        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int DevelopmentPlanId)
        {
            if (AccessibilityParameter1 == null || AccessibilityValue1 == null)
            {
                return false;
            }
            bool isvalid = true;

            if (_db.DevelopmentPlanForCourse.Where(y => y.Id == DevelopmentPlanId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return isvalid;
        }

        public async Task<string> GetDevelopmentPlanName(int DevelopmentPlanId)
        {
            var DevelopmentPlanName = await (from c in _db.DevelopmentPlanForCourse
                                             where c.IsDeleted == false && c.Id == DevelopmentPlanId
                                             select c.DevelopmentName).SingleOrDefaultAsync();
            return DevelopmentPlanName;
        }

        public async Task<List<APIAccessibilityRulesDevelopment>> GetAccessibilityRulesForExport(int DevelopmentPlanId, string orgnizationCode, string token, string DevelopmentPlanName)
        {
            var Result = await (from userDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                join developmentPlanForCourse in _db.DevelopmentPlanForCourse on userDevelopmentPlanMapping.DevelopmentPlanid equals developmentPlanForCourse.Id
                                into c
                                from developmentPlanForCourse in c.DefaultIfEmpty()
                                where userDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && userDevelopmentPlanMapping.IsDeleted == false
                                select new
                                {
                                    userDevelopmentPlanMapping.ConfigurationColumn1,
                                    userDevelopmentPlanMapping.ConfigurationColumn2,
                                    userDevelopmentPlanMapping.ConfigurationColumn3,
                                    userDevelopmentPlanMapping.ConfigurationColumn4,
                                    userDevelopmentPlanMapping.ConfigurationColumn5,
                                    userDevelopmentPlanMapping.ConfigurationColumn6,
                                    userDevelopmentPlanMapping.ConfigurationColumn7,
                                    userDevelopmentPlanMapping.ConfigurationColumn8,
                                    userDevelopmentPlanMapping.ConfigurationColumn9,
                                    userDevelopmentPlanMapping.ConfigurationColumn10,
                                    userDevelopmentPlanMapping.ConfigurationColumn11,
                                    userDevelopmentPlanMapping.ConfigurationColumn12,
                                    userDevelopmentPlanMapping.ConfigurationColumn13,
                                    userDevelopmentPlanMapping.ConfigurationColumn14,
                                    userDevelopmentPlanMapping.ConfigurationColumn15,
                                    userDevelopmentPlanMapping.Area,
                                    userDevelopmentPlanMapping.Business,
                                    userDevelopmentPlanMapping.EmailID,
                                    userDevelopmentPlanMapping.MobileNumber,
                                    userDevelopmentPlanMapping.Location,
                                    userDevelopmentPlanMapping.Group,
                                    userDevelopmentPlanMapping.UserID,
                                    userDevelopmentPlanMapping.ConditionForRules,
                                    userDevelopmentPlanMapping.DevelopmentPlanid,
                                    userDevelopmentPlanMapping.Id,
                                    developmentPlanForCourse.DevelopmentName,

                                }).ToListAsync();


            var UserTeamsApplicability = await (from userDevelopmentPlanMapping in _db.UserDevelopmentPlanMapping
                                                join developmentPlanForCourse in _db.DevelopmentPlanForCourse on userDevelopmentPlanMapping.DevelopmentPlanid equals developmentPlanForCourse.Id
                                                join userTeams in _db.UserTeams on userDevelopmentPlanMapping.UserTeamId equals userTeams.Id
                                                into d
                                                from userTeams in d.DefaultIfEmpty()
                                                where (userDevelopmentPlanMapping.UserTeamId != null && userDevelopmentPlanMapping.UserTeamId != 0) && userDevelopmentPlanMapping.DevelopmentPlanid == DevelopmentPlanId && userDevelopmentPlanMapping.IsDeleted == false
                                                select new
                                                {
                                                    userDevelopmentPlanMapping.DevelopmentPlanid,
                                                    userDevelopmentPlanMapping.Id,
                                                    developmentPlanForCourse.DevelopmentName,
                                                    userDevelopmentPlanMapping.UserTeamId,
                                                    userTeams.TeamName
                                                }).ToListAsync();

            List<APIAccessibilityRulesDevelopment> AccessibilityRules = new List<APIAccessibilityRulesDevelopment>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int Developmentplanid = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("developmentplanid"))
                        Developmentplanid = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("DevelopmentName") &&
                        !rule.Name.Equals("DevelopmentPlanid") &&
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
                    i++;
                }
                if (Rules.Count == 2)
                {
                    APIAccessibilityRulesDevelopment ApiRule = new APIAccessibilityRulesDevelopment
                    {
                        DevelopmentPlanid = Developmentplanid,
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
                    APIAccessibilityRulesDevelopment ApiRule = new APIAccessibilityRulesDevelopment
                    {
                        DevelopmentPlanid = Developmentplanid,
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
            foreach (APIAccessibilityRulesDevelopment AccessRule in AccessibilityRules)
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
                    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count() > 0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter1;
                    AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Count() > 0 ? ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault() : AccessRule.AccessibilityParameter2;
                }
            }

            if (UserTeamsApplicability != null)
            {
                foreach (var item in UserTeamsApplicability)
                {
                    int DevelopmentPlan = 0;

                    DevelopmentPlan = Int32.Parse(item.DevelopmentPlanid.ToString());

                    APIAccessibilityRulesDevelopment accessRule = new APIAccessibilityRulesDevelopment
                    {
                        Id = item.Id,
                        AccessibilityParameter1 = "User Team",
                        AccessibilityValue1 = item.TeamName,
                        AccessibilityValueId1 = Int32.Parse(item.UserTeamId.ToString()),
                        DevelopmentPlanid = DevelopmentPlan
                    };
                    AccessibilityRules.Add(accessRule);
                }
            }

            return AccessibilityRules;
        }

        public FileInfo GetApplicableUserListExcel(List<APIAccessibilityRulesDevelopment> aPIAccessibilityRules, List<DevelopmentPlanApplicableUser> courseApplicableUsers, string DevelopmentPlanName, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"DevelopmentPlanApplicableUser.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Development Plan Applicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "Development Plan Name";
                row++;
                worksheet.Cells[row, column].Value = DevelopmentPlanName;
                row++;
                column = 1;
                row++;
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    worksheet.Cells[row, column++].Value = "Accessibility Value11";
                //    worksheet.Cells[row, column].Style.Font.Bold = true;
                //}
                worksheet.Cells[row, column++].Value = "Additional Criteria";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                //{
                //    worksheet.Cells[row, column++].Value = "Accessibility Value22";
                //    worksheet.Cells[row, column].Style.Font.Bold = true;
                //}

                foreach (APIAccessibilityRulesDevelopment course in aPIAccessibilityRules)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                    //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    worksheet.Cells[row, column++].Value = course.AccessibilityValue11 == null ? "-" : course.AccessibilityValue11;
                    //}
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

                foreach (DevelopmentPlanApplicableUser courseApplicableUser in courseApplicableUsers)
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

    }
}