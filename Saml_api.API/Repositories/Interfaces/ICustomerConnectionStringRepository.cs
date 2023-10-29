﻿using System.Threading.Tasks;
using Saml.API.Data;
using Saml.API.Models;
using Saml.API.Repositories.Interfaces;

namespace Saml.API.Repositories.Interfaces
{
    public interface ICustomerConnectionStringRepository : IRepository<CustomerConnectionString>
    {
        Task<string> GetConnectionString(string clientId);
        void SetDbContext(UserDbContext db);
        Task<string> GetConnectionStringByOrgnizationCode(string OrgnizationCode);
        UserDbContext GetDbContext();
        UserDbContext GetDbContext(string ConnetionString);

        UserDbContext GetDbContextByOrgCode(string orgCode);
    }
}
