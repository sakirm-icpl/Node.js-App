using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserHRAssociation : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }
        [MaxLength(50)]
        public string Level { get; set; }

    }
}
