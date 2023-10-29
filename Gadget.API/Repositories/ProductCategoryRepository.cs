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
    public class ProductCategoryRepository : Repository<ProductCategory>, IProductCategoryRepository
    {
        private GadgetDbContext context;
        public ProductCategoryRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }
        
        public async Task<List<APIProductCategory>> GetProductCategoryList()
        {
            return await this.context.ProductCategorys.Select(x => new APIProductCategory { ProductCategoryId = x.ProductCategoryId, ProductCategoryName = x.ProductCategoryName }).ToListAsync();
        }

        public async Task<List<APIProductCategory>> GetProductCategoryListByUserId(int id)
        {
            return await this.context.ProductCategorys.Where(x => x.CreatedBy == id).Select(x => new APIProductCategory { ProductCategoryId = x.ProductCategoryId, ProductCategoryName = x.ProductCategoryName }).ToListAsync();
        }
        public async Task<APIProductCategorybyCategoryId> ProductCategorywithCategoryId(int categoryid)
        {
            return await this.context.ProductCategorys.Where(x => x.ProductCategoryId == categoryid).Select(x => new APIProductCategorybyCategoryId { productCategoryId = x.ProductCategoryId, productCategoryName = x.ProductCategoryName }).FirstAsync();
        }

        public async Task<bool> IsCategoryNameExists(string productCategoryName)
        {
            if (await this.context.ProductCategorys.Where(x => x.ProductCategoryName == productCategoryName).CountAsync() > 0)
                return true;
            return false;
        }
    }
}
