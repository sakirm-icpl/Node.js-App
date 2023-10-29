
using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<bool> Exists(string name, int? moduleId = null);
       

    
    }
}
