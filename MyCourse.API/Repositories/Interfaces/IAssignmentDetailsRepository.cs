using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IAssignmentDetailsRepository : IRepository<AssignmentDetails>
    {
        Task<AssignmentDetails> GetAssignmentDetail(int id);
        Task<List<ApiAssignmentInfo>> GetAssignmentDetails(int loginUserId, SearchAssignmentDetails searchAssignmentDetails);
        FileInfo GetReportExcel(IEnumerable<ApiAssignmentInfo> apiAssignmentInfo);
        Task<string> UpdateAdssignmentDetail(ApiAssignmentDetails apiAssignmentDetails, string OrgCode = null);
        Task<GetUserInfo> GetUserMasterInfo(string UserName);
        void Delete();
        Task<string> ProcessImportFile(FileInfo file, IAssignmentDetailsRepository _assignmentRepository, ICourseRepository _courseRepository,// IAssessmentQuestionRejectedRepository _assessmentQuestionBankRejectedRepository,               
             int userid, string OrganisationCode);

       Task<AssignmentDetailsRejected> AddAssignmentDetailsRejected(AssignmentDetailsRejected apiAssignmentDetails);

        Task<IEnumerable<AssignmentDetailsRejected>> GetAssignmentDetailsRejected(int page, int pageSize, string search = null);
        Task<int> Count(string search = null);

         void TRUNCATEAssignmentDetailsRejected();
    }

}
