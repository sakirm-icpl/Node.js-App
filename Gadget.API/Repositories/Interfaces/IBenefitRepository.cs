using Gadget.API.APIModel;
using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IBenefitRepository : IRepository<Benefit>
    {
        Task<List<APIBenefit>> GetBenefitList();
        Task<List<APIBenefit>> GetBenefitById(int id);
        Task<List<Benefit>> GetBenefitByIdForDelete(int id);
    }
}
