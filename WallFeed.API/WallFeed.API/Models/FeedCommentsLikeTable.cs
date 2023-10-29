namespace WallFeed.API.Models
{
    public class FeedCommentsLikeTable : WallFeedCommonFields
    {
        public int Id { get; set; }
        public int FeedCommentsId { get; set; }
        public int UserId { get; set; }
    }
}
