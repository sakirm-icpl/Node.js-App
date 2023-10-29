using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IAccessibilityRule : IRepository<AccessibilityRule>
    {
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);

    }
}
