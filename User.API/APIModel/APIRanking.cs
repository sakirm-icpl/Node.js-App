namespace User.API.APIModel
{
    public class APIRanking
    {
        public int UserId { get; set; }
        public string EUSerId { get; set; }
        public string UserName { get; set; }
        public int TotalPoint { get; set; }
        public string ProfilePicture { get; set; }
        public string Gender { get; set; }
        public int Rank { get; set; }
        public string Level { get; set; }
        public int MaximumLevelPoint { get; set; }
        public string LevelCode { get; set; }
        public string HouseCode { get; set; }
        public string HouseName { get; set; }
        public string eId { get; set; }
        public string country { get; set; }
    }
}
