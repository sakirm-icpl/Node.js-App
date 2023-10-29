using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIBusiness
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(80)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Theme { get; set; }
        [MaxLength(100)]
        public string LogoName { get; set; }
    }
}
