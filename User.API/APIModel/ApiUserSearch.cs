namespace User.API.APIModel
{
    public class ApiUserSearch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
        public string MobileNumber { get; set; }
        public string UserType { get; set; }
    }

    public class ApiUserSearchV2
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
        public string MobileNumber { get; set; }
        public string UserType { get; set; }
        public string? NameUserId { get; set; }
        public bool IsDeleted { get; set; }


    }
    public class APIUserTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
        public string MobileNumber { get; set; }
        public string ReportsTo { get; set; }
        public string UserRole { get; set; }
        public string RoleCode { get; set; }
        public string Gender { get; set; }
        public string ChangedUserRole { get; set; }
        public string ProfilePictureFullPath { get; set; }
    }
    public class APIUserGrade
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class APIMySupervisor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserRole { get; set; }
        public string ProfilePicture { get; set; }
        public string ProfilePictureFullPath { get; set; }
        public string EmployeeId { get; set; }
        public string Worklocation { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
    }
    public class APIUserMyTeam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
        public string ProfilePictureFullPath { get; set; }
        public string ChangedUserRole { get; set; }
        public bool notifyManagerEvaluation { get; set; }
        public int Rank { get; set; }
        public string Designation { get; set; }
        public bool trainerFlag { get; set; }
        public string Gender { get; set; }
        public string country { get; set; }
    }


}
