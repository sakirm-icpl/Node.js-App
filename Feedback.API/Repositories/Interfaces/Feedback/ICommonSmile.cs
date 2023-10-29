using Courses.API.APIModel.Feedback;
using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface ICommonSmile : IRepository<CommonSmileSheet>
    {
        int Count(string? search = null, string? filter = null);
        bool Exists(string name);
        List<CommonSmileSheet> Get(int page, int pageSize, string? search = null, string? filter = null);
        List<APISmileSheetSectionTypeAhead> GetSectionTypehead(string? search = null);
    }
}
