using WallFeed.API.APIModel;
using WallFeed.API.Data;
using WallFeed.API.Helper;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using WallFeed.API.Metadata;
using Microsoft.Data.SqlClient;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace WallFeed.API.Repositories
{
    public class FeedRepository : Repository<Feed>, IFeedRepository
    {
        ITokensRepository _tokensRepository;
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedRepository));
        private GadgetDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IFeedLikeRepository _feedLikeRepository;
        private IFeedContentRepository _feedContentRepository;
        private IFeedCommentsRepository _feedCommentsRepository;
        public FeedRepository(GadgetDbContext context, 
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,
                              ICustomerConnectionStringRepository customerConnectionString,
                              IFeedLikeRepository feedLikeRepository,
                              IAzureStorage azurestorage,
                              ITokensRepository tokensRepository,
                              IFeedContentRepository feedContentRepository,
                              IFeedCommentsRepository feedCommentsRepository) : base(context)
        {
            this._tokensRepository = tokensRepository;
            this._azurestorage = azurestorage;
            this.context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._customerConnectionString = customerConnectionString;
            this._feedLikeRepository = feedLikeRepository;
            this._feedContentRepository = feedContentRepository;
            this._feedCommentsRepository = feedCommentsRepository;
        }

        public async Task<IEnumerable<APIFeed>> GetFeedData(int UID)
        {
            try
            {
                APIFeed feed = new APIFeed();
                List<APIFeed> feeds = new List<APIFeed>();
                bool flag;
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        //await GetContentByFeedTableId(13);
                        cmd.CommandText = "GetFeed";
                        cmd.CommandType = CommandType.StoredProcedure;
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (IsSelfLiked(Convert.ToInt32(Convert.ToString(row["Id"])), UID) == 0)
                                    flag = false;
                                else
                                    flag = true;

                                feed = new APIFeed
                                {
                                    Id = Convert.ToInt32(Convert.ToString(row["Id"])),
                                    UserId = Convert.ToInt32(Convert.ToString(row["UserId"])),
                                    Caption = Convert.ToString(row["Caption"]),
                                    Type = Convert.ToString(row["Type"]),
                                    SelfLiked = flag,
                                    Likes = await this._feedLikeRepository.GetNumberOfLikes(Convert.ToInt32(Convert.ToString(row["Id"]))),
                                    Comments = await this._feedCommentsRepository.GetNumberOfComments(Convert.ToInt32(Convert.ToString(row["Id"]))),
                                    UserName = Convert.ToString(row["UserName"]),
                                    ProfilePicture = Convert.ToString(row["ProfilePicture"]),
                                    CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
                                    Content = await this._feedContentRepository.GetContentByFeedTableId(Convert.ToInt32(Convert.ToString(row["Id"])))
                                };
                                feeds.Add(feed);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }

                var t = from f in feeds 
                        orderby f.CreatedDate descending 
                        select f;

                return t;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public int IsSelfLiked(int FeedTableId,int UserId)
        {
            return context.feedLikes.Where(x => x.FeedTableId == FeedTableId && x.UserId == UserId).Select(x => x.Id).Count();
        }
        public List<Feed> GetFeedId(int Id, DateTime time)
        {

            IQueryable<Feed> result = (from feed in this.context.Feeds
                                       where feed.UserId == Id && feed.CreatedDate == time 
                                       select feed);

            return result.ToList();

        }
        public async Task<List<APIFeed>> GetFeed()
        {
            IQueryable<APIFeed> result = (from feed in this.context.Feeds
                                             select new APIFeed
                                             {
                                                 Id = feed.Id,
                                                 UserId = feed.UserId
                                             });
            return await result.ToListAsync();
        }
        public Task<List<Feed>> GetSingleFeed(int UserId, DateTime time)
        {
            return this.context.Feeds.Where(x => x.UserId == UserId && x.CreatedDate == time).Select(x => x).ToListAsync();
        }

        public async Task<string> SaveImage(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string saveFilePath = string.Empty;
            string gadgetsPath = _configuration["ApiGatewayWwwroot"];
            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                _logger.Info("video EnableBlobStorage : " + EnableBlobStorage);
                 if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                 {
                    if (!Directory.Exists(gadgetsPath))
                    {
                        Directory.CreateDirectory(gadgetsPath);
                    }
                    fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                    fileName = string.Concat(fileName.Split(' '));
                    filePath = Path.Combine(gadgetsPath, FileType.Image);
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    saveFilePath = Path.Combine(FileType.Image, fileName);
                    saveFilePath = Path.Combine("\\", saveFilePath);
                    filePath = Path.Combine(gadgetsPath, FileType.Image, fileName);
                    filePath = string.Concat(filePath.Split(' '));
                    await SaveFile(uploadedFile, filePath);

                    return saveFilePath;
                 }
                 else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Image);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            //string[] name = res.Blob.Name.Split('\\');
                            //fileName = name[name.Length - 1];
                            return filePath;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }
        }


        public async Task<string> SaveVideo(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string saveFilePath = string.Empty;
            string gadgetsPath = this._configuration["ApiGatewayWwwroot"];

            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
            _logger.Info("video EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                //coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
                if (!Directory.Exists(gadgetsPath))
                {
                    Directory.CreateDirectory(gadgetsPath);
                }

                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(gadgetsPath, FileType.Video);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);

                }

                saveFilePath = Path.Combine(FileType.Video, fileName);
                saveFilePath = Path.Combine("\\", saveFilePath);

                filePath = Path.Combine(gadgetsPath, FileType.Video, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);

                return saveFilePath;
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Video);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            //string[] name = res.Blob.Name.Split('\\');
                            //fileName = name[name.Length - 1];
                            return filePath;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }
        }

        public async Task<string> SaveAudio(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string saveFilePath = string.Empty;
            string gadgetsPath = this._configuration["ApiGatewayWwwroot"];
            //coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
            _logger.Info("video EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                if (!Directory.Exists(gadgetsPath))
                {
                    Directory.CreateDirectory(gadgetsPath);
                }

                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(gadgetsPath, FileType.Audio);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                saveFilePath = Path.Combine(FileType.Audio, fileName);
                saveFilePath = Path.Combine("\\", saveFilePath);

                filePath = Path.Combine(gadgetsPath, FileType.Audio, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);

                return saveFilePath;
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Audio);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            //string[] name = res.Blob.Name.Split('\\');
                            //fileName = name[name.Length - 1];
                            return filePath;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }
        }

        public async Task<string> SavePdf(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string saveFilePath = string.Empty;
            string gadgetsPath = this._configuration["ApiGatewayWwwroot"];
            //coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
            _logger.Info("video EnableBlobStorage : " + EnableBlobStorage);
            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                if (!Directory.Exists(gadgetsPath))
                {
                    Directory.CreateDirectory(gadgetsPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(gadgetsPath, FileType.Pdf);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                saveFilePath = Path.Combine(FileType.Pdf, fileName);
                saveFilePath = Path.Combine("\\", saveFilePath);

                filePath = Path.Combine(gadgetsPath, FileType.Pdf, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);

                return saveFilePath;
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Pdf);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            //string[] name = res.Blob.Name.Split('\\');
                            //fileName = name[name.Length - 1];
                            return filePath;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return null;
            }
        }

        public async Task<bool> SaveFile(IFormFile uploadedFile, string filePath)
        {
            try
            {
                using (var fs = new FileStream(Path.Combine(filePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }
        public void DeleteImages(List<APIContent> feedContent)
        {
            try
            {

                for (int i = 0; i < feedContent.Count; i++)
                {
                    DeleteFile(feedContent[i].Location.ToString());

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                string gadgetsPath = _configuration["ApiGatewayWwwroot"];
                File.Delete(gadgetsPath + filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return false;
            }
        }
        public void DeleteWallFeed(int FeedTableId)
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "DeleteFeed";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FeedTableId", SqlDbType.Int) { Value = FeedTableId });
                        dbContext.Database.OpenConnection();
                        cmd.ExecuteReader();
                        dbContext.Database.CloseConnection();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
        }
        public Task<List<Feed>> FeedValidation(int Id)
        {
            IQueryable<Feed> result = (from feed in this.context.Feeds
                                       where feed.Id == Id
                                       select feed);

            return result.ToListAsync();
        }
    }
}
