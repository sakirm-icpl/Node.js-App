using System;

namespace Courses.API.APIModel
{
    public class ApiGoogleEvent
    {
        public string Summary { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EmailID { get; set; }
    }
}
