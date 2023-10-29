using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gadget.API.Models;
using Gadget.API.APIModel;
using Microsoft.AspNetCore.Http;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IProjectRepository :IRepository<ProjectMaster>
    {
        Task<int> GetCountByUser(int Userid);
        Task<List<APITileDetails>> GetTileDetails(string Search);
        Task<APIGetProjectTeam> SaveTeamMemberDetails(APIProjectTeamDetails aPIProjectTeamDetails,int Userid);
        Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode, string ApplicationCode);
        Task<APISaveProjectApplication> SaveProjectApplication(APISaveProjectApplication aPISaveProject, int Userid);
        Task<APIGetProjectAppDetails> GetProjectUserAppDetails(int UserId);
        Task<int> GetCountByUserApplication(int Userid);
        Task<List<APIGetUserProjectReport>> GetUserProjectReport(int UserId);
        Task<APIProjectMaster> GetNominationUserDetails(int Id);
        Task<APIGetProjectAppDetails> GetApplicationDetailsbyId(int Id,int UserId);
        Task<List<APIGetAllProjectApplicationList>> GetAll(int userId,int page, int pageSize , string filter = null, string search = null);
        Task<int> GetAllCount(string filter = null, string search = null);

    }
}
