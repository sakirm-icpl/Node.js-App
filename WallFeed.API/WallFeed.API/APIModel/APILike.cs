using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.APIModel
{
    public class APILike
    {
        public int Id { get; set; }
        public bool SelfLiked { get; set; }
        public int Likes { get; set; }
    }
}
