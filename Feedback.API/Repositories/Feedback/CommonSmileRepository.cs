using Courses.API.APIModel.Feedback;
using Feedback.API.Model;
using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;

namespace Feedback.API.Repositories
{
    public class CommonSmileRepository : Repository<CommonSmileSheet>, ICommonSmile
    {
        private FeedbackContext _db;
        public CommonSmileRepository(FeedbackContext _context) : base(_context)
        {
            _db = _context;
        }
        public int Count(string? search = null, string? filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return _db.CommonSmileSheet.Where(r => r.Section.Contains(search) && r.IsDeleted == false).Count();
            return _db.CommonSmileSheet.Where(r => r.IsDeleted == false).Count();
        }

        public bool Exists(string name)
        {
            if (_db.CommonSmileSheet.Count(y => y.Section.Contains(name) && y.IsDeleted == false) > 0)
                return true;
            return false;
        }

        public List<CommonSmileSheet> Get(int page, int pageSize, string? search = null, string? filter = null)
        {
            IQueryable<CommonSmileSheet> Query = _db.CommonSmileSheet.Where(csm => csm.IsDeleted == false);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Section.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return Query.ToList();
        }
        public List<APISmileSheetSectionTypeAhead> GetSectionTypehead(string? search = null)
        {
            IQueryable<APISmileSheetSectionTypeAhead> Query = _db.CommonSmileSheet.Where(csm => csm.IsDeleted == false).Select(csm => new APISmileSheetSectionTypeAhead { Title = csm.Section, Id = csm.Id });
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Title.StartsWith(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            return Query.ToList();
        }
    }
}
