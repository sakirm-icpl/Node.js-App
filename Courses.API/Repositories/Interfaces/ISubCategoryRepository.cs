using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ISubCategoryRepository : IRepository<SubCategory>
    {
        Task<List<APICourseSubCategory>> Get(int page, int pageSize, string category_name = null, int? categoryId = null);
        Task<List<APICourseSubCategory>> GetCategory(int category);
        Task<List<APICourseSubCategory>> GetAllSubcategories(int? categoryId = null);
        Task<int> count(string name = null, int? categoryId = null);
        Task<bool> Exists(int? Id, int categoryId, string code = null, string name = null);
        Task<List<APICourseSubCategory>> GetSubCategoty(int categoryId);
        Task<bool> IsDependecyExists(int? id);
        Task<Message> UpdateSubCategories(List<APISubCategoryDTO> apiCategories);
        Task<int> GetSequenceNo();
    }
}
