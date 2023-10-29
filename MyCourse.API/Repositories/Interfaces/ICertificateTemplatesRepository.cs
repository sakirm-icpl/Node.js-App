using MyCourse.API.APIModel;
using MyCourse.API.APIModel.NodalManagement;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface ICertificateTemplatesRepository : IRepository<CertificateTemplates>
    {
        Task<bool> IsCourseCompleted(int courseId, int userId);

        Task<ApiCourseCompletionDetails> GetCourseCompletionDetails(int courseId, int userId);

        Task<string> AddCerificationDownloadDetails(int userId, int courseId, string OrgCode,string coursTitle);
        Task<List<APINodalUserDetails>> GetGroupUsers(int GroupId);
        Task<CourseCertificateData> GetAllCourseCompletionDetails(int courseId, int userId, int page, int pageSize, int loggedInUserId);
        Task<bool> GetCertificateDownloadStatus(int userId, int courseId);
    }
}
