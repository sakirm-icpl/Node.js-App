using System;
using System.ComponentModel.DataAnnotations;
using User.API.APIModel;
using User.API.Repositories.Interfaces;
using static User.API.Models.UserMaster;

namespace User.API.Models
{
    public class Signup : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }
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
        public string ReportsTo { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(50)]
        public string Currency { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        [MaxLength(10)]
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? AccountExpiryDate { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn1 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn2 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn3 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn4 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn5 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn6 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn7 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn8 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn9 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn10 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn11 { get; set; }
        [MaxLength(100)]
        public string ConfigurationColumn12 { get; set; }
        [MaxLength(10)]
        public string Accept { get; set; }
        [MaxLength(100)]
        public string ActivationSentTo { get; set; }
        public string OrganizationCode { get; set; }
        public Exists Validations(ISignUpRepository userRepository, Signup user)
        {
            if (userRepository.Exists(user.UserId))
                return Exists.UserIdExist;
            if (userRepository.EmailExists(user.EmailId))
                return Exists.EmailIdExist;
            if (userRepository.MobileExists(user.MobileNumber))
                return Exists.MobileExist;
            if (userRepository.ActivationEmailNotExists(user.ActivationSentTo))
                return Exists.ActivationSentToNotExist;
            return Exists.No;
        }
        public bool IsUniqueDataIsChanged(Signup oldUser, Signup user)
        {
            if (!oldUser.EmailId.Equals(user.EmailId))
                return true;
            if (!oldUser.MobileNumber.Equals(user.MobileNumber))
                return true;
            if (!oldUser.UserId.Equals(user.UserId))
                return true;
            return false;
        }

        public APIUserMaster MapSignupToAPIUser(Signup user)
        {
            APIUserMaster apiUser = new APIUserMaster
            {
                AccountExpiryDate = user.AccountExpiryDate,
                ConfigurationColumn1 = user.ConfigurationColumn1,
                ConfigurationColumn2 = user.ConfigurationColumn2,
                ConfigurationColumn3 = user.ConfigurationColumn3,
                ConfigurationColumn4 = user.ConfigurationColumn4,
                ConfigurationColumn5 = user.ConfigurationColumn5,
                ConfigurationColumn6 = user.ConfigurationColumn6,
                ConfigurationColumn7 = user.ConfigurationColumn7,
                ConfigurationColumn8 = user.ConfigurationColumn8,
                ConfigurationColumn9 = user.ConfigurationColumn9,
                ConfigurationColumn10 = user.ConfigurationColumn10,
                ConfigurationColumn11 = user.ConfigurationColumn11,
                ConfigurationColumn12 = user.ConfigurationColumn12,
                Currency = user.Currency,
                DateOfBirth = user.DateOfBirth,
                DateOfJoining = user.DateOfJoining,
                EmailId = user.EmailId,
                Gender = user.Gender,
                Id = user.Id,
                Language = user.Language,
                ReportsTo = user.ActivationSentTo,
                MobileNumber = user.MobileNumber,
                TimeZone = user.TimeZone,
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
                UserType = user.UserType
            };
            return apiUser;
        }
    }
}
