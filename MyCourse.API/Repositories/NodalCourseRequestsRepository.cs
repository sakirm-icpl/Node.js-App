using AutoMapper;
using CCA.Util;
using MyCourse.API.APIModel;
//using MyCourse.API.APIModel.NodalManagement;
using MyCourse.API.Common;
using MyCourse.API.Helper;
//using MyCourse.API.Helper.Metadata;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Dapper;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace MyCourse.API.Repositories
{
    public class NodalCourseRequestsRepository : Repository<NodalCourseRequests>, INodalCourseRequestsRepository
    {
        private CourseContext _db;
        private ICustomerConnectionStringRepository _customerConnection;
        //private IEmail _email;
        //private ITLSHelper _tLSHelper;
        private IAccessibilityRule _accessibilityRule;
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NodalCourseRequestsRepository));
        public NodalCourseRequestsRepository(CourseContext context,
            ICustomerConnectionStringRepository customerConnection,
         //   IEmail email, ITLSHelper tLSHelper,
            IAccessibilityRule accessibilityRule,
            IConfiguration configuration) : base(context)
        {
            _db = context;
            _customerConnection = customerConnection;
          //  _email = email;
          //  _tLSHelper = tLSHelper;
            _accessibilityRule = accessibilityRule;
            _configuration = configuration;
        }
        //#region single_course_request
        //public async Task<List<APINodalCourseRequests>> GetCourseRequests(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false,string OrgCode=null)
        //{
        //    if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
        //        SearchText = Security.Encrypt(SearchText);

        //    List<APINodalCourseRequests> aPINodalCourseRequests = new List<APINodalCourseRequests>();
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@Page", Page);
        //            parameters.Add("@PageSize", PageSize);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            parameters.Add("@OrgCode", OrgCode);
        //            var result = await SqlMapper.QueryAsync<APINodalCourseRequests>((SqlConnection)connection, "[dbo].[GetNodalCourseRequests]", parameters, null, null, CommandType.StoredProcedure);
        //            aPINodalCourseRequests = result.ToList();
        //            connection.Close();
        //        }
        //    }
        //    aPINodalCourseRequests.ForEach(obj =>
        //    {
        //        obj.UserId = !string.IsNullOrEmpty(obj.UserId) ? obj.UserId.Decrypt() : null;
        //        obj.EmailId = !string.IsNullOrEmpty(obj.EmailId.Decrypt()) ? obj.EmailId.Decrypt() : null;
        //        obj.MobileNumber = !string.IsNullOrEmpty(obj.MobileNumber.Decrypt()) ? obj.MobileNumber.Decrypt() : null;
        //        obj.AadharNumber = !string.IsNullOrEmpty(obj.AadharNumber.Decrypt()) ? obj.AadharNumber.Decrypt() : null;
        //    });
        //    return aPINodalCourseRequests;
        //}
        //public async Task<int> GetCourseRequestsCount(int UserId, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
        //        SearchText = Security.Encrypt(SearchText);

        //    int count = 0;
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@Page", 1);
        //            parameters.Add("@PageSize", 0);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetNodalCourseRequests]", parameters, null, null, CommandType.StoredProcedure);
        //            count = result.Select(x => x.RecordCount).FirstOrDefault();
        //            connection.Close();
        //        }
        //    }
        //    return count;
        //}
        //public async Task<FileInfo> ExportCourseRequests(int UserId, string OrgCode, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    IEnumerable<APINodalCourseRequests> aPINodalCourseRequests = await GetCourseRequests(UserId, 1, 0, Search, SearchText, true,OrgCode);
        //    FileInfo fileInfo = await GetCourseRequests(aPINodalCourseRequests, OrgCode);
        //    return fileInfo;
        //}
        //private async Task<FileInfo> GetCourseRequests(IEnumerable<APINodalCourseRequests> aPINodalCourseRequests, string OrgCode)
        //{
        //    int RowNumber = 0;
        //    Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();
        //    List<string> ExportHeader = GetCourseRequestsHeader();
        //    ExportData.Add(RowNumber, ExportHeader);
        //    string applicationDateFormat = await GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
        //    if (string.IsNullOrEmpty(applicationDateFormat))
        //        applicationDateFormat = "dd/MM/yyyy";

        //    foreach (APINodalCourseRequests courseRequest in aPINodalCourseRequests)
        //    {
        //        List<string> DataRow = new List<string>();
        //        DataRow = GetCourseRequestsRowData(courseRequest, applicationDateFormat);
        //        RowNumber++;
        //        ExportData.Add(RowNumber, DataRow);
        //    }

        //    FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ILTBatches, ExportData);
        //    return fileInfo;
        //}
        //public async Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "")
        //{
        //    DataTable dtConfigurationValues;
        //    string configValue;
        //    try
        //    {
        //        var cache = new CacheManager.CacheManager();
        //        string cacheKeyConfig = (Constants.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

        //        if (cache.IsAdded(cacheKeyConfig))
        //            dtConfigurationValues = cache.Get<DataTable>(cacheKeyConfig);
        //        else
        //        {
        //            dtConfigurationValues = this.GetAllConfigurableParameterValue();
        //            cache.Add(cacheKeyConfig, dtConfigurationValues, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
        //        }
        //        DataRow[] dr = dtConfigurationValues.Select("Code ='" + configurationCode + "'");
        //        if (dr.Length > 0)
        //            configValue = Convert.ToString(dr[0]["Value"]);
        //        else
        //            configValue = defaultValue;
        //        _logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
        //    }
        //    catch (System.Exception ex)
        //    {
        //        _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
        //        return null;
        //    }
        //    return configValue;
        //}
        //public DataTable GetAllConfigurableParameterValue()
        //{
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (var dbContext = _customerConnection.GetDbContext())
        //        {
        //            var connection = dbContext.Database.GetDbConnection();
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            using (var cmd = connection.CreateCommand())
        //            {
        //                cmd.CommandText = "GetAllConfigurableParameterValues";
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                DbDataReader reader = cmd.ExecuteReader();
        //                dt.Load(reader);

        //                reader.Dispose();
        //            }
        //            connection.Close();
        //        }
        //    }
        //    catch (System.Exception ex)
        //    { _logger.Error("Exception in function GetAllConfigurableParameterValue :-" + Utilities.GetDetailedException(ex)); }

        //    return dt;
        //}
        //private List<string> GetCourseRequestsHeader()
        //{
        //    List<string> ExportHeader = new List<string>()
        //    {
        //        HeaderName.UserId,
        //        HeaderName.UserName,
        //        HeaderName.EmailId,
        //        HeaderName.MobileNumber,
        //        HeaderName.FHName,
        //        HeaderName.AadharNumber,
        //        HeaderName.CourseCode,
        //        HeaderName.CourseTitle,
        //        HeaderName.RequestedDate,
        //        HeaderName.RequestStatus,
        //        HeaderName.Reason,
        //        HeaderName.PaymentStatus,
        //        HeaderName.CourseStatus,
        //        HeaderName.CompletionDate,
        //    };
        //    return ExportHeader;
        //}
        //private List<string> GetCourseRequestsRowData(APINodalCourseRequests courseRequest, string applicationDateFormat)
        //{
        //    List<string> ExportData = new List<string>()
        //    {
        //        courseRequest.UserId.Decrypt(),
        //        courseRequest.UserName,
        //        courseRequest.EmailId.Decrypt(),
        //        courseRequest.MobileNumber.Decrypt(),
        //        courseRequest.FHName,
        //        courseRequest.AadharNumber,
        //        courseRequest.CourseCode,
        //        courseRequest.CourseTitle,
        //        courseRequest.RequestedDate.ToString(applicationDateFormat),
        //        courseRequest.RequestStatus,
        //        courseRequest.Reason,
        //        courseRequest.PaymentStatus,
        //        courseRequest.CourseStatus,
        //        courseRequest.CompletionDate!=null ? ((DateTime)courseRequest.CompletionDate).ToString(applicationDateFormat) : string.Empty
        //    };
        //    return ExportData;
        //}
        //#endregion

        //#region group_course_request
        //public async Task<List<APINodalCourseRequestGroupDetails>> GetRequestedCourseGroups(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    List<APINodalCourseRequestGroupDetails> aPINodalCourseRequestGroupDetails = new List<APINodalCourseRequestGroupDetails>();
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@Page", Page);
        //            parameters.Add("@PageSize", PageSize);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            var result = await SqlMapper.QueryAsync<APINodalCourseRequestGroupDetails>((SqlConnection)connection, "[dbo].[GetNodalRequestedGroupCourses]", parameters, null, null, CommandType.StoredProcedure);
        //            aPINodalCourseRequestGroupDetails = result.ToList();
        //            connection.Close();
        //        }
        //    }
        //    return aPINodalCourseRequestGroupDetails;
        //}
        //public async Task<int> GetRequestedCourseGroupsCount(int UserId, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    int count = 0;
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@Page", 1);
        //            parameters.Add("@PageSize", 0);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetNodalRequestedGroupCourses]", parameters, null, null, CommandType.StoredProcedure);
        //            count = result.Select(x => x.RecordCount).FirstOrDefault();
        //            connection.Close();
        //        }
        //    }
        //    return count;
        //}
        //public async Task<List<APINodalCourseRequestUserDetails>> GetGroupCourseRequests(int UserId, int Page, int PageSize, int? GroupId = null, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
        //        SearchText = Security.Encrypt(SearchText);

        //    List<APINodalCourseRequestUserDetails> aPINodalCourseRequestUserDetails = new List<APINodalCourseRequestUserDetails>();
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@GroupId", GroupId);
        //            parameters.Add("@Page", Page);
        //            parameters.Add("@PageSize", PageSize);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            var result = await SqlMapper.QueryAsync<APINodalCourseRequestUserDetails>((SqlConnection)connection, "[dbo].[GetNodalGroupCourseRequests]", parameters, null, null, CommandType.StoredProcedure);
        //            aPINodalCourseRequestUserDetails = result.ToList();
        //            connection.Close();
        //        }
        //    }
        //    aPINodalCourseRequestUserDetails.ForEach(x =>
        //    {
        //        x.UserId = Security.Decrypt(x.UserId);
        //        x.EmailId = Security.Decrypt(x.EmailId);
        //        x.AadharNumber = Security.Decrypt(x.AadharNumber);
        //        x.MobileNumber = Security.Decrypt(x.MobileNumber);
        //    });

        //    return aPINodalCourseRequestUserDetails;
        //}
        //public async Task<int> GetGroupCourseRequestsCount(int UserId, int GroupId, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
        //        SearchText = Security.Encrypt(SearchText);

        //    int count = 0;
        //    using (var dbContext = this._customerConnection.GetDbContext())
        //    {
        //        using (var connection = dbContext.Database.GetDbConnection())
        //        {
        //            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                connection.Open();
        //            DynamicParameters parameters = new DynamicParameters();
        //            parameters.Add("@UserId", UserId);
        //            parameters.Add("@GroupId", GroupId);
        //            parameters.Add("@Page", 1);
        //            parameters.Add("@PageSize", 0);
        //            parameters.Add("@Search", Search);
        //            parameters.Add("@SearchText", SearchText);
        //            parameters.Add("@IsExport", IsExport);
        //            var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetNodalGroupCourseRequests]", parameters, null, null, CommandType.StoredProcedure);
        //            count = result.Select(x => x.RecordCount).FirstOrDefault();
        //            connection.Close();
        //        }
        //    }
        //    return count;
        //}
        //public async Task<FileInfo> ExportGroupCourseRequests(int UserId, string OrgCode, int? GroupId = null, string Search = null, string SearchText = null, bool IsExport = false)
        //{
        //    IEnumerable<APINodalCourseRequestUserDetails> aPINodalCourseRequestUserDetails = await GetGroupCourseRequests(UserId, 1, 0, GroupId, Search, SearchText, true);
        //    FileInfo fileInfo = await GetGroupCourseRequests(aPINodalCourseRequestUserDetails, OrgCode);
        //    return fileInfo;
        //}
        //private async Task<FileInfo> GetGroupCourseRequests(IEnumerable<APINodalCourseRequestUserDetails> aPINodalCourseRequestUserDetails, string OrgCode)
        //{
        //    int RowNumber = 0;
        //    Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();
        //    List<string> ExportHeader = GetGroupCourseRequestsHeader();
        //    ExportData.Add(RowNumber, ExportHeader);
        //    string applicationDateFormat = await GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
        //    if (string.IsNullOrEmpty(applicationDateFormat))
        //        applicationDateFormat = "dd/MM/yyyy";

        //    foreach (APINodalCourseRequestUserDetails courseRequest in aPINodalCourseRequestUserDetails)
        //    {
        //        List<string> DataRow = new List<string>();
        //        DataRow = GetGroupCourseRequestsRowData(courseRequest, applicationDateFormat);
        //        RowNumber++;
        //        ExportData.Add(RowNumber, DataRow);
        //    }

        //    FileInfo fileInfo = _tLSHelper.GenerateExcelFile(FileName.ILTBatches, ExportData);
        //    return fileInfo;
        //}
        //private List<string> GetGroupCourseRequestsHeader()
        //{
        //    List<string> ExportHeader = new List<string>()
        //    {
        //        HeaderName.UserId,
        //        HeaderName.UserName,
        //        HeaderName.EmailId,
        //        HeaderName.MobileNumber,
        //        HeaderName.FHName,
        //        HeaderName.AadharNumber,
        //        HeaderName.GroupCode,
        //        HeaderName.CourseCode,
        //        HeaderName.CourseTitle,
        //        HeaderName.RequestedDate,
        //        HeaderName.RequestStatus,
        //        HeaderName.Reason,
        //        HeaderName.PaymentStatus,
        //        HeaderName.CourseStatus,
        //        HeaderName.CompletionDate,
        //    };
        //    return ExportHeader;
        //}
        //private List<string> GetGroupCourseRequestsRowData(APINodalCourseRequestUserDetails courseRequest, string applicationDateFormat)
        //{
        //    List<string> ExportData = new List<string>()
        //    {
        //        courseRequest.UserId.Decrypt(),
        //        courseRequest.UserName,
        //        courseRequest.EmailId.Decrypt(),
        //        courseRequest.MobileNumber.Decrypt(),
        //        courseRequest.FHName,
        //        courseRequest.AadharNumber,
        //        courseRequest.GroupCode,
        //        courseRequest.CourseCode,
        //        courseRequest.CourseTitle,
        //        courseRequest.RequestedDate.ToString(applicationDateFormat),
        //        courseRequest.RequestStatus,
        //        courseRequest.Reason,
        //        courseRequest.PaymentStatus,
        //        courseRequest.CourseStatus,
        //        courseRequest.CompletionDate!=null ? ((DateTime)courseRequest.CompletionDate).ToString(applicationDateFormat) : string.Empty
        //    };
        //    return ExportData;
        //}
        //#endregion
        //public async Task<string> ProcessCourseRequest(int UserId, APINodalRequest aPINodalRequests, string OrgCode)
        //{
        //    if (aPINodalRequests.IsAccepted)
        //    {
        //        NodalCourseRequests nodalCourseRequests = await _db.NodalCourseRequests.Where(f => f.Id == aPINodalRequests.Id && f.IsDeleted == false).FirstOrDefaultAsync();
        //        UserMaster users = await _db.UserMaster.Where(x => x.Id == nodalCourseRequests.UserId && x.IsDeleted == false).FirstOrDefaultAsync();
        //        Model.Course course = await _db.Course.Where(x => x.Id == nodalCourseRequests.CourseId && x.IsDeleted == false).FirstOrDefaultAsync();
        //        List<APINodalRequestList> aPINodalRequestList = new List<APINodalRequestList>();
        //      //  List<NodalCourseRequests> nodalCourseRequestsList = new List<NodalCourseRequests>();

        //        APINodalRequestList aPINodalRequest = new APINodalRequestList();
        //        aPINodalRequest.Id = aPINodalRequests.Id;
        //        aPINodalRequest.CourseTitle = course.Title;
        //        aPINodalRequest.OrgCode = OrgCode;
        //        aPINodalRequest.RequestType = nodalCourseRequests.RequestType;

        //        aPINodalRequest.UserMasterId = users.Id;
        //        aPINodalRequest.UserId = Security.Decrypt(users.UserId);
        //        aPINodalRequest.UserName = users.UserName;
        //        aPINodalRequest.EmailId = users.EmailId;
        //        aPINodalRequest.IsApprove = aPINodalRequests.IsApprovedByNodal;
        //        aPINodalRequest.Reason = aPINodalRequests.Reason;
        //        if (nodalCourseRequests.IsPaymentDone == true && aPINodalRequests.IsApprovedByNodal == false)
        //            return "Cannot reject as payment is made.";
        //        else if (nodalCourseRequests.IsApprovedByNodal == aPINodalRequests.IsApprovedByNodal)
        //        {
        //            string status = Convert.ToBoolean(aPINodalRequests.IsApprovedByNodal) ? "Approved" : "Rejected";
        //            return "Course Request Already " + status + ".";
        //        }
        //        else
        //        {
        //            List<NodalCourseRequests> otherNodalCourseRequestsExists = await _db.NodalCourseRequests.Where(x => x.CourseId == nodalCourseRequests.CourseId && x.UserId == nodalCourseRequests.UserId && x.Id != nodalCourseRequests.Id && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
        //            if (otherNodalCourseRequestsExists.Count > 0)
        //                return "Cannot approve as other pending/approved request for same course exist.";
        //            else
        //            {
        //                if (OrgCode.ToLower().Contains("ttgroupglobal"))
        //                    {
        //                    try
        //                    {
        //                        string accessCode = "AVMF24JI86BF24FMFB";//from avenues
        //                        string workingKey = "3F597153F7F1D6BB240C007F2F1EB41B";// from avenues

        //                        InvoiceBasePaymentJson paymentRequest = new InvoiceBasePaymentJson()
        //                        {
        //                            customer_name = users.UserName,
        //                            bill_delivery_type = "BOTH",
        //                            customer_mobile_no = "",
        //                            customer_email_id = Security.Decrypt( users.EmailId),
        //                            customer_email_subject = "T T SKILLS DEVELOPEMENT PRIVATE LIMITED ",
        //                            invoice_description = "T T SKILLS DEVELOPEMENT PRIVATE LIMITED ",
        //                            currency = course.Currency,
        //                            valid_for = "434",
        //                            valid_type = "minutes",
        //                            amount = course.CourseFee.ToString(),
        //                            merchant_reference_no = nodalCourseRequests.Id.ToString(),
        //                            merchant_reference_no1 = nodalCourseRequests.Id.ToString(),
        //                            merchant_reference_no2 = nodalCourseRequests.CourseId.ToString(),
        //                            merchant_reference_no3 = nodalCourseRequests.UserId.ToString(),
        //                            merchant_reference_no4 = OrgCode.ToString(),
        //                            merchant_reference_no5 = OrgCode.ToString(),
        //                            sub_acc_id = "",
        //                            terms_and_conditions = "testing mode",
        //                            sms_content = "Plscall022-2121212121topayyourLegalEntity_Namebill#Invoice_IDforInvoice_CurrencyInvoice_AmountorpayonlineatPay_Link"

        //                        };
        //                        string paymentRequestQueryJson = JsonSerializer.Serialize(paymentRequest);
        //                        string encJson = "";
        //                        _logger.Info("paymentRequestQueryJson");
        //                        _logger.Info(paymentRequestQueryJson);
        //                        string queryUrl = "https://apitest.ccavenue.com/apis/servlet/DoWebTrans";

        //                        CCACrypto ccaCrypto = new CCACrypto();
        //                        encJson = ccaCrypto.Encrypt(paymentRequestQueryJson, workingKey);

        //                        // make query for the status of the order to ccAvenues change the command param as per your need generateQuickInvoice
        //                        string authQueryUrlParam = "enc_request=" + encJson + "&access_code=" + accessCode + "&request_type=JSON&response_type=JSON&command=generateQuickInvoice&version=1.2";

        //                        // Url Connection
        //                        String message = postPaymentRequestToGateway(queryUrl, authQueryUrlParam);
        //                        //Response.Write(message);
        //                        _logger.Info(message);
        //                        NameValueCollection param = getResponseMap(message);
        //                        String status = "";
        //                        String encResJson = "";
        //                        if (param != null && param.Count == 2)
        //                        {
        //                            for (int i = 0; i < param.Count; i++)
        //                            {
        //                                if ("status".Equals(param.Keys[i]))
        //                                {
        //                                    status = param[i];
        //                                }
        //                                if ("enc_response".Equals(param.Keys[i]))
        //                                {
        //                                    encResJson = param[i];
        //                                    //Response.Write(encResXML);
        //                                }
        //                            }
        //                            if (!"".Equals(status) && status.Equals("0"))
        //                            {
        //                                String ResJson = ccaCrypto.Decrypt(encResJson, workingKey);
        //                                PaymentResponceJson responce = JsonSerializer.Deserialize<PaymentResponceJson>(ResJson);
        //                                if (!string.IsNullOrEmpty( responce.tiny_url))
        //                                {
        //                                    _logger.Info(responce.tiny_url);
        //                                    _logger.Debug("Payment Url - " + responce.tiny_url);
        //                                    aPINodalRequest.Status = "Inserted";
        //                                    aPINodalRequest.PaymentUrl = responce.tiny_url;
        //                                    try
        //                                    {
        //                                        nodalCourseRequests.IsApprovedByNodal = aPINodalRequests.IsApprovedByNodal;
        //                                        nodalCourseRequests.Reason = aPINodalRequests.Reason;
        //                                        nodalCourseRequests.IsAccepted = true;
        //                                        _db.NodalCourseRequests.Update(nodalCourseRequests);
        //                                        await _db.SaveChangesAsync();
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        _logger.Error(ex.ToString());
        //                                    }

        //                                   // nodalCourseRequestsList.Add(nodalCourseRequests);
        //                                }

        //                                else
        //                                {
        //                                    return responce.error_desc;
        //                                }
                                        
        //                                InvoicePaymentResponse resObj = Mapper.Map<InvoicePaymentResponse>(responce);
        //                                resObj.CreatedBy = UserId;
        //                                resObj.CreatedDate = DateTime.Now;
        //                                resObj.IsActive = true;
        //                                resObj.IsDeleted = false;
        //                                resObj.CourseId = course.Id;
        //                                resObj.UserId = users.Id;
        //                                _db.InvoicePaymentResponse.Add(resObj);
        //                                await _db.SaveChangesAsync();
        //                            }
        //                            else if (!"".Equals(status) && status.Equals("1"))
        //                            {
        //                                return "failure response from ccAvenues: " + encResJson;
        //                            }

        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                        _logger.Error(ex.ToString());
        //                    }

        //                }
        //                else {
        //                    _logger.Debug("Request Id - " + nodalCourseRequests.Id.ToString());
        //                    string PaymentUrl = string.Format("{0}{1}?q={2}", _configuration["ApiGatewayUrl"], "Payment", HttpUtility.UrlEncode(Security.Encrypt(nodalCourseRequests.Id.ToString())));
        //                    _logger.Debug("Payment Url - " + PaymentUrl);
        //                    aPINodalRequest.Status = "Inserted";
        //                    aPINodalRequest.PaymentUrl = PaymentUrl;
        //                    nodalCourseRequests.IsApprovedByNodal = aPINodalRequests.IsApprovedByNodal;
        //                    nodalCourseRequests.Reason = aPINodalRequests.Reason;
        //                    nodalCourseRequests.IsAccepted = true;
        //                    _db.NodalCourseRequests.Update(nodalCourseRequests);
        //                    await _db.SaveChangesAsync();
        //                    //nodalCourseRequestsList.Add(nodalCourseRequests);
        //                }
                       
        //            }
        //        }
        //        aPINodalRequestList.Add(aPINodalRequest);

        //        //Send Course Approval Notification
        //        if (aPINodalRequestList.Where(x => x.IsApprove == true && x.Status == "Inserted").Count() > 0)
        //            _email.SendCourseRequestApprovalMailToUsers(aPINodalRequestList.Where(x => x.IsApprove == true && x.Status == "Inserted").ToList());
        //        if (aPINodalRequestList.Where(x => x.IsApprove == false && x.Status == "Inserted").Count() > 0)
        //            _email.SendCourseRequestRejectedMailToUsers(aPINodalRequestList.Where(x => x.IsApprove == false && x.Status == "Inserted").ToList());

        //        return "Success";
        //    }
        //    else
        //    {
        //        return "Please accept terms before approval or rejection of requests";
        //    }
        //}
        //public async Task<APINodalRequestResponse> ProcessCourseGroupRequest(int UserId, List<APINodalRequest> aPINodalRequests, string OrgCode)
        //{
        //    bool IsAccepted = aPINodalRequests.Select(x => x.IsAccepted).FirstOrDefault();
        //    if (IsAccepted)
        //    {
        //        List<int> RequestIds = new List<int>();
        //        RequestIds = aPINodalRequests.Select(d => d.Id).ToList();
        //        List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(f => RequestIds.Contains(f.Id) && f.IsDeleted == false).ToListAsync();
        //        UserMaster GroupAdminUser = null;
        //        string GroupCode = string.Empty;
        //        if (nodalCourseRequests.Where(x => x.RequestType == "Group").Count() > 0)
        //        {
        //            GroupAdminUser = await _db.UserMaster.Where(x => x.Id == nodalCourseRequests.Select(x => x.CreatedBy).FirstOrDefault() && x.IsDeleted == false).FirstOrDefaultAsync();
        //            GroupCode = await _db.NodalUserGroups.Where(x => x.Id == nodalCourseRequests.Select(x => x.GroupId).FirstOrDefault() && x.IsDeleted == false).Select(x => x.GroupCode).FirstOrDefaultAsync();
        //        }
        //        List<int> UserIds = nodalCourseRequests.Select(x => x.UserId).ToList();
        //        List<UserMaster> users = await _db.UserMaster.Where(x => UserIds.Contains(x.Id) && x.IsDeleted == false).ToListAsync();
        //        Model.Course course = await _db.Course.Where(x => x.Id == nodalCourseRequests.Select(x => x.CourseId).FirstOrDefault() && x.IsDeleted == false).FirstOrDefaultAsync();
        //        List<APINodalRequestList> aPINodalRequestList = new List<APINodalRequestList>();
        //        List<NodalCourseRequests> nodalCourseRequestsList = new List<NodalCourseRequests>();
        //        foreach (APINodalRequest request in aPINodalRequests)
        //        {
        //            NodalCourseRequests nodalCourseRequests1 = nodalCourseRequests.Where(x => x.Id == request.Id).FirstOrDefault();
        //            APINodalRequestList aPINodalRequest = new APINodalRequestList();
        //            aPINodalRequest.Id = request.Id;
        //            aPINodalRequest.CourseTitle = course.Title;
        //            aPINodalRequest.OrgCode = OrgCode;
        //            aPINodalRequest.RequestType = nodalCourseRequests1.RequestType;
        //            aPINodalRequest.GroupCode = GroupCode;
        //            aPINodalRequest.Reason = request.Reason;
        //            if (GroupAdminUser != null)
        //            {
        //                aPINodalRequest.GA_Id = GroupAdminUser.Id;
        //                aPINodalRequest.GA_Name = GroupAdminUser.UserName;
        //                aPINodalRequest.GA_EmailId = GroupAdminUser.EmailId;
        //            }
        //            var userDetails = users.Where(x => x.Id == nodalCourseRequests1.UserId).Select(d => new { UserMasterId = d.Id, UserId = d.UserId, UserName = d.UserName, EmailId = d.EmailId }).FirstOrDefault();

        //            aPINodalRequest.UserMasterId = userDetails.UserMasterId;
        //            aPINodalRequest.UserId = Security.Decrypt(userDetails.UserId);
        //            aPINodalRequest.UserName = userDetails.UserName;
        //            aPINodalRequest.EmailId = userDetails.EmailId;
        //            aPINodalRequest.IsApprove = request.IsApprovedByNodal;

        //            if (nodalCourseRequests1.IsPaymentDone == true && request.IsApprovedByNodal == false)
        //            {
        //                aPINodalRequest.Status = "Rejected";
        //                aPINodalRequest.ErrorMessage = "Cannot reject as payment is made.";
        //            }
        //            else if (nodalCourseRequests1.IsApprovedByNodal == request.IsApprovedByNodal)
        //            {
        //                aPINodalRequest.Status = "Duplicate";
        //                string status = Convert.ToBoolean(nodalCourseRequests1.IsApprovedByNodal) ? "Approved" : "Rejected";
        //                aPINodalRequest.ErrorMessage = "Course Request Already " + status + ".";
        //            }
        //            else
        //            {
        //                List<NodalCourseRequests> otherNodalCourseRequestsExists = await _db.NodalCourseRequests.Where(x => x.IsDeleted==false && x.CourseId == nodalCourseRequests1.CourseId && x.UserId == nodalCourseRequests1.UserId && x.Id != nodalCourseRequests1.Id && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
        //                if (otherNodalCourseRequestsExists.Count > 0)
        //                {
        //                    aPINodalRequest.Status = "Rejected";
        //                    aPINodalRequest.ErrorMessage = "Cannot approve as other pending/approved request for same course exist.";
        //                }
        //                else
        //                {
        //                    aPINodalRequest.Status = "Inserted";

        //                    nodalCourseRequests1.IsApprovedByNodal = request.IsApprovedByNodal;
        //                    nodalCourseRequests1.Reason = request.Reason;
        //                    nodalCourseRequests1.IsAccepted = true;
        //                    nodalCourseRequestsList.Add(nodalCourseRequests1);
        //                } 
        //            }
        //            aPINodalRequestList.Add(aPINodalRequest);
        //        }
        //        if (nodalCourseRequestsList.Count > 0)
        //        {
        //            _db.NodalCourseRequests.UpdateRange(nodalCourseRequestsList);
        //            await _db.SaveChangesAsync();
        //        }
        //        //Send Course Approval Notification
        //        if (aPINodalRequestList.Where(x => x.IsApprove == true && x.Status == "Inserted").Count() > 0)
        //        {
        //            List<APINodalRequestList> approvedRequestList = aPINodalRequestList.Where(x => x.IsApprove == true && x.Status == "Inserted").ToList();
        //            string ApprovedUserIds = string.Join(",", approvedRequestList.Select(x => x.UserMasterId));
        //            string PaymentUrl = string.Format("{0}{1}?q={2}&u={3}", _configuration["ApiGatewayUrl"], "Payment", HttpUtility.UrlEncode(Security.Encrypt(nodalCourseRequests.Select(x => x.GroupId).FirstOrDefault().ToString())), HttpUtility.UrlEncode(Security.Encrypt(ApprovedUserIds)));
        //            approvedRequestList.ForEach(x => { x.PaymentUrl = PaymentUrl; });
        //            _email.SendCourseRequestApprovalMailToUsers(approvedRequestList);
        //        }
        //        if (aPINodalRequestList.Where(x => x.IsApprove == false && x.Status == "Inserted").Count() > 0)
        //        {
        //            List<APINodalRequestList> rejectedRequestList = aPINodalRequestList.Where(x => x.IsApprove == false && x.Status == "Inserted").ToList();
        //            _email.SendCourseRequestRejectedMailToUsers(rejectedRequestList);
        //        }

        //        int TotalInserted = aPINodalRequestList.Where(x => x.Status == "Inserted").Count();
        //        int TotalRejected = aPINodalRequestList.Where(x => x.Status != "Inserted").Count();
        //        return new APINodalRequestResponse
        //        {
        //            StatusCode = TotalRejected > 0 ? 400 : 200,
        //            Description = "Total records inserted: " + TotalInserted + ", Total records rejected: " + TotalRejected + ".",
        //            aPINodalRequestList = aPINodalRequestList
        //        };
        //    }
        //    else
        //    {
        //        return new APINodalRequestResponse
        //        {
        //            StatusCode = 400,
        //            Description = "Please accept terms before approval or rejection of requests.",
        //            aPINodalRequestList = null
        //        };
        //    }
        //}
        public async Task<APIScormGroup> GetUserforCompletion(int GroupId)
        {
            var result = (from groups in _db.NodalUserGroups
                          join requests in _db.NodalCourseRequests on groups.Id equals requests.GroupId
                          join coursestatus in _db.CourseCompletionStatus on new { requests.UserId, requests.CourseId } equals new { coursestatus.UserId, coursestatus.CourseId } into CourseGroup
                          from CStatus in CourseGroup.DefaultIfEmpty()
                          where groups.IsDeleted == false
                          && requests.IsDeleted == false
                          && requests.IsApprovedByNodal == true
                          //&& requests.IsPaymentDone == true
                          && requests.GroupId == GroupId
                          && requests.Status == NodalCourseStatus.Inprogress
                          orderby requests.Id ascending
                          select new APIScormGroup
                          {
                              UserId = requests.UserId,
                              GroupId = (int)requests.GroupId,
                              RequestId = requests.Id,
                              Status = requests.Status=="inprogress" && CStatus.Status == null ? null : requests.Status
                          });

            APIScormGroup aPIScormGroup = await result.FirstOrDefaultAsync();
            if (aPIScormGroup != null)
                return aPIScormGroup;
            else
            {
                var completedresult = (from groups in _db.NodalUserGroups
                              join requests in _db.NodalCourseRequests on groups.Id equals requests.GroupId
                              where groups.IsDeleted == false
                              && requests.IsDeleted == false
                              && requests.IsApprovedByNodal == true
                              //&& requests.IsPaymentDone == true
                              && requests.GroupId == GroupId
                              && requests.Status == NodalCourseStatus.Completed
                              orderby requests.Id ascending
                              select new APIScormGroup
                              {
                                  UserId = requests.UserId,
                                  GroupId = (int)requests.GroupId,
                                  RequestId = requests.Id,
                                  Status = requests.Status
                              });

                return await completedresult.FirstOrDefaultAsync();
            }
        }
        //public async Task<string> DeleteCourseRequest(int UserId, APICourseRequestDelete aPICourseRequestDelete)
        //{
        //    NodalCourseRequests nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.Id == aPICourseRequestDelete.Id && x.IsDeleted == false).FirstOrDefaultAsync();
        //    if (nodalCourseRequests != null)
        //    {
        //        if (nodalCourseRequests.IsApprovedByNodal != null)
        //            return "Cannot delete as request is already " + (nodalCourseRequests.IsApprovedByNodal == true ? "approved." : "rejected.");
        //        else
        //        {
        //            nodalCourseRequests.IsDeleted = true;
        //            nodalCourseRequests.ModifiedBy = UserId;
        //            nodalCourseRequests.ModifiedDate = DateTime.UtcNow;

        //            _db.NodalCourseRequests.Update(nodalCourseRequests);
        //            await _db.SaveChangesAsync();

        //            return "Success";
        //        }
        //    }
        //    else
        //        return "Course request not exists.";
        //}
        //public async Task<string> SelfRegisterCourse(int UserId, string OrgCode, APISelfCourseRequest aPISelfCourseRequest)
        //{
        //    NodalCourseRequests nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.CourseId == aPISelfCourseRequest.CourseId && x.UserId == UserId && x.IsDeleted == false).FirstOrDefaultAsync();
        //    if (nodalCourseRequests == null)
        //    {
        //        var courses = (from cm in _db.CourseModuleAssociation
        //                       join m in _db.Module on cm.ModuleId equals m.Id
        //                       where cm.CourseId == aPISelfCourseRequest.CourseId
        //                       && cm.Isdeleted == false && m.IsDeleted == false
        //                       && m.ModuleType == "SCORM"
        //                       select new { cm.Id });
        //        if (await courses.CountAsync()>0)
        //        {
        //            var CourseInfo = await _db.Course.Where(x => x.Id == aPISelfCourseRequest.CourseId && x.IsDeleted == false).Select(x => new { Title = x.Title, CourseFee = x.CourseFee }).FirstOrDefaultAsync();

        //            NodalCourseRequests nodalCourseRequests1 = new NodalCourseRequests();
        //            nodalCourseRequests1.CourseId = aPISelfCourseRequest.CourseId;
        //            nodalCourseRequests1.UserId = UserId;
        //            nodalCourseRequests1.RequestType = "Individual";
        //            nodalCourseRequests1.IsPaymentDone = false;
        //            nodalCourseRequests1.PaymentUrl = null;
        //            nodalCourseRequests1.GroupId = 0;
        //            nodalCourseRequests1.CreatedBy = nodalCourseRequests1.ModifiedBy = UserId;
        //            nodalCourseRequests1.CreatedDate = nodalCourseRequests1.ModifiedDate = DateTime.UtcNow;
        //            nodalCourseRequests1.CourseFee = CourseInfo.CourseFee;
        //            _db.NodalCourseRequests.Add(nodalCourseRequests1);
        //            await _db.SaveChangesAsync();

        //            var userDetails = await (from user in _db.UserMaster
        //                                     join userdetails in _db.UserMasterDetails on user.Id equals userdetails.UserMasterId
        //                                     where user.IsDeleted == false && userdetails.IsDeleted == false && user.Id == UserId
        //                                     select new
        //                                     {
        //                                         UserMasterId = user.Id,
        //                                         EmailID = user.EmailId,
        //                                         UserName = user.UserName,
        //                                         Config12Id = userdetails.ConfigurationColumn12,
        //                                         UserId = user.UserId,
        //                                         MobileNumber = userdetails.MobileNumber
        //                                     }).FirstOrDefaultAsync();

                    

        //            APISelfCourseRequestEmail aPISelfCourseRequestEmail = new APISelfCourseRequestEmail()
        //            {
        //                UserMasterId = userDetails.UserMasterId,
        //                UserName = userDetails.UserName,
        //                EmailID = userDetails.EmailID,
        //                CourseTitle = CourseInfo.Title,
        //                OrgCode = OrgCode
        //            };

        //            APINodalUser Apiuser = new APINodalUser();
        //            Apiuser.Id = userDetails.UserMasterId;
        //            Apiuser.UserId = Security.Decrypt(userDetails.UserId);
        //            Apiuser.UserName = userDetails.UserName;
        //            Apiuser.EmailId = Security.Decrypt(userDetails.EmailID);
        //            Apiuser.MobileNumber = Security.Decrypt(userDetails.MobileNumber);
        //            Apiuser.ConfigurationColumn12Id = userDetails.Config12Id;
        //            Apiuser.OrganizationCode = OrgCode;
        //            Apiuser.Title = CourseInfo.Title;

        //            await _email.SendSelfCourseRequestMail(aPISelfCourseRequestEmail);

        //            await this.SendEmailAfterAddingUser(Apiuser);

        //            return "Success";
        //        }
        //        else
        //            return "You can request only SCORM type courses.";
        //    }
        //    else
        //    {
        //        if (nodalCourseRequests.IsApprovedByNodal==false)
        //        {
        //            var courses = (from cm in _db.CourseModuleAssociation
        //                           join m in _db.Module on cm.ModuleId equals m.Id
        //                           where cm.CourseId == aPISelfCourseRequest.CourseId
        //                           && cm.Isdeleted == false && m.IsDeleted == false
        //                           && m.ModuleType == "SCORM"
        //                           select cm.Id);
        //            if (await courses.CountAsync() > 0)
        //            {
        //                var CourseInfo = await _db.Course.Where(x => x.Id == aPISelfCourseRequest.CourseId && x.IsDeleted == false).Select(x => new { Title = x.Title, CourseFee = x.CourseFee }).FirstOrDefaultAsync();

        //                NodalCourseRequests nodalCourseRequests1 = new NodalCourseRequests();
        //                nodalCourseRequests1.CourseId = aPISelfCourseRequest.CourseId;
        //                nodalCourseRequests1.UserId = UserId;
        //                nodalCourseRequests1.RequestType = "Individual";
        //                nodalCourseRequests1.IsPaymentDone = false;
        //                nodalCourseRequests1.PaymentUrl = null;
        //                nodalCourseRequests1.GroupId = 0;
        //                nodalCourseRequests1.CreatedBy = nodalCourseRequests1.ModifiedBy = UserId;
        //                nodalCourseRequests1.CreatedDate = nodalCourseRequests1.ModifiedDate = DateTime.UtcNow;
        //                nodalCourseRequests1.CourseFee = CourseInfo.CourseFee;
        //                _db.NodalCourseRequests.Add(nodalCourseRequests1);
        //                await _db.SaveChangesAsync();

        //                var userDetails = await (from user in _db.UserMaster
        //                                    join userdetails in _db.UserMasterDetails on user.Id equals userdetails.UserMasterId
        //                                    where user.IsDeleted == false && userdetails.IsDeleted == false && user.Id == UserId
        //                                    select new
        //                                    {
        //                                        UserMasterId = user.Id,
        //                                        EmailID = user.EmailId,
        //                                        UserName = user.UserName,
        //                                        Config12Id = userdetails.ConfigurationColumn12,
        //                                        UserId = user.UserId,
        //                                        MobileNumber = userdetails.MobileNumber
        //                                    }).FirstOrDefaultAsync();

        //                APISelfCourseRequestEmail aPISelfCourseRequestEmail = new APISelfCourseRequestEmail()
        //                {
        //                    UserMasterId = userDetails.UserMasterId,
        //                    UserName = userDetails.UserName,
        //                    EmailID = userDetails.EmailID,
        //                    CourseTitle = CourseInfo.Title,
        //                    OrgCode = OrgCode
        //                };
        //                APINodalUser Apiuser = new APINodalUser();
        //                Apiuser.Id = userDetails.UserMasterId;
        //                Apiuser.UserId = Security.Decrypt(userDetails.UserId);
        //                Apiuser.UserName = userDetails.UserName;
        //                Apiuser.EmailId = Security.Decrypt(userDetails.EmailID);
        //                Apiuser.MobileNumber = Security.Decrypt(userDetails.MobileNumber);
        //                Apiuser.ConfigurationColumn12Id = userDetails.Config12Id;
        //                Apiuser.OrganizationCode = OrgCode;
        //                Apiuser.Title = CourseInfo.Title;

        //                await _email.SendSelfCourseRequestMail(aPISelfCourseRequestEmail);

        //                await this.SendEmailAfterAddingUser(Apiuser);

        //                return "Success";
        //            }
        //            else
        //                return "You can request only SCORM type courses.";
        //        }
        //        else
        //            return "Already requested for this course.";
        //    }
        //}
        //private async Task<int> SendEmailAfterAddingUser(APINodalUser Apiuser)
        //{
        //    var result = (from um in _db.UserMaster
        //                  join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
        //                  where
        //                    um.IsDeleted == false && um.IsActive == true
        //                    && umd.ConfigurationColumn12 == Apiuser.ConfigurationColumn12Id
        //                    && (um.UserRole == "NO" || um.UserRole == "CA")
        //                  select new APINodalUserDetailsEmail
        //                  {
        //                      NodalUserId = um.Id,
        //                      NodalUserName = um.UserName,
        //                      NodalEmailID = um.EmailId,
        //                      NodalMobileNumber = umd.MobileNumber,
        //                      CourseTitle = Apiuser.Title
        //                  });
        //    List<APINodalUserDetailsEmail> aPINodalUserDetailsList = await result.ToListAsync();
        //    aPINodalUserDetailsList.ForEach(x =>
        //    {
        //        x.UserMasterId = Apiuser.Id;
        //        x.UserId = Apiuser.UserId;
        //        x.UserName = Apiuser.UserName;
        //        x.EmailID = Apiuser.EmailId;
        //        x.MobileNumber = Apiuser.MobileNumber;
        //        x.AirPort = Apiuser.ConfigurationColumn12;
        //        x.RegistrationDate = DateTime.UtcNow;
        //        x.OrgCode = Apiuser.OrganizationCode;
        //    });

        //    _email.UserSignUpMailToNodalOfficers(aPINodalUserDetailsList);

        //    return 1;
        //}
        //public async Task<string> InitCourse(int UserId, int CourseId, int GroupId)
        //{
        //    var result = (from groups in _db.NodalUserGroups
        //                  join requests in _db.NodalCourseRequests on groups.Id equals requests.GroupId
        //                  where groups.IsDeleted == false && requests.IsDeleted == false
        //                  && requests.GroupId == GroupId && requests.CourseId == CourseId
        //                  orderby requests.Id ascending
        //                  select requests);
        //    List<NodalCourseRequests> nodalCourseRequestsList = await result.ToListAsync();

        //    if (nodalCourseRequestsList!=null && nodalCourseRequestsList.Count > 0)
        //    {
        //        List<NodalCourseRequests> nodalApprovedCourseRequestsList = nodalCourseRequestsList.Where(x => x.IsApprovedByNodal == true).ToList();
        //        if(nodalApprovedCourseRequestsList!=null && nodalApprovedCourseRequestsList.Count > 0)
        //        {
        //            List<NodalCourseRequests> nodalPaidCourseRequestsList = nodalCourseRequestsList.Where(x => x.IsApprovedByNodal == true && x.IsPaymentDone == true).ToList();
        //            if (nodalPaidCourseRequestsList != null && nodalPaidCourseRequestsList.Count > 0)
        //            {
        //                NodalCourseRequests nodalCourseRequestsInprogress = nodalCourseRequestsList.Where(x => x.IsApprovedByNodal == true && x.IsPaymentDone == true && x.Status == NodalCourseStatus.Inprogress).OrderBy(x => x.Id).FirstOrDefault();
        //                if (nodalCourseRequestsInprogress != null)
        //                    return "Success";
        //                else
        //                {
        //                    NodalCourseRequests nodalCourseRequests = nodalCourseRequestsList.Where(x => x.Status == null).OrderBy(x => x.Id).FirstOrDefault();
        //                    if (nodalCourseRequests != null)
        //                    {
        //                        nodalCourseRequests.Status = NodalCourseStatus.Inprogress;
        //                        nodalCourseRequests.ModifiedBy = UserId;
        //                        nodalCourseRequests.ModifiedDate = DateTime.UtcNow;

        //                        _db.NodalCourseRequests.Update(nodalCourseRequests);
        //                        await _db.SaveChangesAsync();
        //                        return "Success";
        //                    }
        //                    else
        //                        return "Course already completed for this group.";
        //                }
        //            }
        //            else
        //                return "No paid course requests found for this group.";
        //        }
        //        else
        //            return "No approved course requests found for this group.";
        //    }
        //    else
        //        return "No course requests found for this group.";
            
        //}
        //public async Task<string> GroupCourseCompletion(int UserId, List<APIGroupCourseCompletion> aPIGroupCourseCompletion)
        //{
        //    int GroupId = aPIGroupCourseCompletion.Select(x => x.GroupId).FirstOrDefault();
        //    int CourseId = aPIGroupCourseCompletion.Select(x => x.CourseId).FirstOrDefault();
        //    List<int> UserIds = aPIGroupCourseCompletion.Select(x => x.UserIds).ToList();
        //    List<NodalCourseRequests> nodalCourseRequestsList = await _db.NodalCourseRequests.Where(x => x.CourseId == CourseId
        //                                                                 && x.GroupId == GroupId
        //                                                                 && x.IsApprovedByNodal == true && x.IsDeleted == false).ToListAsync();

        //    List<NodalCourseRequests> nodalCourseRequestsCompletionList = nodalCourseRequestsList.Where(x => UserIds.Contains(x.UserId)).ToList();
        //    NodalCourseRequests nodalCourseRequests = nodalCourseRequestsList.Where(x => x.Status == NodalCourseStatus.Inprogress).FirstOrDefault();
        //    if (nodalCourseRequests != null)
        //    {
        //        List<ContentCompletionStatus> contentCompletionStatusList = await _db.ContentCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false && x.Status == NodalCourseStatus.Completed).ToListAsync();
        //        List<ModuleCompletionStatus> moduleCompletionStatusList = await _db.ModuleCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false && x.Status == NodalCourseStatus.Completed).ToListAsync();
        //        CourseCompletionStatus courseCompletionStatus = await _db.CourseCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false && x.Status == NodalCourseStatus.Completed).FirstOrDefaultAsync();

        //        List<ContentCompletionStatus> contentCompletionStatusCompletionList = new List<ContentCompletionStatus>();
        //        List<ModuleCompletionStatus> moduleCompletionStatusCompletionList = new List<ModuleCompletionStatus>();
        //        List<CourseCompletionStatus> courseCompletionStatusCompletionList = new List<CourseCompletionStatus>();

        //        List<ContentCompletionStatus> contentCompletionStatusExistsList = await _db.ContentCompletionStatus.Where(x => UserIds.Contains(x.UserId)
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();
        //        List<ModuleCompletionStatus> moduleCompletionStatusExistsList = await _db.ModuleCompletionStatus.Where(x => UserIds.Contains(x.UserId)
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();
        //        List<CourseCompletionStatus> courseCompletionStatusExistsList = await _db.CourseCompletionStatus.Where(x => UserIds.Contains(x.UserId)
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();

        //        if (contentCompletionStatusList.Count > 0 && moduleCompletionStatusList.Count > 0 && courseCompletionStatus != null)
        //        {
        //            foreach (NodalCourseRequests user in nodalCourseRequestsCompletionList)
        //            {
        //                foreach (ContentCompletionStatus content in contentCompletionStatusList)
        //                {
        //                    ContentCompletionStatus contentCompletionStatus = contentCompletionStatusExistsList.Where(x => x.ModuleId == content.ModuleId && x.CourseId == content.CourseId && x.UserId == user.UserId && x.IsDeleted == false).FirstOrDefault();
        //                    if (contentCompletionStatus == null)
        //                    {
        //                        ContentCompletionStatus contentCompletion = new ContentCompletionStatus();
        //                        contentCompletion.CourseId = content.CourseId;
        //                        contentCompletion.ModuleId = content.ModuleId;
        //                        contentCompletion.CreatedDate = content.CreatedDate;
        //                        contentCompletion.ModifiedDate = content.ModifiedDate;
        //                        contentCompletion.UserId = user.UserId;
        //                        contentCompletion.Status = content.Status;
        //                        contentCompletionStatusCompletionList.Add(contentCompletion);
        //                    }
        //                }

        //                foreach (ModuleCompletionStatus module in moduleCompletionStatusList)
        //                {
        //                    ModuleCompletionStatus moduleCompletionStatus = moduleCompletionStatusExistsList.Where(x => x.ModuleId == module.ModuleId && x.CourseId == module.CourseId && x.UserId == user.UserId && x.IsDeleted == false).FirstOrDefault();
        //                    if (moduleCompletionStatus == null)
        //                    {
        //                        ModuleCompletionStatus moduleCompletion = new ModuleCompletionStatus();
        //                        moduleCompletion.CourseId = module.CourseId;
        //                        moduleCompletion.ModuleId = module.ModuleId;
        //                        moduleCompletion.CreatedDate = module.CreatedDate;
        //                        moduleCompletion.ModifiedDate = module.ModifiedDate;
        //                        moduleCompletion.UserId = user.UserId;
        //                        moduleCompletion.Status = module.Status;
        //                        moduleCompletionStatusCompletionList.Add(moduleCompletion);
        //                    }
        //                }

        //                CourseCompletionStatus courseCompletionStatus1 = courseCompletionStatusExistsList.Where(x => x.CourseId == CourseId && x.UserId == user.UserId && x.IsDeleted == false).FirstOrDefault();
        //                if (courseCompletionStatus1 == null)
        //                {
        //                    CourseCompletionStatus courseCompletion = new CourseCompletionStatus();
        //                    courseCompletion.CourseId = courseCompletionStatus.CourseId;
        //                    courseCompletion.UserId = user.UserId;
        //                    courseCompletion.Status = courseCompletionStatus.Status;
        //                    courseCompletion.CreatedDate = courseCompletionStatus.CreatedDate;
        //                    courseCompletion.ModifiedDate = courseCompletionStatus.ModifiedDate;
        //                    courseCompletionStatusCompletionList.Add(courseCompletion);
        //                }
        //            }
        //            if (contentCompletionStatusCompletionList.Count > 0)
        //            {
        //                _db.ContentCompletionStatus.AddRange(contentCompletionStatusCompletionList);
        //                await _db.SaveChangesAsync();
        //            }
        //            if (moduleCompletionStatusCompletionList.Count > 0)
        //            {
        //                _db.ModuleCompletionStatus.AddRange(moduleCompletionStatusCompletionList);
        //                await _db.SaveChangesAsync();
        //            }
        //            if (courseCompletionStatusCompletionList.Count > 0)
        //            {
        //                _db.CourseCompletionStatus.AddRange(courseCompletionStatusCompletionList);
        //                await _db.SaveChangesAsync();
        //            }

        //            foreach (NodalCourseRequests requests in nodalCourseRequestsCompletionList)
        //                requests.Status = NodalCourseStatus.Completed;

        //            _db.NodalCourseRequests.UpdateRange(nodalCourseRequestsCompletionList);
        //            await _db.SaveChangesAsync();

        //            List<ContentCompletionStatus> contentCompletionStatusRemovalList = new List<ContentCompletionStatus>();
        //            List<ModuleCompletionStatus> moduleCompletionStatusRemovalList = new List<ModuleCompletionStatus>();
        //            List<CourseCompletionStatus> courseCompletionStatusRemovalList = new List<CourseCompletionStatus>();
        //            if (!UserIds.Contains(nodalCourseRequests.UserId))
        //            {

        //                List<ContentCompletionStatus> contentCompletionStatusRemovalExistsList = await _db.ContentCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();
        //                List<ModuleCompletionStatus> moduleCompletionStatusRemovalExistsList = await _db.ModuleCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                        && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();
        //                List<CourseCompletionStatus> courseCompletionStatusRemovalExistsList = await _db.CourseCompletionStatus.Where(x => x.UserId == nodalCourseRequests.UserId
        //                                                                        && x.CourseId == CourseId && x.IsDeleted == false).ToListAsync();

        //                int UserIdToReplace = UserIds.FirstOrDefault();
        //                foreach (ContentCompletionStatus content in contentCompletionStatusList)
        //                {
        //                    ContentCompletionStatus contentCompletionStatus = contentCompletionStatusRemovalExistsList.Where(x => x.ModuleId == content.ModuleId && x.CourseId == content.CourseId && x.UserId == nodalCourseRequests.UserId && x.IsDeleted == false).FirstOrDefault();
        //                    if (contentCompletionStatus != null)
        //                    {
        //                        contentCompletionStatus.IsDeleted = true;
        //                        contentCompletionStatus.ModifiedDate = DateTime.UtcNow;
        //                        contentCompletionStatusRemovalList.Add(contentCompletionStatus);
        //                    }
        //                }

        //                foreach (ModuleCompletionStatus module in moduleCompletionStatusList)
        //                {
        //                    ModuleCompletionStatus moduleCompletionStatus = moduleCompletionStatusRemovalExistsList.Where(x => x.ModuleId == module.ModuleId && x.CourseId == module.CourseId && x.UserId == nodalCourseRequests.UserId && x.IsDeleted == false).FirstOrDefault();
        //                    if (moduleCompletionStatus != null)
        //                    {
        //                        moduleCompletionStatus.IsDeleted = true;
        //                        moduleCompletionStatus.ModifiedDate = DateTime.UtcNow;
        //                        moduleCompletionStatusRemovalList.Add(moduleCompletionStatus);
        //                    }
        //                }

        //                CourseCompletionStatus courseCompletionStatus1 = courseCompletionStatusRemovalExistsList.Where(x => x.CourseId == CourseId && x.UserId == nodalCourseRequests.UserId && x.IsDeleted == false).FirstOrDefault();
        //                if (courseCompletionStatus1 != null)
        //                {
        //                    courseCompletionStatus1.IsDeleted = true;
        //                    courseCompletionStatus1.ModifiedDate = DateTime.UtcNow;
        //                    courseCompletionStatusRemovalList.Add(courseCompletionStatus1);
        //                }

        //                if (contentCompletionStatusRemovalList.Count > 0)
        //                {
        //                    _db.ContentCompletionStatus.RemoveRange(contentCompletionStatusRemovalList);
        //                    await _db.SaveChangesAsync();
        //                }
        //                if (moduleCompletionStatusRemovalList.Count > 0)
        //                {
        //                    _db.ModuleCompletionStatus.RemoveRange(moduleCompletionStatusRemovalList);
        //                    await _db.SaveChangesAsync();
        //                }
        //                if (courseCompletionStatusRemovalList.Count > 0)
        //                {
        //                    _db.CourseCompletionStatus.RemoveRange(courseCompletionStatusRemovalList);
        //                    await _db.SaveChangesAsync();
        //                }

        //                List<ScormVars> scormVars = await _db.ScormVars.Where(x => x.CourseId == nodalCourseRequests.CourseId && x.UserId == nodalCourseRequests.UserId && x.IsDeleted == false).ToListAsync();
        //                foreach (ScormVars item in scormVars)
        //                {
        //                    item.UserId = UserIdToReplace;
        //                    item.ModifiedDate = DateTime.UtcNow;
        //                }
        //                _db.ScormVars.UpdateRange(scormVars);
        //                await _db.SaveChangesAsync();

        //                List<ScormVarResult> scormVarResults = await _db.ScormVarResult.Where(x => x.CourseId == nodalCourseRequests.CourseId && x.UserId == nodalCourseRequests.UserId && x.IsDeleted == false).ToListAsync();
        //                foreach (ScormVarResult item in scormVarResults)
        //                {
        //                    item.UserId = UserIdToReplace;
        //                    item.ModifiedDate = DateTime.UtcNow;
        //                }
        //                _db.ScormVarResult.UpdateRange(scormVarResults);
        //                await _db.SaveChangesAsync();

        //                nodalCourseRequests.Status = null;
        //                _db.NodalCourseRequests.Update(nodalCourseRequests);
        //                await _db.SaveChangesAsync();
        //            }

        //            return "Success";
        //        }
        //        else
        //            return "Course is not completed yet! Please complete course first inorder to mark group completion.";
        //    }
        //    else
        //        return "No inprogress course or is already completed for this group.";
        //}

        //public  string postPaymentRequestToGateway(String queryUrl, String urlParam)
        //{
        //    _logger.Info("in  postPaymentRequestToGateway");
        //    String message = "";
        //    try
        //    {
        //        StreamWriter myWriter = null;// it will open a http connection with provided url
        //        WebRequest objRequest = WebRequest.Create(queryUrl);//send data using objxmlhttp object
        //        objRequest.Method = "POST";
        //        //objRequest.ContentLength = TranRequest.Length;
        //        objRequest.ContentType = "application/x-www-form-urlencoded";//to set content type
        //        myWriter = new System.IO.StreamWriter(objRequest.GetRequestStream());
        //        myWriter.Write(urlParam);//send data
        //        myWriter.Close();//closed the myWriter object

        //        // Getting Response
        //        System.Net.HttpWebResponse objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();//receive the responce from objxmlhttp object 
        //        using (System.IO.StreamReader sr = new System.IO.StreamReader(objResponse.GetResponseStream()))
        //        {
        //            message = sr.ReadToEnd();
        //            //Response.Write(message);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.Write("Exception occured while connection." + exception);
        //    }
        //    return message;

        //}

        //public  NameValueCollection getResponseMap(String message)
        //{
        //    NameValueCollection Params = new NameValueCollection();
        //    if (message != null || !"".Equals(message))
        //    {
        //        string[] segments = message.Split('&');
        //        foreach (string seg in segments)
        //        {
        //            string[] parts = seg.Split('=');
        //            if (parts.Length > 0)
        //            {
        //                string Key = parts[0].Trim();
        //                string Value = parts[1].Trim();
        //                Params.Add(Key, Value);
        //            }
        //        }
        //    }
        //    return Params;
        //}

    }
}
