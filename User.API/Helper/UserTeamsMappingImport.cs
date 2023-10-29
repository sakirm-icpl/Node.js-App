using log4net;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Repositories.Interfaces;
using OfficeOpenXml.FormulaParsing;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;


using Microsoft.Data.SqlClient;
using User.API.Common;
using User.API.Data;

namespace User.API.Helper
{
    public static class ExtensionMethod
    {
        public static SqlParameter WithValue(this SqlParameter parameter, object value)
        {
            parameter.Value = value;
            return parameter;
        }
    }
    public class UserTeamsMappingImport
    {

        public class ProcessFile
        {
            private UserDbContext _db;
            public ProcessFile(UserDbContext db)
            {
                this._db = db;
            }
            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));

            private DataTable ExecuteStoredProcedureWithData(ICustomerConnectionStringRepository _customerConnectionRepository, SqlParameter[] parameters, string spName, int timeOut = 0)
            {
                DataTable dt = new DataTable();

                using (SqlConnection connection = new SqlConnection(_customerConnectionRepository.GetDbContext().Database.GetDbConnection().ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandTimeout = timeOut;
                        cmd.CommandText = spName;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        DbDataReader reader = cmd.ExecuteReader();
                        dt.Load(reader);
                        reader.Dispose();
                    }
                }
                return dt;

            }

            public ApiResponse ProcessRecords(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
            {
                string fileName = "UserTeamsImport.csv";

                DataTable courseApplicabilityImportData = new ExcelUtilities(_configuration).ReadFromExcel(file.FullName, true);

                if (!ValidColumnsInExcel(courseApplicabilityImportData))
                    return new ApiResponse() { StatusCode = 400, Description = "Excel file is not in required format. Please check sample excel file format and try uploading again." };

                if (courseApplicabilityImportData != null && courseApplicabilityImportData.Rows.Count == 0)
                {
                    return new ApiResponse()
                    {
                        StatusCode = 204,
                        ResponseObject = new { TotalRecords = 0, Successful = 0, Failed = 0, Description = "Imported file does not contain data." },
                    };
                }

                #region "Verify and Insert records into Accessibility rule table"
                foreach (DataRow row in courseApplicabilityImportData.Rows)
                {
                    row["UserId*"] = Security.Encrypt(Convert.ToString(row["UserId*"]).ToLower());
                    _logger.Debug(row["UserId*"].ToString());
                }

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                        new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                        new SqlParameter("@UserTeamsApplicabilityBulkImport_TVP",courseApplicabilityImportData )
                };
                try
                {
                    courseApplicabilityImportData = ExecuteStoredProcedureWithData(_customerConnectionRepository, sqlParameters, "UserTeamsApplicabilityBulkImport");
                }
                catch (Exception ex)
                {
                    _logger.Debug("Exception in ExecuteStoredProcedureWithData " + ex.ToString());
                    _logger.Error(ex.ToString());
                }
                #endregion

                foreach (DataRow row in courseApplicabilityImportData.Rows)
                {
                    row["UserId"] = Security.Decrypt(Convert.ToString(row["UserId"]));

                    if((Convert.ToString(row["ImportStatus"]).ToLower() == "success"))
                    {
                        UserTeams userTeams = this._db.UserTeams.Where(a => a.Id == Convert.ToInt32(row["TeamId"])).FirstOrDefault();
                        userTeams.NumberofMembers++;
                        userTeams.NumberOfRules++;

                        this._db.UserTeams.Update(userTeams);
                        this._db.SaveChanges();

                        if (userTeams != null)
                        {
                            _db.Entry(userTeams).State = EntityState.Detached;
                        }
                    }
                }
                
                courseApplicabilityImportData.Columns.Remove("TeamId");
                courseApplicabilityImportData.Columns.Remove("UserName");

                int successCount = courseApplicabilityImportData.Select("ImportStatus =" + "'success'").Length;
                int failureCount = courseApplicabilityImportData.Select("ImportStatus =" + "'Failed'").Length;

                DataRow[] dr = courseApplicabilityImportData.Select("ImportStatus = 'failed'");
                string excelFileName = string.Empty;
                if (dr.Length > 0)
                {
                    DataTable failedApplicabilityReport = dr.CopyToDataTable();
                    FileInfo fileInfo = new ExcelUtilities(_configuration).WriteDataTableToExcel(failedApplicabilityReport, orgcode, fileName);
                    excelFileName = fileInfo.Name;
                }
                return new ApiResponse()
                {
                    StatusCode = 200,
                    ResponseObject = new { Filename = excelFileName, TotalRecords = successCount + failureCount, Successful = successCount, Failed = failureCount },
                };
            }

            private bool ValidColumnsInExcel(DataTable dataTable)
            {
                if (dataTable.Columns.Count == 2)
                {
                    DataColumnCollection columns = dataTable.Columns;
                    if (columns.Contains("TeamCode*") && columns.Contains("userid*"))
                        return true;
                }
                return false;
            }
        }
    }
}

