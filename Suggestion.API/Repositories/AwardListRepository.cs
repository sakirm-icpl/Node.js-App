using Suggestion.API.Data;
using Suggestion.API.Repositories.Interfaces;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Suggestion.API.APIModel;

namespace Suggestion.API.Repositories
{
    public class AwardListRepository : Repository<AwardList> , IAwardList
    {
        private GadgetDbContext _db;
        public AwardListRepository(GadgetDbContext context) : base(context)
        {
            _db = context;
        }

        public async Task<APIAwardListsListandCount> GetAwardList(int page, int pageSize, string search)
        {
            var Query = (from x in _db.AwardList
                         orderby x.CreatedDate descending
                         where x.IsDeleted == false
                         select new APIAwardList
                         {
                             Id = x.Id,
                             Title = x.Title,
                             FilePath = x.FilePath,
                             IsActive = x.IsActive,

                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.Title.Contains(search)));
            }
            APIAwardListsListandCount ListandCount = new APIAwardListsListandCount();
            ListandCount.Count = Query.Distinct().Count();
            ListandCount.AwardListListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            return ListandCount;
        }

    }
}