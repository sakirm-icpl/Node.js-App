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
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace WallFeed.API.Repositories
{
    public class FeedCommentsRepository : Repository<FeedComments>, IFeedCommentsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedCommentsRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private GadgetDbContext context;
        
        public FeedCommentsRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.context = context;
            this._customerConnectionString = customerConnectionString;

        }
                
        public async Task<IEnumerable<APIFeedComments>> GetComments(int FeedTableId,int UID)
        {
            try
            {

                APIFeedComments comment = new APIFeedComments();
                List<APIFeedComments> comments = new List<APIFeedComments>();
                bool flag,likeflag;
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetComments";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FeedTableId", SqlDbType.Int) { Value = FeedTableId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (Convert.ToInt32(Convert.ToString(row["UserId"])) == UID)
                                    flag = true;
                                else
                                    flag = false;

                                if (IsSelfLikedComment(Convert.ToInt32(Convert.ToString(row["Id"])), UID) == 0)
                                    likeflag = false;
                                else
                                    likeflag = true;

                                comment = new APIFeedComments
                                {
                                    Id = Convert.ToInt32(Convert.ToString(row["Id"])),
                                    FeedTableId = Convert.ToInt32(Convert.ToString(row["FeedTableId"])),
                                    UserId = Convert.ToInt32(Convert.ToString(row["UserId"])),
                                    Comment = Convert.ToString(row["Comment"]),
                                    UserName = Convert.ToString(row["UserName"]),
                                    ProfilePicture = Convert.ToString(row["ProfilePicture"]),
                                    CreatedDate = Convert.ToDateTime(Convert.ToString(row["CreatedDate"])),
                                    Likes = Convert.ToInt32(Convert.ToString(row["Likes"])),
                                    SelfCommented = flag,
                                    SelfLiked = likeflag
                                };
                                comments.Add(comment);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return comments;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public int IsSelfLikedComment(int FeedCommentsId, int UserId)
        {
            return context.feedCommentsLikeTable.Where(x => x.FeedCommentsId == FeedCommentsId && x.UserId == UserId).Select(x => x.Id).Count();
        }
        public Task<int> GetNumberOfComments(int FeedTableId)
        {
            return this.context.feedComments.Where(x => x.FeedTableId == FeedTableId).Select(x => x.Id).CountAsync();
        }

        public Task<APIComment> GetSingleComment(int UserId, DateTime time)
        {
            return this.context.feedComments.
                                            Where(x => x.UserId == UserId && x.CreatedDate == time).
                                            Select(x => new APIComment { 
                                                Id = x.Id,
                                                FeedTableId = x.FeedTableId,
                                                Comment = x.Comment,
                                                UserId = x.UserId,
                                                CreatedDate = Convert.ToDateTime(x.CreatedDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"))
                                            }).FirstAsync();
        }

    }
}
