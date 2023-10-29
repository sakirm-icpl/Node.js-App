using WallFeed.API.APIModel;
using WallFeed.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface IFeedCommentsRepository : IRepository<FeedComments>
    {
        Task<IEnumerable<APIFeedComments>> GetComments(int FeedTableId, int UID);
        Task<int> GetNumberOfComments(int FeedTableId);
        Task<APIComment> GetSingleComment(int UserId, DateTime time);
    }
}
