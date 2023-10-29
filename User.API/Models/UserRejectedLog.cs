using System;

namespace User.API.Models
{
    public class UserRejectedLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
