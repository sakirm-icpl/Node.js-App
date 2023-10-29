using Survey.API.APIModel;
using System.Threading.Tasks;

namespace Survey.API.Repositories.Interfaces
{
    public interface INotification
    {
        Task<int> SendNotification(ApiNotification notification, string token = null);
        Task<int> SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null);
    }
}
