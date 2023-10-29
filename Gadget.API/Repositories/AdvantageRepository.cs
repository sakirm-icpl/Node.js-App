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
    public class AdvantageRepository : Repository<Advantage>, IAdvantageRepository
    {
        private GadgetDbContext context;
        public AdvantageRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<List<APIAdvantage>> GetAdvantageList()
        {
            return await this.context.Advantages.Select(x => new APIAdvantage { AdvantageId = x.AdvantageId, AdvantageName = x.AdvantageName, AdvantageDescription = x.AdvantageDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<APIAdvantage>> GetAdvantageById(int id)
        {
            return await this.context.Advantages.Where(x => x.ProductId == id).Select(x => new APIAdvantage { AdvantageId = x.AdvantageId, AdvantageName = x.AdvantageName, AdvantageDescription = x.AdvantageDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<Advantage>> GetAdvantageByIdForDelete(int id)
        {
            return await this.context.Advantages.Where(x => x.ProductId == id).Select(x => x).ToListAsync();
        }
    }
}
