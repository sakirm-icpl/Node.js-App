using Gadget.API.APIModel;
using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IIdeaApplicationApproverRepository : IRepository<IdeaAssignJury>
    {
        Task<int> CheckDuplicateInsert(int Userid, string Region, string Jurylevel);
        Task<List<IdeaAssignJury>> GetAllAssignJuryDetails(int UserId,int page,int pagesize, string search,string searchText);
        Task<int> GetAllAssignJuryDetailsCount(string search, string searchText);
        Task<APIResponse> DeleteJury(int Id);

        Task<int> AssignApplicationToJuries();
        Task<List<APIIdeaGetAllAppToJury>> GetAllApplicationForJuery(int UserId, int page, int pagesize, string search, string searchText);
        Task<int> GetAllApplicationForJueryCount(int Userid,string search, string searchText);
        Task<int> CheckandUpdate(IdeaApplicationJuryAssocation idea,int Userid); 
        Task<APIResponse> CheckProgressApplicationStatus(int JuryId, int ApplicationId);
        Task<APIResponse> PostStatusforadmin(APIApplicationStatusFromAdmin statusFromAdmin);


    }
}
