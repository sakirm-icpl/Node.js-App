using AutoMapper;
using CCA.Util;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Payment.API.APIModel;
using Payment.API.Data;
using Payment.API.Helper;
using Payment.API.Models;
using Payment.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Payment.API.Models.UserMaster;

namespace Payment.API.Repositories
{
    public class UserSignUpRepository : Repository<UserSignUp>, IUserSignUpRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSignUpRepository));
        private UserDbContext _db;
        private IEmail _email;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IConfiguration _configuration;
        private IUserRepository userRepository;
        public UserSignUpRepository(UserDbContext context, IEmail email,
            ICustomerConnectionStringRepository customerConnectionString,
            IConfiguration configuration) : base(context)
        {
            this._db = context;
            this._email = email;
            _customerConnectionString = customerConnectionString;
            _configuration = configuration;
        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<UserSignUp>();
        }
        public bool ExistsUserSignUp(string userId)
        {
            if (this._db.UserSignUp.Count(x => x.UserId == Security.Encrypt(userId) && x.IsDeleted == 0) > 0)
                return true;
            return false;
        }
        public bool UserNameExists(string userName)
        {
            if (this._db.UserSignUp.Count(x => x.UserName == userName) > 0)
                return true;
            return false;
        }

        public bool EmailExistsForUserSignUp(string emailId)
        {
            if (this._db.UserSignUp.Where(users => users.EmailId == emailId.ToLower() && users.IsDeleted == 0).Select(u => u.Id).Count() > 0)
                return true;
            return false;
        }
        public bool MobileExistsForUserSignUp(string mobileNumber)
        {
            if (this._db.UserSignUp.Count(x => x.MobileNumber == Security.Encrypt(mobileNumber) && x.IsDeleted == 0) > 0)
                return true;
            return false;
        }
        public async Task<int> SendEmailtoUser(APIUserMaster Apiuser, string orgCode, string Password)
        {


            string Email = string.Empty;
            string ToEmail = string.Empty;
            string Subject = string.Empty;
            string Message = string.Empty;
            Task.Run(() => _email.SendEmailtoUser1(Security.Decrypt(Apiuser.EmailId), orgCode, Apiuser.UserName, Security.Decrypt(Apiuser.MobileNumber), Security.Decrypt(Apiuser.UserId), Password));

            return 1;
        }

        //  public async Task<int> ExistsOTP(string empcode, string otp, string customercode, string OrganisationString)
        //{
        //    this.ChangeDbContext(OrganisationString);
        //    SignUpOTP signUpOTP = new SignUpOTP();
        //    var query2 = (from query1 in _db.SignUpOTP
        //                  where query1.EmpCode == empcode
        //                  orderby query1.CreatedDate descending
        //                  select new
        //                  {
        //                      query1.CreatedDate,
        //                      query1.OTP
        //                  }).Take(1);
        //    var result = await query2.FirstOrDefaultAsync();
        //    DateTime createdDate = result.CreatedDate;
        //    DateTime ExpDate = createdDate.AddMinutes(5);
        //    DateTime CurrentTime = DateTime.UtcNow;
        //    int IsTimeExpired = DateTime.Compare(ExpDate, CurrentTime);
        //    if (IsTimeExpired == -1)
        //    {
        //        return 2;
        //    }
        //    if (result.OTP == otp)

        //        return 1;
        //    else
        //        return 0;
        //}
        //public async Task<bool> ExistsOTPForVFS(string empcode, string otp, string customercode, string OrganisationString)
        //{
        //    this.ChangeDbContext(OrganisationString);
        //    SignUpOTP signUpOTP = new SignUpOTP();
        //    var query = _db.SignUpOTP.Where(e => e.EmpCode == empcode).OrderByDescending(e => e.CreatedDate).Select(e => e.OTP).Take(1);

        //    if (query.Contains(otp))

        //        return true;
        //    else
        //        return false;
        //}


        //public async Task<int> getdataforVFS(string empcode, string otp, string customercode, string OrganisationString)
        //{
        //    this.ChangeDbContext(OrganisationString);
        //    SignUpOTP signUpOTP = await this._db.SignUpOTP.Where(f => f.OTP == otp && f.EmpCode == empcode).FirstOrDefaultAsync();

        //    this._db.SignUpOTP.Remove(signUpOTP);
        //    await this._db.SaveChangesAsync();
        //    return 1;
        //}

        // public async Task<List<APIUserSignUpTypeAhead>> GetVFSSignUp(APISignUpTypeAhead aPISignUpTypeAhead, string OrganisationConnectionString)
        //{
        //    List<APIUserSignUpTypeAhead> Query = null;
        //    this.ChangeDbContext(OrganisationConnectionString);
        //    aPISignUpTypeAhead.ColumnName = aPISignUpTypeAhead.ColumnName;
        //    switch (aPISignUpTypeAhead.ColumnName)
        //    {
        //        case "Area":
        //            this.ChangeDbContext(OrganisationConnectionString);
        //            Query = (from area in this._db.Area
        //                     where (area.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || area.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = area.Name,
        //                         Id = area.Id
        //                     }
        //                     ).ToList();

        //            break;
        //        case "Business":
        //            Query = (from business in this._db.Business
        //                     where (business.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || business.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = business.Name,
        //                         Id = business.Id
        //                     }
        //                   ).ToList();
        //            break;
        //        case "Group":
        //            Query = (from group1 in this._db.Group
        //                     where (group1.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || group1.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = group1.Name,
        //                         Id = group1.Id
        //                     }
        //                  ).ToList();
        //            break;
        //        case "Location":
        //            Query = (from location in this._db.Location
        //                     where (location.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || location.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = location.Name,
        //                         Id = location.Id
        //                     }
        //                 ).ToList();
        //            break;
        //        case "ConfigurationColumn1":
        //            Query = (from config1 in this._db.Configure1
        //                     where (config1.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config1.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config1.Name,
        //                         Id = config1.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn2":
        //            Query = (from config2 in this._db.Configure2
        //                     where (config2.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config2.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config2.Name,
        //                         Id = config2.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn3":
        //            Query = (from config3 in this._db.Configure3
        //                     where (config3.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config3.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config3.Name,
        //                         Id = config3.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn4":
        //            Query = (from config4 in this._db.Configure4
        //                     where (config4.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config4.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config4.Name,
        //                         Id = config4.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn5":
        //            Query = (from config5 in this._db.Configure5
        //                     where (config5.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config5.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config5.Name,
        //                         Id = config5.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn6":
        //            Query = (from config6 in this._db.Configure6
        //                     where (config6.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config6.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config6.Name,
        //                         Id = config6.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn7":
        //            Query = (from config7 in this._db.Configure7
        //                     where (config7.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config7.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config7.Name,
        //                         Id = config7.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn8":
        //            Query = (from config8 in this._db.Configure8
        //                     where (config8.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config8.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config8.Name,
        //                         Id = config8.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn9":
        //            Query = (from config9 in this._db.Configure9
        //                     where (config9.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config9.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config9.Name,
        //                         Id = config9.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn10":
        //            Query = (from config10 in this._db.Configure10
        //                     where (config10.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config10.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config10.Name,
        //                         Id = config10.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn11":
        //            Query = (from config11 in this._db.Configure11
        //                     where (config11.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config11.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config11.Name,
        //                         Id = config11.Id
        //                     }
        //                ).ToList();
        //            break;
        //        case "ConfigurationColumn12":
        //            Query = (from config12 in this._db.Configure12
        //                     where (config12.IsDeleted == Record.NotDeleted && (aPISignUpTypeAhead.Search == null || config12.Name.Contains(aPISignUpTypeAhead.Search)))
        //                     select new APIUserSignUpTypeAhead
        //                     {
        //                         Name = config12.Name,
        //                         Id = config12.Id
        //                     }
        //                ).ToList();
        //            break;
        //    }
        //    return Query;
        //}


        //public async Task<IEnumerable<APIUserSetting>> GetVFSSettings(string OrganisationString = null)
        //{
        //    try
        //    {
        //        this.ChangeDbContext(OrganisationString);
        //        var userSetting = (from userSettings in this._db.UserSettings
        //                           where (userSettings.IsDeleted == Record.NotDeleted && userSettings.IsConfigured == true)
        //                           select new APIUserSetting
        //                           {
        //                               Id = userSettings.Id,
        //                               ConfiguredColumnName = userSettings.ConfiguredColumnName,
        //                               ChangedColumnName = userSettings.ChangedColumnName,
        //                               IsConfigured = userSettings.IsConfigured,
        //                               IsMandatory = userSettings.IsMandatory,
        //                               IsShowInReport = userSettings.IsShowInReport,
        //                               FieldType = userSettings.FieldType
        //                           });



        //        return await userSetting.AsNoTracking().ToListAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        string exception = ex.Message;
        //    }
        //    return null;
        //}


        //public async Task<APIUserSignUp> AddUserSignUp(APIUserSignUp signUpOTP, string OrganisationString)
        //{
        //    this.ChangeDbContext(OrganisationString);
        //    SignUpOTP signUp = new SignUpOTP();
        //    signUp.EmpCode = Security.Decrypt(signUpOTP.EmailId);
        //    signUp.OTP = signUpOTP.Otp;
        //    signUp.CreatedDate = DateTime.Now;
        //    this._db.SignUpOTP.Add(signUp);
        //    await this._db.SaveChangesAsync();
        //    return (signUpOTP);

        //}
        //public async Task<List<APIAirportInfo>> GetAirports(string ConnectionString)
        //{
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        var courses = (from ap in context.Configure12
        //                       where ap.IsDeleted == 0
        //                       orderby ap.Name
        //                       select new APIAirportInfo
        //                       {
        //                           Id = ap.Id,
        //                           Name = ap.Name
        //                       });
        //        return await courses.ToListAsync();
        //    }
        //}
        //public async Task<int> NodalUserSignUp(APINodalUserSignUp aPINodalUserSignUp, string ConnectionString)
        //{
        //    APINodalUser aPINodalUser = Mapper.Map<APINodalUser>(aPINodalUserSignUp);
        //    ChangeDbContext(ConnectionString);
        //    aPINodalUser.CreatedBy = aPINodalUser.ModifiedBy = 1;
        //    aPINodalUser.UserRole = "EU";
        //    if (aPINodalUserSignUp.DateOfBirth != null && (((DateTime)aPINodalUserSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //        return -7;

        //    if (aPINodalUserSignUp.DateOfBirth != null)
        //        aPINodalUserSignUp.DateOfBirth = ((DateTime)aPINodalUserSignUp.DateOfBirth).Date;

        //    var courseInfo = await _db.Course.Where(x => x.IsDeleted == false && x.Id == aPINodalUserSignUp.CourseId)
        //                    .Select(x => new { Code=x.Code,Title=x.Title }).FirstOrDefaultAsync();
        //    aPINodalUser.Code = courseInfo.Code;
        //    aPINodalUser.Title = courseInfo.Title;

        //    aPINodalUser = await AddUser(aPINodalUser, ConnectionString);
        //    if (aPINodalUser.Id > 0)
        //    {
        //        List<NodalCourseRequests> nodalCourseRequestsExistsList = await _db.NodalCourseRequests.Where(x => x.UserId == aPINodalUser.Id && x.IsDeleted == false && (x.IsApprovedByNodal==true || x.IsApprovedByNodal==null)).ToListAsync();
        //        if (nodalCourseRequestsExistsList.Count>0)
        //        {
        //            if (nodalCourseRequestsExistsList.Where(x=>x.IsApprovedByNodal==true).Count()>0)
        //                aPINodalUser.Response = -9;
        //            else
        //                aPINodalUser.Response = -10;
        //        }
        //        else
        //        {
        //            Course course = await _db.Course.Where(x => x.Id == aPINodalUserSignUp.CourseId && x.IsDeleted == false).FirstOrDefaultAsync();

        //            //Add course request
        //            NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
        //            nodalCourseRequest.CourseId = aPINodalUserSignUp.CourseId;
        //            nodalCourseRequest.UserId = aPINodalUser.Id;
        //            nodalCourseRequest.RequestType = NodalCourseRequest.Individual;
        //            nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = aPINodalUser.Id;
        //            nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
        //            nodalCourseRequest.CourseFee = course.CourseFee;
        //            _db.NodalCourseRequests.Add(nodalCourseRequest);
        //            await _db.SaveChangesAsync();

        //            UserMasterLogs _userMasterLogs = new UserMasterLogs();
        //            _userMasterLogs.ModifiedBy = 1;
        //            _userMasterLogs.IsInserted = 1;
        //            _userMasterLogs.UserId = aPINodalUser.Id;
        //            await this.AddUserMasterLogs(_userMasterLogs);

        //            await SendEmailAfterAddingUser(aPINodalUser);
        //        }

        //    }
        //    return aPINodalUser.Response;
        //}
        //public async Task<int> TTUserSignUp(APITTUserSignUp aPITTUserSignUp, string ConnectionString)
        //{
        //    APITTUser aPINodalUser = Mapper.Map<APITTUser>(aPITTUserSignUp);
        //    ChangeDbContext(ConnectionString);
        //    aPINodalUser.CreatedBy = aPINodalUser.ModifiedBy = 1;
        //    aPINodalUser.UserRole = "EU";
        //    if (aPITTUserSignUp.DateOfBirth != null && (((DateTime)aPITTUserSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //        return -7;

        //    if (aPITTUserSignUp.DateOfBirth != null)
        //        aPITTUserSignUp.DateOfBirth = ((DateTime)aPITTUserSignUp.DateOfBirth).Date;

        //    var courseInfo = await _db.Course.Where(x => x.IsDeleted == false && x.Id == aPITTUserSignUp.CourseId)
        //                    .Select(x => new { Code = x.Code, Title = x.Title }).FirstOrDefaultAsync();
        //    aPINodalUser.Code = courseInfo.Code;
        //    aPINodalUser.Title = courseInfo.Title;

        //    aPINodalUser = await AddTTUser(aPINodalUser, ConnectionString);
        //    if (aPINodalUser.Id > 0)
        //    {
        //        List<NodalCourseRequests> nodalCourseRequestsExistsList = await _db.NodalCourseRequests.Where(x => x.UserId == aPINodalUser.Id && x.IsDeleted == false && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
        //        if (nodalCourseRequestsExistsList.Count > 0)
        //        {
        //            if (nodalCourseRequestsExistsList.Where(x => x.IsApprovedByNodal == true).Count() > 0)
        //                aPINodalUser.Response = -9;
        //            else
        //                aPINodalUser.Response = -10;
        //        }
        //        else
        //        {
        //            Course course = await _db.Course.Where(x => x.Id == aPITTUserSignUp.CourseId && x.IsDeleted == false).FirstOrDefaultAsync();

        //            //Add course request
        //            NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
        //            nodalCourseRequest.CourseId = aPITTUserSignUp.CourseId;
        //            nodalCourseRequest.UserId = aPINodalUser.Id;
        //            nodalCourseRequest.RequestType = NodalCourseRequest.Individual;
        //            nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = aPINodalUser.Id;
        //            nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
        //            nodalCourseRequest.CourseFee = course.CourseFee;
        //            _db.NodalCourseRequests.Add(nodalCourseRequest);
        //            await _db.SaveChangesAsync();

        //            UserMasterLogs _userMasterLogs = new UserMasterLogs();
        //            _userMasterLogs.ModifiedBy = 1;
        //            _userMasterLogs.IsInserted = 1;
        //            _userMasterLogs.UserId = aPINodalUser.Id;
        //            await this.AddUserMasterLogs(_userMasterLogs);

        //           // await SendEmailAfterAddingUser(aPINodalUser);
        //        }

        //    }
        //    return aPINodalUser.Response;
        //}

        //public async Task<int> CreateNodalUser(int UserId, APINodalUserSignUp aPINodalUserSignUp, string ConnectionString)
        //{
        //    int? AirPortId = await _db.UserMasterDetails.Where(x => x.UserMasterId == UserId && x.IsDeleted == false).Select(x => x.ConfigurationColumn12).FirstOrDefaultAsync();
        //    if (AirPortId == null || AirPortId == 0)
        //        return -3;
        //    else
        //        aPINodalUserSignUp.AirPortId = (int)AirPortId;

        //    if (aPINodalUserSignUp.DateOfBirth != null && (((DateTime)aPINodalUserSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //        return -7;

        //    if (aPINodalUserSignUp.DateOfBirth != null)
        //        aPINodalUserSignUp.DateOfBirth = ((DateTime)aPINodalUserSignUp.DateOfBirth).Date;

        //    APINodalUser aPINodalUser = Mapper.Map<APINodalUser>(aPINodalUserSignUp);
        //    aPINodalUser.CreatedBy = aPINodalUser.ModifiedBy = UserId;
        //    aPINodalUser.UserRole = "EU";

        //    var courseInfo = await _db.Course.Where(x => x.IsDeleted == false && x.Id == aPINodalUserSignUp.CourseId)
        //                    .Select(x => new { Code = x.Code, Title = x.Title }).FirstOrDefaultAsync();
        //    aPINodalUser.Code = courseInfo.Code;
        //    aPINodalUser.Title = courseInfo.Title;

        //    aPINodalUser = await AddUser(aPINodalUser, ConnectionString);
        //    if (aPINodalUser.Id > 0)
        //    {
        //        List<NodalCourseRequests> nodalCourseRequestsExistsList = await _db.NodalCourseRequests.Where(x => x.UserId == aPINodalUser.Id && x.CourseId == aPINodalUserSignUp.CourseId && x.IsDeleted == false && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
        //        if (nodalCourseRequestsExistsList.Count > 0)
        //        {
        //            if (nodalCourseRequestsExistsList.Where(x => x.IsApprovedByNodal == true).Count() > 0)
        //                aPINodalUser.Response = -9;
        //            else
        //                aPINodalUser.Response = -10;
        //        }
        //        else
        //        {
        //            Course course = await _db.Course.Where(x => x.Id == aPINodalUserSignUp.CourseId && x.IsDeleted == false).FirstOrDefaultAsync();

        //            //Add course request
        //            NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
        //            nodalCourseRequest.CourseId = aPINodalUserSignUp.CourseId;
        //            nodalCourseRequest.UserId = aPINodalUser.Id;
        //            nodalCourseRequest.RequestType = NodalCourseRequest.Individual;
        //            nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = UserId;
        //            nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
        //            nodalCourseRequest.CourseFee = nodalCourseRequest.CourseFee;
        //            _db.NodalCourseRequests.Add(nodalCourseRequest);
        //            await _db.SaveChangesAsync();

        //            UserMasterLogs _userMasterLogs = new UserMasterLogs();
        //            _userMasterLogs.ModifiedBy = UserId;
        //            _userMasterLogs.IsInserted = 1;
        //            _userMasterLogs.UserId = aPINodalUser.Id;
        //            await this.AddUserMasterLogs(_userMasterLogs);

        //            await SendEmailAfterCreatingUser(aPINodalUser, UserId);

        //            await SendEmailAfterAddingUser(aPINodalUser);
        //        }
        //    }
        //    return aPINodalUser.Response;
        //}
        //public async Task<APINodalUser> AddUser(APINodalUser Apiuser, string ConnectionString)
        //{
        //    string OrgCode = Apiuser.OrganizationCode;
        //    Exists exist = await this.Validations(Apiuser);
        //    if (!exist.Equals(Exists.No))
        //    {
        //        if (exist.Equals(Exists.UserIdExist))
        //        {
        //            Apiuser.Response = -1;
        //            var userMasterId = await (from user in _db.UserMaster
        //                                     join userdetails in _db.UserMasterDetails on user.Id equals userdetails.UserMasterId
        //                                     where user.IsDeleted == false && userdetails.IsDeleted == false
        //                                     && user.UserId == Security.Encrypt(Apiuser.UserId) && user.EmailId == Security.Encrypt(Apiuser.EmailId)
        //                                     && userdetails.MobileNumber == Security.Encrypt(Apiuser.MobileNumber)
        //                                     && userdetails.AadharNumber == Security.Encrypt(Apiuser.AadharNumber)
        //                                     && user.UserName == Apiuser.UserName && userdetails.FHName == Apiuser.FHName
        //                                     && userdetails.ConfigurationColumn12 == Apiuser.ConfigurationColumn12Id
        //                                      select user.Id).FirstOrDefaultAsync();
        //            if (userMasterId > 0)
        //                Apiuser.Id = userMasterId;
        //        }
        //        else if (exist.Equals(Exists.EmailIdExist))
        //            Apiuser.Response = -5;
        //        else if (exist.Equals(Exists.MobileExist))
        //            Apiuser.Response = -6;
        //        else if (exist.Equals(Exists.AadharExist))
        //            Apiuser.Response = -8;

        //        return Apiuser;
        //    }
        //    else
        //    {
        //        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
        //        Apiuser.AccountCreatedDate = DateTime.UtcNow;
        //        Apiuser.CreatedDate = DateTime.UtcNow;
        //        Apiuser.ModifiedDate = DateTime.UtcNow;


        //        var allowRANDOMPASSWORD = await this.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode, ConnectionString);
        //        if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
        //        {
        //            if (OrgCode.ToLower().Contains("keventers"))
        //            {
        //                Apiuser.RandomUserPassword = "Keventers@123";
        //                Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
        //            }
        //            else
        //            {
        //                if (OrgCode.ToLower() == "pepperfry")
        //                    Apiuser.Password = Helper.Security.EncryptSHA512("123456");
        //                else
        //                    Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
        //            }
        //        }
        //        else
        //        {
        //            Apiuser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
        //            Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
        //        }

        //        if (Apiuser.MobileNumber != "")
        //        {
        //            if (Apiuser.MobileNumber.Length < 10)
        //            {
        //                Apiuser.Response = -2;
        //                return Apiuser;
        //            }
        //        }

        //        Apiuser.MobileNumber = (Apiuser.MobileNumber == "" ? null : Security.Encrypt(Apiuser.MobileNumber));
        //        Apiuser.EmailId = (Apiuser.EmailId == "" ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

        //        Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
        //        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
        //        Apiuser.Id = 0;
        //        if (OrgCode.ToLower() == "spectra")
        //        {
        //            Apiuser.TermsCondintionsAccepted = true;
        //            Apiuser.IsPasswordModified = true;
        //            Apiuser.AcceptanceDate = DateTime.UtcNow;
        //            Apiuser.PasswordModifiedDate = DateTime.UtcNow;
        //        }
        //        else
        //        {
        //            Apiuser.TermsCondintionsAccepted = false;
        //            Apiuser.IsPasswordModified = false;
        //        }

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn7) && (Apiuser.ConfigurationColumn7Id == null || Apiuser.ConfigurationColumn7Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn7 = Apiuser.ConfigurationColumn7.Trim();
        //            Configure7 configure7 = await _db.Configure7.Where(x => x.Name == Apiuser.ConfigurationColumn7 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure7 == null)
        //            {
        //                Configure7 configure77 = new Configure7();
        //                configure77.Name = Apiuser.ConfigurationColumn7;
        //                configure77.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn7);
        //                configure77.CreatedDate = DateTime.UtcNow;
        //                _db.Configure7.Add(configure77);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn7Id = configure77.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn7Id = configure7.Id;
        //        }

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn10) && (Apiuser.ConfigurationColumn10Id == null || Apiuser.ConfigurationColumn10Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn10 = Apiuser.ConfigurationColumn10.Trim();
        //            Configure10 configure10 = await _db.Configure10.Where(x => x.Name == Apiuser.ConfigurationColumn10 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure10 == null)
        //            {
        //                Configure10 configure101 = new Configure10();
        //                configure101.Name = Apiuser.ConfigurationColumn10;
        //                configure101.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn10);
        //                configure101.CreatedDate = DateTime.UtcNow;
        //                _db.Configure10.Add(configure101);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn10Id = configure101.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn10Id = configure10.Id;
        //        }

        //        Apiuser.ConfigurationColumn12 = await _db.Configure12.Where(x => x.Id == Apiuser.ConfigurationColumn12Id && x.IsDeleted == 0).Select(x => x.Name).FirstOrDefaultAsync();
        //        Apiuser.UserType = "Internal";
        //        Apiuser = await this.AddUserToDb(Apiuser);

        //        return Apiuser;
        //    }
        //}
        //public async Task<APITTUser> AddTTUser(APITTUser Apiuser, string ConnectionString)
        //{
        //    string OrgCode = Apiuser.OrganizationCode;
        //    Exists exist = await this.TTGroupValidations(Apiuser);
        //    if (!exist.Equals(Exists.No))
        //    {
        //        if (exist.Equals(Exists.UserIdExist))
        //        {
        //            Apiuser.Response = -1;
        //            var userMasterId = await (from user in _db.UserMaster
        //                                      join userdetails in _db.UserMasterDetails on user.Id equals userdetails.UserMasterId
        //                                      where user.IsDeleted == false && userdetails.IsDeleted == false
        //                                      && user.UserId == Security.Encrypt(Apiuser.UserId) && user.EmailId == Security.Encrypt(Apiuser.EmailId)
        //                                      && userdetails.MobileNumber == Security.Encrypt(Apiuser.MobileNumber)                                             
        //                                      && user.UserName == Apiuser.UserName                                              
        //                                      select user.Id).FirstOrDefaultAsync();
        //            if (userMasterId > 0)
        //                Apiuser.Id = userMasterId;
        //        }
        //        else if (exist.Equals(Exists.EmailIdExist))
        //            Apiuser.Response = -5;
        //        else if (exist.Equals(Exists.MobileExist))
        //            Apiuser.Response = -6;
        //        else if (exist.Equals(Exists.AadharExist))
        //            Apiuser.Response = -8;

        //        return Apiuser;
        //    }
        //    else
        //    {
        //        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
        //        Apiuser.AccountCreatedDate = DateTime.UtcNow;
        //        Apiuser.CreatedDate = DateTime.UtcNow;
        //        Apiuser.ModifiedDate = DateTime.UtcNow;


        //        var allowRANDOMPASSWORD = await this.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode, ConnectionString);
        //        if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
        //        {

        //                    Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);

        //        }
        //        else
        //        {
        //            Apiuser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
        //            Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
        //        }

        //        if (Apiuser.MobileNumber != "")
        //        {
        //            if (Apiuser.MobileNumber.Length < 10)
        //            {
        //                Apiuser.Response = -2;
        //                return Apiuser;
        //            }
        //        }

        //        Apiuser.MobileNumber = (Apiuser.MobileNumber == "" ? null : Security.Encrypt(Apiuser.MobileNumber));
        //        Apiuser.EmailId = (Apiuser.EmailId == "" ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

        //        Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
        //        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
        //        Apiuser.Id = 0;

        //            Apiuser.TermsCondintionsAccepted = false;
        //            Apiuser.IsPasswordModified = false;


        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn1) && (Apiuser.ConfigurationColumn1Id == null || Apiuser.ConfigurationColumn1Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn1 = Apiuser.ConfigurationColumn1.Trim();
        //            Configure1 configure1 = await _db.Configure1.Where(x => x.Name == Apiuser.ConfigurationColumn1 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure1 == null)
        //            {
        //                Configure1 configure11 = new Configure1();
        //                configure11.Name = Apiuser.ConfigurationColumn1;
        //                configure11.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn1);
        //                configure11.CreatedDate = DateTime.UtcNow;
        //                _db.Configure1.Add(configure11);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn1Id = configure11.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn1Id = configure1.Id;
        //        }

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn11) && (Apiuser.ConfigurationColumn11Id == null || Apiuser.ConfigurationColumn11Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn11 = Apiuser.ConfigurationColumn11.Trim();
        //            Configure11 configure11 = await _db.Configure11.Where(x => x.Name == Apiuser.ConfigurationColumn11 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure11 == null)
        //            {
        //                Configure11 configure111 = new Configure11();
        //                configure111.Name = Apiuser.ConfigurationColumn11;
        //                configure111.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn11);
        //                configure111.CreatedDate = DateTime.UtcNow;
        //                _db.Configure11.Add(configure111);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn11Id = configure111.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn11Id = configure11.Id;
        //        }

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn12) && (Apiuser.ConfigurationColumn12Id == null || Apiuser.ConfigurationColumn11Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn12 = Apiuser.ConfigurationColumn12.Trim();
        //            Configure12 configure12 = await _db.Configure12.Where(x => x.Name == Apiuser.ConfigurationColumn12 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure12 == null)
        //            {
        //                Configure12 configure112 = new Configure12();
        //                configure112.Name = Apiuser.ConfigurationColumn12;
        //                configure112.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn12);
        //                configure112.CreatedDate = DateTime.UtcNow;
        //                _db.Configure12.Add(configure112);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn12Id = configure112.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn12Id = configure12.Id;
        //        }


        //        Apiuser.UserType = "Internal";
        //        Apiuser = await this.AddTTUserToDb(Apiuser);

        //        return Apiuser;
        //    }
        //}

        //public async Task<Exists> Validations(APINodalUser user)
        //{
        //    if (await this.IsExists(user.UserId))
        //        return Exists.UserIdExist;
        //    if (!string.IsNullOrEmpty(user.EmailId))
        //    {
        //        if (await this.EmailExists(user.EmailId))
        //            return Exists.EmailIdExist;
        //    }
        //    if (!string.IsNullOrEmpty(user.MobileNumber))
        //    {
        //        if (await this.MobileExists(user.MobileNumber))
        //            return Exists.MobileExist;
        //    }
        //    if (!string.IsNullOrEmpty(user.AadharNumber))
        //    {
        //        if (await this.AadharExists(user.AadharNumber))
        //            return Exists.AadharExist;
        //    }
        //    return Exists.No;
        //}
        //public async Task<Exists> TTGroupValidations(APITTUser user)
        //{
        //    if (await this.IsExists(user.UserId))
        //        return Exists.UserIdExist;
        //    if (!string.IsNullOrEmpty(user.EmailId))
        //    {
        //        if (await this.EmailExists(user.EmailId))
        //            return Exists.EmailIdExist;
        //    }
        //    if (!string.IsNullOrEmpty(user.MobileNumber))
        //    {
        //        if (await this.MobileExists(user.MobileNumber))
        //            return Exists.MobileExist;
        //    }
        //    _logger.Info("TTGroupValidations");
        //    return Exists.No;
        //}
        //public async Task<bool> IsExists(string userId)
        //{
        //    if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId == Security.Encrypt(userId.ToLower())).CountAsync() > 0)
        //        return true;
        //    return false;
        //}
        //public async Task<bool> EmailExists(string emailId)
        //{
        //    if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == Security.Encrypt(emailId.ToLower())) > 0)
        //        return true;
        //    return false;
        //}
        //public async Task<bool> MobileExists(string mobileNumber)
        //{
        //    if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.MobileNumber == Security.Encrypt(mobileNumber)) > 0)
        //        return true;
        //    return false;
        //}
        //public async Task<bool> AadharExists(string aadharNumber)
        //{
        //    if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.AadharNumber == Security.Encrypt(aadharNumber)) > 0)
        //        return true;
        //    return false;
        //}
        //public async Task<int> GetTotalUserCount()
        //{
        //    return await _db.UserMaster.CountAsync();
        //}
        public async Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string ConnectionString, string defaultValue = "")
        {
            DataTable dtConfigurationValues;
            string configValue;
            try
            {
                var cache = new CacheManager.CacheManager();
                string cacheKeyConfig = (Helper.Constants.CacheKeyNames.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

                if (cache.IsAdded(cacheKeyConfig))
                    dtConfigurationValues = cache.Get<DataTable>(cacheKeyConfig);
                else
                {
                    dtConfigurationValues = this.GetAllConfigurableParameterValue(ConnectionString);
                    cache.Add(cacheKeyConfig, dtConfigurationValues, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
                DataRow[] dr = dtConfigurationValues.Select("Code ='" + configurationCode + "'");
                if (dr.Length > 0)
                    configValue = Convert.ToString(dr[0]["Value"]);
                else
                    configValue = defaultValue;
                _logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
            }
            catch (System.Exception ex)
            {
                _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
                return null;
            }
            return configValue;
        }
        public DataTable GetAllConfigurableParameterValue(string ConnectionString)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext(ConnectionString))
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllConfigurableParameterValues";
                        cmd.CommandType = CommandType.StoredProcedure;
                        DbDataReader reader = cmd.ExecuteReader();
                        dt.Load(reader);

                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            { _logger.Error("Exception in function GetAllConfigurableParameterValue :-" + Utilities.GetDetailedException(ex)); }

            return dt;
        }
        //public async Task<APINodalUser> AddUserToDb(APINodalUser apiUser)
        //{
        //    using (var transaction = this._context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            UserMaster User = Mapper.Map<UserMaster>(apiUser);
        //            UserMasterDetails UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
        //            await this._db.UserMaster.AddAsync(User);
        //            await this._db.SaveChangesAsync();
        //            UserDetails.UserMasterId = User.Id;

        //            if (UserDetails.UserMasterId > 0)
        //            {
        //                var Num = User.Id % 10;
        //                switch (Num)
        //                {
        //                    case 1:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 2:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 3:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 4:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 5:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 6:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 7:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 8:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 9:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    default:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                }

        //                UserDetails.IsActive = User.IsActive;
        //                UserDetails.IsDeleted = User.IsDeleted;
        //                UserDetails.CreatedBy = UserDetails.ModifiedBy = User.Id;
        //                apiUser.Id = apiUser.Response = User.Id;
        //                await this._db.UserMasterDetails.AddAsync(UserDetails);
        //                await this._db.SaveChangesAsync();
        //                transaction.Commit();
        //                return apiUser;
        //            }
        //            else
        //            {
        //                transaction.Rollback();
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            _logger.Error(Utilities.GetDetailedException(ex));
        //            transaction.Rollback();
        //            throw ex;
        //        }
        //    }
        //    apiUser.Id = apiUser.Response = 0;
        //    return apiUser;
        //}
        //public async Task<APITTUser> AddTTUserToDb(APITTUser apiUser)
        //{
        //    using (var transaction = this._context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            _logger.Info("AddTTUserToDb");
        //            UserMaster User = Mapper.Map<UserMaster>(apiUser);
        //            UserMasterDetails UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
        //            await this._db.UserMaster.AddAsync(User);
        //            await this._db.SaveChangesAsync();
        //            UserDetails.UserMasterId = User.Id;

        //            if (UserDetails.UserMasterId > 0)
        //            {
        //                var Num = User.Id % 10;
        //                switch (Num)
        //                {
        //                    case 1:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 2:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 3:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 4:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 5:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 6:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 7:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 8:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 9:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    default:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                }

        //                UserDetails.IsActive = User.IsActive;
        //                UserDetails.IsDeleted = User.IsDeleted;
        //                UserDetails.CreatedBy = UserDetails.ModifiedBy = User.Id;
        //                apiUser.Id = apiUser.Response = User.Id;
        //                await this._db.UserMasterDetails.AddAsync(UserDetails);
        //                await this._db.SaveChangesAsync();
        //                transaction.Commit();
        //                return apiUser;
        //            }
        //            else
        //            {
        //                transaction.Rollback();
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            _logger.Error(Utilities.GetDetailedException(ex));
        //            transaction.Rollback();
        //            throw ex;
        //        }
        //    }
        //    apiUser.Id = apiUser.Response = 0;
        //    return apiUser;
        //}
        //public async Task<int> AddUserMasterLogs(UserMasterLogs userMasterLogs)
        //{
        //    UserMasterLogs _userMasterLogs = new UserMasterLogs();
        //    _userMasterLogs.UserId = userMasterLogs.UserId;
        //    _userMasterLogs.ModifiedDate = DateTime.Now;
        //    _userMasterLogs.ModifiedBy = userMasterLogs.ModifiedBy;
        //    _userMasterLogs.IsUpdated = userMasterLogs.IsUpdated;
        //    _userMasterLogs.IsDeleted = userMasterLogs.IsDeleted;
        //    _userMasterLogs.IsInserted = userMasterLogs.IsInserted;
        //    await this._db.UserMasterLogs.AddAsync(_userMasterLogs);
        //    await this._db.SaveChangesAsync();
        //    return 1;

        //}
        //private async Task<int> SendEmailAfterAddingUser(APINodalUser Apiuser)
        //{
        //    var result = (from um in _db.UserMaster
        //                  join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
        //                  where
        //                    um.IsDeleted == false && um.IsActive == true
        //                    && umd.ConfigurationColumn12 == Apiuser.ConfigurationColumn12Id
        //                    && (um.UserRole == "NO" || um.UserRole == "CA")
        //                  select new APINodalUserDetails
        //                  {
        //                      NodalUserId = um.Id,
        //                      NodalUserName = um.UserName,
        //                      NodalEmailID = um.EmailId,
        //                      NodalMobileNumber = umd.MobileNumber,
        //                      CourseTitle = Apiuser.Title
        //                  });
        //    List<APINodalUserDetails> aPINodalUserDetailsList = await result.ToListAsync();
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
        //private async Task<int> SendEmailAfterCreatingUser(APINodalUser Apiuser, int UserId)
        //{
        //    try
        //    {
        //        var result = (from um in _db.UserMaster
        //                      join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
        //                      where
        //                        um.IsDeleted == false && um.IsActive == true
        //                        && umd.ConfigurationColumn12 == Apiuser.ConfigurationColumn12Id
        //                        && (um.UserRole == "NO" || um.UserRole == "GA" || um.UserRole == "CA") && um.Id == UserId
        //                      select new APINodalUserDetails
        //                      {
        //                          NodalUserId = um.Id,
        //                          NodalUserName = um.UserName,
        //                          NodalEmailID = um.EmailId,
        //                          NodalMobileNumber = umd.MobileNumber,
        //                          CourseTitle = Apiuser.Title,
        //                          UserMasterId = Apiuser.Id,
        //                          UserId = Apiuser.UserId,
        //                          UserName = Apiuser.UserName,
        //                          EmailID = Apiuser.EmailId,
        //                          MobileNumber = Apiuser.MobileNumber,
        //                          AirPort = Apiuser.ConfigurationColumn12,
        //                          RegistrationDate = (DateTime)Apiuser.AccountCreatedDate,
        //                          OrgCode = Apiuser.OrganizationCode
        //                      });

        //        List<APINodalUserDetails> aPINodalUserDetailsList = await result.ToListAsync();

        //        _email.UserCreationMailByNodalOfficerToUser(aPINodalUserDetailsList);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    return 1;
        //}
        public async Task<int> CheckRequest(int RequestId, string UserIds, string ConnectionString)
        {
            this.ChangeDbContext(ConnectionString);
            if (string.IsNullOrEmpty(UserIds))
            {
                NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == RequestId && x.RequestType == NodalCourseRequest.Individual && x.IsApprovedByNodal == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (nodalCourseRequest == null)
                    return 0;
                else
                {
                    if (nodalCourseRequest.IsPaymentDone == false)
                        return 1;
                    else
                        return -1;
                }
            }
            else
            {
                string[] Users = UserIds.Split(',');
                List<int> UserMasterIds = new List<int>();
                foreach (string item in Users)
                    UserMasterIds.Add(Convert.ToInt32(item));

                List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.GroupId == RequestId && x.RequestType == NodalCourseRequest.Group && x.IsApprovedByNodal == true && x.IsDeleted == false && UserMasterIds.Contains(x.UserId)).ToListAsync();
                if (nodalCourseRequests.Count() > 0)
                {
                    if (nodalCourseRequests.Where(x => x.IsPaymentDone == false).Count() > 0)
                        return 1;
                    else
                        return -1;
                }
                else
                    return 0;
            }
        }
        public async Task<string> MakePayment(int RequestId, string UserIds, string ConnectionString, string OrgCode)
        {
            this.ChangeDbContext(ConnectionString);
            string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "html");

            if (!Directory.Exists(sWebRootFolder))
                Directory.CreateDirectory(sWebRootFolder);

            string sFileName = Guid.NewGuid().ToString("N") + ".html";
            string DomainName = _configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}/{3}", DomainName, OrgCode, "html", sFileName);
            string filePath = Path.Combine(sWebRootFolder, sFileName);
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
                file.Delete();

            MerchantParams merchantParams = await InitializePaymentRequest(RequestId, UserIds, sFileName);
            string encryptedRequest = EncryptPaymentRequest(merchantParams);

            APIPaymentRequestData aPIPaymentRequestData = new APIPaymentRequestData();
            aPIPaymentRequestData.Url = _configuration["IAAPaymentUrl"];
            aPIPaymentRequestData.requestData = encryptedRequest;
            aPIPaymentRequestData.merchantId = merchantParams.merchantId;

            string formId = "onePayForm";

            StringBuilder htmlForm = new StringBuilder();
            htmlForm.AppendLine("<html>");
            htmlForm.AppendLine(String.Format("<body onload='document.forms[\"{0}\"].submit()'>", formId));
            htmlForm.AppendLine(String.Format("<form id='{0}' method='POST' action='{1}'>", formId, aPIPaymentRequestData.Url));
            htmlForm.AppendLine(String.Format("<input type='hidden' name='reqData' id='reqData' value='{0}' />", aPIPaymentRequestData.requestData));
            htmlForm.AppendLine(String.Format("<input type='hidden' name='merchantId' id='merchantId' value='{0}' />", aPIPaymentRequestData.merchantId));
            htmlForm.AppendLine("</form>");
            htmlForm.AppendLine("</body>");
            htmlForm.AppendLine("</html>");

            File.WriteAllText(filePath, htmlForm.ToString());

            return URL;
        }

        private async Task<MerchantParams> InitializePaymentRequest(int RequestId, string UserIds, string filePath)
        {
            float CourseFees = 0;
            if (string.IsNullOrEmpty(UserIds))
            {

                NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == RequestId && x.IsDeleted == false && x.IsApprovedByNodal == true).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(Convert.ToString(nodalCourseRequest.CourseFee)))
                    CourseFees = nodalCourseRequest.CourseFee;

                if (CourseFees == 0)
                    CourseFees = 1;

                var UserDetails = await (from um in _db.UserMaster
                                         join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                                         where um.Id == nodalCourseRequest.UserId && um.IsDeleted == false
                                              && umd.IsDeleted == false
                                         select new
                                         {
                                             Id = um.Id,
                                             UserId = um.UserId,
                                             UserName = um.UserName,
                                             EmailId = um.EmailId,
                                             MobileNumber = umd.MobileNumber
                                         }).FirstOrDefaultAsync();
                string TransactionId = CreateUnique16DigitString();
                MerchantParams merchantParams = new MerchantParams()
                {
                    merchantId = _configuration["MerchantId"],
                    apiKey = _configuration["ApiKey"],
                    txnId = TransactionId,
                    amount = string.Format("{0:0.00}", CourseFees),
                    dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    custMail = Security.Decrypt(UserDetails.EmailId),
                    custMobile = Security.Decrypt(UserDetails.MobileNumber),
                    udf1 = Security.Encrypt(UserDetails.Id.ToString()),
                    udf2 = Security.Encrypt(TransactionId),
                    returnURL = string.Format("{0}{1}", _configuration["ApiGatewayUrl"], "Payment"),
                    isMultiSettlement = "0",
                    productId = "DEFAULT",
                    channelId = "0",
                    txnType = "DIRECT",
                    udf3 = Security.Encrypt(Convert.ToString(RequestId)),
                    udf4 = Security.Encrypt(filePath),
                    udf5 = Security.Encrypt(NodalCourseRequest.Individual),
                    instrumentId = "NA",
                    cardDetails = "NA",
                    cardType = "NA"
                };

                return merchantParams;
            }
            else
            {
                string[] Users = UserIds.Split(',');
                List<int> UserMasterIds = new List<int>();
                foreach (string item in Users)
                    UserMasterIds.Add(Convert.ToInt32(item));

                List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => x.GroupId == RequestId && x.RequestType == NodalCourseRequest.Group && x.IsApprovedByNodal == true && x.IsDeleted == false && UserMasterIds.Contains(x.UserId) && x.IsPaymentDone == false).ToListAsync();

                if (!string.IsNullOrEmpty(Convert.ToString(nodalCourseRequests.Select(x => x.CourseFee).FirstOrDefault())))
                    CourseFees = nodalCourseRequests.Select(x => x.CourseFee).FirstOrDefault();

                if (CourseFees == 0)
                    CourseFees = 1;

                int CourseId = nodalCourseRequests.Select(x => x.CourseId).FirstOrDefault();
                int NosUsers = nodalCourseRequests.Count();
                float GroupCourseFees = CourseFees * NosUsers;

                int UserId = nodalCourseRequests.Select(x => x.CreatedBy).FirstOrDefault();

                var UserDetails = await (from um in _db.UserMaster
                                         join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                                         where um.Id == UserId && um.IsDeleted == false
                                              && umd.IsDeleted == false
                                         select new
                                         {
                                             Id = um.Id,
                                             UserId = um.UserId,
                                             UserName = um.UserName,
                                             EmailId = um.EmailId,
                                             MobileNumber = umd.MobileNumber
                                         }).FirstOrDefaultAsync();

                string RequestIds = string.Join(",", nodalCourseRequests.Select(x => x.Id).ToList());
                string TransactionId = CreateUnique16DigitString();
                MerchantParams merchantParams = new MerchantParams()
                {
                    merchantId = _configuration["MerchantId"],
                    apiKey = _configuration["ApiKey"],
                    txnId = TransactionId,
                    amount = string.Format("{0:0.00}", GroupCourseFees),
                    dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    custMail = Security.Decrypt(UserDetails.EmailId),
                    custMobile = Security.Decrypt(UserDetails.MobileNumber),
                    udf1 = Security.Encrypt(UserDetails.Id.ToString()),
                    udf2 = Security.Encrypt(TransactionId),
                    returnURL = string.Format("{0}{1}", _configuration["ApiGatewayUrl"], "Payment"),
                    isMultiSettlement = "0",
                    productId = "DEFAULT",
                    channelId = "0",
                    txnType = "DIRECT",
                    udf3 = Security.Encrypt(Convert.ToString(RequestIds)),
                    udf4 = Security.Encrypt(filePath),
                    udf5 = Security.Encrypt(NodalCourseRequest.Group),
                    instrumentId = "NA",
                    cardDetails = "NA",
                    cardType = "NA"
                };

                return merchantParams;

            }
        }

        private static HashSet<string> Results = new HashSet<string>();

        public string CreateUnique16DigitString()
        {
            var result = Create16DigitString();
            while (!Results.Add(result))
            {
                result = Create16DigitString();
            }
            return result;
        }
        private Random RNG = new Random();
        public string Create16DigitString()
        {
            var builder = new StringBuilder();
            while (builder.Length < 16)
                builder.Append(RNG.Next(0, 10).ToString());
            return builder.ToString();
        }

        public string EncryptPaymentRequest(MerchantParams merchantParams)
        {
            string merchantEncryptionKey = _configuration["ApiKey"]; //16 Charachter String
            string encryptedText = string.Empty;

            try
            {
                string original = JsonConvert.SerializeObject(merchantParams);  //"Here is some data to encrypt!";
                _logger.Debug("Request-" + original);
                UTF8Encoding UTF8 = new UTF8Encoding();
                AesManaged tdes = new AesManaged();
                tdes.Key = UTF8.GetBytes(merchantEncryptionKey);
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform crypt = tdes.CreateEncryptor();
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
        public static async Task<PayproceesResponseClass> PostFormUrlEncoded<TResult>(string url, IEnumerable<KeyValuePair<string, string>> postData)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                    HttpResponseMessage responseMessage = httpClient.PostAsync(url, content).Result;

                    string response = await responseMessage.Content.ReadAsStringAsync();
                    PayproceesResponseClass payproceesResponseClass = JsonConvert.DeserializeObject<PayproceesResponseClass>(response);
                    return payproceesResponseClass;
                }
            }
        }
        public async Task<PaymentResponseMessage> ProcessResponse(string merchantParamsJson, string ConnectionString, string OrgCode)
        {
            this.ChangeDbContext(ConnectionString);
            PaymentResponseMessage responseMessage = new PaymentResponseMessage();
            PayproceesResponseClass response = DecreptePaymentRespone(merchantParamsJson);
            _logger.Debug("Response-" + JsonConvert.SerializeObject(response));

            string verficationUrl = _configuration["VerificationUrl"];
            IEnumerable<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("merchantId",response.merchant_id),
                new KeyValuePair<string, string>("txnId",response.txn_id)
            };

            PayproceesResponseClass verifyResponse = await PostFormUrlEncoded<HttpResponseMessage>(verficationUrl, postData);
            _logger.Debug("Verification Response-" + JsonConvert.SerializeObject(response));

            if (verifyResponse.trans_status == "To" || verifyResponse.trans_status == "F")
            {
                DualVerification dualVerification = new DualVerification();
                dualVerification.Elapsed += ReverifyPaymentStatus;
                dualVerification.MerchantId = verifyResponse.merchant_id;
                dualVerification.TransactionId = verifyResponse.txn_id;
                dualVerification.ConnectionString = ConnectionString;
                dualVerification.OrgCode = OrgCode;
                dualVerification.Interval = (Convert.ToInt32(_configuration["IntervalMinutes"]) * 60 * 1000);
                dualVerification.AutoReset = false;
                dualVerification.Start();
            }

            //Payment response write to database.
            PaymentResponse paymentResponseExists = await _db.PaymentResponse.Where(x => x.txn_id == verifyResponse.txn_id).FirstOrDefaultAsync();
            if (paymentResponseExists != null)
            {
                responseMessage.Message = "Order Number Duplicate";
                responseMessage.Description = "Transaction Order Number is Duplicate.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;

                return responseMessage;
            }

            PaymentResponse paymentResponse = Mapper.Map<PaymentResponse>(verifyResponse);
            _db.PaymentResponse.Add(paymentResponse);
            await _db.SaveChangesAsync();

            int UserId = Convert.ToInt32(Security.Decrypt(verifyResponse.udf1));

            responseMessage.OrderNumber = paymentResponse.txn_id;
            responseMessage.OrderAmount = paymentResponse.txn_amount;
            responseMessage.UserName = await _db.UserMaster.Where(x => x.Id == UserId && x.IsDeleted == false).Select(x => x.UserName).FirstOrDefaultAsync();

            string RequestTransactionId = Security.Decrypt(verifyResponse.udf2);
            if (RequestTransactionId != verifyResponse.txn_id)
            {
                responseMessage.Message = "Invalid Order Number";
                responseMessage.Description = "Transaction Order Number Not Matched.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
            }
            else if (verifyResponse.trans_status == "F")
            {
                if (verifyResponse.resp_message == "Customer Cancelled")
                    responseMessage.Message = "Customer Cancelled";
                else
                    responseMessage.Message = "Transaction Failed";
                responseMessage.Description = verifyResponse.resp_message;
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
                responseMessage.TransactionId = paymentResponse.txn_id;
            }
            else if (verifyResponse.trans_status == "To")
            {
                responseMessage.Message = "Transaction Timeout";
                responseMessage.Description = verifyResponse.resp_message;
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
                responseMessage.TransactionId = paymentResponse.txn_id;
            }
            else if (verifyResponse.trans_status == "Ok")
            {
                responseMessage.Message = "Transaction Successful";
                responseMessage.Description = "Your transaction has been successful.";
                responseMessage.StatusCode = StatusCodes.Status200OK;
                responseMessage.TransactionId = paymentResponse.txn_id;

                string RequestIds = Security.Decrypt(verifyResponse.udf3);
                string fileName = Security.Decrypt(verifyResponse.udf4);
                string RequestType = Security.Decrypt(verifyResponse.udf5);

                string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "html");
                string filePath = Path.Combine(sWebRootFolder, fileName);
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                    file.Delete();

                if (RequestType == NodalCourseRequest.Individual)
                {
                    int RequestId = Convert.ToInt32(RequestIds);
                    NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == RequestId && x.IsApprovedByNodal == true && x.IsDeleted == false).FirstOrDefaultAsync();
                    if (nodalCourseRequest != null)
                    {
                        nodalCourseRequest.IsPaymentDone = true;
                        _db.NodalCourseRequests.Update(nodalCourseRequest);
                        await _db.SaveChangesAsync();

                        UserMaster user = await _db.UserMaster.Where(x => x.Id == nodalCourseRequest.UserId && x.IsDeleted == false && x.IsActive == false).FirstOrDefaultAsync();
                        if (user != null)
                        {
                            user.IsActive = true;
                            _db.UserMaster.Update(user);
                            await _db.SaveChangesAsync();
                        }
                        UserMasterDetails userDetails = await _db.UserMasterDetails.Where(x => x.UserMasterId == nodalCourseRequest.UserId && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (userDetails != null)
                        {
                            userDetails.IsActive = true;
                            userDetails.AppearOnLeaderboard = true;
                            _db.UserMasterDetails.Update(userDetails);
                            await _db.SaveChangesAsync();
                        }
                        string CourseTitle = _db.Course.Where(a => a.Id == nodalCourseRequest.CourseId).Select(a => a.Title).SingleOrDefault();
                        _logger.Debug("user active done");
                        AccessibilityRule accessibilityRule = await _db.AccessibilityRule.Where(x => x.CourseId == nodalCourseRequest.CourseId && x.UserID == nodalCourseRequest.UserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (accessibilityRule == null)
                        {
                            _logger.Debug("Applicability not found. Adding Applicability");
                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };
                            _db.AccessibilityRule.Add(accessibilityRules);
                            await _db.SaveChangesAsync();
                            _logger.Debug("accessibility done");
                            try
                            {
                                #region "Send Email Notifications"
                                string url = _configuration[Configuration.NotificationApi];

                                url = url + "/CourseApplicability";
                                JObject oJsonObject = new JObject();
                                oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                                oJsonObject.Add("OrganizationCode", OrgCode);
                                HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                                _logger.Debug("Applicability Response" + responses);
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Bell Notifications"
                                bool IsApplicableToAll = false;
                                int notificationID = 0;

                                List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId), ConnectionString);

                                if (aPINotification.Count > 0)
                                    notificationID = aPINotification.FirstOrDefault().Id;
                                else
                                {
                                    _logger.Debug("Adding applicability bell notification");
                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = CourseTitle;
                                    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                    Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                    Notification.Url = "myCourseModule/" + accessibilityRules.CourseId;
                                    Notification.Type = Record.Course;
                                    Notification.UserId = nodalCourseRequest.UserId;
                                    Notification.CourseId = accessibilityRules.CourseId;
                                    notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, ConnectionString);
                                }
                                DataTable dtUserIds = new DataTable();
                                dtUserIds.Columns.Add("UserIds");
                                dtUserIds.Rows.Add(nodalCourseRequest.UserId);

                                if (dtUserIds.Rows.Count > 0)
                                    await this.SendDataForApplicableNotifications(notificationID, dtUserIds, nodalCourseRequest.UserId, ConnectionString);
                                _logger.Debug("added applicability bell notification");
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Push Notifications"
                                string url = string.Empty;
                                url = _configuration[Configuration.NotificationApi];
                                url += "/CourseApplicabilityPushNotification";
                                JObject Pushnotification = new JObject();
                                Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                                Pushnotification.Add("OrganizationCode", OrgCode);

                                HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                            try
                            {
                                #region "Send SMS Notifications"
                                string urlSMS = string.Empty;
                                var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", OrgCode, ConnectionString);
                                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                                {
                                    urlSMS = _configuration[Configuration.NotificationApi];
                                    urlSMS += "/CourseApplicabilitySMS";
                                    JObject oJsonObjectSMS = new JObject();
                                    oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                                    oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                                    HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                        }
                        else
                        {
                            _logger.Debug("applicab found.");
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = null;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequest.UserId;
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = OrgCode;
                        aPIPaymentMailDetails.Password = _configuration["DeafultPassword"];

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }
                else if (RequestType == NodalCourseRequest.Group)
                {
                    List<string> reqIds = RequestIds.Split(',').ToList();
                    List<int> RequestIdList = new List<int>();
                    foreach (string item in reqIds)
                        RequestIdList.Add(Convert.ToInt32(item));

                    List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => RequestIdList.Contains(x.Id) && x.RequestType == NodalCourseRequest.Group && x.IsApprovedByNodal == true && x.IsDeleted == false).ToListAsync();
                    List<AccessibilityRule> accessibilityRulesList = new List<AccessibilityRule>();
                    if (nodalCourseRequests != null && nodalCourseRequests.Count() > 0)
                    {
                        foreach (NodalCourseRequests nodalCourseRequest in nodalCourseRequests)
                        {
                            nodalCourseRequest.IsPaymentDone = true;

                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };

                            accessibilityRulesList.Add(accessibilityRules);

                        }

                        _db.NodalCourseRequests.UpdateRange(nodalCourseRequests);
                        await _db.SaveChangesAsync();

                        _db.AccessibilityRule.AddRange(accessibilityRulesList);
                        await _db.SaveChangesAsync();

                        int CId = (int)accessibilityRulesList.Select(x => x.CourseId).FirstOrDefault();
                        string CourseTitle = await _db.Course.Where(a => a.Id == CId).Select(a => a.Title).FirstOrDefaultAsync();
                        string GroupCode = await _db.NodalUserGroups.Where(a => a.Id == nodalCourseRequests.Select(x => x.GroupId).FirstOrDefault()).Select(a => a.GroupCode).FirstOrDefaultAsync();
                        try
                        {
                            #region "Send Email Notifications"
                            string url = _configuration[Configuration.NotificationApi];

                            url = url + "/CourseApplicability";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", CId);
                            oJsonObject.Add("OrganizationCode", OrgCode);
                            HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                        try
                        {
                            #region "Send Bell Notifications"
                            bool IsApplicableToAll = false;
                            int notificationID = 0;

                            List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(CId, ConnectionString);

                            if (aPINotification.Count > 0)
                                notificationID = aPINotification.FirstOrDefault().Id;
                            else
                            {
                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = CourseTitle;
                                Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                Notification.Url = "myCourseModule/" + CId;
                                Notification.Type = Record.Course;
                                Notification.UserId = UserId;
                                Notification.CourseId = CId;
                                notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, ConnectionString);
                            }
                            DataTable dtUserIds = new DataTable();
                            dtUserIds.Columns.Add("UserIds");

                            foreach (var result in nodalCourseRequests)
                            {
                                dtUserIds.Rows.Add(result.UserId);
                            }
                            if (dtUserIds.Rows.Count > 0)
                                await this.SendDataForApplicableNotifications(notificationID, dtUserIds, UserId, ConnectionString);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Utilities.GetDetailedException(ex);
                        }
                        try
                        {
                            #region "Send Push Notifications"
                            string url = string.Empty;
                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("CourseId", CId);
                            Pushnotification.Add("OrganizationCode", OrgCode);

                            HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        try
                        {
                            #region "Send SMS Notifications"
                            string urlSMS = string.Empty;
                            var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", OrgCode, ConnectionString);
                            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            {
                                urlSMS = _configuration[Configuration.NotificationApi];
                                urlSMS += "/CourseApplicabilitySMS";
                                JObject oJsonObjectSMS = new JObject();
                                oJsonObjectSMS.Add("CourseId", CId);
                                oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                                HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = GroupCode;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequests.Select(x => x.CreatedBy).FirstOrDefault();
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = OrgCode;

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }

            }

            return responseMessage;
        }

        private async Task<int> SendEmailAfterPaymentSuccessful(APIPaymentMailDetails aPIPaymentMailDetails)
        {
            try
            {
                var result = await (from um in _db.UserMaster
                                    join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
                                    where
                                        um.IsDeleted == false && um.IsActive == true
                                        && um.Id == aPIPaymentMailDetails.UserMasterId
                                        && (um.UserRole == "NO" || um.UserRole == "CA" || um.UserRole == "GA" || um.UserRole == "EU")
                                    select new
                                    {
                                        UserMaserId = um.Id,
                                        UserName = um.UserName,
                                        EmailId = um.EmailId,
                                        MobileNumber = umd.MobileNumber,
                                        UserId = um.UserId
                                    }).FirstOrDefaultAsync();

                aPIPaymentMailDetails.UserId = result.UserId;
                aPIPaymentMailDetails.UserName = result.UserName;
                aPIPaymentMailDetails.EmailId = result.EmailId;
                aPIPaymentMailDetails.MobileNumber = result.MobileNumber;

                _email.PaymentSuccessfulMailToUser(aPIPaymentMailDetails);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }

        public PayproceesResponseClass DecreptePaymentRespone(string merchantParamsJson)
        {
            string merchantEncryptionKey = _configuration["ApiKey"];
            string decryptedText = string.Empty;
            try
            {
                string original = merchantParamsJson;
                byte[] src = Convert.FromBase64String(original);

                RijndaelManaged aes = new RijndaelManaged();

                byte[] key = Encoding.ASCII.GetBytes(merchantEncryptionKey);
                aes.KeySize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.ECB;

                using (ICryptoTransform decrypt = aes.CreateDecryptor(key, null))
                {
                    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                    decrypt.Dispose();
                    decryptedText = Encoding.UTF8.GetString(dest);
                }
                PayproceesResponseClass payresponse = JsonConvert.DeserializeObject<PayproceesResponseClass>(decryptedText);
                return payresponse;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            return null;
        }

        public async Task<List<ApiNotification>> GetCountByCourseIdAndUserId(int Url, string ConnectionString)
        {
            List<ApiNotification> listUserApplicability = new List<ApiNotification>();

            try
            {
                using (var dbContext = this._customerConnectionString.GetDbContext(ConnectionString))
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetCountByCourseIdAndUserId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = Url });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    listUserApplicability.Add(new ApiNotification() { Title = row["Title"].ToString(), Id = Convert.ToInt32(row["Id"]) });
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
            return listUserApplicability;
        }
        public async Task<int> SendNotificationCourseApplicability(ApiNotification apiNotification, bool IsApplicabletoall, string ConnectionString)
        {
            int Id = 0;

            try
            {
                using (var dbContext = this._customerConnectionString.GetDbContext(ConnectionString))
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "InsertNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@IsRead", SqlDbType.Int) { Value = apiNotification.IsRead });
                            cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar) { Value = apiNotification.Message });
                            cmd.Parameters.Add(new SqlParameter("@Url", SqlDbType.NVarChar) { Value = apiNotification.Url });
                            cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = apiNotification.Title });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = apiNotification.UserId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = apiNotification.CourseId });

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
                                Id = Convert.ToInt32(row["Id"].ToString());
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
            return Id;
        }

        public async Task SendDataForApplicableNotifications(int notificationId, DataTable dtUserIds, int createdBy, string ConnectionString)
        {
            try
            {
                using (var dbContext = this._customerConnectionString.GetDbContext(ConnectionString))
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (SqlCommand cmd = (SqlCommand)connection.CreateCommand())
                        {
                            cmd.CommandText = "ApplicableInsertNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.NVarChar) { Value = notificationId });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = createdBy });
                            cmd.Parameters.AddWithValue("@TVP_UserIDs", dtUserIds);

                            DbDataReader reader = await cmd.ExecuteReaderAsync();

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
        }
        //public async Task<ApiResponse> GetOrganizationDetailsTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString)
        //{
        //    ApiResponse obj = new ApiResponse();
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        IQueryable<APINodalUserTypeAheadResponse> Query = context.Configure10.Where(x => x.IsDeleted == 0).Select(x=>new APINodalUserTypeAheadResponse { Id = x.Id, Name = x.Name });

        //        if (!string.IsNullOrEmpty(aPINodalUserTypeAhead.SearchText))
        //            Query = Query.Where(a => a.Name.ToLower().StartsWith(aPINodalUserTypeAhead.SearchText.ToLower()));

        //        obj.ResponseObject = await Query.ToListAsync();
        //        return obj;
        //    }
        //}
        //public async Task<ApiResponse> GetBillingAddressTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString)
        //{
        //    ApiResponse obj = new ApiResponse();
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        IQueryable<APINodalUserTypeAheadResponse> Query = context.Configure11.Where(x => x.IsDeleted == 0).Select(x => new APINodalUserTypeAheadResponse { Id = x.Id, Name = x.Name });

        //        if (!string.IsNullOrEmpty(aPINodalUserTypeAhead.SearchText))
        //            Query = Query.Where(a => a.Name.ToLower().StartsWith(aPINodalUserTypeAhead.SearchText.ToLower()));

        //        obj.ResponseObject = await Query.ToListAsync();
        //        return obj;
        //    }
        //}
        //public async Task<ApiResponse> GetOrganizationIDTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString)
        //{
        //    ApiResponse obj = new ApiResponse();
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        IQueryable<APINodalUserTypeAheadResponse> Query = context.Configure7.Where(x => x.IsDeleted == 0).Select(x => new APINodalUserTypeAheadResponse { Id = x.Id, Name = x.Name });

        //        if (!string.IsNullOrEmpty(aPINodalUserTypeAhead.SearchText))
        //            Query = Query.Where(a => a.Name.ToLower().StartsWith(aPINodalUserTypeAhead.SearchText.ToLower()));

        //        obj.ResponseObject = await Query.ToListAsync();
        //        return obj;
        //    }
        //}
        //public async Task<int> GroupAdminSignUp(APIGroupAdminSignUp aPIGroupAdminSignUp, string ConnectionString)
        //{
        //    APINodalUser aPINodalUser = Mapper.Map<APINodalUser>(aPIGroupAdminSignUp);
        //    ChangeDbContext(ConnectionString);
        //    aPINodalUser.CreatedBy = aPINodalUser.ModifiedBy = 1;
        //    aPINodalUser.UserRole = "GA";
        //    aPINodalUser.CustomerCode = Security.DecryptForUI(aPIGroupAdminSignUp.OrgCode).ToLower();
        //    if (aPIGroupAdminSignUp.DateOfBirth != null && (((DateTime)aPIGroupAdminSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //        return -7;

        //    if (aPIGroupAdminSignUp.DateOfBirth != null)
        //        aPIGroupAdminSignUp.DateOfBirth = ((DateTime)aPIGroupAdminSignUp.DateOfBirth).Date;

        //    aPINodalUser = await AddGroupAdminUser(aPINodalUser, ConnectionString);
        //    if (aPINodalUser.Id > 0)
        //    {
        //        await _email.GroupAdminSelfRegistrationMail(aPINodalUser);
        //    }

        //    return aPINodalUser.Response;
        //}
        //public async Task<APINodalUser> AddGroupAdminUser(APINodalUser Apiuser, string ConnectionString)
        //{
        //    string OrgCode = Apiuser.OrganizationCode;
        //    Exists exist = await this.Validations(Apiuser);
        //    if (!exist.Equals(Exists.No))
        //    {
        //        if (exist.Equals(Exists.UserIdExist))
        //            Apiuser.Response = -1;
        //        else if (exist.Equals(Exists.EmailIdExist))
        //            Apiuser.Response = -5;
        //        else if (exist.Equals(Exists.MobileExist))
        //            Apiuser.Response = -6;
        //        else if (exist.Equals(Exists.AadharExist))
        //            Apiuser.Response = -8;

        //        return Apiuser;
        //    }
        //    else
        //    {
        //        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
        //        Apiuser.AccountCreatedDate = DateTime.UtcNow;
        //        Apiuser.CreatedDate = DateTime.UtcNow;
        //        Apiuser.ModifiedDate = DateTime.UtcNow;


        //        var allowRANDOMPASSWORD = await this.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode, ConnectionString);
        //        if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
        //        {
        //            if (OrgCode.ToLower().Contains("keventers"))
        //            {
        //                Apiuser.RandomUserPassword = "Keventers@123";
        //                Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
        //            }
        //            else
        //            {
        //                if (OrgCode.ToLower() == "pepperfry")
        //                    Apiuser.Password = Helper.Security.EncryptSHA512("123456");
        //                else
        //                    Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
        //            }
        //        }
        //        else
        //        {
        //            Apiuser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
        //            Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
        //        }

        //        if (Apiuser.MobileNumber != "")
        //        {
        //            if (Apiuser.MobileNumber.Length < 10)
        //            {
        //                Apiuser.Response = -2;
        //                return Apiuser;
        //            }
        //        }

        //        Apiuser.MobileNumber = (Apiuser.MobileNumber == "" ? null : Security.Encrypt(Apiuser.MobileNumber));
        //        Apiuser.EmailId = (Apiuser.EmailId == "" ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

        //        Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
        //        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
        //        Apiuser.Id = 0;
        //        Apiuser.IsActive = true;
        //        if (OrgCode.ToLower() == "spectra")
        //        {
        //            Apiuser.TermsCondintionsAccepted = true;
        //            Apiuser.IsPasswordModified = true;
        //            Apiuser.AcceptanceDate = DateTime.UtcNow;
        //            Apiuser.PasswordModifiedDate = DateTime.UtcNow;
        //        }
        //        else
        //        {
        //            Apiuser.TermsCondintionsAccepted = false;
        //            Apiuser.IsPasswordModified = false;
        //        }

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn10) && (Apiuser.ConfigurationColumn10Id==null || Apiuser.ConfigurationColumn10Id==0))
        //        {
        //            Apiuser.ConfigurationColumn10 = Apiuser.ConfigurationColumn10.Trim();
        //            Configure10 configure10 = await _db.Configure10.Where(x => x.Name == Apiuser.ConfigurationColumn10 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure10==null)
        //            {
        //                Configure10 configure101 = new Configure10();
        //                configure101.Name = Apiuser.ConfigurationColumn10;
        //                configure101.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn10);
        //                configure101.CreatedDate = DateTime.UtcNow;
        //                _db.Configure10.Add(configure101);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn10Id = configure101.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn10Id = configure10.Id;
        //        }
        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn11) && (Apiuser.ConfigurationColumn11Id == null || Apiuser.ConfigurationColumn11Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn11 = Apiuser.ConfigurationColumn11.Trim();
        //            Configure11 configure11 = await _db.Configure11.Where(x => x.Name == Apiuser.ConfigurationColumn11 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure11 == null)
        //            {
        //                Configure11 configure111 = new Configure11();
        //                configure111.Name = Apiuser.ConfigurationColumn11;
        //                configure111.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn11);
        //                configure111.CreatedDate = DateTime.UtcNow;
        //                _db.Configure11.Add(configure111);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn11Id = configure111.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn11Id = configure11.Id;
        //        }

        //        Apiuser.ConfigurationColumn12 = await _db.Configure12.Where(x => x.Id == Apiuser.ConfigurationColumn12Id && x.IsDeleted == 0).Select(x => x.Name).FirstOrDefaultAsync();

        //        Apiuser = await this.AddUserToDb(Apiuser);

        //        return Apiuser;
        //    }
        //}

        public async void ReverifyPaymentStatus(Object source, ElapsedEventArgs e)
        {
            DualVerification dualVerification1 = source as DualVerification;
            _logger.Debug("Starting Dual Verification - " + dualVerification1.TransactionId);
            this.ChangeDbContext(dualVerification1.ConnectionString);

            string verficationUrl = _configuration["VerificationUrl"];
            IEnumerable<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("merchantId",dualVerification1.MerchantId),
                new KeyValuePair<string, string>("txnId",dualVerification1.TransactionId)
            };

            PayproceesResponseClass verifyResponse = await PostFormUrlEncoded<HttpResponseMessage>(verficationUrl, postData);
            _logger.Debug("Dual Verification Response-" + JsonConvert.SerializeObject(verifyResponse));

            PaymentResponse paymentResponse = Mapper.Map<PaymentResponse>(verifyResponse);
            _db.PaymentResponse.Add(paymentResponse);
            await _db.SaveChangesAsync();

            int UserId = Convert.ToInt32(Security.Decrypt(verifyResponse.udf1));

            string RequestTransactionId = Security.Decrypt(verifyResponse.udf2);
            if (verifyResponse.trans_status == "Ok")
            {
                string RequestIds = Security.Decrypt(verifyResponse.udf3);
                string fileName = Security.Decrypt(verifyResponse.udf4);
                string RequestType = Security.Decrypt(verifyResponse.udf5);

                string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, dualVerification1.OrgCode, "html");
                string filePath = Path.Combine(sWebRootFolder, fileName);
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                    file.Delete();

                if (RequestType == NodalCourseRequest.Individual)
                {
                    int RequestId = Convert.ToInt32(RequestIds);
                    NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == RequestId && x.IsApprovedByNodal == true && x.IsDeleted == false).FirstOrDefaultAsync();
                    if (nodalCourseRequest != null)
                    {
                        nodalCourseRequest.IsPaymentDone = true;
                        _db.NodalCourseRequests.Update(nodalCourseRequest);
                        await _db.SaveChangesAsync();

                        UserMaster user = await _db.UserMaster.Where(x => x.Id == nodalCourseRequest.UserId && x.IsDeleted == false && x.IsActive == false).FirstOrDefaultAsync();
                        if (user != null)
                        {
                            user.IsActive = true;
                            _db.UserMaster.Update(user);
                            await _db.SaveChangesAsync();
                        }
                        UserMasterDetails userDetails = await _db.UserMasterDetails.Where(x => x.UserMasterId == nodalCourseRequest.UserId && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (userDetails != null)
                        {
                            userDetails.IsActive = true;
                            userDetails.AppearOnLeaderboard = true;
                            _db.UserMasterDetails.Update(userDetails);
                            await _db.SaveChangesAsync();
                        }
                        string CourseTitle = _db.Course.Where(a => a.Id == nodalCourseRequest.CourseId).Select(a => a.Title).SingleOrDefault();
                        _logger.Debug("user active done");
                        AccessibilityRule accessibilityRule = await _db.AccessibilityRule.Where(x => x.CourseId == nodalCourseRequest.CourseId && x.UserID == nodalCourseRequest.UserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (accessibilityRule == null)
                        {
                            _logger.Debug("Applicability not found. Adding Applicability");
                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };
                            _db.AccessibilityRule.Add(accessibilityRules);
                            await _db.SaveChangesAsync();
                            _logger.Debug("accessibility done");
                            try
                            {
                                #region "Send Email Notifications"
                                string url = _configuration[Configuration.NotificationApi];

                                url = url + "/CourseApplicability";
                                JObject oJsonObject = new JObject();
                                oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                                oJsonObject.Add("OrganizationCode", dualVerification1.OrgCode);
                                HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                                _logger.Debug("Applicability Response" + responses);
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Bell Notifications"
                                bool IsApplicableToAll = false;
                                int notificationID = 0;

                                List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId), dualVerification1.ConnectionString);

                                if (aPINotification.Count > 0)
                                    notificationID = aPINotification.FirstOrDefault().Id;
                                else
                                {
                                    _logger.Debug("Adding applicability bell notification");
                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = CourseTitle;
                                    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                    Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                    Notification.Url = "myCourseModule/" + accessibilityRules.CourseId;
                                    Notification.Type = Record.Course;
                                    Notification.UserId = nodalCourseRequest.UserId;
                                    Notification.CourseId = accessibilityRules.CourseId;
                                    notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, dualVerification1.ConnectionString);
                                }
                                DataTable dtUserIds = new DataTable();
                                dtUserIds.Columns.Add("UserIds");
                                dtUserIds.Rows.Add(nodalCourseRequest.UserId);

                                if (dtUserIds.Rows.Count > 0)
                                    await this.SendDataForApplicableNotifications(notificationID, dtUserIds, nodalCourseRequest.UserId, dualVerification1.ConnectionString);
                                _logger.Debug("added applicability bell notification");
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Push Notifications"
                                string url = string.Empty;
                                url = _configuration[Configuration.NotificationApi];
                                url += "/CourseApplicabilityPushNotification";
                                JObject Pushnotification = new JObject();
                                Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                                Pushnotification.Add("OrganizationCode", dualVerification1.OrgCode);

                                HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                            try
                            {
                                #region "Send SMS Notifications"
                                string urlSMS = string.Empty;
                                var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", dualVerification1.OrgCode, dualVerification1.ConnectionString);
                                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                                {
                                    urlSMS = _configuration[Configuration.NotificationApi];
                                    urlSMS += "/CourseApplicabilitySMS";
                                    JObject oJsonObjectSMS = new JObject();
                                    oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                                    oJsonObjectSMS.Add("OrganizationCode", dualVerification1.OrgCode);
                                    HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                        }
                        else
                        {
                            _logger.Debug("applicab found.");
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = null;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequest.UserId;
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = dualVerification1.OrgCode;
                        aPIPaymentMailDetails.Password = _configuration["DeafultPassword"];

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }
                else if (RequestType == NodalCourseRequest.Group)
                {
                    List<string> reqIds = RequestIds.Split(',').ToList();
                    List<int> RequestIdList = new List<int>();
                    foreach (string item in reqIds)
                        RequestIdList.Add(Convert.ToInt32(item));

                    List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => RequestIdList.Contains(x.Id) && x.RequestType == NodalCourseRequest.Group && x.IsApprovedByNodal == true && x.IsDeleted == false).ToListAsync();
                    List<AccessibilityRule> accessibilityRulesList = new List<AccessibilityRule>();
                    if (nodalCourseRequests != null && nodalCourseRequests.Count() > 0)
                    {
                        foreach (NodalCourseRequests nodalCourseRequest in nodalCourseRequests)
                        {
                            nodalCourseRequest.IsPaymentDone = true;

                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };

                            accessibilityRulesList.Add(accessibilityRules);

                        }

                        _db.NodalCourseRequests.UpdateRange(nodalCourseRequests);
                        await _db.SaveChangesAsync();

                        _db.AccessibilityRule.AddRange(accessibilityRulesList);
                        await _db.SaveChangesAsync();

                        int CId = (int)accessibilityRulesList.Select(x => x.CourseId).FirstOrDefault();
                        string CourseTitle = await _db.Course.Where(a => a.Id == CId).Select(a => a.Title).FirstOrDefaultAsync();
                        string GroupCode = await _db.NodalUserGroups.Where(a => a.Id == nodalCourseRequests.Select(x => x.GroupId).FirstOrDefault()).Select(a => a.GroupCode).FirstOrDefaultAsync();
                        try
                        {
                            #region "Send Email Notifications"
                            string url = _configuration[Configuration.NotificationApi];

                            url = url + "/CourseApplicability";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", CId);
                            oJsonObject.Add("OrganizationCode", dualVerification1.OrgCode);
                            HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                        try
                        {
                            #region "Send Bell Notifications"
                            bool IsApplicableToAll = false;
                            int notificationID = 0;

                            List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(CId, dualVerification1.ConnectionString);

                            if (aPINotification.Count > 0)
                                notificationID = aPINotification.FirstOrDefault().Id;
                            else
                            {
                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = CourseTitle;
                                Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                Notification.Url = "myCourseModule/" + CId;
                                Notification.Type = Record.Course;
                                Notification.UserId = UserId;
                                Notification.CourseId = CId;
                                notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, dualVerification1.ConnectionString);
                            }
                            DataTable dtUserIds = new DataTable();
                            dtUserIds.Columns.Add("UserIds");

                            foreach (var result in nodalCourseRequests)
                            {
                                dtUserIds.Rows.Add(result.UserId);
                            }
                            if (dtUserIds.Rows.Count > 0)
                                await this.SendDataForApplicableNotifications(notificationID, dtUserIds, UserId, dualVerification1.ConnectionString);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Utilities.GetDetailedException(ex);
                        }
                        try
                        {
                            #region "Send Push Notifications"
                            string url = string.Empty;
                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("CourseId", CId);
                            Pushnotification.Add("OrganizationCode", dualVerification1.OrgCode);

                            HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        try
                        {
                            #region "Send SMS Notifications"
                            string urlSMS = string.Empty;
                            var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", dualVerification1.OrgCode, dualVerification1.ConnectionString);
                            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            {
                                urlSMS = _configuration[Configuration.NotificationApi];
                                urlSMS += "/CourseApplicabilitySMS";
                                JObject oJsonObjectSMS = new JObject();
                                oJsonObjectSMS.Add("CourseId", CId);
                                oJsonObjectSMS.Add("OrganizationCode", dualVerification1.OrgCode);
                                HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = GroupCode;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequests.Select(x => x.CreatedBy).FirstOrDefault();
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = dualVerification1.OrgCode;

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }
            }
            _logger.Debug("Done Dual Verification - " + dualVerification1.TransactionId);
        }
        public async Task<PaymentStatusResponse> GetStatus(PaymentStatusRequest paymentStatusRequest, string ConnectionString)
        {
            PaymentStatusResponse paymentStatusResponse = new PaymentStatusResponse();
            this.ChangeDbContext(ConnectionString);
            var result = await (from payment in _db.PaymentResponse
                                join user in _db.UserMaster on payment.udf1 equals Convert.ToString(user.Id)
                                where payment.txn_id == Security.DecryptForUI(paymentStatusRequest.Id)
                                select new PaymentStatusResponseInner
                                {
                                    s = payment.trans_status == "Ok" ? "success" : (payment.trans_status == "To" ? "timeout" : ((payment.trans_status == "F" && payment.resp_message == "Customer Cancelled") ? "cancelled" : ((payment.trans_status == "F" && payment.resp_message != "Customer Cancelled") ? "failed" : "failed"))),
                                    m = payment.trans_status == "Ok" ? "Your transaction has been successful." : (payment.trans_status == "To" ? "Transaction has been timeout." : ((payment.trans_status == "F" && payment.resp_message == "Customer Cancelled") ? "Transaction has been cancelled." : ((payment.trans_status == "F" && payment.resp_message != "Customer Cancelled") ? "Transaction has been failed." : "Transaction has been failed."))),
                                    onum = payment.txn_id,
                                    oa = payment.txn_amount,
                                    u = user.UserName
                                }).FirstOrDefaultAsync();
            if (result != null)
            {
                PaymentStatusResponseInner response = new PaymentStatusResponseInner()
                {
                    s = Security.EncryptForUI(result.s),
                    m = Security.EncryptForUI(result.m),
                    onum = Security.EncryptForUI(result.onum),
                    oa = Security.EncryptForUI(result.oa),
                    u = Security.EncryptForUI(result.u)
                };
                paymentStatusResponse.status = Security.EncryptForUI(JsonConvert.SerializeObject(response));
                paymentStatusResponse.c = Security.EncryptSHA512(JsonConvert.SerializeObject(paymentStatusResponse.status));
                return paymentStatusResponse;
            }
            else
                return null;
        }
        //#region Dhangyan Signup
        //public async Task<APIDhangyanUserSignUpResponse> DhangyanSignUp(APIDhangyanUserSignUp aPIDhangyanUserSignUp, string ConnectionString)
        //{
        //    APIDhangyanUser aPIDhangyanUser = Mapper.Map<APIDhangyanUser>(aPIDhangyanUserSignUp);
        //    APIDhangyanUserSignUpResponse aPIDhangyanUserSignUpResponse = Mapper.Map<APIDhangyanUserSignUpResponse>(aPIDhangyanUserSignUp);
        //    ChangeDbContext(ConnectionString);
        //    aPIDhangyanUser.CreatedBy = aPIDhangyanUser.ModifiedBy = 1;
        //    aPIDhangyanUser.UserRole = "EU";
        //    aPIDhangyanUser.CustomerCode = Security.DecryptForUI(aPIDhangyanUserSignUp.OrgCode).ToLower();
        //    if (aPIDhangyanUserSignUp.DateOfBirth != null && (((DateTime)aPIDhangyanUserSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -7;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    if (aPIDhangyanUserSignUp.DateOfBirth != null)
        //        aPIDhangyanUserSignUp.DateOfBirth = ((DateTime)aPIDhangyanUserSignUp.DateOfBirth).Date;

        //    if(aPIDhangyanUserSignUp.FirstName.Length<2)
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -3;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    if (aPIDhangyanUserSignUp.LastName.Length < 2)
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -4;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    aPIDhangyanUser.UserId = aPIDhangyanUserSignUp.FirstName.Substring(0, 2) + aPIDhangyanUserSignUp.LastName.Substring(0, 2) + string.Format("{0:000}", await this.GetTotalUserCount() + 1);

        //    aPIDhangyanUser = await AddDhangyanUser(aPIDhangyanUser);

        //    aPIDhangyanUserSignUpResponse.UserId = Security.Decrypt(aPIDhangyanUser.UserId);
        //    aPIDhangyanUserSignUpResponse.Response = aPIDhangyanUser.Response;

        //    return aPIDhangyanUserSignUpResponse;
        //}
        //public async Task<APIDhangyanUser> AddDhangyanUser(APIDhangyanUser Apiuser)
        //{
        //    string OrgCode = Apiuser.OrganizationCode;
        //    Exists exist = await this.DhangyanValidations(Apiuser);
        //    if (!exist.Equals(Exists.No))
        //    {
        //        if (exist.Equals(Exists.UserIdExist))
        //            Apiuser.Response = -1;
        //        else if (exist.Equals(Exists.EmailIdExist))
        //            Apiuser.Response = -5;
        //        else if (exist.Equals(Exists.MobileExist))
        //            Apiuser.Response = -6;
        //        else if (exist.Equals(Exists.AadharExist))
        //            Apiuser.Response = -8;

        //        return Apiuser;
        //    }
        //    else
        //    {
        //        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
        //        Apiuser.AccountCreatedDate = DateTime.UtcNow;
        //        Apiuser.CreatedDate = DateTime.UtcNow;
        //        Apiuser.ModifiedDate = DateTime.UtcNow;
        //        Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);

        //        if (!string.IsNullOrEmpty(Apiuser.MobileNumber))
        //        {
        //            if (Apiuser.MobileNumber.Length < 10)
        //            {
        //                Apiuser.Response = -2;
        //                return Apiuser;
        //            }
        //        }

        //        Apiuser.MobileNumber = (string.IsNullOrEmpty(Apiuser.MobileNumber) ? null : Security.Encrypt(Apiuser.MobileNumber));
        //        Apiuser.EmailId = (string.IsNullOrEmpty(Apiuser.EmailId) ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

        //        Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
        //        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
        //        Apiuser.Id = 0;
        //        Apiuser.IsActive = true;
        //        Apiuser.TermsCondintionsAccepted = true;
        //        Apiuser.IsPasswordModified = true;
        //        Apiuser.AcceptanceDate = DateTime.UtcNow;
        //        Apiuser.PasswordModifiedDate = DateTime.UtcNow;

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn2) && (Apiuser.ConfigurationColumn2Id == null || Apiuser.ConfigurationColumn2Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn2 = Apiuser.ConfigurationColumn2.Trim();
        //            Configure2 configure2 = await _db.Configure2.Where(x => x.Name == Apiuser.ConfigurationColumn2 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure2 == null)
        //            {
        //                Configure2 configure22 = new Configure2();
        //                configure22.Name = Apiuser.ConfigurationColumn2;
        //                configure22.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn2);
        //                configure22.CreatedDate = DateTime.UtcNow;
        //                _db.Configure2.Add(configure22);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn2Id = configure22.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn2Id = configure2.Id;
        //        }
        //        Apiuser.ConfigurationColumn1 = await _db.Configure1.Where(x => x.Id == Apiuser.ConfigurationColumn1Id && x.IsDeleted == 0).Select(x => x.Name).FirstOrDefaultAsync();
        //        Apiuser = await this.AddDhangyanUserToDb(Apiuser);

        //        return Apiuser;
        //    }
        //}
        //public async Task<APIDhangyanUser> AddDhangyanUserToDb(APIDhangyanUser apiUser)
        //{
        //    using (var transaction = this._context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            UserMaster User = Mapper.Map<UserMaster>(apiUser);
        //            UserMasterDetails UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
        //            await this._db.UserMaster.AddAsync(User);
        //            await this._db.SaveChangesAsync();
        //            UserDetails.UserMasterId = User.Id;

        //            if (UserDetails.UserMasterId > 0)
        //            {
        //                var Num = User.Id % 10;
        //                switch (Num)
        //                {
        //                    case 1:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 2:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 3:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 4:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 5:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    case 6:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                    case 7:
        //                        UserDetails.HouseId = 3;
        //                        break;
        //                    case 8:
        //                        UserDetails.HouseId = 4;
        //                        break;
        //                    case 9:
        //                        UserDetails.HouseId = 1;
        //                        break;
        //                    default:
        //                        UserDetails.HouseId = 2;
        //                        break;
        //                }

        //                UserDetails.IsActive = User.IsActive;
        //                UserDetails.IsDeleted = User.IsDeleted;
        //                UserDetails.CreatedBy = UserDetails.ModifiedBy = User.Id;
        //                apiUser.Id = apiUser.Response = User.Id;
        //                await this._db.UserMasterDetails.AddAsync(UserDetails);
        //                await this._db.SaveChangesAsync();
        //                transaction.Commit();
        //                return apiUser;
        //            }
        //            else
        //            {
        //                transaction.Rollback();
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            _logger.Error(Utilities.GetDetailedException(ex));
        //            transaction.Rollback();
        //            throw ex;
        //        }
        //    }
        //    apiUser.Id = apiUser.Response = 0;
        //    return apiUser;
        //}
        //public async Task<Exists> DhangyanValidations(APIDhangyanUser user)
        //{
        //    if (await this.IsExists(user.UserId))
        //        return Exists.UserIdExist;
        //    if (!string.IsNullOrEmpty(user.EmailId))
        //    {
        //        if (await this.EmailExists(user.EmailId))
        //            return Exists.EmailIdExist;
        //    }
        //    if (!string.IsNullOrEmpty(user.MobileNumber))
        //    {
        //        if (await this.MobileExists(user.MobileNumber))
        //            return Exists.MobileExist;
        //    }
        //    return Exists.No;
        //}
        //public async Task<List<APIAirportInfo>> GetStates(string ConnectionString)
        //{
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        var states = (from ap in context.Configure1
        //                       where ap.IsDeleted == 0
        //                       orderby ap.Name
        //                       select new APIAirportInfo
        //                       {
        //                           Id = ap.Id,
        //                           Name = ap.Name
        //                       });
        //        return await states.ToListAsync();
        //    }
        //}
        //public async Task<List<APIAirportInfo>> GetOrganizations(APIDhangyanUserTypeAhead aPIDhangyanUserTypeAhead, string ConnectionString)
        //{
        //    using (UserDbContext context = _customerConnectionString.GetDbContext(ConnectionString))
        //    {
        //        List<APIAirportInfo> result = new List<APIAirportInfo>();
        //        IQueryable <APIAirportInfo> orgs = (from org in context.Configure2
        //                      where org.IsDeleted == 0
        //                      orderby org.Name
        //                      select new APIAirportInfo
        //                      {
        //                          Id = org.Id,
        //                          Name = org.Name
        //                      });

        //        string SearchText = string.Empty;
        //        if (!string.IsNullOrEmpty(aPIDhangyanUserTypeAhead.OrganizationType))
        //            SearchText = aPIDhangyanUserTypeAhead.OrganizationType+ "-";
        //        if (!string.IsNullOrEmpty(aPIDhangyanUserTypeAhead.SearchText))
        //            SearchText += aPIDhangyanUserTypeAhead.SearchText;

        //        if (!string.IsNullOrEmpty(SearchText))
        //        {
        //            orgs = orgs.Where(x => x.Name.StartsWith(SearchText));
        //            result = await orgs.ToListAsync();

        //            foreach (var item in result)
        //            {
        //                if (item.Name.IndexOf('-') > -1)
        //                    item.Name = item.Name.Substring(item.Name.IndexOf('-') + 1, item.Name.Length - (item.Name.IndexOf('-') + 1));
        //            }
        //        }
        //        return result.ToList();
        //    }
        //}

        //public async Task<APIDhangyanUserSignUpResponse> DhangyanSchoolSignUp(APISchoolDhangyanSignUp aPIDhangyanUserSignUp, string ConnectionString)
        //{
        //    APIDhangyanUser aPIDhangyanUser = Mapper.Map<APIDhangyanUser>(aPIDhangyanUserSignUp);
        //    APIDhangyanUserSignUpResponse aPIDhangyanUserSignUpResponse = Mapper.Map<APIDhangyanUserSignUpResponse>(aPIDhangyanUserSignUp);
        //    ChangeDbContext(ConnectionString);
        //    aPIDhangyanUser.CreatedBy = aPIDhangyanUser.ModifiedBy = 1;
        //    aPIDhangyanUser.UserRole = "EU";
        //    aPIDhangyanUser.CustomerCode = Security.DecryptForUI(aPIDhangyanUserSignUp.OrgCode).ToLower();
        //    if (aPIDhangyanUserSignUp.DateOfBirth != null && (((DateTime)aPIDhangyanUserSignUp.DateOfBirth).Date > DateTime.Now.AddYears(-10).Date))
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -7;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    if (aPIDhangyanUserSignUp.DateOfBirth != null)
        //        aPIDhangyanUserSignUp.DateOfBirth = ((DateTime)aPIDhangyanUserSignUp.DateOfBirth).Date;

        //    if (aPIDhangyanUserSignUp.FirstName.Length < 2)
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -3;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    if (aPIDhangyanUserSignUp.LastName.Length < 2)
        //    {
        //        aPIDhangyanUserSignUpResponse.Response = -4;
        //        return aPIDhangyanUserSignUpResponse;
        //    }

        //    aPIDhangyanUser.UserId =  "tech" + string.Format("{0:000}", await this.GetTotalUserCount() + 1);

        //    aPIDhangyanUser = await AddDhangyanSchoolUser(aPIDhangyanUser);

        //    aPIDhangyanUserSignUpResponse.UserId = Security.Decrypt(aPIDhangyanUser.UserId);
        //    aPIDhangyanUserSignUpResponse.Response = aPIDhangyanUser.Response;

        //    return aPIDhangyanUserSignUpResponse;
        //}
        //public async Task<APIDhangyanUser> AddDhangyanSchoolUser(APIDhangyanUser Apiuser)
        //{
        //    string OrgCode = Apiuser.OrganizationCode;
        //    Exists exist = await this.DhangyanValidations(Apiuser);
        //    if (!exist.Equals(Exists.No))
        //    {
        //        if (exist.Equals(Exists.UserIdExist))
        //            Apiuser.Response = -1;
        //        else if (exist.Equals(Exists.EmailIdExist))
        //            Apiuser.Response = -5;
        //        else if (exist.Equals(Exists.MobileExist))
        //            Apiuser.Response = -6;
        //        else if (exist.Equals(Exists.AadharExist))
        //            Apiuser.Response = -8;

        //        return Apiuser;
        //    }
        //    else
        //    {
        //        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
        //        Apiuser.AccountCreatedDate = DateTime.UtcNow;
        //        Apiuser.CreatedDate = DateTime.UtcNow;
        //        Apiuser.ModifiedDate = DateTime.UtcNow;
        //        Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);

        //        if (!string.IsNullOrEmpty(Apiuser.MobileNumber))
        //        {
        //            if (Apiuser.MobileNumber.Length < 10)
        //            {
        //                Apiuser.Response = -2;
        //                return Apiuser;
        //            }
        //        }

        //        Apiuser.MobileNumber = (string.IsNullOrEmpty(Apiuser.MobileNumber) ? null : Security.Encrypt(Apiuser.MobileNumber));
        //        Apiuser.EmailId = (string.IsNullOrEmpty(Apiuser.EmailId) ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

        //        Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
        //        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
        //        Apiuser.Id = 0;
        //        Apiuser.IsActive = true;
        //        Apiuser.TermsCondintionsAccepted = true;
        //        Apiuser.IsPasswordModified = true;
        //        Apiuser.AcceptanceDate = DateTime.UtcNow;
        //        Apiuser.PasswordModifiedDate = DateTime.UtcNow;

        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn10) && (Apiuser.ConfigurationColumn10Id == null || Apiuser.ConfigurationColumn10Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn10 = Apiuser.ConfigurationColumn10.Trim();
        //            Configure10 configure10 = await _db.Configure10.Where(x => x.Name == Apiuser.ConfigurationColumn10 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure10 == null)
        //            {
        //                Configure10 configure100 = new Configure10();
        //                configure100.Name = Apiuser.ConfigurationColumn10;
        //                configure100.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn10);
        //                configure100.CreatedDate = DateTime.UtcNow;
        //                _db.Configure10.Add(configure100);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn10Id = configure100.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn10Id = configure10.Id;
        //        }
        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn11) && (Apiuser.ConfigurationColumn11Id == null || Apiuser.ConfigurationColumn11Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn11 = Apiuser.ConfigurationColumn11.Trim();
        //            Configure11 configure11 = await _db.Configure11.Where(x => x.Name == Apiuser.ConfigurationColumn11 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure11 == null)
        //            {
        //                Configure11 configure111 = new Configure11();
        //                configure111.Name = Apiuser.ConfigurationColumn11;
        //                configure111.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn11);
        //                configure111.CreatedDate = DateTime.UtcNow;
        //                _db.Configure11.Add(configure111);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn11Id = configure111.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn11Id = configure11.Id;
        //        }
        //        if (!string.IsNullOrEmpty(Apiuser.ConfigurationColumn12) && (Apiuser.ConfigurationColumn12Id == null || Apiuser.ConfigurationColumn12Id == 0))
        //        {
        //            Apiuser.ConfigurationColumn12 = Apiuser.ConfigurationColumn12.Trim();
        //            Configure12 configure12 = await _db.Configure12.Where(x => x.Name == Apiuser.ConfigurationColumn12 && x.IsDeleted == 0).FirstOrDefaultAsync();
        //            if (configure12 == null)
        //            {
        //                Configure12 configure122 = new Configure12();
        //                configure122.Name = Apiuser.ConfigurationColumn12;
        //                configure122.NameEncrypted = Security.Encrypt(Apiuser.ConfigurationColumn12);
        //                configure122.CreatedDate = DateTime.UtcNow;
        //                _db.Configure12.Add(configure122);
        //                await _db.SaveChangesAsync();
        //                Apiuser.ConfigurationColumn12Id = configure122.Id;
        //            }
        //            else
        //                Apiuser.ConfigurationColumn12Id = configure12.Id;
        //        }

        //        Apiuser = await this.AddDhangyanUserToDb(Apiuser);

        //        return Apiuser;
        //    }
        //}

        public async Task<PaymentResponseMessage> ProcessPaymentResponse(string merchantParamsJson, string ConnectionString, string OrgCode)
        {
            this.ChangeDbContext(ConnectionString);
            PaymentResponseMessage responseMessage = new PaymentResponseMessage();
            PayproceesResponseClass response = DecreptePaymentRespone(merchantParamsJson);
            _logger.Debug("Response-" + JsonConvert.SerializeObject(response));

            string verficationUrl = _configuration["VerificationUrl"];
            IEnumerable<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("merchantId",response.merchant_id),
                new KeyValuePair<string, string>("txnId",response.txn_id)
            };

            PayproceesResponseClass verifyResponse = await PostFormUrlEncoded<HttpResponseMessage>(verficationUrl, postData);
            _logger.Debug("Verification Response-" + JsonConvert.SerializeObject(response));

            if (verifyResponse.trans_status == "To" || verifyResponse.trans_status == "F")
            {
                DualVerification dualVerification = new DualVerification();
                dualVerification.Elapsed += ReverifyPaymentStatus;
                dualVerification.MerchantId = verifyResponse.merchant_id;
                dualVerification.TransactionId = verifyResponse.txn_id;
                dualVerification.ConnectionString = ConnectionString;
                dualVerification.OrgCode = OrgCode;
                dualVerification.Interval = (Convert.ToInt32(_configuration["IntervalMinutes"]) * 60 * 1000);
                dualVerification.AutoReset = false;
                dualVerification.Start();
            }

            //Payment response write to database.
            PaymentResponse paymentResponseExists = await _db.PaymentResponse.Where(x => x.txn_id == verifyResponse.txn_id).FirstOrDefaultAsync();
            if (paymentResponseExists != null)
            {
                responseMessage.Message = "Order Number Duplicate";
                responseMessage.Description = "Transaction Order Number is Duplicate.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;

                return responseMessage;
            }

            PaymentResponse paymentResponse = Mapper.Map<PaymentResponse>(verifyResponse);
            _db.PaymentResponse.Add(paymentResponse);
            await _db.SaveChangesAsync();

            int UserId = Convert.ToInt32(Security.Decrypt(verifyResponse.udf1));

            responseMessage.OrderNumber = paymentResponse.txn_id;
            responseMessage.OrderAmount = paymentResponse.txn_amount;
            responseMessage.UserName = await _db.UserMaster.Where(x => x.Id == UserId && x.IsDeleted == false).Select(x => x.UserName).FirstOrDefaultAsync();

            string RequestTransactionId = Security.Decrypt(verifyResponse.udf2);
            if (RequestTransactionId != verifyResponse.txn_id)
            {
                responseMessage.Message = "Invalid Order Number";
                responseMessage.Description = "Transaction Order Number Not Matched.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
            }
            else if (verifyResponse.trans_status == "F")
            {
                if (verifyResponse.resp_message == "Customer Cancelled")
                    responseMessage.Message = "Customer Cancelled";
                else
                    responseMessage.Message = "Transaction Failed";
                responseMessage.Description = verifyResponse.resp_message;
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
                responseMessage.TransactionId = paymentResponse.txn_id;
            }
            else if (verifyResponse.trans_status == "To")
            {
                responseMessage.Message = "Transaction Timeout";
                responseMessage.Description = verifyResponse.resp_message;
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
                responseMessage.TransactionId = paymentResponse.txn_id;
            }
            else if (verifyResponse.trans_status == "Ok")
            {
                responseMessage.Message = "Transaction Successful";
                responseMessage.Description = "Your transaction has been successful.";
                responseMessage.StatusCode = StatusCodes.Status200OK;
                responseMessage.TransactionId = paymentResponse.txn_id;

                string RequestIds = Security.Decrypt(verifyResponse.udf3);
                string fileName = Security.Decrypt(verifyResponse.udf4);
                string RequestType = Security.Decrypt(verifyResponse.udf5);

                string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode, "html");
                string filePath = Path.Combine(sWebRootFolder, fileName);
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                    file.Delete();

                if (RequestType == NodalCourseRequest.Individual)
                {
                    int RequestId = Convert.ToInt32(RequestIds);
                    NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == RequestId && x.IsApprovedByNodal == true && x.IsDeleted == false).FirstOrDefaultAsync();
                    if (nodalCourseRequest != null)
                    {
                        nodalCourseRequest.IsPaymentDone = true;
                        _db.NodalCourseRequests.Update(nodalCourseRequest);
                        await _db.SaveChangesAsync();

                        UserMaster user = await _db.UserMaster.Where(x => x.Id == nodalCourseRequest.UserId && x.IsDeleted == false && x.IsActive == false).FirstOrDefaultAsync();
                        if (user != null)
                        {
                            user.IsActive = true;
                            _db.UserMaster.Update(user);
                            await _db.SaveChangesAsync();
                        }
                        UserMasterDetails userDetails = await _db.UserMasterDetails.Where(x => x.UserMasterId == nodalCourseRequest.UserId && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (userDetails != null)
                        {
                            userDetails.IsActive = true;
                            userDetails.AppearOnLeaderboard = true;
                            _db.UserMasterDetails.Update(userDetails);
                            await _db.SaveChangesAsync();
                        }
                        string CourseTitle = _db.Course.Where(a => a.Id == nodalCourseRequest.CourseId).Select(a => a.Title).SingleOrDefault();
                        _logger.Debug("user active done");
                        AccessibilityRule accessibilityRule = await _db.AccessibilityRule.Where(x => x.CourseId == nodalCourseRequest.CourseId && x.UserID == nodalCourseRequest.UserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                        if (accessibilityRule == null)
                        {
                            _logger.Debug("Applicability not found. Adding Applicability");
                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };
                            _db.AccessibilityRule.Add(accessibilityRules);
                            await _db.SaveChangesAsync();
                            _logger.Debug("accessibility done");
                            try
                            {
                                #region "Send Email Notifications"
                                string url = _configuration[Configuration.NotificationApi];

                                url = url + "/CourseApplicability";
                                JObject oJsonObject = new JObject();
                                oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                                oJsonObject.Add("OrganizationCode", OrgCode);
                                HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                                _logger.Debug("Applicability Response" + responses);
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Bell Notifications"
                                bool IsApplicableToAll = false;
                                int notificationID = 0;

                                List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId), ConnectionString);

                                if (aPINotification.Count > 0)
                                    notificationID = aPINotification.FirstOrDefault().Id;
                                else
                                {
                                    _logger.Debug("Adding applicability bell notification");
                                    ApiNotification Notification = new ApiNotification();
                                    Notification.Title = CourseTitle;
                                    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                    Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                    Notification.Url = "myCourseModule/" + accessibilityRules.CourseId;
                                    Notification.Type = Record.Course;
                                    Notification.UserId = nodalCourseRequest.UserId;
                                    Notification.CourseId = accessibilityRules.CourseId;
                                    notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, ConnectionString);
                                }
                                DataTable dtUserIds = new DataTable();
                                dtUserIds.Columns.Add("UserIds");
                                dtUserIds.Rows.Add(nodalCourseRequest.UserId);

                                if (dtUserIds.Rows.Count > 0)
                                    await this.SendDataForApplicableNotifications(notificationID, dtUserIds, nodalCourseRequest.UserId, ConnectionString);
                                _logger.Debug("added applicability bell notification");
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }

                            try
                            {
                                #region "Send Push Notifications"
                                string url = string.Empty;
                                url = _configuration[Configuration.NotificationApi];
                                url += "/CourseApplicabilityPushNotification";
                                JObject Pushnotification = new JObject();
                                Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                                Pushnotification.Add("OrganizationCode", OrgCode);

                                HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                            try
                            {
                                #region "Send SMS Notifications"
                                string urlSMS = string.Empty;
                                var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", OrgCode, ConnectionString);
                                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                                {
                                    urlSMS = _configuration[Configuration.NotificationApi];
                                    urlSMS += "/CourseApplicabilitySMS";
                                    JObject oJsonObjectSMS = new JObject();
                                    oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                                    oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                                    HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                        }
                        else
                        {
                            _logger.Debug("applicab found.");
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = null;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequest.UserId;
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = OrgCode;
                        aPIPaymentMailDetails.Password = _configuration["DeafultPassword"];

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }
                else if (RequestType == NodalCourseRequest.Group)
                {
                    List<string> reqIds = RequestIds.Split(',').ToList();
                    List<int> RequestIdList = new List<int>();
                    foreach (string item in reqIds)
                        RequestIdList.Add(Convert.ToInt32(item));

                    List<NodalCourseRequests> nodalCourseRequests = await _db.NodalCourseRequests.Where(x => RequestIdList.Contains(x.Id) && x.RequestType == NodalCourseRequest.Group && x.IsApprovedByNodal == true && x.IsDeleted == false).ToListAsync();
                    List<AccessibilityRule> accessibilityRulesList = new List<AccessibilityRule>();
                    if (nodalCourseRequests != null && nodalCourseRequests.Count() > 0)
                    {
                        foreach (NodalCourseRequests nodalCourseRequest in nodalCourseRequests)
                        {
                            nodalCourseRequest.IsPaymentDone = true;

                            AccessibilityRule accessibilityRules = new AccessibilityRule
                            {
                                CourseId = nodalCourseRequest.CourseId,
                                ConditionForRules = "and",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                CreatedBy = nodalCourseRequest.UserId,
                                ModifiedBy = nodalCourseRequest.UserId,
                                IsActive = true,
                                UserID = nodalCourseRequest.UserId
                            };

                            accessibilityRulesList.Add(accessibilityRules);

                        }

                        _db.NodalCourseRequests.UpdateRange(nodalCourseRequests);
                        await _db.SaveChangesAsync();

                        _db.AccessibilityRule.AddRange(accessibilityRulesList);
                        await _db.SaveChangesAsync();

                        int CId = (int)accessibilityRulesList.Select(x => x.CourseId).FirstOrDefault();
                        string CourseTitle = await _db.Course.Where(a => a.Id == CId).Select(a => a.Title).FirstOrDefaultAsync();
                        string GroupCode = await _db.NodalUserGroups.Where(a => a.Id == nodalCourseRequests.Select(x => x.GroupId).FirstOrDefault()).Select(a => a.GroupCode).FirstOrDefaultAsync();
                        try
                        {
                            #region "Send Email Notifications"
                            string url = _configuration[Configuration.NotificationApi];

                            url = url + "/CourseApplicability";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", CId);
                            oJsonObject.Add("OrganizationCode", OrgCode);
                            HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                        try
                        {
                            #region "Send Bell Notifications"
                            bool IsApplicableToAll = false;
                            int notificationID = 0;

                            List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(CId, ConnectionString);

                            if (aPINotification.Count > 0)
                                notificationID = aPINotification.FirstOrDefault().Id;
                            else
                            {
                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = CourseTitle;
                                Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                Notification.Url = "myCourseModule/" + CId;
                                Notification.Type = Record.Course;
                                Notification.UserId = UserId;
                                Notification.CourseId = CId;
                                notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, ConnectionString);
                            }
                            DataTable dtUserIds = new DataTable();
                            dtUserIds.Columns.Add("UserIds");

                            foreach (var result in nodalCourseRequests)
                            {
                                dtUserIds.Rows.Add(result.UserId);
                            }
                            if (dtUserIds.Rows.Count > 0)
                                await this.SendDataForApplicableNotifications(notificationID, dtUserIds, UserId, ConnectionString);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Utilities.GetDetailedException(ex);
                        }
                        try
                        {
                            #region "Send Push Notifications"
                            string url = string.Empty;
                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("CourseId", CId);
                            Pushnotification.Add("OrganizationCode", OrgCode);

                            HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        try
                        {
                            #region "Send SMS Notifications"
                            string urlSMS = string.Empty;
                            var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", OrgCode, ConnectionString);
                            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            {
                                urlSMS = _configuration[Configuration.NotificationApi];
                                urlSMS += "/CourseApplicabilitySMS";
                                JObject oJsonObjectSMS = new JObject();
                                oJsonObjectSMS.Add("CourseId", CId);
                                oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                                HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                        aPIPaymentMailDetails.CourseTitle = CourseTitle;
                        aPIPaymentMailDetails.GroupCode = GroupCode;
                        aPIPaymentMailDetails.UserMasterId = nodalCourseRequests.Select(x => x.CreatedBy).FirstOrDefault();
                        aPIPaymentMailDetails.OrderNumber = paymentResponse.txn_id;
                        aPIPaymentMailDetails.OrderAmount = paymentResponse.txn_amount;
                        aPIPaymentMailDetails.OrgCode = OrgCode;

                        this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);
                    }
                }

            }

            return responseMessage;
        }


        // #endregion

        #region CCavenue
        public async Task<PaymentResponseMessage> ProcessPaymentResponse(TransactionRequest transactionResponse, string ConnectionString, string OrgCode)
        {
            this.ChangeDbContext(ConnectionString);

            PaymentResponseMessage responseMessage = new PaymentResponseMessage();
            _logger.Info("Response-" + transactionResponse);


            if (string.IsNullOrEmpty(transactionResponse.merchant_param1))
            {
                responseMessage.Message = "merchant_param1 parameter value is null";
                responseMessage.Description = "merchant_param1 parameter value is null.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;

                return responseMessage;
            }
            if (transactionResponse.merchant_param1 == "0")
            {
                responseMessage.Message = "merchant_param1 parameter value is 0";
                responseMessage.Description = "merchant_param1 parameter value is 0.";
                responseMessage.StatusCode = StatusCodes.Status400BadRequest;
                return responseMessage;
            }

            string RequestTransactionId = transactionResponse.merchant_param1;
            string userid_json = transactionResponse.merchant_param3;
            string courseid_json = transactionResponse.merchant_param2;

            _logger.Info("userid_json");
            _logger.Info(userid_json);
            _logger.Info("RequestTransactionId");
            _logger.Info(RequestTransactionId);
            _logger.Info("courseid_json");
            _logger.Info(courseid_json);
            _logger.Info("TransactionRequest");
            _logger.Info(transactionResponse.ToString());

            //Payment response write to database.
            //TransactionRequest paymentResponseExists = await _db.TransactionRequest.Where(x => x.tracking_id == transactionResponse.tracking_id).FirstOrDefaultAsync();
            //if (paymentResponseExists != null)
            //{
            //    responseMessage.Message = "transaction Order Number Duplicate";
            //    responseMessage.Description = "Transaction Order Number is Duplicate.";
            //    responseMessage.StatusCode = StatusCodes.Status400BadRequest;

            //    return responseMessage;
            //}

            try
            {
                _db.TransactionRequest.Add(transactionResponse);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }


            NodalCourseRequests nodalCourseRequest = await _db.NodalCourseRequests.Where(x => x.Id == Convert.ToInt32(RequestTransactionId) && x.IsDeleted == false).FirstOrDefaultAsync();
            if (nodalCourseRequest != null)
            {
                int UserId = Convert.ToInt32(Security.Decrypt(transactionResponse.merchant_param3));
                // int UserId = Convert.ToInt32(nodalCourseRequest.UserId);
                responseMessage.OrderNumber = transactionResponse.order_id;
                responseMessage.OrderAmount = transactionResponse.amount;
                responseMessage.UserName = await _db.UserMaster.Where(x => x.Id == UserId && x.IsDeleted == false).Select(x => x.UserName).FirstOrDefaultAsync();



                if (transactionResponse.order_status == "Success")
                {
                    responseMessage.Message = "Transaction Successful";
                    responseMessage.Description = "Your transaction has been successful.";
                    responseMessage.StatusCode = StatusCodes.Status200OK;
                    responseMessage.TransactionId = transactionResponse.tracking_id;





                    nodalCourseRequest.IsPaymentDone = true;
                    _db.NodalCourseRequests.Update(nodalCourseRequest);
                    await _db.SaveChangesAsync();

                    UserMaster user = await _db.UserMaster.Where(x => x.Id == nodalCourseRequest.UserId && x.IsDeleted == false && x.IsActive == false).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        user.IsActive = true;
                        _db.UserMaster.Update(user);
                        await _db.SaveChangesAsync();
                    }
                    UserMasterDetails userDetails = await _db.UserMasterDetails.Where(x => x.UserMasterId == nodalCourseRequest.UserId && x.IsDeleted == false).FirstOrDefaultAsync();
                    if (userDetails != null)
                    {
                        userDetails.IsActive = true;
                        userDetails.AppearOnLeaderboard = true;
                        _db.UserMasterDetails.Update(userDetails);
                        await _db.SaveChangesAsync();
                    }
                    string CourseTitle = _db.Course.Where(a => a.Id == nodalCourseRequest.CourseId).Select(a => a.Title).SingleOrDefault();
                    _logger.Debug("user active done");
                    AccessibilityRule accessibilityRule = await _db.AccessibilityRule.Where(x => x.CourseId == nodalCourseRequest.CourseId && x.UserID == nodalCourseRequest.UserId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                    if (accessibilityRule == null)
                    {
                        _logger.Debug("Applicability not found. Adding Applicability");
                        AccessibilityRule accessibilityRules = new AccessibilityRule
                        {
                            CourseId = nodalCourseRequest.CourseId,
                            ConditionForRules = "and",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = nodalCourseRequest.UserId,
                            ModifiedBy = nodalCourseRequest.UserId,
                            IsActive = true,
                            UserID = nodalCourseRequest.UserId
                        };
                        _db.AccessibilityRule.Add(accessibilityRules);
                        await _db.SaveChangesAsync();
                        _logger.Debug("accessibility done");
                        try
                        {
                            #region "Send Email Notifications"
                            string url = _configuration[Configuration.NotificationApi];

                            url = url + "/CourseApplicability";
                            JObject oJsonObject = new JObject();
                            oJsonObject.Add("CourseId", accessibilityRules.CourseId);
                            oJsonObject.Add("OrganizationCode", OrgCode);
                            HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
                            _logger.Debug("Applicability Response" + responses);
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        try
                        {
                            #region "Send Bell Notifications"
                            bool IsApplicableToAll = false;
                            int notificationID = 0;

                            List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(Convert.ToInt32(accessibilityRules.CourseId), ConnectionString);

                            if (aPINotification.Count > 0)
                                notificationID = aPINotification.FirstOrDefault().Id;
                            else
                            {
                                _logger.Debug("Adding applicability bell notification");
                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = CourseTitle;
                                Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                                Notification.Message = Notification.Message.Replace("{course}", CourseTitle);
                                Notification.Url = "myCourseModule/" + accessibilityRules.CourseId;
                                Notification.Type = Record.Course;
                                Notification.UserId = nodalCourseRequest.UserId;
                                Notification.CourseId = accessibilityRules.CourseId;
                                notificationID = await this.SendNotificationCourseApplicability(Notification, IsApplicableToAll, ConnectionString);
                            }
                            DataTable dtUserIds = new DataTable();
                            dtUserIds.Columns.Add("UserIds");
                            dtUserIds.Rows.Add(nodalCourseRequest.UserId);

                            if (dtUserIds.Rows.Count > 0)
                                await this.SendDataForApplicableNotifications(notificationID, dtUserIds, nodalCourseRequest.UserId, ConnectionString);
                            _logger.Debug("added applicability bell notification");
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        try
                        {
                            #region "Send Push Notifications"
                            string url = string.Empty;
                            url = _configuration[Configuration.NotificationApi];
                            url += "/CourseApplicabilityPushNotification";
                            JObject Pushnotification = new JObject();
                            Pushnotification.Add("CourseId", accessibilityRules.CourseId);
                            Pushnotification.Add("OrganizationCode", OrgCode);
                            HttpResponseMessage responses1 = Api.CallAPI(url, Pushnotification).Result;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                        try
                        {
                            #region "Send SMS Notifications"
                            string urlSMS = string.Empty;
                            var SendSMSToUser = await this.GetConfigurationValueAsync("SMS_FOR_APPLICABILITY", OrgCode, ConnectionString);
                            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                            {
                                urlSMS = _configuration[Configuration.NotificationApi];
                                urlSMS += "/CourseApplicabilitySMS";
                                JObject oJsonObjectSMS = new JObject();
                                oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                                oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                                HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                    else
                    {
                        _logger.Debug("applicab found.");
                    }

                    APIPaymentMailDetails aPIPaymentMailDetails = new APIPaymentMailDetails();
                    aPIPaymentMailDetails.CourseTitle = CourseTitle;
                    aPIPaymentMailDetails.GroupCode = null;
                    aPIPaymentMailDetails.UserMasterId = nodalCourseRequest.UserId;
                    aPIPaymentMailDetails.OrderNumber = transactionResponse.order_id;
                    aPIPaymentMailDetails.OrderAmount = transactionResponse.amount;
                    aPIPaymentMailDetails.OrgCode = OrgCode;
                    aPIPaymentMailDetails.Password = _configuration["DeafultPassword"];

                    this.SendEmailAfterPaymentSuccessful(aPIPaymentMailDetails);



                }
                else
                {

                }
            }
            return responseMessage;
        }


        public TransactionRequest DecryptTransactionRespone(String encRes)
        {
            try
            {
                CCACrypto ccaCrypto = new CCACrypto();
                string workingKey = "3F597153F7F1D6BB240C007F2F1EB41B";// from avenues
                TransactionRequest responce = new TransactionRequest();

                if (!string.IsNullOrEmpty(encRes))
                {
                    String ResJson = ccaCrypto.Decrypt(encRes, workingKey);

                    var products = ResJson.Split('&');
                    var keyValueList = new List<KeyValuePair<string, string>>();
                    foreach (var product in products)
                    {
                        keyValueList.Add(new KeyValuePair<string, string>(product.Split('=')[0], product.Split('=')[1]));
                    }

                    foreach (var item in keyValueList)
                    {

                        if (item.Key == "Id")
                        {
                            responce.Id = Convert.ToInt32(item.Value);
                        }
                        else if (item.Key == "order_id")
                        {
                            responce.order_id = item.Value;
                        }
                        else if (item.Key == "orderNo")
                        {
                            responce.orderNo = item.Value;
                        }
                        else if (item.Key == "tracking_id")
                        {
                            responce.tracking_id = item.Value;
                        }
                        else if (item.Key == "bank_ref_no")
                        {
                            responce.bank_ref_no = item.Value;
                        }
                        else if (item.Key == "order_status")
                        {
                            responce.order_status = item.Value;
                        }
                        else if (item.Key == "failure_message")
                        {
                            responce.failure_message = item.Value;
                        }
                        else if (item.Key == "payment_mode")
                        {
                            responce.payment_mode = item.Value;
                        }
                        else if (item.Key == "card_name")
                        {
                            responce.card_name = item.Value;
                        }
                        else if (item.Key == "status_code")
                        {
                            responce.status_code = item.Value;
                        }
                        else if (item.Key == "status_message")
                        {
                            responce.status_message = item.Value;
                        }
                        else if (item.Key == "amount")
                        {
                            responce.amount = item.Value;
                        }
                        else if (item.Key == "billing_address")
                        {
                            responce.billing_address = item.Value;
                        }
                        else if (item.Key == "billing_city")
                        {
                            responce.billing_city = item.Value;
                        }
                        else if (item.Key == "billing_country")
                        {
                            responce.billing_country = item.Value;
                        }
                        else if (item.Key == "billing_email")
                        {
                            responce.billing_email = item.Value;
                        }
                        else if (item.Key == "billing_name")
                        {
                            responce.billing_name = item.Value;
                        }
                        else if (item.Key == "billing_notes")
                        {
                            responce.billing_notes = item.Value;
                        }
                        else if (item.Key == "billing_state")
                        {
                            responce.billing_state = item.Value;
                        }
                        else if (item.Key == "billing_tel")
                        {
                            responce.billing_tel = item.Value;
                        }
                        else if (item.Key == "billing_zip")
                        {
                            responce.billing_zip = item.Value;
                        }
                        else if (item.Key == "merchant_param1")
                        {
                            responce.merchant_param1 = item.Value;
                        }
                        else if (item.Key == "merchant_param2")
                        {
                            responce.merchant_param2 = item.Value;
                        }
                        else if (item.Key == "merchant_param3")
                        {
                            responce.merchant_param3 = item.Value;
                        }
                        else if (item.Key == "merchant_param4")
                        {
                            responce.merchant_param4 = item.Value;
                        }
                        else if (item.Key == "merchant_param5")
                        {
                            responce.merchant_param5 = item.Value;
                        }
                        else if (item.Key == "mer_amount")
                        {
                            responce.mer_amount = item.Value;
                        }
                        else if (item.Key == "response_code")
                        {
                            responce.response_code = item.Value;
                        }
                        else if (item.Key == "eci_value")
                        {
                            responce.eci_value = item.Value;
                        }
                        else if (item.Key == "trans_date")
                        {
                            responce.trans_date = item.Value;
                        }

                    }


                    var json = JsonConvert.SerializeObject(responce);



                    responce = System.Text.Json.JsonSerializer.Deserialize<TransactionRequest>(json);


                    return responce;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            return null;

        }


        #endregion

    }
}
