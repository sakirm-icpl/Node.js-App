using WallFeed.API.Data;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories
{
    public class FeedCommentsLikeRepository : Repository<FeedCommentsLikeTable>, IFeedCommentsLikeRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedLikeRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private GadgetDbContext context;
        public FeedCommentsLikeRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.context = context;
            this._customerConnectionString = customerConnectionString;

        }
        public async Task<List<FeedCommentsLikeTable>> ValidateCommentsLike(FeedCommentsLikeTable feedCommentsLikeData)
        {
            IQueryable<FeedCommentsLikeTable> result = (from feedCommentsLike in this.context.feedCommentsLikeTable
                                                        where (feedCommentsLike.FeedCommentsId == feedCommentsLikeData.FeedCommentsId && feedCommentsLike.UserId == feedCommentsLikeData.UserId)
                                                        select feedCommentsLike);
            return await result.ToListAsync();
        }
        public async Task<int> GetNumberOfCommentsLikes(int Id)
        {
            return await context.feedCommentsLikeTable.Where(x => x.FeedCommentsId == Id).Select(x => x.Id).CountAsync();
        }
    }
}