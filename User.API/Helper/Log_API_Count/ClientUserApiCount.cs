using System;

namespace User.API.Helper.Log_API_Count
{
    public class ClientUserApiCount
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string ServiceName { get; set; }
        public string OrgCode { get; set; }
    }
}
