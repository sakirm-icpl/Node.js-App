using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    public class SignUpOTP
    {
        public int Id { get; set; }
        public string EmpCode { get; set; }
        public string OTP { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
