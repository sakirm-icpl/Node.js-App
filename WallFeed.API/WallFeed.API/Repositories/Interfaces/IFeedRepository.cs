using WallFeed.API.APIModel;
using WallFeed.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Repositories.Interfaces
{
    public interface IFeedRepository : IRepository<Feed>
    {
        Task<IEnumerable<APIFeed>> GetFeedData(int UID);
        List<Feed> GetFeedId(int Id, DateTime time);
        Task<List<APIFeed>> GetFeed();
        Task<List<Feed>> GetSingleFeed(int UserId, DateTime time);
        Task<string> SaveImage(IFormFile uploadedFile);
        Task<string> SaveVideo(IFormFile uploadedFile);
        Task<string> SaveAudio(IFormFile uploadedFile);
        Task<string> SavePdf(IFormFile uploadedFile);
        void DeleteWallFeed(int FeedTableId);
        bool DeleteFile(string filePath);
        Task<List<Feed>> FeedValidation(int Id);
        void DeleteImages(List<APIContent> feedContent);
    }
}
