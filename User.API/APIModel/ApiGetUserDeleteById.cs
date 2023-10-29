namespace User.API.APIModel
{
    public class ApiGetUserDeleteById
    {
        public string ID { get; set; }
    }
    public class APIGetUserDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
