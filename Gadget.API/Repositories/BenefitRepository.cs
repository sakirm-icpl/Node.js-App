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
    public class BenefitRepository : Repository<Benefit>, IBenefitRepository
    {
        private GadgetDbContext context;
        public BenefitRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<List<APIBenefit>> GetBenefitList()
        {
            return await this.context.Benefits.Select(x => new APIBenefit { BenefitId = x.BenefitId, BenefitName = x.BenefitName, BenefitDescription = x.BenefitDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<APIBenefit>> GetBenefitById(int id)
        {
            return await this.context.Benefits.Where(x => x.ProductId == id).Select(x => new APIBenefit { BenefitId = x.BenefitId, BenefitName = x.BenefitName, BenefitDescription = x.BenefitDescription, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<Benefit>> GetBenefitByIdForDelete(int id)
        {
            return await this.context.Benefits.Where(x => x.ProductId == id).Select(x => x).ToListAsync();
        }
    }
}

