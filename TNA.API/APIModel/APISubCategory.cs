using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TNA.API.APIModel
{
    public class APISubCategory
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [MinLength(2), MaxLength(8)]
        [Required]
        public string Code { get; set; }
        [Required]
        [MinLength(1), MaxLength(80)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? SequenceNo { get; set; }
    }
}
