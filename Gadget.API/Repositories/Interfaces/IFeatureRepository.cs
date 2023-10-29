using Gadget.API.APIModel;
using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IFeatureRepository : IRepository<Feature>
    {
        Task<List<APIFeature>> GetFeatureList();
        Task<List<APIFeature>> GetFeatureById(int id);
        Task<List<Feature>> GetFeatureByIdForDelete(int id);
    }
}
