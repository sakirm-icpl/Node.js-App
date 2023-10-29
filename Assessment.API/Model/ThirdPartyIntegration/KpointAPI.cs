using Newtonsoft.Json;

namespace Assessment.API.APIModel.ThirdPartyIntegration
{
    public class KpointAPI
    {
    }
    public class AuthenticationKpoint
    {
        public string? key { get; set; }
        public string? bucket { get; set; }
        public string? s3url { get; set; }
        public string? upload_bucket_region { get; set; }
        public string? cdnupload { get; set; }
        public string? uploadcdn { get; set; }
        public Parameters? parameters { get; set; }
    }
    public class Parameters
    {

        [JsonProperty("X-amz-credential")]
        public string? X_amz_credential { get; set; }

        public string? bucket { get; set; }
        [JsonProperty("X-amz-signature")]
        public string? X_amz_signature { get; set; }
        [JsonProperty("X-amz-algorithm")]
        public string? X_amz_algorithm { get; set; }
        [JsonProperty("X-amz-date")]
        public string? X_amz_date { get; set; }
        [JsonProperty("X-amz-expires")]
        public string? X_amz_expires { get; set; }
        public string? acl { get; set; }
        [JsonProperty("x-amz-server-side-encryption")]
        public string? x_amz_server_side_encryption { get; set; }
        [JsonProperty("Content-Type")]
        public string? Content_Type { get; set; }
        public string? policy { get; set; }
    }
    public class KPointResponse
    {
        public string? kapsule_id { get; set; }
        public string? displayname { get; set; }
    }
    public class XtTokenKpoint
    {
        public string? XtToken { get; set; }
    }
}
