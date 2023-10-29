namespace MyCourse.API.APIModel.CourseRating
{
    public class APICourseRating
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
        public decimal Average { get; set; }
    }

    public class APICourseReview
    {
        public int? Id { get; set; }
        public int RatingId { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewText { get; set; }
        public string UseName { get; set; }
        public string Date { get; set; }
    }


    public class APICourseRatingAndReview
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
        public decimal Average { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewText { get; set; }
        public string UseName { get; set; }
    }
    public class APICourseRatingMerged
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
        public decimal Average { get; set; }
        public int OneStarPercentage { get; set; }
        public int TwoStarPercentage { get; set; }
        public int ThreeStarPercentage { get; set; }
        public int FourStarPercentage { get; set; }
        public int FiveStarPercentage { get; set; }

        public APICourseReview[] aPICourseReview { get; set; }
    }


}
