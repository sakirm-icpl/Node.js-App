namespace TNA.API.Services
{
    public interface IIdentityService
    {
        string GetUserIdentity();
        string GetUserId();
        string GetConnectionString();
        string GetCustomerCode();
        string GetOrgCode();
        string GetUserName();
        string GetToken();
        string GetRoleCode();
        string GetiOS();
        string GetIsExternal();
        string GetIsInstitute();
    }
}
