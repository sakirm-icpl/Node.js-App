using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Models
{
    public class FeedContent : WallFeedCommonFields
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public string Location { get; set; }
    }
}
