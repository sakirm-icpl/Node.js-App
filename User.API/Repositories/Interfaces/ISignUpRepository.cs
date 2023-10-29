using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface ISignUpRepository : IRepository<Signup>
    {
        bool Exists(string userId);
        bool EmailExists(string emailTd);
        bool MobileExists(string mobileNumber);
        Task<int> GetUserCount(string search = null);
        Task<IEnumerable<Signup>> GetAllUser(int page, int pageSize, string search = null);
        bool UserNameExists(string userName);
        bool ActivationEmailNotExists(string emailTd);
        Task<APISignup> SignUpEmail();
        Task<ApiResponse> GetOrganization(string search = null);
        Task<int> SendEmailtoUser(APIUserSignUp Apiuser, string orgCode, string Password);
        bool ExistsUserSignUp(string userId);
        bool EmailExistsForUserSignUp(string emailTd);
        bool MobileExistsForUserSignUp(string mobileNumber);
        Task<int> SendOtpEmailToUser(string EmailId, string orgCode, string Otp);
    }
}
