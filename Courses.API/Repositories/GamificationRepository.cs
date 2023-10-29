using Courses.API.APIModel;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Courses.API.Helper;
using log4net;
namespace Courses.API.Repositories
{
    public class GamificationRepository : IGamificationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GamificationRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        public GamificationRepository(CourseContext db,
            ICustomerConnectionStringRepository customerConnection)
        {
            this._db = db;
            this._customerConnection = customerConnection;
        }
        public async Task<ApiResponse> GetMissionCounts(int userId)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        ApiMissionCounts MissionCounts = new ApiMissionCounts();
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllMissionCount";
                            cmd.CommandType = CommandType.StoredProcedure;
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
                                MissionCounts.CompletedBossMission = string.IsNullOrEmpty(row["CompletedBossMission"].ToString()) ? 0 : int.Parse(row["CompletedBossMission"].ToString());
                                MissionCounts.CompletedMiniMission = string.IsNullOrEmpty(row["CompletedMiniMission"].ToString()) ? 0 : int.Parse(row["CompletedMiniMission"].ToString());
                                MissionCounts.CompletedNormalMission = string.IsNullOrEmpty(row["CompletedNormalMission"].ToString()) ? 0 : int.Parse(row["CompletedNormalMission"].ToString());
                                MissionCounts.TotalBossMission = string.IsNullOrEmpty(row["TotalBossMission"].ToString()) ? 0 : int.Parse(row["TotalBossMission"].ToString());
                                MissionCounts.TotalMiniMission = string.IsNullOrEmpty(row["TotalMiniMission"].ToString()) ? 0 : int.Parse(row["TotalMiniMission"].ToString());
                                MissionCounts.TotalNormalMission = string.IsNullOrEmpty(row["TotalNormalMission"].ToString()) ? 0 : int.Parse(row["TotalNormalMission"].ToString());

                            }
                            Response.ResponseObject = MissionCounts;
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
            return Response;
        }
        public async Task<ApiResponse> GetDaywiseMissionCounts(int userId, int noOfDays, string missionType = null)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                DateTime FromDate = DateTime.UtcNow.AddDays(-noOfDays);
                ApiMissionCounts MissionCounts = new ApiMissionCounts();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "DayWiseMissionCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date) { Value = FromDate });
                            cmd.Parameters.Add(new SqlParameter("@MissionType", SqlDbType.VarChar) { Value = missionType });
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
                                MissionCounts.CompletedBossMission = string.IsNullOrEmpty(row["CompletedBossMission"].ToString()) ? 0 : int.Parse(row["CompletedBossMission"].ToString());
                                MissionCounts.CompletedMiniMission = string.IsNullOrEmpty(row["CompletedMiniMission"].ToString()) ? 0 : int.Parse(row["CompletedMiniMission"].ToString());
                                MissionCounts.CompletedNormalMission = string.IsNullOrEmpty(row["CompletedNormalMission"].ToString()) ? 0 : int.Parse(row["CompletedNormalMission"].ToString());
                                MissionCounts.TotalBossMission = string.IsNullOrEmpty(row["TotalBossMission"].ToString()) ? 0 : int.Parse(row["TotalBossMission"].ToString());
                                MissionCounts.TotalMiniMission = string.IsNullOrEmpty(row["TotalMiniMission"].ToString()) ? 0 : int.Parse(row["TotalMiniMission"].ToString());
                                MissionCounts.TotalNormalMission = string.IsNullOrEmpty(row["TotalNormalMission"].ToString()) ? 0 : int.Parse(row["TotalNormalMission"].ToString());

                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                Response.ResponseObject = MissionCounts;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Response;
        }
    }
}
