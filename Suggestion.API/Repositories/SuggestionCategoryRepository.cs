using Suggestion.API.Data;
using Suggestion.API.Repositories.Interfaces;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Suggestion.API.APIModel;

namespace Suggestion.API.Repositories
{
    public class SuggestionCategoriesRepository : Repository<SuggestionCategory>, ISuggestionCategories
    {
        private GadgetDbContext _db;
        public SuggestionCategoriesRepository(GadgetDbContext context) : base(context)
        {
            _db = context;
        }

        public async Task<APISuggestionCategoriesListandCount> GetSuggestionCategories(int page, int pageSize, string search)
        {
            var Query = (from x in _db.SuggestionCategory
                         orderby x.CreatedDate descending
                         where x.IsDeleted == false
                         select new APISuggestionCategory
                         {
                             Id = x.Id,
                             Code = x.Code,
                             SuggestionsCategory = x.SuggestionsCategory,
                             IsActive = x.IsActive,
                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.SuggestionsCategory.Contains(search) || r.Code.Contains(search)));
            }

            APISuggestionCategoriesListandCount ListandCount = new APISuggestionCategoriesListandCount();
            ListandCount.Count = Query.Distinct().Count();
            ListandCount.SuggestionCategoryListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            return ListandCount;
        }
    }
}