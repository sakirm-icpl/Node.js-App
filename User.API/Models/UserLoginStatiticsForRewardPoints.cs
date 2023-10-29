using System;

namespace User.API.Models
{
    public class UserLoginStatiticsForRewardPoints
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LastLoginDateTime { get; set; }
        public int SevenDayCount { get; set; }
        public int EightHoursCount { get; set; }
    }
}
