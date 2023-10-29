using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalIntegration.MetaData
{
    public class RequestLogin
    {
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string external_id { get; set; }
    }

    public class ResponseLogin
    {
        public string RespData { get; set; }
    }
}
