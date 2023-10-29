//======================================
// <copyright file="UserRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using AutoMapper;
using Dapper;
using log4net;
using LDAPService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Saml.API.APIModel;
using Saml.API.Data;
using Saml.API.Helper;
using Saml.API.Helper.Interface;
using Saml.API.Models;
using Saml.API.Repositories.Interfaces;
using Saml.API.Services;
using static Saml.API.Models.UserMaster;
using log4net;
using NPOI.SS.Formula.Functions;
using System.Security.Policy;

namespace Saml.API.Repositories
{
	public class UserRepository : Repository<UserMaster>, IUserRepository
	{
		private UserDbContext _db;
		//private ILocationRepository _locationRepository;
		//private IUserSettingsRepository _userSettingsRepository;
		//private IBusinessRepository _businessRepository;
		//private IGroupRepository _groupRepository;
		//private IAreaRepository _areaRepository;
		//private IConfigure1Repository _configure1Repository;
		//private IConfigure2Repository _configure2Repository;
		//private IConfigure3Repository _configure3Repository;
		//private IConfigure4Repository _configure4Repository;
		//private IConfigure5Repository _configure5Repository;
		//private IConfigure6Repository _configure6Repository;
		//private IConfigure7Repository _configure7Repository;
		//private IConfigure8Repository _configure8Repository;
		//private IConfigure9Repository _configure9Repository;
		//private IConfigure10Repository _configure10Repository;
		//private IConfigure11Repository _configure11Repository;
		//private IConfigure12Repository _configure12Repository;
		//private IConfigure13Repository _configure13Repository;
		//private IConfigure14Repository _configure14Repository;
		//private IConfigure15Repository _configure15Repository;
		private IConfigurationParameterRepository _configurationParameter;
		private IEmail _email;
		IIdentityService _identitySv;
		private ICustomerConnectionStringRepository _customerConnectionString;
		private readonly ITLSHelper _tlsHelper;
		private IConfiguration _configuration { get; }
		//private IUserMasterRejectedRepository _userMasterRejectedRepository;
		private readonly IHttpContextAccessor _httpContextAccessor;
		//private IRolesRepository _rolesRepository;
		private IMemoryCache _cacheIVP;
		private IMemoryCache _cacheENT;
		private ICustomerConnectionStringRepository _customerConnection;
		
		private static readonly ILog _logger = LogManager.GetLogger(typeof(UserRepository));
		public UserRepository
			(UserDbContext context,
			//ILocationRepository locationRepository,
			//IBusinessRepository businessReposiory,
			//IGroupRepository groupRepository,
			//IAreaRepository areaRepository,
			//IConfigure1Repository configure1Repository,
			//IConfigure2Repository configure2Repository,
			//IConfigure3Repository configure3Repository,
			//IConfigure4Repository configure4Repository,
			//IConfigure5Repository configure5Repository,
			//IConfigure6Repository configure6Repository,
			//IConfigure7Repository configure7Repository,
			//IConfigure8Repository configure8Repository,
			//IConfigure9Repository configure9Repository,
			//IConfigure10Repository configure10Repository,
			//IConfigure11Repository configure11Repository,
			//IConfigure12Repository configure12Repository,
			//IConfigure13Repository configure13Repository,
			//IConfigure14Repository configure14Repository,
			//IConfigure15Repository configure15Repository,

			IConfiguration configuration,
			ICustomerConnectionStringRepository customerConnectionString,
			IEmail email,
			IIdentityService identitySv,
			IConfigurationParameterRepository configurationParameter,
			//IUserMasterRejectedRepository userMasterRejectedRepository,
			ITLSHelper tlsHelper,
			IHttpContextAccessor httpContextAccessor,
			//IRolesRepository rolesRepository,
			//IUserSettingsRepository userSettingsRepository,
			IMemoryCache memoryCacheIVP,
			IMemoryCache memoryCacheENT, ICustomerConnectionStringRepository customerConnection
			) : base(context)
		{
			this._db = context;
			//this._locationRepository = locationRepository;
			//this._businessRepository = businessReposiory;
			//this._groupRepository = groupRepository;
			//this._areaRepository = areaRepository;
			//this._configure1Repository = configure1Repository;
			//this._configure2Repository = configure2Repository;
			//this._configure3Repository = configure3Repository;
			//this._configure4Repository = configure4Repository;
			//this._configure5Repository = configure5Repository;
			//this._configure6Repository = configure6Repository;
			//this._configure7Repository = configure7Repository;
			//this._configure8Repository = configure8Repository;
			//this._configure9Repository = configure9Repository;
			//this._configure10Repository = configure10Repository;
			//this._configure11Repository = configure11Repository
			//this._configure12Repository = configure12Repository;
			//this._configure13Repository = configure13Repository;
			//this._configure14Repository = configure14Repository;
			//this._configure15Repository = configure15Repository;
			this._configuration = configuration;
			this._email = email;
			this._identitySv = identitySv;
			this._customerConnectionString = customerConnectionString;
			this._configurationParameter = configurationParameter;
			//this._userMasterRejectedRepository = userMasterRejectedRepository;
			this._httpContextAccessor = httpContextAccessor;
			//this._rolesRepository = rolesRepository;
			this._tlsHelper = tlsHelper;
			//this._userSettingsRepository = userSettingsRepository;
			this._cacheIVP = memoryCacheIVP;
			this._cacheENT = memoryCacheENT;
			_customerConnection = customerConnection;
		}

		public bool IsUserChanged(APIUserMaster oldObject, APIUserMaster newObject)
		{
			string[] changedValues = this._configuration["ChangedValues"].Split(",");
			Type type = typeof(APIUserMaster);
			PropertyInfo[] properties = type.GetProperties();
			foreach (string value in changedValues)
			{
				foreach (PropertyInfo property in properties)
				{
					if (property.Name.Equals(value))
					{
						Type objType = oldObject.GetType();
						Type objTypeNew = newObject.GetType();
						var oldValue = objType.GetProperty(property.Name).GetValue(oldObject);
						var newValue = objTypeNew.GetProperty(property.Name).GetValue(newObject);
						if (oldValue == null)
							oldValue = string.Empty;
						if (newValue == null)
							newValue = string.Empty;

						if (!oldValue.Equals(newValue))
						{
							return true;
						}
					}

				}

			}
			return false;
		}
			
