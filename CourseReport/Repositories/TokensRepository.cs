﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CourseReport.API.Data;
using CourseReport.API.Model;
using CourseReport.API.Repositories.Interface;
using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using log4net;
using CourseReport.API.Helper;

namespace CourseReport.API.Repositories
{
    public class TokensRepository : Repository<Tokens>, ITokensRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TokensRepository));


        private ReportDbContext _db;
        private IHttpContextAccessor _httpContext;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public TokensRepository(ReportDbContext context, ICustomerConnectionStringRepository customerConnectionString, IHttpContextAccessor httpContext) : base(context)
        {
            this._db = context;
            this._httpContext = httpContext;
            this._customerConnectionString = customerConnectionString;
        }

        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<Tokens>();
        }

        public async Task<int> CheckUserToken(int userId, string token)
        {
            try
            {
                using (ReportDbContext dbContext = _customerConnectionString.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "CheckUserToken";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@token", SqlDbType.VarChar) { Value = token });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
            return 1;
        }

        public async Task<bool> UserTokenExists(string token)
        {
            bool value = false;
            try
            {
                using (ReportDbContext dbContext = _customerConnectionString.GetDbContext())
                {
                    DbConnection connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UserTokenExists";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@token", SqlDbType.VarChar) { Value = token.Substring(token.Length - 100) });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = string.IsNullOrEmpty(dt.Rows[0]["Value"].ToString()) ? false : Convert.ToBoolean(dt.Rows[0]["Value"].ToString());
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
            return value;
        }

    }
}
