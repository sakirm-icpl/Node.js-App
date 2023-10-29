using WallFeed.API.Data;
using WallFeed.API.Helper;
using WallFeed.API.Models;
using WallFeed.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace WallFeed.API.Repositories
{
    public class FeedLikeRepository : Repository<FeedLike>, IFeedLikeRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedLikeRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private GadgetDbContext context;

        public FeedLikeRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.context = context;
            this._customerConnectionString = customerConnectionString;

        }
        public async Task<int> GetNumberOfLikes(int Id)
        {
            try
            {
                int Likes = 0;
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetNumberOfLikes";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar) { Value = Id });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        Likes = dt.Rows.Count;
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
                return Likes;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return 0;
        }
        public async Task<int> GetNumberOfComments(int Id)
        {
            try
            {
                int Comments = 0;
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetNumberOfComments";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar) { Value = Id });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        Comments = dt.Rows.Count;
                        reader.Dispose();
                    }
                    await dbContext.Database.CloseConnectionAsync();
                }
                return Comments;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return 0;
        }
        public async Task<List<FeedLike>> ValidateLike(FeedLike feedLikeData)
        {
            IQueryable<FeedLike> result = (from feedLike in this.context.feedLikes
                                      where (feedLike.FeedTableId == feedLikeData.FeedTableId && feedLike.UserId == feedLikeData.UserId)
                                      select feedLike);
            return await result.ToListAsync();
        }

        public async Task<List<Feed>> IsSelfLiked(int FeedTableId,int UserId)
        {
            IQueryable<Feed> result = (from feed  in this.context.Feeds
                                      where feed.Id == FeedTableId && feed.CreatedBy == UserId
                                      select feed);
            return await result.ToListAsync();
        }
    }
}
