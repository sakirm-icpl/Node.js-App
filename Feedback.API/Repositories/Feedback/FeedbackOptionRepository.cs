using Feedback.API.Model;
using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;

namespace Feedback.API.Repositories
{
    public class FeedbackOptionRepository : Repository<FeedbackOption>, IFeedbackOption
    {
        private FeedbackContext _db;

        public FeedbackOptionRepository(FeedbackContext context) : base(context)
        {
            this._db = context;
        }
        public bool Exists(string search)
        {
            if (_db.FeedbackOption.Count(f => f.OptionText == search && f.IsDeleted == false) > 0)
                return true;
            return false;
        }
        public int Count(string? search = null, string? filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return _db.FeedbackOption.Where(f => f.OptionText.Contains(search)).Count();
            return _db.FeedbackOption.Where(c => c.IsDeleted == false).Count();
        }

        public List<FeedbackOption> Get(int page, int pageSize, string? search = null, string? filter = null)
        {
            IQueryable<FeedbackOption> Query = _db.FeedbackOption.Where(c => c.IsDeleted == false);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.OptionText.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return Query.ToList();
        }
    }
}
