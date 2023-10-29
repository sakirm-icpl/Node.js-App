using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIUserConfiguration
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [MaxLength(100)]
        public string CustomerName { get; set; }
        [MaxLength(100)]
        public string EmailId { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(100)]
        public string UserName { get; set; }
        public string UserRole { get; set; }
        [MaxLength(10)]
        public string UserType { get; set; }
        public bool IsPasswordModified { get; set; }
        public int RoleID { get; set; }
        public string LanguageCode { get; set; }

        public string ProfilePicture { get; set; }

        public string LandingPage { get; set; }

        public string ProductName { get; set; }

        public string ImplicitRole { get; set; }

        public int ImplicitRoleID { get; set; }
        public int Notifications { get; set; }

        public bool IsEnableDegreed { get; set; }
        public string LastLoggedInTime { get; set; }
        public string ProfilePictureFullPath { get; set; }
        public string Gender { get; set; }
        public string ColorCode { get; set; }
        public string LogoPath { get; set; }
        public string DisplayRoleName { get; set; }
        public string Region { get; set; }
        public bool IsShowTODODashboard { get; set; }
        public bool IsShowHourseInfoOnLeaderboard { get; set; }
        public string ApplicationDateFormat { get; set; }
        public bool IsSupervisor { get; set; }
        public string LocationName { get; set; }
        public string EmpCode { get; set; }
        public string country { get; set; }
    }

    public class APIUserDashboardConfiguration
    {
        public bool IsAchieveMastery { get; set; }
        public int AchieveMasteryCourseID { get; set; }
        public string AchieveMasteryCourseTitle { get; set; }
        public string TimeSpent { get; set; }
        public bool IsShowCEOMessageAfterLogin { get; set; }
        public bool IsShowCEOMessageOnLandingPage { get; set; }
        public string CEOMessageHeading { get; set; }
        public string CEOMessageDescription { get; set; }
        public string CEOProfilePicture { get; set; }
        public bool IsShowThoughtMessage { get; set; }
        public string ThoughtMessage { get; set; }
    }
}
