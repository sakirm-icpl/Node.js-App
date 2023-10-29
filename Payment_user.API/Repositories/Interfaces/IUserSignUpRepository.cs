using Payment.API.APIModel;
using Payment.API.Models;
using Payment.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Repositories.Interfaces
{
    public interface IUserSignUpRepository : IRepository<UserSignUp>
    {
        void ChangeDbContext(string connectionString);
        bool ExistsUserSignUp(string userId);
        bool EmailExistsForUserSignUp(string emailId);
        bool MobileExistsForUserSignUp(string mobileNumber);
        //Task<int> SendEmailtoUser(APIUserMaster Apiuser, string orgCode, string Password);
        //Task<int> ExistsOTP(string empcode, string otp, string customercode, string OrganisationString);
        ////Task<List<APIUserSignUpTypeAhead>> GetVFSSignUp(APISignUpTypeAhead aPISignUpTypeAhead, string OrganisationConnectionString);
        //Task<IEnumerable<APIUserSetting>> GetVFSSettings(string OrganisationString = null);
        //Task<APIUserSignUp> AddUserSignUp(APIUserSignUp signUpOTP, string OrganisationString);
        //Task<int> getdataforVFS(string empcode, string otp, string customercode, string OrganisationString);
        //Task<bool> ExistsOTPForVFS(string empcode, string otp, string customercode, string OrganisationString);
        //Task<List<APIAirportInfo>> GetAirports(string ConnectionString);
        //Task<int> TTUserSignUp(APITTUserSignUp aPITTUserSignUp, string ConnectionString);
        //Task<int> NodalUserSignUp(APINodalUserSignUp aPINodalUserSignUp, string ConnectionString);
        Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string ConnectionString, string defaultValue = "");
        //Task<int> CreateNodalUser(int UserId, APINodalUserSignUp aPINodalUserSignUp, string ConnectionString);
        Task<string> MakePayment(int RequestId, string UserIds, string ConnectionString, string OrgCode);
        Task<PaymentResponseMessage> ProcessResponse(string merchantParamsJson, string ConnectionString, string OrgCode);
        Task<int> CheckRequest(int RequestId, string UserIds, string ConnectionString);
        //Task<ApiResponse> GetOrganizationDetailsTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString);
        //Task<ApiResponse> GetBillingAddressTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString);
        //Task<int> GroupAdminSignUp(APIGroupAdminSignUp aPIGroupAdminSignUp, string ConnectionString);
        Task<PaymentStatusResponse> GetStatus(PaymentStatusRequest paymentStatusRequest, string ConnectionString);
        //Task<APIDhangyanUserSignUpResponse> DhangyanSignUp(APIDhangyanUserSignUp aPIDhangyanUserSignUp, string ConnectionString);
        //Task<List<APIAirportInfo>> GetStates(string ConnectionString);
        //Task<List<APIAirportInfo>> GetOrganizations(APIDhangyanUserTypeAhead aPIDhangyanUserTypeAhead, string ConnectionString);
        //Task<ApiResponse> GetOrganizationIDTypeahead(APINodalUserTypeAhead aPINodalUserTypeAhead, string ConnectionString);
        //Task<APIDhangyanUserSignUpResponse> DhangyanSchoolSignUp(APISchoolDhangyanSignUp aPIDhangyanUserSignUp, string ConnectionString);

        Task<PaymentResponseMessage> ProcessPaymentResponse(TransactionRequest transactionResponse, string ConnectionString, string OrgCode);
        TransactionRequest DecryptTransactionRespone(string merchantParamsJson);

    }
}
