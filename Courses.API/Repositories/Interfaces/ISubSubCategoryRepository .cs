using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ISubSubCategoryRepository : IRepository<SubSubCategory>
    {
        Task<List<APICourseSubSubCategory>> Get(int page, int pageSize, string category_name = null, int? categoryId = null, int? subcategoryId = null);
        Task<List<APICourseSubSubCategory>> GetSubCategory(int category);
        Task<List<APICourseSubSubCategory>> GetAllSubSubcategories(int? categoryId = null);
        Task<int> count(string name = null, int? categoryId = null, int? subcategoryId = null);
        Task<bool> Exists(int? Id, int categoryId, string code = null, string name = null);
        Task<List<APICourseSubSubCategory>> GetSubSubCategoty(int categoryId);
        Task<bool> IsDependecyExists(int? id);
        Task<Message> UpdateSubSubCategories(List<APISubSubCategoryDTO> apiSubCategories);

        void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId);
        Task<int> GetSequenceNo();
    }
}
