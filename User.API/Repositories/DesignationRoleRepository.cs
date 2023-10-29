
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Models;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class DesignationRoleRepository : Repository<DesignationRoleMapping>, IDesignationRoleRepository
    {
        private UserDbContext _db;
        public DesignationRoleRepository(UserDbContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<DesignationRoleMapping>> GetAllDesignations(int page, int pageSize, string search = null)
        {
            IQueryable<DesignationRoleMapping> Query = this._db.DesignationRoleMapping;

            if (!string.IsNullOrWhiteSpace(search) && (search != "null"))
            {
                Query = Query.Where(r => ((r.UserRole.Contains(search))));

            }


            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);

            return await Query.ToListAsync();
        }

      
        public async Task<int> Count(string search = null)
        {


            var result = (from designationrole in _db.DesignationRoleMapping

                          select new APIDesignationRoleMapping
                          {
                              Id = designationrole.Id,
                              UserRole = designationrole.UserRole

                          });
            if (!string.IsNullOrWhiteSpace(search))
                return await result.Where(a => a.UserRole.StartsWith(search)).CountAsync();
            return await result.CountAsync();
        }
    }
}

