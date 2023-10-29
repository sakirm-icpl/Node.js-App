using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APISendSMS
    {
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string OrgCode { get; set; }
    }
}
