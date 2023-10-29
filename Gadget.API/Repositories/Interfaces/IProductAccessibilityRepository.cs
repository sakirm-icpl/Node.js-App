using Gadget.API.APIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories.Interfaces
{
    public interface IProductAccessibilityRepository : IRepository<ProductAccessibility>
    {
        Task<ProductAccessibility> GetLatestProductEntry(int productId, int userId);
    }
}