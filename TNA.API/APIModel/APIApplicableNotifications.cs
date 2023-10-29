using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TNA.API.APIModel
{
    public class APIApplicableNotifications
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsRead { get; set; }
        public int NotificationId { get; set; }
        public bool IsReadCount { get; set; }
    }
}
