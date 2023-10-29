using Competency.API.Model.Competency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface ICompetencyReviewParametersRepository: IRepository<CompetencyReviewParameters>
    {
        Task<bool> Exists(string review);
        Task<IApiResponse> GetAllCompetencyReviewParameters(ReviewParametersPostModel postModel);
        Task<IApiResponse> GetCompetencyReviewParameters(GetReviewParametersPostModel postModel);
        Task<IApiResponse> GetCompetencyReviewParametersOptions();
        Task<IApiResponse> SaveAssessment(APICompetencyReviewParametersSelfAssessment postModel,int UserId);
        Task<IApiResponse> SaveAssessmentUpdate(APICompetencyReviewParametersSelfAssessment postModel, int UserId);
        Task<IApiResponse> SaveSupervisorAssessment(CompetencySupervisorUpdate postModel, int UserId);
        Task<IApiResponse> GetUserSelfAssessment(int userid);
        Task<IApiResponse> getCompetencyReviewParametersSupervisorOptions();
        Task<IApiResponse> GetSelfRatingforSupervisor(SelfRatingForSupervisor selfRatingForSupervisor);
        Task<IApiResponse> GetLastAssessmentDate(UserIdPayload selfRatingForSupervisor);
    }
}
