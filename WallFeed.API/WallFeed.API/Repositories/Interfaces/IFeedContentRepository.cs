using WallFeed.API.APIModel;
using WallFeed.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface IFeedContentRepository : IRepository<FeedContent>
    {
        Task<List<APIContent>> GetContentByFeedTableId(int FeedTableId);
    }
}
