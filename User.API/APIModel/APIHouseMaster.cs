using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIHouseMaster
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }

        [MaxLength(100)]
        public string LogoName { get; set; }
    }
}
