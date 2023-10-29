
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using UserMasterImportFieldEncrypted = User.API.APIModel.UserMasterImportFieldEncrypted;
using APIIsUserValid = User.API.APIModel.APIIsUserValidHRMS;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using User.API.Repositories.Interfaces;
using AutoMapper.Configuration;
using Microsoft.Data.SqlClient;
using SqlParameter = Microsoft.Data.SqlClient.SqlParameter;
using Microsoft.IdentityModel.Tokens;

namespace User.API.Repositories
{
    public class HRMSBaseRepository : IHRMSBaseRepository
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(HRMSBaseRepository));
        private UserDbContext _db;
        public HRMSBaseRepository(UserDbContext context
          )
        {
            this._db = context;
            
        }

        public   UserDbContext CreateContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder
            .UseSqlServer(connectionString, opt => opt.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new UserDbContext(optionsBuilder.Options);
        }

        public async Task<IEnumerable<APIUserSettingNew>> GetUserSetting( string connectionString)
        {
            List<APIUserSettingNew> allUserSetting;
            try
            {
                using (var dbContext = CreateContext(connectionString))
                {
                    var userSetting = (from userSettings in dbContext.UserSettings
                                       where (userSettings.IsDeleted == 0 && userSettings.IsConfigured == true)
                                       select new APIUserSettingNew
                                       {
                                           Id = userSettings.Id,
                                           ConfiguredColumnName = userSettings.ConfiguredColumnName,
                                           ChangedColumnName = userSettings.ChangedColumnName,
                                           IsConfigured = userSettings.IsConfigured,
                                           IsMandatory = userSettings.IsMandatory,
                                           IsShowInReport = userSettings.IsShowInReport,
                                           //IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard,
                                           FieldType = userSettings.FieldType
                                       });
                    allUserSetting = await userSetting.ToListAsync();
                    if (allUserSetting == null)
                        allUserSetting = new List<APIUserSettingNew>();
                }
                return allUserSetting;

            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        public DataTable ChangeRawTable(DataTable userImportdt, string OrgCode, Dictionary<String, string> initialColumns)
        {
            DataTable tempDt = new DataTable();
            for (int i = 0; i < userImportdt.Columns.Count; i++)
            {
                string col = userImportdt.Columns[i].ColumnName.Replace("*", "");
                if (initialColumns.ContainsKey(col))
                    tempDt.Columns.Add(initialColumns[col], typeof(string));
                else
                    continue;
            }

            for (int i = 0; i < userImportdt.Rows.Count; i++)
            {
                DataRow dr = tempDt.NewRow();
                int cellCount = 0;
                for (int j = 0; j < userImportdt.Columns.Count; j++)
                {
                    if (initialColumns.ContainsKey(userImportdt.Columns[j].ColumnName))
                        dr[cellCount++] = userImportdt.Rows[i][j];
                }
                tempDt.Rows.Add(dr);
            }
            return tempDt;
        }

        public async Task<APIIsFileValid> ValidateFileColumnHeaders(DataTable userImportdt, string OrgCode, List<APIUserSettingNew> aPIUserSettings)
        {
            try
            {
                List<string> importColumns = GetUserMasterImportColumns(OrgCode);
                List<string> validColumnsList = GetMandatoryColumns();
                validColumnsList.AddRange(aPIUserSettings.Where(x => x.IsMandatory == true).Select(x => x.ConfiguredColumnName).ToList());
                APIIsFileValid aPIIsFileValid = new APIIsFileValid();

                if (validColumnsList.Count > userImportdt.Columns.Count)
                {
                    aPIIsFileValid.Flag = false;
                    return aPIIsFileValid;
                }
                DataTable tempDt = new DataTable();
                for (int i = 0; i < userImportdt.Columns.Count; i++)
                {
                    string col = userImportdt.Columns[i].ColumnName.Replace("*", "");
                    userImportdt.Columns[i].ColumnName = col;
                   
                    APIUserSettingNew aPIUserSetting = aPIUserSettings.Where(x => x.ChangedColumnName.Trim().ToLower() == col.ToLower()).FirstOrDefault();
                    if (aPIUserSetting != null)
                        tempDt.Columns.Add(aPIUserSetting.ConfiguredColumnName, typeof(string));
                    else
                        tempDt.Columns.Add(userImportdt.Columns[i].ColumnName, typeof(string));

                    if (!importColumns.Contains(tempDt.Columns[i].ColumnName))
                    {
                        if (col.ToLower() == "isactive")
                            continue;
                        return new APIIsFileValid { Flag = false, userImportdt = userImportdt }; 
                    }
                    //continue;

                }
                for (int i = 0; i < userImportdt.Rows.Count; i++)
                {
                    DataRow dr = tempDt.NewRow();
                    for (int j = 0; j < userImportdt.Columns.Count; j++)
                        dr[j] = userImportdt.Rows[i][j];
                    tempDt.Rows.Add(dr);
                }

                return new APIIsFileValid { Flag = true, userImportdt = tempDt };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private List<string> GetUserMasterImportColumns(string OrgCode = null)
        {
            List<string> columns = new List<string>();
            columns.AddRange(GetMandatoryColumns());
            columns.Add(UserMasterImportFieldNew.Business);
            columns.Add(UserMasterImportFieldNew.Group);
            columns.Add(UserMasterImportFieldNew.Area);
            columns.Add(UserMasterImportFieldNew.Location);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn1);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn2);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn3);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn4);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn5);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn6);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn7);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn8);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn9);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn10);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn11);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn12);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn13);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn14);
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn15);
            columns.Add(UserMasterImportFieldNew.Language);
            columns.Add(UserMasterImportFieldNew.Currency);
            columns.Add(UserMasterImportFieldNew.DateOfBirth);
            columns.Add(UserMasterImportFieldNew.DateOfJoining);
            columns.Add(UserMasterImportFieldNew.AccountExpiryDate);
            columns.Add(UserMasterImportFieldNew.Gender);
            columns.Add(UserMasterImportFieldNew.ReportsTo);
            columns.Add(UserMasterImportFieldNew.UserType);
            columns.Add(UserMasterImportFieldNew.UserRole);
            columns.Add(UserMasterImportFieldNew.JobRole);
            columns.Add(UserMasterImportFieldNew.DateIntoRole);
            // if (OrgCode.ToLower() == "sbil"|| OrgCode.ToLower() == "rebit"|| OrgCode.ToLower() == "rockman"||OrgCode.ToLower()=="akgroup"
            //   ||OrgCode.ToLower() == "analytix"||OrgCode=="padmini")
            if (OrgCode != "tatasky" && OrgCode.ToLower() != "kotak" && OrgCode.ToLower() != "firstmeridian")
                columns.Add(UserMasterImportFieldNew.IsActive);
            return columns;
        }

        private List<string> GetMandatoryColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportFieldNew.UserId);
            columns.Add(UserMasterImportFieldNew.EmailId);
            columns.Add(UserMasterImportFieldNew.UserName);
            columns.Add(UserMasterImportFieldNew.MobileNumber);
            return columns;
        }

        private List<string> GetUserMasterColumnsForSQLInjection(string OrgCode)
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportFieldNew.UserName.ToLower());
            columns.Add(UserMasterImportFieldNew.Business.ToLower());
            columns.Add(UserMasterImportFieldNew.Group.ToLower());
            columns.Add(UserMasterImportFieldNew.Area.ToLower());
            columns.Add(UserMasterImportFieldNew.Location.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn12.ToLower());
            if (String.Compare(OrgCode, "ujjivan", true) == 0)
            {
                columns.Add(UserMasterImportFieldNew.ConfigurationColumn13.ToLower());
                columns.Add(UserMasterImportFieldNew.ConfigurationColumn14.ToLower());
                columns.Add(UserMasterImportFieldNew.ConfigurationColumn15.ToLower());
            }
            columns.Add(UserMasterImportFieldNew.Language.ToLower());
            columns.Add(UserMasterImportFieldNew.Currency.ToLower());
            columns.Add(UserMasterImportFieldNew.Gender.ToLower());
            columns.Add(UserMasterImportFieldNew.UserType.ToLower());
            columns.Add(UserMasterImportFieldNew.UserRole.ToLower());
            columns.Add(UserMasterImportFieldNew.JobRole.ToLower());
            return columns;
        }

        private List<string> GetUserMasterEncryptedImportColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportFieldNew.UserId.ToLower());
            columns.Add(UserMasterImportFieldNew.EmailId.ToLower());
            columns.Add(UserMasterImportFieldNew.MobileNumber.ToLower());
            columns.Add(UserMasterImportFieldNew.ReportsTo.ToLower());
            columns.AddRange(GetConfigurationColumns());
            return columns;
        }

        private List<string> GetConfigurationColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportFieldNew.Business.ToLower());
            columns.Add(UserMasterImportFieldNew.Group.ToLower());
            columns.Add(UserMasterImportFieldNew.Area.ToLower());
            columns.Add(UserMasterImportFieldNew.Location.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn12.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn13.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn14.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn15.ToLower());
            return columns;
        }

        private List<string> GetColumnsForLowerCase()
        {
            List<string> columns = new List<string>();
            columns.AddRange(GetConfigurationColumns());
            columns.Add(UserMasterImportFieldNew.JobRole.ToLower());
            return columns;
        }

        public async Task<string> ProcessRecordsAsync(int UserId, DataTable userImportdt, string OrgCode, List<APIUserSettingNew> aPIUserSettings, string RandomPassword, string spName, string importTableName, 
            HRMSBasicData hRMSBasicData = null, string connectionString=null)
        {
            int totalInserted = 0, totalUpdated = 0, totalRejected = 0;
            string DeafultPassword = Security.EncryptSHA512(RandomPassword);

            bool Ldap = false;
           
            {
                if (Convert.ToBoolean(hRMSBasicData.LDap.ToLower()))
                    Ldap = true;
            }
            ///Default Settings
            string allowSpecialCharInUserName = "false";
            string lowerCaseAllow = "true";
            string applicationDateFormat = "null";

            if (hRMSBasicData != null)
            {
                allowSpecialCharInUserName = hRMSBasicData.allowSpecialCharInUserName;
                lowerCaseAllow = hRMSBasicData.lowerCaseAllow;
                applicationDateFormat = hRMSBasicData.applicationDateFormat;
            }
            List<string> allcolumns = this.GetUserMasterImportColumns(OrgCode);
            List<string> encryptedcolumns = this.GetUserMasterEncryptedImportColumns();
            List<string> lowerCaseColumns = this.GetColumnsForLowerCase();

            int columnIndex = 0;
            DataColumnCollection columns = userImportdt.Columns;
            foreach (string column in allcolumns)
            {
                if (!columns.Contains(column))
                    userImportdt.Columns.Add(column, typeof(string));
                userImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }

            foreach (string column in encryptedcolumns)
            {
                userImportdt.Columns.Add(column + "Encrypted", typeof(string));
            }
            if (!userImportdt.Columns.Contains("IsValid"))
                userImportdt.Columns.Add("IsValid", typeof(bool));
            if (!userImportdt.Columns.Contains("ErrorMessage"))
                userImportdt.Columns.Add("ErrorMessage", typeof(string));

            List<string> sQLInjectionColumns = this.GetUserMasterColumnsForSQLInjection(OrgCode);
            //_logger.Debug("Table Processing Started at: " + DateTime.Now);
            int totalrecords = userImportdt.Rows.Count;

            int totalRecordsSplitCount = totalrecords > 100 ? totalrecords / 10 : 1;
            DataTable[] processedDataTables = null;
            if (totalRecordsSplitCount == 1)
            {
                Task<DataTable> task = Task.Run(() => ProcessInputRecordsAsync(userImportdt, allcolumns, sQLInjectionColumns, encryptedcolumns, OrgCode, allowSpecialCharInUserName, lowerCaseAllow, lowerCaseColumns, applicationDateFormat, aPIUserSettings, hRMSBasicData));
                processedDataTables = await Task.WhenAll(task);
            }
            else
            {
                List<DataTable> dataTables = CloneTable(userImportdt, totalRecordsSplitCount);
                int dtcount = dataTables.Count;
                Task<DataTable>[] tasks = new Task<DataTable>[dtcount];
                for (int i = 0; i < dtcount; i++)
                {
                    DataTable tempDt = dataTables[i];
                    tasks[i] = Task.Run(() => ProcessInputRecordsAsync(tempDt, allcolumns, sQLInjectionColumns, encryptedcolumns, OrgCode, allowSpecialCharInUserName, lowerCaseAllow, lowerCaseColumns, applicationDateFormat, aPIUserSettings, hRMSBasicData));
                }

                try
                {
                    processedDataTables = await Task.WhenAll(tasks);
                }
                catch (AggregateException ae)
                {
                    throw ae;
                }
            }

            DataTable finalDt = new DataTable();
            foreach (DataTable dt in processedDataTables)
                finalDt.Merge(dt);

            finalDt = setExplicitPassword(finalDt, RandomPassword, OrgCode);
            
            DataTable resultTable = new DataTable();

            DataTable dtResult = new DataTable();
            try
            {
                using (var dbContext = CreateContext(connectionString))
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        UserId = 1;
                        Ldap = false;
                        cmd.CommandText = spName;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                        cmd.Parameters.Add(new SqlParameter("@IsLDAP", SqlDbType.Bit) { Value = Ldap });
                        cmd.Parameters.Add(new SqlParameter("@DefaultPassword", SqlDbType.VarChar) { Value = DeafultPassword });
                        cmd.Parameters.Add(new SqlParameter(importTableName, SqlDbType.Structured) { Value = finalDt });

                        cmd.CommandTimeout = 0;
                        SqlParameter totalInsertedParam = new SqlParameter("@TotalInserted", SqlDbType.Int);
                        totalInsertedParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(totalInsertedParam);
                        SqlParameter totalUpdatedParam = new SqlParameter("@TotalUpdated", SqlDbType.Int);
                        totalUpdatedParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(totalUpdatedParam);
                        SqlParameter totalRejectedParam = new SqlParameter("@TotalRejected", SqlDbType.Int);
                        totalRejectedParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(totalRejectedParam);
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        dtResult.Load(reader);
                        totalInserted = Convert.ToInt32(totalInsertedParam.Value);
                        totalUpdated = Convert.ToInt32(totalUpdatedParam.Value);
                        totalRejected = Convert.ToInt32(totalRejectedParam.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (OrgCode.ToLower() == "tablez")
            {
                //if (status == "Inserted")
                //{
                //    // SMS send function
                //    if (Program.SMS == "ON")
                //    {
                //        string sms = "Event: User Activation "

                //                 + " Your user name is " + ' ' + aPIHRMS.UserId + ' ' + "and Password is  Pass@123  for Empowered. Go to" + ' ' + Program.SMSLink + ' ' + "or download the mobile app from the App/ Play Store.";

                //        SendSMS_TableZ(+91 + aPIHRMS.MobileNumber, sms);
                //    }
                //}

            }

            return "TotalRecordInserted: " + totalInserted + "  TotalRecordUpdated: " + totalUpdated + "   TotalRecordRejected: " + totalRejected;
        }

        public async Task<APIHRMSResponseNew> GetHRMSProcessStatus(DataTable dtTable, string connectionstring)        
        {
            try
            {
                DataRow dr = dtTable.Rows[0];
                string username = dr["UserId"].ToString();
                string IsActive = dr["isactive"].ToString();

                var context = CreateContext(connectionstring);
                var hrmsusers = await context.UserRejectedLog.Where(u=> u.CreatedDate>DateTime.Now.AddMinutes(-15)).Select(
                     u=>new APIHRMSEmployee
                     {
                        EmpId=u.UserId,
                        Action=u.ErrorMessage,
                        createdDate=u.CreatedDate??DateTime.Now
                     }
                    ).ToListAsync();
                //foreach (var emp in hrmsusers)
                //{
                var emp =  hrmsusers.OrderByDescending(u=>u.createdDate).FirstOrDefault();
                    string action = emp.Action;
                    if (action.ToLower().Contains("inserted") || action.ToLower().Contains("updated"))
                    {
                        var user2 = await context.UserMaster.FirstOrDefaultAsync(u => u.UserId == Security.Encrypt(emp.EmpId) && u.IsDeleted == false);
                        if (user2 != null)
                        {
                            emp.EmpName = user2.UserName;
                            emp.EmpStatus = user2.IsActive == true ? "Active" : "InActive";
                        }
                    }
                //}
                APIHRMSResponseNew aPIHRMSResponse = new APIHRMSResponseNew();    
                aPIHRMSResponse.AppName = "Empowered LMS";
                //var user= hrmsusers.FirstOrDefault();
                aPIHRMSResponse.EmpId = emp.EmpId;
                aPIHRMSResponse.EmpName = emp.EmpName;
                if (emp.EmpStatus !=  null)
                    aPIHRMSResponse.EmpStatus = emp.EmpStatus == "Active" ? "Y" : "N";

                aPIHRMSResponse.StatusMessage = emp.Action;
                if (emp.Action.ToLower().Contains("insert"))
                {
                    aPIHRMSResponse.Action = "Created";
                    aPIHRMSResponse.StatusCode = "SR";
                }
                else if (emp.Action.ToLower().Contains("update"))
                {
                    aPIHRMSResponse.Action = "Updated";
                    aPIHRMSResponse.StatusCode = "SR";
                }
                else
                {
                    aPIHRMSResponse.Action = "Failed";
                    aPIHRMSResponse.StatusCode = "ER";
                }

                if (aPIHRMSResponse.EmpName is null)
                    aPIHRMSResponse.EmpName = username;
                
                if (aPIHRMSResponse.EmpStatus is null)
                    aPIHRMSResponse.EmpStatus = IsActive== "True" ? "Y":"N";
                
                return aPIHRMSResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<DataTable> ProcessIMSRecord(DataTable dataTable, String OrgCode)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                string email = Convert.ToString(row["EmailId"]);
                string mobile = Convert.ToString(row["MobileNumber"]);
                var aPIIsUserValidemail = ValidateEmailId(email, OrgCode);
                var aPIIsUserValidmobile = ValidateMobileNumber(mobile, OrgCode);
                if (aPIIsUserValidemail.Flag && aPIIsUserValidmobile.Flag)
                {
                    row["ErrorMessage"] = "Enter valid email and Mobile Number";
                    row["IsValid"] = 0;
                }
            }
            return dataTable;
        }

        //
        //public DataTable selectiveValidationByOrg(DataTable datatable, HRMSBasicData hRMSBasicData)
        //{

        //    if (hRMSBasicData.OrgCode.ToLower() == "loconav")
        //    {
        //        LoconavHRMS loconavHRMS = new LoconavHRMS(hRMSBasicData);
        //        DataTable dTable = new DataTable();
        //        dTable = loconavHRMS.ValidationPostDataProcessing(datatable).Result;
        //        datatable = dTable;
        //    }

        //    return datatable;

        //}
        public DataTable setExplicitPassword(DataTable passwordtable, string DefaultPassword, string OrgCode)
        {

            string encryptedPwdColumn = "EncryptedPassword";
            DataColumn dc = new DataColumn(encryptedPwdColumn);
            dc.DefaultValue = DBNull.Value;
            passwordtable.Columns.Add(dc);

            string pwdColumn = "Password";
            dc = new DataColumn(pwdColumn);
            dc.DefaultValue = DBNull.Value;
            passwordtable.Columns.Add(dc);
            string encryptedPwd = Security.EncryptSHA512(DefaultPassword);
            foreach (DataRow row in passwordtable.Rows)
            {
                row[pwdColumn] = DefaultPassword;
                row[encryptedPwdColumn] = encryptedPwd;
            }
            return passwordtable;
        }

        private DataTable extraColumn(DataTable dt, string isActive)
        {
            DataColumn dc = new DataColumn("IsActive");
            bool value = false;
            if (isActive.ToLower().Contains("inactive"))
                value = false;
            else
                value = true;
            dc.DefaultValue = value;
            dt.Columns.Add(dc);
            return dt;
        }

        private List<KeyValuePair<string, int>> GetUserMasterColumnDataLength()
        {
            var list = new List<KeyValuePair<string, int>>() {
              new KeyValuePair<string, int>(UserMasterImportFieldNew.UserId,100),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.EmailId,100),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.UserName,200),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.MobileNumber,20),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.Business,200),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.Group,200),
              new KeyValuePair<string, int>(UserMasterImportFieldNew.Area,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.Location,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn1,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn2,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn3,600),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn4,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn5,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn6,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn7,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn8,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn9,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn10,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn11,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn12,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn13,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn14,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ConfigurationColumn15,200),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.Language,50),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.Currency,50),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.Gender,100),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.ReportsTo,100),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.UserType,100),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.UserRole,100),
             new KeyValuePair<string, int>(UserMasterImportFieldNew.JobRole,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.UserIdEncrypted,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.EmailIdEncrypted,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.MobileNumberEncrypted,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ReportsToEncrypted,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.BusinessEncrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.GroupEncrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.AreaEncrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.LocationEncrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn1Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn2Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn3Encrypted,1000),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn4Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn5Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn6Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn7Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn8Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn9Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn10Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn11Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn12Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn13Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn14Encrypted,200),
             new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn15Encrypted,200),
         };
            return list;
        }
        private List<string> GetUserMasterImportColumnsForLength()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportFieldNew.UserId.ToLower());
            columns.Add(UserMasterImportFieldNew.EmailId.ToLower());
            columns.Add(UserMasterImportFieldNew.UserName.ToLower());
            columns.Add(UserMasterImportFieldNew.MobileNumber.ToLower());
            columns.Add(UserMasterImportFieldNew.Business.ToLower());
            columns.Add(UserMasterImportFieldNew.Group.ToLower());
            columns.Add(UserMasterImportFieldNew.Area.ToLower());
            columns.Add(UserMasterImportFieldNew.Location.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn12.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn13.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn14.ToLower());
            columns.Add(UserMasterImportFieldNew.ConfigurationColumn15.ToLower());
            columns.Add(UserMasterImportFieldNew.Language.ToLower());
            columns.Add(UserMasterImportFieldNew.Currency.ToLower());
            columns.Add(UserMasterImportFieldNew.Gender.ToLower());
            columns.Add(UserMasterImportFieldNew.ReportsTo.ToLower());
            columns.Add(UserMasterImportFieldNew.UserType.ToLower());
            columns.Add(UserMasterImportFieldNew.UserRole.ToLower());
            columns.Add(UserMasterImportFieldNew.JobRole.ToLower());
            return columns;
        }
        private async Task<DataTable> ProcessInputRecordsAsync(DataTable processedDataTable, List<string> allColumns, List<string> SQLInjectionColumns, List<string> encryptedcolumns, string OrgCode, string userNameValidation, string lowerCaseAllow, List<string> lowerCaseColumns, string validDateFormat, List<APIUserSettingNew> aPIUserSettings, HRMSBasicData hRMSBasicData = null)
        {
            Dictionary<string, bool> validationMAtrix = hRMSBasicData.validationMatrix;

            //To Do - Need to check for threading large records
            DataTable userImportdt = processedDataTable.Copy();
            var columnValueLength = GetUserMasterColumnDataLength();
            List<string> columnsForLength = GetUserMasterImportColumnsForLength();
            try
            {
                foreach (DataRow row in userImportdt.Rows)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(row["ErrorMessage"])))  //Record alredy been validated.
                        continue;

                    APIIsUserValid aPIIsUserValid = new APIIsUserValid
                    {
                        Flag = false,
                        ErrorMessage = null
                    };
                    foreach (string currentHeader in allColumns)
                    {
                        string celldata = Convert.ToString(row[currentHeader]).Trim();
                        row[currentHeader] = celldata;
                        switch (currentHeader)
                        {
                            case UserMasterImportFieldNew.UserId:
                                row[currentHeader] = celldata.ToLower();
                                aPIIsUserValid = ValidateUserIdNew(currentHeader, celldata.ToLower());
                                break;
                            case UserMasterImportFieldNew.EmailId:
                                row[currentHeader] = celldata.ToLower();
                                string userType = null;
                                try
                                {
                                    userType = row["UserType"].ToString();
                                }
                                catch (Exception)
                                {
                                }

                                aPIIsUserValid = ValidateEmailId(celldata.ToLower(), OrgCode);
                                break;

                            case UserMasterImportFieldNew.UserName:
                                if (validationMAtrix["username"])
                                    aPIIsUserValid = ValidateUserName(currentHeader, celldata, userNameValidation);
                                else
                                    aPIIsUserValid = new APIIsUserValid { Flag = false, ErrorMessage = "" };
                                break;
                            case UserMasterImportFieldNew.MobileNumber:
                                if (OrgCode.ToLower() != "tablez" && OrgCode.ToLower() != "tatacapital")
                                    aPIIsUserValid = ValidateMobileNumber(celldata, OrgCode);
                                break;
                            case UserMasterImportFieldNew.Gender:
                                if (celldata.ToLower() == "m")
                                    row["Gender"] = "Male";
                                else if (celldata.ToLower() == "f")
                                    row["Gender"] = "Female";
                                else if (celldata.ToLower() == "o")
                                    row["Gender"] = "Other";

                                break;
                            case UserMasterImportFieldNew.ReportsTo:
                                row[currentHeader] = celldata.ToLower();
                                aPIIsUserValid = ValidateReportsTo(currentHeader, celldata);
                                break;
                            case UserMasterImportFieldNew.DateOfBirth:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateOfBirth = this.ValidateDateOfBirth(celldata, validDateFormat);
                                    /*if (DateOfBirth == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = "PleaseEnterValid" + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateOfBirth;*/
                                    if (DateOfBirth != null)
                                        row[currentHeader] = DateOfBirth;
                                    else
                                        row[currentHeader] = DBNull.Value;

                                }
                                break;
                            case UserMasterImportFieldNew.DateOfJoining:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateOfJoining = this.ValidateDateOfJoining(celldata, validDateFormat);
                                    /*if (DateOfJoining == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = "PleaseEnterValid:" + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateOfJoining;*/
                                    if (DateOfJoining != null)
                                        row[currentHeader] = DateOfJoining;
                                    else
                                        row[currentHeader] = DBNull.Value;

                                }
                                break;
                            case UserMasterImportFieldNew.AccountExpiryDate:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string AccountExpiryDate = this.ValidateAccountExpiryDate(celldata, validDateFormat);
                                    /*if (AccountExpiryDate == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = "PleaseEnterValid" + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = AccountExpiryDate;
                                    */

                                    if (AccountExpiryDate != null)
                                        row[currentHeader] = AccountExpiryDate;
                                    else
                                        row[currentHeader] = DBNull.Value;
                                }
                                break;
                            case UserMasterImportFieldNew.DateIntoRole:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateIntoRole = this.ValidateDateIntoRole(celldata, validDateFormat);
                                    /*if (DateIntoRole == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = "PleaseEnterValid" + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateIntoRole;
                                    */
                                    if (DateIntoRole != null)
                                        row[currentHeader] = DateIntoRole;
                                    else
                                        row[currentHeader] = DBNull.Value;
                                }
                                break;
                        }

                        if (columnsForLength.Contains(currentHeader.ToLower()))
                        {
                            string currentHeaderdata = Convert.ToString(row[currentHeader]).ToLower();
                            string changedHeaderName = aPIUserSettings.Where(x => x.ConfiguredColumnName.ToLower() == currentHeader.ToLower()).Select(x => x.ChangedColumnName).FirstOrDefault();
                            changedHeaderName = changedHeaderName == null ? currentHeader : changedHeaderName;
                            if (currentHeaderdata.Length > Convert.ToInt32(columnValueLength.Where(h => h.Key == currentHeader).Select(h => h.Value).FirstOrDefault()))
                            {
                                aPIIsUserValid.Flag = true;
                                row[currentHeader] = string.Empty;
                                aPIIsUserValid.ErrorMessage = "Please Enter Valid Length Data" + changedHeaderName;
                                break;
                            }

                            if (encryptedcolumns.Contains(currentHeader.ToLower()) && !string.IsNullOrEmpty(Convert.ToString(row[currentHeader])))
                            {
                                currentHeaderdata = Security.Encrypt(row[currentHeader].ToString());
                                if (currentHeaderdata.Length > Convert.ToInt32(columnValueLength.Where(h => h.Key == currentHeader + "Encrypted").Select(h => h.Value).FirstOrDefault()))
                                {
                                    aPIIsUserValid.Flag = true;
                                    aPIIsUserValid.ErrorMessage = "Please Enter Valid Length Data" + changedHeaderName; break;
                                }
                            }
                        }

                        if (!aPIIsUserValid.Flag)
                        {
                            if (lowerCaseColumns.Contains(currentHeader.ToLower()) && lowerCaseAllow == "Yes")
                                row[currentHeader] = Convert.ToString(row[currentHeader]).ToLower();

                            if (encryptedcolumns.Contains(currentHeader.ToLower()) && !string.IsNullOrEmpty(Convert.ToString(row[currentHeader])))
                                row[currentHeader + "Encrypted"] = Security.Encrypt(row[currentHeader].ToString().Trim());
                        }


                        if (aPIIsUserValid.ErrorMessage != null && aPIIsUserValid.Flag)
                            break;
                    }

                    row["IsValid"] = !aPIIsUserValid.Flag;
                    row["ErrorMessage"] = aPIIsUserValid.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return userImportdt;
        }
        private List<DataTable> CloneTable(DataTable tableToClone, int countLimit)
        {
            List<DataTable> tables = new List<DataTable>();
            int count = 0;
            DataTable copyTable = null;

            foreach (DataRow dr in tableToClone.Rows)
            {
                if ((count++ % countLimit) == 0)
                {
                    copyTable = new DataTable();
                    copyTable = tableToClone.Clone();
                    copyTable.TableName = "TableCount" + count;
                    tables.Add(copyTable);
                }
                copyTable.ImportRow(dr);
            }
            return tables;
        }
        public APIIsUserValid ValidateUserIdNew(string headerText, string userId)
        {
            bool flag = true;
            string errorMessage = string.Empty;
            if (!string.IsNullOrEmpty(userId))
            {
                var userIdPattern = new Regex(Constants.RegularExpression.userIdPattern);
                var regexofficialmailid = new Regex(Constants.RegularExpression.emailIdPattern);
                bool validuserid = (userIdPattern.IsMatch(userId) || regexofficialmailid.IsMatch(userId));
                if (!validuserid || userId.Contains(" "))
                    errorMessage = "Special Character contains in User ID, Record rejected";
                else if (hasSpecialChar(userId, @"&='-/+,<>*_@"))
                {
                    flag = true;
                    char specialChar = returnSpecialChar(userId);
                    errorMessage = String.Format("Special Character {0} contains in UserId, Record rejected ", specialChar);
                }
                else
                    flag = false;
            }
            else
                errorMessage = headerText + "Empty Field Message";

            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
        }
        public bool ValidateUserId(string headerText, string userId)
        {
            bool valid = false;
            var userIdPattern = new Regex("[a-zA-Z0-9][a-zA-Z0-9_-]*");
            var regexofficialmailid = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            //UserId
            try
            {
                if (userId != null && !string.IsNullOrEmpty(userId))
                {
                    bool iduser = false;
                    bool idwithEmail = false;
                    idwithEmail = (regexofficialmailid.IsMatch(userId));
                    iduser = (userIdPattern.IsMatch(userId));

                    bool validuserid = (iduser || idwithEmail);
                    if (validuserid && !userId.ToString().Contains(" "))
                    {

                    }
                    else
                    {
                        valid = true;
                    }

                }
                else
                {
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public APIIsUserValid ValidateEmailId(string emailId, string OrgCode = null)
        {
            var regexofficialmailid = new Regex(Constants.RegularExpression.emailIdPattern);
            bool flag = true;
            string errorMessage = string.Empty;



            if (String.IsNullOrEmpty(emailId))
                errorMessage = "EmailId is  Required";
            else if (!regexofficialmailid.IsMatch(emailId))
            {
                //errorMessage = "Special Character Contains in Email ID, Record rejected";
                char specialChar = returnSpecialChar(emailId);
                errorMessage = String.Format("Special Character {0} contains in Email, Record rejected ", specialChar);
            }
            else if (hasSpecialChar(emailId, @"&='-/+,<>*"))
            {
                flag = true;
                // errorMessage = "Special Character Contains in Email ID, Record rejected ";
                char specialChar = returnSpecialChar(emailId);
                errorMessage = String.Format("Special Character {0} contains in Email, Record rejected ", specialChar);
            }
            else
                flag = false;

            if (OrgCode.ToLower() == "tatacapital" && string.IsNullOrEmpty(emailId))
            {
                flag = false;
                errorMessage = null;
            }
            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
        }
        public static bool hasSpecialChar(string input, string specialChar)
        {
            //string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) return true;
            }

            return false;
        }
        public static char returnSpecialChar(string input)
        {
            string specialChar = @"&='-/+,<>*_@1234567890";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) return item;
            }
            return ' ';
        }
        public APIIsUserValid ValidateUserName(string headerText, string userName, string allowUserNameValidation = "")
        {
            bool flag = true;
            string errorMessage = string.Empty;
            if (!string.IsNullOrEmpty(userName))
            {
                if (userName.Length <= 2)
                {
                    errorMessage = headerText + " must be more than 2 characters long.";
                }
                else
                {
                    if (String.Compare(allowUserNameValidation, "false", true) == 0)
                    {
                        var usernameNew = new Regex(Constants.RegularExpression.userNamePattern);
                        if (!usernameNew.IsMatch(userName))
                        { //errorMessage = "Special Character Contains in User Name, Record rejected"; 
                            char specialChar = returnSpecialChar(userName);
                            errorMessage = String.Format("Special Character {0} contains in User name, Record rejected ", specialChar);
                        }
                        else if (hasSpecialChar(userName, @"&='-/+,<>*_@"))
                        {
                            flag = true;
                            //errorMessage = "Special Character Contains in User Name, Record rejected ";
                            char specialChar = returnSpecialChar(userName);
                            errorMessage = String.Format("Special Character {0} contains in User Name, Record rejected ", specialChar);
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = false;
                }
            }
            else
                errorMessage = headerText + "Empty Field Message";

            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
        }
        public APIIsUserValid ValidateMobileNumber(string mobileNumber, string OrgCode)
        {
            var regexmbileno = new Regex(Constants.RegularExpression.mobileNumberPattern);
            bool flag = true;
            string errorMessage = string.Empty;

            if (String.IsNullOrEmpty(mobileNumber))
                errorMessage = "Mobile Number Required";
            else
            {
                if (mobileNumber.Length < 10)
                    errorMessage = "Mobile number not Valid";
                else if (String.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                {
                    string res = mobileNumber.Length > 10 ? mobileNumber.Substring(mobileNumber.Length - 10) : mobileNumber;
                    if (!regexmbileno.IsMatch(res))
                        errorMessage = "Enter Valid Mobile Number";
                    else
                        flag = false;
                }
                else
                {
                    if (!regexmbileno.IsMatch(mobileNumber))
                        errorMessage = "Enter Valid Mobile Number";
                    else
                        flag = false;
                }
            }

            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
        }
        public string ValidateDateOfBirth(string dateOfBirth, string validDateFormat)
        {
            string dtDateOfBirth = null;
            try
            {
                /* DateTime result;
                 result = DateTime.ParseExact(dateOfBirth, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                 string inputstring = result.ToString("dd/MM/yyyy");
                 inputstring = inputstring.Replace("-", "/");
                 inputstring = inputstring.Replace("-", "/");
                 inputstring = inputstring.Replace(".", "/");
                 string[] dateParts = inputstring.Split('/');
                 string day = dateParts[0];
                 string month = dateParts[1];
                 string year = dateParts[2];
                 dtDateOfBirth = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
             */
                if (string.IsNullOrEmpty(dateOfBirth))
                    return null;

                return Convert.ToDateTime(dateOfBirth, new CultureInfo("en-IN")).ToString("yyyy-MM-dd");
                // return Convert.ToDateTime(dateOfBirth, new CultureInfo("en-IN")).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public string ValidateDateOfJoining(string dateOfJoining, string validDateFormat)
        {
            DateTime dtDateOfJoin;

            try
            {
                /*DateTime result;
                result = DateTime.ParseExact(dateOfJoining, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                dtDateOfJoin = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                if (dtDateOfJoin != null && CheckPastTodayDate((DateTime)dtDateOfJoin))
                    return dtDateOfJoin.ToString("yyyy-MM-dd HH:mm:ss");
                */

                if (string.IsNullOrEmpty(dateOfJoining))
                    return null;
                // DateTime dateTime = Convert.ToDateTime(dateOfJoining);

                return Convert.ToDateTime(dateOfJoining, new CultureInfo("en-IN")).ToString("yyyy-MM-dd");
                //         return Convert.ToDateTime(dateOfJoining, new CultureInfo("en-IN")).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public string ValidateAccountExpiryDate(string accountExpoiryDate, string validDateFormat)
        {
            DateTime dtDateOfJoin;

            try
            {
                /* DateTime result;
                 result = DateTime.ParseExact(accountExpoiryDate, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                 string inputstring = result.ToString("dd/MM/yyyy");
                 inputstring = inputstring.Replace("-", "/");
                 inputstring = inputstring.Replace("-", "/");
                 inputstring = inputstring.Replace(".", "/");
                 string[] dateParts = inputstring.Split('/');
                 string day = dateParts[0];
                 string month = dateParts[1];
                 string year = dateParts[2];
                 dtDateOfJoin = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                 if (dtDateOfJoin != null)
                     return dtDateOfJoin.ToString("yyyy-MM-dd HH:mm:ss");
                */
                if (string.IsNullOrEmpty(accountExpoiryDate))
                    return null;
                return Convert.ToDateTime(accountExpoiryDate, new CultureInfo("en-IN")).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public string ValidateDateIntoRole(string dateIntoRole, string validDateFormat)
        {
            DateTime dtDateIntoRole;

            try
            {
                if (!string.IsNullOrEmpty(dateIntoRole))
                {
                    /* DateTime result;
                     result = DateTime.ParseExact(dateIntoRole, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                     string inputstring = result.ToString("dd/MM/yyyy");
                     inputstring = inputstring.Replace("-", "/");
                     inputstring = inputstring.Replace("-", "/");
                     inputstring = inputstring.Replace(".", "/");
                     string[] dateParts = inputstring.Split('/');
                     string day = dateParts[0];
                     string month = dateParts[1];
                     string year = dateParts[2];
                     dtDateIntoRole = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                     if (dtDateIntoRole != null && CheckPastTodayDate((DateTime)dtDateIntoRole))
                         return dtDateIntoRole.ToString("yyyy-MM-dd HH:mm:ss");
                    */
                    if (string.IsNullOrEmpty(dateIntoRole))
                        return null;
                    return Convert.ToDateTime(dateIntoRole, new CultureInfo("en-IN")).ToString("yyyy-MM-dd");
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public APIIsUserValid ValidateReportsTo(string headerText, string reportTo)
        {
            bool flag = true;
            string errorMessage = string.Empty;
            if (!string.IsNullOrEmpty(reportTo) && reportTo.Contains("@"))
            {
                var regexofficialmailid = new Regex(Constants.RegularExpression.emailIdPattern);
                if (!regexofficialmailid.IsMatch(reportTo))
                    errorMessage = "Please Enter Valid ReportsTo ";
                else
                    flag = false;
            }
            else
                flag = false;

            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
        }

        public bool CheckPastTodayDate(DateTime inputDate)
        {
            bool isValidDate = true;
            int result = DateTime.Compare(inputDate, DateTime.Now);
            if (result == 1)
                isValidDate = false;
            return isValidDate;
        }


        //public static void SendSMS_TableZ(string MobileNumber, string msg)
        //{
        //    try
        //    {

        //        string url = "http://httpapi.zone:7501/failsafe/HttpLink?aid=639809&pin=Surya123&mnumber=" + MobileNumber + "&signature=SSFBNK&message=" + msg;
        //        //create an HTTP request to the URL that we need to invoke 
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //        request.Method = "POST";
        //        // Get the response. 
        //        WebResponse response = request.GetResponse();
        //        var streamReader = new StreamReader(response.GetResponseStream());
        //        var result = streamReader.ReadToEnd();
        //        //Response.Write(result);
        //    }
        //    catch (Exception)
        //    {

        //    }
    }

}

    
