using MyCourse.API.APIModel;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MyCourse.API.Helper;
using log4net;
namespace MyCourse.API.Repositories
{
    public class ScormVarRepository : Repository<ScormVars>, IScormVarRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ScormVarRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;

        public ScormVarRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            _customerConnection = customerConnection;
        }

        public async Task<bool> Exists(string name)

        {
            throw new NotImplementedException();
        }
        public async Task<string> GetScorm(string varName, int UserId, int courseId, int moduleId)
        {

            string VarValue = null;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetScormValuebyVarName";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.BigInt) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.BigInt) { Value = moduleId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@VarName", SqlDbType.NVarChar) { Value = varName });
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
                            VarValue = string.IsNullOrEmpty(row["VarValue"].ToString()) ? null : row["VarValue"].ToString();
                        }

                        reader.Dispose();

                    }
                    connection.Close();
                }
            }


            return VarValue;
        }
        public async Task<string> DeleteScorm(int UserId, int courseId, int moduleId)
        {
            try
            {
                string Data = null;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "ScormVarDeleteRecordsByUI";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@courseCode", SqlDbType.BigInt) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleName", SqlDbType.BigInt) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@empCode", SqlDbType.BigInt) { Value = UserId });
                            SqlParameter outputParameter = new SqlParameter("@Result", SqlDbType.NVarChar) { Size = 50, Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outputParameter);
                            int count = await cmd.ExecuteNonQueryAsync();
                            Data = outputParameter.Value.ToString();
                        }
                        connection.Close();
                    }
                }
                return Data;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }
        public async Task<ScormVars> Get(string varName, int UserId, int courseId, int moduleId)
        {
            return await _db.ScormVars.Where(sv => sv.UserId.Equals(UserId) && sv.ModuleId == moduleId && sv.VarName == varName && sv.CourseId == courseId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<int> Count(string varName, int UserId, int courseId, int moduleId)
        {
            return await _db.ScormVars.Where(sv => sv.UserId.Equals(UserId) && sv.ModuleId == moduleId && sv.VarName == varName && sv.CourseId == courseId).CountAsync();
        }

        public async Task<List<ApiScormPost>> GetScormForMobile(int UserId, int courseId, int moduleId)
        {
            List<ApiScormPost> list = new List<ApiScormPost>();
            try
            {

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();

                        parameters.Add("@CourseId", courseId);
                        parameters.Add("@ModuleId", moduleId);
                        parameters.Add("@UserId", UserId);

                        IEnumerable<ApiScormPost> Result = await SqlMapper.QueryAsync<ApiScormPost>((SqlConnection)connection, "dbo.GetScormDataForMobile", parameters, null, null, CommandType.StoredProcedure);
                        list = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return list;
        }


        public async Task<string> ClearBookmarkingData(APIClearScorm obj, int ModifiedBy, DateTime ModifiedDate, int UserID)
        {
            try
            {
                string Result = null;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "Course_ClearBookMarkingData";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = obj.CourseID });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = obj.ModuleID });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = ModifiedBy });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime) { Value = ModifiedDate });
                            SqlParameter outputParameter = new SqlParameter("@Result", SqlDbType.NVarChar) { Size = 50, Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(outputParameter);
                            int count = await cmd.ExecuteNonQueryAsync();
                            Result = outputParameter.Value.ToString();
                        }
                        connection.Close();
                    }
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }
        public async Task<ApiResponse> GetModules(int search)
        {
            ApiResponse obj = new ApiResponse();

            var Query = await (from Module in this._db.Module
                               join CourseModuleAssociation in this._db.CourseModuleAssociation on Module.Id equals CourseModuleAssociation.ModuleId
                               where Module.IsDeleted == Record.NotDeleted && CourseModuleAssociation.CourseId == search
                               orderby Module.Id ascending
                               select new { Module.Id, Module.Name }).Distinct().ToListAsync();

            obj.ResponseObject = Query;
            //if (search>0)
            //{
            //    obj.ResponseObject = Query.Where(a => a.Id == search);
            //}

            return obj;
        }
        public async Task<IEnumerable<APIBookMarkingData>> GetAllBookmarkingClearData()
        {
            List<APIBookMarkingData> apiBookmarkingDataList = new List<APIBookMarkingData>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        APIBookMarkingData apiData = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllClearBookMarkingData";
                            cmd.CommandType = CommandType.StoredProcedure;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    apiData = new APIBookMarkingData
                                    {
                                        CourseCode = row["Code"].ToString(),
                                        CourseName = row["CourseTitle"].ToString(),
                                        ModuleName = row["ModuleName"].ToString(),
                                        UserID = Security.Decrypt(row["UserID"].ToString()),
                                        ModifiedBy = Security.Decrypt(row["ModifiedBy"].ToString()),
                                        ModifiedDate = row["ModifiedDate"].ToString(),
                                    };
                                    apiBookmarkingDataList.Add(apiData);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return apiBookmarkingDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        public async Task<List<APIBookMarkingData>> GetClearBookMarkingViewData(int? page, int? pageSize)
        {
            List<APIBookMarkingData> apiBookmarkingDataList = new List<APIBookMarkingData>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        APIBookMarkingData apiData = null;
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllClearBookMarkingData";
                            cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@pagesize", SqlDbType.Int) { Value = pageSize });
                            cmd.CommandType = CommandType.StoredProcedure;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    apiData = new APIBookMarkingData
                                    {
                                        CourseCode = row["Code"].ToString(),
                                        CourseName = row["CourseTitle"].ToString(),
                                        ModuleName = row["ModuleName"].ToString(),
                                        UserID = Security.Decrypt(row["UserID"].ToString()),
                                        ModifiedBy = Security.Decrypt(row["ModifiedBy"].ToString()),
                                        ModifiedDate = row["ModifiedDate"].ToString(),
                                    };
                                    apiBookmarkingDataList.Add(apiData);
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return apiBookmarkingDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }
        public async Task<int> GetClearBookMarkingViewDataCount()
        {
            int count = 0;
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
                            cmd.CommandText = "GetAllClearBookMarkingDataCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    count = (int)row["Count"];
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                        return count;
                    }
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

