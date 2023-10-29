using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface ILoggedInHistoryRepository : IRepository<LoggedInHistory>
    {
        Task<LoggedInHistory> GetLatestRecord(int userId);
        Task<int> AlreadyLoggedInForDay(int userId);

        Task<int> FirstTimeLoggedIn(int userId);
    }
}
