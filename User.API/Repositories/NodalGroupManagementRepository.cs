using AutoMapper;
using Dapper;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using static User.API.Models.UserMaster;

namespace User.API.Repositories
{
    public class NodalGroupManagementRepository : Repository<NodalUserGroups>, INodalGroupManagementRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NodalGroupManagementRepository));
        private UserDbContext _db;
        private IConfiguration _configuration;
        private IUserRepository _userRepository;
        private ICustomerConnectionStringRepository _customerConnection;
        private IEmail _email;
        public NodalGroupManagementRepository(UserDbContext context, IConfiguration configuration,
            IUserRepository userRepository, ICustomerConnectionStringRepository customerConnection,
            IEmail email):base(context)
        {
            _db = context;
            _configuration = configuration;
            _userRepository = userRepository;
            _customerConnection = customerConnection;
            _email = email;
        }
        public async Task<GroupCode> GetGroupCode(int UserId)
        {
            int? groupId = _db.GroupCode.Max(u => (int?)u.Id);
            if (groupId != null)
            {
                GroupCode groupCode = await _db.GroupCode.Where(f => f.Id == groupId).FirstOrDefaultAsync();
                if (groupCode != null)
                {
                    if (groupCode.IsDeleted == true && groupCode.UserId == UserId)
                    {
                        string gpCode = "GROUP" + Convert.ToString(groupCode.Id);
                        NodalUserGroups nodalUserGroups = await _db.NodalUserGroups.Where(f => f.GroupCode == gpCode).FirstOrDefaultAsync();
                        if (nodalUserGroups != null)
                        {
                            GroupCode groupCode1 = new GroupCode();
                            groupCode1.IsDeleted = false;
                            groupCode1.UserId = UserId;
                            _db.GroupCode.Add(groupCode1);
                            await _db.SaveChangesAsync();
                            return groupCode1;
                        }
                        else
                        {
                            return groupCode;
                        }
                    }
                    else
                    {
                        GroupCode GroupCode = new GroupCode();
                        GroupCode.IsDeleted = false;
                        GroupCode.UserId = UserId;
                        _db.GroupCode.Add(GroupCode);
                        await _db.SaveChangesAsync();
                        return GroupCode;
                    }
                }
                else
                {
                    GroupCode GroupCode = new GroupCode();
                    GroupCode.IsDeleted = false;
                    GroupCode.UserId = UserId;
                    _db.GroupCode.Add(GroupCode);
                    await _db.SaveChangesAsync();
                    return GroupCode;
                }
            }
            else
            {
                GroupCode GroupCode = new GroupCode();
                GroupCode.IsDeleted = false;
                GroupCode.UserId = UserId;
                _db.GroupCode.Add(GroupCode);
                await _db.SaveChangesAsync();
                return GroupCode;
            }
        }

        public async Task CancelGroupCode(APIGroupCode aPIGroupCode, int UserId)
        {
            string Code = aPIGroupCode.GroupCode.Replace("GROUP", "");
            int GroupId = Convert.ToInt32(Code);
            GroupCode groupCode = await _db.GroupCode.Where(f => f.Id == GroupId).FirstOrDefaultAsync();
            groupCode.IsDeleted = true;
            groupCode.UserId = UserId;
            _db.GroupCode.Update(groupCode);
            await _db.SaveChangesAsync();
        }
        public async Task<List<APICourseGroups>> GetCourseGroups(int UserId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            List<APICourseGroups> aPICourseGroups = new List<APICourseGroups>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@Page", Page);
                    parameters.Add("@PageSize", PageSize);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync<APICourseGroups>((SqlConnection)connection, "[dbo].[GetCourseGroups]", parameters, null, null, CommandType.StoredProcedure);
                    aPICourseGroups = result.ToList();
                    connection.Close();
                }
            }
            return aPICourseGroups;
        }
        public async Task<int> GetCourseGroupsCount(int UserId, string Search = null, string SearchText = null)
        {
            int count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@Page", 1);
                    parameters.Add("@PageSize", 0);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetCourseGroups]", parameters, null, null, CommandType.StoredProcedure);
                    count = result.Select(x=>x.RecordCount).FirstOrDefault();
                    connection.Close();
                }
            }
            return count;
        }
        public async Task<List<APICourseGroupUsers>> GetCourseGroupUsers(int UserId,int GroupId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            if (Search!=null && SearchText!=null)
            {
                if (Search.ToLower().Equals("userid"))
                    SearchText = Security.Encrypt(SearchText);
            }

            List<APICourseGroupUsers> aPICourseGroupUsers = new List<APICourseGroupUsers>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@GroupId", GroupId);
                    parameters.Add("@Page", Page);
                    parameters.Add("@PageSize", PageSize);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync<APICourseGroupUsers>((SqlConnection)connection, "[dbo].[GetCourseGroupUsers]", parameters, null, null, CommandType.StoredProcedure);
                    aPICourseGroupUsers = result.ToList();
                    connection.Close();
                }
                aPICourseGroupUsers.ForEach(x =>
                {
                    x.UserId = Security.Decrypt(x.UserId);
                    x.EmailId = Security.Decrypt(x.EmailId);
                    x.MobileNumber = Security.Decrypt(x.MobileNumber);
                    x.AadharNumber = Security.Decrypt(x.AadharNumber);
                });
            }
            return aPICourseGroupUsers;
        }
        public async Task<int> GetCourseGroupUsersCount(int UserId, int GroupId, string Search = null, string SearchText = null)
        {
            if (Search != null && SearchText != null)
            {
                if (Search.ToLower().Equals("userid"))
                    SearchText = Security.Encrypt(SearchText);
            }
            int count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@GroupId", GroupId);
                    parameters.Add("@Page", 1);
                    parameters.Add("@PageSize", 0);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetCourseGroupUsers]", parameters, null, null, CommandType.StoredProcedure);
                    count = result.Select(x => x.RecordCount).FirstOrDefault();
                    connection.Close();
                }
            }
            return count;
        }
        public async Task<string> DeleteUser(APINodalUserDelete aPINodalUserDelete, int UserId)
        {
            NodalGroupsUsersMapping nodalGroupsUsersMapping = await (from nodalmap in _db.NodalGroupsUsersMapping
                          join nodalgroup in _db.NodalUserGroups on nodalmap.GroupId equals nodalgroup.Id
                          where nodalmap.Id == aPINodalUserDelete.GroupMapId && nodalmap.IsDeleted == false
                          select nodalmap).FirstOrDefaultAsync();
            if (nodalGroupsUsersMapping != null)
            {
                var requestexists = await _db.NodalCourseRequests.Where(x => x.UserId == nodalGroupsUsersMapping.UserId && x.GroupId == nodalGroupsUsersMapping.GroupId && x.IsDeleted == false && x.IsApprovedByNodal!=null).ToListAsync();
                if (requestexists != null && requestexists.Count > 0)
                    return "Cannot delete user as approved/rejected requests exists.";
                else
                {
                    List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.UserId == nodalGroupsUsersMapping.UserId && x.GroupId == nodalGroupsUsersMapping.GroupId && x.IsDeleted == false && x.IsApprovedByNodal == null).ToListAsync();
                    foreach (NodalCourseRequests item in nodalCourseRequests)
                    {
                        item.IsDeleted = true;
                        item.ModifiedBy = UserId;
                        item.ModifiedDate = DateTime.UtcNow;
                    }
                    _db.NodalCourseRequests.UpdateRange(nodalCourseRequests);
                    await _db.SaveChangesAsync();

                    nodalGroupsUsersMapping.IsDeleted = true;
                    nodalGroupsUsersMapping.ModifiedBy = UserId;
                    nodalGroupsUsersMapping.ModifiedDate = DateTime.UtcNow;

                    _db.NodalGroupsUsersMapping.Update(nodalGroupsUsersMapping);
                    await _db.SaveChangesAsync();

                    return "Success";
                }
            }
            else
                return "User not exists in this group.";
        }
        public async Task<string> DeleteGroup(APINodalGroupDelete aPINodalGroupDelete, int UserId)
        {
            NodalUserGroups nodalUserGroups = await _db.NodalUserGroups.Where(x => x.Id == aPINodalGroupDelete.GroupId && x.IsDeleted == false).FirstOrDefaultAsync();

            if (nodalUserGroups != null)
            {
                var requestexists = await _db.NodalCourseRequests.Where(x => x.GroupId == nodalUserGroups.Id && x.RequestType == NodalCourseRequest.Group.ToString() && x.IsDeleted == false && x.IsApprovedByNodal!=null).ToListAsync();
                if (requestexists != null && requestexists.Count > 0)
                    return "Cannot delete group as approved/rejected requests exists.";
                else
                {
                    List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.GroupId == nodalUserGroups.Id && x.RequestType == NodalCourseRequest.Group.ToString() && x.IsDeleted == false && x.IsApprovedByNodal == null).ToListAsync();
                    foreach (NodalCourseRequests item in nodalCourseRequests)
                    {
                        item.IsDeleted = true;
                        item.ModifiedBy = UserId;
                        item.ModifiedDate = DateTime.UtcNow;
                    }
                    _db.NodalCourseRequests.UpdateRange(nodalCourseRequests);
                    await _db.SaveChangesAsync();

                    nodalUserGroups.IsDeleted = true;
                    nodalUserGroups.ModifiedBy = UserId;
                    nodalUserGroups.ModifiedDate = DateTime.UtcNow;

                    _db.NodalUserGroups.Update(nodalUserGroups);
                    await _db.SaveChangesAsync();

                    return "Success";
                }
            }
            else
                return "Group not exists.";
        }
        public async Task<byte[]> ExportImportFormat(string OrgCode)
        {
            string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = APIFileName.GroupImportFormat;
            string DomainName = _configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Group Import");
                
                worksheet.Cells[1, 1].Value = "Full Name*";
                worksheet.Cells[1, 2].Value = "Father/Husband Name*";
                worksheet.Cells[1, 3].Value = "Date Of Birth*";
                worksheet.Cells[1, 4].Value = "Email Id*";
                worksheet.Cells[1, 5].Value = "Mobile Number*";
                worksheet.Cells[1, 6].Value = "Aadhar Number*";

                worksheet.Cells["C1:C3000"].Style.Numberformat.Format = "@";

                using (var rngitems = worksheet.Cells["A1:F1"])
                    rngitems.Style.Font.Bold = true;

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
        public async Task<ApiResponse> ProcessImportFile(APINodalUserGroups aPINodalUserGroups, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder);
            string filepath = sWebRootFolder + aPINodalUserGroups.Path;

            DataTable groupdt = ReadFile(filepath);
            if (groupdt == null || groupdt.Rows.Count == 0)
            {
                string resultstring = Record.FileDoesNotContainsData;
                return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
            }
            bool resultMessage = await ValidateFileColumnHeaders(groupdt);
            if (resultMessage)
            {
                return await ProcessRecordsAsync(aPINodalUserGroups, groupdt, UserId, OrgCode);
            }
            else
            {
                Response.StatusCode = 400;
                string resultstring = Record.FileInvalid;
                Response.ResponseObject = new { resultstring };
                return Response;
            }
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
                        dt.Columns.Add(firstRowCell.Text);
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
        public async Task<bool> ValidateFileColumnHeaders(DataTable groupdt)
        {
            List<string> importColumns = GetImportColumnsLength().Select(x=>x.Key).ToList();
            if (groupdt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < groupdt.Columns.Count; i++)
            {
                string col = groupdt.Columns[i].ColumnName.Replace("*", "").Replace("/", "").Replace(" ", "");
                groupdt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(groupdt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }
        private List<KeyValuePair<string, int>> GetImportColumnsLength()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.UserName, 400));
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.FatherHusbandName, 400));
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.DateOfBirth, 50));
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.EmailId, 2000));
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.MobileNumber, 2000));
            columns.Add(new KeyValuePair<string, int>(APINodalUserGroupImportColumns.AadharNumber, 2000));
            return columns;
        }
        public async Task<ApiResponse> ProcessRecordsAsync(APINodalUserGroups aPINodalUserGroups, DataTable nominationImportDt, int UserId, string OrgCode)
        {
            int? AirPortId = await _db.UserMasterDetails.Where(x => x.UserMasterId == UserId && x.IsDeleted == false).Select(x => x.ConfigurationColumn12).FirstOrDefaultAsync();
            if (AirPortId == null)
                return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring = "No Airport defined for this user." } };

            List<APINodalUserInfo> aPINodalUserInfosRejected = new List<APINodalUserInfo>();

            List<APINodalUserInfo> aPINodalUserInfos = nominationImportDt.ConvertToList<APINodalUserInfo>();
            string appDateFormat = await _userRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
            var courseInfo = await _db.Course.Where(x => x.IsDeleted == false && x.Id == aPINodalUserGroups.CourseId)
                            .Select(x => new { Code = x.Code, Title = x.Title, CourseFee = x.GroupCourseFee }).FirstOrDefaultAsync();
            foreach (APINodalUserInfo item in aPINodalUserInfos)
            {
                if(string.IsNullOrEmpty(item.FullName))
                {
                    item.ErrorMessage = "Full Name is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if (string.IsNullOrEmpty(item.FatherHusbandName))
                {
                    item.ErrorMessage = "Father/Husband Name is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if (string.IsNullOrEmpty(item.DateOfBirth))
                {
                    item.ErrorMessage = "Date Of Birth is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if (!string.IsNullOrEmpty(item.DateOfBirth))
                {
                    DateTime? dateOfBirth = ValidateDateOfBirth(item.DateOfBirth, appDateFormat);
                    if (dateOfBirth == null)
                    {
                        item.ErrorMessage = "Date of birth is not in " + appDateFormat + " format.";
                        aPINodalUserInfosRejected.Add(item);
                    }
                    else
                    {
                        if (((DateTime)dateOfBirth).Date <= DateTime.Now.AddYears(-10).Date)
                            item.DateOfBirth1 = ((DateTime)dateOfBirth).Date;
                        else
                        {
                            item.ErrorMessage = "Invalid Date of birth.";
                            aPINodalUserInfosRejected.Add(item);
                        }
                    }
                }
                else if (string.IsNullOrEmpty(item.EmailId))
                {
                    item.ErrorMessage = "Email Id is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if(string.IsNullOrEmpty(item.MobileNumber))
                {
                    item.ErrorMessage = "Mobile Number is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if (string.IsNullOrEmpty(item.AadharNumber))
                {
                    item.ErrorMessage = "Aadhar Number is required.";
                    aPINodalUserInfosRejected.Add(item);
                }
                else if (!string.IsNullOrEmpty(item.AadharNumber))
                {
                    if (!Regex.IsMatch(item.AadharNumber, "^(\\d{12})$"))
                    {
                        item.ErrorMessage = "Aadhar Number must be 12 digit number.";
                        aPINodalUserInfosRejected.Add(item);
                    }
                }
                
                item.AirPortId = (int)AirPortId;
                item.Code = courseInfo.Code;
                item.Title = courseInfo.Title;
            }

            List<APINodalUserInfo> aPINodalUserInfosCreated = await CreateNodalUser(UserId, aPINodalUserGroups.CourseId, OrgCode, aPINodalUserInfos.Where(x=>x.ErrorMessage==null).ToList(), aPINodalUserGroups.GroupCode);
            aPINodalUserInfosRejected.AddRange(aPINodalUserInfosCreated);
            if (aPINodalUserInfosCreated.Where(x=>x.ErrorMessage=="Success" || x.ErrorMessage == "Account Already Exists!").Count()>0)
            {
                List<APINodalUserInfo> checkRequestExists = aPINodalUserInfosCreated.Where(x => x.ErrorMessage == "Success" || x.ErrorMessage == "Account Already Exists!").ToList();
                foreach (APINodalUserInfo item in checkRequestExists)
                {
                    NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.CourseId == aPINodalUserGroups.CourseId && x.UserId == item.Id && x.IsDeleted == false && (x.IsApprovedByNodal==true || x.IsApprovedByNodal==null)).FirstOrDefaultAsync();
                    if (nodalCourseRequest != null)
                    {
                        aPINodalUserInfosCreated.Where(x => (x.ErrorMessage == "Success" || x.ErrorMessage == "Account Already Exists!") && x.Id == nodalCourseRequest.UserId).ToList().ForEach(x =>
                        {
                            x.ErrorMessage = nodalCourseRequest.IsApprovedByNodal==true ? "Already registered for this course." : "Already requested for this course.";
                        });
                    }
                }

                List<APINodalUserInfo> aPINodalUserInfos1 = aPINodalUserInfosCreated.Where(x => x.ErrorMessage == "Success" || x.ErrorMessage == "Account Already Exists!").ToList();
                List<NodalCourseRequests> addedNodalCourseRequestsList = new List<NodalCourseRequests>();
                NodalUserGroups nodalUserGroups = await _db.NodalUserGroups.Where(x => x.GroupCode == aPINodalUserGroups.GroupCode && x.IsDeleted == false).FirstOrDefaultAsync();
                if (nodalUserGroups == null)
                {
                    using (var transaction = this._context.Database.BeginTransaction())
                    {
                        try
                        {
                            NodalUserGroups nodalUserGroups1 = new NodalUserGroups();
                            nodalUserGroups1.GroupCode = aPINodalUserGroups.GroupCode;
                            nodalUserGroups1.CreatedBy = nodalUserGroups1.ModifiedBy = UserId;
                            nodalUserGroups1.CreatedDate = nodalUserGroups1.ModifiedDate = DateTime.UtcNow;
                            _db.Add(nodalUserGroups1);
                            await _db.SaveChangesAsync();

                            foreach (APINodalUserInfo item in aPINodalUserInfos1)
                            {
                                if (item.Id == 0 && item.ErrorMessage == "Account Already Exists!")
                                    item.ErrorMessage = "Account Already Exists! Information mismatched.";
                                else
                                {
                                    NodalGroupsUsersMapping mappingExists = await _db.NodalGroupsUsersMapping.Where(x => x.GroupId == nodalUserGroups1.Id && x.UserId == item.Id && x.IsDeleted == false).FirstOrDefaultAsync();
                                    if (mappingExists == null)
                                    {
                                        NodalGroupsUsersMapping mapping = new NodalGroupsUsersMapping();
                                        mapping.GroupId = nodalUserGroups1.Id;
                                        mapping.UserId = item.Id;
                                        mapping.CreatedBy = mapping.ModifiedBy = UserId;
                                        mapping.CreatedDate = mapping.ModifiedDate = DateTime.UtcNow;
                                        _db.NodalGroupsUsersMapping.Add(mapping);
                                        await _db.SaveChangesAsync();
                                    }

                                    NodalCourseRequests nodalCourseRequestExists = await _db.NodalCourseRequests.Where(x => x.UserId == item.Id && x.CourseId == aPINodalUserGroups.CourseId && (x.IsApprovedByNodal == null || x.IsApprovedByNodal == true) && x.IsDeleted == false).FirstOrDefaultAsync();
                                    //Add course request
                                    if (nodalCourseRequestExists == null)
                                    {
                                        NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
                                        nodalCourseRequest.CourseId = aPINodalUserGroups.CourseId;
                                        nodalCourseRequest.UserId = item.Id;
                                        nodalCourseRequest.RequestType = NodalCourseRequest.Group;
                                        nodalCourseRequest.GroupId = nodalUserGroups1.Id;
                                        nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = UserId;
                                        nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
                                        nodalCourseRequest.CourseFee = courseInfo.CourseFee;
                                        _db.NodalCourseRequests.Add(nodalCourseRequest);
                                        await _db.SaveChangesAsync();
                                        addedNodalCourseRequestsList.Add(nodalCourseRequest);
                                    }
                                    else
                                        item.ErrorMessage = "Already requested for this course";
                                }
                            }
                            if (addedNodalCourseRequestsList.Count > 0)
                                transaction.Commit();
                            else
                                transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.Error(Utilities.GetDetailedException(ex));
                            throw;
                        }
                    }
                }
                else
                {
                    foreach (APINodalUserInfo item in aPINodalUserInfos1)
                    {
                        if (item.Id == 0 && item.ErrorMessage == "Account Already Exists!")
                            item.ErrorMessage = "Account Already Exists! Information mismatched.";
                        else
                        {
                            NodalGroupsUsersMapping mapping = await _db.NodalGroupsUsersMapping.Where(x => x.GroupId == nodalUserGroups.Id && x.UserId == item.Id && x.IsDeleted == false).FirstOrDefaultAsync();
                            if (mapping == null)
                            {
                                NodalGroupsUsersMapping mapping1 = new NodalGroupsUsersMapping();
                                mapping1.GroupId = nodalUserGroups.Id;
                                mapping1.UserId = item.Id;
                                mapping1.CreatedBy = mapping1.ModifiedBy = UserId;
                                mapping1.CreatedDate = mapping1.ModifiedDate = DateTime.UtcNow;
                                _db.NodalGroupsUsersMapping.Add(mapping1);
                                await _db.SaveChangesAsync();
                            }

                            NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.UserId == item.Id && x.CourseId == aPINodalUserGroups.CourseId && (x.IsApprovedByNodal == null || x.IsApprovedByNodal == true) && x.IsDeleted == false).FirstOrDefaultAsync();
                            //Add course request
                            if (nodalCourseRequest == null)
                            {
                                NodalCourseRequests nodalCourseRequest1 = new NodalCourseRequests();
                                nodalCourseRequest1.CourseId = aPINodalUserGroups.CourseId;
                                nodalCourseRequest1.UserId = item.Id;
                                nodalCourseRequest1.RequestType = NodalCourseRequest.Group;
                                nodalCourseRequest1.GroupId = nodalUserGroups.Id;
                                nodalCourseRequest1.CreatedBy = nodalCourseRequest1.ModifiedBy = UserId;
                                nodalCourseRequest1.CreatedDate = nodalCourseRequest1.ModifiedDate = DateTime.UtcNow;
                                nodalCourseRequest1.CourseFee = courseInfo.CourseFee;
                                _db.NodalCourseRequests.Add(nodalCourseRequest1);
                                await _db.SaveChangesAsync();
                            }
                            else
                                item.ErrorMessage = "Already requested for this course";
                        }
                    }
                }
            }

            int totalInserted = aPINodalUserInfosRejected.Where(x => x.ErrorMessage == "Success" || x.ErrorMessage == "Account Already Exists!").Count();
            int totalRejected = aPINodalUserInfosRejected.Where(x => x.ErrorMessage != "Success" && x.ErrorMessage != "Account Already Exists!").Count();

            if (totalInserted>0)
            {
                this.SendEmailAfterCreatingGroup(aPINodalUserInfosRejected.Where(x => x.ErrorMessage == "Success" || x.ErrorMessage == "Account Already Exists!").ToList(), aPINodalUserGroups.GroupCode, OrgCode);
            }

            string resultstring = "Total number of record inserted :" + totalInserted + ",  Total number of record rejected : " + totalRejected;
            int TotalCount = totalInserted + totalRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, TotalCount, aPINodalUserInfosRejected = aPINodalUserInfosRejected.Where(x => x.ErrorMessage != "Success" && x.ErrorMessage != "Account Already Exists!").ToList() };
            return response;
        }
        private async Task<int> SendEmailAfterCreatingGroup(List<APINodalUserInfo> aPINodalUserInfos, string GroupCode, string OrgCode)
        {
            try
            {
                APIGroupEmails aPIGroupEmails = new APIGroupEmails();
                var Details = aPINodalUserInfos.Select(x => new { CoursTitle = x.Title, AirPortId = x.AirPortId }).FirstOrDefault();
                aPIGroupEmails.CourseTitle = Details.CoursTitle;
                aPIGroupEmails.GroupCode = GroupCode;
                aPIGroupEmails.OrgCode = OrgCode;
                List<APINodalUsers> aPINodalUsersList = await (from um in _db.UserMaster
                              join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                              where
                                um.IsDeleted == false && um.IsActive == true
                                && umd.ConfigurationColumn12 == Details.AirPortId
                                && (um.UserRole == "NO" || um.UserRole == "CA")
                              select new APINodalUsers
                              {
                                  UserMaserId = um.Id,
                                  UserName = um.UserName,
                                  EmailId = um.EmailId,
                                  MobileNumber = umd.MobileNumber
                              }).ToListAsync();

                List<APIGroupUsers> aPIGroupUsersList = aPINodalUserInfos.Select(x => new APIGroupUsers
                {
                    UserMasterId = x.Id,
                    UserId = x.MobileNumber,
                    EmailId = x.EmailId,
                    MobileNumber = x.MobileNumber,
                    UserName = x.FullName
                }).ToList();

                aPIGroupEmails.aPINodalUsers = aPINodalUsersList;
                aPIGroupEmails.aPIGroupUsers = aPIGroupUsersList;

                if (aPIGroupEmails.aPIGroupUsers.Count>0 && aPIGroupEmails.aPINodalUsers.Count>0)
                {
                    _email.GroupRequestMailToNodalOfficers(aPIGroupEmails);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }
        public DateTime? ValidateDateOfBirth(string dateOfBirth, string validDateFormat)
        {
            DateTime? dateOfBirth1 = null;
            try
            {
                DateTime result;
                result = DateTime.ParseExact(dateOfBirth, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                dateOfBirth1 = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return dateOfBirth1;
        }
        public async Task<List<APINodalUserInfo>> CreateNodalUser(int UserId, int CourseId, string OrgCode, List<APINodalUserInfo> aPINodalUserInfos, string GroupCode)
        {
            int count = 0;
            count = await (from ngrp in _db.NodalUserGroups
                     join nmap in _db.NodalGroupsUsersMapping on ngrp.Id equals nmap.GroupId
                     where ngrp.IsDeleted == false && nmap.IsDeleted == false
                        && ngrp.GroupCode == GroupCode
                     select nmap.Id).CountAsync();
            int insertedcount = 1;
            int maxlimit = 50;
            if (!string.IsNullOrEmpty(_configuration["GroupLimit"]))
                maxlimit = Convert.ToInt32(_configuration["GroupLimit"]);
            
            foreach (APINodalUserInfo item in aPINodalUserInfos)
            {
                if (insertedcount+count > maxlimit)
                {
                    var data = await _db.NodalCourseRequests.Join(_db.NodalUserGroups, no => no.GroupId, ni => ni.Id, (no, ni) => new
                    {
                        no,
                        GroupCode = ni.GroupCode
                    }).Join(_db.NodalGroupsUsersMapping, no => no.no.GroupId, ni => ni.GroupId, (no, ni) => new
                    {
                        no.no,
                        GroupCode = no.GroupCode
                    }).Join(_db.UserMaster,no=>no.no.UserId, ni=>ni.Id, (no, ni) => new
                    {
                        no.no,
                        GroupCode = no.GroupCode,
                        UserId = ni.UserId
                    }).Where(x => x.no.CourseId == CourseId && x.UserId == Security.Encrypt(item.MobileNumber) && x.no.IsDeleted == false && (x.no.IsApprovedByNodal == true || x.no.IsApprovedByNodal == null))
                    .Select(x => new { GroupCode = x.GroupCode, IsApprovedByNodal = x.no.IsApprovedByNodal }).FirstOrDefaultAsync();
                    if (data!=null)
                    {
                        if(data.GroupCode == GroupCode)
                            item.ErrorMessage = data.IsApprovedByNodal == true ? "Already registered for this course in same group." : "Already requested for this course in same group.";
                        else
                            item.ErrorMessage = data.IsApprovedByNodal == true ? "Already registered for this course." : "Already requested for this course.";
                    }
                    else
                        item.ErrorMessage = $"Can't Register in this Group! Group limit reached {maxlimit}";
                    continue;
                }

                APINodalUser aPINodalUser = Mapper.Map<APINodalUser>(item);
                aPINodalUser.CreatedBy = aPINodalUser.ModifiedBy = UserId;
                aPINodalUser.UserRole = "EU";
                aPINodalUser.UserType = "Internal";
                aPINodalUser.CustomerCode = aPINodalUser.OrganizationCode = OrgCode;

                aPINodalUser = await AddUser(aPINodalUser);
                if (aPINodalUser.Response == -1)
                {
                    item.ErrorMessage = "Account Already Exists!";

                    var userMasterId = await (from user in _db.UserMaster
                                              join userdetails in _db.UserMasterDetails on user.Id equals userdetails.UserMasterId
                                              where user.IsDeleted == false && userdetails.IsDeleted == false
                                              && user.UserId == Security.Encrypt(aPINodalUser.MobileNumber) && user.EmailId == Security.Encrypt(item.EmailId)
                                              && userdetails.MobileNumber == Security.Encrypt(aPINodalUser.MobileNumber)
                                              && userdetails.AadharNumber == Security.Encrypt(aPINodalUser.AadharNumber)
                                              && user.UserName == aPINodalUser.UserName
                                              select new { UserMasterId = user.Id, AirPortId = userdetails.ConfigurationColumn12 }).FirstOrDefaultAsync();
                    if (userMasterId!=null && userMasterId.UserMasterId>0)
                    {
                        if (userMasterId.AirPortId == aPINodalUser.ConfigurationColumn12Id)
                            item.Id = userMasterId.UserMasterId;
                        else
                        {
                            string AirPort = await _db.Configure12.Where(x => x.Id == userMasterId.AirPortId).Select(x => x.Name).FirstOrDefaultAsync();
                            item.ErrorMessage = string.Format("These users are already registered in {0} airport.", AirPort);
                        }
                    }
                    else
                    {
                        item.ErrorMessage = "Duplicate! User Id Already exist.";
                    }
                }
                else if (aPINodalUser.Response == -5)
                    item.ErrorMessage = "Duplicate! Email Id Already exist.";
                else if (aPINodalUser.Response == -6)
                    item.ErrorMessage = "Duplicate! Mobile Number Already exist.";
                else if (aPINodalUser.Response == -2)
                    item.ErrorMessage = "Please Enter Valid Mobile Number";
                else if (aPINodalUser.Response == -7)
                    item.ErrorMessage = "Duplicate! Aadhar Number Already exist.";
                else if (aPINodalUser.Id > 0)
                {
                    item.Id = aPINodalUser.Id;
                    item.ErrorMessage = "Success";
                    UserMasterLogs _userMasterLogs = new UserMasterLogs();
                    _userMasterLogs.ModifiedBy = UserId;
                    _userMasterLogs.IsInserted = 1;
                    _userMasterLogs.UserId = aPINodalUser.Id;
                    await this.AddUserMasterLogs(_userMasterLogs);
                }
                if (item.ErrorMessage == "Success" || item.ErrorMessage == "Account Already Exists!")
                {
                    var data = await _db.NodalCourseRequests.Join(_db.NodalUserGroups, no => no.GroupId, ni => ni.Id, (no, ni) => new
                                                                    {
                                                                        no,
                                                                        GroupCode = ni.GroupCode
                                                                    }).Join(_db.NodalGroupsUsersMapping, no => no.no.GroupId, ni => ni.GroupId, (no, ni) => new
                                                                    {
                                                                        no.no,
                                                                        GroupCode = no.GroupCode
                                                                    }).Where(x => x.no.CourseId == CourseId && x.no.UserId == item.Id && x.no.IsDeleted == false && (x.no.IsApprovedByNodal == true || x.no.IsApprovedByNodal == null))
                                                                    .Select(x=> new { GroupCode = x.GroupCode, IsApprovedByNodal = x.no.IsApprovedByNodal }).FirstOrDefaultAsync();
                    if (data != null)
                    {
                        if (data.GroupCode == GroupCode)
                        {
                            item.ErrorMessage = data.IsApprovedByNodal == true ? "Already registered for this course in same group." : "Already requested for this course in same group.";
                            //insertedcount++;
                        }
                        else
                            item.ErrorMessage = data.IsApprovedByNodal == true ? "Already registered for this course." : "Already requested for this course.";
                    }
                    else
                        insertedcount++;
                }
            }
            return aPINodalUserInfos;
        }
        public async Task<APINodalUser> AddUser(APINodalUser Apiuser)
        {
            string OrgCode = Apiuser.OrganizationCode;
            Exists exist = await this.Validations(Apiuser);
            if (!exist.Equals(Exists.No))
            {
                if (exist.Equals(Exists.UserIdExist))
                    Apiuser.Response = -1;
                else if (exist.Equals(Exists.EmailIdExist))
                    Apiuser.Response = -5;
                else if (exist.Equals(Exists.MobileExist))
                    Apiuser.Response = -6;
                else if (exist.Equals(Exists.AadharExist))
                    Apiuser.Response = -7;

                return Apiuser;
            }
            else
            {
                Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
                Apiuser.AccountCreatedDate = DateTime.UtcNow;
                Apiuser.CreatedDate = DateTime.UtcNow;
                Apiuser.ModifiedDate = DateTime.UtcNow;


                var allowRANDOMPASSWORD = await _userRepository.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode);
                if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
                {
                    if (OrgCode.ToLower().Contains("keventers"))
                    {
                        Apiuser.RandomUserPassword = "Keventers@123";
                        Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
                    }
                    else
                    {
                        if (OrgCode.ToLower() == "pepperfry")
                            Apiuser.Password = Helper.Security.EncryptSHA512("123456");
                        else
                            Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
                    }
                }
                else
                {
                    Apiuser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
                    Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
                }

                if (Apiuser.MobileNumber != "")
                {
                    if (Apiuser.MobileNumber.Length < 10)
                    {
                        Apiuser.Response = -2;
                        return Apiuser;
                    }
                }

                Apiuser.MobileNumber = (Apiuser.MobileNumber == "" ? null : Security.Encrypt(Apiuser.MobileNumber));
                Apiuser.EmailId = (Apiuser.EmailId == "" ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

                Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
                Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
                Apiuser.Id = 0;

                if (OrgCode.ToLower() == "spectra")
                {
                    Apiuser.TermsCondintionsAccepted = true;
                    Apiuser.IsPasswordModified = true;
                    Apiuser.AcceptanceDate = DateTime.UtcNow;
                    Apiuser.PasswordModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    Apiuser.TermsCondintionsAccepted = false;
                    Apiuser.IsPasswordModified = false;
                }

                Apiuser.ConfigurationColumn12 = await _db.Configure12.Where(x => x.Id == Apiuser.ConfigurationColumn12Id && x.IsDeleted == 0).Select(x => x.Name).FirstOrDefaultAsync();

                Apiuser = await this.AddUserToDb(Apiuser);

                return Apiuser;
            }
        }
        public async Task<Exists> Validations(APINodalUser user)
        {
            if (await this.IsExists(user.UserId))
                return Exists.UserIdExist;
            if (!string.IsNullOrEmpty(user.EmailId))
            {
                if (await this.EmailExists(user.EmailId))
                    return Exists.EmailIdExist;
            }
            if (!string.IsNullOrEmpty(user.MobileNumber))
            {
                if (await this.MobileExists(user.MobileNumber))
                    return Exists.MobileExist;
            }
            if (!string.IsNullOrEmpty(user.AadharNumber))
            {
                if (await this.AadharExists(user.AadharNumber))
                    return Exists.AadharExist;
            }
            return Exists.No;
        }
        public async Task<bool> IsExists(string userId)
        {
            if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId == Security.Encrypt(userId.ToLower())).CountAsync() > 0)
                return true;
            return false;
        }
        public async Task<bool> EmailExists(string emailId)
        {
            if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == Security.Encrypt(emailId.ToLower())) > 0)
                return true;
            return false;
        }
        public async Task<bool> MobileExists(string mobileNumber)
        {
            if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.MobileNumber == Security.Encrypt(mobileNumber)) > 0)
                return true;
            return false;
        }
        public async Task<bool> AadharExists(string aadharNumber)
        {
            if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.AadharNumber == Security.Encrypt(aadharNumber)) > 0)
                return true;
            return false;
        }
        public async Task<int> GetTotalUserCount()
        {
            return await _db.UserMaster.CountAsync();
        }
        
        public async Task<APINodalUser> AddUserToDb(APINodalUser apiUser)
        {
            using (var transaction = this._context.Database.BeginTransaction())
            {
                try
                {
                    UserMaster User = Mapper.Map<UserMaster>(apiUser);
                    UserMasterDetails UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
                    await this._db.UserMaster.AddAsync(User);
                    await this._db.SaveChangesAsync();
                    UserDetails.UserMasterId = User.Id;

                    if (UserDetails.UserMasterId > 0)
                    {
                        var Num = User.Id % 10;
                        switch (Num)
                        {
                            case 1:
                                UserDetails.HouseId = 1;
                                break;
                            case 2:
                                UserDetails.HouseId = 2;
                                break;
                            case 3:
                                UserDetails.HouseId = 3;
                                break;
                            case 4:
                                UserDetails.HouseId = 4;
                                break;
                            case 5:
                                UserDetails.HouseId = 1;
                                break;
                            case 6:
                                UserDetails.HouseId = 2;
                                break;
                            case 7:
                                UserDetails.HouseId = 3;
                                break;
                            case 8:
                                UserDetails.HouseId = 4;
                                break;
                            case 9:
                                UserDetails.HouseId = 1;
                                break;
                            default:
                                UserDetails.HouseId = 2;
                                break;
                        }

                        UserDetails.IsActive = User.IsActive;
                        UserDetails.IsDeleted = User.IsDeleted;
                        UserDetails.CreatedBy = UserDetails.ModifiedBy = User.Id;
                        apiUser.Id = apiUser.Response = User.Id;
                        await this._db.UserMasterDetails.AddAsync(UserDetails);
                        await this._db.SaveChangesAsync();
                        transaction.Commit();
                        return apiUser;
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    transaction.Rollback();
                    throw ex;
                }
            }
            apiUser.Id = apiUser.Response = 0;
            return apiUser;
        }
        public async Task<int> AddUserMasterLogs(UserMasterLogs userMasterLogs)
        {
            UserMasterLogs _userMasterLogs = new UserMasterLogs();
            _userMasterLogs.UserId = userMasterLogs.UserId;
            _userMasterLogs.ModifiedDate = DateTime.Now;
            _userMasterLogs.ModifiedBy = userMasterLogs.ModifiedBy;
            _userMasterLogs.IsUpdated = userMasterLogs.IsUpdated;
            _userMasterLogs.IsDeleted = userMasterLogs.IsDeleted;
            _userMasterLogs.IsInserted = userMasterLogs.IsInserted;
            await this._db.UserMasterLogs.AddAsync(_userMasterLogs);
            await this._db.SaveChangesAsync();
            return 1;

        }
        public async Task<List<APICourseRegistrations>> GetCourseRegistrations(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false)
        {
            if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
                SearchText = Security.Encrypt(SearchText);

            List<APICourseRegistrations> aPICourseRegistrations = new List<APICourseRegistrations>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@Page", Page);
                    parameters.Add("@PageSize", PageSize);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    parameters.Add("@IsExport", IsExport);
                    var result = await SqlMapper.QueryAsync<APICourseRegistrations>((SqlConnection)connection, "[dbo].[GetCourseRegistrations]", parameters, null, null, CommandType.StoredProcedure);
                    aPICourseRegistrations = result.ToList();
                    connection.Close();
                    aPICourseRegistrations.ForEach(x =>
                    {
                        x.UserId = Security.Decrypt(x.UserId);
                        x.EmailId = Security.Decrypt(x.EmailId);
                        x.MobileNumber = Security.Decrypt(x.MobileNumber);
                        x.AadharNumber = x.AadharNumber != null ? Security.Decrypt(x.AadharNumber) : null;
                    });
                }
            }
            return aPICourseRegistrations;
        }
        public async Task<int> GetCourseRegistrationsCount(int UserId, string Search = null, string SearchText = null, bool IsExport=false)
        {
            if (Search != null && SearchText != null && (Search.ToLower() == "userid" || Search.ToLower() == "mobilenumber" || Search.ToLower() == "emailid"))
                SearchText = Security.Encrypt(SearchText);

            int count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@Page", 1);
                    parameters.Add("@PageSize", 0);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    parameters.Add("@IsExport", IsExport);
                    var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetCourseRegistrations]", parameters, null, null, CommandType.StoredProcedure);
                    count = result.Select(x => x.RecordCount).FirstOrDefault();
                    connection.Close();
                }
            }
            return count;
        }
        public async Task<List<APICourseGroupUsers>> GetApprovedCourseGroupUsers(int UserId, int GroupId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            if (Search != null && SearchText != null)
            {
                if (Search.ToLower().Equals("userid"))
                    SearchText = Security.Encrypt(SearchText);
            }

            List<APICourseGroupUsers> aPICourseGroupUsers = new List<APICourseGroupUsers>();
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@GroupId", GroupId);
                    parameters.Add("@Page", Page);
                    parameters.Add("@PageSize", PageSize);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync<APICourseGroupUsers>((SqlConnection)connection, "[dbo].[GetApprovedCourseGroupUsers]", parameters, null, null, CommandType.StoredProcedure);
                    aPICourseGroupUsers = result.ToList();
                    connection.Close();
                }
                aPICourseGroupUsers.ForEach(x =>
                {
                    x.UserId = Security.Decrypt(x.UserId);
                    x.EmailId = Security.Decrypt(x.EmailId);
                    x.MobileNumber = Security.Decrypt(x.MobileNumber);
                    x.AadharNumber = Security.Decrypt(x.AadharNumber);
                });
            }
            return aPICourseGroupUsers;
        }
        public async Task<int> GetApprovedCourseGroupUsersCount(int UserId, int GroupId, string Search = null, string SearchText = null)
        {
            if (Search != null && SearchText != null)
            {
                if (Search.ToLower().Equals("userid"))
                    SearchText = Security.Encrypt(SearchText);
            }
            int count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@GroupId", GroupId);
                    parameters.Add("@Page", 1);
                    parameters.Add("@PageSize", 0);
                    parameters.Add("@Search", Search);
                    parameters.Add("@SearchText", SearchText);
                    var result = await SqlMapper.QueryAsync((SqlConnection)connection, "[dbo].[GetApprovedCourseGroupUsers]", parameters, null, null, CommandType.StoredProcedure);
                    count = result.Select(x => x.RecordCount).FirstOrDefault();
                    connection.Close();
                }
            }
            return count;
        }
        public async Task<APIPaymentRequestData> MakePayment(int UserId, string RequestId)
        {
            MerchantParams merchantParams = await InitializePaymentRequest(UserId, RequestId);
            string encryptedRequest = EncryptPaymentRequest(merchantParams);

            APIPaymentRequestData aPIPaymentRequestData = new APIPaymentRequestData();
            aPIPaymentRequestData.Url = "https://onepaypgtest.in/onepayVAS/payprocessor";
            aPIPaymentRequestData.requestData = encryptedRequest;
            aPIPaymentRequestData.merchantId = merchantParams.merchantId;
            return aPIPaymentRequestData;
        }

        private async Task<MerchantParams> InitializePaymentRequest(int UserId, string RequestId)
        {
            var UserDetails = await (from um in _db.UserMaster
                                     join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                                     where um.Id == UserId && um.IsDeleted == false
                                          && umd.IsDeleted == false
                                     select new
                                     {
                                         Id = um.Id,
                                         UserId = um.UserId,
                                         EmailId = um.EmailId,
                                         MobileNumber = umd.MobileNumber
                                     }).FirstOrDefaultAsync();

            MerchantParams merchantParams = new MerchantParams()
            {
                merchantId = _configuration["MerchantId"],
                apiKey = _configuration["ApiKey"],
                txnId = getTransactionId(),
                amount = "10.00",
                dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                custMail = Security.Decrypt(UserDetails.EmailId),
                custMobile = Security.Decrypt(UserDetails.MobileNumber),
                udf1 = "NA",
                udf2 = "NA",
                returnURL = _configuration[Configuration.ApplicationUrl],
                isMultiSettlement = "0",
                productId = "DEFAULT",
                channelId = "0",
                txnType = "DIRECT",
                udf3 = "NA",
                udf4 = "NA",
                udf5 = "NA",
                instrumentId = "NA",
                cardDetails = "NA",
                cardType = "NA"
            };

            return merchantParams;
        }

        public Random a = new Random();
        public List<int> randomList = new List<int>();
        int MyNumber = 0;
        private string getTransactionId()
        {
            MyNumber = a.Next(0, 10);
            while (randomList.Contains(MyNumber))
                MyNumber = a.Next(0, 10);

            return MyNumber.ToString();
        }

        public string EncryptPaymentRequest(MerchantParams merchantParams)
        {
            //add the Transaction Posting URL	
            /* 
                Merchant has to enter the Merchant Id shared by 1Pay in the below variable 'merchantId' and pass in the Request along with encrypted Request Data.
                Merchant has to enter the API Key shared by 1Pay in the below variable 'merchantEncryptionKey' to Encrypt the Transaction Request.
                Algorithm used for encryption is AES.
                Merchant Encryption Key will be different for TEST and PRODUCTION environment.
            */
            String merchantId = merchantParams.merchantId;
            String merchantEncryptionKey = _configuration["ApiKey"]; //16 Charachter String
            String encryptedText = string.Empty;

            try
            {
                string original = JsonConvert.SerializeObject(merchantParams);  //"Here is some data to encrypt!";

                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                AesManaged tdes = new AesManaged();
                tdes.Key = UTF8.GetBytes(merchantEncryptionKey);
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform crypt = tdes.CreateEncryptor();
                //byte[] plain = Encoding.UTF8.GetBytes("{\"merchantId\":\"M0002\",\"ApiKey\":\"jpuT6032\",\"txnId\":\"1234567890\",\"amount\":\"10.00\",\"DateTime\":\"\\/Date(1547311525000)\\/\",\"CustMail\":\"test@test.com\",\"CustMobile\":\"9876543210\",\"UDF1\":\"NA\",\"UDF2\":\"NA\",\"ReturnUrl\":\"www.merchantUrl.com/merchantResponseProcessor.jsp\",\"IsMultiSettlement\":\"0\",\"ProductId\":\"DEFAULT\",\"ChannelId\":\"0\",\"TxnType\":\"DIRECT\",\"UDF3\":\"NA\",\"UDF4\":\"NA\",\"UDF5\":\"NA\",\"InstrumentId\":\"NA\",\"CardDetails\":\"NA\",\"CardType\":\"NA\"}");
                //byte[] plain = Encoding.UTF8.GetBytes("{\"merchantId\":\"M0002\",\"apiKey\":\"jpuT6032\",\"txnId\":\"1234567892\",\"amount\":\"10.00\",\"dateTime\":\"2019-01-19 20:15:25\",\"custMail\":\"test@test.com\",\"custMobile\":\"9876543210\",\"udf1\":\"NA\",\"udf2\":\"NA\",\"returnURL\":\"http://139.59.1.254:8080/payone/merchantResponse.jsp\",\"isMultiSettlement\":\"0\",\"productId\":\"DEFAULT\",\"channelId\":\"0\",\"txnType\":\"DIRECT\",\"udf3\":\"NA\",\"udf4\":\"NA\",\"udf5\":\"NA\",\"instrumentId\":\"NA\",\"cardDetails\":\"NA\",\"cardType\":\"NA\"}");
                byte[] plain = Encoding.UTF8.GetBytes(original);
                byte[] cipher = crypt.TransformFinalBlock(plain, 0, plain.Length);
                encryptedText = Convert.ToBase64String(cipher);
                Console.WriteLine(encryptedText);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            return encryptedText;
        }
        public APIPaymentRequestData ProcessPaymentRequest(string encryptedRequestParams, string merchantId)
        {
            return null;
            String tranStatus = null;
            String message = null;
            String errorCode = null;
        
            //JavaScriptSerializer ser = new JavaScriptSerializer();
            //add the Transaction Posting URL	
            
            //String reqUrl = "https://hdfcprodsigning.in/onepayVAS/payprocessorV2"; //32 Bit
            string responseString = string.Empty;

            APIPaymentRequestData aPIPaymentRequestData = new APIPaymentRequestData();
            //======================================= Server Side Form Post ===========================================================



            //=========================================================================================================================

            //var request = (HttpWebRequest)WebRequest.Create(reqUrl);

            /*
            try
            {
                var postData = string.Format("reqData={0}&merchantId={1}", encryptedRequestParams.Trim(), merchantId);
                var data = Encoding.UTF8.GetBytes(postData);
                
                HttpResponseMessage response;
                _logger.Debug("test 1");
                using (var client = new HttpClient())
                {
                    using (var content = new StringContent(postData, Encoding.UTF8, "application/json"))
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        response = client.PostAsync(new Uri(reqUrl), content).Result;
                        _logger.Debug("test 2");
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.Debug("test 3");
                    var responseResult = response.Content.ReadAsStringAsync();
                    responseResult.Wait();
                    responseString = responseResult.Result;
                    _logger.Debug("test 4 - "+ responseString);
                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine("Error: {0}", e.Message);
            }
            
            Console.WriteLine("verification started");

            if (!string.IsNullOrEmpty(responseString))
            {


                // Here please read response and deserialize that response into 'PayproceesResponseClass' class (create class same as 'VerificationResponse' in VerificationUtil.cs file)			
                var payresponse = JsonConvert.DeserializeObject<PayproceesResponseClass>(responseString);

                String txn_id = payresponse.txn_id; // Put Here Merchant transaction id from the response;			

                String verParams = "merchantId=" + merchantId + "&txnId=" + txn_id;
                String verUrl = "https://hdfcprodsigning.in/onepayVAS/getTxnDetails";

                var verResp = VerificationCall(verUrl, verParams);
                if (verResp.resp_code == "00000" && verResp.trans_status == "ok")
                {
                    tranStatus = "Ok";
                    message = "Transaction Successful.";
                    errorCode = "00000";
                }
                else
                {
                    tranStatus = "F";
                    errorCode = "FFFFF";
                    message = "Transaction Failed.";
                }

                Console.WriteLine("::: tranStatus : " + tranStatus);
                Console.WriteLine("::: errorCode : " + errorCode);
                Console.WriteLine("::: message : " + message);



            }

            return responseString;
            */
        }

        public VerificationResponse VerificationCall(String sURL, String data)
        {
            VerificationResponse resp = new VerificationResponse();
            var requestJson = string.Empty;
            try
            {
                string requestUrl = sURL + "?" + data;
                Console.WriteLine(":: Verify URL : " + requestUrl);
                HttpResponseMessage response;

                using (var handler = new HttpClientHandler())
                using (var client = new HttpClient(handler))
                {
                    using (var content = new StringContent(requestJson, Encoding.Unicode, "application/json"))
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        //client.DefaultRequestHeaders.Add(Common.AuthenticateHeaderKey, Settings.AppAuthKey);
                        response = client.PostAsync(new Uri(requestUrl), content).Result;
                    }
                }

                Console.WriteLine("Verification call HTTP response code " + (short)response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(":: HTTP OK");
                    var responseResult = response.Content.ReadAsStringAsync();
                    responseResult.Wait();
                    var result = responseResult.Result;
                    Console.WriteLine(":: Response : " + result);
                    resp = JsonConvert.DeserializeObject<VerificationResponse>(result);

                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (ex.InnerException != null)
                    Console.WriteLine(":: Error Occurred while Processing Request : " + ex.InnerException.Message);
                else
                    Console.WriteLine(":: Error Occurred while Processing Request : " + ex.Message);
                Console.ResetColor();
            }
            return resp;
        }
    }
}
