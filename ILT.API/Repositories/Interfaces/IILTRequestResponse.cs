using ILT.API.APIModel;
using ILT.API.Model.ILT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IILTRequestResponse : IRepository<ILTRequestResponse>
    {
        Task<List<APIILTRequestResponse>> GetRequest(int UserId, string RoleCode, int page, int pageSize, string searchParameter = null, string searchText = null);
        Task<int> GetRequestedUserCount(int UserId, string RoleCode, string searchParameter = null, string searchText = null);
        Task<APIILTRequestRsponse> GetAllRequestDetails(int moduleId, int courseId, int userId);
        Task<APIILTRequestRsponse> GetWNSAllRequestDetails(int moduleId, int courseId, int userId);
        Task<APIILTRequestRsponse> GetRequestDetails(int moduleId, int courseId, int userId);
        Task<string> PostRequest(APIILTRequestResponse aPIILTRequestResponse, int UserId, string OrganizationCode);
        Task<string> PostResponse(APIILTRequestResponse aPIILTRequestResponse, int UserId, string organizationcode);
        Task<List<APIILTBatchResponse>> GetBatchesForRequest(int CourseId, int UserId);
        Task<string> PostBatchRequest(APIILTBatchRequestResponse aPIILTBatchRequestResponse, int UserId, string OrganizationCode);
        Task<List<APIILTBatchRequests>> GetBatchRequests(int UserId, string RoleCode, int batchId, int page, int pageSize, string searchParameter = null, string searchText = null);
        Task<int> GetBatchRequestsCount(int UserId, string RoleCode, int batchId, string searchParameter = null, string searchText = null);
        Task<string> PostBatchResponse(List<APIILTBatchRequestApprove> aPIILTBatchRequestApprove, int UserId, string OrgCode);
        Task<List<APIILTRequestedBatches>> GetAllRequestedBatches(int UserId, string RoleCode, int page, int pageSize, string searchParameter = null, string searchText = null);
        Task<int> GetAllRequestedBatchesCount(int UserId, string RoleCode, string searchParameter = null, string searchText = null);
        Task<ILTSchedule> GetSchedulePurpose(int id);
        Task<string> PostDropOutRequest(APIILTRequestResponse aPIILTRequestResponse, int UserId, string OrganizationCode);
        Task<ILTRequestResponse> GetRequestResponse(APIILTRequestResponse aPIILTRequestResponse, int UserId);
    }
}
