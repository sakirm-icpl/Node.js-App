namespace Assessment.API.Model.ThirdPartyIntegration
{
    public class DataAlison
    {
        public int id { get; set; }
        public string? email { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? phone { get; set; }
        public string? is_public { get; set; }
        public string? avatar { get; set; }
        public string? verified { get; set; }
        public string? first_access { get; set; }
        public string? last_access { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
    }
    public class AlisonUser
    {
        public DataAlison? data { get; set; }
    }
    public class Email
    {
        public string? EmailId { get; set; }
    }
    public class RegisterAlsion
    {
        public string? email { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? city { get; set; }
        public string? country { get; set; }
    }
    public class RegisterData
    {
        public RegisterToken? data { get; set; }
    }
    public class RegisterToken
    {
        public string? token { get; set; }
    }
}
