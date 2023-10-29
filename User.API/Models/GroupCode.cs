using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    public class GroupCode
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public bool? IsDeleted { get; set; }
        public int? UserId { get; set; }
    }
}
