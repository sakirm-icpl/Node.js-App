using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;

namespace User.API.Repositories.Interfaces
{
    public interface IEmail
    {
        int SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null);
        int SendEmailtoUser1(string toEmail, string orgCode, string UserName, string MobileNumber, string UserId, string Password);

        //int NewUserAdded(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, string password,string SendSMSToUser, int Id);

        int UserSignUpMailManager(string orgCode, string userName, string UserId, string toEmail, string EmailId, string mobileNumber);

        int NewUserAdded_Import(string toEmail, string orgCode, string userName, string mobileNumber, string UserID,string DeafultPassword);
        int SendApplicableCoursesEmail(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, int Id,bool IsActive);
        int SendOtpEmailToUser(string toEmail, string orgCode, string Otp);
        int NewUserAddedForVFS(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, string password, string SendSMSToUser, int Id);
        int UserSignUpMailToNodalOfficers(List<APINodalUserDetails> aPINodalUserDetailsList);
        int UserCreationMailByNodalOfficerToUser(List<APINodalUserDetails> aPINodalUserDetailsList);
        int GroupRequestMailToNodalOfficers(APIGroupEmails aPIGroupEmails);
        int PaymentSuccessfulMailToUser(APIPaymentMailDetails aPIPaymentMailDetails);
        Task GroupAdminSelfRegistrationMail(APINodalUser aPINodalUser);
    }
}
