using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Metadata;
using User.API.Models;
using User.API.Repositories.Interfaces;
using Dapper;

namespace User.API.Repositories
{
    public class RewardsPointRepository : IRewardsPoint
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RewardsPointRepository));
        private UserDbContext _db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public RewardsPointRepository(UserDbContext db, ICustomerConnectionStringRepository customerConnectionString)
        {
            this._customerConnectionString = customerConnectionString;
            this._db = db;
        }
        public async Task<int> AddUserLoginStatiticsForRewardPoints(UserLoginStatiticsForRewardPoints userLoginSatatitics)
        {
            if (userLoginSatatitics != null)
            {
                this._db.UserLoginStatiticsForRewardPoints.Add(userLoginSatatitics);
                await this._db.SaveChangesAsync();
                return 1;
            }
            return 0;

        }
        public async Task<int> UpdateUserLoginStatiticsForRewardPoints(UserLoginStatiticsForRewardPoints userLoginSatatitics)
        {
            if (userLoginSatatitics != null)
            {
                this._db.UserLoginStatiticsForRewardPoints.Update(userLoginSatatitics);
                await this._db.SaveChangesAsync();
                return 1;
            }
            return 0;

        }
        public async Task<int> AddDailyLoginRewardPoints(int userId)
        {
            string Category = RewardPointCategory.Normal;
            string FunctionCode = "DailyLogin";
            string Description = "Daily Login";
            int referenceId = userId;
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }

        public async Task<int> AddJobAid(int userId, string Title)
        {
            string Category = RewardPointCategory.Normal;

            string FunctionCode = "TLS0670";

            int referenceId = userId;
            string Description = "You have read/observed job aid" + " " + Title;
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }

        public async Task<int> AddKeySetting(int userId, string Title)
        {
            string Category = RewardPointCategory.Normal;
            string FunctionCode = "TLS0660";
            int referenceId = userId;
            string Description = "You have read key result area" + " " + Title;
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }
        public async Task<int> AddFirstTimeLoginRewardPoints(int userId)
        {
            string Category = RewardPointCategory.Normal;
            string FunctionCode = "FirstTimeLogin";
            string Description = "First Time Login";
            int referenceId = userId;
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }
        public async Task<int> FirstTimeLoggedInLogin(int UserId)
        {
            try
            {
                int Count = 0;
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();


                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "FirstTimeLoginCheckInRewardPointsUser";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
                    }
                    return Count;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> DailyLoggedInLogin(int UserId)
        {
            try
            {
                int Count = 0;
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();


                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DailyLoginCheckInRewardPointsUser";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
                    }
                    return Count;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> AddLoginRewardPoints(int userId)
        {
            DateTime CurrentLoginTime = DateTime.UtcNow;
            UserLoginStatiticsForRewardPoints UserLoginStatitics = await this._db.UserLoginStatiticsForRewardPoints.Where(l => l.UserId == userId).FirstOrDefaultAsync();
            if (UserLoginStatitics == null)
            {
                UserLoginStatitics = new UserLoginStatiticsForRewardPoints
                {
                    UserId = userId,
                    LastLoginDateTime = DateTime.UtcNow,
                    SevenDayCount = 1,
                    EightHoursCount = 1
                };
                int Result = await this.AddUserLoginStatiticsForRewardPoints(UserLoginStatitics);
                return Result;
            }
            else
            {
                if (UserLoginStatitics.SevenDayCount < 7)
                {
                    UserLoginStatitics.SevenDayCount = GetSevenDayCount(CurrentLoginTime, UserLoginStatitics.LastLoginDateTime, UserLoginStatitics.SevenDayCount);
                    if (UserLoginStatitics.SevenDayCount == 7)
                    {
                    }
                }

                if (UserLoginStatitics.EightHoursCount < 4)
                {
                    UserLoginStatitics.EightHoursCount = GetEightHoursCount(CurrentLoginTime, UserLoginStatitics.LastLoginDateTime, UserLoginStatitics.EightHoursCount);
                    if (UserLoginStatitics.EightHoursCount == 4)
                        await AddEightHoursRewardPoint(userId);
                }
                UserLoginStatitics.LastLoginDateTime = DateTime.UtcNow;
                return await this.UpdateUserLoginStatiticsForRewardPoints(UserLoginStatitics);
            }

        }
        private int GetSevenDayCount(DateTime currentLoginDate, DateTime lastLoginDate, int sevenDayCount)
        {
            int DateDiff = Convert.ToInt32((currentLoginDate.Date - lastLoginDate.Date).TotalDays);

            if (DateDiff == 1)
                return sevenDayCount += 1;

            if (DateDiff == 0)
                return sevenDayCount;

            return 1;
        }

        private int GetEightHoursCount(DateTime currentLoginDate, DateTime lastLoginDate, int eightHoursCount)
        {
            TimeSpan Difference = currentLoginDate - lastLoginDate;
            int Hours = Convert.ToInt32(Difference.TotalHours);

            if (Hours >= 8 && Hours < 10)
                return eightHoursCount = +1;
            if (Hours < 8 && Hours >= 0)
                return eightHoursCount;
            return 1;
        }

        private async Task<int> AddSevenDayRewardPoint(int userId)
        {
            int Points = 10;
            string FunctionCode = "SevenDayLogin";
            string Category = "Special";
            string Description = "You have completed Seven day login challange";
            return await AddRewardsPoint(userId, Points, FunctionCode, Category, Description);
        }
        private async Task<int> AddEightHoursRewardPoint(int userId)
        {
            int Points = 10;
            string FunctionCode = "EightHoursLogin";
            string Category = "Special";
            string Description = "You have completed Eight hours login challange";
            return await AddRewardsPoint(userId, Points, FunctionCode, Category, Description);
        }

        private async Task<int> AddRewardsPoint(int userId, int? points, string functionCode, string category, string description)
        {

            int referenceId = userId;
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "RewardPointsDaily_Upsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.VarChar) { Value = functionCode });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.VarChar) { Value = category });
                        cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                        cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = points });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = description });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return 1;
        }

        public async Task<IEnumerable<APIRanking>> GetTopRanking(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string OrgCode = null, string configuredColumnValue = null, string isinstitute = "false")
        {
            List<APIRanking> ranking = new List<APIRanking>();
            try
            {
                if (isinstitute.ToLower() == "false")
                {
                    using (var dbContext = _customerConnectionString.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetTopRanking";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnValue", SqlDbType.VarChar) { Value = configuredColumnValue });

                            cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                            cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    APIRanking aPIRanking = new APIRanking();
                                    if (OrgCode.Contains("sbil"))
                                    {
                                        aPIRanking.ProfilePicture = null;
                                    }
                                    else
                                    {
                                        aPIRanking.ProfilePicture = row["ProfilePicture"].ToString();
                                    }
                                    var rank = new APIRanking
                                    {
                                        EUSerId = row["UserID"].ToString(),
                                        UserName = row["UserName"].ToString(),
                                        TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),

                                        ProfilePicture = aPIRanking.ProfilePicture,
                                        Gender = row["Gender"].ToString(),
                                        Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                        Level = row["Level"].ToString(),
                                        MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                        LevelCode = row["LevelCode"].ToString(),
                                        HouseCode = row["HouseCode"].ToString(),
                                        HouseName = row["HouseName"].ToString(),
                                        eId = (Security.EncryptForUI(row["ID"].ToString())),
                                        country = row["country"].ToString()
                                    };
                                    ranking.Add(rank);
                                }
                            }
                            reader.Dispose();
                        }
                        return ranking;
                    }
                }
                else
                {
                    using (var dbContext = _customerConnectionString.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetTopRanking_WithIsInstitute";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnValue", SqlDbType.VarChar) { Value = configuredColumnValue });

                            cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                            cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    APIRanking aPIRanking = new APIRanking();
                                    if (OrgCode.Contains("sbil"))
                                    {
                                        aPIRanking.ProfilePicture = null;
                                    }
                                    else
                                    {
                                        aPIRanking.ProfilePicture = row["ProfilePicture"].ToString();
                                    }
                                    var rank = new APIRanking
                                    {
                                        EUSerId = row["UserID"].ToString(),
                                        UserName = row["UserName"].ToString(),
                                        TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),

                                        ProfilePicture = aPIRanking.ProfilePicture,
                                        Gender = row["Gender"].ToString(),
                                        Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                        Level = row["Level"].ToString(),
                                        MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                        LevelCode = row["LevelCode"].ToString(),
                                        HouseCode = row["HouseCode"].ToString(),
                                        HouseName = row["HouseName"].ToString(),
                                        eId = (Security.EncryptForUI(row["ID"].ToString())),
                                    };
                                    ranking.Add(rank);
                                }
                            }
                            reader.Dispose();
                        }
                        return ranking;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<APILeaderBoardData> GetLeaderBoardData(string UserID)
        {
            using (var dbContext = _customerConnectionString.GetDbContext())
            {
                System.Data.Common.DbConnection connection = dbContext.Database.GetDbConnection();
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UserId", UserID);
                SqlMapper.GridReader Result = await SqlMapper.QueryMultipleAsync((SqlConnection)connection, "[dbo].[LeaderBoard_AllUserLeaderBoardDetails]", parameters, null, null, CommandType.StoredProcedure);
                APILeaderBoardData LeaderBoardData = new APILeaderBoardData
                {
                    AllUsers = Result.Read<APILeaderBoardData1>().ToList(),
                    SpecificUser = Result.Read<APILeaderBoardData1>().ToList()
                };
                connection.Close();
                return LeaderBoardData;
            }
            
        }

        public async Task<List<APIRewardLeaderBoard>> GetDatewiseLeaderBoardData(APIRewardLeaderBoardDate APIRewardPointsSummery, int UserId)
        {
            List<APIRewardLeaderBoard> APIRewardLeaderBoard = new List<APIRewardLeaderBoard>();
            IEnumerable<APIRewardPointsSummery> RewardPointSummary = null;  
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@FromDate", APIRewardPointsSummery.FromDate);
                    parameters.Add("@ToDate", APIRewardPointsSummery.ToDate);
                    parameters.Add("@UserId", UserId);

                    IEnumerable<APIRewardPointsSummery> Result = await SqlMapper.QueryAsync<APIRewardPointsSummery>((SqlConnection)connection, "[dbo].[Report_RewardPointsSummeryExport]", parameters, null, null, CommandType.StoredProcedure);
                    RewardPointSummary = Result.Take(APIRewardPointsSummery.Rank).ToList();
                    int rank = 0;
                    foreach(var item in RewardPointSummary)
                    {
                        APIRewardLeaderBoard aPIRewardLeaderBoard = new APIRewardLeaderBoard();
                        UserMaster um = _db.UserMaster.Where(a => a.UserId == item.UserId).FirstOrDefault();
                        aPIRewardLeaderBoard.UserName = um.UserName;
                        aPIRewardLeaderBoard.UserId = Security.Decrypt(item.UserId);
                        aPIRewardLeaderBoard.Gender = _db.UserMasterDetails.Where(a => a.UserMasterId == um.Id).Select(a => a.Gender).FirstOrDefault();
                        aPIRewardLeaderBoard.ProfilePicture = _db.UserMasterDetails.Where(a => a.UserMasterId == um.Id).Select(a => a.ProfilePicture).FirstOrDefault();
                        aPIRewardLeaderBoard.Points = item.Points;
                        aPIRewardLeaderBoard.Rank = ++rank;
                        aPIRewardLeaderBoard.EId = (Security.EncryptForUI(um.Id.ToString()));
                        APIRewardLeaderBoard.Add(aPIRewardLeaderBoard);
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));


            }
            return APIRewardLeaderBoard;
        }



        public async Task<IEnumerable<APIRankingExport>> GetTopRankingForExport(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string OrgCode = null, string configuredColumnValue = null, string IsInstitute = "false")
        {

            List<APIRankingExport> ranking = new List<APIRankingExport>();
            if (OrgCode.Contains("iocl"))
            {
                try
                {
                    if (IsInstitute.ToLower() == "false")
                    {
                        using (var dbContext = _customerConnectionString.GetDbContext())
                        {
                            var connection = dbContext.Database.GetDbConnection();
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();

                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetTopRankingForExport";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                                cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                                cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnValue", SqlDbType.VarChar) { Value = configuredColumnValue });

                                cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                                cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count > 0)
                                {
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        APIRankingExport aPIRanking = new APIRankingExport();
                                        if (OrgCode.Contains("sbil"))
                                        {
                                            aPIRanking.ProfilePicture = null;
                                        }
                                        else
                                        {
                                            aPIRanking.ProfilePicture = row["ProfilePicture"].ToString();
                                        }

                                        var rank = new APIRankingExport
                                        {
                                            EUSerId = row["UserID"].ToString(),
                                            UserName = row["UserName"].ToString(),
                                            TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),

                                            ProfilePicture = aPIRanking.ProfilePicture,
                                            Gender = row["Gender"].ToString(),
                                            Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                            Level = row["Level"].ToString(),
                                            MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                            LevelCode = row["LevelCode"].ToString(),
                                            HouseCode = row["HouseCode"].ToString(),
                                            HouseName = row["HouseName"].ToString(),
                                            eId = (Security.EncryptForUI(row["ID"].ToString())),
                                            UserCategory = row["UserCategory"].ToString(),
                                            SalesArea = row["SalesArea"].ToString(),
                                            SalesOfficer = row["SalesOfficer"].ToString(),
                                            ControllingOffice = row["ControllingOffice"].ToString(),
                                            District = row["District"].ToString(),
                                            Designation = row["Designation"].ToString(),
                                            UserStatus = Convert.ToBoolean(row["IsActive"].ToString())
                                        };
                                        ranking.Add(rank);
                                    }
                                }
                                reader.Dispose();
                            }
                            return ranking;
                        }
                    }
                    else
                    {
                        using (var dbContext = _customerConnectionString.GetDbContext())
                        {
                            var connection = dbContext.Database.GetDbConnection();
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();

                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetTopRankingForExport_WithIsInstitute";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                                cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                                cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnValue", SqlDbType.VarChar) { Value = configuredColumnValue });

                                cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                                cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count > 0)
                                {
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        APIRankingExport aPIRanking = new APIRankingExport();
                                        if (OrgCode.Contains("sbil"))
                                        {
                                            aPIRanking.ProfilePicture = null;
                                        }
                                        else
                                        {
                                            aPIRanking.ProfilePicture = row["ProfilePicture"].ToString();
                                        }

                                        var rank = new APIRankingExport
                                        {
                                            EUSerId = row["UserID"].ToString(),
                                            UserName = row["UserName"].ToString(),
                                            TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),

                                            ProfilePicture = aPIRanking.ProfilePicture,
                                            Gender = row["Gender"].ToString(),
                                            Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                            Level = row["Level"].ToString(),
                                            MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                            LevelCode = row["LevelCode"].ToString(),
                                            HouseCode = row["HouseCode"].ToString(),
                                            HouseName = row["HouseName"].ToString(),
                                            eId = (Security.EncryptForUI(row["ID"].ToString())),
                                            UserCategory = row["UserCategory"].ToString(),
                                            SalesArea = row["SalesArea"].ToString(),
                                            SalesOfficer = row["SalesOfficer"].ToString(),
                                            ControllingOffice = row["ControllingOffice"].ToString(),
                                            District = row["District"].ToString(),
                                            Designation = row["Designation"].ToString(),
                                            UserStatus = Convert.ToBoolean(row["IsActive"].ToString())
                                        };
                                        ranking.Add(rank);
                                    }
                                }
                                reader.Dispose();
                            }
                            return ranking;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw ex;
                }
            }
            else
            {
                try
                {
                    using (var dbContext = _customerConnectionString.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetTopRankingForExport";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnValue", SqlDbType.VarChar) { Value = configuredColumnValue });

                            cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                            cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    APIRankingExport aPIRanking = new APIRankingExport();
                                    if (OrgCode.Contains("sbil"))
                                    {
                                        aPIRanking.ProfilePicture = null;
                                    }
                                    else
                                    {
                                        aPIRanking.ProfilePicture = row["ProfilePicture"].ToString();
                                    }

                                    var rank = new APIRankingExport
                                    {
                                        EUSerId = row["UserID"].ToString(),
                                        UserName = row["UserName"].ToString(),
                                        TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),

                                        ProfilePicture = aPIRanking.ProfilePicture,
                                        Gender = row["Gender"].ToString(),
                                        Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                        Level = row["Level"].ToString(),
                                        MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                        LevelCode = row["LevelCode"].ToString(),
                                        HouseCode = row["HouseCode"].ToString(),
                                        HouseName = row["HouseName"].ToString(),
                                        eId = (Security.EncryptForUI(row["ID"].ToString()))

                                    };
                                    ranking.Add(rank);
                                }
                            }
                            reader.Dispose();
                        }
                        return ranking;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return ranking;
        }

        public async Task<IEnumerable<APIRanking>> GetMyRanking(int? ranks, int? id, string configuredColumnName, string houseCode, bool alwaysShowUserDetails, string isinstitute = "false")
        {
            List<APIRanking> ranking = new List<APIRanking>();
            try
            {
                if (isinstitute.ToLower() == "false")
                {
                    using (var dbContext = _customerConnectionString.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetMyRanking";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                            cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                            cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    var rank = new APIRanking
                                    {
                                        EUSerId = row["UserID"].ToString(),
                                        UserName = row["UserName"].ToString(),
                                        TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),
                                        ProfilePicture = row["ProfilePicture"].ToString(),
                                        Gender = row["Gender"].ToString(),
                                        Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                        Level = row["Level"].ToString(),
                                        MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                        LevelCode = row["LevelCode"].ToString(),
                                        HouseCode = row["HouseCode"].ToString(),
                                        HouseName = row["HouseName"].ToString(),
                                        eId = (Security.EncryptForUI(row["ID"].ToString())),
                                    };
                                    ranking.Add(rank);
                                }
                            }
                            reader.Dispose();
                        }
                        return ranking;
                    }
                }
                else
                {
                    using (var dbContext = _customerConnectionString.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetMyRanking_WithIsInstitute";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ranks", SqlDbType.Int) { Value = ranks });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@ConfiguredColumnName", SqlDbType.VarChar) { Value = configuredColumnName });
                            cmd.Parameters.Add(new SqlParameter("@HouseCode", SqlDbType.VarChar) { Value = houseCode });
                            cmd.Parameters.Add(new SqlParameter("@alwaysShowUserDetails", SqlDbType.Bit) { Value = alwaysShowUserDetails });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    var rank = new APIRanking
                                    {
                                        EUSerId = row["UserID"].ToString(),
                                        UserName = row["UserName"].ToString(),
                                        TotalPoint = string.IsNullOrEmpty(row["TotalPoint"].ToString()) ? 0 : Convert.ToInt32(row["TotalPoint"].ToString()),
                                        Gender = row["Gender"].ToString(),
                                        Rank = string.IsNullOrEmpty(row["Rank"].ToString()) ? 0 : Convert.ToInt32(row["Rank"].ToString()),
                                        Level = row["Level"].ToString(),
                                        MaximumLevelPoint = string.IsNullOrEmpty(row["MaximumLevelPoint"].ToString()) ? 0 : Convert.ToInt32(row["MaximumLevelPoint"].ToString()),
                                        LevelCode = row["LevelCode"].ToString(),
                                        HouseCode = row["HouseCode"].ToString(),
                                        HouseName = row["HouseName"].ToString(),
                                        eId = (Security.EncryptForUI(row["ID"].ToString())),
                                    };
                                    ranking.Add(rank);
                                }
                            }
                            reader.Dispose();
                        }
                        return ranking;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> JobAidReadRewardPoint(int userId, int jobAidId)
        {
            JobAid JobAid = await this._db.JobAid.Where(j => j.Id == jobAidId).FirstOrDefaultAsync();
            string FunctionCode = RewardsFunctionCode.JobAidRoleGuide;
            string Category = RewardPointCategory.Normal;
            string Description = "You have read Job Aid " + JobAid.AdditionalDescription;
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }

        public async Task<int> JobResposibilityReadRewardPoint(int userId)
        {
            string FunctionCode = RewardsFunctionCode.RolesResponsibilities;
            string Category = RewardPointCategory.Normal;
            string Description = "You have read Job responsibility ";
            int Point = 1;
            await AddRewardsPoint(userId, Point, FunctionCode, Category, Description);
            return 1;
        }
        public async Task<int> KeyAreaSettingsReadRewardPoint(int userId)
        {
            string FunctionCode = RewardsFunctionCode.KeyResultAreasSetting;
            string Category = RewardPointCategory.Normal;
            string Description = "You have read key result area";
            await AddRewardsPoint(userId, null, FunctionCode, Category, Description);
            return 1;
        }

        public async Task<int> GetSevenDayCount(int userId)
        {
            return await this._db.UserLoginStatiticsForRewardPoints.Where(u => u.UserId == userId).Select(u => u.SevenDayCount).FirstOrDefaultAsync();
        }

        public async Task<int> AddSevenDayPoints(int userId)
        {
            UserLoginStatiticsForRewardPoints UserLoginStatitics = await this._db.UserLoginStatiticsForRewardPoints.Where(l => l.UserId == userId).FirstOrDefaultAsync();
            if (UserLoginStatitics == null)
            {
                return 0;
            }
            if (UserLoginStatitics.SevenDayCount == 7)
            {
                await AddSevenDayRewardPoint(userId);
                UserLoginStatitics.SevenDayCount = 0;
                UserLoginStatitics.LastLoginDateTime = DateTime.UtcNow;
                return await this.UpdateUserLoginStatiticsForRewardPoints(UserLoginStatitics);
            }
            return 0;

        }

        public async Task<APIRewardPointCount> GetHouseRewardPointCount()
        {
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    APIRewardPointCount rewardPointCount = null;
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetHouseRewardPointCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                rewardPointCount = new APIRewardPointCount
                                {
                                    RED = string.IsNullOrEmpty(row["RED"].ToString()) ? 0 : Convert.ToInt32(row["RED"].ToString()),
                                    GREEN = string.IsNullOrEmpty(row["GREEN"].ToString()) ? 0 : Convert.ToInt32(row["GREEN"].ToString()),
                                    BLUE = string.IsNullOrEmpty(row["BLUE"].ToString()) ? 0 : Convert.ToInt32(row["BLUE"].ToString()),
                                    YELLOW = string.IsNullOrEmpty(row["YELLOW"].ToString()) ? 0 : Convert.ToInt32(row["YELLOW"].ToString()),
                                };
                            }
                        }
                        reader.Dispose();
                    }
                    return rewardPointCount;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

    }
}
