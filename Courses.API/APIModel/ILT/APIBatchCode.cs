using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel.ILT
{
    public class APIBatchCode
    {
        [Required]
        public string BatchCode { get; set; }
    }
}
