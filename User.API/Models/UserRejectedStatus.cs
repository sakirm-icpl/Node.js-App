using System;

namespace User.API.Models
{
    public class UserRejectedStatus
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
