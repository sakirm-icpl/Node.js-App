using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IAccessibilityRule : IRepository<AccessibilityRule>
    {
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);

    }
}
