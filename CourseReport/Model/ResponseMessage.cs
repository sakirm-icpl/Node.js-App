using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.Model
{
    public class ResponseMessage
    {
        public string Message { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }
    }
    internal enum MessageType
    {
        [Description("Record was saved successfully")]
        Success,
        [Description("Failed to save")]
        Fail,
        [Description("Duplicate! Already exist.")]
        Duplicate,
        [Description("Deleted record")]
        Delete,
        [Description("No record found!")]
        NotFound,
        [Description("Record does not exist!")]
        NotExist,
        [Description("Data not available")]
        DataNotAvailable,
        [Description("Invalid data!")]
        InvalidData,
        [Description("Internal Server Error!")]
        InternalServerError,
        [Description("Invalid File!")]
        InvalidFile,
    }
}
