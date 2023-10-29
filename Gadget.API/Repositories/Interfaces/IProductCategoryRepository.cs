using Gadget.API.APIModel;
using Gadget.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IProductCategoryRepository : IRepository<ProductCategory>
    {
        Task<List<APIProductCategory>> GetProductCategoryList();
        Task<List<APIProductCategory>> GetProductCategoryListByUserId(int id);

        Task<APIProductCategorybyCategoryId> ProductCategorywithCategoryId(int categoryId);

        Task<bool> IsCategoryNameExists(string productCategoryName);
    }
}
