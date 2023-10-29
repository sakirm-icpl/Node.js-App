using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Models;

namespace User.API.Repositories.Interfaces
{
    public interface INodalGroupManagementRepository : IRepository<NodalUserGroups>
    {
        Task CancelGroupCode(APIGroupCode aPIGroupCode, int UserId);
        Task<string> DeleteGroup(APINodalGroupDelete aPINodalGroupDelete, int UserId);
        Task<string> DeleteUser(APINodalUserDelete aPINodalUserDelete, int UserId);
        Task<byte[]> ExportImportFormat(string OrgCode);
        Task<List<APICourseGroupUsers>> GetApprovedCourseGroupUsers(int UserId, int GroupId, int Page, int PageSize, string Search = null, string SearchText = null);
        Task<int> GetApprovedCourseGroupUsersCount(int UserId, int GroupId, string Search = null, string SearchText = null);
        Task<List<APICourseGroups>> GetCourseGroups(int UserId, int Page, int PageSize, string Search = null, string SearchText = null);
        Task<int> GetCourseGroupsCount(int UserId, string Search = null, string SearchText = null);
        Task<List<APICourseGroupUsers>> GetCourseGroupUsers(int UserId, int GroupId, int Page, int PageSize, string Search = null, string SearchText = null);
        Task<int> GetCourseGroupUsersCount(int UserId, int GroupId, string Search = null, string SearchText = null);
        Task<List<APICourseRegistrations>> GetCourseRegistrations(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false);
        Task<int> GetCourseRegistrationsCount(int UserId, string Search = null, string SearchText = null, bool IsExport = false);
        Task<GroupCode> GetGroupCode(int UserId);
        Task<APIPaymentRequestData> MakePayment(int UserId, string RequestId);
        Task<ApiResponse> ProcessImportFile(APINodalUserGroups aPINodalUserGroups, int UserId, string OrgCode);
    }
}
