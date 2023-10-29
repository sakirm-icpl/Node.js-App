using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publication.API.Models
{
    public class SocialCheckHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? LastWallfeedCheckTime { get; set; }
        public DateTime? LastNewsCheckTime { get; set; }
        public DateTime? LastArticleCheckTime { get; set; }
    }
}
