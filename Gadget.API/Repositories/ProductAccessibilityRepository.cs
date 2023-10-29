using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class ProductAccessibilityRepository : Repository<ProductAccessibility>, IProductAccessibilityRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProductAccessibilityRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private GadgetDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        public ProductAccessibilityRepository(GadgetDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._customerConnectionString = customerConnectionString;
        }

        public async Task<ProductAccessibility> GetLatestProductEntry(int productId, int userId)
        {
            return this.context.ProductAccessibility.AsEnumerable().Where(x => x.ProductId == productId && x.UserId == userId).Select(x => new ProductAccessibility { Id = x.Id, UserId = x.UserId, ProductId = x.ProductId, FromDate = x.FromDate, ToDate = x.ToDate }).LastOrDefault();
        }
    }
}