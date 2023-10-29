using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IExportFileLog : IRepository<ExportFileLog>
    {
        void ChangeDbContext(string connectionString);
    }
}
