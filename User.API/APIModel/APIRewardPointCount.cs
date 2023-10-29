using System;

namespace User.API.APIModel
{
    public class APIRewardPointCount
    {
        public int RED { get; set; }
        public int GREEN { get; set; }
        public int BLUE { get; set; }
        public int YELLOW { get; set; }
    }

    public class APIRewardLeaderBoardDate
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int  Rank { get; set; }
    }

    public class APIRewardLeaderBoard
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Points { get; set; }
        public string Gender { get; set; }
        public string ProfilePicture { get; set; }
        public int Rank { get; set; }
        public string EId { get; set; }

    }
    public class APIRewardPointsSummery : APIRewardLeaderBoard
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string CircleZone { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string ReportingManager { get; set; }
        public string? RestaurantId { get; set; }
        public string? CurrentRestaurant { get; set; }

        public string? Region { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ClusterManager { get; set; }

        public string? AreaManager { get; set; }
    }
}
