using System;

namespace Survey.API.APIModel
{
    public class APIResponse
    {
        public int StatusCode { get; set; }
        public Object ResponseObject { get; set; }
        public string Description { get; set; }
    }
}
