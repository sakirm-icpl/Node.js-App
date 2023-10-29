using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Models
{
    public class FeedComments : WallFeedCommonFields
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
    }
}
