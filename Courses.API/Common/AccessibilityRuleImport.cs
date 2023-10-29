using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using log4net;

namespace Courses.API.Common
{
    public static class ExtensionMethod
    {
        public static SqlParameter WithValue(this SqlParameter parameter, object value)
        {
            parameter.Value = value;
            return parameter;
        }
    }

    public class AccessibilityRuleImport
    {
        public class ProcessFile
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));
            static StringBuilder sb = new StringBuilder();
            static string[] header = { };

            static List<string> accessibilityRuleRecord = new List<string>();

            static Courses.API.Model.Course courseInfo = new Courses.API.Model.Course();
            static AccessibilityRule accessibilityRule = new AccessibilityRule();
            static AccessibilityRuleRejected accessibilityRuleRejected = new AccessibilityRuleRejected();
            static APIAccessibilityRuleImport accessibilityRuleImportRejected = new APIAccessibilityRuleImport();
            static APIAccessibilityRuleFilePath accessibilityRuleFilePath = new APIAccessibilityRuleFilePath();
            static StringBuilder sbError = new StringBuilder();
            static APIAccessibilityRuleImport accessibilityRuleImport = new APIAccessibilityRuleImport();

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

            private DataTable ExecuteStoredProcedureWithData(ICustomerConnectionStringRepository _customerConnectionRepository, SqlParameter[] parameters, string spName,int timeOut = 0)
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

            public async Task<bool> InitilizeAsync1(FileInfo file)
            {
                bool result = false;
                try
                {
                    using (ExcelPackage package = new ExcelPackage(file))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                        int rowCount = worksheet.Dimension.Rows;
                        int ColCount = worksheet.Dimension.Columns;
                        for (int row = 1; row <= rowCount; row++)
                        {
                            for (int col = 1; col <= ColCount; col++)
                            {
                                string append = "";
                                if (worksheet.Cells[row, col].Value == null)
                                {

                                }
                                else
                                {
                                    append = Convert.ToString(worksheet.Cells[row, col].Value.ToString());
                                }
                                string finalAppend = append + "\t";
                                sb.Append(finalAppend);

                            }
                            sb.Append(Environment.NewLine);
                        }

                        string fileInfo = sb.ToString();
                        accessibilityRuleRecord = new List<string>(fileInfo.Split('\n'));
                        foreach (string record in accessibilityRuleRecord)
                        {
                            string[] mainsp = record.Split('\r');
                            string[] mainsp2 = mainsp[0].Split('\"');
                            header = mainsp2[0].Split('\t');

                            break;
                        }
                        accessibilityRuleRecord.RemoveAt(0);

                    }
                    /////Remove Star from Header
                    for (int i = 0; i < header.Count(); i++)
                    {
                        header[i] = header[i].Replace("*", "");

                    }
                    // invalid file
                    int count = 0;
                    for (int i = 0; i < header.Count() - 1; i++)
                    {
                        string headerColumn = header[i].ToString().Trim();
                        if (!string.IsNullOrEmpty(headerColumn))
                        {
                            count++;
                        }

                    }
                    if (count == 6)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    return result;
                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return result;
            }

            public async Task<ApiResponse> ProcessRecordsAsync(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
            {
                string rowguid = Guid.NewGuid().ToString();
                string fileName = "CourseApplicabilityImport.csv";

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
                    row["UserId"] = Security.Encrypt(Convert.ToString(row["UserId"]).ToLower());
                    _logger.Debug(row["UserId"].ToString());
                }

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                        new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                        new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                        new SqlParameter("@CourseApplicabilityImportType_TVP",courseApplicabilityImportData )
                };
                try
                {
                    courseApplicabilityImportData = ExecuteStoredProcedureWithData(_customerConnectionRepository, sqlParameters, "CourseApplicabilityBulkImport");
                }
                catch(Exception ex)
                {
                    _logger.Debug("Exception in ExecuteStoredProcedureWithData " + ex.ToString());
                    _logger.Error(ex.ToString());
                }
                #endregion

                #region "If Valid records are found then send notifications

                if (courseApplicabilityImportData.Rows.Count > 0 && courseApplicabilityImportData.Select("importstatus =" + "'success'").Length > 0)
                {
                    sqlParameters = new SqlParameter[]
                    {
                              new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                              new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                              new SqlParameter("@CourseTemplate", Convert.ToString( _configuration[Configuration.CourseNotification]))
                    };
                    try
                    {
                        ExecuteStoredProcedure(_customerConnectionRepository, sqlParameters, "CourseApplicabilityBulkNotifications");
                    }
                    catch(Exception ex)
                    {
                        _logger.Debug("Exception in ExecuteStoredProcedure " + ex.ToString());
                        _logger.Error("Exception in ExecuteStoredProcedure " + ex.ToString());
                    }
                    List<string> distinctCourseCodes = (courseApplicabilityImportData.AsEnumerable().Select(x => x["CourseId"].ToString())).Distinct().ToList();
                    distinctCourseCodes = distinctCourseCodes.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    _logger.Debug(distinctCourseCodes.Count.ToString());
                    try
                    {
                        if (distinctCourseCodes.Count > 0)
                            await CallNotificationServices(_configuration, orgcode, rowguid, distinctCourseCodes, _customerConnectionRepository);
                    }
                    catch(Exception ex)
                    {
                        _logger.Debug("Exception in CallNotificationServices" + ex.ToString());
                        _logger.Error("Exception in CallNotificationServices" + ex.ToString());
                    }
                }
                #endregion

                foreach (DataRow row in courseApplicabilityImportData.Rows)
                {
                    row["UserId"] = Security.Decrypt(Convert.ToString(row["UserId"]));
                }

                courseApplicabilityImportData.Columns.Remove("CourseId");
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

            public async Task<ApiResponse> ProcessGroupRecordsAsync(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, string GroupName, IConfiguration _configuration, string orgcode)
            {
                string rowguid = Guid.NewGuid().ToString();
                string fileName = "UserGroupImport.csv";

                DataTable courseApplicabilityImportData = new ExcelUtilities(_configuration).ReadFromExcel(file.FullName, true);

                if (!ValidColumnsInExcelUserGroup(courseApplicabilityImportData))
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
                    row["UserId"] = Security.Encrypt(Convert.ToString(row["UserId"]).ToLower());
                    _logger.Debug(row["UserId"].ToString());
                }

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                        new SqlParameter("@RowGUID", SqlDbType.VarChar).WithValue(rowguid),
                        new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                        new SqlParameter("@GroupName", SqlDbType.NVarChar).WithValue(GroupName),
                        new SqlParameter("@UserIDs_TVP", SqlDbType.Structured).WithValue(courseApplicabilityImportData)
                };
                try
                {
                    courseApplicabilityImportData = ExecuteStoredProcedureWithData(_customerConnectionRepository, sqlParameters, "UserGroupBulkImport");
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
                }

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

            public async Task<ApiResponse> ProcessRecordsDeleteApplicabilityAsync(string filePath, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
            {

                string sWebRootFolder = _configuration[AppSetting.ApiGatewayWwwRootFolderName];
                string filefinal = sWebRootFolder + filePath;
                FileInfo file = new FileInfo(Path.Combine(filefinal));

                string rowguid = Guid.NewGuid().ToString();
                string fileName = "CourseUnAssignImport.csv";

                DataTable courseUnassignyImportData = new ExcelUtilities(_configuration).ReadFromExcel(file.FullName, true);

                if (!ValidColumnsInExcel(courseUnassignyImportData))
                    return new ApiResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(FileMessages.FileFormatError) };

                if (courseUnassignyImportData != null && courseUnassignyImportData.Rows.Count == 0)
                {
                    return new ApiResponse()
                    {
                        StatusCode = 204,
                        ResponseObject = new { TotalRecords = 0, Successful = 0, Failed = 0, Description = EnumHelper.GetEnumDescription(FileMessages.FileEmpty) },
                    };
                }

                #region "Verify and delete Accessibility rule table"
                foreach (DataRow row in courseUnassignyImportData.Rows)
                {
                    row["UserId"] = Security.Encrypt(Convert.ToString(row["UserId"]).ToLower());
                }

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                        new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                        new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid),
                        new SqlParameter("@CourseApplicabilityImportType_TVP",courseUnassignyImportData )
                };
                courseUnassignyImportData = ExecuteStoredProcedureWithData(_customerConnectionRepository, sqlParameters, "CourseUnassignBulkImport");

                #endregion

                #region "If Valid records are found then update from  notifications

                if (courseUnassignyImportData.Rows.Count > 0 && courseUnassignyImportData.Select("importstatus =" + "'success'").Length > 0)
                {
                    sqlParameters = new SqlParameter[]
                    {
                              new SqlParameter("@RowGuid", SqlDbType.VarChar).WithValue(rowguid),
                              new SqlParameter("@CreatedBy", SqlDbType.Int).WithValue(userid)                             
                    };
                    ExecuteStoredProcedure(_customerConnectionRepository, sqlParameters, "CourseUnAssignBulkNotifications");

                    List<string> distinctCourseCodes = (courseUnassignyImportData.AsEnumerable().Select(x => x["CourseId"].ToString())).Distinct().ToList();
                    distinctCourseCodes = distinctCourseCodes.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                }
                #endregion

                foreach (DataRow row in courseUnassignyImportData.Rows)
                {
                    row["UserId"] = Security.Decrypt(Convert.ToString(row["UserId"]));
                }

                courseUnassignyImportData.Columns.Remove("CourseId");
                courseUnassignyImportData.Columns.Remove("UserName");

                int successCount = courseUnassignyImportData.Select("ImportStatus =" + "'success'").Length;
                int failureCount = courseUnassignyImportData.Select("ImportStatus =" + "'Failed'").Length;

                DataRow[] dr = courseUnassignyImportData.Select("ImportStatus = 'failed'");
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
                    if (columns.Contains("coursecode") && columns.Contains("userid"))
                        return true;
                }
                return false;
            }

            private bool ValidColumnsInExcelUserGroup(DataTable dataTable)
            {
                if (dataTable.Columns.Count == 1)
                {
                    DataColumnCollection columns = dataTable.Columns;
                    if (columns.Contains("userid"))
                        return true;
                }
                return false;
            }

            private async Task<bool> GetConfigurableValue(string configName, ICustomerConnectionStringRepository _customerConnectionRepository)
            {
                try
                {
                    using (var dbContext = _customerConnectionRepository.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetConfigurableParameterValue";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = configName });
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
                                    return string.IsNullOrEmpty(Convert.ToString(row["Value"])) ? false :
                                         Convert.ToString(row["Value"]).ToUpper() == "YES" ? true : false;
                                }
                                reader.Dispose();
                            }
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return false;
                }
                return false;
            }

            public async Task CallNotificationServices(IConfiguration _configuration, string orgcode, string RowGUID, List<string> CoursesList, ICustomerConnectionStringRepository _customerConnectionRepository)
            {
                try
                {
                    string pushNotificationUrl = _configuration[Configuration.NotificationApi] + "/CourseApplicabilityImportPushNotificationWithGUID";
                    string sendEmailNotificationUrl = _configuration[Configuration.NotificationApi] + "/CourseApplicabilityImportWithGUID";
                    string sendSMSNotificationUrl = _configuration[Configuration.NotificationApi] + "/CourseApplicabilityImportSMS";
                    bool sendPushNotification = await GetConfigurableValue("PUSH_NOTIFICATION", _customerConnectionRepository);
                    bool sendEmail = await GetConfigurableValue("EMAIL_NOTIFICATION", _customerConnectionRepository);
                    bool sendSMS = await GetConfigurableValue("SMS_NOTIFICATION", _customerConnectionRepository);

                    foreach (string courseID in CoursesList)
                    {
                        if (sendPushNotification)
                        {
                            JObject oPushJsonObject = new JObject();
                            oPushJsonObject.Add("CourseID", Convert.ToInt32(courseID));
                            oPushJsonObject.Add("OrganizationCode", orgcode);
                            oPushJsonObject.Add("RowGUID", RowGUID);
                            HttpResponseMessage Pushresponses = CallAPI(pushNotificationUrl, oPushJsonObject).Result;
                        }

                        if (sendEmail)
                        {
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseID", Convert.ToInt32(courseID));
                            oJsonObject.Add("organizationCode", orgcode);
                            oJsonObject.Add("RowGUID", RowGUID);
                            HttpResponseMessage responses = CallAPI(sendEmailNotificationUrl, oJsonObject).Result;
                        }

                        if (sendSMS)
                        {
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseID", Convert.ToInt32(courseID));
                            oJsonObject.Add("organizationCode", orgcode);
                            oJsonObject.Add("RowGUID", RowGUID);
                            HttpResponseMessage responses = CallAPI(sendSMSNotificationUrl, oJsonObject).Result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
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

            public async Task<ApiResponse> ProcessRecordCategoryAsync(IAccessibilityRule _accessibilityRule, ICourseRepository _courseRepository, IAccessibilityRuleRejectedRepository _accessibilityRuleRejectedRepository, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode)
            {
                ApiResponse response = new ApiResponse();
                APIConfiguration configurations = new APIConfiguration();
                List<APIAccessibilityRuleFilePath> aPIAccessibilityRuleFilePathsRejected = new List<APIAccessibilityRuleFilePath>();
                List<Model.Course> CoursesList = new List<Model.Course>();
                APIAccessibilityRuleImport rulerejected = new APIAccessibilityRuleImport();
                List<APIAccessibilityRuleImport> rulerejected1 = new List<APIAccessibilityRuleImport>();
                List<APIAccessibilityRuleImport> accessibilityRuleImports = new List<APIAccessibilityRuleImport>();
                AccessibilityRule accrule = new AccessibilityRule();

                Model.Course courseInfo = new Model.Course();
                bool validvalue = false;
                if (accessibilityRuleRecord != null && accessibilityRuleRecord.Count > 0)
                {
                    List<APIAccessibilityRuleFilePath> aPIAccessibilityRuleFilePaths = new List<APIAccessibilityRuleFilePath>();

                    foreach (string record in accessibilityRuleRecord)
                    {

                        int countLenght = record.Length;
                        if (record != null && countLenght > 1)
                        {
                            string[] textpart = record.Split('\t');
                            string[][] mainRecord = { header, textpart };
                            string txtUniversity = string.Empty;
                            string txtSem = string.Empty;
                            string txtCourse = string.Empty;
                            string txtUserId = string.Empty;
                            string txtCategory = string.Empty;
                            string txtSubCategory = string.Empty;
                            string displayUserID = string.Empty;
                            string headerText = "";
                            string txtUserName = string.Empty;
                            string txtLanguages = string.Empty;

                            int arrasize = header.Count();

                            for (int j = 0; j < arrasize - 1; j++)
                            {
                                headerText = header[j];
                                string[] mainspilt = headerText.Split('\t');

                                headerText = mainspilt[0];


                                if (headerText == "Course")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtCourse = textuserMobilesplit[0];
                                        txtCourse = txtCourse.Trim();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Semester")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtSem = textuserMobilesplit[0];
                                        txtSem = txtSem.Trim();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Subject")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtCategory = textuserMobilesplit[0];
                                        txtCategory = txtCategory.Trim();
                                        bool valid = ValidateCategory(headerText, txtCategory);

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Unit")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtSubCategory = textuserMobilesplit[0];
                                        txtSubCategory = txtSubCategory.Trim();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "UserId")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtUserName = textuserMobilesplit[0];
                                        txtUserName = txtUserName.ToLower();
                                        txtUserId = txtUserName.Trim().Encrypt();
                                        displayUserID = txtUserName.Trim();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Languages")
                                {
                                    try
                                    {
                                        string mobile = mainRecord[1][j];
                                        string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                        txtLanguages = textuserMobilesplit[0];
                                        txtLanguages = txtLanguages.Trim();

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }

                            }

                            if (validvalue == false)
                            {
                                try
                                {

                                    using (var dbContext = _customerConnectionRepository.GetDbContext())
                                    {
                                        using (var connection = dbContext.Database.GetDbConnection())
                                        {
                                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                                connection.Open();
                                            using (var cmd = connection.CreateCommand())
                                            {
                                                cmd.CommandText = "GetConfigurations";
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.Parameters.Add(new SqlParameter("@Course", SqlDbType.NVarChar) { Value = txtCourse });
                                                cmd.Parameters.Add(new SqlParameter("@Sem", SqlDbType.NVarChar) { Value = txtSem });
                                                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = txtUserId });
                                                cmd.Parameters.Add(new SqlParameter("@Language", SqlDbType.NVarChar) { Value = txtLanguages });
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
                                                    if (txtCourse != "")
                                                    {
                                                        configurations.Course = string.IsNullOrEmpty(row["Course"].ToString()) ? null : (row["Course"].ToString());
                                                    }
                                                    else
                                                    {
                                                        configurations.Course = string.IsNullOrEmpty(row["Course"].ToString()) ? "null" : (row["Course"].ToString());
                                                    }
                                                    if (txtSem != "")
                                                    {
                                                        configurations.Semester = string.IsNullOrEmpty(row["Sem"].ToString()) ? null : (row["Sem"].ToString());
                                                    }
                                                    else
                                                    {
                                                        configurations.Semester = string.IsNullOrEmpty(row["Sem"].ToString()) ? "null" : (row["Sem"].ToString());
                                                    }
                                                    if (txtUserId != "")
                                                    {
                                                        configurations.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : (row["UserId"].ToString());
                                                    }
                                                    else
                                                    {
                                                        configurations.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? "null" : (row["UserId"].ToString());
                                                    }

                                                    if (txtLanguages != "")
                                                    {
                                                        configurations.Languages = string.IsNullOrEmpty(row["Languages"].ToString()) ? null : (row["Languages"].ToString());
                                                    }
                                                    else
                                                    {
                                                        configurations.Languages = string.IsNullOrEmpty(row["Languages"].ToString()) ? "null" : (row["Languages"].ToString());
                                                    }
                                                }
                                                reader.Dispose();
                                            }
                                            connection.Close();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }

                                string result = null;

                                try
                                {
                                    int? SubCategoryId = 0;
                                    int? LocationId = 0;
                                    int? Config1 = 0;
                                    int? LanguageId = 0;
                                    int? UserID = 0;
                                    if (configurations.Course == "null")
                                    {
                                        Config1 = null;
                                    }
                                    else
                                    {
                                        Config1 = Convert.ToInt32(configurations.Course);
                                    }
                                    if (configurations.Course == null)
                                    {
                                        totalRecordRejected++;
                                        sbError.Clear();
                                        rulerejected.Semester = txtSem;
                                        rulerejected.UserId = displayUserID;
                                        rulerejected.Languages = txtLanguages;
                                        rulerejected.Subject = txtCategory;
                                        rulerejected.Unit = txtSubCategory;
                                        rulerejected.ErrMessage = "Course Not Found.Please enter valid data";
                                        accessibilityRuleImports.Add(rulerejected);
                                        sbError.Clear();
                                        rulerejected = new APIAccessibilityRuleImport();
                                        continue;
                                    }

                                    if (configurations.Semester == "null")
                                    {
                                        LocationId = null;
                                    }
                                    else
                                    {
                                        LocationId = Convert.ToInt32(configurations.Semester);
                                    }
                                    if (configurations.Semester == null)
                                    {

                                        totalRecordRejected++;
                                        sbError.Clear();
                                        rulerejected.Semester = txtSem;
                                        rulerejected.UserId = displayUserID;
                                        rulerejected.Languages = txtLanguages;
                                        rulerejected.Subject = txtCategory;
                                        rulerejected.Unit = txtSubCategory;
                                        rulerejected.ErrMessage = "Semester Not Found.Please enter valid data";
                                        accessibilityRuleImports.Add(rulerejected);

                                        rulerejected = new APIAccessibilityRuleImport();

                                        continue;
                                    }
                                    Category getcategory = await _accessibilityRule.GetCategoryId(txtCategory);
                                    if (txtCategory != "")
                                    {
                                        if (getcategory == null)
                                        {
                                            totalRecordRejected++;
                                            sbError.Clear();
                                            rulerejected.Semester = txtSem;
                                            rulerejected.UserId = displayUserID;
                                            rulerejected.Languages = txtLanguages;
                                            rulerejected.Subject = txtCategory;
                                            rulerejected.Unit = txtSubCategory;
                                            rulerejected.ErrMessage = "Subject Not Found.Please Enter Valid data";
                                            accessibilityRuleImports.Add(rulerejected);
                                            rulerejected = new APIAccessibilityRuleImport();
                                            continue;
                                        }
                                    }

                                    APISubCategory getsubcategory = await _accessibilityRule.GetSubCategoryId(txtSubCategory, getcategory.Id);
                                    if (txtSubCategory != "")
                                    {
                                        if (getsubcategory == null)
                                        {
                                            totalRecordRejected++;
                                            sbError.Clear();
                                            rulerejected.Semester = txtSem;
                                            rulerejected.UserId = displayUserID;
                                            rulerejected.Languages = txtLanguages;
                                            rulerejected.Subject = txtCategory;
                                            rulerejected.Unit = txtSubCategory;

                                            rulerejected.ErrMessage = "Unit Not Found.Please enter valid data";
                                            accessibilityRuleImports.Add(rulerejected);
                                            rulerejected = new APIAccessibilityRuleImport();
                                            continue;
                                        }

                                    }
                                    if (getsubcategory == null)
                                    {
                                        SubCategoryId = null;
                                    }
                                    else
                                    {
                                        SubCategoryId = getsubcategory.Id;
                                    }

                                    if (configurations.UserId == "null")
                                    {
                                        UserID = null;
                                    }
                                    else
                                    {
                                        UserID = Convert.ToInt32(configurations.UserId);
                                    }
                                    if (configurations.UserId == null)
                                    {

                                        totalRecordRejected++;
                                        sbError.Clear();
                                        rulerejected.Semester = txtSem;
                                        rulerejected.UserId = displayUserID;
                                        rulerejected.Languages = txtLanguages;
                                        rulerejected.Subject = txtCategory;
                                        rulerejected.Unit = txtSubCategory;
                                        rulerejected.ErrMessage = "UserId Not Found.Please enter valid data";
                                        accessibilityRuleImports.Add(rulerejected);

                                        rulerejected = new APIAccessibilityRuleImport();
                                        continue;

                                    }

                                    if (configurations.Languages == "null")
                                    {
                                        LanguageId = null;
                                    }
                                    else
                                    {
                                        LanguageId = Convert.ToInt32(configurations.Languages);
                                    }
                                    if (configurations.Languages == null)
                                    {

                                        totalRecordRejected++;
                                        sb.Clear();
                                        rulerejected.Semester = txtSem;
                                        rulerejected.UserId = displayUserID;
                                        rulerejected.Languages = txtLanguages;
                                        rulerejected.Subject = txtCategory;
                                        rulerejected.Unit = txtSubCategory;
                                        rulerejected.ErrMessage = "Languages Not Found.Please enter valid data";
                                        accessibilityRuleImports.Add(rulerejected);

                                        rulerejected = new APIAccessibilityRuleImport();
                                        continue;
                                    }


                                    bool accessibilityRule = await _accessibilityRule.GetAccessibilityRulesForCategory(getcategory.Id, SubCategoryId, LocationId, Config1, UserID, LanguageId);
                                    if (accessibilityRule == false)
                                    {
                                        totalRecordInsert++;
                                        accrule.Id = 0;
                                        accrule.ConfigurationColumn2 = LanguageId == null ? null : LanguageId;
                                        accrule.UserID = UserID == null ? null : UserID;
                                        accrule.CourseId = courseInfo.Id;
                                        accrule.CategoryId = getcategory.Id;//usermaster id 
                                        accrule.SubCategoryId = SubCategoryId == null ? null : SubCategoryId;
                                        accrule.ConfigurationColumn1 = Config1 == null ? null : Config1;
                                        accrule.Location = LocationId == null ? null : LocationId;
                                        accrule.ConditionForRules = "and";
                                        accrule.CreatedBy = 1;
                                        accrule.ModifiedBy = 1;
                                        accrule.CreatedDate = DateTime.UtcNow;
                                        accrule.ModifiedDate = DateTime.UtcNow;
                                        await _accessibilityRule.Add(accrule);
                                    }
                                    else
                                    {
                                        totalRecordRejected++;
                                        sbError.Clear();
                                        rulerejected.Semester = txtSem;
                                        rulerejected.UserId = displayUserID;
                                        rulerejected.Languages = txtLanguages;
                                        rulerejected.Subject = txtCategory;
                                        rulerejected.Unit = txtSubCategory;
                                        rulerejected.ErrMessage = "Duplicate Record";
                                        accessibilityRuleImports.Add(rulerejected);

                                        rulerejected = new APIAccessibilityRuleImport();
                                        continue;

                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                    result = "true";
                                }
                            }

                            errorzone:
                            if (validvalue == true)
                            {
                                totalRecordRejected++;

                                rulerejected.Languages = txtLanguages;
                                rulerejected.Semester = txtSem;
                                rulerejected.Course = txtCourse;
                                rulerejected.Subject = txtCategory;
                                rulerejected.Unit = txtSubCategory;
                                rulerejected.ErrMessage = sbError.ToString();
                                rulerejected.UserId = displayUserID;
                                accessibilityRuleImports.Add(rulerejected);
                                courseInfo = new Model.Course();
                                sbError.Clear();
                                accrule = new AccessibilityRule();

                            }

                        }
                    }

                }
                string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record rejected : " + totalRecordRejected;
                int TotalCount = totalRecordInsert + totalRecordRejected;
                if (rulerejected.Course == null && rulerejected.Unit == null && rulerejected.Semester == null && rulerejected.UserId == null && rulerejected.ErrMessage == null && rulerejected.Languages == null)
                {
                    //response.
                    response.StatusCode = 200;

                    response.ResponseObject = new { resultstring, TotalCount, rulerejected = accessibilityRuleImports.ToArray() };

                }
                else
                {
                    response.StatusCode = 200;
                    response.ResponseObject = new { resultstring, TotalCount, rulerejected = accessibilityRuleImports.ToArray() };
                }
                return response;
            }

            public bool ValidateCategory(string headerText, string Category)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (Category != null && !string.IsNullOrEmpty(Category))
                    {
                        accessibilityRuleImport.Subject = Category;
                    }
                    else
                    {
                        accessibilityRuleImport.Subject = Category;

                        sbError.Append("Subject is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public bool ValidateSubCategory(string headerText, string SubCategory)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (SubCategory != null && !string.IsNullOrEmpty(SubCategory))
                    {
                        accessibilityRuleImport.Unit = SubCategory;
                    }
                    else
                    {
                        accessibilityRuleImport.Unit = SubCategory;

                        sbError.Append("University is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public bool ValidateCourse(string headerText, string Course)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (Course != null && !string.IsNullOrEmpty(Course))
                    {
                        accessibilityRuleImport.Course = Course;
                    }
                    else
                    {
                        accessibilityRuleImport.Course = Course;

                        sbError.Append("Course is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

        }


    }
}
















