using System;

namespace Payment.API.APIModel
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public Object ResponseObject { get; set; }
        public string Description { get; set; }
    }
}
