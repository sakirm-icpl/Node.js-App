using Survey.API.APIModel;
using Survey.API.Helper;
using Survey.API.Metadata;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Survey.API.Common.Constants;
using static Survey.API.Helper.ApiHelper;

namespace Survey.API.Common
{
    public static class ExtensionMethod
    {
        public static SqlParameter WithValue(this SqlParameter parameter, object value)
        {
            parameter.Value = value;
            return parameter;
        }
    }
    public class ProcessFile
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));
            static StringBuilder sb = new StringBuilder();
            static string[] header = { };

            static List<string> accessibilityRuleRecord = new List<string>();          
            static StringBuilder sbError = new StringBuilder();
            

            static int totalRecordInsert = 0;
            static int totalRecordRejected = 0;
            public static void Reset()
            {
                sb.Clear();
                header = new string[0];
                accessibilityRuleRecord.Clear();
                sbError.Clear();
                totalRecordInsert = 0;
                totalRecordRejected = 0;
            }

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

            private void ExecuteStoredProcedure(ICustomerConnectionStringRepository _customerConnectionRepository, SqlParameter[] parameters, string spName)
            {
                using (SqlConnection connection = new SqlConnection(_customerConnectionRepository.GetDbContext().Database.GetDbConnection().ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = spName;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);
                        DbDataReader reader = cmd.ExecuteReader();
                        reader.Dispose();
                    }
                }
            }
            public async Task<APIResponse> ProcessRecordsAsync(string filePath, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
            {
                string rowguid = Guid.NewGuid().ToString();
              

                string sWebRootFolder = _configuration[AppSetting.ApiGatewayWwwRootFolderName];
                string filefinal = sWebRootFolder + filePath;
                FileInfo file = new FileInfo(Path.Combine(filefinal));

                DataTable surveyApplicabilityImportData = new ExcelUtilities(_configuration).ReadFromExcel(file.FullName, true);

                if (!ValidColumnsInExcel(surveyApplicabilityImportData))
                    return new APIResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(FileMessages.FileFormatError) };

                if (surveyApplicabilityImportData != null && surveyApplicabilityImportData.Rows.Count == 0)
                {
                    return new APIResponse()
                    {
                        StatusCode = 204,
                        ResponseObject = new { TotalRecords = 0, Successful = 0, Failed = 0, Description = EnumHelper.GetEnumDescription(FileMessages.FileErrorInImport) },
                    };
                }

                #region "Verify and Insert records into Accessibility rule table"
                foreach (DataRow row in surveyApplicabilityImportData.Rows)
                {
                    row["UserId"] = Security.Encrypt(Convert.ToString(row["UserId"]).ToLower());
                }

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                        new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                        new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                        new SqlParameter("@SurveyApplicabilityImportType_TVP",surveyApplicabilityImportData )
                };
                surveyApplicabilityImportData = ExecuteStoredProcedureWithData(_customerConnectionRepository, sqlParameters, "SurveyApplicabilityBulkImport");

                #endregion

                #region "If Valid records are found then send notifications

                if (surveyApplicabilityImportData.Rows.Count > 0 && surveyApplicabilityImportData.Select("importstatus =" + "'success'").Length > 0)
                {
                   
                    sqlParameters = new SqlParameter[]
                    {
                              new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                              new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid)
                    };
                    ExecuteStoredProcedure(_customerConnectionRepository, sqlParameters, "SurveyApplicabilityBulkNotifications");

                }
            #endregion
                List<int> SurveyIds = new List<int>();
                foreach (DataRow row in surveyApplicabilityImportData.Rows)
                {
                    row["UserId"] = Security.Decrypt(Convert.ToString(row["UserId"]));
                    if(!(row["SurveyId"] is DBNull))
                    {
                        if(!SurveyIds.Contains(Convert.ToInt32(row["SurveyId"])))
                            SurveyIds.Add(Convert.ToInt32(row["SurveyId"]));
                    }
                }

                surveyApplicabilityImportData.Columns.Remove("SurveyId");
                surveyApplicabilityImportData.Columns.Remove("UserName");

                int successCount = surveyApplicabilityImportData.Select("ImportStatus =" + "'success'").Length;
                int failureCount = surveyApplicabilityImportData.Select("ImportStatus =" + "'Failed'").Length;

                DataRow[] dr = surveyApplicabilityImportData.Select("ImportStatus = 'failed'");
                string excelFileName = string.Empty;
                if (dr.Length > 0)
                {
                    DataTable failedApplicabilityReport = dr.CopyToDataTable();
                    FileInfo fileInfo = new ExcelUtilities(_configuration).WriteDataTableToExcel(failedApplicabilityReport, orgcode, ImportBackupFile.SurveyApplicabilityImportFile, ImportBackupFolder.SurveyApplicabilityImportFolder);
                    excelFileName = fileInfo.Name;
                }

                if (successCount>0 && SurveyIds.Count>0)
                {
                    var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY", _customerConnectionRepository);
                    if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                    {
                        if (string.Equals(orgcode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (int SurveyId in SurveyIds)
                            {
                                string urlSMS = _configuration[Configuration.NotificationApi];
                                urlSMS += "/SurveyApplicabilitySMS";
                                JObject jObject = new JObject();
                                jObject.Add("SurveyManagementId", SurveyId);
                                jObject.Add("OrganizationCode", orgcode);
                                HttpResponseMessage responsesSMS = CallAPI(urlSMS, jObject).Result;
                            }
                        }
                    }
                    foreach (int SurveyId in SurveyIds)
                    {
                        try
                        {
                            string url = _configuration[Configuration.NotificationApi];
                            url += "/SurveyApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("SurveyManagementId", SurveyId);
                            Pushnotification.Add("organizationCode", orgcode);
                            HttpResponseMessage responses1 = CallAPI(url, Pushnotification).Result;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }

                return new APIResponse()
                {
                    StatusCode = 200,
                    ResponseObject = new { Filename = excelFileName, TotalRecords = successCount + failureCount, Successful = successCount, Failed = failureCount },
                };
            }

        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode, ICustomerConnectionStringRepository customerConnectionString)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = customerConnectionString.GetDbContext())
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

        public  FileContentResult GenerateSurveyApplicabilityAsync(ControllerBase controller, int userid, IConfiguration _configuration, string orgcode)
            {

                string sWebRootFolder = _configuration[AppSetting.ApiGatewayWwwRootFolderName];
                sWebRootFolder = Path.Combine(sWebRootFolder, orgcode);
                string DomainName =_configuration[AppSetting.ApiGatewayUrlValue];
                string sFileName = ImportFile.SurveyApplicabilityDownloadFile;
                string URL = string.Format("{0}{1}/{2}", DomainName, orgcode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SurveyApplicabilityImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "SurveyName";
                    worksheet.Cells[1, 2].Value = "UserId";
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save();
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
              
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }                
                return controller.File(fileData, FileContentType.Excel);
            }
            private bool ValidColumnsInExcel(DataTable dataTable)
            {
                if (dataTable.Columns.Count == 2)
                {
                    DataColumnCollection columns = dataTable.Columns;
                    if (columns.Contains("surveyname") && columns.Contains("userid"))
                        return true;
                }
                return false;
            }
        }
    
}