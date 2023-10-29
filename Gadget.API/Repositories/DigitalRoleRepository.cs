using Gadget.API.Data;
using Gadget.API.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gadget.API.APIModel;

namespace Gadget.API.Repositories
{
    public class DigitalRolesListRepository : Repository<DigitalRole>, IDigitalRolesList
    {
        private GadgetDbContext _db;
        public DigitalRolesListRepository(GadgetDbContext context) : base(context)
        {
            _db = context;
        }



        public async Task<APIDigitalRolesListandCount> GetDigitalRoles(int page, int pageSize, string search)
        {
            var Query = (from x in _db.DigitalRole

                         orderby x.CreatedDate descending
                         where x.IsDeleted == false
                         select new APIDigitalRole
                         {
                             Id = x.Id,
                             Code = x.Code,
                             Description = x.Description,
                             IsActive = x.IsActive,
                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.Description.Contains(search) || r.Code.Contains(search)));
            }

            APIDigitalRolesListandCount ListandCount = new APIDigitalRolesListandCount();
            ListandCount.Count = Query.Distinct().Count();
            ListandCount.DigitalRoleListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            return ListandCount;
        }
    }
}