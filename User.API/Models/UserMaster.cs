using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.API.Models
{
    public class UserMaster
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string UserId { get; set; }
       [MaxLength(1000)]
        public string EmailId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Password { get; set; }
        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(10)]
        public string UserRole { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? AccountExpiryDate { get; set; }
        public bool Lock { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RowGuid { get; set; }
        public string FederationId { get; set; }
        
        public enum Exists
        {
            [Description("Employee Code already exist.")]
            EmployeeCodeExist,
            [Description("User Id already exist.")]
            UserIdExist,
            [Description("Mobile already exist.")]
            MobileExist,
            [Description("Email Id already exist.")]
            EmailIdExist,
            [Description("User name already exist.")]
            UserNameExist,
            [Description("Activation Email exist.")]
            ActivationSentToExist,
            [Description("Activation Email does not exist.")]
            ActivationSentToNotExist,
            No,
            [Description("Aadhar Number already exist.")]
            AadharExist,
        }
    }

}
