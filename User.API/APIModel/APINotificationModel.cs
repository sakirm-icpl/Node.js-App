using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APINewUserActivationEmail
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public string ReportsTo { get; set; }
    }
    public class UserImportBulk
    {
        public List<APINewUserActivationEmail> NewUserImport;
        public string orgCode { get; set; }
        public int loggedInUserId { get; set; }
    }
}
