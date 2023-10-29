using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIUserDetails
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string MailID { get; set; }
        public string OrganizationCode { get; set; }
        public string location { get; set; }
        public string Designation { get; set; }
        public string UserRole { get; set; }

        public string Result { get; set; }

        public string EntryptedUserID { get; set; }
    }
}
