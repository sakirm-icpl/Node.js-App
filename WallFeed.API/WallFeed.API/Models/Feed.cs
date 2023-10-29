using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallFeed.API.Models
{
    public class Feed : WallFeedCommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
    }
}
