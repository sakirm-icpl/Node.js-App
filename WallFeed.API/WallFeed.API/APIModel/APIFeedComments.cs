using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.APIModel
{
    public class APIFeedComments
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool SelfCommented { get; set; }
        public bool SelfLiked { get; set; }
        public int Likes { get; set; }

    }
}
