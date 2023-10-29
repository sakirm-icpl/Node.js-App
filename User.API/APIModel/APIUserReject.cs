namespace User.API.APIModel
{
    public class APIUserReject 
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string EmailId { get; set; }

        public string MobileNumber { get; set; }
        public string ModifiedDate { get; set; }

        public string ErrorMessage { get; set; }
    }
}
