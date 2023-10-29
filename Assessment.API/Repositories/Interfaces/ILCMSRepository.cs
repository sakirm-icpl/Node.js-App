using Assessment.API.APIModel;
using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface ILCMSRepository : IRepository<LCMS>
    {
        Task<bool> Exist(string fileName, string contentType, int? id = null);
        Task<LCMS> GetLcmsByAssessmentConfigureId(int AssesmentConfigId);
    }

}
