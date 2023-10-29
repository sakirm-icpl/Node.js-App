using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Models
{
    public class FeedLike : WallFeedCommonFields
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public int UserId { get; set; }
    }
}
