using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface IUserTeamsRepository
    {
        Task<int> SaveUserTeams(UserTeams userTeams);
        Task<IEnumerable<UserTeams>> GetAllUserTeams(int page, int pageSize, string search = null, string columnName = null, int? userId = null);
        int DeleteUserTeams(string TeamCode);
        UserTeams GetUserTeamsByTeamsCode(string TeamCode);
        Task<int> UpdateUserTeams(UserTeams userTeams, int UserId);
        Task<int> GetAllUserTeamsCount(int page, int pageSize, string search = null, string columnName = null, int? userId = null);
        Task<List<APIUserTeamsType>> GetUserTeams(string search);
        List<UserTeamApplicableUser> GetUsersForuserTeam(string TeamCode);
        Task<List<Mappingparameter>> CheckmappingStatus(MappingParameters mappingParameters, int UserId);
        Task<List<MappingParameters>> GetAccessibilityRules(int UserTeamsId, string orgnizationCode, string token, int Page, int PageSize);
        Task<int> GetAccessibilityRulesCount(int UserTeamsId);
        Task<int> DeleteRule(int roleId);
        ApiResponse ProcessImportFile(FileInfo file, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode);
        Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int UserTeamsId);
        List<UserTeamsMapping> GetRuleByUserTeams(int UserTeamId);
        FileInfo GetApplicableUserListExcel(List<MappingParameters> aPIAccessibilityRules, List<UserTeamApplicableUser> userTeamApplicableUsers, string CourseName, string OrgCode);
        UserTeams getUserById(int? TeamId);
    }
}
