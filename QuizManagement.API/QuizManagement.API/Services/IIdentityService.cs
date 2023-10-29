namespace QuizManagement.API.Services
{
    public interface IIdentityService
    {
        string GetOrgCode();
        string GetUserIdentity();
        string GetUserId();
        string GetConnectionString();
        string GetCustomerCode();
        string GetUserRole();
        string GetToken();
        string GetSubOrganization();
        string GetTheme();
        string GetLogoname();
        string GetLogonameDark();
        string GetStatusCode();
        string GetStatusCodeMessage();
        string GetiOS();
        string GetLDAP();
        string GetExternalUser();
        string GetFCMToken();
       
    }
}
