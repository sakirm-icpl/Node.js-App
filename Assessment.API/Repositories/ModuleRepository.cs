using log4net;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Models;
using Microsoft.EntityFrameworkCore;
using Assessment.API.Model;

namespace Assessment.API.Repositories
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleRepository));
        private AssessmentContext _db;
        ICustomerConnectionStringRepository _customerConnection;

        public ModuleRepository(AssessmentContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
        }

        public async Task<bool> Exists(string name, int? moduleId = null)
        {
            IQueryable<Module> Query = _db.Module;
            if (moduleId != null)
                Query = Query.Where(m => m.Id != moduleId);
            Query = Query.Where(m => m.IsDeleted == false && m.Name.ToLower().Equals(name));
            if (await Query.CountAsync() > 0)
                return true;
            return false;
        }
    }
}