using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class TokenBlacklist
    {
        public int Id { get; set; }

        [MaxLength(1000)]
        public string Token { get; set; }
        [MaxLength(1000)]
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
    }
}
