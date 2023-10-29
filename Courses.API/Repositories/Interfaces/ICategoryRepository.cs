using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> Exists(int? categoryId, string code = null, string name = null);
        Task<List<Category>> Get(int page, int pageSize, string category_name = null);
        Task<int> count(string name = null);
        Task<bool> IsDependacyExist(int categoryId);
        Task<int> GetSequenceNo();
        Task<Message> UpdateCategories(List<APICategoryDTO> apiCategories);
        Task<IEnumerable<Category>> GetAllCategoriesBySequenceNo();
        Task<List<Category>> checkcategorycache(string cacheKeyConfig);
        Task<IEnumerable<Category>> GetAllApplicableCategories(int UserId);
        Task<IEnumerable<SubCategory>> GetAllApplicableSubCategories(int UserId, int CategoryId);
        Task<List<Category>> GetallCategories();
        Task<List<Category>> GetCategoryTypeAhead(string search = null);
        Task<List<SubCategory>> GetSubCategoryTypeAhead(int CategoryId, string search = null);
        Task<dynamic> GetCategoies(int id);
        Task<dynamic> GetTnaCategories();
    }
}
