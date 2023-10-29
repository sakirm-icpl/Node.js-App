using CourseApplicability.API.APIModel;
using CourseApplicability.API.Model;
using Courses.API.APIModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface INodalCourseRequestsRepository : IRepository<NodalCourseRequests>
    {
        Task<string> DeleteCourseRequest(int UserId, APICourseRequestDelete aPICourseRequestDelete);
        Task<FileInfo> ExportCourseRequests(int UserId, string OrgCode, string Search = null, string SearchText = null, bool IsExport = false);
        Task<FileInfo> ExportGroupCourseRequests(int UserId, string OrgCode, int? GroupId = null, string Search = null, string SearchText = null, bool IsExport = false);
        Task<List<APINodalCourseRequests>> GetCourseRequests(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false, string OrgCode = null);
        Task<int> GetCourseRequestsCount(int UserId, string Search = null, string SearchText = null, bool IsExport = false);
        Task<List<APINodalCourseRequestUserDetails>> GetGroupCourseRequests(int UserId, int Page, int PageSize, int? GroupId = null, string Search = null, string SearchText = null, bool IsExport = false);
        Task<int> GetGroupCourseRequestsCount(int UserId, int GroupId, string Search = null, string SearchText = null, bool IsExport = false);
        Task<List<APINodalCourseRequestGroupDetails>> GetRequestedCourseGroups(int UserId, int Page, int PageSize, string Search = null, string SearchText = null, bool IsExport = false);
        Task<int> GetRequestedCourseGroupsCount(int UserId, string Search = null, string SearchText = null, bool IsExport = false);
        Task<APIScormGroup> GetUserforCompletion(int GroupId);
        Task<string> GroupCourseCompletion(int UserId, List<APIGroupCourseCompletion> aPIGroupCourseCompletion);
        Task<string> InitCourse(int UserId, int CourseId, int GroupId);
        Task<APINodalRequestResponse> ProcessCourseGroupRequest(int UserId, List<APINodalRequest> aPINodalRequests, string OrgCode);
        Task<string> ProcessCourseRequest(int UserId, APINodalRequest aPINodalRequests, string OrgCode);
        Task<string> SelfRegisterCourse(int UserId, string OrgCode, APISelfCourseRequest aPISelfCourseRequest);
    }
}
