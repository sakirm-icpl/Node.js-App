using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.APIModel
{
    public class APIComment
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class APICommentNumber
    {
        public int Id { get; set; }
        public int FeedTableId { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int numberofcomments { get; set; }
    }

}
