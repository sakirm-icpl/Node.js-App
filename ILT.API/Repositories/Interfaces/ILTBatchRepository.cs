using AutoMapper;
using ILT.API.APIModel;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.ILT;
using Dapper;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Repositories
{
    public class ILTBatchRepository : Repository<ILTBatch>, IILTBatchRepository
    {
        private CourseContext _db;
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private ICustomerConnectionStringRepository _customerConnection;
        private ICourseRepository _courseRepository;
        private ITLSHelper _tLSHelper;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTScheduleRepository));
        public ILTBatchRepository(CourseContext courseContext,
                                  IConfiguration configuration,
                                  IHttpContextAccessor httpContextAccessor,
                                  IWebHostEnvironment hostingEnvironment,
                                  ICustomerConnectionStringRepository customerConnection,
                                  ICourseRepository courseRepository,
                                  ITLSHelper tLSHelper
                                  ) : base(courseContext)
        {
            _db = courseContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _customerConnection = customerConnection;
            _courseRepository = courseRepository;
            _tLSHelper = tLSHelper;
        }

        public async Task<BatchCode> GetBatchCode(int UserId)
        {
            int? batchId = _db.BatchCode.Max(u => (int?)u.Id);
            if (batchId != null)
            {
                BatchCode batchCode = await _db.BatchCode.Where(f => f.Id == batchId).FirstOrDefaultAsync();
                if (batchCode != null)
                {
                    if (batchCode.IsDeleted == true && batchCode.UserId == UserId)
                    {
                        string btCode = "B" + Convert.ToString(batchCode.Id);
                        ILTBatch iLTBatch = await _db.ILTBatch.Where(f => f.BatchCode == btCode).FirstOrDefaultAsync();
                        if (iLTBatch != null)
                        {
                            BatchCode batchCode1 = new BatchCode();
                            batchCode1.IsDeleted = false;
                            batchCode1.UserId = UserId;
                            _db.BatchCode.Add(batchCode1);
                            await _db.SaveChangesAsync();
                            return batchCode1;
                        }
                        else
                        {
                            return batchCode;
                        }
                    }
                    else
                    {
                        BatchCode BatchCode = new BatchCode();
                        BatchCode.IsDeleted = false;
                        BatchCode.UserId = UserId;
                        _db.BatchCode.Add(BatchCode);
                        await _db.SaveChangesAsync();
                        return BatchCode;
                    }
                }
                else
                {
                    BatchCode BatchCode = new BatchCode();
                    BatchCode.IsDeleted = false;
                    BatchCode.UserId = UserId;
                    _db.BatchCode.Add(BatchCode);
                    await _db.SaveChangesAsync();
                    return BatchCode;
                }
            }
            else
            {
                BatchCode BatchCode = new BatchCode();
                BatchCode.IsDeleted = false;
                BatchCode.UserId = UserId;
                _db.BatchCode.Add(BatchCode);
                await _db.SaveChangesAsync();
                return BatchCode;
            }
        }

        public async Task CancelBatchCode(APIBatchCode aPIBatchCode, int UserId)
        {
            string Code = aPIBatchCode.BatchCode.Replace("B", "");
            int BatchId = Convert.ToInt32(Code);
            BatchCode batchCode = await _db.BatchCode.Where(f => f.Id == BatchId).FirstOrDefaultAsync();
            batchCode.IsDeleted = true;
            batchCode.UserId = UserId;
            _db.BatchCode.Update(batchCode);
            await _db.SaveChangesAsync();
        }
        public async Task<ApiResponse> PostBatch(APIILTBatch aPIILTBatch, int UserId)
        {
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[ILTBatch_InsertUpdate]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@BatchCode", SqlDbType.VarChar) { Value = aPIILTBatch.BatchCode });
                        cmd.Parameters.Add(new SqlParameter("@BatchName", SqlDbType.VarChar) { Value = aPIILTBatch.BatchName });
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTBatch.CourseId });
                        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = aPIILTBatch.StartDate });
                        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = aPIILTBatch.EndDate });
                        cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = aPIILTBatch.StartTime });
                        cmd.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.Time) { Value = aPIILTBatch.EndTime });
                        cmd.Parameters.Add(new SqlParameter("@ILTBatchRegionType", SqlDbType.Structured) { Value = aPIILTBatch.RegionIds.ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@SeatCapacity", SqlDbType.Int) { Value = aPIILTBatch.SeatCapacity });
                        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = aPIILTBatch.Description });
                        cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = UserId });
                        SqlParameter ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                        ResponseParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ResponseParam);
                        await cmd.ExecuteNonQueryAsync();
                        connection.Close();
                        string ResponseValue = Convert.ToString(ResponseParam.Value);
                        if (ResponseValue == "Success")
                            return new ApiResponse { Description = ResponseValue, StatusCode = 200 };
                        else
                            return new ApiResponse { Description = ResponseValue, StatusCode = 400 };
                    }
                }
            }
        }
        public async Task<ApiResponse> PutBatch(APIILTBatch aPIILTBatch, int UserId)
        {
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[ILTBatch_InsertUpdate]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = aPIILTBatch.Id });
                        cmd.Parameters.Add(new SqlParameter("@BatchCode", SqlDbType.VarChar) { Value = aPIILTBatch.BatchCode });
                        cmd.Parameters.Add(new SqlParameter("@BatchName", SqlDbType.VarChar) { Value = aPIILTBatch.BatchName });
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTBatch.CourseId });
                        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = aPIILTBatch.StartDate });
                        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = aPIILTBatch.EndDate });
                        cmd.Parameters.Add(new SqlParameter("@StartTime", SqlDbType.Time) { Value = aPIILTBatch.StartTime });
                        cmd.Parameters.Add(new SqlParameter("@EndTime", SqlDbType.Time) { Value = aPIILTBatch.EndTime });
                        cmd.Parameters.Add(new SqlParameter("@ILTBatchRegionType", SqlDbType.Structured) { Value = aPIILTBatch.RegionIds.ToDataTable() });
                        cmd.Parameters.Add(new SqlParameter("@SeatCapacity", SqlDbType.Int) { Value = aPIILTBatch.SeatCapacity });
                        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = aPIILTBatch.Description });
                        cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = UserId });
                        SqlParameter ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                        ResponseParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(ResponseParam);
                        await cmd.ExecuteNonQueryAsync();
                        connection.Close();
                        string ResponseValue = Convert.ToString(ResponseParam.Value);
                        if (ResponseValue == "Success")
                            return new ApiResponse { Description = ResponseValue, StatusCode = 200 };
                        else
                            return new ApiResponse { Description = ResponseValue, StatusCode = 400 };
                    }
                }
            }
        }
        public async Task<APIILTBatch> GetBatch(int Id)
        {
            var result = (from batch in _db.ILTBatch
                          where batch.Id==Id && batch.IsDeleted==false
                          select new APIILTBatch
                          {
                              Id = batch.Id,
                              BatchCode = batch.BatchCode,
                              BatchName = batch.BatchName,
                              CourseId = batch.CourseId,
                              StartDate = batch.StartDate,
                              EndDate = batch.EndDate,
                              StartTime = batch.StartTime.ToString(@"hh\:mm"),
                              EndTime = batch.EndTime.ToString(@"hh\:mm"),
                              RegionIds = (from brb in _db.ILTBatchRegionBindings
                                           join region in _db.Configure11 on brb.RegionId equals region.Id
                                           where brb.IsDeleted==false && region.IsDeleted==0
                                                && brb.BatchId == batch.Id
                                           select new APIILTBatchRegionBindings
                                           {
                                               RegionId = brb.RegionId,
                                               RegionName = region.Name
                                           }).ToList(),
                              SeatCapacity = batch.SeatCapacity,
                              Description = batch.Description,
                              BatchType = batch.BatchType,
                              ReasonForCancellation = batch.ReasonForCancellation
                          });
            return await result.FirstOrDefaultAsync();
        }
        public async Task<string> IsBatchwiseNominationEnabled()
        {
            string value = null; //default value
            string configcode = "ENABLE_BATCHWISE_NOMINATION";
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configcode });
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

        public async Task<MessageType> DeleteBatch(APIILTBatchDelete aPIILTBatchDelete)
        {
            ILTBatch iLTBatch = await _db.ILTBatch.Where(x => x.Id == aPIILTBatchDelete.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
            if (iLTBatch == null)
                return MessageType.NotFound;
            ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(x => x.BatchId != null && x.BatchId == aPIILTBatchDelete.Id && x.IsActive==true && x.IsDeleted==false).FirstOrDefaultAsync();
            if (iLTSchedule != null)
                return MessageType.BatchDependancyExist;
            iLTBatch.BatchType = "Cancelled";
            iLTBatch.ReasonForCancellation = aPIILTBatchDelete.ReasonForCancellation;
            iLTBatch.IsActive = false;
            iLTBatch.IsDeleted = true;

            _db.ILTBatch.Update(iLTBatch);
            await _db.SaveChangesAsync();
            return MessageType.Success;
        }
        public async Task<List<APIILTBatchDetails>> GetBatches(int UserId, int page, int pageSize, string search = null, string searchText = null, bool? IsExport = null)
        {
            List<APIILTBatchDetails> aPIILTBatchDetails = new List<APIILTBatchDetails>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@page", page);
                    parameters.Add("@pageSize", pageSize);
                    parameters.Add("@search", search);
                    parameters.Add("@searchText", searchText);
                    parameters.Add("@userId", UserId);
                    parameters.Add("@isExport", IsExport);
                    var result = await SqlMapper.QueryAsync<APIILTBatchDetails>((SqlConnection)connection, "[dbo].[GetILTBatches]", parameters, null, null, CommandType.StoredProcedure);
                    aPIILTBatchDetails = result.ToList();
                    connection.Close();
                }
            }
            foreach (APIILTBatchDetails obj in aPIILTBatchDetails)
            {
                if(!string.IsNullOrEmpty(obj.RegionId))
                {
                    string[] RegionIds = obj.RegionId.Split(",");
                    string[] RegionNames = obj.RegionName.Split(",");
                    List<APIILTBatchRegionBindings> aPIILTBatchRegionBindings = new List<APIILTBatchRegionBindings>();
                    for (int i = 0; i < RegionIds.Length; i++)
                    {
                        APIILTBatchRegionBindings aPIILTBatchRegionBinding = new APIILTBatchRegionBindings();
                        aPIILTBatchRegionBinding.RegionId = Convert.ToInt32(RegionIds[i]);
                        aPIILTBatchRegionBinding.RegionName = RegionNames[i].ToString().Trim();
                        aPIILTBatchRegionBindings.Add(aPIILTBatchRegionBinding);
                    }
                    obj.RegionIds = aPIILTBatchRegionBindings;
                }
            }
            return aPIILTBatchDetails;
        }
        public async Task<int> GetBatchCount(int UserId, string search = null, string searchText = null)
        {
            int count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@page", 1);
                    parameters.Add("@pageSize", 0);
                    parameters.Add("@search", search);
                    parameters.Add("@searchText", searchText);
                    parameters.Add("@userId", UserId);
                    var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetILTBatches]", parameters, null, null, CommandType.StoredProcedure);
                    count = result.Select(x => x.RecordCount).FirstOrDefault();
                    connection.Close();
                }
            }
            return count;
        }
        public async Task<byte[]> ExportImportFormat(string OrgCode)
        {
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = FileName.ILTBatchImportFormat;
            string DomainName = this._configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILTBatch");

                worksheet.Cells[1, 1].Value = "Course Code*";
                worksheet.Cells[1, 2].Value = "Batch Name*";
                worksheet.Cells[1, 3].Value = "Start Date*";
                worksheet.Cells[1, 4].Value = "End Date*";
                worksheet.Cells[1, 5].Value = "Start Time*";
                worksheet.Cells[1, 6].Value = "End Time*";
                worksheet.Cells[1, 7].Value = "Region Name";
                worksheet.Cells[1, 8].Value = "Seat Capacity";
                worksheet.Cells[1, 9].Value = "Description";

                using (var rngitems = worksheet.Cells["A1:F1"])//Applying Css for header
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
            return fileData;
        }
        public async Task<ApiResponse> ProcessImportFile(APIILTBatchImport aPIILTBatchImport, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIILTBatchImport.Path;

                DataTable batchdt = ReadFile(filepath);
                if (batchdt == null || batchdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(batchdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(batchdt, UserId, OrgCode, importcolumns);
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
        public async Task<bool> ValidateFileColumnHeaders(DataTable userImportdt, List<string> importColumns)
        {
            if (userImportdt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < userImportdt.Columns.Count; i++)
            {
                string col = userImportdt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                userImportdt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(userImportdt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }
        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.CourseCode, 60));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.BatchName, 250));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.StartDate, 20));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.EndDate, 20));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.StartTime, 20));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.EndTime, 20));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.RegionName, 400));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.SeatCapacity, 5));
            columns.Add(new KeyValuePair<string, int>(APIILTBatchImportColumns.Description, 500));
            return columns;
        }
        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                    pck.Load(stream);
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    if (!dt.Columns.Contains(firstRowCell.Text))
                        dt.Columns.Add(firstRowCell.Text.Trim());
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    if (!string.IsNullOrEmpty(Convert.ToString(wsRow.ElementAtOrDefault(0))))
                    {
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            if (!string.IsNullOrEmpty(cell.Text))
                                row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    else
                        break;
                }
            }
            //check for empty rows
            DataTable validDt = new DataTable();
            validDt = dt.Clone();
            foreach (DataRow dataRow in dt.Rows)
            {
                bool IsEmpty = true;
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow[dataColumn])))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty)
                    validDt.ImportRow(dataRow);
            }

            return validDt;
        }
        public async Task<ApiResponse> ProcessRecordsAsync(DataTable batchdt, int userId, string OrgCode, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            var applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);

            batchdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = batchdt.Columns;
            foreach (string column in importcolumns)
            {
                batchdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<APIILTBatchRejected> aPIILTBatchRejectedList = new List<APIILTBatchRejected>();
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = batchdt.Clone();
            if (batchdt != null && batchdt.Rows.Count > 0)
            {
                foreach (DataRow dataRow in batchdt.Rows)
                {
                    APIILTBatchRejected aPIILTBatchRejected = new APIILTBatchRejected();
                    
                    bool IsError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "StartDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartDate = Convert.ToString(dataRow[column]);
                                string outStartDate = ValidateDate(StartDate, applicationDateFormat);
                                if (outStartDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                {
                                    dataRow[column] = outStartDate;
                                }
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Date required.";
                            }
                        }
                        else if (string.Compare(column, "EndDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndDate = Convert.ToString(dataRow[column]);
                                string outEndDate = ValidateDate(EndDate, applicationDateFormat);
                                if (outEndDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                {
                                    dataRow[column] = outEndDate;
                                }
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Date required.";
                            }
                        }
                        else if (string.Compare(column, "StartTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartTime = Convert.ToString(dataRow[column]);
                                string outStartTime = ValidateTime(StartTime);
                                if (outStartTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Time is not in hh:mm format.";
                                }
                                else
                                {
                                    dataRow[column] = outStartTime;
                                }
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Time required.";
                            }
                        }
                        else if (string.Compare(column, "EndTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndTime = Convert.ToString(dataRow[column]);
                                string outEndTime = ValidateTime(EndTime);
                                if (outEndTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Time is not in hh:mm format.";
                                }
                                else
                                {
                                    dataRow[column] = outEndTime;
                                }
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Time required.";
                            }
                        }
                        else if (string.Compare(column, "SeatCapacity", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                int seatcapacity;
                                bool flag = int.TryParse(Convert.ToString(dataRow[column]), out seatcapacity);
                                if (flag)
                                {
                                    dataRow[column] = seatcapacity;
                                }
                                else
                                {
                                    IsError = true;
                                    errorMsg = "Invalid Seat Capacity.";
                                }
                            }
                        }
                        if (!DBNull.Value.Equals(dataRow[column]))
                        {
                            int columnlength = columnlengths.Where(c => c.Key == column).Select(len => len.Value).FirstOrDefault();
                            if (columnlength < Convert.ToString(dataRow[column]).Length)
                            {
                                IsError = true;
                                errorMsg = "Invalid data in " + column + ". Must be less than equal to " + Convert.ToString(columnlength) + " characters.";
                                break;
                            }
                        }
                        
                        if (IsError)
                        break;
                    }   
                    if (IsError)
                    {
                        aPIILTBatchRejected.CourseCode = dataRow["CourseCode"] != null ? Convert.ToString(dataRow["CourseCode"]) : null;
                        aPIILTBatchRejected.BatchName = dataRow["BatchName"] != null ? Convert.ToString(dataRow["BatchName"]) : null;
                        aPIILTBatchRejected.StartDate = dataRow["StartDate"] != null ? Convert.ToString(dataRow["StartDate"]) : null;
                        aPIILTBatchRejected.EndDate = dataRow["EndDate"] != null ? Convert.ToString(dataRow["EndDate"]) : null;
                        aPIILTBatchRejected.StartTime = dataRow["StartTime"] != null ? Convert.ToString(dataRow["StartTime"]) : null;
                        aPIILTBatchRejected.EndTime = dataRow["EndTime"] != null ? Convert.ToString(dataRow["EndTime"]) : null;
                        aPIILTBatchRejected.RegionName = dataRow["RegionName"] != null ? Convert.ToString(dataRow["RegionName"]) : null;
                        aPIILTBatchRejected.SeatCapacity = dataRow["SeatCapacity"] != null ? Convert.ToString(dataRow["SeatCapacity"]) : null;
                        aPIILTBatchRejected.Description = dataRow["Description"] != null ? Convert.ToString(dataRow["Description"]) : null;
                        aPIILTBatchRejected.Status = Record.Rejected;
                        aPIILTBatchRejected.ErrorMessage = errorMsg;
                        dataRow["ErrorMessage"] = aPIILTBatchRejected.ErrorMessage;
                        aPIILTBatchRejectedList.Add(aPIILTBatchRejected);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                    }
                }

                try
                {
                    DataTable dtResult = new DataTable();
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[ILTBatch_BulkImport]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@ILTBatchImportType", SqlDbType.Structured) { Value = finalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }
                        connection.Close();
                    }
                    List<ILTBatchRejected> iLTBatchRejecteds = Mapper.Map<List<ILTBatchRejected>>(aPIILTBatchRejectedList);
                    _db.ILTBatchRejected.AddRange(iLTBatchRejecteds);
                    await _db.SaveChangesAsync();
                    aPIILTBatchRejectedList.AddRange(dtResult.ConvertToList<APIILTBatchRejected>());
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            int totalRecordInsert = aPIILTBatchRejectedList.Where(x => x.Status == "Inserted" && x.ErrorMessage == null).Count();
            int totalRecordUpdated = aPIILTBatchRejectedList.Where(x => x.Status == "Updated" && x.ErrorMessage == null).Count();
            int totalRecordRejected = aPIILTBatchRejectedList.Where(x => x.ErrorMessage != null).Count();
            string resultstring = "Total records inserted: " + totalRecordInsert + ", updated: "+totalRecordUpdated+" & rejected: " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIILTBatchRejectedList = aPIILTBatchRejectedList.Where(x=>x.ErrorMessage !=null).ToList() };
            return response;
        }

        public string ValidateDate(string InputDate, string validDateFormat)
        {
            string outputDate = null;
            try
            {
                DateTime result;
                result = DateTime.ParseExact(InputDate, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                outputDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputDate;
        }
        public string ValidateTime(string InputTime)
        {
            string outputTime = null;
            try
            {
                DateTime dt;
                if (DateTime.TryParseExact(InputTime, "HH:mm", CultureInfo.InvariantCulture,
                                                              DateTimeStyles.None, out dt))
                {
                    outputTime = dt.TimeOfDay.ToString(@"hh\:mm");
                }   
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputTime;
        }

        public async Task<List<APIILTBatchRejected>> GetBatchRejected(int page, int pageSize)
        {
            try
            {
                IQueryable<ILTBatchRejected> Query = _db.ILTBatchRejected;

                if (page>0 && pageSize > 0)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.Take(pageSize);
                }
                
                IEnumerable<ILTBatchRejected> iLTBatchRejected = await Query.ToListAsync();
                List<APIILTBatchRejected> aPIILTBatchRejected = Mapper.Map<List<APIILTBatchRejected>>(iLTBatchRejected);

                return aPIILTBatchRejected;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<FileInfo> ExportILTBatchReject()
        {
            IEnumerable<APIILTBatchRejected> aPIILTBatchRejected = await GetBatchRejected(0, 0);
            FileInfo fileInfo = GetILTBatchReject(aPIILTBatchRejected);
            return fileInfo;
        }

        private FileInfo GetILTBatchReject(IEnumerable<APIILTBatchRejected> aPIILTBatchRejecteds)
        {
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();

            List<string> ExportHeader = GetILTBatchRejectHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (APIILTBatchRejected aPIILTBatchRejected in aPIILTBatchRejecteds)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetILTBatchRejectRowData(aPIILTBatchRejected);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ILTBatchRejected, ExportData);
            return fileInfo;
        }

        private List<string> GetILTBatchRejectHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                HeaderName.CourseName,
                HeaderName.BatchName,
                HeaderName.StartDate,
                HeaderName.EndDate,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.RegionName,
                HeaderName.SeatCapacity,
                HeaderName.Description,
                HeaderName.Status,
                HeaderName.ErrorMessage,
            };
            return ExportHeader;
        }

        private List<string> GetILTBatchRejectRowData(APIILTBatchRejected aPIILTBatchRejected)
        {
            List<string> ExportData = new List<string>()
            {
                aPIILTBatchRejected.CourseCode,
                aPIILTBatchRejected.BatchName,
                aPIILTBatchRejected.StartDate,
                aPIILTBatchRejected.EndDate,
                aPIILTBatchRejected.StartTime,
                aPIILTBatchRejected.EndTime,
                aPIILTBatchRejected.RegionName,
                aPIILTBatchRejected.SeatCapacity,
                aPIILTBatchRejected.Description,
                aPIILTBatchRejected.Status,
                aPIILTBatchRejected.ErrorMessage,
            };
            return ExportData;
        }

        public async Task<int> CountRejected()
        {
            return await this._db.ILTBatchRejected.CountAsync();
        }
        public async Task<ApiResponse> GetBatchName(int CourseId, string search = null)
        {
            ApiResponse obj = new ApiResponse();

            var Query = await (from Batch in this._db.ILTBatch
                               where Batch.IsDeleted == Record.NotDeleted
                                    && Batch.CourseId == CourseId
                               select new { Batch.Id, Batch.BatchCode, Batch.BatchName }).Distinct().ToListAsync();

            obj.ResponseObject = Query;
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = Query.Where(a => a.BatchCode.ToLower().StartsWith(search.ToLower()) || a.BatchName.ToLower().StartsWith(search.ToLower()));
            }

            return obj;
        }
        public async Task<ApiResponse> GetBatchForNomination(int CourseId, string search = null)
        {
            ApiResponse obj = new ApiResponse();

            var Query = await (from Batch in this._db.ILTBatch
                               join Schedule in _db.ILTSchedule on Batch.Id equals Schedule.BatchId
                               where Batch.IsDeleted == Record.NotDeleted
                                    && Schedule.IsDeleted == false
                                    && Batch.CourseId == CourseId
                               select new { Batch.Id, Batch.BatchCode, Batch.BatchName }).Distinct().ToListAsync();

            obj.ResponseObject = Query;
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = Query.Where(a => a.BatchCode.ToLower().StartsWith(search.ToLower()) || a.BatchName.ToLower().StartsWith(search.ToLower()));
            }

            return obj;
        }
        public async Task<FileInfo> ExportBatches(int UserId, string OrgCode, string search = null, string searchText = null)
        {
            IEnumerable<APIILTBatchDetails> aPIILTBatchDetails = await GetBatches(UserId, 1, 10, search, searchText, true);
            FileInfo fileInfo = await GetILTBatches(aPIILTBatchDetails, OrgCode);
            return fileInfo;
        }
        private async Task<FileInfo> GetILTBatches(IEnumerable<APIILTBatchDetails> aPIILTBatchDetails, string OrgCode)
        {
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();
            string applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
            List<string> ExportHeader = GetILTBatchesHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (APIILTBatchDetails aPIILTBatchDetail in aPIILTBatchDetails)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetILTBatchesRowData(aPIILTBatchDetail, applicationDateFormat);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ILTBatches, ExportData);
            return fileInfo;
        }
        private List<string> GetILTBatchesHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                HeaderName.BatchCode,
                HeaderName.BatchName,
                HeaderName.CourseCode,
                HeaderName.CourseTitle,
                HeaderName.StartDate,
                HeaderName.EndDate,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.RegionName,
                HeaderName.SeatCapacity,
                HeaderName.Description,
                HeaderName.BatchType,
                HeaderName.ReasonForCancellation
            };
            return ExportHeader;
        }
        private List<string> GetILTBatchesRowData(APIILTBatchDetails aPIILTBatchDetail, string applicationDateFormat)
        {
            List<string> ExportData = new List<string>()
            {
                aPIILTBatchDetail.BatchCode,
                aPIILTBatchDetail.BatchName,
                aPIILTBatchDetail.CourseCode,
                aPIILTBatchDetail.CourseTitle,
                !string.IsNullOrEmpty(applicationDateFormat) ? aPIILTBatchDetail.StartDate.ToString(applicationDateFormat) : aPIILTBatchDetail.StartDate.ToString("dd/MM/yyyy"),
                !string.IsNullOrEmpty(applicationDateFormat) ? aPIILTBatchDetail.EndDate.ToString(applicationDateFormat) : aPIILTBatchDetail.EndDate.ToString("dd/MM/yyyy"),
                aPIILTBatchDetail.StartTime,
                aPIILTBatchDetail.EndTime,
                aPIILTBatchDetail.RegionName,
                aPIILTBatchDetail.SeatCapacity.ToString(),
                aPIILTBatchDetail.Description,
                aPIILTBatchDetail.BatchType,
                aPIILTBatchDetail.ReasonForCancellation
            };
            return ExportData;
        }
    }
}
