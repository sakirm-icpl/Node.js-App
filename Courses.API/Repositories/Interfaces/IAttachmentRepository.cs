using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IAttachmentRepository : IRepository<Attachment>
    {
        Task<bool> Exists(string name);
        Task<List<Attachment>> Get(int page, int pageSize, string search = null, string filter = null);
        Task<int> count(string search = null, string filter = null);
        Task<int> AddAttachment(Attachment attach);
    }
}
