using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Configure14
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }
    }
}
