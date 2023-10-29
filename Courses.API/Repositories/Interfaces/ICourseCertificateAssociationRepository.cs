using Courses.API.APIModel;
using Courses.API.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ICourseCertificateAssociationRepository : IRepository<CourseCertificateAssociation>
    {
        Task<IEnumerable<APICourseCertificateAssociation>> GetAll(int page, int pageSize, string search = null);
        Task<bool>Exists(string CertificateImageName, int courseId, int? recordId,string OrgCode);
        Task<int> Count(string search = null);
        Task<List<APICourseCertificateTypeHead>> GetAllCertificateNames(string search=null);
        Task<bool> ExistsCourseCertificate(int CourseID, int UserID, int DesignationId);
        Task<CourseCertificateAuthority> AddcourseCertificate(CourseCertificateAuthority courseCertificateAuthority);
        Task<IEnumerable<APICourseCertificateAuthorityDetails>> GetAllCourseCertificateAssociation(int page, int pageSize, string search = null, string filter = null);
        Task<CourseCertificateAuthority> GetCourseCertificateAuthorities(int Id);
        Task<int> DeleteRule(int Id);
        Task<CourseCertificateAuthority> UpdatecourseCertificate(CourseCertificateAuthority courseCertificateAuthority);
        Task<int> GetAllCourseCertificateAssociationCount(string search=null, string filter=null);
        Task<List<APICourseCertificateExport>> GetAllCourseCertificateAuthoritiesForExport(int UserId);
        Task<CourseCertificateAssociation> CertificateExists(int courseID);
        Task<CertificationUpload> CerticationUpload(ApiCertificationUpload apiCertificationUpload, int UserId);

        Task<IEnumerable<CertificationUpload>> GetUserCertificatesByUserId(int userId, int page, int pageSize, string filter = null, string search=null);

        Task<int> GetUserCertificatesByUserIdCount(int userId,string filter = null,string search = null);

        Task<CertificationUpload> GetCertificateById(int Id);

        Task<CertificationUpload> UpdateIntExtCourseCertificate(CertificationUpload courseCertificate);

        Task<int> DeleteCertificate(int certificateId, int userId);

        Task<TrainingDetailsCatalog> PostTrainingDetailsCatalog(APITrainingDetailsCatalog apiTrainingDetailsCatalog, int UserId);
        Task<IEnumerable<TrainingDetailsCatalog>> GetTrainingDetails(int userId, int page, int pageSize, string filter = null, string search = null);
        Task<int> GetTrainingDetailsCount(int userId, string filter = null, string search = null);
        Task<TrainingDetailsCatalog> GetTrainingDetailsById(int Id);
        Task<TrainingDetailsCatalog> UpdateTrainingDetailsCatalog(TrainingDetailsCatalog trainingDetailsCatalog);
        Task<int> TrainingDetails(int trainingdetailsId, int userId);
        Task<List<APITrainingDetailsTypeAhead>> GetTrainingNameTypeAhead(string search = null);
        Task<FileInfo> ExportExtCertificateReport(int UserId, string OrgCode, string Search = null, string SearchText = null, bool IsExport = false);
    }
}
