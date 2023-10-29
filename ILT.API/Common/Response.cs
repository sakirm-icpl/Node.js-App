using System.Net;

namespace ILT.API.Common
{
    public class Response
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string JoinMeetingUrl { get; set; }

    }
}
