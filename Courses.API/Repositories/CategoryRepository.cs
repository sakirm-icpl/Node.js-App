using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Courses.API.Helper;
using log4net;



namespace Courses.API.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CategoryRepository));
        private CourseContext _db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;

        public CategoryRepository(CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
        {
            _db = context;
            _customerConnectionStringRepository = customerConnectionStringRepository;
        }

        public async Task<bool> Exists(int? categoryid, string code, string name)
        {
            if (categoryid == null)
            {
                if (await _db.Category.CountAsync(y => y.Code == code) > 0)
                    return true;
                else if (await _db.Category.CountAsync(y => y.Name == name) > 0)
                    return true;
            }
            else
            {
                if (await _db.Category.Where(y => y.Id != categoryid).CountAsync(y => y.Code == code) > 0)
                    return true;
                else if (await _db.Category.Where(y => y.Id != categoryid).CountAsync(y => y.Name == name) > 0)
                    return true;
            }

            return false;
        }
        public async Task<List<Category>> checkcategorycache(string cacheKeyConfig)
        {
            CacheManager.CacheManager cache = new CacheManager.CacheManager();
            List<Category> rewardPoints = null;
            if (cache.IsAdded(cacheKeyConfig))
            {
                rewardPoints = cache.Get<List<Category>>(cacheKeyConfig);
                return rewardPoints;
            }
            else
            {
                return null;
            }

        }
        public async Task<List<Category>> Get(int page, int pageSize, string search = null)
        {

            IQueryable<Category> catQuery = _db.Category;
            if (!string.IsNullOrEmpty(search))
                catQuery = catQuery.Where(r => r.Name.Contains(search) || r.Code.StartsWith(search));
            catQuery = catQuery.OrderBy(r => r.SequenceNo);
            if (page != -1)
                catQuery = catQuery.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                catQuery = catQuery.Take(pageSize);
            var category = await catQuery.ToListAsync();
            return category;

        }

        public async Task<List<Category>> GetallCategories()
        {

            IQueryable<Category> catQuery = _db.Category;

            var category = await catQuery.ToListAsync();
            return category;

        }

        public async Task<int> count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await _db.Category.Where(r => r.Name.Contains(search) || r.Code.StartsWith(search)).CountAsync();
            return await _db.Category.CountAsync();
        }
        public async Task<bool> IsDependacyExist(int categoryId)
        {
            int Count = await (from course in _db.Course
                               join cat in _db.Category on course.CategoryId equals cat.Id
                               where (course.IsDeleted == false && cat.Id == categoryId)
                               select new { cat.Id }).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }
        public async Task<int> GetSequenceNo()
        {
            int? SequenceNumber = await _db.Category.OrderByDescending(c => c.SequenceNo).Select(c => c.SequenceNo).FirstOrDefaultAsync();
            if (SequenceNumber == null)
                return 1;
            SequenceNumber = SequenceNumber + 1;
            return (int)SequenceNumber;
        }
        public async Task<Message> UpdateCategories(List<APICategoryDTO> apiCategories)
        {

            int ExistingCategoryCount = await this._db.Category.CountAsync();
            int UpdateCategoryCount = apiCategories.Count();
            if (ExistingCategoryCount != UpdateCategoryCount || UpdateCategoryCount == 0)
                return Message.InvalidModel;

            List<Category> Categories = new List<Category>();
            int SequenceNo = 0;
            foreach (APICategoryDTO apiCategory in apiCategories)
            {
                Category Category = await this.Get(apiCategory.Id);
                if (Category == null)
                    return Message.InvalidModel;
                Category.CreatedDate = DateTime.UtcNow;
                Category.ModifiedDate = DateTime.UtcNow;
                SequenceNo++;
                Category.SequenceNo = SequenceNo;
                Categories.Add(Category);
            }
            await this.UpdateRange(Categories.ToArray());
            return Message.Ok;
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesBySequenceNo()
        {
            var Categories = await this.GetAll();
            Categories = Categories.OrderBy(category => category.SequenceNo);
            IEnumerable<Category> ApiCategory = new List<Category>();
            ApiCategory = Mapper.Map<List<Category>>(Categories);
            return ApiCategory;
        }


        public async Task<IEnumerable<SubCategory>> GetAllApplicableSubCategories(int UserId, int categoryId)
        {

            IEnumerable<SubCategory> ApiCategory = new List<SubCategory>();

            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@CategoryId", categoryId);

                    var Result = await SqlMapper.QueryAsync<SubCategory>((SqlConnection)connection, "[dbo].[GetApplicableSubCategories]", parameters, null, null, CommandType.StoredProcedure);
                    ApiCategory = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return ApiCategory;
        }

        public async Task<IEnumerable<Category>> GetAllApplicableCategories(int UserId)
        {
           
            IEnumerable<Category> ApiCategory = new List<Category>();
           
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", UserId);

                    var Result = await SqlMapper.QueryAsync<Category>((SqlConnection)connection, "[dbo].[GetApplicableCategories]", parameters, null, null, CommandType.StoredProcedure);
                    ApiCategory = Result.ToList();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return ApiCategory;
        }
        public async Task<List<Category>> GetCategoryTypeAhead(string search = null)
        {

            if (search == "null")
                search = null;

            var Query = (from category in this._db.Category
                             // join course in this._db.Course on category.Id equals course.CategoryId
                         where (category.Name.Contains(search) || search == null)
                         select new Category
                         {
                             Code = category.Code,
                             CreatedDate = category.CreatedDate,
                             Id = category.Id,
                             ImagePath = category.ImagePath,
                             ModifiedDate = category.ModifiedDate,
                             Name = category.Name,
                             SequenceNo = category.SequenceNo
                         });


            Query = Query.OrderByDescending(r => r.Id);
            return await Query.ToListAsync();
        }

        public async Task<List<SubCategory>> GetSubCategoryTypeAhead(int CategoryId, string search = null)
        {

            if (search == "null")
                search = null;

            var Query = (from subcategory in this._db.SubCategory
                             // join course in this._db.Course on category.Id equals course.CategoryId
                         where (subcategory.Name.Contains(search) || search == null)
                         && subcategory.CategoryId == CategoryId
                         select new SubCategory
                         {
                             Code = subcategory.Code,
                             CreatedDate = subcategory.CreatedDate,
                             Id = subcategory.Id,
                             ModifiedDate = subcategory.ModifiedDate,
                             Name = subcategory.Name,
                             SequenceNo = subcategory.SequenceNo
                         });


            Query = Query.OrderByDescending(r => r.Id);
            return await Query.ToListAsync();
        }

        public async Task<dynamic> GetCategoies(int id)
        {
            using (var connection = this._db.Database.GetDbConnection())
            {
                connection.Open();

                var result = await connection.QueryAsync<dynamic>(@"SELECT ct.Id,ct.Name,c.Id As CourseId,c.Title FROM Course.Category ct join course.Course c on ct.Id=c.CategoryId where ct.Id=@id or @id is null", new { id });


                if (result.AsList().Count == 0)
                    return null;

                return MapCategories(result);

            }

        }

        private APICategoriesCourses MapCategories(dynamic result)
        {
            var category = new APICategoriesCourses
            {
                Id = result[0].Id,
                Name = result[0].Name,
                Courses = new List<APICourseByCategory>()
            };
            
            foreach (dynamic item in result)
            {
              
                var courseList = new APICourseByCategory
                {
                    CourseId = item.CourseId,
                    Title = item.Title
                   
                };
               
                category.Courses.Add(courseList);
                
            }
            category.CoursesCount = category.Courses.Count();
            return category;
        }

        public async Task<dynamic> GetTnaCategories()
        {
            using (var connection = this._db.Database.GetDbConnection())
            {
                connection.Open();

                dynamic result = await connection.QueryAsync<dynamic>(@"SELECT CategoryType FROM Course.TnaEmployeeData GROUP BY CategoryType");

                if (result.Count == 0)
                    return null;

                var categories = new List<TnaCategories>();
                
                foreach (dynamic item in result)
                {
                    var category = new TnaCategories
                    {
                        CategoryType = item.CategoryType
                    };

                    categories.Add(category);

                }
                return categories;
            }
        }
    }
}
