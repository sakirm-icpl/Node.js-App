using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("Tokens", Schema = "User")]
    public class Tokens
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        [MaxLength(1000)]
        public string? UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
    }
}
