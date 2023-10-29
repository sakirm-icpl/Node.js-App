using Competency.API.APIModel;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Competency.API.Common;
using log4net;


namespace Competency.API.Repositories.Interfaces.Competency
{
    public class JdUploadRepository : Repository<CompetencyJdUpload>, IJdUploadRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JdUploadRepository));
        CourseContext db;

        private readonly IConfiguration _configuration;
        ICustomerConnectionStringRepository _customerConnection;
        private ICustomerConnectionStringRepository _customerConnectionString;


        IIdentityService _identitySv;

        public JdUploadRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionString, INotification notification, IIdentityService identitySv, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            this._customerConnection = customerConnection;
            this._identitySv = identitySv;
            this._customerConnectionString = customerConnectionString;
        }

        public async Task<CompetencyJdUpload> GetCompetencyJdUpload(int JobRoleId)
        {
            try
            {

                var result = (from competencyJdUpload in db.competencyJdUpload
                              orderby competencyJdUpload.FileVersion descending
                              where competencyJdUpload.CompetencyJobRoleId == JobRoleId

                              select new CompetencyJdUpload
                              {
                                  CompetencyJobRoleId = competencyJdUpload.CompetencyJobRoleId,
                                  CreatedDate = competencyJdUpload.CreatedDate,
                                  FileType = competencyJdUpload.FileType,
                                  FilePath = competencyJdUpload.FilePath,
                                  FileVersion = competencyJdUpload.FileVersion
                              });

                CompetencyJdUpload item = result.FirstOrDefault();
                return item;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<APICompetencyJdUpload>> GetAllJdUpload(int page, int pageSize, string search = null, string columnName = null)
        {
            List<APICompetencyJdUpload> records = new List<APICompetencyJdUpload>();
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "CompetencyJdUpload";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var record = new APICompetencyJdUpload
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    Name = row["Name"].ToString(),
                                    Description = row["Description"].ToString(),
                                    FilePath = row["FilePath"].ToString(),
                                    CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
                                    UserName = row["UserName"].ToString(),

                                };
                                records.Add(record);
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                    return records;
                }
            }

            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        public async Task<int> GetAllJdCount()
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
                        cmd.CommandText = "CompetencyJdUploadCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = string.IsNullOrEmpty(row["TotalRecordCount"].ToString()) ? 0 : Convert.ToInt32(row["TotalRecordCount"].ToString());
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
    }
}


