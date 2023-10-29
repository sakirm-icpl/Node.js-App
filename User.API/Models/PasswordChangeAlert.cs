using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    public class PasswordChangeAlert
    {
        [DefaultValue(false)]
        public bool displayPasswordChangeAlert { get; set; }
        public int ? daysRemainedtoExpirePassword { get; set; }
        public string alertMessage { get; set; }

    }
}