		public async Task<int> AddUserSignUp(APIUserSignUp Apiuser, string UserRole, string OrganisationString)
		{
            string toEmail = Apiuser.EmailId;
            this.ChangeDbContext(OrganisationString);
            Exists exist = await this.ValidationsForUserSignUp(Apiuser);
            if (!exist.Equals(Exists.No))
                return 0;
			else
			{
				UserRole = "CA";
				string ConfigUserId = string.Empty;
				if (Apiuser.CustomerCode.ToLower().Equals("csl") || Apiuser.CustomerCode.ToLower().Equals("csluat"))
				{
					Apiuser.ConfigurationColumn6 = Apiuser.UserId;
					Apiuser.ConfigurationColumn11 = Apiuser.employeeGroup;
					Apiuser.ConfigurationColumn10 = Apiuser.retirementDate;
					Apiuser.AccountExpiryDate = Convert.ToDateTime(Apiuser.retirementDate);
					Apiuser.Business = Apiuser.CustomerCode;
				}
				else
				{
					Apiuser.ConfigurationColumn6 = Apiuser.ConfigurationColumn6;
					Apiuser.ConfigurationColumn11 = Apiuser.ConfigurationColumn11;
					Apiuser.ConfigurationColumn10 = Apiuser.ConfigurationColumn10;
				}
				Apiuser = await this.GetUserObject1(Apiuser, UserRole, OrganisationString);
				if (!(string.IsNullOrEmpty(Apiuser.Area)))
				{
					if (Apiuser.AreaId == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.Business)))
				{
					if (Apiuser.BusinessId == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.Group)))
				{
					if (Apiuser.GroupId == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.Location)))
				{
					if (Apiuser.LocationId == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn1)))
				{
					if (Apiuser.ConfigurationColumn1Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn2)))
				{
					if (Apiuser.ConfigurationColumn2Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn3)))
				{
					if (Apiuser.ConfigurationColumn3Id == null)
					{
						return 2;
					}
				}
				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn4)))
				{
					if (Apiuser.ConfigurationColumn4Id == null)
					{
						return 2;
					}
				}
				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn5)))
				{
					if (Apiuser.ConfigurationColumn5Id == null)
					{
						return 2;
					}
				}
				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn6)))
				{
					if (Apiuser.ConfigurationColumn6Id == null)
					{
						return 2;
					}
				}


				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn7)))
				{
					if (Apiuser.ConfigurationColumn7Id == null)
					{
						return 2;
					}
				}


				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn8)))
				{
					if (Apiuser.ConfigurationColumn8Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn9)))
				{
					if (Apiuser.ConfigurationColumn9Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn10)))
				{
					if (Apiuser.ConfigurationColumn10Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn11)))
				{
					if (Apiuser.ConfigurationColumn11Id == null)
					{
						return 2;
					}
				}

				if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn12)))
				{
					if (Apiuser.ConfigurationColumn12Id == null)
					{
						return 2;
					}
				}


				if (Apiuser.JobRoleId != null)
				{
					if (Apiuser.DateIntoRole == null)
					{
						Apiuser.DateIntoRole = DateTime.UtcNow;
					}
				}



				if (!Apiuser.IsAllConfigured)
				{
					return 2;
				}

				Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
				Apiuser.AccountCreatedDate = DateTime.UtcNow;
				Apiuser.CreatedDate = DateTime.UtcNow;
				Apiuser.ModifiedDate = DateTime.UtcNow;
				string DeafultPassword = this._configuration["DeafultPassword"];
				if (Apiuser.Password == null)
				{

					Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
				}
				else
				{
					DeafultPassword = Apiuser.Password;
					Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.Password);
				}
				if (Apiuser.CustomerCode.Equals("csl") || Apiuser.CustomerCode.ToLower().Equals("csluat"))
				{
					ConfigUserId = Apiuser.loginId;
					Apiuser.UserId = Security.Encrypt(Apiuser.loginId.ToLower().Trim());
					Apiuser.UserName = Apiuser.UserName;
					Apiuser.AppearOnLeaderboard = true;
				}
				else
				{
					Apiuser.UserName = Apiuser.UserName;
					Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower());


				}
				Apiuser.UserRole = "EU";
				Apiuser.UserType = Apiuser.UserType;
				Apiuser.UserSubType = Apiuser.UserSubType;
				Apiuser.MobileNumber = Security.Encrypt(Apiuser.MobileNumber);
				Apiuser.EmailId = Security.Encrypt(Apiuser.EmailId.ToLower());
				Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
				Apiuser.Id = 0;
				bool Ldap = await this.IsLDAP();
				if (Ldap == true)
				{
					Apiuser.TermsCondintionsAccepted = true;
					Apiuser.IsPasswordModified = true;
					Apiuser.AcceptanceDate = DateTime.UtcNow;
					Apiuser.PasswordModifiedDate = DateTime.UtcNow;
				}
				else
				{
					Apiuser.TermsCondintionsAccepted = true;
					Apiuser.IsPasswordModified = true;
					Apiuser.AcceptanceDate = DateTime.UtcNow;
					Apiuser.PasswordModifiedDate = DateTime.UtcNow;
				}


				Exists exist1 = await this.ValidationsDuplicateUseridForSignUp(Apiuser);
				if (!exist1.Equals(Exists.No))
					return 0;


				this.ChangeDbContext(OrganisationString);
				APIUserSignUp user = await this.AddUserToDbForSignUp(Apiuser, UserRole);
				if (user.IsActive == true)
				{
					if (Apiuser.CustomerCode.ToLower().Equals("csl") || Apiuser.CustomerCode.ToLower().Equals("csluat"))
					{
						Task.Run(() => _email.NewUserAddedForVFS(toEmail, user.CustomerCode, user.UserName, user.MobileNumber, ConfigUserId, DeafultPassword, null, user.Id));
					}
					else
					{
						Task.Run(() => _email.NewUserAddedForVFS(toEmail, user.CustomerCode, user.UserName, user.MobileNumber, Security.Decrypt(user.UserId), DeafultPassword, null, user.Id));
					}

				}

				if (Apiuser.CustomerCode.ToLower().Equals("ttglobalgroup") )
				{
					if (Apiuser.Id > 0)
					{
						List<NodalCourseRequests> nodalCourseRequestsExistsList = await _db.NodalCourseRequests.Where(x => x.UserId == Apiuser.Id && x.IsDeleted == false && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
						if (nodalCourseRequestsExistsList.Count > 0)
						{
							if (nodalCourseRequestsExistsList.Where(x => x.IsApprovedByNodal == true).Count() > 0)
								return  3;
							else
							   return  4;
						}
						else
						{
							Course course = await _db.Course.Where(x => x.Id == Apiuser.CourseId && x.IsDeleted == false).FirstOrDefaultAsync();

							//Add course request
							NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
							nodalCourseRequest.CourseId =(int) Apiuser.CourseId;
							nodalCourseRequest.UserId = Apiuser.Id;
							nodalCourseRequest.RequestType = NodalCourseRequest.Individual;
							nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = Apiuser.Id;
							nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
							nodalCourseRequest.CourseFee = course.CourseFee;
							_db.NodalCourseRequests.Add(nodalCourseRequest);
							await _db.SaveChangesAsync();

							UserMasterLogs _userMasterLogs = new UserMasterLogs();
							_userMasterLogs.ModifiedBy = 1;
							_userMasterLogs.IsInserted = 1;
							_userMasterLogs.UserId = Apiuser.Id;
							await this.AddUserMasterLogs(_userMasterLogs);

							//await SendEmailAfterAddingUser(aPINodalUser);
						}

					}
				}
					return 1;
			}
		}

		public async Task<int> AddPasswordHistory(PasswordHistory PasswordHistory)
		{

			await this._db.PasswordHistory.AddAsync(PasswordHistory);

			return 1;

		}

		public async Task<int> AddUpdateUserMasterOTP(UserMasterOtp UserMasterOtp)
		{

			_db.UserMasterOtp.Update(UserMasterOtp);
			await this._db.SaveChangesAsync();

			return 1;

		}
		public async Task<UserMasterOtp> GetUserMasterotp(int userid)
		{
			return await _db.UserMasterOtp.Where(o => o.UserMasterId == userid).FirstOrDefaultAsync();

		}


		public async Task<int> AddUserSignUpOTP(SignUpOTP userotp, string OrganisationString)
		{
			this.ChangeDbContext(OrganisationString);

			_db.SignUpOTP.Add(userotp);
			await this._db.SaveChangesAsync();


			if (userotp.Id > 0)
				return 1;
			else
				return 0;
		}

		public async Task<bool> IsOldPassword(int userId, string password)
		{
			password = Helper.Security.EncryptSHA512(password);
			List<string> OldPassword = await this._db.PasswordHistory.Where(Passwords => Passwords.IsDeleted == Record.NotDeleted && Passwords.UserMasterId == userId)
				.OrderByDescending(Pass => Pass.Id).Select(Pass => Pass.Password).Take(5).ToListAsync();
			if (OldPassword.Contains(password))
				return true;
			return false;
		}
		public async Task<bool> IsUserExists(string userId, string OrganisationString)
		{
			this.ChangeDbContext(OrganisationString);
			if (await this._db.Configure6.Where(u => u.IsDeleted == 0 && u.Name == (userId.ToLower())).CountAsync() > 0)
				return true;
			return false;


		}
		public async Task<bool> IsExists(string userId)
		{
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId == Security.Encrypt(userId.ToLower())).CountAsync() > 0)
				return true;
			return false;


		}

		public async Task<bool> IsExistsInConfig1(string userId)
		{
			if (await this._db.Configure6.Where(u => u.IsDeleted == 0 && u.Name == userId).CountAsync() > 0)
				return true;
			return false;


		}
		//public async Task<bool> IsEmployeeCodeExists(string userId, string OrgCode)
		//{
		//    this.ChangeDbContext(OrgCode);

		//    var result = await (from userMaster in this._db.UserMaster
		//                        join userDetails in this._db.UserMasterDetails on userMaster.Id equals userDetails.UserMasterId
		//                        join config6 in this._db.Configure6 on userDetails.ConfigurationColumn6 equals config6.Id
		//                        where userMaster.IsDeleted == false && config6.Name.ToLower().Trim() == userId.ToLower().Trim()
		//                        select new APIUserId
		//                        {
		//                            Id = userMaster.Id,
		//                            UserId = userMaster.UserId,
		//                            EmailId = userMaster.EmailId,
		//                            MobileNumber = userDetails.MobileNumber
		//                        }).ToListAsync();
		//    if (result.Count > 0)
		//        return true;
		//    return false;
		//}

		//public async Task<List<APIUserId>> GetAllUserList()
		//{
		//    try
		//    {
		//        var UserList = await (from userMaster in this._db.UserMaster
		//                              join userDetails in this._db.UserMasterDetails on userMaster.Id equals userDetails.UserMasterId
		//                              where userMaster.IsDeleted == false
		//                              select new APIUserId
		//                              {
		//                                  Id = userMaster.Id,
		//                                  UserId = userMaster.UserId,
		//                                  EmailId = userMaster.EmailId,
		//                                  MobileNumber = userDetails.MobileNumber
		//                              }).ToListAsync();
		//        return UserList;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}


		//public async Task<List<ApiMobileNumber>> GetAllMobileNumber()
		//{
		//    var MobileNumbers = await (from userMasterDetails in this._db.UserMasterDetails
		//                               where userMasterDetails.IsDeleted == false
		//                               select new ApiMobileNumber
		//                               {
		//                                   MobileNumber = userMasterDetails.MobileNumber

		//                               }).ToListAsync();

		//    return MobileNumbers;
		//}

		//public async Task<bool> UserNameExists(string userName)
		//{
		//    if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.UserName == userName) > 0)
		//        return true;
		//    return false;
		//}

		public async Task<bool> MobileExists(string mobileNumber)
		{
			if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.MobileNumber == Security.Encrypt(mobileNumber)) > 0)
				return true;
			return false;
		}

		public async Task<bool> MobileExists1(string mobileNumber, string OrgCode)
		{
			this.ChangeDbContext(OrgCode);
			if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false).CountAsync(x => x.MobileNumber == Security.Encrypt(mobileNumber)) > 0)
				return true;
			return false;
		}

		public async Task<string> GetEmailByMobileNumber(string mobileNumber)
		{
			return await (from umd in _db.UserMasterDetails
						  join um in _db.UserMaster on umd.UserMasterId equals um.Id
						  where umd.MobileNumber == Security.Encrypt(mobileNumber)
						  select um.EmailId).SingleOrDefaultAsync();
		}

		public async Task<int> GetUserCount(string search = null, string columnName = null, string status = null, int? userId = null, string encryptUserId = null)
		{

			if (!string.IsNullOrEmpty(columnName) && (columnName.ToLower().Equals("emailid") || columnName.ToLower().Equals("userid") || columnName.ToLower().Equals("mobilenumber") || columnName.ToLower().Equals("reportsto")))
			{
				if (!string.IsNullOrEmpty(search))
				{
					search = Security.Encrypt(search.ToLower());
				}
			}
			int Count = 0;
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetUsersCount";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
						cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
						cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
						cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.VarChar) { Value = status });
						cmd.Parameters.Add(new SqlParameter("@encryptUserId", SqlDbType.VarChar) { Value = encryptUserId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);
						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								Count = string.IsNullOrEmpty(row["count"].ToString()) ? 0 : Convert.ToInt32(row["count"].ToString());
							}
						}
						reader.Dispose();
					}
					connection.Close();
					return Count;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return 0;
		}

		public async Task<int> GetTotalUserCount()
		{
			return await this._db.UserMaster.CountAsync();
		}

		public async Task<int> GetTotalUserCount1(string OrgCode)
		{
			this.ChangeDbContext(OrgCode);
			return await this._db.UserMaster.CountAsync();
		}
		public async Task<IEnumerable<APIUserMaster>> GetAllUser(int page, int pageSize, string search = null, string columnName = null, string status = null, int? userId = null, string encryptUserId = null, string OrgCode = null)
		{
			if (!OrgCode.ToLower().Equals("ujjivan") && !OrgCode.ToLower().Equals("wns"))
			{
				if (!string.IsNullOrEmpty(columnName) && (columnName.ToLower().Equals("emailid") || columnName.ToLower().Equals("userid") || columnName.ToLower().Equals("mobilenumber") || columnName.ToLower().Equals("reportsto")))
				{
					if (!string.IsNullOrEmpty(search))
					{
						search = Security.Encrypt(search.ToLower());
					}
				}
				List<APIUserMaster> users = new List<APIUserMaster>();
				try
				{
					using (var dbContext = _customerConnectionString.GetDbContext())
					{
						var connection = dbContext.Database.GetDbConnection();

						if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
							connection.Open();
						using (var cmd = connection.CreateCommand())
						{
							cmd.CommandText = "GetAllUser";
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
							cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
							cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
							cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
							cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
							cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.VarChar) { Value = status });
							cmd.Parameters.Add(new SqlParameter("@encryptUserId", SqlDbType.VarChar) { Value = encryptUserId });
							DbDataReader reader = await cmd.ExecuteReaderAsync();
							DataTable dt = new DataTable();
							dt.Load(reader);
							if (dt.Rows.Count > 0)
							{
								foreach (DataRow row in dt.Rows)
								{

									var user = new APIUserMaster
									{
										AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
										AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
										ConfigurationColumn1 = row["configure1"].ToString(),
										ConfigurationColumn2 = row["configure2"].ToString(),
										ConfigurationColumn3 = row["configure3"].ToString(),
										ConfigurationColumn4 = row["configure4"].ToString(),
										ConfigurationColumn5 = row["configure5"].ToString(),
										ConfigurationColumn6 = row["configure6"].ToString(),
										ConfigurationColumn7 = row["configure7"].ToString(),
										ConfigurationColumn8 = row["configure8"].ToString(),
										ConfigurationColumn9 = row["configure9"].ToString(),
										ConfigurationColumn10 = row["configure10"].ToString(),
										ConfigurationColumn11 = row["configure11"].ToString(),

										ConfigurationColumn12 = row["configure12"].ToString(),
										Currency = row["Currency"].ToString(),
										CustomerCode = row["CustomerCode"].ToString(),
										DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
										DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
										EmailId = Security.Decrypt(row["EmailId"].ToString()),
										Gender = row["Gender"].ToString(),
										Id = Convert.ToInt32(row["Id"].ToString()),
										IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
										Language = string.IsNullOrEmpty(row["Language"].ToString()) ? row["Language"].ToString() : "English",
										LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
										MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
										CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
										CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
										ProfilePicture = row["ProfilePicture"].ToString(),
										SerialNumber = row["SerialNumber"].ToString(),
										TimeZone = row["TimeZone"].ToString(),
										UserId = Security.Decrypt(row["UserId"].ToString()),
										UserName = row["UserName"].ToString(),
										UserRole = row["UserRole"].ToString(),
										UserType = row["UserType"].ToString(),
										Location = row["location"].ToString(),
										Area = row["area"].ToString(),
										Group = row["groupname"].ToString(),
										Business = row["buisness"].ToString(),
										ReportsTo = Security.Decrypt(row["reportsto"].ToString()),
										IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
										Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
										AppearOnLeaderboard = Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
										JobRoleName = row["JobRoleName"].ToString(),
										DateIntoRole = string.IsNullOrEmpty(row["DateIntoRole"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateIntoRole"].ToString()),
										AccountDeactivationDate = string.IsNullOrEmpty(row["AccountDeactivationDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountDeactivationDate"].ToString())

									};
									users.Add(user);
								}
							}
							reader.Dispose();
						}
						connection.Close();
						return users;
					}
				}


				catch (System.Exception ex)
				{
					_logger.Error(Utilities.GetDetailedException(ex));
					throw ex;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(columnName) && (columnName.ToLower().Equals("emailid") || columnName.ToLower().Equals("userid") || columnName.ToLower().Equals("mobilenumber") || columnName.ToLower().Equals("reportsto")))
				{
					if (!string.IsNullOrEmpty(search))
					{
						search = Security.Encrypt(search.ToLower());
					}
				}
				List<APIUserMaster> users = new List<APIUserMaster>();
				try
				{
					using (var dbContext = _customerConnectionString.GetDbContext())
					{
						var connection = dbContext.Database.GetDbConnection();

						if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
							connection.Open();
						using (var cmd = connection.CreateCommand())
						{
							cmd.CommandText = "GetAllUser";
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
							cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
							cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
							cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
							cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
							cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.VarChar) { Value = status });
							cmd.Parameters.Add(new SqlParameter("@encryptUserId", SqlDbType.VarChar) { Value = encryptUserId });
							DbDataReader reader = await cmd.ExecuteReaderAsync();
							DataTable dt = new DataTable();
							dt.Load(reader);
							if (dt.Rows.Count > 0)
							{
								foreach (DataRow row in dt.Rows)
								{

									var user = new APIUserMaster
									{
										AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
										AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
										ConfigurationColumn1 = row["configure1"].ToString(),
										ConfigurationColumn2 = row["configure2"].ToString(),
										ConfigurationColumn3 = row["configure3"].ToString(),
										ConfigurationColumn4 = row["configure4"].ToString(),
										ConfigurationColumn5 = row["configure5"].ToString(),
										ConfigurationColumn6 = row["configure6"].ToString(),
										ConfigurationColumn7 = row["configure7"].ToString(),
										ConfigurationColumn8 = row["configure8"].ToString(),
										ConfigurationColumn9 = row["configure9"].ToString(),
										ConfigurationColumn10 = row["configure10"].ToString(),
										ConfigurationColumn11 = row["configure11"].ToString(),

										ConfigurationColumn12 = row["configure12"].ToString(),
										ConfigurationColumn13 = row["configure13"].ToString(),
										ConfigurationColumn14 = row["configure14"].ToString(),
										ConfigurationColumn15 = row["configure15"].ToString(),
										Currency = row["Currency"].ToString(),
										CustomerCode = row["CustomerCode"].ToString(),
										DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
										DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
										EmailId = Security.Decrypt(row["EmailId"].ToString()),
										Gender = row["Gender"].ToString(),
										Id = Convert.ToInt32(row["Id"].ToString()),
										IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
										Language = string.IsNullOrEmpty(row["Language"].ToString()) ? row["Language"].ToString() : "English",
										LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
										MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
										CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
										CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
										ProfilePicture = row["ProfilePicture"].ToString(),
										SerialNumber = row["SerialNumber"].ToString(),
										TimeZone = row["TimeZone"].ToString(),
										UserId = Security.Decrypt(row["UserId"].ToString()),
										UserName = row["UserName"].ToString(),
										UserRole = row["UserRole"].ToString(),
										UserType = row["UserType"].ToString(),
										Location = row["location"].ToString(),
										Area = row["area"].ToString(),
										Group = row["groupname"].ToString(),
										Business = row["buisness"].ToString(),
										ReportsTo = Security.Decrypt(row["reportsto"].ToString()),
										IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
										Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
										AppearOnLeaderboard = Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
										JobRoleName = row["JobRoleName"].ToString(),
										DateIntoRole = string.IsNullOrEmpty(row["DateIntoRole"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateIntoRole"].ToString()),
									};
									users.Add(user);
								}
							}
							reader.Dispose();
						}
						connection.Close();
						return users;
					}
				}


				catch (System.Exception ex)
				{
					_logger.Error(Utilities.GetDetailedException(ex));
					throw ex;
				}
			}
		}

		public async Task<string> GetCustomerCodeByEmailId(string emailId)
		{
			try
			{
				return await (from users in this._db.UserMaster
							  join userDetails in this._db.UserMasterDetails on users.Id equals userDetails.UserMasterId
							  where users.EmailId == Security.Encrypt(emailId.ToLower())
							  select userDetails.CustomerCode).SingleAsync();

			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return string.Empty;
		}
		public async Task<string> GetModifiedBy(int modifiedById)
		{
			try
			{
				var user = (from users in this._db.UserMaster
							where users.IsDeleted == false && users.Id == modifiedById
							select users.UserName);
				return await user.AsNoTracking().SingleAsync();

			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return string.Empty;
		}
		public async Task<int> GetIdByUserId(string userId)
		{
			int id = await (from users in this._db.UserMaster
							where users.IsDeleted == false && users.UserId == userId
							select users.Id).FirstOrDefaultAsync();

			return id;
		}

		public async Task<int> GetIdByEmailIdAndUserName(string emailid, string userid)
		{
			var id = await (from users in this._db.UserMaster
							where users.IsDeleted == false && users.UserName == userid && users.EmailId == emailid
							select users.Id).FirstOrDefaultAsync();

			return id;
		}
		//public async Task<int> GetIdByLocation(string txtLocation)
		//{
		//    int id = await (from location in this._db.Location
		//                    where location.IsDeleted == 0 && string.Equals(location.Name, txtLocation, StringComparison.OrdinalIgnoreCase)
		//                    select location.Id).FirstOrDefaultAsync();
		//    return id;
		//}
		//public async Task<int> GetIdByGroup(string txtGroup)
		//{
		//	int id = await (from groups in this._db.Group
		//					where groups.IsDeleted == 0 && string.Equals(groups.Name, txtGroup, StringComparison.OrdinalIgnoreCase)
		//					select groups.Id).FirstOrDefaultAsync();
		//	return id;
		//}
		//public async Task<int> GetIdByArea(string txtArea)
		//{
		//	int id = await (from area in this._db.Area
		//					where area.IsDeleted == 0 && string.Equals(area.Name, txtArea, StringComparison.OrdinalIgnoreCase)
		//					select area.Id).FirstOrDefaultAsync();
		//	return id;
		//}
		//public async Task<int> GetIdByBusiness(string txtbusiness)
		//{
		//	int id = await (from business in this._db.Business
		//					where business.IsDeleted == 0 && string.Equals(business.Name, txtbusiness, StringComparison.OrdinalIgnoreCase)
		//					select business.Id).FirstOrDefaultAsync();
		//	return id;
		//}
		public async Task<string> GetEncryptedUserId(int userid)
		{
			string id = await (from users in this._db.UserMaster
							   where users.Id == userid
							   select users.UserId).FirstOrDefaultAsync();

			return id;
		}
		public async Task<int> GetIdByJobName(string txtJobName)
		{
			int id = 0;
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();

					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetJobRoleIdByName";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@JobRoleName", SqlDbType.VarChar) { Value = txtJobName });

						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);

						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								id = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? 0 : int.Parse(row["JobRoleId"].ToString());
							}
						}

						reader.Dispose();
					}
					connection.Close();
					return id;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}

		}
		public async Task<APIUserMaster> GetUser(int id, string decryptUserId = null)
		{
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					APIUserMaster user = null;
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetUser";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = id });
						cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = decryptUserId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);

						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{

								user = new APIUserMaster
								{
									AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
									AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
									ConfigurationColumn1 = row["configure1"].ToString(),
									ConfigurationColumn2 = row["configure2"].ToString(),
									ConfigurationColumn3 = row["configure3"].ToString(),
									ConfigurationColumn4 = row["configure4"].ToString(),
									ConfigurationColumn5 = row["configure5"].ToString(),
									ConfigurationColumn6 = row["configure6"].ToString(),
									ConfigurationColumn7 = row["configure7"].ToString(),
									ConfigurationColumn8 = row["configure8"].ToString(),
									ConfigurationColumn9 = row["configure9"].ToString(),
									ConfigurationColumn10 = row["configure10"].ToString(),
									ConfigurationColumn11 = row["configure11"].ToString(),
									ConfigurationColumn12 = row["configure12"].ToString(),
									ConfigurationColumn13 = row["configure13"].ToString(),
									ConfigurationColumn14 = row["configure14"].ToString(),
									ConfigurationColumn15 = row["configure15"].ToString(),
									ConfigurationColumn1Id = string.IsNullOrEmpty(row["configure1Id"].ToString()) ? null : int.Parse(row["configure1Id"].ToString()) as int?,
									ConfigurationColumn2Id = string.IsNullOrEmpty(row["configure2Id"].ToString()) ? null : int.Parse(row["configure2Id"].ToString()) as int?,
									ConfigurationColumn3Id = string.IsNullOrEmpty(row["configure3Id"].ToString()) ? null : int.Parse(row["configure3Id"].ToString()) as int?,
									ConfigurationColumn4Id = string.IsNullOrEmpty(row["configure4Id"].ToString()) ? null : int.Parse(row["configure4Id"].ToString()) as int?,
									ConfigurationColumn5Id = string.IsNullOrEmpty(row["configure5Id"].ToString()) ? null : int.Parse(row["configure5Id"].ToString()) as int?,
									ConfigurationColumn6Id = string.IsNullOrEmpty(row["configure6Id"].ToString()) ? null : int.Parse(row["configure6Id"].ToString()) as int?,
									ConfigurationColumn7Id = string.IsNullOrEmpty(row["configure7Id"].ToString()) ? null : int.Parse(row["configure7Id"].ToString()) as int?,
									ConfigurationColumn8Id = string.IsNullOrEmpty(row["configure8Id"].ToString()) ? null : int.Parse(row["configure8Id"].ToString()) as int?,
									ConfigurationColumn9Id = string.IsNullOrEmpty(row["configure9Id"].ToString()) ? null : int.Parse(row["configure9Id"].ToString()) as int?,
									ConfigurationColumn10Id = string.IsNullOrEmpty(row["configure10Id"].ToString()) ? null : int.Parse(row["configure10Id"].ToString()) as int?,
									ConfigurationColumn11Id = string.IsNullOrEmpty(row["configure11Id"].ToString()) ? null : int.Parse(row["configure11Id"].ToString()) as int?,
									ConfigurationColumn12Id = string.IsNullOrEmpty(row["configure12Id"].ToString()) ? null : int.Parse(row["configure12Id"].ToString()) as int?,
									ConfigurationColumn13Id = string.IsNullOrEmpty(row["configure13Id"].ToString()) ? null : int.Parse(row["configure13Id"].ToString()) as int?,
									ConfigurationColumn14Id = string.IsNullOrEmpty(row["configure14Id"].ToString()) ? null : int.Parse(row["configure14Id"].ToString()) as int?,
									ConfigurationColumn15Id = string.IsNullOrEmpty(row["configure15Id"].ToString()) ? null : int.Parse(row["configure15Id"].ToString()) as int?,
									Currency = row["Currency"].ToString(),
									CustomerCode = row["CustomerCode"].ToString(),
									DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
									DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
									EmailId = Security.Decrypt(row["EmailId"].ToString()),
									Gender = row["Gender"].ToString(),
									Id = Convert.ToInt32(row["Id"].ToString()),
									IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
									Language = row["Language"].ToString(),
									LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
									LastModifiedDateNew = row["LastModifiedDateNew"].ToString(),
									MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
									Password = row["Password"].ToString(),
									ProfilePicture = row["ProfilePicture"].ToString(),
									SerialNumber = row["SerialNumber"].ToString(),
									TimeZone = row["TimeZone"].ToString(),
									UserId = Security.Decrypt(row["UserId"].ToString()),
									UserName = row["UserName"].ToString(),
									UserRole = row["UserRole"].ToString(),
									UserType = row["UserType"].ToString(),
									Location = row["location"].ToString(),
									Area = row["area"].ToString(),
									Group = row["groupname"].ToString(),
									Business = row["buisness"].ToString(),
									ReportsTo = Security.Decrypt(row["ReportsTo"].ToString()),
									ModifiedByName = row["ModifiedByName"].ToString(),
									BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? null : (int?)Convert.ToInt32(row["BusinessId"].ToString()),
									LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? null : (int?)Convert.ToInt32(row["LocationId"].ToString()),
									AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? null : (int?)Convert.ToInt32(row["AreaId"].ToString()),
									GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? null : (int?)Convert.ToInt32(row["GroupId"].ToString()),
									IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
									IsEnableDegreed = string.IsNullOrEmpty(row["Degreed"].ToString()) ? false : Convert.ToBoolean(row["Degreed"].ToString()),
									TermsCondintionsAccepted = string.IsNullOrEmpty(row["TermsCondintionsAccepted"].ToString()) ? false : Convert.ToBoolean(row["TermsCondintionsAccepted"].ToString()),
									AcceptanceDate = string.IsNullOrEmpty(row["AcceptanceDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AcceptanceDate"].ToString()),
									CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
									CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
									Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
									IsPasswordModified = string.IsNullOrEmpty(row["IsPasswordModified"].ToString()) ? false : Convert.ToBoolean(row["IsPasswordModified"].ToString()),
									ImplicitRole = row["ImplicitRole"].ToString(),
									HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? null : (int?)Convert.ToInt32(row["HouseId"].ToString()),
									House = row["HouseName"].ToString(),
									AppearOnLeaderboard = string.IsNullOrEmpty(row["AppearOnLeaderboard"].ToString()) ? false : Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
									RowGuid = Guid.Parse(row["RowGuid"].ToString()),
									PasswordModifiedDate = string.IsNullOrEmpty(row["PasswordModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["PasswordModifiedDate"].ToString()),
									UserSubType = row["UserSubType"].ToString(),
									JobRoleName = row["JobRoleName"].ToString(),
									JobRoleId = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? null : int.Parse(row["JobRoleId"].ToString()) as int?,
									DateIntoRole = string.IsNullOrEmpty(row["DateIntoRole"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateIntoRole"].ToString()),
									 BuddyTrainerId = string.IsNullOrEmpty(row["BuddyTrainerId"].ToString()) ? null : (int?)Convert.ToInt32(row["BuddyTrainerId"].ToString()),
									 MentorId = string.IsNullOrEmpty(row["MentorId"].ToString()) ? null : (int?)Convert.ToInt32(row["MentorId"].ToString()),
									 HRBPId = string.IsNullOrEmpty(row["HRBPId"].ToString()) ? null : (int?)Convert.ToInt32(row["HRBPId"].ToString()),
									BuddyTrainer = row["BuddyTrainer"].ToString(),
									Mentor = row["Mentor"].ToString(),
									HRBP = row["HRBP"].ToString(),
									AccountDeactivationDate = string.IsNullOrEmpty(row["AccountDeactivationDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountDeactivationDate"].ToString()),
									FederationId = row["FederationId"].ToString(),
									country = row["country"].ToString()
								};
							}
						}

						reader.Dispose();

					}
					connection.Close();
					return user;
				}

			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}
		}
		public async Task<APICompetencyJdUpload> GetCompetencyJdView(int JobRoleId)
		{
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();

					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					APICompetencyJdUpload record = new APICompetencyJdUpload();
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "CompetencyJdUpload";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = JobRoleId });
						cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = "Id" });
						cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = null });
						cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = null });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);
						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								record.Id = Convert.ToInt32(row["Id"].ToString());
								record.Name = row["Name"].ToString();
								record.FilePath = row["FilePath"].ToString();
								record.CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString());
							}
						}
						using (var crd = connection.CreateCommand())
						{
							crd.CommandText = "GetCompetencyJdView";
							crd.CommandType = CommandType.StoredProcedure;
							crd.Parameters.Add(new SqlParameter("@JobRoleId", SqlDbType.VarChar) { Value = JobRoleId });
							DbDataReader readers = await crd.ExecuteReaderAsync();
							DataTable dtb = new DataTable();
							dtb.Load(readers);
							if (dtb.Rows.Count > 0)
							{
								List<string> Temp = new List<string>();
								foreach (DataRow row in dtb.Rows)
								{
									Temp.Add(row["CompetencyName"].ToString());
								}
								record.CompetencyName = Temp;
							}
							reader.Dispose();
						}
					}
					connection.Close();
					return record;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}
		}
		public async Task<APIUserMasterDetails> GetUserDetailsById(string id, string decryptUserId = null)
		{
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					APIUserMasterDetails user = null;
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetUserDetailsById";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@encryptedUserId", SqlDbType.VarChar) { Value = id });
						cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = decryptUserId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);

						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								user = new APIUserMasterDetails
								{
									ConfigurationColumn1 = row["configure1"].ToString(),
									ConfigurationColumn2 = row["configure2"].ToString(),
									ConfigurationColumn3 = row["configure3"].ToString(),
									ConfigurationColumn4 = row["configure4"].ToString(),
									ConfigurationColumn5 = row["configure5"].ToString(),
									ConfigurationColumn6 = row["configure6"].ToString(),
									ConfigurationColumn7 = row["configure7"].ToString(),
									ConfigurationColumn8 = row["configure8"].ToString(),
									ConfigurationColumn9 = row["configure9"].ToString(),
									ConfigurationColumn10 = row["configure10"].ToString(),
									ConfigurationColumn11 = row["configure11"].ToString(),
									ConfigurationColumn12 = row["configure12"].ToString(),
									ConfigurationColumn13 = row["configure13"].ToString(),
									ConfigurationColumn14 = row["configure14"].ToString(),
									ConfigurationColumn15 = row["configure15"].ToString(),
									Currency = row["Currency"].ToString(),
									EmailId = Security.EncryptForUI(Security.Decrypt(row["EmailId"].ToString())),
									Language = row["Language"].ToString(),
									MobileNumber = Security.EncryptForUI(Security.Decrypt(row["MobileNumber"].ToString())),
									ProfilePicture = row["ProfilePicture"].ToString(),
									TimeZone = row["TimeZone"].ToString(),
									UserName = row["UserName"].ToString(),
									Location = row["location"].ToString(),
									Area = row["area"].ToString(),
									Group = row["groupname"].ToString(),
									Business = row["buisness"].ToString(),
									ReportsTo = Security.EncryptForUI(Security.Decrypt(row["ReportsTo"].ToString())),
									House = row["HouseName"].ToString(),
									JobRoleId = row["JobRoleId"].ToString(),
									Department = row["Department"].ToString(),
									Designation = row["Designation"].ToString(),
									WorkLocation = row["WorkLocation"].ToString(),
									EmployeeID = Security.Decrypt(row["EmployeeID"].ToString()),
									country = row["country"].ToString()

								};
							}
						}
						reader.Dispose();
					}
					connection.Close();
					return user;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}
		}
		//public async Task<APIUserProfile> GetUserProfile(int id, string decryptUserId = null)
		//{
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            APIUserProfile user = null;
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetUserProfile";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = id });
		//                cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = decryptUserId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        user = new APIUserProfile
		//                        {

		//                            ConfigurationColumn1 = row["configure1"].ToString(),
		//                            ConfigurationColumn2 = row["configure2"].ToString(),
		//                            ConfigurationColumn3 = row["configure3"].ToString(),
		//                            ConfigurationColumn4 = row["configure4"].ToString(),
		//                            ConfigurationColumn5 = row["configure5"].ToString(),
		//                            ConfigurationColumn6 = row["configure6"].ToString(),
		//                            ConfigurationColumn7 = row["configure7"].ToString(),
		//                            ConfigurationColumn8 = row["configure8"].ToString(),
		//                            ConfigurationColumn9 = row["configure9"].ToString(),
		//                            ConfigurationColumn10 = row["configure10"].ToString(),
		//                            ConfigurationColumn11 = row["configure11"].ToString(),
		//                            ConfigurationColumn12 = row["configure12"].ToString(),
		//                            ConfigurationColumn13 = row["configure13"].ToString(),
		//                            ConfigurationColumn14 = row["configure14"].ToString(),
		//                            ConfigurationColumn15 = row["configure15"].ToString(),
		//                            Currency = row["Currency"].ToString(),
		//                            DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
		//                            DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
		//                            EmailId = Security.EncryptForUI(Security.Decrypt(row["EmailId"].ToString())),
		//                            Gender = row["Gender"].ToString(),
		//                            Language = row["Language"].ToString(),
		//                            MobileNumber = Security.EncryptForUI(Security.Decrypt(row["MobileNumber"].ToString())),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            TimeZone = row["TimeZone"].ToString(),
		//                            UserId = Security.EncryptForUI(Security.Decrypt(row["UserId"].ToString())),
		//                            UserName = row["UserName"].ToString(),
		//                            UserType = row["UserType"].ToString(),
		//                            Location = row["location"].ToString(),
		//                            Area = row["area"].ToString(),
		//                            Group = row["groupname"].ToString(),
		//                            Business = row["buisness"].ToString(),
		//                            ReportsTo = Security.EncryptForUI(Security.Decrypt(row["ReportsTo"].ToString())),
		//                            House = row["HouseName"].ToString(),
		//                            RoleName = row["RoleName"].ToString(),
		//                            JobRoleName = row["JobRoleName"].ToString(),
		//                            District = row["District"].ToString(),
		//                            IsManager = row["IsManager"].ToString(),
		//                            BuddyTrainerName = row["BuddyTrainerName"].ToString(),
		//                            MentorName = row["MentorName"].ToString(),
		//                            HrbpName = row["HrbpName"].ToString(),
		//                            FederationId = row["FederationId"].ToString(),
		//                            country = row["country"].ToString()
		//                        };
		//                    }
		//                }

		//                reader.Dispose();

		//            }
		//            connection.Close();
		//            return user;
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<APIUserMasterProfile> UserProfileUpdate(int id, string decryptUserId = null)
		//{
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            APIUserMasterProfile user = null;
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetUserProfile";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = id });
		//                cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = decryptUserId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        user = new APIUserMasterProfile
		//                        {
		//                            Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString()),
		//                            ConfigurationColumn1 = row["configure1"].ToString(),
		//                            ConfigurationColumn2 = row["configure2"].ToString(),
		//                            ConfigurationColumn3 = row["configure3"].ToString(),
		//                            ConfigurationColumn4 = row["configure4"].ToString(),
		//                            ConfigurationColumn5 = row["configure5"].ToString(),
		//                            ConfigurationColumn6 = row["configure6"].ToString(),
		//                            ConfigurationColumn7 = row["configure7"].ToString(),
		//                            ConfigurationColumn8 = row["configure8"].ToString(),
		//                            ConfigurationColumn9 = row["configure9"].ToString(),
		//                            ConfigurationColumn10 = row["configure10"].ToString(),
		//                            ConfigurationColumn11 = row["configure11"].ToString(),
		//                            ConfigurationColumn12 = row["configure12"].ToString(),
		//                            Currency = row["Currency"].ToString(),
		//                            CustomerCode = row["CustomerCode"].ToString(),
		//                            DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
		//                            DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
		//                            EmailId = Security.EncryptForUI(Security.Decrypt(row["EmailId"].ToString())),
		//                            Gender = row["Gender"].ToString(),
		//                            Language = row["Language"].ToString(),
		//                            LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
		//                            MobileNumber = Security.EncryptForUI(Security.Decrypt(row["MobileNumber"].ToString())),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            SerialNumber = row["SerialNumber"].ToString(),
		//                            TimeZone = row["TimeZone"].ToString(),
		//                            UserId = Security.Decrypt(row["UserId"].ToString()),
		//                            UserName = row["UserName"].ToString(),
		//                            UserRole = row["UserRole"].ToString(),
		//                            UserType = row["UserType"].ToString(),
		//                            Location = row["location"].ToString(),
		//                            Area = row["area"].ToString(),
		//                            Group = row["groupname"].ToString(),
		//                            Business = row["buisness"].ToString(),
		//                            ReportsTo = Security.Decrypt(row["ReportsTo"].ToString()),
		//                            ModifiedByName = row["ModifiedByName"].ToString(),
		//                            BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? null : (int?)Convert.ToInt32(row["BusinessId"].ToString()),
		//                            LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? null : (int?)Convert.ToInt32(row["LocationId"].ToString()),
		//                            AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? null : (int?)Convert.ToInt32(row["AreaId"].ToString()),
		//                            GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? null : (int?)Convert.ToInt32(row["GroupId"].ToString()),
		//                            IsEnableDegreed = string.IsNullOrEmpty(row["Degreed"].ToString()) ? false : Convert.ToBoolean(row["Degreed"].ToString()),
		//                            TermsCondintionsAccepted = string.IsNullOrEmpty(row["TermsCondintionsAccepted"].ToString()) ? false : Convert.ToBoolean(row["TermsCondintionsAccepted"].ToString()),
		//                            AcceptanceDate = string.IsNullOrEmpty(row["AcceptanceDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AcceptanceDate"].ToString()),
		//                            Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
		//                            IsPasswordModified = string.IsNullOrEmpty(row["IsPasswordModified"].ToString()) ? false : Convert.ToBoolean(row["IsPasswordModified"].ToString()),
		//                            ImplicitRole = row["ImplicitRole"].ToString(),
		//                            HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? null : (int?)Convert.ToInt32(row["HouseId"].ToString()),
		//                            House = row["HouseName"].ToString(),
		//                            RoleName = row["RoleName"].ToString(),
		//                            AppearOnLeaderboard = string.IsNullOrEmpty(row["AppearOnLeaderboard"].ToString()) ? false : Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
		//                            RowGuid = Guid.Parse(row["RowGuid"].ToString()

		//                            )
		//                        };
		//                    }
		//                }

		//                reader.Dispose();

		//            }
		//            connection.Close();
		//            return user;
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<IEnumerable<APIUserTeam>> GetTeam(string email)
		//{
		//    string DomainName = this._configuration["ApiGatewayUrl"];
		//    try
		//    {
		//        var result = (from user in this._db.UserMaster
		//                      join userDetails in _db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                      join roles in this._db.Roles on user.UserRole equals roles.RoleCode
		//                      join userSettings in this._db.UserSettings on roles.RoleCode equals userSettings.RoleCode
		//                      into userSettingTemp
		//                      from userSettings in userSettingTemp.DefaultIfEmpty()
		//                      where (userDetails.ReportsTo == Security.Encrypt(email.ToLower()) && user.IsDeleted == false)
		//                      select new APIUserTeam
		//                      {
		//                          UserId = Security.Decrypt(user.UserId),
		//                          Name = user.UserName,
		//                          EmailId = Security.Decrypt(user.EmailId),
		//                          Id = user.Id,
		//                          ProfilePicture = userDetails.ProfilePicture,
		//                          MobileNumber = Security.Decrypt(userDetails.MobileNumber),
		//                          ReportsTo = Security.Decrypt(userDetails.ReportsTo),
		//                          RoleCode = user.UserRole,
		//                          Gender = userDetails.Gender,
		//                          UserRole = roles.RoleName,
		//                          ChangedUserRole = (user.UserRole.Trim() == "CA" || user.UserRole.Trim() == "EU") ? roles.RoleName : userSettings.ChangedColumnName.Trim() + " Admin",
		//                          ProfilePictureFullPath = string.Concat(DomainName, userDetails.ProfilePicture)
		//                      });
		//        return await result.AsNoTracking().ToListAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        string exception = ex.Message;
		//    }
		//    return null;
		//}

		//public async Task<IList<APIUserMyTeam>> GetMyTeam(string email, string OrgCode = null)
		//{
		//    string DomainName = this._configuration["ApiGatewayUrl"];
		//    try
		//    {
		//        List<APIUserMyTeam> Myteamdata = new List<APIUserMyTeam>();
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetUserMyTeam";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar) { Value = email });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        var Data = new APIUserMyTeam
		//                        {
		//                            Id = Security.EncryptForUI(row["id"].ToString()),
		//                            Name = row["Name"].ToString(),
		//                            UserId = row["UserId"].ToString(),
		//                            ChangedUserRole = row["ChangedUserRole"].ToString(),
		//                            Designation = row["Designation"].ToString(),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            ProfilePictureFullPath = string.Concat(DomainName, row["ProfilePictureFullPath"].ToString()),
		//                            Rank = Convert.ToInt32(row["Rank"]),
		//                            trainerFlag = Convert.ToBoolean( row["trainerFlag"]),
		//                            Gender = row["Gender"].ToString(),
		//                            country = row["country"].ToString()
		//                        };
		//                        Myteamdata.Add(Data);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//        }

		//        foreach (APIUserMyTeam item in Myteamdata)
		//        {
		//            bool notifyManagerEvaluation = false;
		//            using (var dbContext = _customerConnectionString.GetDbContext())
		//            {
		//                var connection = dbContext.Database.GetDbConnection();
		//                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                    connection.Open();
		//                using (var cmd = connection.CreateCommand())
		//                {
		//                    cmd.CommandText = "CheckForNotifyManagerEvaluation";
		//                    cmd.CommandType = CommandType.StoredProcedure;
		//                    cmd.Parameters.Add(new SqlParameter("@EncryptedUserID", SqlDbType.VarChar) { Value = item.UserId });

		//                    DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                    DataTable dt = new DataTable();
		//                    dt.Load(reader);
		//                    if (dt.Rows.Count > 0)
		//                    {
		//                        foreach (DataRow row in dt.Rows)
		//                        {
		//                            notifyManagerEvaluation = string.IsNullOrEmpty(row["notifyManagerEvaluation"].ToString()) ? false : bool.Parse(row["notifyManagerEvaluation"].ToString());
		//                        }
		//                    }
		//                    reader.Dispose();
		//                }
		//                connection.Close();

		//            }

		//            item.notifyManagerEvaluation = notifyManagerEvaluation;
		//        }

		//        return Myteamdata;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));

		//    }
		//    return null;
		//}



		//public async Task<APIMySupervisor> GetUserSupervisor(string email)
		//{
		//    string DomainName = this._configuration["ApiGatewayUrl"];
		//    try
		//    {
		//        var result = (from user in this._db.UserMaster
		//                      join userDetails in _db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                      join roles in this._db.Roles on user.UserRole equals roles.RoleCode
		//                      join work in this._db.Location on userDetails.LocationId equals work.Id
		//                      into WorkTemp
		//                      from work in WorkTemp.DefaultIfEmpty()
		//                      join confi9 in this._db.Configure9 on userDetails.ConfigurationColumn9 equals confi9.Id
		//                      into confi9Temp
		//                      from confi9 in confi9Temp.DefaultIfEmpty()
		//                      join confi3 in this._db.Configure3 on userDetails.ConfigurationColumn9 equals confi3.Id
		//                      into confi3Temp
		//                      from confi3 in confi3Temp.DefaultIfEmpty()
		//                      join userSettings in this._db.UserSettings on roles.RoleCode equals userSettings.RoleCode
		//                      into userSettingTemp
		//                      from userSettings in userSettingTemp.DefaultIfEmpty()

		//                      where (user.EmailId == Security.Encrypt(email.ToLower()) && user.IsDeleted == false)
		//                      select new APIMySupervisor
		//                      {
		//                          Name = user.UserName,
		//                          Id = Security.EncryptForUI((user.Id).ToString()),
		//                          UserRole = roles.RoleName,
		//                          EmployeeId = Security.Decrypt(user.UserId),
		//                          Worklocation = work.Name,
		//                          Department = confi9.Name,
		//                          Designation = confi3.Name,
		//                      }); ;
		//        return await result.FirstOrDefaultAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		//public async Task<APIUserTeam> GetUserSupervisorId(string email)
		//{

		//    try
		//    {
		//        var result = from user in this._db.UserMaster
		//                     where (user.EmailId == email && user.IsDeleted == false)
		//                     select new APIUserTeam
		//                     {
		//                         Name = user.UserName,
		//                         EmailId = Security.Decrypt(user.EmailId),
		//                         Id = user.Id

		//                     };
		//        return await result.FirstOrDefaultAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		//public async Task<APIUserTeam> GetUserData(int id)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.UserMaster
		//                      join userDetails in _db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                      join roles in this._db.Roles on user.UserRole equals roles.RoleCode
		//                      where (user.Id == id && user.IsDeleted == false)
		//                      select new APIUserTeam
		//                      {
		//                          UserId = Security.Decrypt(user.UserId),
		//                          Name = user.UserName,
		//                          EmailId = Security.Decrypt(user.EmailId),
		//                          Id = user.Id,
		//                          ProfilePicture = userDetails.ProfilePicture,
		//                          MobileNumber = Security.Decrypt(userDetails.MobileNumber),
		//                          ReportsTo = Security.Decrypt(userDetails.ReportsTo),
		//                          RoleCode = user.UserRole,
		//                          Gender = userDetails.Gender,
		//                          UserRole = roles.RoleName
		//                      });
		//        return await result.FirstOrDefaultAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}

		//public async Task<APIUserGrade> GetUserGrade(int id)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.UserMasterDetails
		//                      join Configure in this._db.Configure2 on user.ConfigurationColumn2 equals Configure.Id
		//                      where user.UserMasterId == id
		//                      select new APIUserGrade
		//                      {
		//                          Id = Configure.Id,
		//                          Name = Configure.Name
		//                      }) ;
				

		//        return await result.FirstOrDefaultAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		//public async Task<List<APIUserHRAssociation>> GetHRByID(int id)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.UserHRAssociation
		//                      where (user.Id == id && user.IsDeleted == Record.NotDeleted)
		//                      select new APIUserHRAssociation
		//                      {
		//                          Id = user.Id,
		//                          UserName = user.UserName,
		//                          Level = user.Level
		//                      });
		//        return await result.ToListAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}

		//public async Task<List<HouseMaster>> GetAllHouseMaster()
		//{
		//    List<HouseMaster> housemasterList = await this._db.HouseMaster.Where(demo => demo.IsDeleted == 0).ToListAsync();
		//    return housemasterList;
		//}

		//public async Task<List<HouseMaster>> GetHouseMasterByID(int id)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.HouseMaster
		//                      where (user.Id == id && user.IsDeleted == Record.NotDeleted)
		//                      select new HouseMaster
		//                      {
		//                          Id = user.Id,
		//                          Code = user.Code,
		//                          Name = user.Name,
		//                          CreatedDate = user.CreatedDate,
		//                          LogoName = user.LogoName,


		//                      });
		//        return await result.ToListAsync();
		//    }
		//    catch (System.Exception ex)
		////    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		public async Task<Exists> Validations(APIUserMaster user)
		{
			if (await this.IsExists(user.UserId))
				return Exists.UserIdExist;
			if (user.EmailId != "")
			{

				if (await this.EmailExists(user.EmailId))
					return Exists.EmailIdExist;
			}
			if (user.MobileNumber != "")
			{
				if (await this.MobileExists(user.MobileNumber))
					return Exists.MobileExist;
			}
			return Exists.No;
		}

		public async Task<Exists> ValidationsDuplicateUserid(APIUserMaster user)
		{
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId.ToLower() == user.UserId.ToLower()).CountAsync() > 0)
				return Exists.UserIdExist;
			if (user.EmailId == null)
			{
				return Exists.No;
			}
			if (user.EmailId != "")
			{
				if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == user.EmailId) > 0)
					return Exists.EmailIdExist;
			}

			return Exists.No;
		}

		public async Task<Exists> ValidationsForUserSignUp(APIUserSignUp user)
		{
			if (!user.CustomerCode.ToLower().Equals("csl") || !user.CustomerCode.ToLower().Equals("csluat"))
			{
				if (await this.IsExists(user.UserId))
					return Exists.UserIdExist;
				if (await this.EmailExists(user.EmailId))
					return Exists.EmailIdExist;
				if (await this.MobileExists(user.MobileNumber))
					return Exists.MobileExist;
			}
			else
			{
				if (await this.IsExists(user.loginId))
					return Exists.UserIdExist;
				if (await this.EmailExists(user.EmailId))
					return Exists.EmailIdExist;
				if (await this.MobileExists(user.MobileNumber))
					return Exists.MobileExist;
			}
			return Exists.No;
		}


		public async Task<Exists> ValidationsDuplicateUseridForSignUp(APIUserSignUp user)
		{
			if (!user.CustomerCode.ToLower().Equals("csl") || !user.CustomerCode.ToLower().Equals("csl"))
			{
				if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId.ToLower() == user.UserId.ToLower()).CountAsync() > 0)
					return Exists.UserIdExist;

				if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == user.EmailId) > 0)
					return Exists.EmailIdExist;
			}
			else
			{

				if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == user.EmailId) > 0)
					return Exists.EmailIdExist;
			}
			return Exists.No;
		}
		public bool IsUniqueDataIsChanged(APIUserMaster oldUser, APIUserMaster user)
		{

			if (!oldUser.UserId.Equals(user.UserId))
				return true;
			return false;
		}

		//public async Task<int> AddUser(APIUserMaster Apiuser, string UserRole, string OrgCode, string IsInstitute)
		//{

		//	Exists exist = await this.Validations(Apiuser);
		//	if (!exist.Equals(Exists.No))
		//		return 0;
		//	else
		//	{
		//		Apiuser = await this.GetUserObject(Apiuser, UserRole, OrgCode);
		//		if (!(string.IsNullOrEmpty(Apiuser.Area)))
		//		{
		//			if (Apiuser.AreaId == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.Business)))
		//		{
		//			if (Apiuser.BusinessId == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.Group)))
		//		{
		//			if (Apiuser.GroupId == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.Location)))
		//		{
		//			if (Apiuser.LocationId == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn1)))
		//		{
		//			if (Apiuser.ConfigurationColumn1Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn2)))
		//		{
		//			if (Apiuser.ConfigurationColumn2Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn3)))
		//		{
		//			if (Apiuser.ConfigurationColumn3Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn4)))
		//		{
		//			if (Apiuser.ConfigurationColumn4Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn5)))
		//		{
		//			if (Apiuser.ConfigurationColumn5Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn6)))
		//		{
		//			if (Apiuser.ConfigurationColumn6Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn7)))
		//		{
		//			if (Apiuser.ConfigurationColumn7Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn8)))
		//		{
		//			if (Apiuser.ConfigurationColumn8Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn9)))
		//		{
		//			if (Apiuser.ConfigurationColumn9Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn10)))
		//		{
		//			if (Apiuser.ConfigurationColumn10Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn11)))
		//		{
		//			if (Apiuser.ConfigurationColumn11Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn12)))
		//		{
		//			if (Apiuser.ConfigurationColumn12Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn13)))
		//		{
		//			if (Apiuser.ConfigurationColumn13Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn14)))
		//		{
		//			if (Apiuser.ConfigurationColumn14Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn15)))
		//		{
		//			if (Apiuser.ConfigurationColumn15Id == null)
		//			{
		//				return 2;
		//			}
		//		}
		//		if (Apiuser.JobRoleId != null)
		//		{
		//			if (Apiuser.DateIntoRole == null)
		//			{
		//				Apiuser.DateIntoRole = DateTime.UtcNow;
		//			}
		//		}
		//		if (!Apiuser.IsAllConfigured)
		//		{
		//			return 2;
		//		}

		//		Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
		//		Apiuser.AccountCreatedDate = DateTime.UtcNow;
		//		Apiuser.CreatedDate = DateTime.UtcNow;
		//		Apiuser.ModifiedDate = DateTime.UtcNow;
		//		var allowRANDOMPASSWORD = await this.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode);
		//		_logger.Debug("allowRANDOMPASSWORD : " + allowRANDOMPASSWORD.ToString());
		//		if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
		//		{
		//			if (OrgCode.ToLower().Contains("keventers"))
		//			{
		//				Apiuser.RandomUserPassword = "Keventers@123";
		//				Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
		//			}
		//			else if (OrgCode == "bandhan" || OrgCode == "bandhanbank" )
		//			{
		//				if (Apiuser.DateOfBirth == null)
		//					return 5;
		//				Apiuser.Password = Security.EncryptSHA512(Apiuser.DateOfBirth?.ToString("MMyyyy"));
		//			}
		//			else
		//			{
		//				if (OrgCode.ToLower() == "pepperfry")
		//				{
		//					Apiuser.Password = Helper.Security.EncryptSHA512("123456");
		//				}
		//				else
		//				{
		//					Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);
		//				}
		//			}

		//		}
		//		else
		//		{
		//			Apiuser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
		//			Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);
		//		}
		//		if (Apiuser.MobileNumber != "")
		//		{
		//			if (Apiuser.MobileNumber.Length < 10)
		//			{
		//				return 3;
		//			}
		//		}

		//		Apiuser.MobileNumber = (Apiuser.MobileNumber == "" ? null : Security.Encrypt(Apiuser.MobileNumber));
		//		Apiuser.EmailId = (Apiuser.EmailId == "" ? null : Security.Encrypt(Apiuser.EmailId.ToLower()));

		//		Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower().Trim());
		//		Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
		//		Apiuser.Id = 0;
		//		bool Ldap = await this.IsLDAP();
		//		if (Ldap == true)
		//		{
		//			Apiuser.TermsCondintionsAccepted = true;
		//			Apiuser.IsPasswordModified = true;
		//			Apiuser.AcceptanceDate = DateTime.UtcNow;
		//			Apiuser.PasswordModifiedDate = DateTime.UtcNow;
		//		}
		//		else
		//		{
		//			if (OrgCode.ToLower() == "spectra")
		//			{
		//				Apiuser.TermsCondintionsAccepted = true;
		//				Apiuser.IsPasswordModified = true;
		//				Apiuser.AcceptanceDate = DateTime.UtcNow;
		//				Apiuser.PasswordModifiedDate = DateTime.UtcNow;
		//			}
		//			else
		//			{
		//				Apiuser.TermsCondintionsAccepted = false;
		//				Apiuser.IsPasswordModified = false;
		//			}
		//		}


		//		//If duplicate user going to insert Recheck to isexist()

		//		Exists exist1 = await this.ValidationsDuplicateUserid(Apiuser);
		//		if (!exist1.Equals(Exists.No))
		//			return 0;

		//		APIUserMaster user = new APIUserMaster();
		//		try
		//		{
		//			user = await this.AddUserToDb(Apiuser, UserRole);
		//		}
		//		catch (System.Exception ex)
		//		{
		//			_logger.Error(Utilities.GetDetailedException(ex));
		//			_logger.Debug("Exception in AddUserToDb : " + ex);
		//			throw ex;
		//		}
		//		try
		//		{
		//			_logger.Debug("Entering SendNewUserAddedMessage");
		//			await SendNewUserAddedMessage(Apiuser, user.Id);
		//			_logger.Debug("exiting from SendNewUserAddedMessage");
		//		}
		//		catch (System.Exception EX)
		//		{
		//			_logger.Error(Utilities.GetDetailedException(EX));
		//			throw EX;
		//		}
		//		if (user.IsActive || Apiuser.ReportsTo != null)
		//		{
		//			_logger.Debug("Entering SendEmailAfterAddingUser");
		//			await SendEmailAfterAddingUser(Apiuser, user.OrganizationCode, user.Id, OrgCode, IsInstitute);
		//			_logger.Debug("exiting from SendEmailAfterAddingUser");
		//		}

		//		return 1;
		//	}
		//}

		//public async Task SendNewUserAddedMessage(APIUserMaster Apiuser, int id)
		//{
		//	try
		//	{
		//		_logger.Debug("Entered SendNewUserAddedMessage");
		//		string urlSMS = "";
		//		string SendSMSToUser = await this.GetConfigurationValueAsync("SMS_USER_CREATED", Apiuser.OrganizationCode);
		//		if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
		//		{
		//			urlSMS = _configuration[Configuration.NotificationApi];
		//			urlSMS += "/UserActivationSMS";

		//			JObject oJsonObjectSMS = new JObject();
		//			oJsonObjectSMS.Add("UserName", Apiuser.UserName);
		//			oJsonObjectSMS.Add("MobileNumber", Security.Decrypt(Apiuser.MobileNumber));
		//			oJsonObjectSMS.Add("orgCode", Apiuser.OrganizationCode);
		//			oJsonObjectSMS.Add("UserID", Security.Decrypt(Apiuser.UserId));

		//			_logger.Debug("urlSMS : " + urlSMS.ToString());
		//			HttpResponseMessage responseMessage = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
		//			string statuscode = responseMessage.StatusCode.ToString();
		//			string Response = responseMessage.RequestMessage.ToString();
		//			_logger.Debug("statuscode : " + statuscode + "Response :" + Response);
		//		}
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Debug("Exception in SendNewUserAddedMessage " + ex.ToString());
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//		throw ex;
		//	}
		//}

		//public async Task<APIUserMaster> GenerateCustomerCode(APIUserMaster Apiuser, int UserId)
		//{
		//    string CustomerCode = Security.Decrypt(_identitySv.GetCustomerCode());
		//    string UserRole = Security.Decrypt(_identitySv.GetUserRole());
		//    Apiuser.CustomerCode = CustomerCode;

		//    APISectionAdminDetails aPISectionAdminDetails = await this.GetSectionAdminDetails(UserId);
		//    APISectionDetails aPISectionDetails = new APISectionDetails
		//    {
		//        CustomerCode = CustomerCode
		//    };

		//    if (UserRole.ToLower().Equals("ba"))
		//    {
		//        Apiuser.Business = aPISectionAdminDetails.Business;
		//    }
		//    else if (UserRole.ToLower().Equals("ga"))
		//    {
		//        Apiuser.Group = aPISectionAdminDetails.Group;
		//    }
		//    else if (UserRole.ToLower().Equals("aa"))
		//    {
		//        Apiuser.Area = aPISectionAdminDetails.Area;
		//    }
		//    else if (UserRole.ToLower().Equals("la"))
		//    {
		//        Apiuser.Location = aPISectionAdminDetails.Location;
		//    }

		//    return Apiuser;
		//}

		//private async Task<int> SendEmailAfterAddingUser(APIUserMaster Apiuser, string orgCode, int id, string OrgCode, string IsInstitute)
		//{
		//    string Email = string.Empty;
		//    string ToEmail = string.Empty;
		//    string Subject = string.Empty;
		//    string Message = string.Empty;
		//    string DeafultPassword = this._configuration["DeafultPassword"];

		//    var allowRANDOMPASSWORD = await this.GetConfigurationValueAsync("RANDOM_PASSWORD", OrgCode);
		//    if (Convert.ToString(allowRANDOMPASSWORD).ToLower() == "no")
		//    {
		//        if (orgCode.ToLower().Contains("keventers"))
		//        {
		//            DeafultPassword = Apiuser.RandomUserPassword;
		//        }
		//        else if (orgCode.ToLower() == "bandhan" || orgCode.ToLower() == "bandhanbank" || orgCode.ToLower() == "enth")
		//        {
		//            DeafultPassword = Apiuser.DateOfBirth?.ToString("MMyyyy");
		//        }
		//        else
		//        {
		//            DeafultPassword = this._configuration["DeafultPassword"];
		//        }
		//    }
		//    else
		//    {
		//        DeafultPassword = Apiuser.RandomUserPassword;
		//    }
		//    if (!string.IsNullOrEmpty(Apiuser.ReportsTo))
		//    {
		//        Email = Security.Decrypt(Apiuser.ReportsTo);
		//        await Task.Run(() => _email.UserSignUpMailManager(orgCode, Apiuser.UserName, Security.Decrypt(Apiuser.UserId), Email, Security.Decrypt(Apiuser.EmailId), Security.Decrypt(Apiuser.MobileNumber)));
		//    }
		//    ToEmail = Security.Decrypt(Apiuser.EmailId);
		//    Subject = "TLS New User Notification";
		//    string SendSMSToUser = await this.GetConfigurationValueAsync("SMS_USER_CREATED", OrgCode);
		//    if (SendSMSToUser.ToLower() == "yes")
		//    {

		//        string Url = _configuration[Configuration.NotificationApi];
		//        Url += "/NewUserAdded";
		//        JObject JsonEnailObj = new JObject();
		//        JsonEnailObj.Add("toEmail", ToEmail);
		//        JsonEnailObj.Add("organizationCode", orgCode);
		//        JsonEnailObj.Add("userName", Apiuser.UserName);
		//        JsonEnailObj.Add("UserID", Security.Decrypt(Apiuser.UserId));
		//        JsonEnailObj.Add("mobileNumber", Security.Decrypt(Apiuser.MobileNumber));
		//        JsonEnailObj.Add("Password", DeafultPassword);
		//        JsonEnailObj.Add("Id", Apiuser.Id);
		//        HttpResponseMessage response = Api.CallAPI(Url, JsonEnailObj).Result;
		//    }
		//    var SendApplicableCoursesEmail = await this.GetConfigurationValueAsync("APP_COURSE_NEWSIGNUPUSER", OrgCode);
		//    if (Convert.ToString(SendApplicableCoursesEmail).ToLower() == "yes")
		//    {
		//        this._email.SendApplicableCoursesEmail(ToEmail, orgCode, Apiuser.UserName, Security.Decrypt(Apiuser.MobileNumber), Security.Decrypt(Apiuser.UserId), id, Apiuser.IsActive);
		//    }
		//    var UserTeamsEmail = await this.GetConfigurationValueAsync("UTAM", OrgCode);
		//    if(UserTeamsEmail != null)
		//    {
		//        if (UserTeamsEmail.ToLower() == "yes")
		//        {
		//           await GetCourseUserTeam(Apiuser.UserId, OrgCode, Apiuser.Id, id);
		//        }
		//    }
			
		//    return 1;
		//}
		//public async Task<int> GetCourseUserTeam(string UserId,string OrgCode,int id,int CreatedBY)
		//{
		//    List<AccessibilityRule> accessibilityRules = _db.AccessibilityRule.Where(
		//        a => a.IsDeleted == false && a.IsActive == true && a.UserTeamId != null
		//        ).ToList();

		//    foreach(AccessibilityRule accessibilityRule in accessibilityRules)
		//    {
		//        UserTeams userTeams = _db.UserTeams.Where(
		//            a => a.Id == accessibilityRule.UserTeamId && a.IsDeleted == false && a.EmailNotification == true
		//            ).FirstOrDefault();
				
		//        if (userTeams != null)
		//        {
				   
		//            List<UserTeamApplicableUser> UserListForUserTeam1 = this.GetUsersForuserTeam(userTeams.TeamCode);

		//            string decryptdId = Security.Decrypt(UserId);
		//            UserTeamApplicableUser userTeamApplicableUser = UserListForUserTeam1.Where(a => a.UserID == decryptdId).FirstOrDefault();

		//            if (userTeamApplicableUser != null)
		//            {
					  
		//                try
		//                {
		//                    #region "Send Email Notifications"
		//                    string url = _configuration[Configuration.NotificationApi];

		//                    url = url + "/CourseApplicability";
		//                    JObject oJsonObject = new JObject();
		//                    oJsonObject.Add("CourseId", accessibilityRule.CourseId);
		//                    oJsonObject.Add("OrganizationCode", OrgCode);
		//                    HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
		//                    #endregion

		//                    Course course = await _db.Course.Where(a => a.Id == accessibilityRule.CourseId).FirstOrDefaultAsync();
		//                    string urls = null;
		//                    if (accessibilityRule.CourseId != null)
		//                    {

		//                        if (course.Code.Contains("urn") || course.Code.Contains("SkillSoft") || course.Code.ToLower().Contains("zobble"))
		//                        {
		//                            urls = course.CourseURL;
		//                        }
		//                        else
		//                        {
		//                            urls = "myCourseModule/" + accessibilityRule.CourseId;
		//                        }
		//                    }
		//                    Notifications notifications = _db.Notifications.Where(a => a.CourseId == course.Id && a.Type == "Course" && a.Url == urls).FirstOrDefault();
		//                    if (notifications != null)
		//                    {
		//                        ApplicableNotifications applicableNotifications = new ApplicableNotifications();
		//                        applicableNotifications.NotificationId = notifications.Id;
		//                        applicableNotifications.ModifiedDate = DateTime.Now;
		//                        applicableNotifications.CreatedDate = DateTime.Now;
		//                        applicableNotifications.CreatedBy = id;
		//                        applicableNotifications.IsDeleted = 0;
		//                        applicableNotifications.UserId = id;
		//                        applicableNotifications.IsReadCount = false;

		//                        _db.Add(applicableNotifications);
		//                        _db.SaveChanges();
		//                    }
		//                }
		//                catch (System.Exception ex)
		//                {
		//                    _logger.Error(Utilities.GetDetailedException(ex));
		//                }
		//            }

		//        }
		//    }

		//    return 0;
			
		//}
	  
		//public UserTeams GetUserTeamsByTeamsCode(string TeamCode)
		//{
		//    if (TeamCode == null)
		//    {
		//        return null;
		//    }
		//    else
		//    {
		//        UserTeams userTeams = _db.UserTeams.Where(a => a.TeamCode == TeamCode && a.IsDeleted == false).FirstOrDefault();
		//        return userTeams;
		//    }
		//}
		//public List<UserTeamApplicableUser> GetUsersForuserTeam(string TeamCode)
		//{
		//    if (TeamCode == null)
		//    {
		//        return null;
		//    }
		//    else
		//    {
		//        UserTeams Result = GetUserTeamsByTeamsCode(TeamCode);

		//        if (Result == null)
		//        {
		//            return null;
		//        }
		//        else
		//        {

		//            List<UserTeamApplicableUser> listUserApplicability = new List<UserTeamApplicableUser>();

		//            var connection = this._db.Database.GetDbConnection();
		//            try
		//            {
		//                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                    connection.Open();
		//                using (var cmd = connection.CreateCommand())
		//                {
		//                    cmd.CommandText = "GetUserTeamApplicableUserList_Export";
		//                    cmd.CommandType = CommandType.StoredProcedure;
		//                    cmd.Parameters.Add(new SqlParameter("UserTeamId", SqlDbType.BigInt) { Value = Result.Id });
		//                    cmd.CommandTimeout = 0;

		//                    DbDataReader reader = cmd.ExecuteReader();
		//                    DataTable dt = new DataTable();
		//                    dt.Load(reader);

		//                    if (dt.Rows.Count > 0)
		//                    {
		//                        foreach (DataRow row in dt.Rows)
		//                        {
		//                            UserTeamApplicableUser rule = new UserTeamApplicableUser();
		//                            rule.UserID = Security.Decrypt(row["UserID"].ToString());
		//                            rule.UserName = row["UserName"].ToString();
		//                            listUserApplicability.Add(rule);
		//                        }
		//                    }
		//                    reader.Dispose();
		//                }
		//                connection.Close();
		//            }
		//            catch (System.Exception ex)
		//            {
		//                _logger.Error(Utilities.GetDetailedException(ex));
		//            }

		//            List<UserTeamApplicableUser> UserList1 = listUserApplicability.GroupBy(p => new { p.UserID, p.UserName })
		//            .Select(g => g.First())
		//            .ToList();
		//            return UserList1;
		//        }
		//    }
		//}
		public int SendNotificationCourseApplicability(ApiNotification apiNotification, bool IsApplicabletoall)
		{
			int Id = 0;
			_logger.Debug(apiNotification.Message+ apiNotification.Title+apiNotification.CourseId+apiNotification.Url);
			try
			{
				using (var dbContext = this._customerConnectionString.GetDbContext())
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
							cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = apiNotification.Type });
							cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = apiNotification.UserId });
							cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = apiNotification.CourseId });

							DbDataReader reader = cmd.ExecuteReader();
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
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return Id;
		}

		//public async Task<List<APICourse>> GetApplicableCoursesId(int UserId, bool IsActive)
		//{
		//    List<APICourse> aPIUserAttendanceDetails = new List<APICourse>();
		//    try
		//    {
		//        using (var dbContext = this._customerConnection.GetDbContext())
		//        {
		//            using (var connection = dbContext.Database.GetDbConnection())
		//            {
		//                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                    connection.Open();

		//                DynamicParameters parameters = new DynamicParameters();
		//                parameters.Add("@UserId", UserId);
		//                parameters.Add("@IsActive", IsActive);

		//                var Result = await SqlMapper.QueryAsync<APICourse>((SqlConnection)connection, "[dbo].[GetCourseApplicableEmails]", parameters, null, null, CommandType.StoredProcedure);
		//                aPIUserAttendanceDetails = Result.ToList();
		//                connection.Close();
		//            }
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//    return aPIUserAttendanceDetails;
		//}
		public async Task<int> AddUserImport(APIUserMaster Apiuser, string UserRole, string IsInstitute)
		{

			Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.RandomUserPassword);

			Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
			Exists exist1 = await this.ValidationsDuplicateUserid(Apiuser);
			if (!exist1.Equals(Exists.No))
				return 0;
			APIUserMaster user = await this.AddUserToDb(Apiuser, UserRole);
			int? userId = user.Id;
			if (userId != 0 || userId != null)
				return 1;
			else
				return 0;
		}
		public async Task<int> UpdateUserImport(APIUserMaster Apiuser)
		{
			Apiuser.EmailId = Security.Encrypt(Apiuser.EmailId.ToLower());
			Apiuser.UserId = Security.Encrypt(Apiuser.UserId.ToLower());
			Apiuser.MobileNumber = Security.Encrypt(Apiuser.MobileNumber);
			Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
			APIUserMaster user = await this.AddUpdateUserImport(Apiuser);
			int? userId = user.Id;
			if (userId != 0 || userId != null)
				return 1;
			else
				return 0;
		}

		public async Task<ApiResponse> CheckForValidUser(string userId, string UserType = null)
		{
			ApiResponse Response = new ApiResponse();
			// --------- Check for Valid Trainer --------- //
			int flagvalidTrainer = 0;
			try
			{
				using (var dbContext = this._customerConnection.GetDbContext())
				{
					using (var connection = dbContext.Database.GetDbConnection())
					{
						if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
							connection.Open();
						using (var cmd = connection.CreateCommand())
						{
							cmd.CommandText = "VerifyUserTypeForTrainer";
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Parameters.Add(new SqlParameter("@UserType", SqlDbType.NVarChar) { Value = UserType });
							cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId });

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
								string value = (row["Value"].ToString());
								if (value == "false")
									flagvalidTrainer = flagvalidTrainer + 1;
							}
							reader.Dispose();
						}
						connection.Close();
					}
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}

			if (flagvalidTrainer != 0)
			{
				Response.Description = "Invalid Trainer Information";
				Response.StatusCode = 417;
				return Response;
			}
			return Response;
			// --------- Check for Valid Trainer --------- //
		}

		//public async Task<IEnumerable<ApiUserSearchV2>> SearchUserType(string userId, string UserType = null)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.UserMaster
		//                      join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                      where ((user.UserId.ToLower() == Security.Encrypt(userId.ToLower())
		//                      || user.UserName.ToLower().StartsWith(userId.ToLower()))
		//                      && user.IsDeleted == false && user.IsActive == true)
		//                      select new ApiUserSearchV2
		//                      {
		//                          UserId = Security.EncryptForUI(Security.Decrypt(user.UserId)),
		//                          Name = user.UserName,
		//                          EmailId = Security.EncryptForUI(Security.Decrypt(user.EmailId)),
		//                          Id = (Security.EncryptForUI(user.Id.ToString())),
		//                          ProfilePicture = userDetails.ProfilePicture,
		//                          MobileNumber = Security.EncryptForUI(Security.Decrypt(userDetails.MobileNumber)),
		//                          UserType = userDetails.UserType,
		//                          IsDeleted =userDetails.IsDeleted

		//                      });

		//        if (UserType != null)
		//        {
		//            result = result.Where(a => a.UserType == UserType);
		//        }

		//        result = result.Where(v => v.IsDeleted == false);

		//        return await result.AsNoTracking().ToListAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		//public async Task<IEnumerable<ApiUserSearchV2>> SearchTrainer(string userId, string UserType = null)
		//{
		//    try
		//    {
		//        if (!string.IsNullOrEmpty(UserType))
		//        {
		//            if (UserType.ToLower() == "consultant")
		//            {
		//                var result = (from user in this._db.UserMaster
		//                              join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                              where ((user.UserId.ToLower() == Security.Encrypt(userId.ToLower())
		//                              || user.UserName.ToLower().StartsWith(userId.ToLower()))
		//                              && user.IsDeleted == false && user.IsActive == true)
		//                              select new ApiUserSearchV2
		//                              {
		//                                  UserId = Security.EncryptForUI(Security.Decrypt(user.UserId)),
		//                                  Name = user.UserName,
		//                                  EmailId = Security.EncryptForUI(Security.Decrypt(user.EmailId)),
		//                                  Id = (Security.EncryptForUI(user.Id.ToString())),
		//                                  ProfilePicture = userDetails.ProfilePicture,
		//                                  MobileNumber = Security.EncryptForUI(Security.Decrypt(userDetails.MobileNumber)),
		//                                  UserType = userDetails.UserSubType,
		//                                  NameUserId = user.UserName + " - " + Security.Decrypt(user.UserId)
		//                              });

		//                if (UserType != null)
		//                {
		//                    result = result.Where(a => a.UserType == UserType);
		//                }
		//                return await result.AsNoTracking().ToListAsync();
		//            }
		//            else
		//            {
		//                var result = (from user in this._db.UserMaster
		//                              join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                              where ((user.UserId.ToLower() == Security.Encrypt(userId.ToLower())
		//                              || user.UserName.ToLower().StartsWith(userId.ToLower()))
		//                              && user.IsDeleted == false && user.IsActive == true)
		//                              select new ApiUserSearchV2
		//                              {
		//                                  UserId = Security.EncryptForUI(Security.Decrypt(user.UserId)),
		//                                  Name = user.UserName,
		//                                  EmailId = Security.EncryptForUI(Security.Decrypt(user.EmailId)),
		//                                  Id = (Security.EncryptForUI(user.Id.ToString())),
		//                                  ProfilePicture = userDetails.ProfilePicture,
		//                                  MobileNumber = Security.EncryptForUI(Security.Decrypt(userDetails.MobileNumber)),
		//                                  UserType = userDetails.UserType,
		//                                  NameUserId = user.UserName + " - " + Security.Decrypt(user.UserId)
		//                              });

		//                if (UserType != null)
		//                {
		//                    result = result.Where(a => a.UserType == UserType);
		//                }
		//                return await result.AsNoTracking().ToListAsync();
		//            }
		//        }
		//        else
		//        {
		//            var result = (from user in this._db.UserMaster
		//                          join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                          where ((user.UserId.ToLower() == Security.Encrypt(userId.ToLower())
		//                          || user.UserName.ToLower().StartsWith(userId.ToLower()))
		//                          && user.IsDeleted == false && user.IsActive == true)
		//                          select new ApiUserSearchV2
		//                          {
		//                              UserId = Security.EncryptForUI(Security.Decrypt(user.UserId)),
		//                              Name = user.UserName,
		//                              EmailId = Security.EncryptForUI(Security.Decrypt(user.EmailId)),
		//                              Id = (Security.EncryptForUI(user.Id.ToString())),
		//                              ProfilePicture = userDetails.ProfilePicture,
		//                              MobileNumber = Security.EncryptForUI(Security.Decrypt(userDetails.MobileNumber)),
		//                              UserType = userDetails.UserType,
		//                              NameUserId = user.UserName + " - " + Security.Decrypt(user.UserId)
		//                          });

		//            if (UserType != null)
		//            {
		//                result = result.Where(a => a.UserType == UserType);
		//            }
		//            return await result.AsNoTracking().ToListAsync();
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}
		//public async Task<IEnumerable<ApiUserSearch>> SearchByUserRole(int tokenId, string userId)
		//{
		//    string userName = userId;
		//    userId = Security.Encrypt(userId.ToLower());

		//    List<ApiUserSearch> users = new List<ApiUserSearch>();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetUserByUserRole";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@UId", SqlDbType.Int) { Value = tokenId });
		//                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar) { Value = userId });
		//                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar) { Value = userName });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {

		//                        var user = new ApiUserSearch
		//                        {

		//                            Id = Convert.ToInt32(row["Id"].ToString()),
		//                            Name = row["Name"].ToString(),
		//                            EmailId = Security.Decrypt(row["EmailId"].ToString()),
		//                            UserId = Security.Decrypt(row["UserId"].ToString()),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),

		//                        };
		//                        users.Add(user);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return users;
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<APIUserMaster> GetUserObject(APIUserMaster user, string UserRole, string orgCode)
		//{
		//	try
		//	{
		//		using (var dbContext = _customerConnectionString.GetDbContext())
		//		{
		//			var connection = dbContext.Database.GetDbConnection();
		//			if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//				connection.Open();
		//			using (var cmd = connection.CreateCommand())
		//			{

		//				var lowerCaseAllow = await GetConfigurationValueAsync("SAVE_USER_IMPORT_ASIS", orgCode);

		//				if (lowerCaseAllow.ToString() == "Yes")
		//				{
		//					user.Location = user.Location == null ? null : user.Location.Trim();
		//					user.Business = user.Business == null ? null : user.Business.Trim();
		//					user.Group = user.Group == null ? null : user.Group.Trim();
		//					user.Area = user.Area == null ? null : user.Area.Trim();
		//					user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.Trim();
		//					user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.Trim();
		//					user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.Trim();
		//					user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.Trim();
		//					user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.Trim();
		//					user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.Trim();
		//					user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.Trim();
		//					user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.Trim();
		//					user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.Trim();
		//					user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.Trim();
		//					user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.Trim();
		//					user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.Trim();
		//					user.ConfigurationColumn13 = user.ConfigurationColumn13 == null ? null : user.ConfigurationColumn13.Trim();
		//					user.ConfigurationColumn14 = user.ConfigurationColumn14 == null ? null : user.ConfigurationColumn14.Trim();
		//					user.ConfigurationColumn15 = user.ConfigurationColumn15 == null ? null : user.ConfigurationColumn15.Trim();
		//					user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();
		//				}
		//				else
		//				{

		//					user.Location = user.Location == null ? null : user.Location.ToLower().Trim();
		//					user.Business = user.Business == null ? null : user.Business.ToLower().Trim();
		//					user.Group = user.Group == null ? null : user.Group.Trim();
		//					user.Area = user.Area == null ? null : user.Area.ToLower().Trim();
		//					user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.ToLower().Trim();
		//					user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.ToLower().Trim();
		//					user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.ToLower().Trim();
		//					user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.ToLower().Trim();
		//					user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.ToLower().Trim();
		//					user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.ToLower().Trim();
		//					user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.ToLower().Trim();
		//					user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.ToLower().Trim();
		//					user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.ToLower().Trim();
		//					user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.ToLower().Trim();
		//					user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.ToLower().Trim();
		//					user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.ToLower().Trim();
		//					user.ConfigurationColumn13 = user.ConfigurationColumn13 == null ? null : user.ConfigurationColumn13.ToLower().Trim();
		//					user.ConfigurationColumn14 = user.ConfigurationColumn14 == null ? null : user.ConfigurationColumn14.ToLower().Trim();
		//					user.ConfigurationColumn15 = user.ConfigurationColumn15 == null ? null : user.ConfigurationColumn15.ToLower().Trim();
		//					user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();

		//				}




		//				cmd.CommandText = "GetIdByName";
		//				cmd.CommandType = CommandType.StoredProcedure;
		//				cmd.Parameters.Add(new SqlParameter("@Location", SqlDbType.VarChar) { Value = user.Location });
		//				cmd.Parameters.Add(new SqlParameter("@Business", SqlDbType.VarChar) { Value = user.Business });
		//				cmd.Parameters.Add(new SqlParameter("@Group", SqlDbType.VarChar) { Value = user.Group });
		//				cmd.Parameters.Add(new SqlParameter("@Area", SqlDbType.VarChar) { Value = user.Area });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration1", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration2", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration3", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration4", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration5", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration6", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration7", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration8", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration9", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration10", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration11", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration12", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration13", SqlDbType.VarChar) { Value = user.ConfigurationColumn13 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration14", SqlDbType.VarChar) { Value = user.ConfigurationColumn14 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration15", SqlDbType.VarChar) { Value = user.ConfigurationColumn15 });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration1Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 == null ? null : Security.Encrypt(user.ConfigurationColumn1) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration2Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 == null ? null : Security.Encrypt(user.ConfigurationColumn2) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration3Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 == null ? null : Security.Encrypt(user.ConfigurationColumn3) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration4Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 == null ? null : Security.Encrypt(user.ConfigurationColumn4) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration5Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 == null ? null : Security.Encrypt(user.ConfigurationColumn5) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration6Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 == null ? null : Security.Encrypt(user.ConfigurationColumn6) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration7Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 == null ? null : Security.Encrypt(user.ConfigurationColumn7) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration8Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 == null ? null : Security.Encrypt(user.ConfigurationColumn8) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration9Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 == null ? null : Security.Encrypt(user.ConfigurationColumn9) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration10Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 == null ? null : Security.Encrypt(user.ConfigurationColumn10) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration11Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 == null ? null : Security.Encrypt(user.ConfigurationColumn11) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration12Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 == null ? null : Security.Encrypt(user.ConfigurationColumn12) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration13Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn13 == null ? null : Security.Encrypt(user.ConfigurationColumn13) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration14Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn14 == null ? null : Security.Encrypt(user.ConfigurationColumn14) });
		//				cmd.Parameters.Add(new SqlParameter("@Configuration15Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn15 == null ? null : Security.Encrypt(user.ConfigurationColumn15) });
		//				cmd.Parameters.Add(new SqlParameter("@AreaEncrypted", SqlDbType.VarChar) { Value = user.Area == null ? null : Security.Encrypt(user.Area) });
		//				cmd.Parameters.Add(new SqlParameter("@BuisnessEncrypted", SqlDbType.VarChar) { Value = user.Business == null ? null : Security.Encrypt(user.Business) });
		//				cmd.Parameters.Add(new SqlParameter("@GroupEncrypted", SqlDbType.VarChar) { Value = user.Group == null ? null : Security.Encrypt(user.Group) });
		//				cmd.Parameters.Add(new SqlParameter("@LocationEncrypted", SqlDbType.VarChar) { Value = user.Location == null ? null : Security.Encrypt(user.Location) });
		//				cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.VarChar) { Value = "10-11-2018" });
		//				cmd.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.VarChar) { Value = UserRole });
		//				cmd.Parameters.Add(new SqlParameter("@JobRoleName", SqlDbType.VarChar) { Value = user.JobRoleName });
		//				DbDataReader reader = await cmd.ExecuteReaderAsync();
		//				DataTable dt = new DataTable();
		//				dt.Load(reader);
		//				if (dt.Rows.Count > 0)
		//				{
		//					foreach (DataRow row in dt.Rows)
		//					{
		//						user.LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? (int?)null : Convert.ToInt32(row["LocationId"].ToString());
		//						user.BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? (int?)null : Convert.ToInt32(row["BusinessId"].ToString());
		//						user.GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? (int?)null : Convert.ToInt32(row["GroupId"].ToString());
		//						user.AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? (int?)null : Convert.ToInt32(row["AreaId"].ToString());
		//						user.ConfigurationColumn1Id = string.IsNullOrEmpty(row["Configuration1Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration1Id"].ToString());
		//						user.ConfigurationColumn2Id = string.IsNullOrEmpty(row["Configuration2Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration2Id"].ToString());
		//						user.ConfigurationColumn3Id = string.IsNullOrEmpty(row["Configuration3Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration3Id"].ToString());
		//						user.ConfigurationColumn4Id = string.IsNullOrEmpty(row["Configuration4Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration4Id"].ToString());
		//						user.ConfigurationColumn5Id = string.IsNullOrEmpty(row["Configuration5Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration5Id"].ToString());
		//						user.ConfigurationColumn6Id = string.IsNullOrEmpty(row["Configuration6Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration6Id"].ToString());
		//						user.ConfigurationColumn7Id = string.IsNullOrEmpty(row["Configuration7Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration7Id"].ToString());
		//						user.ConfigurationColumn8Id = string.IsNullOrEmpty(row["Configuration8Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration8Id"].ToString());
		//						user.ConfigurationColumn9Id = string.IsNullOrEmpty(row["Configuration9Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration9Id"].ToString());
		//						user.ConfigurationColumn10Id = string.IsNullOrEmpty(row["Configuration10Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration10Id"].ToString());
		//						user.ConfigurationColumn11Id = string.IsNullOrEmpty(row["Configuration11Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration11Id"].ToString());
		//						user.ConfigurationColumn12Id = string.IsNullOrEmpty(row["Configuration12Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration12Id"].ToString());
		//						user.ConfigurationColumn13Id = string.IsNullOrEmpty(row["Configuration13Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration13Id"].ToString());
		//						user.ConfigurationColumn14Id = string.IsNullOrEmpty(row["Configuration14Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration14Id"].ToString());
		//						user.ConfigurationColumn15Id = string.IsNullOrEmpty(row["Configuration15Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration15Id"].ToString());
		//						user.HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? (int?)null : Convert.ToInt32(row["HouseId"].ToString());
		//						user.IsAllConfigured = Convert.ToBoolean(row["IsAllConfigured"].ToString());
		//						user.JobRoleId = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? (int?)null : Convert.ToInt32(row["JobRoleId"].ToString());

		//					}
		//				}
		//				reader.Dispose();
		//			}
		//			connection.Close();
		//		}
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//		throw ex;
		//	}
		//	return user;
		//}

		public async Task<int> GetUserIdByEmail(string email)
		{
			return await this._db.UserMaster.Where(u => u.EmailId == Security.Encrypt(email.ToLower()) && u.IsDeleted == false).Select(u => u.Id).SingleOrDefaultAsync();
		}

		//public async Task<IEnumerable<APIUserDto>> GetUserNameAndId(string name)
		//{
		//    var users = (from u in this._db.UserMaster
		//                 where u.UserName.StartsWith(name)
		//                 select new APIUserDto
		//                 {
		//                     Id = u.Id,
		//                     Name = u.UserName
		//                 });
		//    return await users.ToListAsync();
		//}

		//public async Task<IEnumerable<APIGetUserMasterId>> GetUserMasterId(string UserId)
		//{
		//    var users = (from u in this._db.UserMaster
		//                 where u.UserId == UserId
		//                 select new APIGetUserMasterId
		//                 {
		//                     Id = u.Id,
		//                     Name = u.UserName
		//                 });
		//    return await users.ToListAsync();
		//}

		//public async Task<IEnumerable<APIUserDto>> GetAccountManagers(string name)
		//{
		//    //Always get Account Managers list from Enthralltech db
		//    var connectionString = this._configuration.GetConnectionString("DefaultConnection");
		//    using (var context = _customerConnectionString.GetDbContext(connectionString))
		//    {
		//        var users = (from u in context.UserMaster
		//                     where u.UserName.StartsWith(name) && u.UserRole == "AM"
		//                     select new APIUserDto
		//                     {
		//                         Id = u.Id,
		//                         Name = u.UserName
		//                     });
		//        return await users.ToListAsync();
		//    }

		//}


		//public async Task<APIForgetPassword> ForgotPassword(string userId)
		//{
		//    return await (from user in this._db.UserMaster
		//                  join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                  where (user.UserId == Security.Encrypt(userId.ToLower())
		//                  || user.EmailId == Security.Encrypt(userId.ToLower()))
		//                  select new APIForgetPassword
		//                  {
		//                      ToEmail = Security.Decrypt(user.EmailId),
		//                      CustomerCode = userDetails.CustomerCode,
		//                      ID = user.Id,
		//                      UserId = Security.Decrypt(user.UserId),
		//                      UserName = user.UserName,
		//                      MobileNumber = Security.Decrypt(userDetails.MobileNumber)

		//                  }).FirstOrDefaultAsync();
		//}

		//public async Task<bool> CheckUserTypeForAuthOTP(string userId)
		//{
		//    UserMaster user = await this._db.UserMaster.Where(us => us.UserId == Security.Encrypt(userId.ToLower()) || us.EmailId == Security.Encrypt(userId.ToLower())).FirstOrDefaultAsync();
		//    if (user != null)
		//        return true;
		//    else
		//        return false;
		//}
		public async Task<string> GetCslEmpCode(int userId)
		{
			return await (from user in this._db.UserMaster
						  join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
						  join config6 in this._db.Configure6 on userDetails.ConfigurationColumn6 equals config6.Id
						  where user.Id == userId
						  select config6.Name).FirstOrDefaultAsync();
		}

		public async Task<bool> CheckForAuthOTP(string userId)
		{
			string UserRole = await this._db.UserMaster.Where(us => us.UserId == Security.Encrypt(userId.ToLower()) || us.EmailId == Security.Encrypt(userId.ToLower())).Select(r => r.UserRole).FirstOrDefaultAsync();
			if (UserRole == "CA")
				return true;
			else
				return false;
		}
		public async Task<UserMasterDetails> GetUserDetails(int userId)
		{
			UserMasterDetails UserMasterDetails = await (from userDetails in this._db.UserMasterDetails.AsNoTracking()
														 where userDetails.UserMasterId == userId
														 select userDetails).FirstOrDefaultAsync();
			UserMasterDetails.MobileNumber = Security.Decrypt(UserMasterDetails.MobileNumber);
			UserMasterDetails.ReportsTo = Security.Decrypt(UserMasterDetails.ReportsTo);
			return UserMasterDetails;
		}




		//public async Task<APIForgetPassword> GetEmailTemplate(string customerCode)
		//{
		//    return await (from mailTemplateDesigner in this._db.MailTemplateDesigner
		//                  where (mailTemplateDesigner.CustomerCode.StartsWith(customerCode) && mailTemplateDesigner.IsDeleted == 0) && (mailTemplateDesigner.MailSubject == "Forgot Password")
		//                  select new APIForgetPassword
		//                  {
		//                      CustomerCode = mailTemplateDesigner.CustomerCode,
		//                      Subject = mailTemplateDesigner.MailSubject,
		//                      Message = mailTemplateDesigner.TemplateContent

		//                  }).SingleOrDefaultAsync();
		//}
		//public async Task<APIUserMaster> GetUserByUserId(string userId)
		//{
		//    return await (from user in _db.UserMaster
		//                  join userDetails in _db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                  where user.UserId == Security.Encrypt(userId.ToLower())
		//                  && user.IsDeleted == false
		//                  select new APIUserMaster
		//                  {
		//                      AccountCreatedDate = userDetails.AccountCreatedDate,
		//                      AccountExpiryDate = user.AccountExpiryDate,
		//                      Currency = userDetails.Currency,
		//                      CustomerCode = userDetails.CustomerCode,
		//                      DateOfBirth = userDetails.DateOfBirth,
		//                      DateOfJoining = userDetails.DateOfJoining,
		//                      EmailId = Security.Decrypt(user.EmailId),
		//                      Gender = userDetails.Gender,
		//                      Id = user.Id,
		//                      IsActive = user.IsActive,
		//                      Language = userDetails.Language,
		//                      LastModifiedDate = userDetails.LastModifiedDate,
		//                      MobileNumber = Security.Decrypt(userDetails.MobileNumber),
		//                      Password = user.Password,
		//                      ProfilePicture = userDetails.ProfilePicture,
		//                      SerialNumber = userDetails.SerialNumber,
		//                      TimeZone = userDetails.TimeZone,
		//                      UserId = Security.Decrypt(user.UserId),
		//                      UserName = user.UserName,
		//                      UserRole = user.UserRole,
		//                      UserType = userDetails.UserType,
		//                      ConfigurationColumn1Id = userDetails.ConfigurationColumn1,
		//                      ConfigurationColumn2Id = userDetails.ConfigurationColumn2,
		//                      ConfigurationColumn3Id = userDetails.ConfigurationColumn3,
		//                      ConfigurationColumn4Id = userDetails.ConfigurationColumn4,
		//                      ConfigurationColumn5Id = userDetails.ConfigurationColumn5,
		//                      ConfigurationColumn6Id = userDetails.ConfigurationColumn6,
		//                      ConfigurationColumn7Id = userDetails.ConfigurationColumn7,
		//                      ConfigurationColumn8Id = userDetails.ConfigurationColumn8,
		//                      ConfigurationColumn9Id = userDetails.ConfigurationColumn9,
		//                      ConfigurationColumn10Id = userDetails.ConfigurationColumn10,
		//                      ConfigurationColumn11Id = userDetails.ConfigurationColumn11,
		//                      ConfigurationColumn12Id = userDetails.ConfigurationColumn12,
		//                      ReportsTo = Security.Decrypt(userDetails.ReportsTo),
		//                      AreaId = userDetails.AreaId,
		//                      BusinessId = userDetails.BusinessId,
		//                      GroupId = userDetails.GroupId,
		//                      LocationId = userDetails.LocationId
		//                  }).FirstOrDefaultAsync();
		//}

		//public async Task<List<APIUserDto>> GetSearchParameter(string serarchBy, string searchtext)
		//{
		//    IQueryable<APIUserDto> UserDto = null;

		//    if (!string.IsNullOrWhiteSpace(serarchBy) && serarchBy.Equals("EmailID"))
		//    {
		//        UserDto = (from c in _db.UserMaster
		//                   where c.EmailId == Security.Encrypt(searchtext.ToLower())
		//                   select new APIUserDto { EmailId = Security.Decrypt(c.EmailId) });
		//        return await UserDto.ToListAsync();
		//    }
		//    if (!string.IsNullOrWhiteSpace(serarchBy) && serarchBy.Equals("MobileNumber"))
		//    {
		//        UserDto = (from c in _db.UserMasterDetails
		//                   where c.MobileNumber == Security.Encrypt(searchtext.ToLower())
		//                   select new APIUserDto { MobileNumber = Security.Decrypt(c.MobileNumber) });
		//        return await UserDto.ToListAsync();
		//    }
		//    if (!string.IsNullOrWhiteSpace(serarchBy) && serarchBy.Equals("UserRole"))
		//    {
		//        UserDto = (from c in _db.UserMaster where ((c.UserRole.StartsWith(searchtext))) select new APIUserDto { UserRole = c.UserRole });
		//        return await UserDto.ToListAsync();
		//    }
		//    return await UserDto.ToListAsync();
		//}

		//public async Task<object> Search(string searchBy, string searchText, string orgCode, int userId)
		//{
		//    searchBy = searchBy.ToLower();

		//    switch (searchBy)
		//    {
		//        case "userid":
		//            if (orgCode.ToLower() == "ivp" || orgCode.ToLower() == "ent")
		//                return await GetUserSearchResult(searchBy, searchText, orgCode);
		//            else
		//                return await _db.UserMaster.Where(u => u.IsDeleted == false && u.IsActive == true && u.UserId == Security.Encrypt(searchText.ToLower())).Select(e => new { Name = Security.Decrypt(e.UserId), e.Id }).ToListAsync();
		//        case "emailid":
		//            if (orgCode.ToLower() == "ivp" || orgCode.ToLower() == "ent")
		//                return await GetUserSearchResult(searchBy, searchText, orgCode);
		//            else
		//                return await _db.UserMaster.Where(u => u.IsDeleted == false &&  u.IsActive == true && u.EmailId == Security.Encrypt(searchText.ToLower())).Select(e => new { Name = Security.Decrypt(e.EmailId), e.Id }).ToListAsync();
		//        case "mobilenumber":
		//            return await _db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.MobileNumber.StartsWith(Security.Encrypt(searchText.ToLower()))).Select(e => new { Name = Security.Decrypt(e.MobileNumber), Id = e.UserMasterId }).ToListAsync();
		//        case "userrole":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.IsActive == true && u.UserRole.StartsWith(searchText)).Select(e => new { Name = e.UserRole, e.Id }).ToListAsync();
		//        case "username":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.IsActive == true && u.UserName.StartsWith(searchText)).Select(e => new { Name= e.UserName, e.Id ,UserId = Security.Decrypt(e.UserId)}).ToListAsync();
		//        //case "business":
		//        //    return await _db.Business.Where(u => u.IsDeleted == 0  == true && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        //case "group":
		//        //    return await _db.Group.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "area":
		//            return await _db.Area.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "location":
		//            return await _db.Location.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn1":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn1", searchText);
		//            else
		//                return await _db.Configure1.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn2":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn2", searchText);
		//            else
		//                return await _db.Configure2.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn3":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn3", searchText);
		//            else
		//                return await _db.Configure3.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn4":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn4", searchText);
		//            else
		//                return await _db.Configure4.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn5":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn5", searchText);
		//            else
		//                return await _db.Configure5.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn6":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn6", searchText);
		//            else
		//                return await _db.Configure6.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn7":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn7", searchText);
		//            else
		//                return await _db.Configure7.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn8":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn8", searchText);
		//            else
		//                return await _db.Configure8.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn9":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn9", searchText);
		//            else
		//                return await _db.Configure9.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn10":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn10", searchText);
		//            else
		//                return await _db.Configure10.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn11":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn11", searchText);
		//            else
		//                return await _db.Configure11.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn12":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn12", searchText);
		//            else
		//                return await _db.Configure12.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn13":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn13", searchText);
		//            else
		//                return await _db.Configure13.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn14":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn14", searchText);
		//            else
		//                return await _db.Configure14.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();

		//        case "configurationcolumn15":
		//            if (orgCode.ToLower().StartsWith("canh"))
		//                return await GetConfigurationColumnSearchResult(userId, "configurationcolumn15", searchText);
		//            else
		//                return await _db.Configure15.Where(u => u.IsDeleted == 0 && u.Name.StartsWith(searchText)).Select(e => new { e.Name, e.Id }).ToListAsync();


		//    }
		//    return null;
		//}

		//public async Task<List<LocationandAreaSearch>> SearchUserName(string search = null)
		//{

		//    return await (from um in _db.UserMaster
		//                  join umd in _db.UserMasterDetails on um.Id equals umd.UserMasterId
		//                  join area in _db.Area on umd.AreaId equals area.Id
		//                  into areajoin
		//                  from area in areajoin.DefaultIfEmpty()
		//                  join busi in _db.Business on umd.BusinessId equals busi.Id
		//                  into busjoin
		//                  from business in busjoin.DefaultIfEmpty()
		//                  where um.IsDeleted == false && ( search == null || um.UserName.StartsWith(search))
		//                  select new LocationandAreaSearch
		//                  {
		//                      UserId = Security.Decrypt(um.UserId),
		//                      BusinessName = business.Name,
		//                      AreaName = area.Name,
		//                      UserName = um.UserName,
		//                      Id = um.Id
		//                  }
		//            ).ToListAsync();
		//}


		//private async Task<List<APISearchResult>> GetLocationSearchResult(int userId, string search = null)
		//{
		//    return await (from userMasterDetails in this._db.UserMasterDetails
		//                  join location in this._db.Location on userMasterDetails.LocationId equals location.Id
		//                  where userMasterDetails.UserMasterId == userId
		//                  && (search == null || location.Name.StartsWith(search))
		//                  select new APISearchResult
		//                  {
		//                      Name = location.Name,
		//                      Id = location.Id
		//                  }).ToListAsync();
		//}

		//private async Task<List<APISearchResult>> GetConfigurationColumnSearchResult(int userId, string columnName, string search = null)
		//{
		//    List<APISearchResult> searchDetails = new List<APISearchResult>();
		//    try
		//    {
		//        var connection = _db.Database.GetDbConnection();
		//        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//            connection.Open();

		//        using (var cmd = connection.CreateCommand())
		//        {
		//            cmd.CommandText = "GetNominationSearchDataByConfigColumn";
		//            cmd.CommandType = CommandType.StoredProcedure;
		//            cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
		//            cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
		//            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
		//            DbDataReader reader = await cmd.ExecuteReaderAsync();
		//            DataTable dt = new DataTable();
		//            dt.Load(reader);

		//            if (dt.Rows.Count > 0)
		//            {
		//                foreach (DataRow row in dt.Rows)
		//                {

		//                    var detail = new APISearchResult
		//                    {
		//                        Id = Convert.ToInt32(row["Id"].ToString()),
		//                        Name = row["Name"].ToString()
		//                    };

		//                    searchDetails.Add(detail);
		//                }
		//            }

		//            reader.Dispose();

		//        }
		//        connection.Close();
		//        return searchDetails;
		//    }

		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<object> GetNameById(string searchBy, int Id)
		//{
		//    searchBy = searchBy.ToLower();
		//    switch (searchBy)
		//    {
		//        case "userid":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new { Name = Security.Decrypt(e.UserId), e.Id }).FirstOrDefaultAsync();
		//        case "emailid":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new { Name = Security.Decrypt(e.EmailId) }).FirstOrDefaultAsync();
		//        case "mobilenumber":
		//            return await _db.UserMasterDetails.Where(u => u.IsDeleted == false && u.UserMasterId == Id).Select(e => new { Name = Security.Decrypt(e.MobileNumber) }).FirstOrDefaultAsync();
		//        case "userrole":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new { Name = e.UserRole }).FirstOrDefaultAsync();
		//        case "business":
		//            return await _db.Business.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "group":
		//            return await _db.Group.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "area":
		//            return await _db.Area.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "location":
		//            return await _db.Location.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn1":
		//            return await _db.Configure1.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn2":
		//            return await _db.Configure2.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn3":
		//            return await _db.Configure3.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn4":
		//            return await _db.Configure4.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn5":
		//            return await _db.Configure5.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn6":
		//            return await _db.Configure6.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn7":
		//            return await _db.Configure7.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn8":
		//            return await _db.Configure8.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn9":
		//            return await _db.Configure9.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn10":
		//            return await _db.Configure10.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn11":
		//            return await _db.Configure11.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "configurationcolumn12":
		//            return await _db.Configure12.Where(u => u.IsDeleted == 0 && u.Id == Id).Select(e => new { e.Name }).FirstOrDefaultAsync();
		//        case "username":
		//            return await _db.UserMaster.Where(u => u.IsDeleted == false && u.Id == Id).Select(e => new { Name = e.UserName, EmailId = Security.Decrypt(e.EmailId) }).FirstOrDefaultAsync();
		//    }
		//    return null;
		//}

		public async Task<string> GetUserName(int Id)
		{
			return await this._db.UserMaster.Where(u => u.Id == Id && u.IsDeleted == false).Select(u => u.UserName).SingleOrDefaultAsync();
		}

		//public async Task<string> ConfidentialConfigColumn()
		//{
		//    string buddyTrainerConfigColumn = null;
		//    buddyTrainerConfigColumn= await _db.UserSettings.Where(a => a.IsConfidential == true && a.IsConfigured == true).Select(a => a.ConfiguredColumnName).FirstOrDefaultAsync();
		//    return buddyTrainerConfigColumn;
		//}
	   
		//public async Task<string> BusinessDetailsPost(APIBusinessDetails[] businesspostmultiple, int userid)
		//{
		//    try
		//    {
				
		//        foreach (APIBusinessDetails business in businesspostmultiple)
		//        {
		//            if (await this._db.BusinessDetails.Where(u => u.BusinessId == business.BusinessId && u.UserId == business.UserId).CountAsync() > 0)
		//            {
		//            }
		//            else
		//            {
		//                BusinessDetails businesspost = new BusinessDetails();
		//                businesspost.BusinessId = business.BusinessId;
		//                businesspost.UserId = business.UserId;
		//                businesspost.Isdeleted = false;
		//                businesspost.CreatedBy = userid;
		//                businesspost.CreatedDate = DateTime.Now;
		//                businesspost.ModifiedBy = userid;
		//                businesspost.ModifiedDate = DateTime.Now;
		//                this._db.BusinessDetails.Add(businesspost);
		//                await this._db.SaveChangesAsync();
		//            }
		//        }
		//        return "ok";
		//    }
		//    catch(System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<List<APIGetBusinessDetails>> GetBusinessDetails(int userid)
		//{
		//    try
		//    {
		//       var result = (from user in this._db.BusinessDetails
		//                      join Business in this._db.Business on user.BusinessId equals Business.Id
		//                      where ((user.UserId == userid) && user.Isdeleted == false)
		//                      select new APIGetBusinessDetails
		//                      {
		//                          UserId = user.UserId,
		//                          BusinessId = user.BusinessId,
		//                          BusinesName = Business.Name,
		//                          Name = Business.Name,
		//                          Id = user.BusinessId

		//                      }).ToListAsync();
		//            return await result;
				
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<int> DeleteBusinessDetails(APIBusinessDetails businesspostmultiple)
		//{
		//    try
		//    {
		//            BusinessDetails User = this._db.BusinessDetails.Where(u => u.BusinessId == businesspostmultiple.BusinessId && u.UserId == businesspostmultiple.UserId).FirstOrDefault();
		//            if (User == null)
		//                return 0;

		//            User.Isdeleted = true;
		//            this._db.BusinessDetails.Remove(User);

		//            await this._db.SaveChangesAsync();
		//            return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<List<string>> GetDecryptedValues(DecryptedValues decryptedValues)
		//{
		//    try
		//    {
		//        List<string> decrypted = new List<string>();
			  
		//        foreach (string item in decryptedValues.value)
		//        {
		//            decrypted.Add( Security.Decrypt(item) );
					
					
		//        }

		//        return decrypted.ToList();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}


		//public async Task<string> ProcessImportFile(APIFilePath aPIFilePath, int userId, string orgCode)
		//{
		//    string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
		//    sWebRootFolder = Path.Combine(sWebRootFolder);
		//    string filePath = sWebRootFolder + aPIFilePath.Path;

		//    string result;
		//    UserMasterImport ProcessFileobj = new UserMasterImport(_db, this, _customerConnectionString, _configuration, _identitySv);
		//    ProcessFileobj.Reset();
		//    DataTable userImportdt = ProcessFileobj.ReadFile(filePath);
		//    if (userImportdt == null || userImportdt.Rows.Count == 0)
		//        return Record.FileDoesNotContainsData;

		//    var userSetting = await _userSettingsRepository.GetUserSetting(orgCode);
		//    List<APIUserSetting> aPIUserSettings = Mapper.Map<List<APIUserSetting>>(userSetting);

		//    APIIsFileValid resultMessage = await ProcessFileobj.ValidateFileColumnHeaders(userImportdt, orgCode, aPIUserSettings);
		//    if (resultMessage.Flag)
		//        result = await ProcessFileobj.ProcessRecordsAsync(userId, resultMessage.userImportdt, orgCode, aPIUserSettings, await GetPasswordDefaultForUser(orgCode));
		//    else
		//        result = Record.FileFieldInvalid;
		//    ProcessFileobj.Reset();
		//    return result;
		//}

		//private async Task<string> GetPasswordDefaultForUser(string orgCode)
		//{
		//    var allowRANDOMPASSWORD = await GetConfigurationValueAsync("RANDOM_PASSWORD", orgCode);

		//    if (String.Compare(allowRANDOMPASSWORD, "yes", true) == 0)
		//        return RandomPassword.GenerateUserPassword(8, 1);
		//    else if (orgCode.ToLower().Contains("keventers"))
		//        return "Keventers@123";

		//    return this._configuration["DeafultPassword"];
		//}

		public async Task<APIUserMaster> GetByUserIdNew(string userId)
		{
			try
			{
				{
					var connection = _db.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					APIUserMaster user = null;
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetUserByUserId";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.VarChar) { Value = userId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);

						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{

								user = new APIUserMaster
								{
									AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
									AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
									ConfigurationColumn1 = row["configure1"].ToString(),
									ConfigurationColumn2 = row["configure2"].ToString(),
									ConfigurationColumn3 = row["configure3"].ToString(),
									ConfigurationColumn4 = row["configure4"].ToString(),
									ConfigurationColumn5 = row["configure5"].ToString(),
									ConfigurationColumn6 = row["configure6"].ToString(),
									ConfigurationColumn7 = row["configure7"].ToString(),
									ConfigurationColumn8 = row["configure8"].ToString(),
									ConfigurationColumn9 = row["configure9"].ToString(),
									ConfigurationColumn10 = row["configure10"].ToString(),
									ConfigurationColumn11 = row["configure11"].ToString(),
									ConfigurationColumn12 = row["configure12"].ToString(),
									ConfigurationColumn1Id = string.IsNullOrEmpty(row["configure1Id"].ToString()) ? null : int.Parse(row["configure1Id"].ToString()) as int?,
									ConfigurationColumn2Id = string.IsNullOrEmpty(row["configure2Id"].ToString()) ? null : int.Parse(row["configure2Id"].ToString()) as int?,
									ConfigurationColumn3Id = string.IsNullOrEmpty(row["configure3Id"].ToString()) ? null : int.Parse(row["configure3Id"].ToString()) as int?,
									ConfigurationColumn4Id = string.IsNullOrEmpty(row["configure4Id"].ToString()) ? null : int.Parse(row["configure4Id"].ToString()) as int?,
									ConfigurationColumn5Id = string.IsNullOrEmpty(row["configure5Id"].ToString()) ? null : int.Parse(row["configure5Id"].ToString()) as int?,
									ConfigurationColumn6Id = string.IsNullOrEmpty(row["configure6Id"].ToString()) ? null : int.Parse(row["configure6Id"].ToString()) as int?,
									ConfigurationColumn7Id = string.IsNullOrEmpty(row["configure7Id"].ToString()) ? null : int.Parse(row["configure7Id"].ToString()) as int?,
									ConfigurationColumn8Id = string.IsNullOrEmpty(row["configure8Id"].ToString()) ? null : int.Parse(row["configure8Id"].ToString()) as int?,
									ConfigurationColumn9Id = string.IsNullOrEmpty(row["configure9Id"].ToString()) ? null : int.Parse(row["configure9Id"].ToString()) as int?,
									ConfigurationColumn10Id = string.IsNullOrEmpty(row["configure10Id"].ToString()) ? null : int.Parse(row["configure10Id"].ToString()) as int?,
									ConfigurationColumn11Id = string.IsNullOrEmpty(row["configure11Id"].ToString()) ? null : int.Parse(row["configure11Id"].ToString()) as int?,
									ConfigurationColumn12Id = string.IsNullOrEmpty(row["configure12Id"].ToString()) ? null : int.Parse(row["configure12Id"].ToString()) as int?,
									Currency = row["Currency"].ToString(),
									CustomerCode = row["CustomerCode"].ToString(),
									DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
									DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
									EmailId = Security.Decrypt(row["EmailId"].ToString()),
									Gender = row["Gender"].ToString(),
									Id = Convert.ToInt32(row["Id"].ToString()),
									IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
									Language = row["Language"].ToString(),
									LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
									MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
									Password = row["Password"].ToString(),
									ProfilePicture = row["ProfilePicture"].ToString(),
									SerialNumber = row["SerialNumber"].ToString(),
									TimeZone = row["TimeZone"].ToString(),
									UserId = Security.Decrypt(row["UserId"].ToString()),
									UserName = row["UserName"].ToString(),
									UserRole = row["UserRole"].ToString(),
									UserType = row["UserType"].ToString(),
									Location = row["location"].ToString(),
									Area = row["area"].ToString(),
									Group = row["groupname"].ToString(),
									Business = row["buisness"].ToString(),
									ReportsTo = Security.Decrypt(row["ReportsTo"].ToString()),
									ModifiedByName = row["ModifiedByName"].ToString(),
									BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? null : (int?)Convert.ToInt32(row["BusinessId"].ToString()),
									LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? null : (int?)Convert.ToInt32(row["LocationId"].ToString()),
									AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? null : (int?)Convert.ToInt32(row["AreaId"].ToString()),
									GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? null : (int?)Convert.ToInt32(row["GroupId"].ToString()),
									IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
									IsEnableDegreed = string.IsNullOrEmpty(row["Degreed"].ToString()) ? false : Convert.ToBoolean(row["Degreed"].ToString()),
									TermsCondintionsAccepted = string.IsNullOrEmpty(row["TermsCondintionsAccepted"].ToString()) ? false : Convert.ToBoolean(row["TermsCondintionsAccepted"].ToString()),
									AcceptanceDate = string.IsNullOrEmpty(row["AcceptanceDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AcceptanceDate"].ToString()),
									CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
									CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
									Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
									IsPasswordModified = string.IsNullOrEmpty(row["IsPasswordModified"].ToString()) ? false : Convert.ToBoolean(row["IsPasswordModified"].ToString()),
									ImplicitRole = row["ImplicitRole"].ToString(),
									RowGuid = Guid.Parse(row["RowGuid"].ToString()),
									HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? null : (int?)Convert.ToInt32(row["HouseId"].ToString()),
									JobRoleId = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? null : (int?)Convert.ToInt32(row["JobRoleId"].ToString()),
									DateIntoRole = string.IsNullOrEmpty(row["DateIntoRole"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateIntoRole"].ToString()),
									AppearOnLeaderboard = string.IsNullOrEmpty(row["AppearOnLeaderboard"].ToString()) ? false : Convert.ToBoolean(row["AppearOnLeaderboard"].ToString())
								};
							}
						}

						reader.Dispose();

					}
					connection.Close();
					return user;
				}

			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				throw ex;
			}
		}
		public async Task<APIUserMaster> GetByEmailOrUserId(string searchText)
		{
			return await (from user in this._db.UserMaster
						  join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
						  where ((user.UserId == Security.Encrypt(searchText.ToLower()) ||
						  (user.EmailId == Security.Encrypt(searchText.ToLower()))) && user.IsDeleted == false)
						  select new APIUserMaster
						  {
							  AccountCreatedDate = userDetails.AccountCreatedDate,
							  AccountExpiryDate = user.AccountExpiryDate,
							  Currency = userDetails.Currency,
							  CustomerCode = userDetails.CustomerCode,
							  DateOfBirth = userDetails.DateOfBirth,
							  DateOfJoining = userDetails.DateOfJoining,
							  EmailId = Security.Decrypt(user.EmailId),
							  Gender = userDetails.Gender,
							  Id = user.Id,
							  IsActive = user.IsActive,
							  Language = userDetails.Language,
							  LastModifiedDate = userDetails.LastModifiedDate,
							  MobileNumber = Security.Decrypt(userDetails.MobileNumber),
							  Password = user.Password,
							  ProfilePicture = userDetails.ProfilePicture,
							  SerialNumber = userDetails.SerialNumber,
							  TimeZone = userDetails.TimeZone,
							  UserId = Security.Decrypt(user.UserId),
							  UserName = user.UserName,
							  UserRole = user.UserRole,
							  UserType = userDetails.UserType,
							  ConfigurationColumn1Id = userDetails.ConfigurationColumn1,
							  ConfigurationColumn2Id = userDetails.ConfigurationColumn2,
							  ConfigurationColumn3Id = userDetails.ConfigurationColumn3,
							  ConfigurationColumn4Id = userDetails.ConfigurationColumn4,
							  ConfigurationColumn5Id = userDetails.ConfigurationColumn5,
							  ConfigurationColumn6Id = userDetails.ConfigurationColumn6,
							  ConfigurationColumn7Id = userDetails.ConfigurationColumn7,
							  ConfigurationColumn8Id = userDetails.ConfigurationColumn8,
							  ConfigurationColumn9Id = userDetails.ConfigurationColumn9,
							  ConfigurationColumn10Id = userDetails.ConfigurationColumn10,
							  ConfigurationColumn11Id = userDetails.ConfigurationColumn11,
							  ConfigurationColumn12Id = userDetails.ConfigurationColumn12,
							  ReportsTo = Security.Decrypt(userDetails.ReportsTo),
							  AreaId = userDetails.AreaId,
							  BusinessId = userDetails.BusinessId,
							  GroupId = userDetails.GroupId,
							  LocationId = userDetails.LocationId,
							  IsDeleted = user.IsDeleted,
							  IsEnableDegreed = userDetails.Degreed,
							  TermsCondintionsAccepted = userDetails.TermsCondintionsAccepted,
							  AppearOnLeaderboard = userDetails.AppearOnLeaderboard,
							  AcceptanceDate = userDetails.AcceptanceDate,
							  CreatedBy = userDetails.CreatedBy,
							  CreatedDate = userDetails.CreatedDate,
							  Lock = user.Lock,
							  IsPasswordModified = userDetails.IsPasswordModified,
							  ProfilePicturePath = userDetails.ProfilePicture,
						  }).FirstOrDefaultAsync();
		}
		public async Task<UserMasterDetails> GetDetailsByEmailOrUserId(string searchText)
		{
			return await (from user in this._db.UserMaster
						  join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.Id
						  where ((user.UserId == Security.Encrypt(searchText.ToLower()) ||
						  (user.EmailId == Security.Encrypt(searchText.ToLower()))) && user.IsDeleted == false)
						  select userDetails).FirstOrDefaultAsync();
		}

		public async Task<bool> UserAccountLocked(int userId)
		{
			if (await this._db.UserMaster.Where(u => u.Lock == true && u.Id == userId).CountAsync() > 0)
				return true;
			else
				return false;
		}

		public async Task<bool> TermsConditionsAccepted(int userId)
		{
			if (await this._db.UserMasterDetails.Where(u => u.TermsCondintionsAccepted == true && u.UserMasterId == userId).CountAsync() > 0)
				return true;
			else
				return false;
		}

		public async Task<bool> IsPasswordModified(int userId)
		{
			if (await this._db.UserMasterDetails.Where(u => u.IsPasswordModified == true && u.UserMasterId == userId).CountAsync() > 0)
				return true;
			else
				return false;
		}

		//public async Task<int?> LoginReminderDays(int userId, string OrgCode)
		//{
		//	try
		//	{
		//		DateTime? lastLoggedInTime = await GetAccountLoginDate(userId);
		//		DateTime outputDateTimeValue = new DateTime();
		//		DateTime datetimeNow = new DateTime();
		//		DateTime LoggedInDate = new DateTime();

		//		if (lastLoggedInTime != null)
		//		{
		//			if (DateTime.TryParse(lastLoggedInTime.ToString(), out outputDateTimeValue))
		//			{
		//				LoggedInDate = outputDateTimeValue;
		//			}

		//			if (DateTime.TryParse(DateTime.Now.ToShortDateString(), out outputDateTimeValue))
		//			{
		//				datetimeNow = outputDateTimeValue;
		//			}
		//			int loginReminderDays = Convert.ToInt32(await GetConfigurationValueAsync("LOGIN_REMINDER", OrgCode, "30"));
		//			DateTime tempDate;
		//			if (loginReminderDays > 0)
		//				tempDate = LoggedInDate.AddDays(loginReminderDays);
		//			else
		//				tempDate = LoggedInDate.AddDays(30);

		//			int result = (tempDate.Date - datetimeNow.Date).Days;

		//			if (result <= -1)
		//			{
		//				await UpdatePasswordModifiedFlag(userId);
		//			}
		//			return result;
		//		}
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//		return null;
		//	}

		//	return null;
		//}
		//public async Task<int?> LoginPasswordChangeReminderDays(int userId, string OrgCode)
		//{
		//	try
		//	{
		//		DateTime? pwdmodifiedDate = await GetPasswordModifiedDate(userId);
		//		DateTime outputDateTimeValue = new DateTime();
		//		DateTime datetimeNow = new DateTime();
		//		DateTime passwordModifiedDate = new DateTime();

		//		if (pwdmodifiedDate != null)
		//		{
		//			if (DateTime.TryParse(pwdmodifiedDate.ToString(), out outputDateTimeValue))
		//			{
		//				passwordModifiedDate = outputDateTimeValue;
		//			}

		//			if (DateTime.TryParse(DateTime.Now.ToShortDateString(), out outputDateTimeValue))
		//			{
		//				datetimeNow = outputDateTimeValue;
		//			}


		//			int loginPasswordExpiryDays = Convert.ToInt32(await GetConfigurationValueAsync("PASSWORD_EXPIRY", OrgCode));
		//			DateTime modifiedpasswordModifiedDate;
		//			if (loginPasswordExpiryDays > 0)
		//				modifiedpasswordModifiedDate = passwordModifiedDate.AddDays(loginPasswordExpiryDays);
		//			else
		//				modifiedpasswordModifiedDate = passwordModifiedDate.AddDays(90);

		//			int result = (modifiedpasswordModifiedDate.Date - datetimeNow.Date).Days;

		//			if (result <= -1)
		//			{
		//				await UpdatePasswordModifiedFlag(userId);
		//			}

		//			return result;
		//		}
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//		return null;
		//	}

		//	return null;
		//}

		private async Task<DateTime?> GetPasswordModifiedDate(int Id)
		{
			return await this._db.UserMasterDetails.Where(u => u.UserMasterId == Id).Select(u => u.PasswordModifiedDate).SingleOrDefaultAsync();
		}
		//private async Task<DateTime?> GetAccountLoginDate(int userId)
		//{
		//    return await this._db.LoggedInHistory.Where(r => r.UserMasterId == userId).OrderByDescending(r => r.Id).Select(r => r.LocalLoggedInTime).FirstOrDefaultAsync();  //  Select(u => u.PasswordModifiedDate).SingleOrDefaultAsync();
		//}

		private async Task<int> UpdateAccountLocked(int id)
		{
			UserMaster User = this._db.UserMaster.Where(u => u.Id == id).FirstOrDefault();
			if (User == null)
				return 0;

			User.Lock = true;
			this._db.UserMaster.Update(User);

			await this._db.SaveChangesAsync();
			return 1;
		}

		private async Task<int> UpdatePasswordModifiedFlag(int id)
		{
			UserMasterDetails userMasterDetails = this._db.UserMasterDetails.Where(u => u.UserMasterId == id).FirstOrDefault();
			if (userMasterDetails == null)
				return 0;

			userMasterDetails.IsPasswordModified = false;
			this._db.UserMasterDetails.Update(userMasterDetails);

			await this._db.SaveChangesAsync();
			return 1;
		}




		public async Task<string> GetMasterConfigurableParameterValueByConnectionString(string configurationCode, string OrgnizationConnectionString)
		{
			string value = null; //default value

			try
			{

				using (var dbContext = _customerConnectionString.GetDbContext(OrgnizationConnectionString))
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
		public async Task<bool> ExistsForUpdate(string searchOn, string searchText, int userId)
		{
			if (searchOn.ToLower().Equals("email"))
				if (await this._db.UserMaster.Where(u => u.IsDeleted == false && userId != u.Id && (u.EmailId == Security.Encrypt(searchText.ToLower()))).CountAsync() > 0)
					return true;
			if (searchOn.ToLower().Equals("mobile"))
				if (await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && userId != u.UserMasterId && u.MobileNumber == Security.Encrypt(searchText.ToLower())).CountAsync() > 0)
					return true;
			if (searchOn.ToLower().Equals("userid"))
				if (await this._db.UserMaster.Where(u => u.IsDeleted == false && userId != u.Id && (u.UserId == Security.Encrypt(searchText.ToLower()))).CountAsync() > 0)
					return true;
			return false;
		}

		public async Task<bool> IsUniqueFieldExist(string emailId, string mobile, string userId, int id)
		{

			int Count = await (from user in this._db.UserMaster
							   join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
							   where (user.IsActive == true && user.IsDeleted == false && id != user.Id
							   && (((emailId == null) || user.EmailId.ToLower() == Security.Encrypt(emailId.ToLower()))
							   || (user.UserId.ToLower() == Security.Encrypt(userId.ToLower()))
							   || ((mobile == null) || userDetails.MobileNumber == Security.Encrypt(mobile.ToLower()))))
							   select user.Id).CountAsync();
			if (Count > 0)
				return true;
			return false;
		}
		public async Task<bool> IsUniqueFieldExistWithoutMobile(string emailId, string mobile, string userId, int id)
		{

			int Count = await (from user in this._db.UserMaster
							   join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
							   where (user.IsActive == true && user.IsDeleted == false && id != user.Id
							   && (((emailId == null) || user.EmailId.ToLower() == Security.Encrypt(emailId.ToLower()))
							   || (user.UserId.ToLower() == Security.Encrypt(userId.ToLower()))
							   ))
							   select user.Id).CountAsync();
			if (Count > 0)
				return true;
			return false;
		}
		//public async Task<bool> IsUserRoleExist(string UserRole)
		//{
		//	int Count = await (from role in this._db.Roles
		//					   where (role.IsDeleted == 0 && (role.RoleCode.ToLower() == UserRole.ToLower()))
		//					   select role.Id).CountAsync();
		//	if (Count > 0)
		//		return false;
		//	return true;
		//}
		public async Task<bool> IsActiveUserExist(string emailId, string mobile, string userId, int id)
		{
			int Count = await (from user in this._db.UserMaster
							   join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
							   where (user.IsDeleted == false && id != user.Id
							   && ((user.EmailId.ToLower() == Security.Encrypt(emailId.ToLower()))
							   || (user.UserId.ToLower() == Security.Encrypt(userId.ToLower()))
							   || userDetails.MobileNumber == Security.Encrypt(mobile.ToLower())))
							   select user.Id).CountAsync();
			if (Count > 0)
				return true;
			return false;
		}

		public async Task<APIUserMaster> AddUserToDb(APIUserMaster apiUser, string UserRole)
		{
			_logger.Debug("in AddUserToDb");
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
						apiUser.Id = User.Id;
					   
					   if(string.IsNullOrEmpty(UserDetails.ProfilePicture))
						{
							UserDetails.ProfilePicture = RandomProfileImage(UserDetails.Gender);
						} 
						await this._db.UserMasterDetails.AddAsync(UserDetails);
						await this._db.SaveChangesAsync();
						transaction.Commit();
					}
					else
					{
						transaction.Rollback();
					}

					return apiUser;
				}
				catch (System.Exception ex)
				{
					_logger.Error(Utilities.GetDetailedException(ex));
					_logger.Debug("Exception while adding user in database" + Utilities.GetDetailedException(ex));
					transaction.Rollback();
					throw ex;
				}
			}
		}

		public  string RandomProfileImage(string gender)
		{
			string imgPath = string.Empty;
			Random random = new Random();
			int randomImag = random.Next(1, 25);
			if (gender.ToLower() == "female")
			{
				imgPath = "profilePicture/female/f" + randomImag + ".png";
			}
			else if (gender.ToLower() == "male")
			{
				imgPath = "profilePicture/male/m" + randomImag + ".png";
			}
			else
			{
				 randomImag = random.Next(1, 7);
				imgPath = "profilePicture/other/o" + randomImag + ".png";
			}
			return imgPath;
		}
		public async Task<APIUserSignUp> AddUserToDbForSignUp(APIUserSignUp apiUser, string UserRole)
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
						apiUser.Id = User.Id;
						await this._db.UserMasterDetails.AddAsync(UserDetails);
						await this._db.SaveChangesAsync();
						transaction.Commit();
					}
					else
					{
						transaction.Rollback();
					}

					return apiUser;
				}
				catch (System.Exception ex)
				{
					_logger.Error(Utilities.GetDetailedException(ex));
					transaction.Rollback();
					throw ex;
				}
			}
		}
		public async Task<string> UpdateReportTo(string oldemailid, string EmailId)
		{
			try
			{
				UserMasterDetails UserDetails = new UserMasterDetails();
				List<UserMasterDetails> result1 = (from p in this._db.UserMasterDetails
												   where p.ReportsTo == oldemailid || oldemailid == ""
												   select p).ToList();
				foreach (var item in result1)
				{
					if (EmailId != null)
					{
						item.ReportsTo = EmailId;
						this._db.Update(item);
						_db.Entry(item).Property(p => p.AadharNumber).IsModified = false;
						_db.Entry(item).Property(p => p.FHName).IsModified = false;
						_db.Entry(item).Property(p => p.AadhaarPath).IsModified = false;
					}
				}
				await this._db.SaveChangesAsync();
				return result1.ToString();
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return null;
		}
		//public async Task<int> Delete(int id, int userId)
		//{
		//	try
		//	{

		//		UserMaster User = await this.Get(id);

		//		UserMasterDelete userMasterDelete = new UserMasterDelete
		//		{
		//			AccountExpiryDate = User.AccountExpiryDate,
		//			EmailId = User.EmailId,
		//			Password = User.Password,
		//			UserId = User.UserId,
		//			UserRole = User.UserRole,
		//			Lock = User.Lock,
		//			RowGuid = User.RowGuid,
		//			UserName = User.UserName,
		//			IsActive = User.IsActive,
		//			IsDeleted = User.IsDeleted,
		//			UserMaster_Id = User.Id
		//		};
		//		await this._db.UserMasterDelete.AddAsync(userMasterDelete);
		//		await this._db.SaveChangesAsync();

		//		UserMasterDetails userMasterDetails = await this.GetUserDetails(id);
		//		UserMasterDetailsDelete userMasterDetailsDelete = new UserMasterDetailsDelete
		//		{
		//			AcceptanceDate = userMasterDetails.AcceptanceDate,
		//			CustomerCode = userMasterDetails.CustomerCode,
		//			SerialNumber = userMasterDetails.SerialNumber,
		//			MobileNumber = userMasterDetails.MobileNumber,
		//			UserType = userMasterDetails.UserType,
		//			Gender = userMasterDetails.Gender,
		//			TimeZone = userMasterDetails.TimeZone,
		//			Currency = userMasterDetails.Currency,
		//			Language = userMasterDetails.Language,
		//			ProfilePicture = userMasterDetails.ProfilePicture,
		//			ReportsTo = userMasterDetails.ReportsTo,
		//			ConfigurationColumn1 = userMasterDetails.ConfigurationColumn1,
		//			ConfigurationColumn2 = userMasterDetails.ConfigurationColumn2,
		//			ConfigurationColumn3 = userMasterDetails.ConfigurationColumn3,
		//			ConfigurationColumn4 = userMasterDetails.ConfigurationColumn4,
		//			ConfigurationColumn5 = userMasterDetails.ConfigurationColumn5,
		//			ConfigurationColumn6 = userMasterDetails.ConfigurationColumn6,
		//			ConfigurationColumn7 = userMasterDetails.ConfigurationColumn7,
		//			ConfigurationColumn8 = userMasterDetails.ConfigurationColumn8,
		//			ConfigurationColumn9 = userMasterDetails.ConfigurationColumn9,
		//			ConfigurationColumn10 = userMasterDetails.ConfigurationColumn10,
		//			ConfigurationColumn11 = userMasterDetails.ConfigurationColumn11,
		//			ConfigurationColumn12 = userMasterDetails.ConfigurationColumn12,
		//			IsPasswordModified = userMasterDetails.IsPasswordModified,
		//			CreatedBy = userMasterDetails.CreatedBy,
		//			CreatedDate = userMasterDetails.CreatedDate,
		//			ModifiedBy = userId,
		//			AccountCreatedDate = userMasterDetails.AccountCreatedDate,
		//			LastModifiedDate = userMasterDetails.LastModifiedDate,
		//			PasswordModifiedDate = userMasterDetails.PasswordModifiedDate,
		//			DateOfBirth = userMasterDetails.DateOfBirth,
		//			DateOfJoining = userMasterDetails.DateOfJoining,
		//			ModifiedDate = userMasterDetails.ModifiedDate,
		//			LastLoggedInDate = userMasterDetails.LastLoggedInDate,
		//			IsActive = userMasterDetails.IsActive,
		//			IsDeleted = userMasterDetails.IsDeleted,
		//			TermsCondintionsAccepted = userMasterDetails.TermsCondintionsAccepted,
		//			Degreed = userMasterDetails.Degreed
		//		};
		//		userMasterDetailsDelete.IsDeleted = userMasterDetails.IsDeleted;
		//		userMasterDetailsDelete.AppearOnLeaderboard = userMasterDetails.AppearOnLeaderboard;
		//		userMasterDetailsDelete.HouseId = userMasterDetails.HouseId;
		//		userMasterDetailsDelete.BusinessId = userMasterDetails.BusinessId;
		//		userMasterDetailsDelete.GroupId = userMasterDetails.GroupId;
		//		userMasterDetailsDelete.AreaId = userMasterDetails.AreaId;
		//		userMasterDetailsDelete.LocationId = userMasterDetails.LocationId;
		//		userMasterDetailsDelete.UserMasterId = userMasterDetails.UserMasterId;
		//		userMasterDetailsDelete.UserMasterDetail_Id = userMasterDetails.Id;
		//		await this._db.UserMasterDetailsDelete.AddAsync(userMasterDetailsDelete);
		//		await this._db.SaveChangesAsync();

		//		if (User == null)
		//			return -1;
		//		if (User.IsActive == true)
		//			return -1;
		//		User.IsDeleted = true;
		//		userMasterDetails.IsDeleted = true;


		//		this._db.UserMaster.Remove(User);
		//		this._db.UserMasterDetails.Remove(userMasterDetails);
		//		await this._db.SaveChangesAsync();

		//		//if deleted user's email exists in Report To column then remove such reference
		//		await this.UpdateReportsToEmail(User.EmailId);

		//		return 1;
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//		return 0;
		//	}
		//}

		private async Task<int> UpdateReportsToEmail(string emailId)
		{
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "UpdateReportsToEmail";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@EmailId", SqlDbType.VarChar) { Value = emailId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						reader.Dispose();
					}
					connection.Close();
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return 1;
		}

		public async Task<int> UpdateUserDetails(UserMasterDetails userDetails)
		{
			this._db.UserMasterDetails.Update(userDetails);
			await this._db.SaveChangesAsync();
			return 1;
		}

		public async Task<int> UpdateProfilePicture(string fileName, int UserId)
		{
			UserMasterDetails obj = new UserMasterDetails();
			obj = this._db.UserMasterDetails.Where(a => a.UserMasterId == UserId && a.IsActive == true).FirstOrDefault();
			obj.UserMasterId = UserId;
			obj.ProfilePicture = fileName;
			this._db.UserMasterDetails.Update(obj);
			await this._db.SaveChangesAsync();
			return 1;
		}

		//public async Task<int> UpdateUserPasswordHistory(string userid)
		//{
		//	try
		//	{

		//		FailedLoginStatistics failedLoginstat = await this._db.FailedLoginStatistics.Where(f => f.UserId == userid).FirstOrDefaultAsync();
		//		failedLoginstat.Counter = 0;
		//		failedLoginstat.CreatedDate = DateTime.UtcNow;
		//		this._db.FailedLoginStatistics.Update(failedLoginstat);
		//		await this._db.SaveChangesAsync();
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));
		//	}
		//	return 1;
		//}
		public async Task<int> UpdateUser(APIUserMaster apiUser)
		{
			UserMaster User = Mapper.Map<UserMaster>(apiUser);
			this._db.UserMaster.Update(User);
			await this._db.SaveChangesAsync();
			UserMasterDetails UserDetails = await GetUserDetails(User.Id);
			int Id = UserDetails.Id;
			UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
			UserDetails.Id = Id;
			UserDetails.UserMasterId = User.Id;
			UserDetails.ModifiedBy = apiUser.Id;
			UserDetails.Degreed = apiUser.IsEnableDegreed;
			UserDetails.ModifiedDate = DateTime.UtcNow;
			UserDetails.IsActive = User.IsActive;
			UserDetails.IsDeleted = User.IsDeleted;
			UserDetails.JobRoleId = apiUser.JobRoleId;
			UserDetails.DateIntoRole = apiUser.DateIntoRole;
		   
			this._db.UserMasterDetails.Update(UserDetails);
			_db.Entry(UserDetails).Property(p => p.AadharNumber).IsModified = false;
			_db.Entry(UserDetails).Property(p => p.FHName).IsModified = false;
			_db.Entry(UserDetails).Property(p => p.AadhaarPath).IsModified = false;
			await this._db.SaveChangesAsync();
			return 1;
		}


		//public async Task<int> UserPatch(APIUpdateUserMobileEmail apiUser, string OrgCode, int userid)
		//{
		//    UserMasterDetails UserDetails = await GetUserDetails(userid);
		//    UserMaster UserMaster = await Get(userid);

		//    if (UserDetails == null || UserMaster == null)
		//    {
		//        return 0;
		//    }
		//    else
		//    {

		//        UserDetails.MobileNumber = Security.Encrypt(apiUser.MobileNumber.ToLower());
		//        UserMaster.EmailId = Security.Encrypt(apiUser.EmailId.ToLower());
		//        UserDetails.ModifiedDate = DateTime.UtcNow;
		//        UserDetails.ModifiedBy = userid;
		//        this._db.UserMaster.Update(UserMaster);
		//        this._db.UserMasterDetails.Update(UserDetails);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//}

		public async Task<APIUserMaster> AddUpdateUserImport(APIUserMaster apiUser)
		{
			try
			{
				UserMaster User = Mapper.Map<UserMaster>(apiUser);
				this._db.UserMaster.Update(User);
				await this._db.SaveChangesAsync();
				UserMasterDetails UserDetails = await GetUserDetails(User.Id);
				int Id = UserDetails.Id;
				UserDetails = Mapper.Map<UserMasterDetails>(apiUser);
				UserDetails.Id = Id;
				UserDetails.UserMasterId = User.Id;
				UserDetails.ModifiedBy = apiUser.Id;
				UserDetails.Degreed = apiUser.IsEnableDegreed;
				UserDetails.ModifiedDate = DateTime.UtcNow;
				UserDetails.IsActive = User.IsActive;
				UserDetails.IsDeleted = User.IsDeleted;
				this._db.UserMasterDetails.Update(UserDetails);
				await this._db.SaveChangesAsync();
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return apiUser;
		}
		public void CloseConnection()
		{
			var connection = this._db.Database.GetDbConnection();
			if (connection.State == ConnectionState.Open)
				connection.Close();
		}

		//public async Task<APISectionAdminDetails> GetSectionAdminDetails(int userId)
		//{
		//    APISectionAdminDetails apiSectionAdminDetails = new APISectionAdminDetails();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetSectionAdminDetails";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        apiSectionAdminDetails.Location = string.IsNullOrEmpty(row["location"].ToString()) ? null : row["location"].ToString();
		//                        apiSectionAdminDetails.Business = string.IsNullOrEmpty(row["business"].ToString()) ? null : row["business"].ToString();
		//                        apiSectionAdminDetails.Group = string.IsNullOrEmpty(row["group"].ToString()) ? null : row["group"].ToString();
		//                        apiSectionAdminDetails.Area = string.IsNullOrEmpty(row["area"].ToString()) ? null : row["area"].ToString();
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//    return apiSectionAdminDetails;
		//}

		public async Task<bool> UserDataExist(string column)
		{
			int TotalUser = await this._db.UserMaster.Where(u => u.IsDeleted == false && u.IsActive == true).Select(u => u.Id).CountAsync();
			int ColumnCount = 0;
			if (column.ToLower().Equals("business"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.BusinessId != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("group"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.GroupId != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("area"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.AreaId != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("location"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.LocationId != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn1"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn1 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn2"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn2 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn3"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn3 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn4"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn4 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn5"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn5 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn6"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn6 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn7"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn7 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn8"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn8 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn9"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn9 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn10"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn10 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn11"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn11 != null).Select(u => u.Id).CountAsync();
			if (column.ToLower().Equals("configurationcolumn12"))
				ColumnCount = await this._db.UserMasterDetails.Where(u => u.IsDeleted == false && u.IsActive == true && u.ConfigurationColumn12 != null).Select(u => u.Id).CountAsync();

			if (TotalUser == ColumnCount)
				return true;
			return false;
		}
		public void ChangeDbContext(string connectionString)
		{
			this._db = DbContextFactory.Create(connectionString);
			this._context = this._db;
			this._entities = this._context.Set<UserMaster>();
		}
		//public async Task<int> AddUserHistory(APIUserMaster Olduser, APIUserMaster user)
		//{
		//	try
		//	{
		//		string OldUser = JsonConvert.SerializeObject(Olduser);
		//		string NewUser = JsonConvert.SerializeObject(user);
		//		JObject oJsonObject = new JObject
		//	{
		//		{ "oldUser", OldUser },
		//		{ "newUser", NewUser },
		//		{ "userId", Olduser.Id }
		//	};
		//		string Url = this._configuration[Configuration.AuditApi];
		//		Url = Url + "Audit/AddUserDetails";
		//		string token = this._identitySv.GetToken();
		//		HttpResponseMessage response = await Api.CallPostAPI(Url, oJsonObject, token);
		//		return 1;
		//	}
		//	catch (System.Exception ex)
		//	{
		//		_logger.Error(Utilities.GetDetailedException(ex));

		//	}
		//	return 1;
		//}

		public async Task<string> GetUserRole(int userId)
		{
			return await this._db.UserMaster
				 .Where(u => u.IsDeleted == false && u.Id == userId)
				 .Select(u => u.UserRole)
				 .FirstOrDefaultAsync();
		}

		//public async Task<APIUserConfiguration> GetUserConfiguration(string userid, int id, string OrgCode)
		//{
		//    APIUserConfiguration userConfiguration = new APIUserConfiguration();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            using (var connection = dbContext.Database.GetDbConnection())
		//            {
		//                connection.Open();
		//                using (var cmd = connection.CreateCommand())
		//                {
		//                    cmd.CommandText = "GetUserConfiguration";
		//                    cmd.CommandType = CommandType.StoredProcedure;
		//                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.VarChar) { Value = Security.Encrypt(userid.ToLower()) });
		//                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
		//                    cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = userid.ToLower() });
		//                    cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode.ToLower() });

		//                    DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                    DataTable dt = new DataTable();
		//                    dt.Load(reader);
		//                    if (dt.Rows.Count > 0)
		//                    {
		//                        userConfiguration = new APIUserConfiguration
		//                        {
		//                            Id = Convert.ToInt32(dt.Rows[0]["Id"].ToString()),
		//                            CustomerCode = dt.Rows[0]["CustomerCode"].ToString(),
		//                            EmailId = Security.EncryptForUI(Security.Decrypt(dt.Rows[0]["EmailId"].ToString())),
		//                            UserName = dt.Rows[0]["UserName"].ToString(),
		//                            UserRole = Security.EncryptForUI(dt.Rows[0]["UserRole"].ToString()),
		//                            UserType = dt.Rows[0]["UserType"].ToString(),
		//                            IsPasswordModified = Convert.ToBoolean(dt.Rows[0]["IsPasswordModified"].ToString()),
		//                            LanguageCode = dt.Rows[0]["LanguageCode"].ToString(),
		//                            ProfilePicture = dt.Rows[0]["ProfilePicture"].ToString(),
		//                            LandingPage = dt.Rows[0]["LandingPage"].ToString(),
		//                            ProductName = dt.Rows[0]["ProductName"].ToString(),
		//                            ImplicitRole = Security.EncryptForUI(dt.Rows[0]["ImplicitRole"].ToString()),
		//                            LastLoggedInTime = dt.Rows[0]["LastLoggedInTime"].ToString(),
		//                            Gender = dt.Rows[0]["Gender"].ToString(),
		//                            DisplayRoleName = dt.Rows[0]["DisplayRoleName"].ToString(),

		//                            Region = dt.Rows[0]["Region"].ToString(),
		//                            LocationName = dt.Rows[0]["LocationName"].ToString(),
		//                            IsShowTODODashboard = Convert.ToBoolean(dt.Rows[0]["ShowTODODashboard"].ToString()),
		//                            IsShowHourseInfoOnLeaderboard = Convert.ToBoolean(dt.Rows[0]["ShowHourseInfoOnLeaderboard"].ToString()),
		//                            ApplicationDateFormat = dt.Rows[0]["ApplicationDateFormat"].ToString(),
		//                            IsSupervisor = Convert.ToBoolean(dt.Rows[0]["IsSupervisor"].ToString()),
		//                            EmpCode = Security.EncryptForUI(Security.Decrypt(dt.Rows[0]["EmpCode"].ToString())),
		//                            country = dt.Rows[0]["country"].ToString()
		//                        };
		//                    }
		//                    reader.Dispose();
		//                    connection.Close();
		//                }
		//            }
		//        }
		//        return userConfiguration;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<APIUserDashboardConfiguration> GetUserDashboardConfiguration(string userid, int id, string OrgCode)
		//{
		//    APIUserDashboardConfiguration userDashboardConfiguration = new APIUserDashboardConfiguration();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            using (var connection = dbContext.Database.GetDbConnection())
		//            {
		//                connection.Open();
		//                using (var cmd = connection.CreateCommand())
		//                {
		//                    cmd.CommandText = "GetUserDashboardConfiguration";
		//                    cmd.CommandType = CommandType.StoredProcedure;
		//                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.VarChar) { Value = Security.Encrypt(userid.ToLower()) });
		//                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
		//                    cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode.ToLower() });

		//                    DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                    DataTable dt = new DataTable();
		//                    dt.Load(reader);

		//                    if (dt.Rows.Count > 0)
		//                    {
		//                        userDashboardConfiguration = new APIUserDashboardConfiguration
		//                        {
		//                            IsAchieveMastery = Convert.ToBoolean(dt.Rows[0]["AchieveMastery"].ToString()),
		//                            AchieveMasteryCourseID = Convert.ToInt32(dt.Rows[0]["AchieveMasteryCourseID"].ToString()),
		//                            AchieveMasteryCourseTitle = dt.Rows[0]["AchieveMasteryCourseTitle"].ToString(),
		//                            TimeSpent = dt.Rows[0]["TimeSpent"].ToString(),
		//                            IsShowCEOMessageAfterLogin = Convert.ToBoolean(dt.Rows[0]["ShowCEOMessageAfterLogin"].ToString()),
		//                            IsShowCEOMessageOnLandingPage = Convert.ToBoolean(dt.Rows[0]["ShowCEOMessage"].ToString()),
		//                            CEOMessageHeading = dt.Rows[0]["CEOMessageHeading"].ToString(),
		//                            CEOMessageDescription = dt.Rows[0]["CEOMessageDescription"].ToString(),
		//                            CEOProfilePicture = dt.Rows[0]["CEOProfilePicture"].ToString(),
		//                            IsShowThoughtMessage = Convert.ToBoolean(dt.Rows[0]["ShowThoughtMessage"].ToString()),
		//                            ThoughtMessage = dt.Rows[0]["ThoughtMessage"].ToString(),
		//                        };
		//                    }
		//                    reader.Dispose();
		//                    connection.Close();
		//                }
		//            }
		//        }
		//        return userDashboardConfiguration;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		public async Task<string> GetEmail(int Id)
		{
			return await this._db.UserMaster.Where(u => u.Id == Id).Select(u => u.EmailId).SingleOrDefaultAsync();
		}

		public async Task<string> GetReportToEmail(int Id)
		{
			return await this._db.UserMasterDetails.Where(u => u.UserMasterId == Id).Select(u => u.ReportsTo).SingleOrDefaultAsync();
		}
		public async Task<string> GetEmailUserExists(string userId)
		{
			return await this._db.UserMaster.Where(u => u.UserId == userId && u.IsDeleted == false).Select(u => u.EmailId).FirstOrDefaultAsync();
		}
		public async Task<bool> EmailUserExists(string emailId, string userId)
		{
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false && u.UserId == Security.Encrypt(userId.ToLower())).CountAsync(x => x.EmailId == Security.Encrypt(emailId.ToLower())) > 0)
				return true;
			return false;

		}
		public async Task<bool> EmailExists(string emailId)
		{
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == Security.Encrypt(emailId.ToLower())) > 0)
				return true;
			return false;

		}

		public async Task<bool> EmailExists1(string emailId, string OrgCode)
		{
			this.ChangeDbContext(OrgCode);
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.EmailId == Security.Encrypt(emailId.ToLower())) > 0)
				return true;
			return false;

		}
		public async Task<bool> UserIdExists(string userId)
		{
			if (await this._db.UserMaster.Where(u => u.IsDeleted == false).CountAsync(x => x.UserId == Security.Encrypt(userId.ToLower())) > 0)
				return true;
			return false;

		}

		public async Task<bool> MobileuserExists(string mobileNumber, string userId)
		{
			int count1 =
				   await (from user in this._db.UserMaster
						  join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
						  where (user.IsDeleted == false && user.UserId == userId && userDetails.MobileNumber == Security.Encrypt(mobileNumber))
						  select userDetails.MobileNumber).CountAsync();
			if (count1 > 0)
				return true;
			return false;
		}

		public async Task<bool> EmailUserIdExists(string EmailID, string userId)
		{
			int count1 =
				   await (from user in this._db.UserMaster
						  where (user.IsDeleted == false && user.UserId == userId && user.EmailId == Security.Encrypt(EmailID.ToLower())
						 )
						  select user.EmailId).CountAsync();
			if (count1 > 0)
				return true;
			return false;
		}

		//public async Task<int> AddUserHRAssociation(APIUserHRAssociation aPIUserHRAssociation, int UserId)
		//{
		//    if (await this.IsExistsUserHR(aPIUserHRAssociation.UserMasterId))
		//    {
		//        return 0;
		//    }
		//    else
		//    {
		//        UserHRAssociation userHRAssociation = new UserHRAssociation
		//        {
		//            Id = 0,
		//            UserMasterId = aPIUserHRAssociation.UserMasterId,
		//            UserName = aPIUserHRAssociation.UserName,
		//            Level = aPIUserHRAssociation.Level,
		//            ModifiedDate = DateTime.UtcNow,
		//            CreatedDate = DateTime.UtcNow,
		//            CreatedBy = UserId,
		//            ModifiedBy = UserId,
		//            IsDeleted = 0
		//        };
		//        await this._db.UserHRAssociation.AddAsync(userHRAssociation);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//}
		//public async Task<IEnumerable<UserHRAssociation>> GetHRAssociationData(int page, int pageSize)
		//{
		//    IQueryable<UserHRAssociation> Query = this._db.UserHRAssociation.Where(r => r.IsDeleted == Record.NotDeleted);

		//    Query = Query.OrderByDescending(v => v.Id);
		//    if (page != -1)
		//    {
		//        Query = Query.Skip((page - 1) * pageSize);
		//        Query = Query.OrderByDescending(v => v.Id);
		//    }
		//    if (pageSize != -1)
		//    {
		//        Query = Query.Take(pageSize);
		//        Query = Query.OrderByDescending(v => v.Id);
		//    }
		//    return await Query.ToListAsync();
		//}

		//public async Task<int> GetHRCount()
		//{
		//    return await this._db.UserHRAssociation.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();

		//}

		//public async Task<UserHRAssociation> GetUserHRA(int id)
		//{
		//    var UserHRAId = (from t in this._db.UserHRAssociation
		//                     where t.Id == id && t.IsDeleted == 0
		//                     select t);
		//    return await UserHRAId.FirstOrDefaultAsync();
		//}

		//public async Task<UserHRAssociation> GetUserHR(int id)
		//{
		//    var UserHRAId = (from t in this._db.UserHRAssociation
		//                     where t.Id == id && t.IsDeleted == 0
		//                     select t);
		//    return await UserHRAId.FirstOrDefaultAsync();
		//}

		//public async Task<HouseMaster> GetHouseMaster(int id)
		//{
		//    var HouseMasterId = (from t in this._db.HouseMaster
		//                         where t.Id == id && t.IsDeleted == 0
		//                         select t);
		//    return await HouseMasterId.FirstOrDefaultAsync();
		//}
		//public async Task<int> UpdateUserHRA(UserHRAssociation userHRAssociation)
		//{
		//    this._db.UserHRAssociation.Update(userHRAssociation);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

		//public async Task<int> UpdateUserHR(UserHRAssociation userHRAssociation)
		//{
		//    this._db.UserHRAssociation.Update(userHRAssociation);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

		//public async Task<int> UpdateHouseMaster(HouseMaster housemaster)
		//{
		//    this._db.HouseMaster.Update(housemaster);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

		//public async Task<int> HRADelete(int id)
		//{
		//    UserHRAssociation association = this._db.UserHRAssociation.Where(u => u.IsDeleted == 0 && u.Id == id).FirstOrDefault();
		//    if (association == null)
		//        return 0;
		//    association.IsDeleted = 1;
		//    this._db.UserHRAssociation.Update(association);
		//    await this._db.SaveChangesAsync();
		//    return 1;

		//}


		//public async Task<IEnumerable<APIDynamicColumnDetails>> GetDynamicColumnDetails(string columnName)
		//{
		//    List<APIDynamicColumnDetails> dynamicColumnDetails = new List<APIDynamicColumnDetails>();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetDynamicColumnDetails";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.VarChar) { Value = columnName });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {

		//                        var detail = new APIDynamicColumnDetails
		//                        {
		//                            ConfiguredColumnName = row["ConfiguredColumnName"].ToString(),
		//                            Id = Convert.ToInt32(row["Id"].ToString()),
		//                            Name = row["Name"].ToString(),

		//                        };
		//                        dynamicColumnDetails.Add(detail);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return dynamicColumnDetails;
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		public async Task<string> GetDecryptUserId(int id)
		{
			return await this._db.UserMaster.Where(u => u.Id == id).Select(u => Security.Decrypt(u.UserId)).SingleOrDefaultAsync();
		}

		public async Task<bool> IsLDAP()
		{
			bool _Title = false;
			string Url = this._configuration[Configuration.IdentityUrl];
			Url = Url + "/GetIsLDAP";
			string token = this._identitySv.GetToken();
			HttpResponseMessage response = await Api.CallGetAPI(Url, token);
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsStringAsync();
				_Title = JsonConvert.DeserializeObject<bool>(result);

			}
			return _Title;

		}
		//public async Task<List<TypeHeadDto>> GetUsersForTA(int userId, string search = null)
		//{
		//    try
		//    {
		//        List<TypeHeadDto> typeAheadList = new List<TypeHeadDto>();
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetUsersForTA";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        TypeHeadDto obj = new TypeHeadDto
		//                        {
		//                            Id = Convert.ToInt32(row["Id"].ToString()),
		//                            Name = row["UserName"].ToString()
		//                        };
		//                        typeAheadList.Add(obj);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return typeAheadList;
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<int> AddUserAdminTrainingAssociation(APIUserTrainingAdminAssociation aPIUserTrainingAdminAssociation, int UserId)
		//{
		//    UserTrainingAdminAssociation trainingAdminAssociation = new UserTrainingAdminAssociation();
		//    if (await this.IsExistsTrainingAdmin(aPIUserTrainingAdminAssociation.DepartmentId, aPIUserTrainingAdminAssociation.UserMasterId))
		//    {
		//        return 0;
		//    }
		//    else
		//    {
		//        trainingAdminAssociation.Id = 0;
		//        trainingAdminAssociation.DepartmentId = aPIUserTrainingAdminAssociation.DepartmentId;
		//        trainingAdminAssociation.DepartmentName = aPIUserTrainingAdminAssociation.DepartmentName;
		//        trainingAdminAssociation.UserMasterId = aPIUserTrainingAdminAssociation.UserMasterId;
		//        trainingAdminAssociation.UserName = aPIUserTrainingAdminAssociation.UserName;
		//        trainingAdminAssociation.ModifiedDate = DateTime.UtcNow;
		//        trainingAdminAssociation.CreatedDate = DateTime.UtcNow;
		//        trainingAdminAssociation.CreatedBy = UserId;
		//        trainingAdminAssociation.ModifiedBy = UserId;
		//        trainingAdminAssociation.IsDeleted = 0;
		//        await this._db.UserTrainingAdminAssociation.AddAsync(trainingAdminAssociation);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//}
		//public async Task<IEnumerable<UserTrainingAdminAssociation>> GetTrainingAdminAssociationData(int page, int pageSize)
		//{
		//    IQueryable<UserTrainingAdminAssociation> Query = this._db.UserTrainingAdminAssociation.Where(r => r.IsDeleted == Record.NotDeleted);

		//    Query = Query.OrderByDescending(v => v.Id);
		//    if (page != -1)
		//    {
		//        Query = Query.Skip((page - 1) * pageSize);
		//        Query = Query.OrderByDescending(v => v.Id);
		//    }
		//    if (pageSize != -1)
		//    {
		//        Query = Query.Take(pageSize);
		//        Query = Query.OrderByDescending(v => v.Id);
		//    }
		//    return await Query.ToListAsync();
		//}
		//public async Task<int> GetTrainingAdminCount()
		//{
		//    return await this._db.UserTrainingAdminAssociation.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
		//}

		//public async Task<int> TrainingAdminDelete(int id)
		//{
		//    UserTrainingAdminAssociation trainingAdminAssociation = this._db.UserTrainingAdminAssociation.Where(u => u.IsDeleted == 0 && u.Id == id).FirstOrDefault();
		//    if (trainingAdminAssociation == null)
		//        return 0;
		//    trainingAdminAssociation.IsDeleted = 1;
		//    this._db.UserTrainingAdminAssociation.Update(trainingAdminAssociation);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}
		//public async Task<List<TypeHeadDto>> GetUsersOfDept(int deptId, string search = null)
		//{
		//    return await (from userMaster in this._db.UserMaster
		//                  join userMasterDetails in this._db.UserMasterDetails on userMaster.Id equals userMasterDetails.UserMasterId
		//                  where userMaster.IsActive == true && userMaster.IsDeleted == false && userMasterDetails.GroupId == deptId
		//                  && (search == null || userMaster.UserName.StartsWith(search))
		//                  select new TypeHeadDto
		//                  {
		//                      Name = userMaster.UserName,
		//                      Id = userMaster.Id
		//                  }).ToListAsync();
		//}

		//public async Task<bool> IsExistsTrainingAdmin(int DepartmentId, int UserMasterId)
		//{
		//    if (await this._db.UserTrainingAdminAssociation.Where(u => u.IsDeleted == 0 && u.DepartmentId == DepartmentId && u.UserMasterId == UserMasterId).CountAsync() > 0)
		//        return true;
		//    return false;
		//}

		//public async Task<bool> IsExistsUserHR(int UserMasterId)
		//{
		//    if (await this._db.UserHRAssociation.Where(u => u.IsDeleted == 0 && u.UserMasterId == UserMasterId).CountAsync() > 0)
		//        return true;
		//    return false;
		//}

		//public async Task<int> AddOrganizationPrefernces(APIOrganizationPreferences aPIOrganizationPreferences, int UserID, string OrgCode)
		//{

		//    var request = _httpContextAccessor.HttpContext.Request;
		//    if (request.Form.Files.Count > 0)
		//    {
		//        foreach (IFormFile uploadedFile in request.Form.Files)
		//        {
		//            OrganizationPreferences organization = new OrganizationPreferences
		//            {
		//                LandingPage = aPIOrganizationPreferences.LandingPage,
		//                Language = aPIOrganizationPreferences.Language,
		//                ColorCode = aPIOrganizationPreferences.ColorCode,

		//                CreatedBy = UserID,
		//                CreatedDate = DateTime.UtcNow,
		//                ModifiedDate = DateTime.UtcNow,
		//                ModifiedBy = UserID,
		//                IsDeleted = 0
		//            };
		//            string logoPath = this._configuration["ApiGatewayWwwroot"];
		//            logoPath = Path.Combine(logoPath, OrgCode, Record.Logo);
		//            if (!Directory.Exists(logoPath))
		//            {
		//                Directory.CreateDirectory(logoPath);
		//            }
		//            string fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
		//            fileName = string.Concat(fileName.Split(' '));
		//            logoPath = Path.Combine(logoPath, fileName);
		//            try
		//            {
		//                using (var fs = new FileStream(Path.Combine(logoPath), FileMode.Create))
		//                {
		//                    await uploadedFile.CopyToAsync(fs);
		//                }
		//            }
		//            catch (System.Exception ex)
		//            {
		//                _logger.Error(Utilities.GetDetailedException(ex));
		//            }
		//            var uri = new System.Uri(logoPath);
		//            logoPath = uri.AbsoluteUri;
		//            string DomainName = this._configuration["ApiGatewayUrl"];
		//            string FPath = new System.Uri(logoPath).AbsoluteUri;
		//            organization.LogoPath = string.Concat(DomainName, OrgCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Logo)));
		//            await this._db.OrganizationPreferences.AddAsync(organization);
		//            await this._db.SaveChangesAsync();

		//        }
		//        return 1;
		//    }
		//    return 0;
		//}
		//public async Task<int> UpdateOrganizationPrefernces(int Id, APIOrganizationPreferences aPIOrganizationPreferences, int UserID, string OrgCode)
		//{

		//    var request = _httpContextAccessor.HttpContext.Request;
		//    OrganizationPreferences organization = await this._db.OrganizationPreferences.Where(o => o.Id == Id).FirstOrDefaultAsync();
		//    if (organization == null)
		//        return 0;

		//    organization.LandingPage = aPIOrganizationPreferences.LandingPage;
		//    organization.Language = aPIOrganizationPreferences.Language;
		//    organization.ColorCode = aPIOrganizationPreferences.ColorCode;
		//    organization.ModifiedDate = DateTime.UtcNow;
		//    organization.ModifiedBy = UserID;
		//    if (request.Form.Files.Count > 0 && aPIOrganizationPreferences.IsLogoUpdated == true)
		//    {
		//        foreach (IFormFile uploadedFile in request.Form.Files)
		//        {
		//            string logoPath = this._configuration["ApiGatewayWwwroot"];
		//            logoPath = Path.Combine(logoPath, OrgCode, Record.Logo);
		//            if (!Directory.Exists(logoPath))
		//            {
		//                Directory.CreateDirectory(logoPath);
		//            }
		//            string fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
		//            fileName = string.Concat(fileName.Split(' '));
		//            logoPath = Path.Combine(logoPath, fileName);
		//            try
		//            {
		//                using (var fs = new FileStream(Path.Combine(logoPath), FileMode.Create))
		//                {
		//                    await uploadedFile.CopyToAsync(fs);
		//                }
		//            }
		//            catch (System.Exception ex)
		//            {
		//                _logger.Error(Utilities.GetDetailedException(ex));
		//            }
		//            var uri = new System.Uri(logoPath);
		//            logoPath = uri.AbsoluteUri;
		//            string DomainName = this._configuration["ApiGatewayUrl"];
		//            string FPath = new System.Uri(logoPath).AbsoluteUri;
		//            organization.LogoPath = string.Concat(DomainName, OrgCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Logo)));

		//        }
		//    }
		//    this._db.OrganizationPreferences.Update(organization);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}
		//public async Task<OrganizationPreferences> GetOrganizationLogo()
		//{
		//    var OrganizationId = (from t in this._db.OrganizationPreferences
		//                          where t.IsDeleted == 0
		//                          select t);
		//    if (OrganizationId == null)
		//    {
		//        return null;
		//    }
		//    return await OrganizationId.FirstOrDefaultAsync();
		//}

		//public async Task<List<GetAllImagesPath>> GetAllImages()
		//{

		//    IQueryable<GetAllImagesPath> result = (from t in this._db.OrganizationPreferences

		//                                           where (t.IsDeleted == Record.NotDeleted)
		//                                           select new GetAllImagesPath
		//                                           {
		//                                               ImagePath = t.LogoPath

		//                                           });
		//    return await result.ToListAsync();


		//}

		//public async Task<int> ResetFailedLoginStatistics(string userId)
		//{
		//    FailedLoginStatistics failedLoginStatistics = new FailedLoginStatistics();
		//    failedLoginStatistics = this._db.FailedLoginStatistics.Where(a => a.UserId == userId).FirstOrDefault();
		//    if (failedLoginStatistics == null)
		//        return 0;
		//    failedLoginStatistics.Counter = 0;
		//    this._db.FailedLoginStatistics.Update(failedLoginStatistics);
		//    await this._db.SaveChangesAsync();
		//    return 1;

		//}

		//public async Task<List<LoggedInHistory>> GetLoggedInHistory(int? UserID, int page, int pageSize)
		//{
		//    IQueryable<LoggedInHistory> Query = (from LoggedInHistory in this._db.LoggedInHistory
		//                                         where LoggedInHistory.UserMasterId == UserID
		//                                         select LoggedInHistory);
		//    Query = Query.Select(a => new LoggedInHistory
		//    {
		//        Id = a.Id,
		//        UserMasterId = a.UserMasterId,
		//        LocalLoggedInTime = a.LocalLoggedInTime,
		//        LocalLoggedOutTime = a.LocalLoggedOutTime,
		//        TotalTimeInMinutes = a.TotalTimeInMinutes
		//    }).OrderByDescending(a => a.Id);

		//    if (page != -1)
		//        Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
		//    if (pageSize != -1)
		//        Query = Query.Take(Convert.ToInt32(pageSize));

		//    return await Query.ToListAsync();
		//}
		//public async Task<int> LoggedInHistoryCount(int? UserID)
		//{
		//    var Query = (from LoggedInHistory in this._db.LoggedInHistory
		//                 where LoggedInHistory.UserMasterId == UserID
		//                 select LoggedInHistory).OrderByDescending(a => a.Id);

		//    return await Query.CountAsync();
		//}
		public async Task<UserMasterOtp> GetOTP(int UserID)
		{
			var Query = (from userMasterOTP in this._db.UserMasterOtp
						 where userMasterOTP.UserMasterId == UserID
						 select userMasterOTP).OrderByDescending(a => a.Id).Take(1);

			return await Query.FirstAsync();
		}

		public async Task<string> GetUserType(string userId)
		{
			string userType = null;
			UserMaster objUserMaster = new UserMaster();
			objUserMaster = await this._db.UserMaster.Where(a => a.UserId == Security.Encrypt(userId.ToLower()) && a.IsActive == true).FirstOrDefaultAsync();
			if (objUserMaster != null)
				userType = "EMPLOYEE";
			return userType;
		}

		public async Task<Boolean> checkuserexistanceinldap(string uid)
		{
			try
			{
				//Create object of the Binding

				System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
				binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;


				//Create endpointAddress of the Service
				System.ServiceModel.EndpointAddress endpointAddress = new EndpointAddress("https://securelogin.sbilife.co.in/acas/LoginService?wsdl");

				LoginServiceClient objAgentAuth = new LoginServiceClient(binding, endpointAddress);

				checkUserExists chku = new checkUserExists
				{
					username = uid
				};

				var chkur = await objAgentAuth.checkUserExistsAsync(chku);


				checkUserExistsResponse chkur1 = chkur.checkUserExistsResponse;
				return chkur1.@return;
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return false;
		}

		public async Task<String> resetPasswordWeb(string uid, string dob, string npass, string cpass)
		{
			try
			{
				String st = null;
				Task<DataTable> ud = null;
				ud = getUserDetails(uid);
				DataTable dt = ud.Result;

				int status = getLDAPStatus(uid);
				if (status == 1)
				{
					foreach (DataRow dr in dt.Rows)
					{
						if (dr["Field"].ToString().Contains("acasdob"))
						{
							string ldob = (dr["Value"].ToString()).Trim();

							ldob = ldob.Substring(0, 8);
							int b = ldob.CompareTo(dob);
							if (b == 0)
							{

								if (npass.Equals(cpass.ToString()))
								{

									System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
									binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;

									System.ServiceModel.EndpointAddress endpointAddress = new EndpointAddress("https://securelogin.sbilife.co.in/acas/LoginService?wsdl");
									LoginServiceClient objAgentAuth = new LoginServiceClient(binding, endpointAddress);
									resetPasswordweb rpw = new resetPasswordweb
									{
										username = uid,
										password = npass
									};
									var rpwr = await objAgentAuth.resetPasswordwebAsync(rpw);
									resetPasswordwebResponse rpwr1 = rpwr.resetPasswordwebResponse;
									st = rpwr1.@return;
									return st;
								}
								else
								{
									st = "New Password and Confirm password does not match";
									return st;
								}
							}
							else
							{
								st = "Incorrect details entered";
								return st;
							}
						}
					}
				}
				else
				{
					st = "Your Status is not Valid";
					return st;
				}

				return st;
			}

			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				return null;

			}
		}

		public int getLDAPStatus(string u)
		{
			Task<DataTable> ud = null;
			try
			{
				ud = getUserDetails(u);
				int userStatus = 0;
				DataTable dt = ud.Result;

				foreach (DataRow dr in dt.Rows)
				{
					if (dr["Field"].ToString().Contains("status"))
					{
						string loginStatus = (dr["Value"].ToString()).Trim();
						if (!(loginStatus.Contains("Active")))
						{
							userStatus = 0;
						}
						else
						{
							userStatus = 1;
						}
					}

				}
				return userStatus;
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				return 0;
			}
		}

		public async Task<DataTable> getUserDetails(String uid)
		{
			getUserAttributes ua = new getUserAttributes
			{
				username = uid
			};
			try
			{

				System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
				binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;

				System.ServiceModel.EndpointAddress endpointAddress = new EndpointAddress("https://securelogin.sbilife.co.in/acas/LoginService?wsdl");
				LoginServiceClient objAgentAuth = new LoginServiceClient(binding, endpointAddress);

				var uar = await objAgentAuth.getUserAttributesAsync(ua);
				getUserAttributesResponse uar1 = uar.getUserAttributesResponse;

				String[] keyval = uar1.@return.ToString().Split(',');
				DataTable dt = new DataTable();

				dt.Columns.Add("Field");
				dt.Columns.Add("Value");

				foreach (string s in keyval)
				{
					String[] cols = s.Split(':');
					String key = decodeString(cols[0]);
					String val = decodeString(cols[1]);

					dt.Rows.Add(key, val);

				}

				return dt;
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				return null;
			}
		}
		//public async Task<IEnumerable<Group>> GetAllConfigurationGroups(int UserId, string search)
		//{
		//    try
		//    {
		//        var result = (from userMasterDetails in this._db.UserMasterDetails
		//                      join Designation in this._db.Group on userMasterDetails.GroupId equals Designation.Id
		//                      where userMasterDetails.GroupId == Designation.Id && userMasterDetails.UserMasterId == UserId
		//                      select new Group
		//                      {
		//                          Name = Designation.Name,
		//                          Id = Designation.Id

		//                      });
		//        return await result.AsNoTracking().ToListAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));

		//    }
		//    return null;
		//}

		public String decodeString(String sStringValue)
		{
			String sb = "";
			try
			{
				for (int i = 0; i < sStringValue.Length / 2; i++)
				{
					String sHexDigit = "";
					sHexDigit = sStringValue.Substring(i * 2, 2);
					int iDigit = int.Parse(sHexDigit, System.Globalization.NumberStyles.HexNumber);
					sb = sb + ((char)iDigit);
				}
				return sb;
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				return null;
			}
		}

		public DataTable CheckResultString(string resultString, DataTable dt, DataRow dr, string empStatus)
		{
			string Message;

			if (resultString == "Y")
			{
				dr["StatusCode"] = resultString;
				dr["Message"] = "SUCCESS";
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else if (resultString == "F")
			{
				Message = "Password is similar to previous 3 passwords hence Failed.";
				dr["StatusCode"] = resultString;
				dr["Message"] = Message;
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else if (resultString == "M")
			{
				Message = "Does meet minimum requirement of password complexity.";
				dr["StatusCode"] = resultString;
				dr["Message"] = Message;
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else if (resultString == "S")
			{
				Message = "Password is too small.";
				dr["StatusCode"] = resultString;
				dr["Message"] = Message;
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else if (resultString == "N1" || resultString == "N2" || resultString == "N3")
			{
				Message = "Invalid User ID.";
				dr["StatusCode"] = resultString;
				dr["Message"] = Message;
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else if (resultString == null)
			{
				dr["StatusCode"] = "EXTRA";
				dr["Message"] = "Exception Occured";
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			else
			{
				dr["StatusCode"] = "Invalid";
				dr["Message"] = resultString;
				dr["UserType"] = empStatus;
				dt.Rows.Add(dr);
			}
			return dt;
		}
		public async Task<List<string>> passwordchanged(int id, string OrgCode)
		{
			if (string.Equals(OrgCode, "bandhan", StringComparison.CurrentCultureIgnoreCase) || string.Equals(OrgCode, "bandhanbank", StringComparison.CurrentCultureIgnoreCase) || string.Equals(OrgCode, "enth", StringComparison.CurrentCultureIgnoreCase))
			{
				var query = _db.PasswordHistory.Where(e => e.UserMasterId == id).OrderByDescending(e => e.CreatedDate).Select(e => e.Password).Take(5);
				return query.ToList();
			}
			else
			{
				var query = _db.PasswordHistory.Where(e => e.UserMasterId == id).OrderByDescending(e => e.CreatedDate).Select(e => e.Password).Take(3);
				return query.ToList();
			}
		}



		//public async Task<FileInfo> ExportUserData(string columnName, string status, string search, int UserId, string UserName, string FIXED_JOB_ROLE, string OrgCode = null)
		//{
		//    if (columnName != null)
		//        columnName = columnName.ToLower().Equals("null") ? null : columnName;
		//    if (status != null)
		//        status = status.ToLower().Equals("null") ? null : status;
		//    if (search != null)
		//        search = search.ToLower().Equals("null") ? null : search;

		//    IEnumerable<APIUserMaster> users = await GetAllUser(1, 400000, search, columnName, status, UserId, UserName, OrgCode);
		//    var userSettings = await this._userSettingsRepository.GetUserSetting(OrgCode);
		//    var roles = await this._rolesRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);

		//    FileInfo File = GetExportUserDataExcel(users, userSettings, roles, FIXED_JOB_ROLE);
		//    return File;
		//}

		//private FileInfo GetExportUserDataExcel(IEnumerable<APIUserMaster> users, IEnumerable<APIUserSetting> userSettings, List<Roles> roles, string
		//    FIXED_JOB_ROLE)
		//{
		//    String ExcelName = @UserMasterImportField.UserMasterWithData;
		//    int RowNumber = 0;
		//    Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();


		//    List<string> UserDataHeaders = GetUserDataHeaders(userSettings, FIXED_JOB_ROLE);
		//    ExcelData.Add(RowNumber, UserDataHeaders);


		//    foreach (var userdata in users)
		//    {
		//        List<string> courseWiseCompletionRow = UserDataRow(userdata, userSettings, roles, FIXED_JOB_ROLE);
		//        RowNumber++;
		//        ExcelData.Add(RowNumber, courseWiseCompletionRow);
		//    }

		//    FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
		//    return ExcelFile;
		//}

		//private List<string> GetUserDataHeaders(IEnumerable<APIUserSetting> userSettings, string FIXED_JOB_ROLE)
		//{

		//    List<string> UserDataHeader = new List<string>
		//    {
		//        "User Id",
		//        "User Name",
		//        "Email Id",
		//        "Mobile Number",
		//        "Gender",
		//        "User Role",
		//        "Account Created Date",
		//        "Account Expiry Date",
		//        "Status",
		//        "TimeZone",
		//        "Currency",
		//        "Language",
		//        "Reports To",
		//        "Date Of Birth",
		//        "Date Of Joining",
		//        "User Type",
		//        "Account Deactivation Date",
		//        "Account Locked"

		//    };

		//    int i = 19;
		//    foreach (var userSetting in userSettings)
		//    {
		//        string ChangedColumnName = userSetting.ChangedColumnName.ToString();
		//        UserDataHeader.Add(ChangedColumnName);
		//        i++;
		//    }
		//    i++;

		//    if (FIXED_JOB_ROLE.ToString().ToLower() == "yes")
		//    {

		//        UserDataHeader.Add("JobRoleName");
		//        UserDataHeader.Add("DateIntoRole");

		//    }
		//    return UserDataHeader;

		//}

		//private List<string> UserDataRow(APIUserMaster usersdata, IEnumerable<APIUserSetting> userSettings, List<Roles> roles, string FIXED_JOB_ROLE)
		//{
		//    DateTime dateValue = new DateTime();

		//    List<string> UserDataRow = new List<string>
		//    {
		//        usersdata.UserId,
		//        usersdata.UserName,
		//        usersdata.EmailId,
		//        usersdata.MobileNumber,
		//        usersdata.Gender
		//    };
		//    var role = roles.Find(x => x.RoleCode == usersdata.UserRole);
		//    if (role != null)
		//    {
		//        UserDataRow.Add(role.RoleName);
		//    }
		//    else
		//    {
		//        UserDataRow.Add(usersdata.UserRole);
		//    }
		//    if (DateTime.TryParse(usersdata.AccountCreatedDate.ToString(), out DateTime outputDateTimeValue))
		//    {
		//        dateValue = outputDateTimeValue;

		//        if (dateValue == DateTime.MinValue)
		//            UserDataRow.Add(string.Empty);
		//        else
		//            UserDataRow.Add(dateValue.ToString("MMM dd, yyyy").ToString());
		//    }
		//    else
		//    {
		//        UserDataRow.Add(string.Empty);
		//    }
		//    UserDataRow.Add(this.DateValidation(usersdata.AccountExpiryDate));
		//    UserDataRow.Add(usersdata.IsActive == true ? "Active" : "Inactive");
		//    UserDataRow.Add(usersdata.TimeZone);
		//    UserDataRow.Add(usersdata.Currency);
		//    UserDataRow.Add(usersdata.Language);
		//    UserDataRow.Add(usersdata.ReportsTo);
		//    UserDataRow.Add(this.DateValidation(usersdata.DateOfBirth));
		//    UserDataRow.Add(this.DateValidation(usersdata.DateOfJoining));
		//    UserDataRow.Add(usersdata.UserType);
		//    UserDataRow.Add(this.DateValidation(usersdata.AccountDeactivationDate));
		//    UserDataRow.Add(usersdata.Lock == true ? "Yes" : "No");


		//    int i = 19;
		//    foreach (APIUserSetting opt in userSettings)
		//    {
		//        string ConfiguredColumnName = opt.ConfiguredColumnName.ToString();

		//        System.Reflection.PropertyInfo pi = usersdata.GetType().GetProperty(ConfiguredColumnName);
		//        if (pi != null)
		//        {
		//            String name = (String)(pi.GetValue(usersdata, null));

		//            UserDataRow.Add(name);
		//            i++;
		//        }
		//    }

		//    if (FIXED_JOB_ROLE.ToString().ToLower() == "yes")
		//    {
		//        UserDataRow.Add((usersdata.JobRoleName));
		//        UserDataRow.Add(this.DateValidation(usersdata.DateIntoRole));
		//    }

		//    return UserDataRow;
		//}

		private string DateValidation(DateTime? date)
		{
			DateTime dateValue = new DateTime();
			if (DateTime.TryParse(date.ToString(), out DateTime outputDateTimeValue))
			{
				dateValue = outputDateTimeValue;

				if (dateValue == DateTime.MinValue)
					return string.Empty;
				else
					return dateValue.ToString("MMM dd, yyyy").ToString();
			}
			else
			{
				return string.Empty;
			}
		}
		//public async Task<APIGetBespokeUserDetails> GetUserByBespoke(int userid)
		//{
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            APIGetBespokeUserDetails bespokeUserDetails = null;
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetBespokeUserDetails";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userid });

		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        bespokeUserDetails = new APIGetBespokeUserDetails
		//                        {
		//                            DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
		//                            UserId = Security.Decrypt(row["UserId"].ToString()),
		//                            UserName = row["UserName"].ToString(),
		//                            Grade = row["Grade"].ToString(),
		//                            Department = row["Department"].ToString(),
		//                            CostCode = row["CostCode"].ToString(),
		//                            Id = Security.Decrypt(row["Id"].ToString()),
		//                            EmployeeCode = row["EmployeeCode"].ToString()
		//                        };
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return bespokeUserDetails;
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//private async Task<List<APIUserId>> GetAllUsersListCache()
		//{
		//    try
		//    {
		//        var userList = await (from userMaster in this._db.UserMaster
		//                                  //join userDetails in this._db.UserMasterDetails on userMaster.Id equals userDetails.UserMasterId
		//                              where userMaster.IsDeleted == false
		//                              select new APIUserId
		//                              {
		//                                  Id = userMaster.Id,
		//                                  UserId = Security.Decrypt(userMaster.UserId),
		//                                  EmailId = Security.Decrypt(userMaster.EmailId),
		//                                  MobileNumber = "" //Security.Decrypt(userDetails.MobileNumber)
		//                              }).ToListAsync();
		//        return userList;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}


		//private async Task<List<APIUserId>> CacheTryGetValueSet(string orgCode, bool forceRefreshCache = false)
		//{
		//    List<APIUserId> cacheEntryIVP = new List<APIUserId>();
		//    List<APIUserId> cacheEntryENT = new List<APIUserId>();

		//    switch (orgCode)
		//    {
		//        case "ivp":
		//            if (forceRefreshCache)
		//            {
		//                // Key not in cache, so get data.
		//                cacheEntryIVP = await this.GetAllUsersListCache();

		//                // Set cache options.
		//                var cacheEntryOptions = new MemoryCacheEntryOptions()
		//                    // Keep in cache for this time, reset time if accessed.
		//                    .SetSlidingExpiration(TimeSpan.FromHours(4));

		//                // Save data in cache.
		//                _cacheIVP.Set(CacheKeys.EntryIVP, cacheEntryIVP, cacheEntryOptions);
		//            }
		//            else
		//            {
		//                // Look for cache key.
		//                if (!_cacheIVP.TryGetValue(CacheKeys.EntryIVP, out cacheEntryIVP))
		//                {
		//                    // Key not in cache, so get data.
		//                    cacheEntryIVP = await this.GetAllUsersListCache();

		//                    // Set cache options.
		//                    var cacheEntryOptions = new MemoryCacheEntryOptions()
		//                        // Keep in cache for this time, reset time if accessed.
		//                        .SetSlidingExpiration(TimeSpan.FromHours(4));

		//                    // Save data in cache.
		//                    _cacheIVP.Set(CacheKeys.EntryIVP, cacheEntryIVP, cacheEntryOptions);
		//                }
		//            }
		//            return cacheEntryIVP;
		//        case "ent":
		//            if (forceRefreshCache)
		//            {
		//                // Key not in cache, so get data.
		//                cacheEntryENT = await this.GetAllUsersListCache();

		//                // Set cache options.
		//                var cacheEntryOptions = new MemoryCacheEntryOptions()
		//                    // Keep in cache for this time, reset time if accessed.
		//                    .SetSlidingExpiration(TimeSpan.FromHours(4));

		//                // Save data in cache.
		//                _cacheENT.Set(CacheKeys.EntryENT, cacheEntryENT, cacheEntryOptions);
		//            }
		//            else
		//            {
		//                // Look for cache key.
		//                if (!_cacheIVP.TryGetValue(CacheKeys.EntryENT, out cacheEntryENT))
		//                {
		//                    // Key not in cache, so get data.
		//                    cacheEntryENT = await this.GetAllUsersListCache();

		//                    var cacheEntryOptions = new MemoryCacheEntryOptions()
		//                        .SetSlidingExpiration(TimeSpan.FromHours(4));
		//                    _cacheIVP.Set(CacheKeys.EntryENT, cacheEntryENT, cacheEntryOptions);
		//                }
		//            }
		//            return cacheEntryENT;
		//    }
		//    return null;
		//}

		//private async Task<List<APISearchResult>> GetUserSearchResult(string searchBy, string searchText, string orgCode)
		//{
		//    List<APIUserId> userList = new List<APIUserId>();
		//    List<APIUserId> userListResult = new List<APIUserId>();
		//    List<APISearchResult> searchUserList = new List<APISearchResult>();

		//    userList = await CacheTryGetValueSet(orgCode);

		//    switch (searchBy)
		//    {
		//        case "userid":
		//            userListResult = userList.FindAll(x => x.UserId.StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
		//            if (userListResult.Count == 0)
		//            {
		//                if (await IsExists(searchText))
		//                {
		//                    userList = await CacheTryGetValueSet(orgCode, true);
		//                    userListResult = userList.FindAll(x => x.UserId.StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
		//                }
		//            }

		//            foreach (var item in userListResult)
		//            {
		//                APISearchResult searchResult = new APISearchResult
		//                {
		//                    Id = item.Id,
		//                    Name = item.UserId
		//                };
		//                searchUserList.Add(searchResult);
		//            }
		//            return searchUserList;
		//        case "emailid":
		//            userListResult = userList.FindAll(x => x.EmailId.StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
		//            if (userListResult.Count == 0)
		//            {
		//                if (await EmailExists(searchText))
		//                {
		//                    userList = await CacheTryGetValueSet(orgCode, true);
		//                    userListResult = userList.FindAll(x => x.UserId.StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
		//                }
		//            }

		//            foreach (var item in userListResult)
		//            {
		//                APISearchResult searchResult = new APISearchResult
		//                {
		//                    Id = item.Id,
		//                    Name = item.EmailId
		//                };
		//                searchUserList.Add(searchResult);
		//            }
		//            return searchUserList;
		//    }
		//    return searchUserList;
		//}

		//public async Task<List<ApiGetUserName>> GetUsernamefromgroup(string groupname, string username)
		//{
		//    List<ApiGetUserName> EmpList;
		//    if (groupname == "null")
		//        groupname = null;
		//    if (groupname == "employee")
		//    {
		//        EmpList = await (from users in this._db.UserMasterDetails
		//                         join usermaster in this._db.UserMaster on users.UserMasterId equals usermaster.Id
		//                         where users.ConfigurationColumn8 == 3 && usermaster.UserName.StartsWith(username)
		//                         select new ApiGetUserName
		//                         {
		//                             ID = usermaster.Id,
		//                             Name = usermaster.UserName
		//                         }).ToListAsync();
		//    }
		//    else if (!string.IsNullOrEmpty(groupname))
		//    {
		//        EmpList = await (from config8 in this._db.Configure8
		//                         join userDetails in this._db.UserMasterDetails on config8.Id equals userDetails.ConfigurationColumn8
		//                         join usermaster in this._db.UserMaster on userDetails.UserMasterId equals usermaster.Id
		//                         where config8.Name == groupname && usermaster.UserName.StartsWith(username)
		//                         select new ApiGetUserName
		//                         {
		//                             ID = usermaster.Id,
		//                             Name = usermaster.UserName
		//                         }).ToListAsync();


		//    }
		//    else
		//    {
		//        EmpList = await (from userDetails in this._db.UserMasterDetails
		//                         join usermaster in this._db.UserMaster on userDetails.UserMasterId equals usermaster.Id
		//                         where userDetails.UserType == "External" && usermaster.UserName.StartsWith(username)
		//                         select new ApiGetUserName
		//                         {
		//                             ID = usermaster.Id,
		//                             Name = usermaster.UserName
		//                         }).ToListAsync();
		//    }
		//    return EmpList;
		//}
		//public async Task<bool> ExistsCode(string Code)
		//{
		//    Code = Code.ToLower().Trim();
		//    var result = await (from c in this._db.HouseMaster
		//                        where c.IsDeleted == 0 && (c.Code.ToLower().Equals(Code))
		//                        select new
		//                        { c.Id }).CountAsync();


		//    if (result > 0)
		//        return true;
		//    return false;


		//}
		//public async Task<bool> ExistsLandingPage(string LandingPage)
		//{
		//    LandingPage = LandingPage.ToLower().Trim();
		//    var result = await (from c in this._db.OrganizationPreferences
		//                        where c.IsDeleted == 0 && (c.LandingPage.ToLower().Equals(LandingPage))
		//                        select new
		//                        { c.Id }).CountAsync();


		//    if (result > 0)
		//        return true;
		//    return false;


		//}
		//public async Task<List<DesignationRoleMapping>> GetAllDesignationRoleList()
		//{
		//    var UserList = await (from designationroles in this._db.DesignationRoleMapping

		//                          select new DesignationRoleMapping
		//                          {
		//                              UserRole = designationroles.UserRole,
		//                              Designation = designationroles.Designation
		//                          }).ToListAsync();
		//    return UserList;
		//}
		//public async Task<string> ProcessImportFile(FileInfo file, IUserSettingsRepository _userSettingsRepository, IUserRepository _userRepository, IUserMasterRejectedRepository _userMasterRejectedRepository, IConfiguration _configuration, ICustomerConnectionStringRepository _customerConnectionStringRepository)
		//{
		//    string result;
		//    UserMasterImport ProcessFileobj = new UserMasterImport(_db, this, _customerConnectionStringRepository, _configuration, _identitySv);
		//    ProcessFileobj.Reset();
		//    bool resultMessage = await ProcessFileobj.InitilizeStatusAsync(file, _userSettingsRepository);
		//    if (resultMessage == true)
		//    {
		//        result = await ProcessFileobj.ProcessRecordAsync(file, _userSettingsRepository, _userRepository, _userMasterRejectedRepository, _configuration, _customerConnectionStringRepository);

		//        ProcessFileobj.Reset();
		//        return result;
		//    }
		//    else
		//    {
		//        result = Record.FileFieldInvalid;
		//        ProcessFileobj.Reset();
		//        return result;
		//    }
		//}


		public async Task<int> UpdateImport(APIUserMaster apiUser, string status)
		{ 
			try
			{
				apiUser.UserId = apiUser.UserId.ToLower();
				UserMaster User = this._db.UserMaster.Where(u => (u.UserId == Security.Encrypt(apiUser.UserId) && u.IsDeleted == false)).FirstOrDefault();

				if (User == null)
					return 0;

				UserMasterDetails UserDetails = this._db.UserMasterDetails.Where(u => (u.UserMasterId == User.Id && u.IsDeleted == false)).FirstOrDefault();


			   // User.IsActive = apiUser.IsActive;
				if (status.ToLower() == "active")
				{
					User.IsActive = true;
				}
				else
				{
					if (User.IsActive == true)
					{
						UserDetails.AccountDeactivationDate = DateTime.UtcNow;
					}
					User.IsActive = false;
				  
				}
				this._db.UserMaster.Update(User);
				this._db.UserMasterDetails.Update(UserDetails);

				await this._db.SaveChangesAsync();
				return 1;


			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
			}
			return 1;
		}


		//public async Task<List<APIUsersStatus>> GetAllUsersStatus()
		//{
		//    try
		//    {
		//        using (var context = this._db)
		//        {
		//            var result = (from userstatus in context.UserMaster

		//                          select new APIUsersStatus
		//                          {
		//                              UserId = userstatus.UserId,
		//                              Status = userstatus.IsActive,



		//                          });
		//            return await result.ToListAsync();
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));

		//    }
		//    return null;

		//}


		//public async Task<IEnumerable<APIUserMasterDelete>> GetAllDeletedUser(int page, int pageSize, string search = null, string columnName = null, int? userId = null, string encryptUserId = null)
		//{
		//    if (!string.IsNullOrEmpty(columnName) && (columnName.ToLower().Equals("emailid") || columnName.ToLower().Equals("userid") || columnName.ToLower().Equals("mobilenumber")))
		//    {
		//        if (!string.IsNullOrEmpty(search))
		//        {
		//            search = Security.Encrypt(search.ToLower());
		//        }
		//    }
		//    List<APIUserMasterDelete> users = new List<APIUserMasterDelete>();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();

		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetAllDeletedUser";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
		//                cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
		//                cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
		//                cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {

		//                        var user = new APIUserMasterDelete
		//                        {
		//                            AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
		//                            AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
		//                            ConfigurationColumn1 = row["configure1"].ToString(),
		//                            ConfigurationColumn2 = row["configure2"].ToString(),
		//                            ConfigurationColumn3 = row["configure3"].ToString(),
		//                            ConfigurationColumn4 = row["configure4"].ToString(),
		//                            ConfigurationColumn5 = row["configure5"].ToString(),
		//                            ConfigurationColumn6 = row["configure6"].ToString(),
		//                            ConfigurationColumn7 = row["configure7"].ToString(),
		//                            ConfigurationColumn8 = row["configure8"].ToString(),
		//                            ConfigurationColumn9 = row["configure9"].ToString(),
		//                            ConfigurationColumn10 = row["configure10"].ToString(),
		//                            ConfigurationColumn11 = row["configure11"].ToString(),
		//                            ConfigurationColumn12 = row["configure12"].ToString(),
		//                            Currency = row["Currency"].ToString(),
		//                            CustomerCode = row["CustomerCode"].ToString(),
		//                            DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
		//                            DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
		//                            EmailId = Security.Decrypt(row["EmailId"].ToString()),
		//                            Gender = row["Gender"].ToString(),
		//                            Id = Convert.ToInt32(row["Id"].ToString()),
		//                            IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
		//                            Language = row["Language"].ToString(),
		//                            LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
		//                            MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
		//                            CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
		//                            CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            SerialNumber = row["SerialNumber"].ToString(),
		//                            TimeZone = row["TimeZone"].ToString(),
		//                            UserId = Security.Decrypt(row["UserId"].ToString()),
		//                            UserName = row["UserName"].ToString(),
		//                            UserRole = row["UserRole"].ToString(),
		//                            UserType = row["UserType"].ToString(),
		//                            Location = row["location"].ToString(),
		//                            Area = row["area"].ToString(),
		//                            Group = row["groupname"].ToString(),
		//                            Business = row["buisness"].ToString(),
		//                            ReportsTo = Security.Decrypt(row["reportsto"].ToString()),
		//                            IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
		//                            Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
		//                            AppearOnLeaderboard = Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
		//                            House = row["House"].ToString(),
		//                            deletedBy = string.IsNullOrEmpty(row["DeletedBy"].ToString()) ? null : row["DeletedBy"].ToString(),
		//                            deletedDate = string.IsNullOrEmpty(row["DeletedOn"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DeletedOn"].ToString()),
		//                            AcceptanceDate = string.IsNullOrEmpty(row["AcceptanceDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AcceptanceDate"].ToString())
		//                        };
		//                        users.Add(user);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return users;
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}



		public async Task<int> GetDeletedUserCount(string search = null, string columnName = null, int? userId = null, string encryptUserId = null)
		{

			if (!string.IsNullOrEmpty(columnName) && (columnName.ToLower().Equals("emailid") || columnName.ToLower().Equals("userid") || columnName.ToLower().Equals("mobilenumber")))
			{
				if (!string.IsNullOrEmpty(search))
				{
					search = Security.Encrypt(search.ToLower());
				}
			}
			int Count = 0;
			try
			{
				using (var dbContext = _customerConnectionString.GetDbContext())
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "GetAllDeletedUserCount";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
						cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = columnName });
						cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);
						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								Count = string.IsNullOrEmpty(row["DeletedCount"].ToString()) ? 0 : Convert.ToInt32(row["DeletedCount"].ToString());
							}
						}
						reader.Dispose();
					}
					connection.Close();
					return Count;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));

			}
			return 0;
		}

		//public async Task<APIUserMasterDelete> GetDeletedUserInfo(int id, string decryptUserId = null)
		//{
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            APIUserMasterDelete user = null;
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetDeletedUser";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = id });
		//                cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.VarChar) { Value = decryptUserId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);

		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {

		//                        user = new APIUserMasterDelete
		//                        {
		//                            AccountCreatedDate = string.IsNullOrEmpty(row["AccountCreatedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountCreatedDate"].ToString()),
		//                            AccountExpiryDate = string.IsNullOrEmpty(row["AccountExpiryDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AccountExpiryDate"].ToString()),
		//                            ConfigurationColumn1 = row["configure1"].ToString(),
		//                            ConfigurationColumn2 = row["configure2"].ToString(),
		//                            ConfigurationColumn3 = row["configure3"].ToString(),
		//                            ConfigurationColumn4 = row["configure4"].ToString(),
		//                            ConfigurationColumn5 = row["configure5"].ToString(),
		//                            ConfigurationColumn6 = row["configure6"].ToString(),
		//                            ConfigurationColumn7 = row["configure7"].ToString(),
		//                            ConfigurationColumn8 = row["configure8"].ToString(),
		//                            ConfigurationColumn9 = row["configure9"].ToString(),
		//                            ConfigurationColumn10 = row["configure10"].ToString(),
		//                            ConfigurationColumn11 = row["configure11"].ToString(),
		//                            ConfigurationColumn12 = row["configure12"].ToString(),
		//                            ConfigurationColumn1Id = string.IsNullOrEmpty(row["configure1Id"].ToString()) ? null : int.Parse(row["configure1Id"].ToString()) as int?,
		//                            ConfigurationColumn2Id = string.IsNullOrEmpty(row["configure2Id"].ToString()) ? null : int.Parse(row["configure2Id"].ToString()) as int?,
		//                            ConfigurationColumn3Id = string.IsNullOrEmpty(row["configure3Id"].ToString()) ? null : int.Parse(row["configure3Id"].ToString()) as int?,
		//                            ConfigurationColumn4Id = string.IsNullOrEmpty(row["configure4Id"].ToString()) ? null : int.Parse(row["configure4Id"].ToString()) as int?,
		//                            ConfigurationColumn5Id = string.IsNullOrEmpty(row["configure5Id"].ToString()) ? null : int.Parse(row["configure5Id"].ToString()) as int?,
		//                            ConfigurationColumn6Id = string.IsNullOrEmpty(row["configure6Id"].ToString()) ? null : int.Parse(row["configure6Id"].ToString()) as int?,
		//                            ConfigurationColumn7Id = string.IsNullOrEmpty(row["configure7Id"].ToString()) ? null : int.Parse(row["configure7Id"].ToString()) as int?,
		//                            ConfigurationColumn8Id = string.IsNullOrEmpty(row["configure8Id"].ToString()) ? null : int.Parse(row["configure8Id"].ToString()) as int?,
		//                            ConfigurationColumn9Id = string.IsNullOrEmpty(row["configure9Id"].ToString()) ? null : int.Parse(row["configure9Id"].ToString()) as int?,
		//                            ConfigurationColumn10Id = string.IsNullOrEmpty(row["configure10Id"].ToString()) ? null : int.Parse(row["configure10Id"].ToString()) as int?,
		//                            ConfigurationColumn11Id = string.IsNullOrEmpty(row["configure11Id"].ToString()) ? null : int.Parse(row["configure11Id"].ToString()) as int?,
		//                            ConfigurationColumn12Id = string.IsNullOrEmpty(row["configure12Id"].ToString()) ? null : int.Parse(row["configure12Id"].ToString()) as int?,
		//                            Currency = row["Currency"].ToString(),
		//                            CustomerCode = row["CustomerCode"].ToString(),
		//                            DateOfBirth = string.IsNullOrEmpty(row["DateOfBirth"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfBirth"].ToString()),
		//                            DateOfJoining = string.IsNullOrEmpty(row["DateOfJoining"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["DateOfJoining"].ToString()),
		//                            EmailId = Security.Decrypt(row["EmailId"].ToString()),
		//                            Gender = row["Gender"].ToString(),
		//                            Id = Convert.ToInt32(row["Id"].ToString()),
		//                            IsActive = Convert.ToBoolean(row["IsActive"].ToString()),
		//                            Language = row["Language"].ToString(),
		//                            LastModifiedDate = string.IsNullOrEmpty(row["LastModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["LastModifiedDate"].ToString()),
		//                            MobileNumber = Security.Decrypt(row["MobileNumber"].ToString()),
		//                            Password = row["Password"].ToString(),
		//                            ProfilePicture = row["ProfilePicture"].ToString(),
		//                            SerialNumber = row["SerialNumber"].ToString(),
		//                            TimeZone = row["TimeZone"].ToString(),
		//                            UserId = Security.Decrypt(row["UserId"].ToString()),
		//                            UserName = row["UserName"].ToString(),
		//                            UserRole = row["UserRole"].ToString(),
		//                            UserType = row["UserType"].ToString(),
		//                            Location = row["location"].ToString(),
		//                            Area = row["area"].ToString(),
		//                            Group = row["groupname"].ToString(),
		//                            Business = row["buisness"].ToString(),
		//                            ReportsTo = Security.Decrypt(row["ReportsTo"].ToString()),
		//                            ModifiedByName = row["ModifiedByName"].ToString(),
		//                            BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? null : (int?)Convert.ToInt32(row["BusinessId"].ToString()),
		//                            LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? null : (int?)Convert.ToInt32(row["LocationId"].ToString()),
		//                            AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? null : (int?)Convert.ToInt32(row["AreaId"].ToString()),
		//                            GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? null : (int?)Convert.ToInt32(row["GroupId"].ToString()),
		//                            IsDeleted = Convert.ToBoolean(row["IsDeleted"].ToString()),
		//                            IsEnableDegreed = string.IsNullOrEmpty(row["Degreed"].ToString()) ? false : Convert.ToBoolean(row["Degreed"].ToString()),
		//                            TermsCondintionsAccepted = string.IsNullOrEmpty(row["TermsCondintionsAccepted"].ToString()) ? false : Convert.ToBoolean(row["TermsCondintionsAccepted"].ToString()),
		//                            AcceptanceDate = string.IsNullOrEmpty(row["AcceptanceDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["AcceptanceDate"].ToString()),
		//                            CreatedDate = Convert.ToDateTime(row["CreatedDate"].ToString()),
		//                            CreatedBy = Convert.ToInt32(row["CreatedBy"].ToString()),
		//                            Lock = string.IsNullOrEmpty(row["Lock"].ToString()) ? false : Convert.ToBoolean(row["Lock"].ToString()),
		//                            IsPasswordModified = string.IsNullOrEmpty(row["IsPasswordModified"].ToString()) ? false : Convert.ToBoolean(row["IsPasswordModified"].ToString()),
		//                            ImplicitRole = row["ImplicitRole"].ToString(),
		//                            HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? null : (int?)Convert.ToInt32(row["HouseId"].ToString()),
		//                            House = row["HouseName"].ToString(),
		//                            AppearOnLeaderboard = string.IsNullOrEmpty(row["AppearOnLeaderboard"].ToString()) ? false : Convert.ToBoolean(row["AppearOnLeaderboard"].ToString()),
		//                            RowGuid = Guid.Parse(row["RowGuid"].ToString()),
		//                            PasswordModifiedDate = string.IsNullOrEmpty(row["PasswordModifiedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["PasswordModifiedDate"].ToString())

		//                        };
		//                    }
		//                }

		//                reader.Dispose();

		//            }
		//            connection.Close();
		//            return user;
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<List<UserRejectedStatus>> GetAllUserRejectedStatus()
		//{
		//    var result = (from userRejectedStatus in this._db.UserRejectedStatus


		//                  select new UserRejectedStatus
		//                  {
		//                      Id = userRejectedStatus.Id,
		//                      UserId = userRejectedStatus.UserId,
		//                      Status = userRejectedStatus.Status
		//                  });


		//    return await result.ToListAsync();

		//}

		//public async Task<UserRejectedStatus> AddStatus(APIUserRejectedStatus apiuserRejectedStatus)
		//{
		//    UserRejectedStatus userRejectedStatus = new UserRejectedStatus
		//    {
		//        CreatedDate = DateTime.UtcNow,
		//        UserId = apiuserRejectedStatus.UserId,
		//        Status = apiuserRejectedStatus.Status
		//    };


		//    this._db.UserRejectedStatus.Add(userRejectedStatus);
		//    await this._db.SaveChangesAsync();




		//    return (userRejectedStatus);
		//}
		//public async Task<IEnumerable<APIMobilePermissions>> GetMobilePermissions(string RoleCode, int UserId)
		//{
		//    List<APIMobilePermissions> mobilePermissions = new List<APIMobilePermissions>();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
		//        {
		//            var connection = dbContext.Database.GetDbConnection();

		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "GetMobilePermissions";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = RoleCode });
		//                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar) { Value = UserId });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {

		//                        var permissions = new APIMobilePermissions
		//                        {
		//                            Name = row["Name"].ToString(),
		//                            Code = row["Code"].ToString(),
		//                            IsAccess = Convert.ToBoolean(row["IsAccess"].ToString())

		//                        };
		//                        mobilePermissions.Add(permissions);
		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return mobilePermissions;
		//        }

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}

		//public async Task<APIGetUserDetails> GetUserDetailsByUserId(string UserId)
		//{
		//    APIGetUserDetails aPIGetUserDetails = new APIGetUserDetails();
		//    UserMaster userMaster = new UserMaster();

		//    userMaster = await this._db.UserMaster.Where(a => a.UserId == UserId).FirstOrDefaultAsync();
		//    aPIGetUserDetails.Id = userMaster.Id;
		//    aPIGetUserDetails.UserId = Security.Decrypt(userMaster.UserId);
		//    aPIGetUserDetails.UserName = userMaster.UserName;

		//    return aPIGetUserDetails;
		//}


		//public async Task<APIUserDetails> GetOAuthUser(string userName)
		//{
		//    APIUserDetails apiUserDetails = new APIUserDetails();
		//    try
		//    {
		//        var connection = this._db.Database.GetDbConnection();

		//        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//            connection.Open();

		//        using (var cmd = connection.CreateCommand())
		//        {
		//            cmd.CommandText = "GetOAuthUser";
		//            cmd.CommandType = CommandType.StoredProcedure;
		//            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Char) { Value = userName });
		//            DbDataReader reader = await cmd.ExecuteReaderAsync();
		//            DataTable dt = new DataTable();
		//            string dateString = DateTime.UtcNow.AddMinutes(5).ToString();

		//            dt.Load(reader);
		//            if (dt.Columns.Count == 1)
		//            {
		//                foreach (DataRow row in dt.Rows)
		//                {
		//                    apiUserDetails.Result = row["Result"].ToString();
		//                }
		//            }
		//            else
		//            {
		//                foreach (DataRow row in dt.Rows)
		//                {
		//                    apiUserDetails.UserID = Security.Decrypt(row["UserID"].ToString());
		//                    apiUserDetails.UserName = row["UserName"].ToString();
		//                    apiUserDetails.MailID = Security.Decrypt(row["MailID"].ToString());
		//                    apiUserDetails.OrganizationCode = row["OrganizationCode"].ToString();
		//                    apiUserDetails.location = row["location"].ToString();
		//                    apiUserDetails.Designation = row["Designation"].ToString();
		//                    apiUserDetails.UserRole = row["UserRole"].ToString();
		//                    apiUserDetails.Result = "SUCCESS";
		//                    apiUserDetails.EntryptedUserID = System.Web.HttpUtility.UrlEncode(Security.Encrypt(row["UserID"].ToString() + "|" + dateString));
		//                }

		//                reader.Dispose();
		//            }
		//            connection.Close();
		//            return apiUserDetails;
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<object> GetConfigurationColumsData(string columnName)
		//{
		//    columnName = columnName.ToLower();

		//    switch (columnName)
		//    {
		//        case "business":
		//            return await _db.Business.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "group":
		//            return await _db.Group.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "area":
		//            return await _db.Area.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "location":
		//            return await _db.Location.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn1":
		//            return await _db.Configure1.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn2":
		//            return await _db.Configure2.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();

		//        case "configurationcolumn3":
		//            return await _db.Configure3.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn4":
		//            return await _db.Configure4.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn5":
		//            return await _db.Configure5.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn6":
		//            return await _db.Configure6.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn7":
		//            return await _db.Configure7.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn8":
		//            return await _db.Configure8.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn9":
		//            return await _db.Configure9.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn10":
		//            return await _db.Configure10.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn11":
		//            return await _db.Configure11.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();
		//        case "configurationcolumn12":
		//            return await _db.Configure12.Where(u => u.IsDeleted == 0).Select(e => new { e.Name, e.Id }).ToListAsync();


		//    }
		//    return null;
		//}
		//public async Task<object> GetConfigurationColumsDataForDashboard(string columnName)
		//{
		//    columnName = columnName.ToLower();

		//    switch (columnName)
		//    {
		//        case "business":
		//            return await (from bu in _db.Business
		//                          join ud in _db.UserMasterDetails on bu.Id equals ud.BusinessId
		//                          where bu.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = bu.Id,
		//                              Name = bu.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "group":
		//            return await (from grp in _db.Group
		//                          join ud in _db.UserMasterDetails on grp.Id equals ud.GroupId
		//                          where grp.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = grp.Id,
		//                              Name = grp.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "area":
		//            return await (from ar in _db.Area
		//                          join ud in _db.UserMasterDetails on ar.Id equals ud.AreaId
		//                          where ar.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = ar.Id,
		//                              Name = ar.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "location":
		//            return await (from lc in _db.Location
		//                          join ud in _db.UserMasterDetails on lc.Id equals ud.LocationId
		//                          where lc.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = lc.Id,
		//                              Name = lc.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn1":
		//            return await (from c1 in _db.Configure1
		//                          join ud in _db.UserMasterDetails on c1.Id equals ud.ConfigurationColumn1
		//                          where c1.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c1.Id,
		//                              Name = c1.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn2":
		//            return await (from c2 in _db.Configure2
		//                          join ud in _db.UserMasterDetails on c2.Id equals ud.ConfigurationColumn2
		//                          where c2.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c2.Id,
		//                              Name = c2.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn3":
		//            return await (from c3 in _db.Configure3
		//                          join ud in _db.UserMasterDetails on c3.Id equals ud.ConfigurationColumn3
		//                          where c3.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c3.Id,
		//                              Name = c3.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn4":
		//            return await (from c4 in _db.Configure4
		//                          join ud in _db.UserMasterDetails on c4.Id equals ud.ConfigurationColumn4
		//                          where c4.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c4.Id,
		//                              Name = c4.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn5":
		//            return await (from c5 in _db.Configure5
		//                          join ud in _db.UserMasterDetails on c5.Id equals ud.ConfigurationColumn5
		//                          where c5.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c5.Id,
		//                              Name = c5.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn6":
		//            return await (from c6 in _db.Configure6
		//                          join ud in _db.UserMasterDetails on c6.Id equals ud.ConfigurationColumn6
		//                          where c6.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c6.Id,
		//                              Name = c6.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn7":
		//            return await (from c7 in _db.Configure7
		//                          join ud in _db.UserMasterDetails on c7.Id equals ud.ConfigurationColumn7
		//                          where c7.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c7.Id,
		//                              Name = c7.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn8":
		//            return await (from c8 in _db.Configure8
		//                          join ud in _db.UserMasterDetails on c8.Id equals ud.ConfigurationColumn8
		//                          where c8.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c8.Id,
		//                              Name = c8.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn9":
		//            return await (from c9 in _db.Configure9
		//                          join ud in _db.UserMasterDetails on c9.Id equals ud.ConfigurationColumn9
		//                          where c9.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c9.Id,
		//                              Name = c9.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn10":
		//            return await (from c10 in _db.Configure10
		//                          join ud in _db.UserMasterDetails on c10.Id equals ud.ConfigurationColumn10
		//                          where c10.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c10.Id,
		//                              Name = c10.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn11":
		//            return await (from c11 in _db.Configure11
		//                          join ud in _db.UserMasterDetails on c11.Id equals ud.ConfigurationColumn11
		//                          where c11.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c11.Id,
		//                              Name = c11.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();
		//        case "configurationcolumn12":
		//            return await (from c12 in _db.Configure12
		//                          join ud in _db.UserMasterDetails on c12.Id equals ud.ConfigurationColumn12
		//                          where c12.IsDeleted == 0 && ud.IsDeleted == false
		//                          select new
		//                          {
		//                              Id = c12.Id,
		//                              Name = c12.Name
		//                          }).Distinct().OrderBy(x => x.Name).ToListAsync();


		//    }
		//    return null;
		//}

		public async Task<int> InActiveUser(int id)
		{
			try
			{
				UserMaster User = await this.Get(id);
				if (User == null)
					return 0;

				if (User.IsActive)
				{
					User.IsActive = false;


					this._db.UserMaster.Update(User);
					await this._db.SaveChangesAsync();


					return 1;
				}
				else
				{
					return -1;
				}
			}
			catch (System.Exception ex)
			{
				_logger.Error(Utilities.GetDetailedException(ex));
				return 0;
			}
		}

		//public async Task<List<APIUsersByLocation>> GetUsersByLocation(int locationId, string search = null)
		//{

		//    var result = (from userMaster in this._db.UserMaster
		//                  join userMasterDetails in this._db.UserMasterDetails on userMaster.Id equals userMasterDetails.UserMasterId
		//                  join Location in this._db.Location on userMasterDetails.LocationId equals Location.Id
		//                  where ((search == null || userMaster.UserName.StartsWith(search)) && userMasterDetails.LocationId == locationId)
		//                  select new APIUsersByLocation
		//                  {
		//                      UserName = userMaster.UserName,
		//                      UserId = Security.Decrypt(userMaster.UserId),
		//                      Id = userMaster.Id

		//                  });


		//    return await result.ToListAsync();

		//}

		//public async Task<List<APISearchUserforApplication>> GetSearchApplicationTypeAhead(string search = null)
		//{
		//    try
		//    {

		//        if (!string.IsNullOrEmpty(search))
		//        {
		//            search = Security.Encrypt(search.ToLower());
		//        }
		//        var result = (from userMaster in this._db.UserMaster
		//                      join userMasterDetails in this._db.UserMasterDetails on userMaster.Id equals userMasterDetails.UserMasterId
		//                      join Location in this._db.Location on userMasterDetails.LocationId equals Location.Id into ps
		//                      from Location in ps.DefaultIfEmpty()
		//                      where userMaster.UserId == search
		//                      select new APISearchUserforApplication
		//                      {
		//                          UserName = userMaster.UserName,
		//                          UserId = Security.Decrypt(userMaster.UserId),
		//                          Id = userMaster.Id,
		//                          EmailId = userMaster.EmailId,
		//                          MobileNumber = userMasterDetails.MobileNumber,
		//                          LocationName = Location.Name,
		//                          LocationId = userMasterDetails.LocationId

		//                      });


		//        return await result.ToListAsync();

		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }

		//}

		//public async Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "")
		//{
		//    DataTable dtConfigurationValues;
		//    string configValue;
		//    try
		//    {
		//        var cache = new CacheManager.CacheManager();
		//        string cacheKeyConfig = (Helper.Constants.CacheKeyNames.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

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

		//public async Task<List<AppConfiguration>> GetUserConfigurationValueAsync(string [] str_arr, string orgCode, string defaultValue = "")
		//{
		//    DataTable dtConfigurationValues;
		//    List<AppConfiguration> configValues=new List<AppConfiguration>();
		//    try
		//    {
		//        var cache = new CacheManager.CacheManager();
		//        string cacheKeyConfig = (Helper.Constants.CacheKeyNames.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

		//        if (cache.IsAdded(cacheKeyConfig))
		//            dtConfigurationValues = cache.Get<DataTable>(cacheKeyConfig);
		//        else
		//        {
		//            dtConfigurationValues = this.GetAllConfigurableParameterValue();
		//            cache.Add(cacheKeyConfig, dtConfigurationValues, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
		//        }
		//        foreach (var item in str_arr)
		//        {
		//            AppConfiguration configValue = new AppConfiguration();
		//            DataRow[] dr = dtConfigurationValues.Select("Code = '" + item.ToString() + "'");
		//            if (dr.Length > 0)
		//            {
		//                configValue.Code = item.ToString();
		//                configValue.value = Convert.ToString(dr[0]["Value"]);

		//                configValues.Add(configValue);
		//            }
		//            else
		//            {
		//                configValue.Code = item.ToString();
		//                configValue.value = defaultValue;

		//                configValues.Add(configValue);
		//            }
					   
		//        }
			   
		//        //_logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
		//        return null;
		//    }
		//    return configValues;
		//}

		//public DataTable GetAllConfigurableParameterValue()
		//{
		//    DataTable dt = new DataTable();
		//    try
		//    {
		//        using (var dbContext = _customerConnectionString.GetDbContext())
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


		public async Task<APIUserSignUp> GetUserObject1(APIUserSignUp user, string UserRole, string orgCode)
		{
			try
			{
				this.ChangeDbContext(orgCode);
				using (var dbContext = _customerConnectionString.GetDbContext(orgCode))
				{
					var connection = dbContext.Database.GetDbConnection();
					if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
						connection.Open();
					using (var cmd = connection.CreateCommand())
					{

						var lowerCaseAllow = await GetMasterConfigurableParameterValueByConnectionString("SAVE_USER_IMPORT_ASIS", orgCode);

						if (lowerCaseAllow.ToString() == "Yes")
						{
							user.Location = user.Location == null ? null : user.Location.Trim();
							user.Business = user.Business == null ? null : user.Business.Trim();
							user.Group = user.Group == null ? null : user.Group.Trim();
							user.Area = user.Area == null ? null : user.Area.Trim();
							user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.Trim();
							user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.Trim();
							user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.Trim();
							user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.Trim();
							user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.Trim();
							user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.Trim();
							user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.Trim();
							user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.Trim();
							user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.Trim();
							user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.Trim();
							user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.Trim();
							user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.Trim();
							user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();
						}
						else
						{

							user.Location = user.Location == null ? null : user.Location.ToLower().Trim();
							user.Business = user.Business == null ? null : user.Business.ToLower().Trim();
							user.Group = user.Group == null ? null : user.Group.Trim();
							user.Area = user.Area == null ? null : user.Area.ToLower().Trim();
							user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.ToLower().Trim();
							user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.ToLower().Trim();
							user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.ToLower().Trim();
							user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.ToLower().Trim();
							user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.ToLower().Trim();
							user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.ToLower().Trim();
							user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.ToLower().Trim();
							user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.ToLower().Trim();
							user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.ToLower().Trim();
							user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.ToLower().Trim();
							user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.ToLower().Trim();
							user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.ToLower().Trim();
							user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();

						}




						cmd.CommandText = "GetIdByName";
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@Location", SqlDbType.VarChar) { Value = user.Location });
						cmd.Parameters.Add(new SqlParameter("@Business", SqlDbType.VarChar) { Value = user.Business });
						cmd.Parameters.Add(new SqlParameter("@Group", SqlDbType.VarChar) { Value = user.Group });
						cmd.Parameters.Add(new SqlParameter("@Area", SqlDbType.VarChar) { Value = user.Area });
						cmd.Parameters.Add(new SqlParameter("@Configuration1", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 });
						cmd.Parameters.Add(new SqlParameter("@Configuration2", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 });
						cmd.Parameters.Add(new SqlParameter("@Configuration3", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 });
						cmd.Parameters.Add(new SqlParameter("@Configuration4", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 });
						cmd.Parameters.Add(new SqlParameter("@Configuration5", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 });
						cmd.Parameters.Add(new SqlParameter("@Configuration6", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 });
						cmd.Parameters.Add(new SqlParameter("@Configuration7", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 });
						cmd.Parameters.Add(new SqlParameter("@Configuration8", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 });
						cmd.Parameters.Add(new SqlParameter("@Configuration9", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 });
						cmd.Parameters.Add(new SqlParameter("@Configuration10", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 });
						cmd.Parameters.Add(new SqlParameter("@Configuration11", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 });
						cmd.Parameters.Add(new SqlParameter("@Configuration12", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 });
						cmd.Parameters.Add(new SqlParameter("@Configuration1Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 == null ? null : Security.Encrypt(user.ConfigurationColumn1) });
						cmd.Parameters.Add(new SqlParameter("@Configuration2Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 == null ? null : Security.Encrypt(user.ConfigurationColumn2) });
						cmd.Parameters.Add(new SqlParameter("@Configuration3Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 == null ? null : Security.Encrypt(user.ConfigurationColumn3) });
						cmd.Parameters.Add(new SqlParameter("@Configuration4Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 == null ? null : Security.Encrypt(user.ConfigurationColumn4) });
						cmd.Parameters.Add(new SqlParameter("@Configuration5Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 == null ? null : Security.Encrypt(user.ConfigurationColumn5) });
						cmd.Parameters.Add(new SqlParameter("@Configuration6Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 == null ? null : Security.Encrypt(user.ConfigurationColumn6) });
						cmd.Parameters.Add(new SqlParameter("@Configuration7Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 == null ? null : Security.Encrypt(user.ConfigurationColumn7) });
						cmd.Parameters.Add(new SqlParameter("@Configuration8Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 == null ? null : Security.Encrypt(user.ConfigurationColumn8) });
						cmd.Parameters.Add(new SqlParameter("@Configuration9Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 == null ? null : Security.Encrypt(user.ConfigurationColumn9) });
						cmd.Parameters.Add(new SqlParameter("@Configuration10Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 == null ? null : Security.Encrypt(user.ConfigurationColumn10) });
						cmd.Parameters.Add(new SqlParameter("@Configuration11Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 == null ? null : Security.Encrypt(user.ConfigurationColumn11) });
						cmd.Parameters.Add(new SqlParameter("@Configuration12Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 == null ? null : Security.Encrypt(user.ConfigurationColumn12) });
						cmd.Parameters.Add(new SqlParameter("@AreaEncrypted", SqlDbType.VarChar) { Value = user.Area == null ? null : Security.Encrypt(user.Area) });
						cmd.Parameters.Add(new SqlParameter("@BuisnessEncrypted", SqlDbType.VarChar) { Value = user.Business == null ? null : Security.Encrypt(user.Business) });
						cmd.Parameters.Add(new SqlParameter("@GroupEncrypted", SqlDbType.VarChar) { Value = user.Group == null ? null : Security.Encrypt(user.Group) });
						cmd.Parameters.Add(new SqlParameter("@LocationEncrypted", SqlDbType.VarChar) { Value = user.Location == null ? null : Security.Encrypt(user.Location) });
						cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.VarChar) { Value = "10-11-2018" });
						cmd.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.VarChar) { Value = UserRole });
						cmd.Parameters.Add(new SqlParameter("@JobRoleName", SqlDbType.VarChar) { Value = user.JobRoleName });
						DbDataReader reader = await cmd.ExecuteReaderAsync();
						DataTable dt = new DataTable();
						dt.Load(reader);
						if (dt.Rows.Count > 0)
						{
							foreach (DataRow row in dt.Rows)
							{
								user.LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? (int?)null : Convert.ToInt32(row["LocationId"].ToString());
								user.BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? (int?)null : Convert.ToInt32(row["BusinessId"].ToString());
								user.GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? (int?)null : Convert.ToInt32(row["GroupId"].ToString());
								user.AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? (int?)null : Convert.ToInt32(row["AreaId"].ToString());
								user.ConfigurationColumn1Id = string.IsNullOrEmpty(row["Configuration1Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration1Id"].ToString());
								user.ConfigurationColumn2Id = string.IsNullOrEmpty(row["Configuration2Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration2Id"].ToString());
								user.ConfigurationColumn3Id = string.IsNullOrEmpty(row["Configuration3Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration3Id"].ToString());
								user.ConfigurationColumn4Id = string.IsNullOrEmpty(row["Configuration4Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration4Id"].ToString());
								user.ConfigurationColumn5Id = string.IsNullOrEmpty(row["Configuration5Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration5Id"].ToString());
								user.ConfigurationColumn6Id = string.IsNullOrEmpty(row["Configuration6Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration6Id"].ToString());
								user.ConfigurationColumn7Id = string.IsNullOrEmpty(row["Configuration7Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration7Id"].ToString());
								user.ConfigurationColumn8Id = string.IsNullOrEmpty(row["Configuration8Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration8Id"].ToString());
								user.ConfigurationColumn9Id = string.IsNullOrEmpty(row["Configuration9Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration9Id"].ToString());
								user.ConfigurationColumn10Id = string.IsNullOrEmpty(row["Configuration10Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration10Id"].ToString());
								user.ConfigurationColumn11Id = string.IsNullOrEmpty(row["Configuration11Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration11Id"].ToString());
								user.ConfigurationColumn12Id = string.IsNullOrEmpty(row["Configuration12Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration12Id"].ToString());
								user.HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? (int?)null : Convert.ToInt32(row["HouseId"].ToString());
								user.IsAllConfigured = Convert.ToBoolean(row["IsAllConfigured"].ToString());
								user.JobRoleId = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? (int?)null : Convert.ToInt32(row["JobRoleId"].ToString());

							}
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
			return user;
		}
		public async Task<APIUserSignUp> GetUserIdByEmailId(string emailid, string orgcode)
		{
			this.ChangeDbContext(orgcode);
			var users = (from userMaster in this._db.UserMaster
						 where userMaster.EmailId == emailid
						 select new APIUserSignUp
						 {
							 Id = userMaster.Id,
							 UserId = userMaster.UserId,
							 UserName = userMaster.UserName

						 });
			return await users.FirstOrDefaultAsync();
		}

		//public async Task<int> AddVFSUserSignUp(APIVFSUserSignUp Apiuser, string UserRole, string OrganisationString)
		//{
		//    string toEmail = Apiuser.EmailId;
		//    string MobileNumber = Apiuser.MobileNumber;
		//    this.ChangeDbContext(OrganisationString);
		//    Exists exist = await this.ValidationsForUserVFSSignUp1(Apiuser, OrganisationString);
		//    if (!exist.Equals(Exists.No))
		//        return 0;
		//    else
		//    {
		//        UserRole = "CA";
		//        Apiuser = await this.GetUserObjectForVFSSignUp(Apiuser, UserRole, OrganisationString);
		//        if (!(string.IsNullOrEmpty(Apiuser.Area)))
		//        {
		//            if (Apiuser.AreaId == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.Business)))
		//        {
		//            if (Apiuser.BusinessId == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.Group)))
		//        {
		//            if (Apiuser.GroupId == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.Location)))
		//        {
		//            if (Apiuser.LocationId == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn1)))
		//        {
		//            if (Apiuser.ConfigurationColumn1Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn2)))
		//        {
		//            if (Apiuser.ConfigurationColumn2Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn3)))
		//        {
		//            if (Apiuser.ConfigurationColumn3Id == null)
		//            {
		//                return 2;
		//            }
		//        }
		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn4)))
		//        {
		//            if (Apiuser.ConfigurationColumn4Id == null)
		//            {
		//                return 2;
		//            }
		//        }
		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn5)))
		//        {
		//            if (Apiuser.ConfigurationColumn5Id == null)
		//            {
		//                return 2;
		//            }
		//        }
		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn6)))
		//        {
		//            if (Apiuser.ConfigurationColumn6Id == null)
		//            {
		//                return 2;
		//            }
		//        }


		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn7)))
		//        {
		//            if (Apiuser.ConfigurationColumn7Id == null)
		//            {
		//                return 2;
		//            }
		//        }


		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn8)))
		//        {
		//            if (Apiuser.ConfigurationColumn8Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn9)))
		//        {
		//            if (Apiuser.ConfigurationColumn9Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn10)))
		//        {
		//            if (Apiuser.ConfigurationColumn10Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn11)))
		//        {
		//            if (Apiuser.ConfigurationColumn11Id == null)
		//            {
		//                return 2;
		//            }
		//        }

		//        if (!(string.IsNullOrEmpty(Apiuser.ConfigurationColumn12)))
		//        {
		//            if (Apiuser.ConfigurationColumn12Id == null)
		//            {
		//                return 2;
		//            }
		//        }
		//        string mail = Security.Decrypt(Apiuser.EmailId);

		//        if (Apiuser.JobRoleId != null)
		//        {
		//            if (Apiuser.DateIntoRole == null)
		//            {
		//                Apiuser.DateIntoRole = DateTime.UtcNow;
		//            }
		//        }



		//        if (!Apiuser.IsAllConfigured)
		//        {
		//            return 2;
		//        }

		//        Apiuser.SerialNumber = Convert.ToString(await this.GetTotalUserCount() + 1);
		//        Apiuser.AccountCreatedDate = DateTime.UtcNow;
		//        Apiuser.CreatedDate = DateTime.UtcNow;
		//        Apiuser.ModifiedDate = DateTime.UtcNow;
		//        string DeafultPassword = this._configuration["DeafultPassword"];
		//        if (Apiuser.Password == null)
		//        {
		//            Apiuser.Password = Helper.Security.EncryptSHA512(this._configuration["DeafultPassword"]);

		//        }
		//        else
		//        {
		//            DeafultPassword = Apiuser.Password;
		//            Apiuser.Password = Helper.Security.EncryptSHA512(Apiuser.Password);

		//        }

		//        Apiuser.UserName = Apiuser.UserName;
		//        Apiuser.UserRole = "EU";
		//        Apiuser.UserType = Apiuser.UserType;
		//        Apiuser.UserSubType = Apiuser.UserSubType;
		//        Apiuser.MobileNumber = Security.Encrypt(MobileNumber);
		//        string EmailId1 = Security.Encrypt(toEmail.ToLower());
		//        Apiuser.UserId = Apiuser.UserId;
		//        Apiuser.ReportsTo = Security.Encrypt(Apiuser.ReportsTo == null ? null : Apiuser.ReportsTo.ToLower());
		//        Apiuser.Id = 0;
		//        bool Ldap = await this.IsLDAP();
		//        if (Ldap == true)
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


		//        Exists exist1 = await this.ValidationsDuplicateUseridForVFSSignUp(Apiuser, OrganisationString);
		//        if (!exist1.Equals(Exists.No))
		//            return 0;


		//        this.ChangeDbContext(OrganisationString);
		//        Apiuser.EmailId = EmailId1;
		//        APIVFSUserSignUp user = await this.AddUserToDbForVFSSignUp(Apiuser, UserRole);
		//        if (user.IsActive == true)
		//        {

		//            Task.Run(() => _email.NewUserAddedForVFS(toEmail, user.CustomerCode, user.UserName, user.MobileNumber, Security.Decrypt(user.EmailId), DeafultPassword, null, user.Id));


		//        }
		//        return 1;
		//    }
		//}
		//public async Task<Exists> ValidationsForUserVFSSignUp(APIVFSUserSignUp user)
		//{

		//    if (await this.IsExists(user.UserId))
		//        return Exists.UserIdExist;
		//    if (await this.EmailExists(user.EmailId))
		//        return Exists.EmailIdExist;
		//    if (await this.MobileExists(user.MobileNumber))
		//        return Exists.MobileExist;

		//    return Exists.No;
		//}

		//public async Task<Exists> ValidationsForUserVFSSignUp1(APIVFSUserSignUp user, string Orgcode)
		//{
		//    this.ChangeDbContext(Orgcode);
		//    user.EmailId = Security.Encrypt(user.EmailId.ToLower());
		//    user.MobileNumber = Security.Encrypt(user.MobileNumber);
		//    APIVFSExistsCheck aPIVFSExistsCheck = await this.GetVFSCount(user.MobileNumber, user.EmailId, Orgcode);
		//    if (aPIVFSExistsCheck == null)
		//    {
		//        return Exists.No;
		//    }
		//    if (aPIVFSExistsCheck != null)
		//    {
		//        if (aPIVFSExistsCheck.EmailId != null)
		//        {
		//            return Exists.EmailIdExist;
		//        }
		//        if (aPIVFSExistsCheck.MobileNumber != null)
		//        {
		//            return Exists.MobileExist;
		//        }
		//    }

		//    return Exists.No;
		//}

		//public async Task<Exists> ValidationsDuplicateUseridForVFSSignUp(APIVFSUserSignUp user, string Orgcode)
		//{
		//    this.ChangeDbContext(Orgcode);
		//    user.EmailId = Security.Encrypt(user.EmailId.ToLower());
		//    APIVFSExistsCheck aPIVFSExistsCheck = await this.GetVFSCount(user.MobileNumber, user.EmailId, Orgcode);
		//    if (aPIVFSExistsCheck == null)
		//    {
		//        return Exists.No;
		//    }
		//    if (aPIVFSExistsCheck != null)
		//    {
		//        if (aPIVFSExistsCheck.EmailId != null)
		//        {
		//            return Exists.EmailIdExist;
		//        }

		//    }


		//    return Exists.No;
		//}

		//public async Task<APIVFSUserSignUp> AddUserToDbForVFSSignUp(APIVFSUserSignUp apiUser, string UserRole)
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
		//                apiUser.Id = User.Id;
		//                await this._db.UserMasterDetails.AddAsync(UserDetails);
		//                await this._db.SaveChangesAsync();
		//                transaction.Commit();
		//            }
		//            else
		//            {
		//                transaction.Rollback();
		//            }

		//            return apiUser;
		//        }
		//        catch (System.Exception ex)
		//        {
		//            _logger.Error(Utilities.GetDetailedException(ex));
		//            transaction.Rollback();
		//            throw ex;
		//        }
		//    }
		//}

		//public async Task<APIVFSUserSignUp> GetUserObjectForVFSSignUp(APIVFSUserSignUp user, string UserRole, string orgCode)
		//{
		//    try
		//    {
		//        this.ChangeDbContext(orgCode);
		//        using (var dbContext = _customerConnectionString.GetDbContext(orgCode))
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {

		//                var lowerCaseAllow = await GetMasterConfigurableParameterValueByConnectionString("SAVE_USER_IMPORT_ASIS", orgCode);

		//                if (lowerCaseAllow.ToString() == "Yes")
		//                {
		//                    user.Location = user.Location == null ? null : user.Location.Trim();
		//                    user.Business = user.Business == null ? null : user.Business.Trim();
		//                    user.Group = user.Group == null ? null : user.Group.Trim();
		//                    user.Area = user.Area == null ? null : user.Area.Trim();
		//                    user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.Trim();
		//                    user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.Trim();
		//                    user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.Trim();
		//                    user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.Trim();
		//                    user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.Trim();
		//                    user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.Trim();
		//                    user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.Trim();
		//                    user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.Trim();
		//                    user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.Trim();
		//                    user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.Trim();
		//                    user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.Trim();
		//                    user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.Trim();
		//                    user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();
		//                }
		//                else
		//                {

		//                    user.Location = user.Location == null ? null : user.Location.ToLower().Trim();
		//                    user.Business = user.Business == null ? null : user.Business.ToLower().Trim();
		//                    user.Group = user.Group == null ? null : user.Group.Trim();
		//                    user.Area = user.Area == null ? null : user.Area.ToLower().Trim();
		//                    user.ConfigurationColumn1 = user.ConfigurationColumn1 == null ? null : user.ConfigurationColumn1.ToLower().Trim();
		//                    user.ConfigurationColumn2 = user.ConfigurationColumn2 == null ? null : user.ConfigurationColumn2.ToLower().Trim();
		//                    user.ConfigurationColumn3 = user.ConfigurationColumn3 == null ? null : user.ConfigurationColumn3.ToLower().Trim();
		//                    user.ConfigurationColumn4 = user.ConfigurationColumn4 == null ? null : user.ConfigurationColumn4.ToLower().Trim();
		//                    user.ConfigurationColumn5 = user.ConfigurationColumn5 == null ? null : user.ConfigurationColumn5.ToLower().Trim();
		//                    user.ConfigurationColumn6 = user.ConfigurationColumn6 == null ? null : user.ConfigurationColumn6.ToLower().Trim();
		//                    user.ConfigurationColumn7 = user.ConfigurationColumn7 == null ? null : user.ConfigurationColumn7.ToLower().Trim();
		//                    user.ConfigurationColumn8 = user.ConfigurationColumn8 == null ? null : user.ConfigurationColumn8.ToLower().Trim();
		//                    user.ConfigurationColumn9 = user.ConfigurationColumn9 == null ? null : user.ConfigurationColumn9.ToLower().Trim();
		//                    user.ConfigurationColumn10 = user.ConfigurationColumn10 == null ? null : user.ConfigurationColumn10.ToLower().Trim();
		//                    user.ConfigurationColumn11 = user.ConfigurationColumn11 == null ? null : user.ConfigurationColumn11.ToLower().Trim();
		//                    user.ConfigurationColumn12 = user.ConfigurationColumn12 == null ? null : user.ConfigurationColumn12.ToLower().Trim();
		//                    user.JobRoleName = user.JobRoleName == null ? null : user.JobRoleName.Trim();

		//                }




		//                cmd.CommandText = "GetIdByName";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@Location", SqlDbType.VarChar) { Value = user.Location });
		//                cmd.Parameters.Add(new SqlParameter("@Business", SqlDbType.VarChar) { Value = user.Business });
		//                cmd.Parameters.Add(new SqlParameter("@Group", SqlDbType.VarChar) { Value = user.Group });
		//                cmd.Parameters.Add(new SqlParameter("@Area", SqlDbType.VarChar) { Value = user.Area });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration1", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration2", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration3", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration4", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration5", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration6", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration7", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration8", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration9", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration10", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration11", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration12", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration1Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn1 == null ? null : Security.Encrypt(user.ConfigurationColumn1) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration2Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn2 == null ? null : Security.Encrypt(user.ConfigurationColumn2) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration3Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn3 == null ? null : Security.Encrypt(user.ConfigurationColumn3) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration4Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn4 == null ? null : Security.Encrypt(user.ConfigurationColumn4) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration5Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn5 == null ? null : Security.Encrypt(user.ConfigurationColumn5) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration6Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn6 == null ? null : Security.Encrypt(user.ConfigurationColumn6) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration7Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn7 == null ? null : Security.Encrypt(user.ConfigurationColumn7) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration8Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn8 == null ? null : Security.Encrypt(user.ConfigurationColumn8) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration9Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn9 == null ? null : Security.Encrypt(user.ConfigurationColumn9) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration10Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn10 == null ? null : Security.Encrypt(user.ConfigurationColumn10) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration11Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn11 == null ? null : Security.Encrypt(user.ConfigurationColumn11) });
		//                cmd.Parameters.Add(new SqlParameter("@Configuration12Encrypted", SqlDbType.VarChar) { Value = user.ConfigurationColumn12 == null ? null : Security.Encrypt(user.ConfigurationColumn12) });
		//                cmd.Parameters.Add(new SqlParameter("@AreaEncrypted", SqlDbType.VarChar) { Value = user.Area == null ? null : Security.Encrypt(user.Area) });
		//                cmd.Parameters.Add(new SqlParameter("@BuisnessEncrypted", SqlDbType.VarChar) { Value = user.Business == null ? null : Security.Encrypt(user.Business) });
		//                cmd.Parameters.Add(new SqlParameter("@GroupEncrypted", SqlDbType.VarChar) { Value = user.Group == null ? null : Security.Encrypt(user.Group) });
		//                cmd.Parameters.Add(new SqlParameter("@LocationEncrypted", SqlDbType.VarChar) { Value = user.Location == null ? null : Security.Encrypt(user.Location) });
		//                cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.VarChar) { Value = "10-11-2018" });
		//                cmd.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.VarChar) { Value = UserRole });
		//                cmd.Parameters.Add(new SqlParameter("@JobRoleName", SqlDbType.VarChar) { Value = user.JobRoleName });
		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count > 0)
		//                {
		//                    foreach (DataRow row in dt.Rows)
		//                    {
		//                        user.LocationId = string.IsNullOrEmpty(row["LocationId"].ToString()) ? (int?)null : Convert.ToInt32(row["LocationId"].ToString());
		//                        user.BusinessId = string.IsNullOrEmpty(row["BusinessId"].ToString()) ? (int?)null : Convert.ToInt32(row["BusinessId"].ToString());
		//                        user.GroupId = string.IsNullOrEmpty(row["GroupId"].ToString()) ? (int?)null : Convert.ToInt32(row["GroupId"].ToString());
		//                        user.AreaId = string.IsNullOrEmpty(row["AreaId"].ToString()) ? (int?)null : Convert.ToInt32(row["AreaId"].ToString());
		//                        user.ConfigurationColumn1Id = string.IsNullOrEmpty(row["Configuration1Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration1Id"].ToString());
		//                        user.ConfigurationColumn2Id = string.IsNullOrEmpty(row["Configuration2Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration2Id"].ToString());
		//                        user.ConfigurationColumn3Id = string.IsNullOrEmpty(row["Configuration3Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration3Id"].ToString());
		//                        user.ConfigurationColumn4Id = string.IsNullOrEmpty(row["Configuration4Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration4Id"].ToString());
		//                        user.ConfigurationColumn5Id = string.IsNullOrEmpty(row["Configuration5Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration5Id"].ToString());
		//                        user.ConfigurationColumn6Id = string.IsNullOrEmpty(row["Configuration6Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration6Id"].ToString());
		//                        user.ConfigurationColumn7Id = string.IsNullOrEmpty(row["Configuration7Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration7Id"].ToString());
		//                        user.ConfigurationColumn8Id = string.IsNullOrEmpty(row["Configuration8Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration8Id"].ToString());
		//                        user.ConfigurationColumn9Id = string.IsNullOrEmpty(row["Configuration9Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration9Id"].ToString());
		//                        user.ConfigurationColumn10Id = string.IsNullOrEmpty(row["Configuration10Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration10Id"].ToString());
		//                        user.ConfigurationColumn11Id = string.IsNullOrEmpty(row["Configuration11Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration11Id"].ToString());
		//                        user.ConfigurationColumn12Id = string.IsNullOrEmpty(row["Configuration12Id"].ToString()) ? (int?)null : Convert.ToInt32(row["Configuration12Id"].ToString());
		//                        user.HouseId = string.IsNullOrEmpty(row["HouseId"].ToString()) ? (int?)null : Convert.ToInt32(row["HouseId"].ToString());
		//                        user.IsAllConfigured = Convert.ToBoolean(row["IsAllConfigured"].ToString());
		//                        user.JobRoleId = string.IsNullOrEmpty(row["JobRoleId"].ToString()) ? (int?)null : Convert.ToInt32(row["JobRoleId"].ToString());

		//                    }
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//    return user;
		//}
		//public async Task<APIVFSExistsCheck> GetVFSCount(string mobileNumber, string EmailId, string OrgnizationConnectionString)
		//{
		//    this.ChangeDbContext(OrgnizationConnectionString);

		//    APIVFSExistsCheck aPIVFSExistsCheck = new APIVFSExistsCheck();
		//    try
		//    {

		//        using (var dbContext = _customerConnectionString.GetDbContext(OrgnizationConnectionString))
		//        {
		//            var connection = dbContext.Database.GetDbConnection();
		//            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
		//                connection.Open();
		//            using (var cmd = connection.CreateCommand())
		//            {
		//                cmd.CommandText = "MobileNumberEmailIdExistCheck";
		//                cmd.CommandType = CommandType.StoredProcedure;
		//                cmd.Parameters.Add(new SqlParameter("@MobileNumber", SqlDbType.VarChar) { Value = mobileNumber });
		//                cmd.Parameters.Add(new SqlParameter("@EmailId", SqlDbType.VarChar) { Value = EmailId });


		//                DbDataReader reader = await cmd.ExecuteReaderAsync();
		//                DataTable dt = new DataTable();
		//                dt.Load(reader);
		//                if (dt.Rows.Count <= 0)
		//                {
		//                    reader.Dispose();
		//                    connection.Close();
		//                    return null;
		//                }
		//                foreach (DataRow row in dt.Rows)
		//                {
		//                    aPIVFSExistsCheck.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? "null" : (row["MobileNumber"].ToString());
		//                    aPIVFSExistsCheck.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? "null" : (row["EmailId"].ToString());
		//                }
		//                reader.Dispose();
		//            }
		//            connection.Close();
		//        }
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//    return aPIVFSExistsCheck;
		//}

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

		//public async Task<List<string>> GetRegion()
		//{
		//    return await _db.Configure11.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
		//}
		//public async Task<List<string>> GetDepartment()
		//{
		//    return await _db.Configure9.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
		//}
		public async Task<string> GetEmailByUserId(string UserId)
		{
			return await (from um in _db.UserMaster
						  where um.UserId == Security.Encrypt(UserId)
						  select um.EmailId).SingleOrDefaultAsync();
		}
		//public async Task<int> PostTeamsCred(ApiTeamsCred apiTeamsCred, int userid)
		//{
		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "TEAMS").FirstOrDefaultAsync();
		//    UserWebinarMaster userWebinar = _db.UserWebinarMaster.Where(a => a.TeamsEmail == apiTeamsCred.TeamsEmail && a.isDeleted == 0).FirstOrDefault();
		//    if (userWebinar != null)
		//    {
		//        return 0;
		//    }
		//    else
		//    {
		//        UserWebinarMaster userWebinarMaster = new UserWebinarMaster();
		//        userWebinarMaster.CreatedBy = userid;
		//        userWebinarMaster.TeamsEmail = apiTeamsCred.TeamsEmail;
		//        userWebinarMaster.Username = apiTeamsCred.Username;
		//        userWebinarMaster.Password = apiTeamsCred.Password;
		//        userWebinarMaster.ModifiedDate = DateTime.UtcNow;
		//        userWebinarMaster.isDeleted = 0;
		//        userWebinarMaster.isDefault = apiTeamsCred.defaultAccount;
		//        userWebinarMaster.CreatedDate = DateTime.UtcNow;
		//        userWebinarMaster.WebinarId = configurableValues.ID;

		//        await this._db.UserWebinarMaster.AddAsync(userWebinarMaster);
		//        await this._db.SaveChangesAsync();
		//    }

		//    return 1;
		//}
		//public async Task<ApiResponse> GetDefaultTeamsCrediential()
		//{
		//    ApiResponse obj = new ApiResponse();

		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "TEAMS").FirstOrDefaultAsync();
		//    var Query = await (from UserWebinarMasters in this._db.UserWebinarMaster
		//                       where UserWebinarMasters.isDeleted == 0 && UserWebinarMasters.isDefault == 1 && UserWebinarMasters.WebinarId == configurableValues.ID
		//                       select new { UserWebinarMasters.Id, UserWebinarMasters.Username, UserWebinarMasters.TeamsEmail, UserWebinarMasters.isDefault }).Distinct().ToListAsync();

		//    obj.ResponseObject = Query;
		//    return obj;
		//}
		//public async Task<int> EditTeamscred(ApiTeamsCred apiTeamsCred, int userid)
		//{
		//    if (apiTeamsCred == null)
		//    {
		//        return 0;
		//    }
		//    if (apiTeamsCred.TeamsEmail == null || apiTeamsCred.Password == null)
		//    {
		//        return 0;
		//    }
		//    try
		//    {
		//        UserWebinarMaster userWebinar = null;
		//        UserWebinarMaster userWebinar1 = null;
		//        ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "TEAMS").FirstOrDefaultAsync();
		//        if (apiTeamsCred.defaultAccount == 0)
		//        {
		//            userWebinar1 = await (from user in this._db.UserWebinarMaster
		//                                  where user.isDeleted == 0 && user.TeamsEmail == apiTeamsCred.TeamsEmail && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                  select user
		//                        ).FirstOrDefaultAsync();
		//            if (userWebinar1 == null)
		//            {
		//                userWebinar = await (from user in this._db.UserWebinarMaster
		//                                     where user.isDeleted == 0 && user.CreatedBy == userid && user.isDefault == 0 && user.WebinarId == configurableValues.ID
		//                                     select user
		//                        ).FirstOrDefaultAsync();
		//            }
		//            else
		//            {
		//                return 0;
		//            }
		//        }
		//        else
		//        {
		//            userWebinar = await (from user in this._db.UserWebinarMaster
		//                                 where user.isDeleted == 0 && user.TeamsEmail == apiTeamsCred.TeamsEmail && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                 select user
		//                        ).FirstOrDefaultAsync();
		//        }
		//        if (userWebinar == null)
		//        {
		//            return 0;
		//        }
		//        userWebinar.TeamsEmail = apiTeamsCred.TeamsEmail;
		//        userWebinar.Password = apiTeamsCred.Password;
		//        userWebinar.Username = apiTeamsCred.Username;
		//        this._db.UserWebinarMaster.Update(userWebinar);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<int> DeleteTeamsUser(string TeamsEmail)
		//{
		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "TEAMS").FirstOrDefaultAsync();
		//    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMaster.Where(u => u.isDeleted == 0 && u.TeamsEmail == TeamsEmail && u.WebinarId == configurableValues.ID).FirstOrDefault();
		//    if (userWebinarMaster == null)
		//        return 0;
		//    userWebinarMaster.isDeleted = 1;
		//    this._db.UserWebinarMaster.Update(userWebinarMaster);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

		//public async Task<int> PostZoomCred(ApiZoomCred apiZoomCred, int userid)
		//{
		//    try
		//    {
		//        ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "zoom").FirstOrDefaultAsync();
		//        apiZoomCred.ZoomEmail = Security.DecryptForUI(apiZoomCred.ZoomEmail);
		//        UserWebinarMaster userWebinar = _db.UserWebinarMaster.Where(a => a.TeamsEmail == apiZoomCred.ZoomEmail && a.isDeleted == 0 && a.WebinarId == configurableValues.ID).FirstOrDefault();
		//        if (userWebinar != null)
		//        {
		//            return 0;
		//        }
		//        else
		//        {
		//            UserWebinarMaster userWebinarMaster = new UserWebinarMaster();
		//            userWebinarMaster.CreatedBy = userid;
		//            userWebinarMaster.TeamsEmail = apiZoomCred.ZoomEmail;
		//            userWebinarMaster.Username = apiZoomCred.Username;
		//            userWebinarMaster.Password = null;
		//            userWebinarMaster.ModifiedDate = DateTime.UtcNow;
		//            userWebinarMaster.isDeleted = 0;
		//            userWebinarMaster.isDefault = apiZoomCred.DefaultAccount;
		//            userWebinarMaster.CreatedDate = DateTime.UtcNow;
		//            userWebinarMaster.WebinarId = configurableValues.ID;

		//            await this._db.UserWebinarMaster.AddAsync(userWebinarMaster);
		//            await this._db.SaveChangesAsync();
		//        }

		//        return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<ApiResponse> GetZoomCrediential(int UserId)
		//{
		//    ApiResponse obj = new ApiResponse();

		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "zoom").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        return new ApiResponse();
		//    }
		//    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMaster.Where(a => a.isDeleted == 0 && (a.isDefault == 0 && a.CreatedBy == UserId && a.WebinarId == configurableValues.ID)).FirstOrDefault();
		//    if (userWebinarMaster == null)
		//    {
		//        return new ApiResponse();
		//    }
		//    ApiGetZoomCred apiGetZoomCred = new ApiGetZoomCred();
		//    apiGetZoomCred.TeamsEmail = Security.EncryptForUI(userWebinarMaster.TeamsEmail);
		//    apiGetZoomCred.Id = userWebinarMaster.Id;
		//    apiGetZoomCred.Username = userWebinarMaster.Username;
		//    obj.ResponseObject = apiGetZoomCred;
		//    return obj;
		//}
		//public async Task<ApiResponse> GetDefaultZoomCrediential()
		//{
		//    ApiResponse obj = new ApiResponse();

		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "zoom").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        obj.StatusCode = 404;
		//        obj.Description = "Not Found";
		//        return obj;
		//    }
		//    var Query = await (from UserWebinarMasters in this._db.UserWebinarMaster
		//                       where UserWebinarMasters.isDeleted == 0 && UserWebinarMasters.isDefault == 1 && UserWebinarMasters.WebinarId == configurableValues.ID
		//                       select new { UserWebinarMasters.Id, UserWebinarMasters.Username, UserWebinarMasters.TeamsEmail, UserWebinarMasters.isDefault }).Distinct().ToListAsync();
		//    List<ApiGetZoomCred> apiGetZoomCred = new List<ApiGetZoomCred>();
		//    for (int i = 0; i < Query.Count; i++)
		//    {
		//        ApiGetZoomCred apiGetZoomCred1 = new ApiGetZoomCred();
		//        apiGetZoomCred1.Id = Query[i].Id;
		//        apiGetZoomCred1.isDefault = Query[i].isDefault;
		//        apiGetZoomCred1.TeamsEmail = Security.EncryptForUI(Query[i].TeamsEmail);
		//        apiGetZoomCred1.Username = Query[i].Username;
		//        apiGetZoomCred.Add(apiGetZoomCred1);
		//    }
		//    obj.ResponseObject = apiGetZoomCred;
		//    return obj;
		//}
		//public async Task<int> EditZoomcred(ApiZoomCred apiZoomCred, int userid)
		//{
		//    if (apiZoomCred == null)
		//    {
		//        return 0;
		//    }
		//    if (apiZoomCred.ZoomEmail == null)
		//    {
		//        return 0;
		//    }
		//    try
		//    {
		//        UserWebinarMaster userWebinar = null;
		//        UserWebinarMaster userWebinar1 = null;
		//        ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "zoom").FirstOrDefaultAsync();
		//        if (apiZoomCred.DefaultAccount == 0)
		//        {
		//            userWebinar1 = await (from user in this._db.UserWebinarMaster
		//                                  where user.isDeleted == 0 && user.TeamsEmail == apiZoomCred.ZoomEmail && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                  select user
		//                        ).FirstOrDefaultAsync();
		//            if (userWebinar1 == null)
		//            {
		//                userWebinar = await (from user in this._db.UserWebinarMaster
		//                                     where user.isDeleted == 0 && user.CreatedBy == userid && user.isDefault == 0 && user.WebinarId == configurableValues.ID
		//                                     select user
		//                        ).FirstOrDefaultAsync();
		//            }
		//            else
		//            {
		//                return 0;
		//            }
		//        }
		//        else
		//        {
		//            userWebinar = await (from user in this._db.UserWebinarMaster
		//                                 where user.isDeleted == 0 && user.TeamsEmail == apiZoomCred.ZoomEmail && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                 select user
		//                        ).FirstOrDefaultAsync();
		//        }
		//        if (userWebinar == null)
		//        {
		//            return 0;
		//        }
		//        userWebinar.TeamsEmail = apiZoomCred.ZoomEmail;
		//        userWebinar.Password = null;
		//        userWebinar.Username = apiZoomCred.Username;
		//        this._db.UserWebinarMaster.Update(userWebinar);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<int> DeleteZoomUser(string ZoomEmail)
		//{
		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "zoom").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        return 0;
		//    }
		//    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMaster.Where(u => u.isDeleted == 0 && u.TeamsEmail == ZoomEmail && u.WebinarId == configurableValues.ID).FirstOrDefault();
		//    if (userWebinarMaster == null)
		//    {
		//        return 0;
		//    }
		//    userWebinarMaster.isDeleted = 1;
		//    this._db.UserWebinarMaster.Update(userWebinarMaster);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

		//public async Task<IEnumerable<ApiUserSearchV2>> SearchAllUser(string userId, string UserType = null)
		//{
		//    try
		//    {
		//        var result = (from user in this._db.UserMaster
		//                      join userDetails in this._db.UserMasterDetails on user.Id equals userDetails.UserMasterId
		//                      where ((user.UserId == Security.Encrypt(userId.ToLower())
		//                      || user.UserName.ToLower().StartsWith(userId.ToLower()))
		//                      && user.IsDeleted == false )
		//                      select new ApiUserSearchV2
		//                      {
		//                          UserId = Security.EncryptForUI(Security.Decrypt(user.UserId)),
		//                          Name = user.UserName,
		//                          EmailId = Security.EncryptForUI(Security.Decrypt(user.EmailId)),
		//                          Id = (Security.EncryptForUI(user.Id.ToString())),
		//                          ProfilePicture = userDetails.ProfilePicture,
		//                          MobileNumber = Security.EncryptForUI(Security.Decrypt(userDetails.MobileNumber)),
		//                          UserType = userDetails.UserType,
		//                          IsDeleted = userDetails.IsDeleted

		//                      });

		//        if (UserType != null)
		//        {
		//            result = result.Where(a => a.UserType == UserType);
		//        }

		//        result = result.Where(v => v.IsDeleted == false);

		//        return await result.AsNoTracking().ToListAsync();
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//    }
		//    return null;
		//}

		//public async Task<ApiResponse> GetDefaultGSuitCredientials(int UserId)
		//{

		//    ApiResponse obj = new ApiResponse();

		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "GoogleMeet").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        obj.StatusCode = 404;
		//        obj.Description = "Not Found";
		//        return obj;
		//    }
		//    var Query = await (from UserWebinarMasters in this._db.UserWebinarMaster
		//                       where UserWebinarMasters.isDeleted == 0 && UserWebinarMasters.isDefault == 1 && UserWebinarMasters.WebinarId == configurableValues.ID
		//                       select new { UserWebinarMasters.Id, UserWebinarMasters.Username, UserWebinarMasters.TeamsEmail, UserWebinarMasters.isDefault }).Distinct().ToListAsync();
		//    List<ApiGsuitCred> apiGetGsuitCred = new List<ApiGsuitCred>();
		//    for (int i = 0; i < Query.Count; i++)
		//    {
		//        ApiGsuitCred apiGetGsuitCred1 = new ApiGsuitCred();
		//        apiGetGsuitCred1.Id = Query[i].Id;
		//        apiGetGsuitCred1.DefaultAccount = Query[i].isDefault;
		//        apiGetGsuitCred1.Email = Query[i].TeamsEmail;
		//        apiGetGsuitCred1.Username = Query[i].Username;
		//        apiGetGsuitCred.Add(apiGetGsuitCred1);
		//    }
		//    obj.ResponseObject = apiGetGsuitCred;
		//    return obj; 
		//}
		//public async Task<ApiResponse> GetGsuitCrediential(int UserId)
		//{
		//    ApiResponse obj = new ApiResponse();

		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode == "GoogleMeet").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        return new ApiResponse();
		//    }
		//    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMaster.Where(a => a.isDeleted == 0 && (a.isDefault == 0 && a.CreatedBy == UserId && a.WebinarId == configurableValues.ID)).FirstOrDefault();
		//    if (userWebinarMaster == null)
		//    {
		//        return new ApiResponse();
		//    }
		//    ApiGsuitCred apiGetGsuitCred = new ApiGsuitCred();
		//    apiGetGsuitCred.Email = userWebinarMaster.TeamsEmail;
		//    apiGetGsuitCred.Id = userWebinarMaster.Id;
		//    apiGetGsuitCred.Username = userWebinarMaster.Username;
		//    obj.ResponseObject = apiGetGsuitCred;
		//    return obj;
		//}

		//public async Task<int> PostGsuitCred(ApiGsuitCred apiGsuitCred, int userid)
		//{
		//    try
		//    {
		//        ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode.ToLower() == "googlemeet").FirstOrDefaultAsync();
		//        apiGsuitCred.Email = Security.DecryptForUI(apiGsuitCred.Email);
		//        UserWebinarMaster userWebinar = _db.UserWebinarMaster.Where(a => a.TeamsEmail == apiGsuitCred.Email && a.isDeleted == 0 && a.WebinarId == configurableValues.ID).FirstOrDefault();
		//        if (userWebinar != null)
		//        {
		//            return 0;
		//        }
		//        else
		//        {
		//            UserWebinarMaster userWebinarMaster = new UserWebinarMaster();
		//            userWebinarMaster.CreatedBy = userid;
		//            userWebinarMaster.TeamsEmail = apiGsuitCred.Email;
		//            userWebinarMaster.Username = apiGsuitCred.Username;
		//            userWebinarMaster.Password = null;
		//            userWebinarMaster.ModifiedDate = DateTime.UtcNow;
		//            userWebinarMaster.isDeleted = 0;
		//            userWebinarMaster.isDefault = apiGsuitCred.DefaultAccount;
		//            userWebinarMaster.CreatedDate = DateTime.UtcNow;
		//            userWebinarMaster.WebinarId = configurableValues.ID;

		//            await this._db.UserWebinarMaster.AddAsync(userWebinarMaster);
		//            await this._db.SaveChangesAsync();
		//        }

		//        return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<int> EditGuitcred(ApiGsuitCred apiGsuitCred, int userid)
		//{
		//    if (apiGsuitCred == null)
		//    {
		//        return 0;
		//    }
		//    if (apiGsuitCred.Email == null)
		//    {
		//        return 0;
		//    }
		//    try
		//    {
		//        UserWebinarMaster userWebinar = null;
		//        UserWebinarMaster userWebinar1 = null;
		//        ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode.ToLower() == "googlemeet").FirstOrDefaultAsync();
		//        if (apiGsuitCred.DefaultAccount == 0)
		//        {
		//            userWebinar1 = await (from user in this._db.UserWebinarMaster
		//                                  where user.isDeleted == 0 && user.TeamsEmail == apiGsuitCred.Email && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                  select user
		//                        ).FirstOrDefaultAsync();
		//            if (userWebinar1 == null)
		//            {
		//                userWebinar = await (from user in this._db.UserWebinarMaster
		//                                     where user.isDeleted == 0 && user.CreatedBy == userid && user.isDefault == 0 && user.WebinarId == configurableValues.ID
		//                                     select user
		//                        ).FirstOrDefaultAsync();
		//            }
		//            else
		//            {
		//                return 0;
		//            }
		//        }
		//        else
		//        {
		//            userWebinar = await (from user in this._db.UserWebinarMaster
		//                                 where user.isDeleted == 0 && user.TeamsEmail == apiGsuitCred.Email && user.isDefault == 1 && user.WebinarId == configurableValues.ID
		//                                 select user
		//                        ).FirstOrDefaultAsync();
		//        }
		//        if (userWebinar == null)
		//        {
		//            return 0;
		//        }
		//        userWebinar.TeamsEmail = apiGsuitCred.Email;
		//        userWebinar.Password = null;
		//        userWebinar.Username = apiGsuitCred.Username;
		//        this._db.UserWebinarMaster.Update(userWebinar);
		//        await this._db.SaveChangesAsync();
		//        return 1;
		//    }
		//    catch (System.Exception ex)
		//    {
		//        _logger.Error(Utilities.GetDetailedException(ex));
		//        throw ex;
		//    }
		//}
		//public async Task<int> DeleteGsuitUser(string Email)
		//{
		//    ConfigurableValues configurableValues = await this._db.ConfigurableValues.Where(x => x.ValueCode.ToLower() == "googlemeet").FirstOrDefaultAsync();
		//    if (configurableValues == null)
		//    {
		//        return 0;
		//    }
		//    UserWebinarMaster userWebinarMaster = this._db.UserWebinarMaster.Where(u => u.isDeleted == 0 && u.TeamsEmail == Email && u.WebinarId == configurableValues.ID).FirstOrDefault();
		//    if (userWebinarMaster == null)
		//    {
		//        return 0;
		//    }
		//    userWebinarMaster.isDeleted = 1;
		//    this._db.UserWebinarMaster.Update(userWebinarMaster);
		//    await this._db.SaveChangesAsync();
		//    return 1;
		//}

	}
}
