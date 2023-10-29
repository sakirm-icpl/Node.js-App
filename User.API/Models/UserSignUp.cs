using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using User.API.Repositories.Interfaces;
using static User.API.Models.UserMaster;

namespace User.API.Models
{
    public class UserSignUp : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Required]
        [MaxLength(100)]
        public string EmailId { get; set; }
        [Required]
        [MaxLength(25)]
        public string MobileNumber { get; set; }
        [MaxLength(10)]
        public string UserRole { get; set; }
        [MaxLength(10)]
        public string UserType { get; set; }
        public bool IsActive { get; set; }
        
    }
    
}
