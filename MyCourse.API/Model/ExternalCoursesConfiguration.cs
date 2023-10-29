namespace MyCourse.API.Model
{
    public class ExternalCoursesConfiguration
    {
        public int Id { get; set; }
        public string Vendor { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string EmailId { get; set; }
        public string Passwords { get; set; }
        public string OrgId { get; set; }
        public string Token { get; set; }
        public string BaseUrl { get; set; }
    }
    public class AlisonToken
    {
        public string token_type { get; set; }
        public int expires_in { get; set;}
        public string access_token { get; set;}
        public string refresh_token { get;}
    }
}
