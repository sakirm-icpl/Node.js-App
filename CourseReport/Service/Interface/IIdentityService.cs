namespace CourseReport.API.Service
{
    public interface IIdentityService
    {
        string GetUserIdentity();
        string GetUserId();
        string GetConnectionString();
        string GetCustomerCode();
        string GetUserRole();
        string GetOrgCode();
        string GetToken();
    }
}
