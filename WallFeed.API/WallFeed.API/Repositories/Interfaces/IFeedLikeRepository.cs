using WallFeed.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface IFeedLikeRepository : IRepository<FeedLike>
    {
        Task<int> GetNumberOfLikes(int Id);
        Task<List<FeedLike>> ValidateLike(FeedLike feedLike);
        Task<List<Feed>> IsSelfLiked(int FeedTableId, int UserId);
        Task<int> GetNumberOfComments(int id);
    }
}
