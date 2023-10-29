using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIUserSignUpOTP
    {
        public string CustomerCode { get; set; }
        public string OTP { get; set; }
        public string UserId { get; set; }
    }

    public class ResponseMessageForUserRegister
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }

    }
    public class ResponseMessageForUserRegisterLogin
    {
        public string loginId { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }

    }
}
