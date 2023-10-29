using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIVFSSignUpOTP
    {
        public string EmailId { get; set; }
        public string OTP { get; set; }
        public string organizationCode { get; set; }
    }
}
