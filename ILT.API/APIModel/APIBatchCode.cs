using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APIBatchCode
    {
        [Required]
        public string BatchCode { get; set; }
    }
}
