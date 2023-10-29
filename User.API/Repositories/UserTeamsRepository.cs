using AutoMapper;
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
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class UserTeamsRepository : IUserTeamsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserTeamsRepository));
        private UserDbContext _db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IConfiguration _configuration;
        private IUserSettingsRepository _userSettingsRepository;
       private IUserRepository _userRepository;
        public UserTeamsRepository(UserDbContext context, ICustomerConnectionStringRepository customerConnectionString,
            IConfiguration configuration, IUserSettingsRepository userSettingsRepository, IUserRepository userReposirotry)
        {
            this._db = context;
            this._customerConnectionString = customerConnectionString;
            _configuration = configuration;
            this._userSettingsRepository = userSettingsRepository;
           this._userRepository = userReposirotry;
        }
        public async Task<int> SaveUserTeams(UserTeams userTeams)
        {
            if (userTeams == null)
            {
                return -1;
            }
            if (userTeams.TeamCode == null || string.IsNullOrEmpty(userTeams.TeamCode))
            {
                return -2;
            }
            if (userTeams.TeamName == null || string.IsNullOrEmpty(userTeams.TeamName))
            {
                return -3;
            }
            UserTeams userTeams1 = _db.UserTeams.Where(a => a.TeamCode == userTeams.TeamCode && a.IsDeleted == false).FirstOrDefault();
            if (userTeams1 != null)
            {
                return -4;
            }
            userTeams1 = _db.UserTeams.Where(a => a.TeamName == userTeams.TeamName && a.IsDeleted == false).FirstOrDefault();
            if (userTeams1 != null)
            {
                return -5;
            }

            await _db.UserTeams.AddAsync(userTeams);
            await _db.SaveChangesAsync();
            return 0;

        }
        public async Task<IEnumerable<UserTeams>> GetAllUserTeams(int page, int pageSize, string search = null, string columnName = null, int? userId = null)
        {
            List<UserTeams> userTeams = new List<UserTeams>();
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllUserTeams";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                //List<UserTeamApplicableUser> Result = GetUsersForuserTeam(row["TeamCode"].ToString());
                                var userTeam = new UserTeams
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
                                    CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
                                    ModifiedDate = Convert.ToDateTime(row["ModifiedDate"].ToString()),
                                    ModifiedBy = Convert.ToInt32(row["ModifiedBy"].ToString()),
                                    TeamCode = row["TeamCode"].ToString(),
                                    TeamName = row["TeamName"].ToString(),
                                    AboutTeam = row["AboutTeam"].ToString(),
                                    NumberOfRules = Convert.ToInt32(row["NumberOfRules"]),
                                    NumberofMembers = Convert.ToInt32(row["NumberofMembers"]),
                                    TeamStatus = Convert.ToBoolean(row["TeamStatus"]),
                                    EmailNotification = Convert.ToBoolean(row["EmailNotification"])
                                };
                                userTeams.Add(userTeam);
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return userTeams;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetAllUserTeamsCount(int page, int pageSize, string search = null, string columnName = null, int? userId = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = "GetUserTeamsCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = string.IsNullOrEmpty(row["count"].ToString()) ? 0 : Convert.ToInt32(row["count"].ToString());
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return Count;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;

        }
        public List<UserTeamApplicableUser> GetUsersForuserTeam(string TeamCode)
        {
            if (TeamCode == null)
            {
                return null;
            }
            else
            {
                UserTeams Result = GetUserTeamsByTeamsCode(TeamCode);

                if (Result == null)
                {
                    return null;
                }
                else
                {

                    List<UserTeamApplicableUser> listUserApplicability = new List<UserTeamApplicableUser>();

                    var connection = this._db.Database.GetDbConnection();
                    try
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetUserTeamApplicableUserList_Export";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("UserTeamId", SqlDbType.BigInt) { Value = Result.Id });

                            DbDataReader reader = cmd.ExecuteReader();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    UserTeamApplicableUser rule = new UserTeamApplicableUser();
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
                    
                    List<UserTeamApplicableUser> UserList1 = listUserApplicability.GroupBy(p => new { p.UserID, p.UserName })
                    .Select(g => g.First())
                    .ToList();
                    return UserList1;
                }
            }
        }
        public int DeleteUserTeams(string TeamCode)
        {
            try
            {
                if (TeamCode == null)
                {
                    return -1;
                }
                else
                {
                    UserTeams Result = GetUserTeamsByTeamsCode(TeamCode);

                    if (Result == null)
                    {
                        return -2;
                    }
                    else
                    {
                        Result.IsDeleted = true;
                        _db.UserTeams.Update(Result);
                        _db.SaveChanges();
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }
        public UserTeams GetUserTeamsByTeamsCode(string TeamCode)
        {
            if (TeamCode == null)
            {
                return null;
            }
            else
            {
                UserTeams userTeams = _db.UserTeams.Where(a => a.TeamCode == TeamCode && a.IsDeleted == false).FirstOrDefault();
                return userTeams;
            }
        }
        public UserTeams getUserById(int? TeamId)
        {

            UserTeams userTeams = _db.UserTeams.Where(a => a.Id == TeamId && a.IsDeleted == false).FirstOrDefault();
            return userTeams;

        }
        public async Task<int> UpdateUserTeams(UserTeams userTeams, int UserId)
        {
            if (userTeams == null)
            {
                return -1;
            }
            if (userTeams.TeamCode == null || string.IsNullOrEmpty(userTeams.TeamCode))
            {
                return -2;
            }
            if (userTeams.TeamName == null || string.IsNullOrEmpty(userTeams.TeamName))
            {
                return -3;
            }
            UserTeams userTeams2 = await _db.UserTeams.Where(a => a.TeamName == userTeams.TeamName && a.TeamCode != userTeams.TeamCode && a.IsDeleted == false).FirstOrDefaultAsync();

            if (userTeams2 != null)
            {
                return -4;
            }
            UserTeams oldUserTeams = await _db.UserTeams.Where(a => a.TeamCode == userTeams.TeamCode && a.IsDeleted == false).FirstOrDefaultAsync();

            oldUserTeams.TeamName = userTeams.TeamName;
            oldUserTeams.TeamStatus = userTeams.TeamStatus;
            oldUserTeams.NumberOfRules = userTeams.NumberOfRules;
            oldUserTeams.NumberofMembers = userTeams.NumberofMembers;
            oldUserTeams.ModifiedBy = UserId;
            oldUserTeams.ModifiedDate = DateTime.Now;
            oldUserTeams.AboutTeam = userTeams.AboutTeam;
            oldUserTeams.EmailNotification = userTeams.EmailNotification;

            this._db.UserTeams.Update(oldUserTeams);
            await this._db.SaveChangesAsync();
            return 0;
        }
        public async Task<List<APIUserTeamsType>> GetUserTeams(string search)
        {
            var data = (from c in _db.UserTeams

                        where (c.TeamName.Contains(search) && c.IsDeleted == false && c.TeamStatus == true)

                        select new APIUserTeamsType
                        {
                            Id = c.Id,
                            TeamCode = c.TeamCode,
                            TeamName = c.TeamName
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

                UserTeams userTeams = _db.UserTeams.Where(a => a.Id == mappingParameters.UserTeamsId && a.IsDeleted == false).FirstOrDefault();
                List<UserTeamApplicableUser> aPIUserMasterDetails = new List<UserTeamApplicableUser>();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (userTeams != null)
                {
                    var Mapping = _db.UserTeamsMapping.Where(a => a.UserTeamId == userTeams.Id);
                    UserTeamsMapping userTeamsMapping = new UserTeamsMapping();

                    switch (mappingParameters.AccessibilityParameter1.ToLower())
                    {

                        case "configurationcolumn1":
                            userTeamsMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "configurationcolumn2":

                            userTeamsMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;

                        case "configurationcolumn3":

                            userTeamsMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn4":

                            userTeamsMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "configurationcolumn5":

                            userTeamsMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn6":

                            userTeamsMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn7":

                            userTeamsMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn8":

                            userTeamsMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "configurationcolumn9":

                            userTeamsMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "configurationcolumn10":

                            userTeamsMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "configurationcolumn11":

                            userTeamsMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "configurationcolumn12":

                            userTeamsMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn13":

                            userTeamsMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn14":

                            userTeamsMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "configurationcolumn15":

                            userTeamsMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;

                        case "area":

                            userTeamsMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                            break;
                        case "business":

                            userTeamsMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "emailid":

                            userTeamsMapping.EmailID = mappingParameters.AccessibilityValue1;
                            break;
                        case "location":

                            userTeamsMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "group":

                            userTeamsMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue1);
                             break;
                        case "userid":

                            userTeamsMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue1);

                            break;
                        case "mobilenumber":

                            userTeamsMapping.MobileNumber = mappingParameters.AccessibilityValue1;
                            break;
                        case "joinedbefore":

                            userTeamsMapping.JoinedBefore = Convert.ToDateTime(mappingParameters.AccessibilityValue1);
                            break;
                        case "joinedafter":

                            userTeamsMapping.JoinedAfter = Convert.ToDateTime(mappingParameters.AccessibilityValue1);
                             break;

                        case "agelessthan":
                            userTeamsMapping.AgeLessThan = Convert.ToDateTime(mappingParameters.AccessibilityValue1);
                             break;

                        case "agegreaterthan":

                            userTeamsMapping.AgeGreaterThan = Convert.ToDateTime(mappingParameters.AccessibilityValue1);
                           break;
                    }


                    if (mappingParameters.AccessibilityParameter2 == null)
                    {

                        userTeamsMapping.ConditionForRules = mappingParameters.condition1;
                        userTeamsMapping.UserTeamId = mappingParameters.UserTeamsId;
                        userTeamsMapping.CreatedBy = UserId;
                        userTeamsMapping.ModifiedBy = UserId;
                        userTeamsMapping.CreatedDate = DateTime.Now;
                        userTeamsMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userTeamsMapping);

                        if (!Result)
                        {
                            userTeamsMapping.IsDeleted = false;
                            userTeamsMapping.MappingStatus = false;
                            this._db.UserTeamsMapping.Add(userTeamsMapping);
                            this._db.SaveChanges();

                            aPIUserMasterDetails = GetUsersForuserTeam(userTeams.TeamCode);
                            userTeams.NumberofMembers = aPIUserMasterDetails.Count();

                            userTeams.NumberOfRules++;
                            userTeams.ModifiedBy = UserId;
                            userTeams.ModifiedDate = DateTime.Now;

                            this._db.UserTeams.Update(userTeams);
                            this._db.SaveChanges();


                            aPIUserMasterDetails = new List<UserTeamApplicableUser>();

                            if (userTeams != null)
                            {
                                _db.Entry(userTeams).State = EntityState.Detached;
                            }
                        }
                        else
                        {
                            RejectMapping.UserTeamId = mappingParameters.UserTeamsId;
                            RejectMapping.AccessibilityParameter = mappingParameters.AccessibilityParameter1;
                            RejectMapping.ParameterValue = mappingParameters.AccessibilityValue1;
                            rejectMappingParameter.Add(RejectMapping);
                        }

                        if (userTeams != null)
                        {
                            _db.Entry(userTeams).State = EntityState.Detached;
                        }

                    }

                    if (mappingParameters.AccessibilityParameter2 != null)
                    {
                        switch (mappingParameters.AccessibilityParameter2.ToLower())
                        {
                            case "configurationcolumn1":
                                userTeamsMapping.ConfigurationColumn1 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;
                            case "configurationcolumn2":
                                userTeamsMapping.ConfigurationColumn2 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;

                            case "configurationcolumn3":
                                userTeamsMapping.ConfigurationColumn3 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn4":
                                userTeamsMapping.ConfigurationColumn4 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn5":
                                userTeamsMapping.ConfigurationColumn5 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn6":
                                userTeamsMapping.ConfigurationColumn6 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn7":
                                userTeamsMapping.ConfigurationColumn7 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn8":
                                userTeamsMapping.ConfigurationColumn8 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;

                            case "configurationcolumn9":
                                userTeamsMapping.ConfigurationColumn9 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "configurationcolumn10":
                                userTeamsMapping.ConfigurationColumn10 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn11":
                                userTeamsMapping.ConfigurationColumn11 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;

                            case "configurationcolumn12":
                                userTeamsMapping.ConfigurationColumn12 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn13":

                                userTeamsMapping.ConfigurationColumn13 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn14":

                                userTeamsMapping.ConfigurationColumn14 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "configurationcolumn15":

                                userTeamsMapping.ConfigurationColumn15 = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;
                            case "area":
                                userTeamsMapping.Area = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "business":
                                userTeamsMapping.Business = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;

                            case "emailid":
                                userTeamsMapping.EmailID = mappingParameters.AccessibilityValue2;
                                Mapping = Mapping.Where(a => a.EmailID == userTeamsMapping.EmailID);

                                userMasters = userMasters.Where(a => a.EmailId == Security.Encrypt(mappingParameters.AccessibilityValue2.ToLower()) && a.IsDeleted == false).ToList();
                                break;

                            case "location":
                                userTeamsMapping.Location = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "group":
                                userTeamsMapping.Group = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                 break;

                            case "userid":
                                userTeamsMapping.UserID = Convert.ToInt32(mappingParameters.AccessibilityValue2);
                                break;

                            case "mobilenumber":
                                userTeamsMapping.MobileNumber = mappingParameters.AccessibilityValue2;
                                  break;

                            case "joinedbefore":

                                userTeamsMapping.JoinedBefore = Convert.ToDateTime(mappingParameters.AccessibilityValue2);
                                break;
                            case "joinedafter":

                                userTeamsMapping.JoinedAfter = Convert.ToDateTime(mappingParameters.AccessibilityValue2);
                                  break;

                            case "agelessthan":
                                userTeamsMapping.AgeLessThan = Convert.ToDateTime(mappingParameters.AccessibilityValue2);
                               break;

                            case "agegreaterthan":

                                userTeamsMapping.AgeGreaterThan = Convert.ToDateTime(mappingParameters.AccessibilityValue2);
                                 break;
                        }


                        userTeamsMapping.ConditionForRules = mappingParameters.condition1;
                        userTeamsMapping.UserTeamId = mappingParameters.UserTeamsId;
                        userTeamsMapping.CreatedBy = UserId;
                        userTeamsMapping.ModifiedBy = UserId;
                        userTeamsMapping.CreatedDate = DateTime.Now;
                        userTeamsMapping.ModifiedDate = DateTime.Now;

                        bool Result = await RuleExist(userTeamsMapping);

                        if (!Result)
                        {
                            userTeamsMapping.IsDeleted = false;
                            userTeamsMapping.MappingStatus = false;
                            this._db.UserTeamsMapping.Add(userTeamsMapping);
                            this._db.SaveChanges();

                            aPIUserMasterDetails = GetUsersForuserTeam(userTeams.TeamCode);
                            userTeams.NumberofMembers = aPIUserMasterDetails.Count();
                            userTeams.NumberOfRules++;
                            userTeams.ModifiedBy = UserId;
                            userTeams.ModifiedDate = DateTime.Now;

                            this._db.UserTeams.Update(userTeams);
                            this._db.SaveChanges();

                            aPIUserMasterDetails = new List<UserTeamApplicableUser>();
                        }
                        else
                        {
                            RejectMapping.UserTeamId = mappingParameters.UserTeamsId;
                            RejectMapping.AccessibilityParameter = mappingParameters.AccessibilityParameter2;
                            RejectMapping.ParameterValue = mappingParameters.AccessibilityValue2;
                            rejectMappingParameter.Add(RejectMapping);
                        }
                    }

                    else
                    {
                        return null;
                    }


                    if (userTeams != null)
                    {
                        _db.Entry(userTeams).State = EntityState.Detached;
                    }

                    return rejectMappingParameter;
                }
                else
                {
                    RejectMapping.UserTeamId = mappingParameters.UserTeamsId;
                    rejectMappingParameter.Add(RejectMapping);

                    return rejectMappingParameter;
                }
            }
        }
        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int UserTeamsId)
        {
            if (AccessibilityParameter1 == null || AccessibilityValue1 == null)
            {
                return false;
            }
            bool isvalid = true;

            if (_db.UserTeams.Where(y => y.Id == UserTeamsId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (AccessibilityParameter1.ToLower() != "agelessthan" && AccessibilityParameter1.ToLower() != "joinedafter" &&
                                AccessibilityParameter1.ToLower() != "agelessthan" && AccessibilityParameter1.ToLower() != "agegreaterthan")
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
        public async Task<bool> RuleExist(UserTeamsMapping accessibilityRule)
        {
            IQueryable<UserTeamsMapping> Query = this._db.UserTeamsMapping.Where(a => a.UserTeamId == accessibilityRule.UserTeamId && a.IsDeleted == false);

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
            if (accessibilityRule.AgeGreaterThan != null)
                Query = Query.Where(a => a.AgeGreaterThan > accessibilityRule.AgeGreaterThan);
            if (accessibilityRule.AgeLessThan != null)
                Query = Query.Where(a => a.AgeLessThan < accessibilityRule.AgeLessThan);
            if (accessibilityRule.JoinedAfter != null)
                Query = Query.Where(a => a.JoinedAfter > accessibilityRule.JoinedAfter);
            if (accessibilityRule.JoinedBefore != null)
                Query = Query.Where(a => a.JoinedBefore < accessibilityRule.JoinedBefore);

            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }
        public async Task<List<MappingParameters>> GetAccessibilityRules(int UserTeamsId, string orgnizationCode, string token, int Page, int PageSize)
        {
            var Result = await (from userTeamsMapping in _db.UserTeamsMapping
                                join userTeams in _db.UserTeams on userTeamsMapping.UserTeamId equals userTeams.Id
                                into c
                                from userTeams in c.DefaultIfEmpty()
                                where userTeamsMapping.UserTeamId == UserTeamsId && userTeamsMapping.IsDeleted == false
                                select new
                                {
                                    userTeamsMapping.ConfigurationColumn1,
                                    userTeamsMapping.ConfigurationColumn2,
                                    userTeamsMapping.ConfigurationColumn3,
                                    userTeamsMapping.ConfigurationColumn4,
                                    userTeamsMapping.ConfigurationColumn5,
                                    userTeamsMapping.ConfigurationColumn6,
                                    userTeamsMapping.ConfigurationColumn7,
                                    userTeamsMapping.ConfigurationColumn8,
                                    userTeamsMapping.ConfigurationColumn9,
                                    userTeamsMapping.ConfigurationColumn10,
                                    userTeamsMapping.ConfigurationColumn11,
                                    userTeamsMapping.ConfigurationColumn12,
                                    userTeamsMapping.ConfigurationColumn13,
                                    userTeamsMapping.ConfigurationColumn14,
                                    userTeamsMapping.ConfigurationColumn15,
                                    userTeamsMapping.Area,
                                    userTeamsMapping.Business,
                                    userTeamsMapping.EmailID,
                                    userTeamsMapping.MobileNumber,
                                    userTeamsMapping.Location,
                                    userTeamsMapping.Group,
                                    userTeamsMapping.UserID,
                                    userTeamsMapping.ConditionForRules,
                                    userTeamsMapping.UserTeamId,
                                    userTeamsMapping.Id,
                                    userTeams.TeamName,
                                    userTeamsMapping.AgeGreaterThan,
                                    userTeamsMapping.AgeLessThan,
                                    userTeamsMapping.JoinedAfter,
                                    userTeamsMapping.JoinedBefore

                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();
            List<MappingParameters> AccessibilityRules = new List<MappingParameters>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int UserTeamsID1 = 0;
                int Id = 0;
                int i = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower().Equals("userteamid"))
                        UserTeamsID1 = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower().Equals("id"))
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        !rule.Name.Equals("ConditionForRules") &&
                        !rule.Name.Equals("TeamName") &&
                        !rule.Name.Equals("UserTeamId") &&
                        !rule.Name.Equals("Id"))
                    {
                        if (string.Equals(rule.Name, "joinedbefore", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(rule.Name, "joinedafter", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(rule.Name, "agelessthan", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(rule.Name, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var applicationDateFormat = await GetMasterConfigurableParameterValue("APPLICATION_DATE_FORMAT");
                            Rules RuleDoj = new Rules
                            {
                                AccessibilityParameter = rule.Name,
                                AccessibilityValue = Convert.ToDateTime(rule.GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                //AccessibilityValue2 = Convert.ToDateTime(properties[++i].GetValue(AccessRule).ToString()).ToString(applicationDateFormat),
                                Condition = Condition
                            };
                            Rules.Add(RuleDoj);
                        }
                        else
                        {
                            Rules Rule = new Rules
                            {
                                AccessibilityParameter = rule.Name,
                                AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                                Condition = Condition,
                                Id = Id
                            };
                            Rules.Add(Rule);
                        }
                    }
                    i++;
                }
                if (Rules.Count == 2)
                {
                    if (string.Equals(Rules.ElementAt(0).AccessibilityParameter, "joinedbefore", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(Rules.ElementAt(0).AccessibilityParameter, "joinedafter", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agelessthan", StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MappingParameters ApiRule = new MappingParameters
                        {
                            Id = Id,
                            UserTeamsId = UserTeamsID1,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = 0,
                            AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                            // AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                            condition1 = "and",
                            AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                            AccessibilityValueId2 = 0,
                            AccessibilityValue2 = Rules.ElementAt(1).AccessibilityValue,
                            //AccessibilityValue22 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue2 : null,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    else
                    {
                        MappingParameters ApiRule = new MappingParameters
                        {
                            Id = Id,
                            UserTeamsId = UserTeamsID1,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                            AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                            // AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                            condition1 = "and",
                            AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                            AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue),
                            AccessibilityValue2 = Rules.ElementAt(1).AccessibilityValue,
                            //AccessibilityValue22 = string.Equals(Rules.ElementAt(1).AccessibilityParameter, "dateofjoining", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(1).AccessibilityValue2 : null,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }


                }
                else if (Rules.Count == 1)
                {
                    if (string.Equals(Rules.ElementAt(0).AccessibilityParameter, "joinedbefore", StringComparison.CurrentCultureIgnoreCase) ||
                           string.Equals(Rules.ElementAt(0).AccessibilityParameter, "joinedafter", StringComparison.CurrentCultureIgnoreCase) ||
                           string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agelessthan", StringComparison.CurrentCultureIgnoreCase) ||
                           string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase))
                    {
                        MappingParameters ApiRule = new MappingParameters
                        {
                            UserTeamsId = UserTeamsID1,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = 0,
                            AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                            //AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    else
                    {
                        MappingParameters ApiRule = new MappingParameters
                        {
                            UserTeamsId = UserTeamsID1,
                            Id = Id,
                            AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                            AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                            AccessibilityValue1 = Rules.ElementAt(0).AccessibilityValue,
                            //AccessibilityValue11 = string.Equals(Rules.ElementAt(0).AccessibilityParameter, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase) ? Rules.ElementAt(0).AccessibilityValue2 : null,
                        };
                        AccessibilityRules.Add(ApiRule);
                    }
                    
                }
            }

            var userSettings = await this._userSettingsRepository.GetAllUserSetting(1, 20);

            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            foreach (APIUserSetting aPIUserSetting in userSettings)
            {
                ConfiguredColumns configuredColumns = new ConfiguredColumns();
                configuredColumns.ChangedColumnName = aPIUserSetting.ChangedColumnName;
                configuredColumns.ConfiguredColumnName = aPIUserSetting.ConfiguredColumnName;
                ConfiguredColumns.Add(configuredColumns);
            }


            foreach (MappingParameters AccessRule in AccessibilityRules)
            {
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                Title _Title = await GetNameById(ColumnName, Value);

                if (!(string.Equals(AccessRule.AccessibilityParameter1, "joinedbefore", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "joinedafter", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "agelessthan", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase)))
                {
                    AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }
                else
                {
                    AccessRule.AccessibilityParameter1 = AccessRule.AccessibilityParameter1.ToLower();
                    if(AccessRule.AccessibilityParameter2 != null)
                    {
                        AccessRule.AccessibilityParameter2 = AccessRule.AccessibilityParameter2.ToLower();

                    }
                }

                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    _Title = await GetNameById(ColumnName, Value);

                    if (!(string.Equals(AccessRule.AccessibilityParameter1, "joinedbefore", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "joinedafter", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "agelessthan", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(AccessRule.AccessibilityParameter1, "agegreaterthan", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter2 = AccessRule.AccessibilityParameter2.ToLower();
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
                    else if (AccessRule.AccessibilityParameter1 == "joinedbefore")
                    {
                        AccessRule.AccessibilityParameter1 = "Joined Before";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "joinedafter")
                    {
                        AccessRule.AccessibilityParameter1 = "Joined After";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "agelessthan")
                    {
                        AccessRule.AccessibilityParameter1 = "Age Less Than";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "agegreaterthan")
                    {
                        AccessRule.AccessibilityParameter1 = "Age Greater Than";
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
                    else if (AccessRule.AccessibilityParameter2 == "joinedbefore")
                    {
                        AccessRule.AccessibilityParameter2 = "Joined Before";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "joinedafter")
                    {
                        AccessRule.AccessibilityParameter2 = "Joined After";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "agelessthan")
                    {
                        AccessRule.AccessibilityParameter2 = "Age Less Than";
                    }
                    else if (AccessRule.AccessibilityParameter2 == "agegreaterthan")
                    {
                        AccessRule.AccessibilityParameter2 = "Age Greater Than";
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                }

            }
            return AccessibilityRules;
        }
        public async Task<int> GetAccessibilityRulesCount(int UserTeamsId)
        {
            int Count = 0;
            Count = await (from userTeamsMapping in _db.UserTeamsMapping
                           join userTeams in _db.UserTeams on userTeamsMapping.UserTeamId equals userTeams.Id
                           into c
                           from userTeams in c.DefaultIfEmpty()
                           where userTeamsMapping.UserTeamId == UserTeamsId && userTeamsMapping.IsDeleted == false
                           select new
                           {
                               userTeamsMapping.ConfigurationColumn1,
                               userTeamsMapping.ConfigurationColumn2,
                               userTeamsMapping.ConfigurationColumn3,
                               userTeamsMapping.ConfigurationColumn4,
                               userTeamsMapping.ConfigurationColumn5,
                               userTeamsMapping.ConfigurationColumn6,
                               userTeamsMapping.ConfigurationColumn7,
                               userTeamsMapping.ConfigurationColumn8,
                               userTeamsMapping.ConfigurationColumn9,
                               userTeamsMapping.ConfigurationColumn10,
                               userTeamsMapping.ConfigurationColumn11,
                               userTeamsMapping.ConfigurationColumn12,
                               userTeamsMapping.ConfigurationColumn13,
                               userTeamsMapping.ConfigurationColumn14,
                               userTeamsMapping.ConfigurationColumn15,
                               userTeamsMapping.Area,
                               userTeamsMapping.Business,
                               userTeamsMapping.EmailID,
                               userTeamsMapping.MobileNumber,
                               userTeamsMapping.Location,
                               userTeamsMapping.Group,
                               userTeamsMapping.UserID,
                               userTeamsMapping.ConditionForRules,
                               userTeamsMapping.UserTeamId,
                               userTeamsMapping.Id,
                               userTeams.TeamName,
                               userTeamsMapping.AgeGreaterThan,
                               userTeamsMapping.AgeLessThan,
                               userTeamsMapping.JoinedAfter,
                               userTeamsMapping.JoinedBefore

                           }).CountAsync();
            return Count;
        }
        public async Task<Title> GetNameById(string searchBy, int Id)
        {
            searchBy = searchBy.ToLower();
            switch (searchBy)
            {
                case "userid":
                    return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new Title { Name = Security.Decrypt(e.UserId) }).FirstOrDefaultAsync();
                case "emailid":
                    return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new Title { Name = Security.Decrypt(e.EmailId) }).FirstOrDefaultAsync();
                case "mobilenumber":
                    return await _db.UserMasterDetails.Where(u => u.IsDeleted == false && u.UserMasterId == Id).Select(e => new Title { Name = Security.Decrypt(e.MobileNumber) }).FirstOrDefaultAsync();
                case "userrole":
                    return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new Title { Name = e.UserRole }).FirstOrDefaultAsync();
                case "business":
                    return await _db.Business.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "group":
                    return await _db.Group.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "area":
                    return await _db.Area.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "location":
                    return await _db.Location.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn1":
                    return await _db.Configure1.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn2":
                    return await _db.Configure2.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn3":
                    return await _db.Configure3.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn4":
                    return await _db.Configure4.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn5":
                    return await _db.Configure5.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn6":
                    return await _db.Configure6.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn7":
                    return await _db.Configure7.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn8":
                    return await _db.Configure8.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn9":
                    return await _db.Configure9.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn10":
                    return await _db.Configure10.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn11":
                    return await _db.Configure11.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn12":
                    return await _db.Configure12.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn13":
                    return await _db.Configure13.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn14":
                    return await _db.Configure14.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "configurationcolumn15":
                    return await _db.Configure15.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new Title { Name = e.Name }).FirstOrDefaultAsync();
                case "username":
                    return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new Title { Name = e.UserName }).FirstOrDefaultAsync();
            }
            return null;
        }
       
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
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
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }
        public async Task<int> DeleteRule(int roleId)
        {
            UserTeamsMapping accessibilityRule = _db.UserTeamsMapping.Where(a => a.Id == roleId && a.IsDeleted == false).FirstOrDefault();

            if (accessibilityRule != null)
            {
                UserTeams userTeams = _db.UserTeams.Where(a => a.Id == accessibilityRule.UserTeamId && a.IsDeleted == false).FirstOrDefault();

                accessibilityRule.IsDeleted = true;
                _db.UserTeamsMapping.Update(accessibilityRule);
                await _db.SaveChangesAsync();

                List<UserTeamApplicableUser> aPIUserMasterDetails = new List<UserTeamApplicableUser>();
                List<UserMaster> userMasters = new List<UserMaster>();

                if (userTeams != null)
                {
                    if (userTeams.TeamCode != null)
                    {
                        aPIUserMasterDetails = GetUsersForuserTeam(userTeams.TeamCode);
                        userTeams.NumberofMembers = aPIUserMasterDetails.Count();
                        userTeams.NumberOfRules = userTeams.NumberOfRules - 1;
                        _db.UserTeams.Update(userTeams);
                        await _db.SaveChangesAsync();
                        return 1;
                    }
                }

            }
            return 0;
        }
        public ApiResponse ProcessImportFile(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
        {
            try
            {
                return new UserTeamsMappingImport.ProcessFile(this._db).ProcessRecords(file, _customerConnectionRepository, userid, _configuration, orgcode);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<UserTeamsMapping> GetRuleByUserTeams(int UserTeamId)
        {
            List<UserTeamsMapping> accessibilityRule = _db.UserTeamsMapping.Where(a => a.UserTeamId == UserTeamId && a.IsDeleted == false).ToList();

            return accessibilityRule;
        }
        public FileInfo GetApplicableUserListExcel(List<MappingParameters> aPIAccessibilityRules, List<UserTeamApplicableUser> userTeamApplicableUsers, string CourseName, string OrgCode)
        {

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"UserTeamApplicableUser.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("UserTeam Applicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "UserTeam Name";
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
                if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
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
                if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                {
                    worksheet.Cells[row, column++].Value = "Accessibility Value22";
                    worksheet.Cells[row, column].Style.Font.Bold = true;
                }

                foreach (MappingParameters course in aPIAccessibilityRules)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                    //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    worksheet.Cells[row, column++].Value = course.AccessibilityValue11 == null ? "-" : course.AccessibilityValue11;
                    //}
                    worksheet.Cells[row, column++].Value = course.condition1 == null ? "-" : course.condition1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter2 == null ? "-" : course.AccessibilityParameter2;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue2 == null ? "-" : course.AccessibilityValue2;
                    //if (string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    worksheet.Cells[row, column++].Value = course.AccessibilityValue22 == null ? "-" : course.AccessibilityValue22;
                    //}
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

                foreach (UserTeamApplicableUser courseApplicableUser in userTeamApplicableUsers)
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
