using WallFeed.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface IFeedCommentsLikeRepository : IRepository<FeedCommentsLikeTable>
    {
        Task<List<FeedCommentsLikeTable>> ValidateCommentsLike(FeedCommentsLikeTable feedCommentsLike);
        Task<int> GetNumberOfCommentsLikes(int Id);
    }
}
