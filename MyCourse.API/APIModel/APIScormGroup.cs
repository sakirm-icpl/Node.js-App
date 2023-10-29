using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.APIModel
{
    public class APIScormGroup
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string Status { get; set; }
    }
}
