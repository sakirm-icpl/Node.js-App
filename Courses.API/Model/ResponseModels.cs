using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class ResponseModels
    {
        public interface IApiResponse
        {
            public bool Success { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; }
        }
        public class APIData<T>
        {
            public List<T> Records { get; set; }
            public int RecordCount { get; set; }

        }

        public class APIResponse<T> : IApiResponse
        {
            public bool Success { get; set; } = true;
            public int StatusCode { get; set; } = 200;
            public string Message { get; set; }
            public APIData<T> Data { get; set; } = new APIData<T>();
        }

        public class APIResponseSingle<T> : IApiResponse
        {
            public bool Success { get; set; } = true;
            public int StatusCode { get; set; } = 200;
            public string Message { get; set; } = "Success";
            public T Record { get; set; }
        }

        public class APIResposeYesNo : IApiResponse
        {
            public bool Success { get; set; } = true;
            public int StatusCode { get; set; } = 200;
            public string Message { get; set; } = "Success";
            public string Content { get; set; } = "Content";
        }


        public class APIResposeNo : IApiResponse
        {
            public bool Success { get; set; } = false;
            public int StatusCode { get; set; } = 400;
            public string Message { get; set; } = "Failed";
            public string Content { get; set; } = "Content";
        }

        public class APIResposeYes : IApiResponse
        {
            public bool Success { get; set; } = true;
            public int StatusCode { get; set; } = 200;
            public string Message { get; set; } = "Success";
            public string Content { get; set; } = "Content";
        }
    }
}
