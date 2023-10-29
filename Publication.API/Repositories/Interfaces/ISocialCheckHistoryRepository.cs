using Publication.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publication.API.Repositories.Interfaces
{
    public interface ISocialCheckHistoryRepository : IRepository<SocialCheckHistory>
    {
        Task<bool> NewPostToShow(int UserId);
        Task<bool> NewNewsToShow(int UserId);
        Task<bool> NewArticleToShow(int UserId);
        Task<SocialCheckHistory> CheckForDuplicate(int userId);
    }
}