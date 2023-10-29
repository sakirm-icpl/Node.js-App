using log4net;
using Payment.API.APIModel;
using Payment.API.Helper;
using Payment.API.Repositories;
using Payment.API.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Payment.API.Services
{
    public class HRMSService : IHRMSService
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(HRMSBaseRepository));
        public IHRMSBaseRepository _hRMSBaseRepository;
        public HRMSService(IHRMSBaseRepository hRMSBaseRepository)
        {
            this._hRMSBaseRepository = hRMSBaseRepository;
        }

        public HRMSBasicData hRMSBasicData = new HRMSBasicData();

        public HRMSService(HRMSBasicData hRMSBasicData)
        {
            this.hRMSBasicData.OrgCode = hRMSBasicData.OrgCode;
            this.hRMSBasicData.RandomPassword = hRMSBasicData.RandomPassword;
            this.hRMSBasicData.spName = hRMSBasicData.spName;
            this.hRMSBasicData.importTableName = hRMSBasicData.importTableName;
            this.hRMSBasicData.LDap = hRMSBasicData.LDap;
            this.hRMSBasicData.validationMatrix = hRMSBasicData.validationMatrix;
        }

        public virtual DataTable addColumn(DataTable dt, string colName, bool val)
        {
            DataColumn dc = new DataColumn();
            dc.ColumnName = colName;
            dc.DefaultValue = Convert.ToInt32(val);
            dt.Columns.Add(dc);
            return dt;
        }



        public async Task<APIHRMSResponseNew> MainHRMSProcess<T>(List<T> userList, string OrgCode, string connectionString)
        {
            HRMSBasicData hRMSBasicData = new HRMSBasicData();
            hRMSBasicData.OrgCode = OrgCode;
            hRMSBasicData.RandomPassword = "Pass@123";
            hRMSBasicData.spName = "[dbo].[UserMaster_BulkImportTVSCreditHrms]";
            hRMSBasicData.importTableName = "@UserImportType";
            hRMSBasicData.LDap = "true";

            _logger.Info(hRMSBasicData.OrgCode.ToUpper() + " HRMS Schedular Started: " + DateTime.Now);
            List<DataTable> allTables = new List<DataTable>();
            try
            {
                List<DataTable> dataTablesList = new List<DataTable>();
                DataTable datatable = new DataTable();// GetHRMSInputDataAPI(pair.Value, hRMSBasicData);   
                datatable = ToDataTable<T>(userList);// GetHRMSInputDataAPI(pair.Value, hRMSBasicData);
                if (datatable.Rows == null || datatable.Rows.Count == 0)
                {
                    _logger.Error("Data not Preset in files");
                }

                DataTable dt0 = AdditionalDataProcessing(datatable).Result;
                _logger.Info("Total Record Fetched : " + dt0.Rows.Count);
                //Task.Run(() => DataTableToCSVAsync(dt0)).Wait();
                List<APIUserSettingNew> userSetting = _hRMSBaseRepository.GetUserSetting(connectionString).Result.ToList();
                DataTable dt = _hRMSBaseRepository.ChangeRawTable(dt0, hRMSBasicData.OrgCode, GetInitialAPIColumnMappingTVS());
                APIIsFileValid aPIIsFileValid = _hRMSBaseRepository.ValidateFileColumnHeaders(dt, hRMSBasicData.OrgCode, userSetting).Result;

                if (aPIIsFileValid.Flag == true)
                {
                    string result = _hRMSBaseRepository.ProcessRecordsAsync(1, aPIIsFileValid.userImportdt, hRMSBasicData.OrgCode,
                     userSetting, hRMSBasicData.RandomPassword, hRMSBasicData.spName, hRMSBasicData.importTableName, hRMSBasicData, connectionString).Result;
                    _logger.Info(hRMSBasicData.OrgCode.ToUpper() + " HRMS Import Result: " + result);
                    return await _hRMSBaseRepository.GetHRMSProcessStatus(aPIIsFileValid.userImportdt, connectionString);
                }
                else
                    _logger.Info("File data is not in Proper format for file : " + datatable.TableName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _logger.Info(hRMSBasicData.OrgCode.ToUpper() + " HRMS Schedular Finished Iteration with Exception: " + DateTime.Now);
            }
            return new APIHRMSResponseNew() { StatusCode = "ER", StatusMessage = "Failed", AppName = "Empowered LMS" };
        }






        async public Task<int> DataTableToCSVAsync(DataTable dt)
        {
            string dirPath = Directory.GetCurrentDirectory();
            string baseDirectory = dirPath + "\\ExtractedFiles\\";
            string locationPath = dirPath + "\\ExtractedFiles\\" + DateTime.Now.ToString("ddMMyyyy") + "\\";
            StreamWriter writer = null;
            try
            {
                if (Directory.Exists(baseDirectory))
                {
                    List<string> dirnames = Directory.GetDirectories(baseDirectory).ToList();
                    foreach (string dirname in dirnames)
                    {
                        FileInfo file = new FileInfo(dirname);
                        if ((DateTime.Now - file.CreationTime).TotalDays > 100)
                        {
                            //  Directory.Delete(dirname, true);
                        }
                    }
                }

                if (!Directory.Exists(locationPath))
                    Directory.CreateDirectory(locationPath);
                string filename = (DateTime.Now.ToString("ddMMyyyHH:mm:ss:fffffff").Replace(" ", "").Replace(":", "")) + ".csv";
                writer = new StreamWriter(File.Create(locationPath + filename));
                string content = null;
                foreach (DataColumn dc in dt.Columns)
                {
                    content += Convert.ToString(dc.ColumnName) + ",";
                }
                await writer.WriteLineAsync(content);

                foreach (DataRow dr in dt.Rows)
                {
                    content = null;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        content += Convert.ToString(dr[dc]) + ",";
                    }
                    await writer.WriteLineAsync(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //_logger.Error(null, ex.Message, ex.StackTrace, "Operation");
            }
            finally
            {
                writer.Close();
            }
            return 1;
        }



        public Dictionary<string, string> GetInitialAPIColumnMappingTVS()
        {
            Dictionary<string, string> initialColumn = new Dictionary<string, string>();

            initialColumn.Add("employee_id", "UserId");
            initialColumn.Add("email", "EmailId");
            initialColumn.Add("fullName", "UserName");
            initialColumn.Add("mobile", "MobileNumber");
            initialColumn.Add("IsActive", "IsActive");
            //initialColumn.Add("Functional Appraiser Email ID", "ReportsTo");
            //initialColumn.Add("Business Classification", "Business");
            initialColumn.Add("functionname", "Group");
            initialColumn.Add("region", "Area");
            initialColumn.Add("location", "Location");
            initialColumn.Add("reporting_manager_name", "Reporting Manager Name");
            initialColumn.Add("reporting_manager_id", "Reporting Manager ID");
            initialColumn.Add("State", "State");
            initialColumn.Add("position", "Position");
            initialColumn.Add("designation", "Designation");
            initialColumn.Add("pl_area", "P&L Area");
            initialColumn.Add("product", "Product");
            initialColumn.Add("department", "Department");
            initialColumn.Add("last_working_date", "LastWorking Day");
            initialColumn.Add("status_of_resignation", "Resignation Status");
            initialColumn.Add("grade", "Grade");

            return initialColumn;
        }



        public DataTable ToDataTable<T>(List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];

            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }



        public async Task<DataTable> AdditionalDataProcessing(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                try
                {
                    dr["reporting_manager_id"] = Convert.ToString(dr["reporting_manager_id"]).ToLower();
                }
                catch (Exception ex) { }
                // dr["UserType"] = "Internal";

                //string mobile = Convert.ToString(dr["MOBILENUMBER"]);
                //if (!string.IsNullOrEmpty(mobile))
                //{
                //    if(mobile.Length>10)
                //     dr["MOBILENUMBER"] = mobile.Substring(mobile.Length - 10);
                //}
            }
            return dataTable;
        }
        public async Task<DataTable> TakeOnlyActiveInactive(DataTable dataTable, string keyType)
        {
            //DataTable data = new DataTable();
            //data = dataTable.Copy();
            //data.Clear();
            //foreach (DataRow dr in dataTable.Rows)
            //{
            //    string recordstatus = Convert.ToString(dr["Employee Status"]);
            //    if (!String.IsNullOrEmpty(recordstatus))
            //    {
            //        if ((keyType == "Inactive" && (recordstatus.ToLower() == "inactive" || recordstatus.ToLower() == "terminated"))
            //           || (keyType == "Active" && recordstatus.ToLower() == "active"))
            //        {
            //            DataRow dtr = data.NewRow();
            //            dtr.ItemArray = dr.ItemArray;
            //            data.Rows.Add(dtr);
            //        }
            //    }
            //}
            return dataTable;
        }

    }



}
