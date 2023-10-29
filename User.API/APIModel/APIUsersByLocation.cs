using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIUsersByLocation
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int Id { get; set; }
    }
    public class APISearchUserforApplication
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string LocationName { get; set; }
        public int? LocationId { get; set; }

    }

}
