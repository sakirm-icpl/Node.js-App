namespace User.API.APIModel
{
    public class APIUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string UserRole { get; set; }

        public string UserId { get; set; }

    }

    public class APIGetUserMasterId
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
