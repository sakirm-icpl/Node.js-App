using Gadget.API.APIModel;
using Gadget.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<APIProduct>> GetProductList();
        Task<List<APIProduct>> GetProductByCategoryId(int id);
        Task<List<APIProduct>> GetProductByUserId(int id);
        Task<Product> SaveImage(IFormFile uploadedFile, Product ProductObj,string orgCode);
        void DeleteProductWithFAB(int ProductId);
        bool DeleteFile(string filePath);

        void EditProduct(Product obj);
    }
}
