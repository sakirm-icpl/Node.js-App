using Courses.API.APIModel;
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
using CourseApplicability.API.Repositories;
using CourseApplicability.API.Model;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Models;
using CourseApplicability.API.Helper;

namespace CourseApplicability.API.Repositories
{
    public class AccessibiltyRuleRepository : Repository<AccessibilityRule>, IAccessibilityRule
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private CoursesApplicabilityContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;

        public AccessibiltyRuleRepository(CoursesApplicabilityContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
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

                        var configParameter = await GetMasterConfigurableParameterValue("SELFENROLL_EMAIL");
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

        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

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
    }
}