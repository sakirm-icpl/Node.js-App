using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using log4net;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class SignUpRepository : Repository<Signup>, ISignUpRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SignUpRepository));
        private UserDbContext _db;
        private IConfiguration _configuration;
        private string mailSubject;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IEmail _email;
        public SignUpRepository(UserDbContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionString, IEmail email) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._customerConnectionString = customerConnectionString;
            this._email = email;
        }
        public bool Exists(string userId)
        {
            if (this._db.Signup.Count(x => x.UserId == userId) > 0)
                return true;
            return false;
        }
        public bool UserNameExists(string userName)
        {
            if (this._db.Signup.Count(x => x.UserName == userName) > 0)
                return true;
            return false;
        }
        public bool ActivationEmailNotExists(string emailId)
        {
            if (this._db.UserMaster.Where(users => String.Equals(users.EmailId, Security.Encrypt(emailId.ToLower())) && users.IsDeleted == false).Select(u => u.Id).Count() > 0)
                return false;
            return true;
        }
        public bool EmailExists(string emailId)
        {
            if (this._db.Signup.Where(users => String.Equals(users.EmailId, emailId.ToLower()) && users.IsDeleted == 0).Select(u => u.Id).Count() > 0)
                return true;
            return false;
        }
        public bool MobileExists(string mobileNumber)
        {
            if (this._db.Signup.Count(x => x.MobileNumber == mobileNumber) > 0)
                return true;
            return false;
        }
        public async Task<int> GetUserCount(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.Signup.Where(u => u.IsDeleted == Record.NotDeleted && u.UserName.StartsWith(search)).CountAsync();
            return await this._db.Signup.Where(u => u.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<IEnumerable<Signup>> GetAllUser(int page, int pageSize, string search = null)
        {
            try
            {

                var users = (from user in this._db.Signup
                             where user.IsDeleted == 0
                             select user);

                if (!string.IsNullOrEmpty(search))
                    users = users.Where(u => u.UserName.StartsWith(search));

                if (page != -1)
                {
                    users = users.Skip((page - 1) * pageSize);
                    users = users.OrderByDescending(v => v.Id);
                }

                if (pageSize != -1)
                {
                    users = users.Take(pageSize);
                    users = users.OrderByDescending(v => v.Id);
                }

                return await users.AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<APISignup> SignUpEmail()
        {
            mailSubject = this._configuration[Configuration.UserActivate];
            try
            {
                var result = (from mailTemplateDesigner in this._db.MailTemplateDesigner
                              where (mailTemplateDesigner.MailSubject == "Use Activted OR Deactivated")
                              select new APISignup
                              {
                                  CustomerCode = mailTemplateDesigner.CustomerCode,
                                  Subject = mailTemplateDesigner.MailSubject,
                                  Message = mailTemplateDesigner.TemplateContent
                              });
                return await result.SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<ApiResponse> GetOrganization(string search = null)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                List<object> OrganizationList = new List<object>();
                var connectionString = this._configuration.GetConnectionString("defaultmasterconnection");
                using (var dbContext = _customerConnectionString.GetDbContext(connectionString))
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetOrganizations";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.VarChar) { Value = search });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var Organization = new
                                {
                                    OrganizationCode = row["Code"].ToString(),
                                    Name = row["Name"].ToString(),
                                };
                                OrganizationList.Add(Organization);
                            }
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
                Response.ResponseObject = OrganizationList;
                Response.StatusCode = 200;
                Response.Description = "success";
                return Response;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> SendEmailtoUser(APIUserSignUp Apiuser, string orgCode, string Password)
        {


            string Email = string.Empty;
            string ToEmail = string.Empty;
            string Subject = string.Empty;
            string Message = string.Empty;
            Task.Run(() => _email.SendEmailtoUser1(Security.Decrypt(Apiuser.EmailId), orgCode, Apiuser.UserName, Security.Decrypt(Apiuser.MobileNumber), Security.Decrypt(Apiuser.UserId), Password));

            return 1;
        }
        public bool ExistsUserSignUp(string userId)
        {
            if (this._db.UserSignUp.Count(x => x.UserId == userId) > 0)
                return true;
            return false;
        }
        public bool EmailExistsForUserSignUp(string emailId)
        {
            if (this._db.UserSignUp.Where(users => String.Equals(users.EmailId, emailId.ToLower()) && users.IsDeleted == 0).Select(u => u.Id).Count() > 0)
                return true;
            return false;
        }
        public bool MobileExistsForUserSignUp(string mobileNumber)
        {
            if (this._db.UserSignUp.Count(x => x.MobileNumber == mobileNumber) > 0)
                return true;
            return false;
        }

        public async Task<APIUserSignUp> AddAccessibilityRule(APIUserSignUp signUpOTP)
        {
            SignUpOTP signUp = new SignUpOTP();
            signUp.EmpCode = signUpOTP.UserId;
            signUp.OTP = signUpOTP.Otp;
            signUp.CreatedDate = DateTime.Now;
            this._db.SignUpOTP.Add(signUp);
            await this._db.SaveChangesAsync();
            return (signUpOTP);

        }

        public async Task<int> SendOtpEmailToUser(string EmailId, string orgCode, string Otp)
        {


            string Email = string.Empty;
            string ToEmail = string.Empty;
            string Subject = string.Empty;
            string Message = string.Empty;
            Task.Run(() => _email.SendOtpEmailToUser(EmailId, orgCode, Otp));

            return 1;
        }
    }
}
