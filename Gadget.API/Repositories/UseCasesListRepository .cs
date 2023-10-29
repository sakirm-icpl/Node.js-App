using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Repositories.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Gadget.API.Repositories
{
    public class UseCasesListRepository : Repository<UseCase>, IUseCasesList
    {
        private GadgetDbContext _db;
        public UseCasesListRepository(GadgetDbContext context) : base(context)
        {
            _db = context;
        }

        public async Task<APIUseCasesListandCount> GetUseCases(int page, int pageSize, string search)
        {
            var Query = (from x in _db.UseCase
                         orderby x.CreatedDate descending
                         where x.IsDeleted == false
                         select new APIUseCase
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

            APIUseCasesListandCount ListandCount = new APIUseCasesListandCount();

            ListandCount.Count = await Query.CountAsync();
            ListandCount.UseCaseListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();

            return ListandCount;
        }

    }
}