using WallFeed.API.APIModel;
using WallFeed.API.Data;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories
{
    public class FeedContentRepository : Repository<FeedContent>, IFeedContentRepository
    {
        private GadgetDbContext context;
        public FeedContentRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<List<APIContent>> GetContentByFeedTableId(int FeedTableId)
        {
            IQueryable<APIContent> result = (from feed in this.context.feedContents
                                             where feed.FeedTableId == FeedTableId
                                             select new APIContent
                                             {
                                                 Id = feed.Id,
                                                 Location = feed.Location.Substring(0,1)=="\\"? feed.Location.Remove(0, 1): feed.Location
                                             }
                                            );

            return await result.ToListAsync();
        }
    }
}
