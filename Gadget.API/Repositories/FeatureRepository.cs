using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class FeatureRepository : Repository<Feature>, IFeatureRepository
    {
        private GadgetDbContext context;
        public FeatureRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<List<APIFeature>> GetFeatureList()
        {
            return await this.context.Features.Select(x => new APIFeature { FeatureId = x.FeatureId, FeatureName = x.FeatureName, FeatureDescription = x.FeatureDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<APIFeature>> GetFeatureById(int id)
        {
            return await this.context.Features.Where(x => x.ProductId == id).Select(x => new APIFeature { FeatureId = x.FeatureId, FeatureName = x.FeatureName, FeatureDescription = x.FeatureDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<Feature>> GetFeatureByIdForDelete(int id)
        {
            return await this.context.Features.Where(x => x.ProductId == id).Select(x => x).ToListAsync();
        }
    }
}
