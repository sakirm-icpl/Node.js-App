//======================================
// <copyright file="IUserRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.Extensions.Configuration;
using Payment.API.APIModel;
using Payment.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using static Payment.API.Models.UserMaster;

namespace Payment.API.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<UserMaster>
    {
        Task<string> GetEncryptedUserId(int userid);
        Task<bool> IsExists(string userID);
        Task<bool> IsEmployeeCodeExists(string userID, string OrgCode);
        Task<bool> EmailExists(string emailId);
        Task<bool> EmailExists1(string emailId, string OrgCode);
        Task<bool> UserIdExists(string userid);
        Task<bool> EmailUserExists(string emailId, string userId);
        Task<bool> UserNameExists(string userName);
        Task<bool> MobileExists(string mobileNumber);
        Task<bool> MobileExists1(string mobileNumber, string OrgCode);
        Task<string> GetEmailByMobileNumber(string mobileNumber);
        Task<bool> MobileuserExists(string mobileNumber, string userId);
        Task<IEnumerable<APIUserMaster>> GetAllUser(int page, int pageSize, string search = null, string columnName = null, string status = null, int? userId = null, string encryptUserId = null, string OrgCode = null);
        Task<APIUserMaster> GetUser(int id, string decryptUserId = null);
        Task<APIUserMasterDetails> GetUserDetailsById(string id, string decryptUserId = null);
        Task<APIUserProfile> GetUserProfile(int id, string decryptUserId = null);
        Task<int> GetIdByUserId(string userId);
        Task<bool> IsUserExists(string userId, string OrganisationString);
        Task<int> GetIdByLocation(string Location);
        Task<int> GetIdByGroup(string txtGroup);
        Task<int> GetIdByBusiness(string txtbusiness);
        Task<int> GetIdByArea(string txtArea);
        //Task<IEnumerable<APIUserTeam>> GetTeam(string email);
        //Task<IList<APIUserMyTeam>> GetMyTeam(string email, string OrgCode = null);
        //Task<APIMySupervisor> GetUserSupervisor(string email);
        //Task<APIUserTeam> GetUserSupervisorId(string email);
        //Task<APIUserTeam> GetUserData(int id);
        //Task<IEnumerable<ApiUserSearchV2>> SearchUserType(string userId, string UserType = null);
        //Task<ApiResponse> CheckForValidUser(string userId, string UserType = null);
        //Task<IEnumerable<ApiUserSearch>> SearchByUserRole(int tokenId, string userId);
        Task<APIUserMaster> GetUserObject(APIUserMaster user, string UserRole, string OrgCode);
        Task<bool> IsUserRoleExist(string UserRole);
        bool IsUserChanged(APIUserMaster oldObject, APIUserMaster newObject);
        Task<int> GetUserCount(string search = null, string columnName = null, string status = null, int? userId = null, string encryptUserId = null);
        Task<int> GetTotalUserCount();
        Task<int> AddUser(APIUserMaster Apiuser, string UserRole, string OrgCode, string IsInstitute);
        Task<int> AddUserImport(APIUserMaster Apiuser, string UserRole, string IsInstitute);
        Task<int> UpdateUserImport(APIUserMaster Apiuser);
        Task<string> GetCustomerCodeByEmailId(string emailId);
        Task<int> GetUserIdByEmail(string email);
        //Task<IEnumerable<APIUserDto>> GetUserNameAndId(string name);
        //Task<IEnumerable<APIUserDto>> GetAccountManagers(string name);
        Task<APIForgetPassword> ForgotPassword(string userId);
        Task<string> GetCslEmpCode(int userId);
        Task<bool> CheckForAuthOTP(string userId);
        Task<APIUserMaster> GetUserByUserId(string userId);
        Task<APIForgetPassword> GetEmailTemplate(string customerCode);

        //Task<List<APIUserDto>> GetSearchParameter(string searchBy, string searchText = null);
        Task<object> Search(string searchBy, string searchText, string orgCode, int userId);

        Task<string> GetUserName(int Id);
        Task<object> GetNameById(string searchBy, int Id);

        //Task<List<APIGetBusinessDetails>> GetBusinessDetails(int userid);
        //Task<string> BusinessDetailsPost(APIBusinessDetails[] businesspostmultiple, int userid);
        //Task<int> DeleteBusinessDetails(APIBusinessDetails businesspostmultiple);
        //Task<List<string>> GetDecryptedValues(DecryptedValues decryptedValues);
        Task<string> ProcessImportFile(APIFilePath aPIFilePath, int userId, string orgCode);
        Task<APIUserMaster> GetByEmailOrUserId(string userId);
        Task<bool> ExistsForUpdate(string searchOn, string searchText, int userId);
        Task<bool> IsUniqueFieldExist(string emailId, string mobile, string userId, int id);
        Task<Exists> Validations(APIUserMaster user);
        Task<int> Delete(int id, int userId);
        //Task<UserMasterDetails> GetUserDetails(int userId);
        //Task<int> UpdateUserDetails(UserMasterDetails userDetails);
        //Task<UserMasterDetails> GetDetailsByEmailOrUserId(string userId);
        Task<int> UpdateUserPasswordHistory(string userid);
        Task<int> UpdateUser(APIUserMaster apiUser);
        Task<int> UpdateUserProfile(APIUpdateUserProfile apiUser, string OrgCode);
        Task<int> UserPatch(APIUpdateUserMobileEmail apiUser, string OrgCode, int userid);
        bool IsUniqueDataIsChanged(APIUserMaster oldUser, APIUserMaster user);
        void CloseConnection();
        Task<APISectionAdminDetails> GetSectionAdminDetails(int userId);
        Task<bool> IsActiveUserExist(string emailId, string mobile, string userId, int id);
        Task<bool> UserDataExist(string column);
        Task<bool> TermsConditionsAccepted(int userId);
        Task<bool> UserAccountLocked(int userId);
        void ChangeDbContext(string connectionString);
        Task<int> AddUserHistory(APIUserMaster Olduser, APIUserMaster user);
        Task<bool> IsPasswordModified(int userId);
        Task<int?> LoginReminderDays(int userId, string OrgCode);
        Task<int?> LoginPasswordChangeReminderDays(int userId, string OrgCode);
        Task<string> GetUserRole(int userId);
        Task<APIUserMaster> GenerateCustomerCode(APIUserMaster Apiuser, int UserId);
        //Task<APIUserConfiguration> GetUserConfiguration(string userid, int id, string OrgCode);
        //Task<APIUserDashboardConfiguration> GetUserDashboardConfiguration(string userid, int id, string OrgCode);
        Task<string> GetEmail(int Id);
        Task<string> GetReportToEmail(int Id);
        //Task<int> AddUserHRAssociation(APIUserHRAssociation aPIUserHRAssociation, int UserID);
        //Task<IEnumerable<UserHRAssociation>> GetHRAssociationData(int page, int pageSize);
        //Task<int> GetHRCount();
        //Task<UserHRAssociation> GetUserHRA(int id);
        //Task<int> UpdateUserHRA(UserHRAssociation userHRAssociation);
        Task<int> HRADelete(int id);


        Task<IEnumerable<APIDynamicColumnDetails>> GetDynamicColumnDetails(string columnName);
        Task<string> GetDecryptUserId(int id);
        Task<bool> IsLDAP();
        //Task<List<TypeHeadDto>> GetUsersForTA(int userId, string search = null);
        //Task<int> AddUserAdminTrainingAssociation(APIUserTrainingAdminAssociation aPIUserTrainingAdminAssociation, int UserID);
        //Task<IEnumerable<UserTrainingAdminAssociation>> GetTrainingAdminAssociationData(int page, int pageSize);
        //Task<int> GetTrainingAdminCount();
        Task<int> TrainingAdminDelete(int id);
        //Task<List<TypeHeadDto>> GetUsersOfDept(int deptId, string search = null);
        //Task<bool> IsExistsTrainingAdmin(int DepartmentId, int UserMasterId);
        //Task<bool> IsExistsUserHR(int UserMasterId);
        //Task<int> AddOrganizationPrefernces(APIOrganizationPreferences aPIOrganizationPreferences, int UserID, string OrgCode);
        //Task<OrganizationPreferences> GetOrganizationLogo();
        //Task<int> UpdateOrganizationPrefernces(int Id, APIOrganizationPreferences aPIOrganizationPreferences, int UserID, string OrgCode);
        //Task<int> UpdateProfilePicture(string fileName, int UserId);
        //Task<string> GetEmailUserExists(string UserId);
        //Task<string> UpdateReportTo(string oldEmailId, string EmailId);
        //Task<List<LoggedInHistory>> GetLoggedInHistory(int? UserID, int page, int pageSize);
        Task<int> LoggedInHistoryCount(int? UserID);
        Task<int> ResetFailedLoginStatistics(string userId);
        Task<APIUserMaster> GetByUserIdNew(string userId);
        Task<Exists> ValidationsDuplicateUserid(APIUserMaster user);
        Task<string> GetMasterConfigurableParameterValueByConnectionString(string configurationCode, string OrgnizationConnectionString);
        Task<List<APIUserId>> GetAllUserList();
        Task<List<ApiMobileNumber>> GetAllMobileNumber();
        Task<bool> EmailUserIdExists(string EmailID, string userId);
        //Task<UserHRAssociation> GetUserHR(int id);
        //Task<HouseMaster> GetHouseMaster(int id);
        //Task<int> UpdateHouseMaster(HouseMaster housemaster);
        //Task<int> UpdateUserHR(UserHRAssociation userHRAssociation);
        //Task<List<APIUserHRAssociation>> GetHRByID(int id);
        //Task<List<HouseMaster>> GetHouseMasterByID(int id);
        //Task<List<HouseMaster>> GetAllHouseMaster();
        Task<string> GetUserType(string userId);
        Task<Boolean> checkuserexistanceinldap(string uid);
        Task<String> resetPasswordWeb(string uid, string dob, string npass, string cpass);
        DataTable CheckResultString(string resultString, DataTable dt, DataRow dr, string empStatus);
        Task<List<string>> passwordchanged(int userid, string OrgCode);
        Task<bool> IsOldPassword(int userId, string password);
        //Task<int> AddPasswordHistory(PasswordHistory PasswordHistory);
        //Task<UserMasterOtp> GetUserMasterotp(int userid);
        //Task<int> AddUpdateUserMasterOTP(UserMasterOtp UserMasterOtp);
        Task<FileInfo> ExportUserData(string columnName, string status, string search, int userid, string UserName, string FIXED_JOB_ROLE, string OrgCode = null);

        Task<APIUserMasterProfile> UserProfileUpdate(int id, string decryptUserId = null);

        //Task<APIGetBespokeUserDetails> GetUserByBespoke(int userid);
        Task<List<ApiGetUserName>> GetUsernamefromgroup(string groupname, string username);
        Task<bool> ExistsCode(string Code);
        //Task<List<DesignationRoleMapping>> GetAllDesignationRoleList();
        Task<int> UpdateImport(APIUserMaster apiUser, string status);
        //Task<string> ProcessImportFile(FileInfo file, IUserSettingsRepository _userSettingsRepository,
        //   IUserRepository _userRepository,
        //   IUserMasterRejectedRepository _userMasterRejectedRepository,
        //   IConfiguration _configuration, ICustomerConnectionStringRepository _customerConnectionStringRepository);

        //Task<List<APIUsersStatus>> GetAllUsersStatus();


        //Task<IEnumerable<APIUserMasterDelete>> GetAllDeletedUser(int page, int pageSize, string search = null, string columnName = null, int? userId = null, string encryptUserId = null);
        //Task<int> GetDeletedUserCount(string search = null, string columnName = null, int? userId = null, string encryptUserId = null);
        //Task<APIUserMasterDelete> GetDeletedUserInfo(int id, string decryptUserId = null);
        //Task<UserRejectedStatus> AddStatus(APIUserRejectedStatus apiuserRejectedStatus);
        //Task<List<UserRejectedStatus>> GetAllUserRejectedStatus();

        //Task<List<APICourse>> GetApplicableCoursesId(int UserId, bool IsActive);
        //Task<IEnumerable<APIMobilePermissions>> GetMobilePermissions(string RoleCode, int UserId);
        //Task<APIGetUserDetails> GetUserDetailsByUserId(string UserId);
        //Task<APIUserDetails> GetOAuthUser(string userName);
        //Task<object> GetConfigurationColumsData(string searchBy);

        Task<int> GetIdByJobName(string txtJobName);
        Task<int> InActiveUser(int id);
        Task<int> AddUserSignUp(APIUserSignUp Apiuser, string UserRole, string OrganisationString);
        //Task<int> AddUserSignUpOTP(SignUpOTP userotp, string OrganisationString);
        //Task<IEnumerable<Group>> GetAllConfigurationGroups(int UserId, string search);

        //Task<List<APIUsersByLocation>> GetUsersByLocation(int locationId, string search = null);
        //Task<List<APISearchUserforApplication>> GetSearchApplicationTypeAhead(string search = null);
        Task<List<string>> GetRegion();
        Task<List<string>> GetDepartment();

        Task<List<AppConfiguration>> GetUserConfigurationValueAsync(string[] str_arr, string orgCode, string defaultValue = "");
        Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "");
        Task<APIUserSignUp> GetUserIdByEmailId(string emailid, string OrgCode);
        //Task<int> AddVFSUserSignUp(APIVFSUserSignUp Apiuser, string UserRole, string OrganisationString);

        //Task<Exists> ValidationsForUserVFSSignUp(APIVFSUserSignUp user);
        //Task<Exists> ValidationsDuplicateUseridForVFSSignUp(APIVFSUserSignUp user, string Orgcode);
        //Task<APIVFSUserSignUp> AddUserToDbForVFSSignUp(APIVFSUserSignUp apiUser, string UserRole);
        Task<int> GetIdByEmailIdAndUserName(string emailid, string userid);
        Task<bool> CheckUserTypeForAuthOTP(string userId);
        Task<int> GetTotalUserCount1(string OrgCode);
        Task<APICompetencyJdUpload> GetCompetencyJdView(int JobRoleId);
        Task<object> GetConfigurationColumsDataForDashboard(string columnName);
        Task<string> GetEmailByUserId(string UserId);
        //Task<int> PostTeamsCred(ApiTeamsCred apiTeamsCred, int userid);
        //Task<int> EditTeamscred(ApiTeamsCred apiTeamsCred, int userid);
        //Task<int> DeleteTeamsUser(string TeamsEmail);
        //Task<ApiResponse> GetTeamsCrediential(int UserId);
        //Task<ApiResponse> GetDefaultTeamsCrediential();
        //Task<IEnumerable<APIGetUserMasterId>> GetUserMasterId(string UserId);
        //Task<APIUserGrade> GetUserGrade(int id);
        //Task<int> PostZoomCred(ApiZoomCred apiZoomCred, int userid);
        //Task<ApiResponse> GetZoomCrediential(int UserId);
        //Task<ApiResponse> GetDefaultZoomCrediential();
        //Task<int> EditZoomcred(ApiZoomCred apiZoomCred, int userid);
        Task<int> DeleteZoomUser(string TeamsEmail);
        Task<bool> IsUniqueFieldExistWithoutMobile(string emailId, string mobile, string userId, int id);
        //List<UserTeamApplicableUser> GetUsersForuserTeam(string TeamCode);
        //Task<ApiResponse> GetDefaultGSuitCredientials(int UserId);
        //Task<ApiResponse> GetGsuitCrediential(int UserId);
        //Task<int> PostGsuitCred(ApiGsuitCred apiGsuitCred, int userid);
        //Task<int> EditGuitcred(ApiGsuitCred apiGsuitCred, int userid);
        Task<int> DeleteGsuitUser(string Email);
        Task<List<LocationandAreaSearch>> SearchUserName(string search = null);
        string RandomProfileImage(string gender);

        //Task<IEnumerable<ApiUserSearchV2>> SearchAllUser(string userId, string UserType = null);
    }
}
