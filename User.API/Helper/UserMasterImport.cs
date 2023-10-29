using AutoMapper;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;

namespace User.API.Helper
{
    public class UserMasterImport
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserMasterImport));
        private StringBuilder sb = new StringBuilder();
        private string[] header = { };
        private string[] headerStar = { };
        private string[] headerWithoutStar = { };
        private List<string> employeeRecord = new List<string>();
        private APIUserMaster user = new APIUserMaster();
        private List<APIUserMaster> bulkUploadUserMaster = new List<APIUserMaster>();
        private UserMasterRejected userRejected = new UserMasterRejected();
        private UserRejectedStatus userRejectedStatus = new UserRejectedStatus();
        private APIUserRejectedStatus aPIUserStatus = new APIUserRejectedStatus();
        private StringBuilder sbError = new StringBuilder();
        private StringBuilder sbUpdated = new StringBuilder();
        private int totalRecordInsert = 0;
        private int totalRecordRejected = 0;
        private string url = null;
        private string TimeZone = null;
        private string Currency = null;
        private UserDbContext _db;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identitySv;

        public UserMasterImport(UserDbContext context, IUserRepository userRepository,
                                ICustomerConnectionStringRepository customerConnectionStringRepository,
                                IConfiguration configuration, IIdentityService identityService)
        {
            _db = context;
            _userRepository = userRepository;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _identitySv = identityService;
        }
        public void Reset()
        {
            sb.Clear();
            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            employeeRecord.Clear();
            user = new APIUserMaster();
            userRejected = new UserMasterRejected();
            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
            url = null;
        }

        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    dt.Columns.Add(firstRowCell.Text);
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var isRowEmpty = wsRow.All(c => c.Value == null);
                    if (isRowEmpty == false)
                    {
                        if (wsRow.ElementAt(0).Text != null && wsRow.ElementAt(0).Text != string.Empty)
                        {
                            DataRow row = dt.Rows.Add();
                            foreach (var cell in wsRow)
                            {
                                row[cell.Start.Column - 1] = cell.Text;
                            }
                        }
                        else
                            break;
                    }
                }
            }

            return dt;
        }

        public async Task<APIIsFileValid> ValidateFileColumnHeaders(DataTable userImportdt, string OrgCode, List<APIUserSetting> aPIUserSettings)
        {

            string[] configvalue = { "HRBP_Rename", "Mentor_Rename", "Companion_Rename" };
            List<AppConfiguration> renameConfigValues = await _userRepository.GetUserConfigurationValueAsync(configvalue, OrgCode, "");
           

            List<string> importColumns = GetUserMasterImportColumns(OrgCode);
            List<string> validColumnsList = new List<string>();
            if (OrgCode.ToLower() == "tcl")
            {
                validColumnsList = GetMandatoryColumnsWithoutMobile();
            }
            else if (OrgCode.ToLower() == "bandhanbank" || OrgCode.ToLower() == "bandhan" )
            {
                validColumnsList = GetMandatoryColumnsWithoutEmail();
            }
            else
            {
                validColumnsList = GetMandatoryColumns();
            }
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

                APIUserSetting aPIUserSetting = aPIUserSettings.Where(x => x.ChangedColumnName.ToLower() == col.ToLower()).FirstOrDefault();

                AppConfiguration renameColumns = renameConfigValues.Where(x => x.value.ToLower() == col.ToLower()).FirstOrDefault();

                if (aPIUserSetting != null)
                    tempDt.Columns.Add(aPIUserSetting.ConfiguredColumnName, typeof(string));

                else if (renameColumns != null)
                { 
                 if(renameColumns.Code== "Companion_Rename")
                        tempDt.Columns.Add(UserMasterImportField.Companion, typeof(string));
                 else if (renameColumns.Code == "Mentor_Rename")
                        tempDt.Columns.Add(UserMasterImportField.Mentor, typeof(string));
                    else if (renameColumns.Code == "HRBP_Rename")
                        tempDt.Columns.Add(UserMasterImportField.HRBP, typeof(string));
                }
                else
                    tempDt.Columns.Add(userImportdt.Columns[i].ColumnName, typeof(string));

                if (!importColumns.Contains(tempDt.Columns[i].ColumnName))
                    return new APIIsFileValid { Flag = false, userImportdt = userImportdt };
                if (!importColumns.Contains(tempDt.Columns[i].ColumnName))
                {
                    aPIIsFileValid.Flag = false;
                    return aPIIsFileValid;
                }
            }
            for (int i = 0; i < userImportdt.Rows.Count; i++)
            {
                DataRow dr = tempDt.NewRow();
                for (int j = 0; j < userImportdt.Columns.Count; j++)
                {                    
                    dr[j] = userImportdt.Rows[i][j];
                }
                tempDt.Rows.Add(dr);
            }
            return new APIIsFileValid { Flag = true, userImportdt = tempDt };

        }
        private List<string> GetMandatoryColumnsWithoutMobile()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId);
            columns.Add(UserMasterImportField.EmailId);
            columns.Add(UserMasterImportField.UserName);
            return columns;
        }
        private List<string> GetMandatoryColumnsWithoutEmail()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId);
            columns.Add(UserMasterImportField.MobileNumber);
            columns.Add(UserMasterImportField.UserName);
            return columns;
        }
        private List<string> GetMandatoryColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId);
            columns.Add(UserMasterImportField.EmailId);
            columns.Add(UserMasterImportField.UserName);
            columns.Add(UserMasterImportField.MobileNumber);
            return columns;
        }


        private List<string> GetUserMasterImportColumns(string OrgCode = null)
        {

           
            List<string> columns = new List<string>();
            columns.AddRange(GetMandatoryColumns());
            columns.Add(UserMasterImportField.Business);
            columns.Add(UserMasterImportField.Group);
            columns.Add(UserMasterImportField.Area);
            columns.Add(UserMasterImportField.Location);
            columns.Add(UserMasterImportField.ConfigurationColumn1);
            columns.Add(UserMasterImportField.ConfigurationColumn2);
            columns.Add(UserMasterImportField.ConfigurationColumn3);
            columns.Add(UserMasterImportField.ConfigurationColumn4);
            columns.Add(UserMasterImportField.ConfigurationColumn5);
            columns.Add(UserMasterImportField.ConfigurationColumn6);
            columns.Add(UserMasterImportField.ConfigurationColumn7);
            columns.Add(UserMasterImportField.ConfigurationColumn8);
            columns.Add(UserMasterImportField.ConfigurationColumn9);
            columns.Add(UserMasterImportField.ConfigurationColumn10);
            columns.Add(UserMasterImportField.ConfigurationColumn11);
            columns.Add(UserMasterImportField.ConfigurationColumn12);
            if (OrgCode == null || String.Compare(OrgCode, "ujjivan", true) == 0 || String.Compare(OrgCode, "WNS", true) == 0)
            {
                columns.Add(UserMasterImportField.ConfigurationColumn13);
                columns.Add(UserMasterImportField.ConfigurationColumn14);
                columns.Add(UserMasterImportField.ConfigurationColumn15);
            }
            columns.Add(UserMasterImportField.Language);
            columns.Add(UserMasterImportField.Currency);
            columns.Add(UserMasterImportField.DateOfBirth);
            columns.Add(UserMasterImportField.DateOfJoining);
            columns.Add(UserMasterImportField.AccountExpiryDate);
            columns.Add(UserMasterImportField.Gender);
            columns.Add(UserMasterImportField.ReportsTo);
            columns.Add(UserMasterImportField.UserType);
            columns.Add(UserMasterImportField.UserRole);
            columns.Add(UserMasterImportField.JobRole);
            columns.Add(UserMasterImportField.DateIntoRole);
            columns.Add(UserMasterImportField.Companion);
            columns.Add(UserMasterImportField.Mentor);
            columns.Add(UserMasterImportField.HRBP);
            return columns;
        }

        private List<string> GetUserMasterColumnsForSQLInjection(string OrgCode)
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId.ToLower());
            columns.Add(UserMasterImportField.UserName.ToLower());
            columns.Add(UserMasterImportField.EmailId.ToLower());
            columns.Add(UserMasterImportField.MobileNumber.ToLower());
            columns.Add(UserMasterImportField.Business.ToLower());
            columns.Add(UserMasterImportField.Group.ToLower());
            columns.Add(UserMasterImportField.Area.ToLower());
            columns.Add(UserMasterImportField.Location.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn12.ToLower());
            if (String.Compare(OrgCode, "ujjivan", true) == 0)
            {
                columns.Add(UserMasterImportField.ConfigurationColumn13.ToLower());
                columns.Add(UserMasterImportField.ConfigurationColumn14.ToLower());
                columns.Add(UserMasterImportField.ConfigurationColumn15.ToLower());
            }
            columns.Add(UserMasterImportField.Language.ToLower());
            columns.Add(UserMasterImportField.Currency.ToLower());
            columns.Add(UserMasterImportField.Gender.ToLower());
            columns.Add(UserMasterImportField.UserType.ToLower());
            columns.Add(UserMasterImportField.UserRole.ToLower());
            columns.Add(UserMasterImportField.JobRole.ToLower());
            return columns;
        }

        private List<string> GetUserMasterEncryptedImportColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId.ToLower());
            columns.Add(UserMasterImportField.EmailId.ToLower());
            columns.Add(UserMasterImportField.MobileNumber.ToLower());
            columns.Add(UserMasterImportField.ReportsTo.ToLower());

            columns.AddRange(GetConfigurationColumns());
            return columns;
        }

        private List<string> GetConfigurationColumns()
        {
            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.Business.ToLower());
            columns.Add(UserMasterImportField.Group.ToLower());
            columns.Add(UserMasterImportField.Area.ToLower());
            columns.Add(UserMasterImportField.Location.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn12.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn13.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn14.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn15.ToLower());
            return columns;
        }

        private List<string> GetColumnsForLowerCase()
        {
            List<string> columns = new List<string>();
            columns.AddRange(GetConfigurationColumns());
            columns.Add(UserMasterImportField.JobRole.ToLower());
            return columns;
        }

        private List<KeyValuePair<string, int>> GetUserMasterColumnDataLength(string OrgCode, List<AppConfiguration> renameConfigValues)
        {
            string companion = renameConfigValues.Where(a => a.Code == "Companion_Rename").Select(a => a.value).FirstOrDefault();
            string mentor = renameConfigValues.Where(a => a.Code == "Mentor_Rename").Select(a => a.value).FirstOrDefault();
            string hrbp = renameConfigValues.Where(a => a.Code == "HRBP_Rename").Select(a => a.value).FirstOrDefault();

            var list = new List<KeyValuePair<string, int>>() {
             new KeyValuePair<string, int>(UserMasterImportField.UserId,100),
             new KeyValuePair<string, int>(UserMasterImportField.EmailId,100),
             new KeyValuePair<string, int>(UserMasterImportField.UserName,200),
             new KeyValuePair<string, int>(UserMasterImportField.MobileNumber,20),
             new KeyValuePair<string, int>(UserMasterImportField.Business,200),
             new KeyValuePair<string, int>(UserMasterImportField.Group,200),
             new KeyValuePair<string, int>(UserMasterImportField.Area,200),
            new KeyValuePair<string, int>(UserMasterImportField.Location,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn1,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn2,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn3,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn4,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn5,200),
            //new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn6,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn7,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn8,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn9,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn10,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn11,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn12,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn13,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn14,200),
            new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn15,200),
            new KeyValuePair<string, int>(UserMasterImportField.Language,50),
            new KeyValuePair<string, int>(UserMasterImportField.Currency,50),
            new KeyValuePair<string, int>(UserMasterImportField.Gender,10),
            new KeyValuePair<string, int>(UserMasterImportField.ReportsTo,100),
            new KeyValuePair<string, int>(UserMasterImportField.UserType,10),
            new KeyValuePair<string, int>(UserMasterImportField.UserRole,50),
            new KeyValuePair<string, int>(UserMasterImportField.JobRole,50),
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
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn3Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn4Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn5Encrypted,200),
            //new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn6Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn7Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn8Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn9Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn10Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn11Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn12Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn13Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn14Encrypted,200),
            new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn15Encrypted,200),

            new KeyValuePair<string, int>(companion,200),
            new KeyValuePair<string, int>(mentor,200),
            new KeyValuePair<string, int>(hrbp,200)




            };

            if (string.Equals(OrgCode,"cpcl",StringComparison.InvariantCultureIgnoreCase))
            {
                list.Add(new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn6, 2000));
                list.Add(new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn6Encrypted, 2000));
            }
            else
            {
                list.Add(new KeyValuePair<string, int>(UserMasterImportField.ConfigurationColumn6, 200));
                list.Add(new KeyValuePair<string, int>(UserMasterImportFieldEncrypted.ConfigurationColumn6Encrypted, 200));
            }
            return list;
        }

        private List<string> GetUserMasterImportColumnsForLength(List<AppConfiguration> renameConfigValues)
        {
            string companion = renameConfigValues.Where(a => a.Code == "Companion_Rename").Select(a => a.value).FirstOrDefault();
            string mentor = renameConfigValues.Where(a => a.Code == "Mentor_Rename").Select(a => a.value).FirstOrDefault();
            string hrbp = renameConfigValues.Where(a => a.Code == "HRBP_Rename").Select(a => a.value).FirstOrDefault();

            List<string> columns = new List<string>();
            columns.Add(UserMasterImportField.UserId.ToLower());
            columns.Add(UserMasterImportField.EmailId.ToLower());
            columns.Add(UserMasterImportField.UserName.ToLower());
            columns.Add(UserMasterImportField.MobileNumber.ToLower());
            columns.Add(UserMasterImportField.Business.ToLower());
            columns.Add(UserMasterImportField.Group.ToLower());
            columns.Add(UserMasterImportField.Area.ToLower());
            columns.Add(UserMasterImportField.Location.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn1.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn2.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn3.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn4.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn5.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn6.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn7.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn8.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn9.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn10.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn11.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn12.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn13.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn14.ToLower());
            columns.Add(UserMasterImportField.ConfigurationColumn15.ToLower());
            columns.Add(UserMasterImportField.Language.ToLower());
            columns.Add(UserMasterImportField.Currency.ToLower());
            columns.Add(UserMasterImportField.Gender.ToLower());
            columns.Add(UserMasterImportField.ReportsTo.ToLower());
            columns.Add(UserMasterImportField.UserType.ToLower());
            columns.Add(UserMasterImportField.UserRole.ToLower());
            columns.Add(UserMasterImportField.JobRole.ToLower());

            columns.Add(companion.ToLower());
            columns.Add(mentor.ToLower());
            columns.Add(hrbp.ToLower());
            return columns;
        }


        public async Task<string> ProcessRecordsAsync(int UserId, DataTable userImportdt, string OrgCode, List<APIUserSetting> aPIUserSettings, string RandomPassword)
        {
            int totalInserted = 0, totalUpdated = 0, totalRejected = 0;
            bool Ldap = await _userRepository.IsLDAP();
            string DeafultPassword = RandomPassword;
            //string DeafultPassword = Helper.Security.EncryptSHA512(RandomPassword);
            string allowSpecialCharInUserName = await _userRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode);
            var lowerCaseAllow = await _userRepository.GetConfigurationValueAsync("SAVE_USER_IMPORT_ASIS", OrgCode);
            var applicationDateFormat = await _userRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
            string[] configvalue = { "HRBP_Rename", "Mentor_Rename", "Companion_Rename" };
            List<AppConfiguration> renameConfigValues = await _userRepository.GetUserConfigurationValueAsync(configvalue, OrgCode, "");


            List<string> allcolumns = this.GetUserMasterImportColumns();
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
            userImportdt.Columns.Add("IsValid", typeof(bool));
            userImportdt.Columns.Add("ErrorMessage", typeof(string));
            List<string> sQLInjectionColumns = this.GetUserMasterColumnsForSQLInjection(OrgCode);
            _logger.Debug("Table Processing Started at: " + DateTime.Now);
            int totalrecords = userImportdt.Rows.Count;

            int totalRecordsSplitCount = totalrecords > 100 ? totalrecords / 10 : 1;
            DataTable[] processedDataTables = null;
            if (totalRecordsSplitCount == 1)
            {
                Task<DataTable> task = Task.Run(() => ProcessInputRecordsAsync(userImportdt, allcolumns, sQLInjectionColumns, encryptedcolumns, OrgCode, allowSpecialCharInUserName, lowerCaseAllow, lowerCaseColumns, applicationDateFormat, aPIUserSettings,  renameConfigValues));
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
                    tasks[i] = Task.Run(() => ProcessInputRecordsAsync(tempDt, allcolumns, sQLInjectionColumns, encryptedcolumns, OrgCode, allowSpecialCharInUserName, lowerCaseAllow, lowerCaseColumns, applicationDateFormat, aPIUserSettings, renameConfigValues));
                }

                try
                {
                    processedDataTables = await Task.WhenAll(tasks);
                }
                catch (AggregateException ae)
                {
                    _logger.Error(Utilities.GetDetailedException(ae));
                }
            }

            _logger.Debug("Total processedDataTables :-" + processedDataTables.Length);

            DataTable finalDt = new DataTable();
            foreach (DataTable dt in processedDataTables)
                finalDt.Merge(dt);

            finalDt = setExplicitPassword(finalDt, DeafultPassword, OrgCode);

            _logger.Debug("Table Processing Finished at: " + DateTime.Now);
            _logger.Debug("Total records in Merge Datatable :-" + finalDt.Rows.Count);
            try
            {
                DataTable dtResult = new DataTable();
                using (var dbContext = _customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[UserMaster_BulkImport]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                        cmd.Parameters.Add(new SqlParameter("@IsLDAP", SqlDbType.Bit) { Value = Ldap });
                        cmd.Parameters.Add(new SqlParameter("@DefaultPassword", SqlDbType.VarChar) { Value = DeafultPassword });
                        cmd.Parameters.Add(new SqlParameter("@UserImportType", SqlDbType.Structured) { Value = finalDt });

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
                    _logger.Debug("Stored Procedure Finished at: " + DateTime.Now);
                }

                #region sendNotificationCode
                if (dtResult!=null && dtResult.Rows.Count > 0)
                {                   
                    var SendSMSToUser = await _userRepository.GetConfigurationValueAsync("SMS_USER_CREATED", OrgCode);
                    var SendEmailSMSOnUserUpdation = await _userRepository.GetConfigurationValueAsync("USER_UPDATION_SMS_EMAIL", OrgCode);
                    var SendApplicableEmail = await _userRepository.GetConfigurationValueAsync("APP_COURSE_NEWSIGNUPUSER", OrgCode);

                    bool sendSMSToUserFlag = string.Compare(SendSMSToUser, "yes", true) == 0 ? true : false;
                    bool sendEmailSmsOnUserUpdationFlag = string.Compare(SendEmailSMSOnUserUpdation, "yes", true) == 0 ? true : false;
                    bool sendApplicableEmailFlag = string.Compare(SendApplicableEmail, "yes", true) == 0 ? true : false;
                    bool isSbilOrIoclFlag = string.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase) || String.Equals(OrgCode, "iocl", StringComparison.CurrentCultureIgnoreCase) ? true : false;

                    string notificationBaseUrl = _configuration[Configuration.NotificationApi];
                    int chunkSize = 1000;
                    double countInserted = Math.Ceiling((double)dtResult.AsEnumerable().Where(c => c.Field<bool>("IsExists") == false).Count() / chunkSize);
                    for (int i = 0; i < countInserted; i++)
                    {
                        List<APINewUserActivationEmail> aPINewUserList = dtResult.AsEnumerable().Where(c => c.Field<bool>("IsExists") == false).Skip(i * chunkSize).Take(chunkSize).Select(d => new APINewUserActivationEmail
                        {
                            Id = d.Field<int>("UserMasterId"),
                            UserId = d.Field<string>("UserId"),
                            UserName = d.Field<string>("UserName"),
                            EmailId = d.Field<string>("EmailId"),
                            MobileNumber = d.Field<string>("MobileNumber"),
                            ReportsTo = d.Field<string>("ReportsTo"),
                            Password = d.Field<string>("Password") //RandomPassword
                        }).ToList();

                        UserImportBulk userImportBulk = new UserImportBulk
                        {
                            NewUserImport = aPINewUserList,
                            orgCode = OrgCode,
                            loggedInUserId = UserId
                        };
                        if ((userImportBulk.orgCode.ToLower() == "bandhanbank" || userImportBulk.orgCode.ToLower() == "bandhan" )&& sendSMSToUserFlag)
                        {
                            string smsNotificationUrl = notificationBaseUrl + "/UserActivationSMSBulk";
                            CallPostAPI(smsNotificationUrl, JsonConvert.SerializeObject(userImportBulk));
                        }

                        if (isSbilOrIoclFlag && sendSMSToUserFlag)
                        {
                            string smsNotificationUrl = notificationBaseUrl + "/UserActivationSMSBulk";
                            CallPostAPI(smsNotificationUrl, JsonConvert.SerializeObject(userImportBulk));
                        }

                        string newUserEmailNotificationUrl = notificationBaseUrl + "/NewUserAddedBulk";
                        CallPostAPI(newUserEmailNotificationUrl, JsonConvert.SerializeObject(userImportBulk));

                        int reportsToUsersCount = aPINewUserList.Where(c => c.ReportsTo != "").ToList().Count;
                        if (reportsToUsersCount > 0)
                        {
                            UserImportBulk userImportBulkNotificationToManagerUrl = new UserImportBulk
                            {
                                NewUserImport = aPINewUserList.Where(c => c.ReportsTo != "").ToList(),
                                orgCode = OrgCode,
                                loggedInUserId = UserId
                            };
                            string emailNotificationToManagerUrl = notificationBaseUrl + "/UserSignUpMailToManagerBulk";
                            CallPostAPI(emailNotificationToManagerUrl, JsonConvert.SerializeObject(userImportBulkNotificationToManagerUrl));
                        }

                        if (sendApplicableEmailFlag)
                        {
                            string applicableNotificationUrl = notificationBaseUrl + "/SendApplicableCoursesEmailBulk";
                            CallPostAPI(applicableNotificationUrl, JsonConvert.SerializeObject(userImportBulk));
                        }
                    }
                    double countUpdated = Math.Ceiling((double)dtResult.AsEnumerable().Where(c => c.Field<bool>("IsExists") == true).Count() / chunkSize);
                    for (int i = 0; i < countUpdated; i++)
                    {
                        List<APINewUserActivationEmail> aPIUpdatedUserList = dtResult.AsEnumerable().Where(c => c.Field<bool>("IsExists") == true).Skip(i * chunkSize).Take(chunkSize).Select(d => new APINewUserActivationEmail
                        {
                            Id = d.Field<int>("UserMasterId"),
                            UserId = d.Field<string>("UserId"),
                            UserName = d.Field<string>("UserName"),
                            EmailId = d.Field<string>("EmailId"),
                            MobileNumber = d.Field<string>("MobileNumber"),
                            ReportsTo = d.Field<string>("ReportsTo"),
                            Password = d.Field<string>("Password")//RandomPassword
                        }).ToList();

                        UserImportBulk userImportBulk = new UserImportBulk
                        {
                            NewUserImport = aPIUpdatedUserList,
                            orgCode = OrgCode,
                            loggedInUserId = UserId
                        };

                        if (sendSMSToUserFlag && sendEmailSmsOnUserUpdationFlag)
                        {
                            string smsNotificationUrl = notificationBaseUrl + "/UserActivationSMSBulk";
                            HttpResponseMessage httpResponseMessage = CallPostAPI(smsNotificationUrl, JsonConvert.SerializeObject(userImportBulk)).Result;
                        }

                        if (sendEmailSmsOnUserUpdationFlag)
                        {
                            string newUserEmailNotificationUrl = notificationBaseUrl + "/NewUserAddedBulk";
                            CallPostAPI(newUserEmailNotificationUrl, JsonConvert.SerializeObject(userImportBulk));

                            int reportsToUsersCount = aPIUpdatedUserList.Where(c => c.ReportsTo != "").ToList().Count;
                            if (reportsToUsersCount > 0)
                            {
                                UserImportBulk userImportBulkNotificationToManagerUrl = new UserImportBulk
                                {
                                    NewUserImport = aPIUpdatedUserList.Where(c => c.ReportsTo != "").ToList(),
                                    orgCode = OrgCode,
                                    loggedInUserId = UserId
                                };
                                string emailNotificationToManagerUrl = notificationBaseUrl + "/UserSignUpMailToManagerBulk";
                                CallPostAPI(emailNotificationToManagerUrl, JsonConvert.SerializeObject(userImportBulkNotificationToManagerUrl));
                            }
                        }
                    }
                }
                _logger.Debug("SMS/Email Finished at: " + DateTime.Now);
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return Record.TotalRecordInserted + totalInserted + Record.TotalRecordUpdated + totalUpdated + Record.TotalRecordRejected + totalRejected;
        }


        DataTable setExplicitPassword(DataTable passwordtable , string DefaultPassword,string OrgCode)
        {
            string encryptedPwdColumn = UserMasterImportField.EncryptedPassword;
            DataColumn dc = new DataColumn(encryptedPwdColumn);
            dc.DefaultValue = DBNull.Value;
            passwordtable.Columns.Add(dc);

            string pwdColumn = UserMasterImportField.Password;
            dc = new DataColumn(pwdColumn);
            //DataColumn column = new DataColumn(Password);
            //column.DefaultValue = DBNull.Value;
            dc.DefaultValue = DBNull.Value;
            passwordtable.Columns.Add(dc);

            string encryptedPwd = Security.EncryptSHA512(DefaultPassword);
            string ErrorMessage = "Birth Date is required to Genearte Password";

            if (OrgCode == "bandhan" || OrgCode == "bandhanbank" )
            {
                foreach (DataRow row in passwordtable.Rows)
                {
                    if (!string.IsNullOrEmpty(row["DateofBirth"].ToString()))
                    {
                        row[pwdColumn] = Convert.ToDateTime(row["DateofBirth"].ToString()).ToString("MMyyyy");
                        row[encryptedPwdColumn] = Security.EncryptSHA512(Convert.ToDateTime(row["DateofBirth"].ToString()).ToString("MMyyyy"));
                    }
                    else
                    {
                        row["ErrorMessage"] = ErrorMessage;
                        row["IsValid"] = false;
                    }
                }
            }
            else
            {   //For other Organizations.    
                foreach (DataRow row in passwordtable.Rows)
                {
                    row[pwdColumn] = DefaultPassword;
                    row[encryptedPwdColumn] = encryptedPwd;
                }
            }
            return passwordtable;
        }

        private async Task<DataTable> ProcessInputRecordsAsync(DataTable processedDataTable, List<string> allColumns, List<string> SQLInjectionColumns, List<string> encryptedcolumns, string OrgCode, string userNameValidation, string lowerCaseAllow, List<string> lowerCaseColumns, string validDateFormat, List<APIUserSetting> aPIUserSettings, List<AppConfiguration> renameConfigValues)
        {
            //To Do - Need to check for threading large records
            DataTable userImportdt = processedDataTable.Copy();
            var columnValueLength = GetUserMasterColumnDataLength(OrgCode, renameConfigValues);
            List<string> columnsForLength = GetUserMasterImportColumnsForLength( renameConfigValues);
            try
            {
                foreach (DataRow row in userImportdt.Rows)
                {
                    APIIsUserValid aPIIsUserValid = new APIIsUserValid
                    {
                        Flag = false,
                        ErrorMessage = null
                    };
                    foreach (string currentHeader in allColumns)
                    {
                        string celldata = Convert.ToString(row[currentHeader]).Trim();
                        row[currentHeader] = celldata;

                        if (SQLInjectionColumns.Contains(currentHeader.ToLower()))
                        {
                            if (FileValidation.CheckForSQLInjection(celldata))
                            {
                                aPIIsUserValid.Flag = true;
                                aPIIsUserValid.ErrorMessage = "Enter valid data";
                                break;
                            }
                        }

                        switch (currentHeader)
                        {
                            case UserMasterImportField.UserId:
                                row[currentHeader] = celldata.ToLower();
                                aPIIsUserValid = ValidateUserIdNew(currentHeader, celldata.ToLower());
                                break;
                            case UserMasterImportField.EmailId:
                                row[currentHeader] = celldata.ToLower();
                                if (OrgCode != "bandhan" && OrgCode != "bandhanbank" )
                                {
                                    aPIIsUserValid = ValidateEmailId(celldata.ToLower());
                                }
                                break;
                            case UserMasterImportField.UserName:
                                aPIIsUserValid = ValidateUserName(currentHeader, celldata, userNameValidation);
                                break;
                            case UserMasterImportField.MobileNumber:
                                if(OrgCode.ToLower() != "tcl")
                                {
                                    aPIIsUserValid = ValidateMobileNumber(celldata, OrgCode);
                                }                                
                                break;
                            case UserMasterImportField.ReportsTo:
                                row[currentHeader] = celldata.ToLower();
                                aPIIsUserValid = ValidateReportsTo(currentHeader, celldata);
                                break;
                            case UserMasterImportField.DateOfBirth:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateOfBirth = this.ValidateDateOfBirth(celldata, validDateFormat);
                                    if (DateOfBirth == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = Record.PleaseEnterValid + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateOfBirth;
                                }
                                else
                                {
                                    if (OrgCode == "bandhan" || OrgCode == "bandhanbank")
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = Record.DateOfBirthRequired;
                                        row[currentHeader] = celldata;
                                    }
                                }
                                break;
                            case UserMasterImportField.DateOfJoining:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateOfJoining = this.ValidateDateOfJoining(celldata, validDateFormat);
                                    if (DateOfJoining == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = Record.PleaseEnterValid + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateOfJoining;
                                }
                                break;
                            case UserMasterImportField.AccountExpiryDate:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string AccountExpiryDate = this.ValidateAccountExpiryDate(celldata, validDateFormat);
                                    if (AccountExpiryDate == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = Record.PleaseEnterValid + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = AccountExpiryDate;
                                }
                                break;
                            case UserMasterImportField.DateIntoRole:
                                if (!String.IsNullOrEmpty(celldata))
                                {
                                    string DateIntoRole = this.ValidateDateIntoRole(celldata, validDateFormat);
                                    if (DateIntoRole == null)
                                    {
                                        aPIIsUserValid.Flag = true;
                                        aPIIsUserValid.ErrorMessage = Record.PleaseEnterValid + currentHeader;
                                        row[currentHeader] = celldata;
                                    }
                                    else
                                        row[currentHeader] = DateIntoRole;
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
                                aPIIsUserValid.ErrorMessage = Record.PleaseEnterValidLengthData + changedHeaderName;
                                break;
                            }

                            if (encryptedcolumns.Contains(currentHeader.ToLower()) && !string.IsNullOrEmpty(Convert.ToString(row[currentHeader])))
                            {
                                currentHeaderdata = Security.Encrypt(row[currentHeader].ToString());
                                if (currentHeaderdata.Length > Convert.ToInt32(columnValueLength.Where(h => h.Key == currentHeader + "Encrypted").Select(h => h.Value).FirstOrDefault()))
                                {
                                    aPIIsUserValid.Flag = true;
                                    aPIIsUserValid.ErrorMessage = Record.PleaseEnterValidLengthData + changedHeaderName; break;
                                }
                            }
                        }

                        if (!aPIIsUserValid.Flag)
                        {
                            if (lowerCaseColumns.Contains(currentHeader.ToLower()) && lowerCaseAllow == "Yes")
                                row[currentHeader] = Convert.ToString(row[currentHeader]).ToLower();

                            if (currentHeader.ToLower() =="companion" || currentHeader.ToLower() == "mentor"|| currentHeader.ToLower() == "hrbp")
                                row[currentHeader] = Security.Encrypt(row[currentHeader].ToString()); 


                            if (encryptedcolumns.Contains(currentHeader.ToLower()) && !string.IsNullOrEmpty(Convert.ToString(row[currentHeader])))
                                row[currentHeader + "Encrypted"] = Security.Encrypt(row[currentHeader].ToString());
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
                _logger.Error(Utilities.GetDetailedException(ex));
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

        public async Task<HttpResponseMessage> CallPostAPI(string url, string body)
        {
            using (var client = new HttpClient())
            {
                string token = _identitySv.GetToken();
                token = token.Replace("Bearer ", "");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                return response;
            }
        }

        public async Task<HttpResponseMessage> CallAPI(string url, string body)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                return response;
            }
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
                    errorMessage = Record.EnterValidUserId;
                else
                    flag = false;
            }
            else
                errorMessage = headerText + Record.EmptyFieldMessage;

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
                        user.UserId = userId;
                        userRejected.UserId = userId;
                    }
                    else
                    {
                        user.UserId = userId;
                        userRejected.UserId = userId;
                        sbError.Append(Record.EnterValidUserId);
                        valid = true;
                    }

                }
                else
                {
                    sbError.Append(headerText + Record.EmptyFieldMessage);
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }

        public APIIsUserValid ValidateEmailId(string emailId)
        {
            var regexofficialmailid = new Regex(Constants.RegularExpression.emailIdPattern);
            bool flag = true;
            string errorMessage = string.Empty;
            if (String.IsNullOrEmpty(emailId))
                errorMessage = Record.EmailIdRequired;
            else if (!regexofficialmailid.IsMatch(emailId))
                errorMessage = Record.EnterValidEmailId;
            else
                flag = false;

            return new APIIsUserValid
            {
                Flag = flag,
                ErrorMessage = errorMessage
            };
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
                    if (String.Compare(allowUserNameValidation, "yes", true) != 0)
                    {
                        var usernameNew = new Regex(Constants.RegularExpression.userNamePattern);
                        if (!usernameNew.IsMatch(userName))
                            errorMessage = Record.EnterValidUserName;
                        else
                            flag = false;
                    }
                    else
                        flag = false;
                }
            }
            else
                errorMessage = headerText + Record.EmptyFieldMessage;

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
                errorMessage = Record.MobileNumberRequired;
            else
            {
                //if (mobileNumber.Length < 10)
                //    errorMessage = Record.Mobilenonotexists;
                if (String.Equals(OrgCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                {
                    string res = mobileNumber.Length > 10 ? mobileNumber.Substring(mobileNumber.Length - 10) : mobileNumber;
                    if (!regexmbileno.IsMatch(res))
                        errorMessage = Record.EnterValidMobileNumber;
                    else
                        flag = false;
                }
                else
                {
                    if (!regexmbileno.IsMatch(mobileNumber))
                        errorMessage = Record.EnterValidMobileNumber;
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
                DateTime result;
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return dtDateOfBirth;
        }

        public string ValidateDateOfJoining(string dateOfJoining, string validDateFormat)
        {
            DateTime dtDateOfJoin;

            try
            {
                DateTime result;
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

                if (dtDateOfJoin != null && Helper.EnumHelper.CheckPastTodayDate((DateTime)dtDateOfJoin))
                    return dtDateOfJoin.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public string ValidateAccountExpiryDate(string accountExpoiryDate, string validDateFormat)
        {
            DateTime dtDateOfJoin;

            try
            {
                DateTime result;
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
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
                    DateTime result;
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

                    if (dtDateIntoRole != null && Helper.EnumHelper.CheckPastTodayDate((DateTime)dtDateIntoRole))
                        return dtDateIntoRole.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public APIIsUserValid ValidateReportsTo(string headerText, string reportTo)
        {
            bool flag = true;
            string errorMessage = string.Empty;

            if (!string.IsNullOrEmpty(reportTo))
            {
                var regexofficialmailid = new Regex(Constants.RegularExpression.emailIdPattern);
                if (!regexofficialmailid.IsMatch(reportTo))
                    errorMessage = Record.PleaseEnterValid + headerText;
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

        public async Task<bool> InitilizeStatusAsync(FileInfo file, IUserSettingsRepository _userSettingsRepository)
        {
            bool result = false;
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
                employeeRecord = new List<string>(fileInfo.Split('\n'));
                foreach (string record in employeeRecord)
                {
                    string[] mainsp = record.Split('\r');
                    string[] mainsp2 = mainsp[0].Split('\"');
                    header = mainsp2[0].Split('\t');
                    headerStar = mainsp2[0].Split('\t');
                    headerWithoutStar = mainsp2[0].Split('\t');
                    break;
                }
                employeeRecord.RemoveAt(0);


            }
            /////Remove Star from Header
            for (int i = 0; i < header.Count(); i++)
            {
                header[i] = header[i].Replace(Record.Star, "");
                headerWithoutStar[i] = headerWithoutStar[i].Replace(Record.Star, "");

            }

            int count = 0;
            for (int i = 0; i < header.Count() - 1; i++)
            {
                string headerColumn = header[i].ToString().Trim();
                if (!string.IsNullOrEmpty(headerColumn))
                {
                    count++;
                }

            }


            if (count == 2)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public async Task<string> ProcessRecordAsync(FileInfo file, IUserSettingsRepository _userSettingsRepository, IUserRepository _userRepository, IUserMasterRejectedRepository _userMasterRejectedRepository, IConfiguration _configuration, ICustomerConnectionStringRepository _customerConnectionStringRepository)
        {
            try
            {
                if (employeeRecord != null && employeeRecord.Count > 0)
                {
                    foreach (string record in employeeRecord)
                    {

                        int countLenght = record.Length;
                        if (record != null && countLenght > 1)
                        {
                            string[] textpart = record.Split('\t');
                            string[][] mainRecord = { header, textpart };
                            string txtUserId = string.Empty;
                            string txtStatus = string.Empty;


                            string headerText = "";

                            int arrasize = header.Count();

                            for (int j = 0; j < arrasize; j++)
                            {
                                headerText = header[j];
                                string[] mainspilt = headerText.Split('\t');

                                headerText = mainspilt[0];

                                if (headerText == UserMasterImportField.UserId)
                                {
                                    try
                                    {

                                        string status = mainRecord[1][j];
                                        string[] textstatus = mainRecord[1][j].Split('\t');
                                        txtUserId = textstatus[0];
                                        string ID = txtUserId.Trim();

                                        bool valid = ValidateUserId(headerText, ID);
                                        if (valid == true)
                                        {
                                            break;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }


                                if (headerText == UserMasterImportField.Status)
                                {
                                    try
                                    {

                                        string status = mainRecord[1][j];
                                        string[] textstatus = mainRecord[1][j].Split('\t');
                                        txtStatus = textstatus[0];
                                        txtStatus = txtStatus.Trim();
                                        if (txtStatus.ToLower() == "true" || txtStatus.ToLower() == "false" || txtStatus == Record.Zero || txtStatus == Record.One)
                                        {
                                            string res = Record.EnterActiveOrInactive;
                                            return res;
                                        }


                                        bool valid = ValidateStatus(headerText, txtStatus);
                                        if (valid == true)
                                        {
                                            break;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                            }
                            {
                                for (int j = 0; j < 1; j++)
                                {
                                    if (sbError.ToString() == "" || string.IsNullOrEmpty(sbError.ToString()))
                                    {

                                        if (txtUserId != null && string.IsNullOrEmpty(txtUserId))
                                        {
                                            break;
                                        }


                                    }
                                }
                            }
                            bool validvalue = false;
                            if (validvalue == true)
                            {
                                sbError.Append("  Please enter valid data,");

                                userRejected.ErrorMessage = sbError.ToString();

                                userRejected.CreatedDate = DateTime.UtcNow;

                                userRejected.ErrorMessage = sbError.ToString();
                                await _userMasterRejectedRepository.Add(userRejected);
                                totalRecordRejected++;
                                user = new APIUserMaster();
                                userRejected = new UserMasterRejected();
                                sbError.Clear();
                            }
                            else
                            {
                                userRejected.ErrorMessage = sbError.ToString();

                                if (sbError.ToString() != "")
                                {

                                    APIUserRejectedStatus user = new APIUserRejectedStatus
                                    {
                                        UserId = txtUserId,
                                        Status = txtStatus
                                    };
                                    userRejectedStatus.CreatedDate = DateTime.Now;
                                    await _userRepository.AddStatus(user);
                                    totalRecordRejected++;
                                }
                                else
                                {

                                    int Result = await _userRepository.UpdateImport(user, txtStatus);

                                    if (Result == 1)
                                    {
                                        totalRecordInsert++;
                                        user = new APIUserMaster();
                                        userRejected = new UserMasterRejected();
                                        sbError.Clear();
                                    }
                                    else
                                    {

                                        APIUserRejectedStatus aPIUserMaster = new APIUserRejectedStatus
                                        {
                                            UserId = txtUserId,

                                            Status = txtStatus
                                        };
                                        await _userRepository.AddStatus(aPIUserMaster);
                                        totalRecordRejected++;

                                        user = new APIUserMaster();
                                        userRejectedStatus = new UserRejectedStatus();

                                        sbError.Clear();

                                    }
                                }

                            }
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return (Record.TotalRecordInserted + totalRecordInsert + Record.TotalRecordRejected + totalRecordRejected);
        }
        public bool ValidateStatus(string headerText, string txtstatus)
        {
            bool valid = false;
            try
            {

                if (txtstatus != null && !string.IsNullOrEmpty(txtstatus))
                {


                    return true;
                }
                else
                {
                    user.IsActive = Convert.ToBoolean(txtstatus);
                    userRejected.IsActive = txtstatus;
                    sbError.Append("Please enter False or True for Status");
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
