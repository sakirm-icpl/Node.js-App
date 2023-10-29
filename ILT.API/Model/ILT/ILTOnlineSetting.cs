namespace ILT.API.Model.ILT
{
    public class ILTOnlineSetting
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Type { get; set; }

        public string? TeamsAuthority { get; set; }
    }
}
