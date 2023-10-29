using System;

namespace User.API.APIModel
{
    public class ALIUserLoggedInHistory
    {
        public int Id { get; set; }
        public int UserMasterId { get; set; }
        public DateTime? LocalLoggedInTime { get; set; }
        public DateTime? LocalLoggedOutTime { get; set; }
        public int TotalTimeInMinutes { get; set; }
    }
}
