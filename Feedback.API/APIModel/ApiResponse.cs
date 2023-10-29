using System;
using System.Collections.Generic;

namespace Feedback.API.APIModel
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public Object? ResponseObject { get; set; }
        public string? Description { get; set; }
    }
    public class ApiResponseILT
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public Object? ResponseObject { get; set; }
        public string? Description { get; set; }
        public List<APINominationUserResponse>? aPINominationResponses { get; set; }
    }

    public class APINominationUserResponse
    {
        public string? CourseName { get; set; }
        public string? BatchCode { get; set; }
        public string? ModuleName { get; set; }
        public string? ScheduleCode { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
