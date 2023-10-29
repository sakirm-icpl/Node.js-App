using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.APIModel
{
    public class APIFeed
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public bool SelfLiked { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<APIContent> Content { get; set; }


    }

    public class APIContent
    {
        public int Id { get; set; }
        public string Location { get; set; }

    }
}
