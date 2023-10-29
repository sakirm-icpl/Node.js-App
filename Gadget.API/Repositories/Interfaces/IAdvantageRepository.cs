using Gadget.API.APIModel;
using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IAdvantageRepository : IRepository<Advantage>
    {
        Task<List<APIAdvantage>> GetAdvantageList();
        Task<List<APIAdvantage>> GetAdvantageById(int id);
        Task<List<Advantage>> GetAdvantageByIdForDelete(int id);
    }
}
