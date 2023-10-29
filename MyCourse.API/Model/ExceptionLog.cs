using System;

namespace MyCourse.API.Model
{
    public class ExceptionLog
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
