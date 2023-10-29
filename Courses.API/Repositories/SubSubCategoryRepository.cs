using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
namespace Courses.API.Repositories
{
    public class SubSubCategoryRepository : Repository<SubSubCategory>, ISubSubCategoryRepository
    {
        private CourseContext _db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        public SubSubCategoryRepository(CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
        {
            _db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
        }
        public async Task<List<APICourseSubSubCategory>> Get(int page, int pageSize, string search = null, int? categoryId = null, int? subcategoryId = null)
        {

            var Query = (from c in this._db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id 
                         join ssc in _db.SubSubCategory on c.Id equals ssc.SubCategoryId

                         select new APICourseSubSubCategory
                         {
                             Id = ssc.Id,
                             Code = ssc.Code,
                             CategoryName = cat.Name,
                             SubCategoryName = c.Name,
                             SubCategoryId = ssc.SubCategoryId,
                             SequenceNo=ssc.SequenceNo,
                             Name = ssc.Name,
                             CategoryId = c.CategoryId

                         });

            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
            {
                Query = Query.Where(r => r.Code.Contains(search) || r.Name.Contains(search) || r.CategoryName.Contains(search) ||r.SubCategoryName.Contains(search));
            }
            if (categoryId != null)
                Query = Query.Where(r => r.CategoryId == categoryId);
            if (subcategoryId != null)
                Query = Query.Where(r => r.SubCategoryId == subcategoryId);
            Query = Query.OrderBy(r => r.CategoryName);
            Query = Query.OrderBy(r => r.SubCategoryName);
            Query = Query.OrderBy(r => r.SequenceNo);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.Distinct().ToListAsync();

        }

        public async Task<List<APICourseSubSubCategory>> GetSubSubCategoty(int catgoryid)
        {
            var Query = (from c in _db.SubSubCategory
                         join cat in this._db.Category on c.SubCategoryId equals cat.Id
                         where c.Id == catgoryid
                         select new APICourseSubSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             SubCategoryId = c.SubCategoryId
                         });

            return await Query.Distinct().ToListAsync();

        }
        public async Task<List<APICourseSubSubCategory>> GetSubCategory(int subcategoryId) 
        {
            var Query = (from c in this._db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id
                         join ssc in _db.SubSubCategory on c.Id equals ssc.SubCategoryId
                         where (ssc.SubCategoryId == subcategoryId )
           select new APICourseSubSubCategory
           {
                             Id = ssc.Id,
                             Code = ssc.Code,
                             CategoryName = cat.Name,
                             SubCategoryName = c.Name,
                             Name = ssc.Name,
                             SubCategoryId = ssc.SubCategoryId,
                             CategoryId = ssc.CategoryId,
                             SequenceNo = c.SequenceNo
                         })
                         .Distinct()
                         .OrderBy(c => c.SequenceNo);

            return await Query.ToListAsync();

        }

        public async Task<List<APICourseSubSubCategory>> GetAllSubSubcategories(int? categoryId=null)
        {
            var Query = (from c in _db.SubSubCategory
                         join cat in this._db.Category on c.SubCategoryId equals cat.Id
                         where (c.SubCategoryId == categoryId || categoryId==null )
                         orderby c.SequenceNo
                         select new APICourseSubSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             SubCategoryId = c.SubCategoryId,
                             SequenceNo = c.SequenceNo
                         })
                         .Distinct()
                         .OrderBy(c => c.SequenceNo);

            return await Query.ToListAsync();

        }

        public async Task<int> count(string search = null, int? categoryId = null, int? subcategoryId = null)
        {

            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id
                         join ssc in _db.SubSubCategory on c.Id equals ssc.SubCategoryId

                         select new APICourseSubSubCategory
                         {
                              Id = ssc.Id,
                             Code = ssc.Code,
                             CategoryName = cat.Name,
                             SubCategoryName = c.Name,
                             SubCategoryId = ssc.SubCategoryId,
                             SequenceNo=ssc.SequenceNo,
                             Name = ssc.Name,
                             CategoryId = c.CategoryId
                         });
            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
            {
                Query = Query.Where(r => r.Code.StartsWith(search) || r.Name.Contains(search) || r.CategoryName.Contains(search) || r.SubCategoryName.Contains(search));
            }

            if (subcategoryId != null)
                Query = Query.Where(r => r.SubCategoryId == subcategoryId);

            return await Query.Select(C => C.Id).CountAsync();
        }

        public async Task<bool> Exists(int? id, int subcategoryid, string code, string name)
        {
            if (id == null)
            {
                if (await _db.SubSubCategory.CountAsync(y => (y.Code == code && y.SubCategoryId == subcategoryid)) > 0)
                    return true;
                else if (await _db.SubSubCategory.CountAsync(y => (y.Name == name && y.SubCategoryId == subcategoryid)) > 0)
                    return true;
            }
            else
            {
                if (await _db.SubSubCategory.Where(y => y.Id != id).CountAsync(y => (y.Code == code && y.SubCategoryId == subcategoryid)) > 0)
                    return true;
                else if (await _db.SubSubCategory.Where(y => y.Id != id).CountAsync(y => y.Name == name && y.SubCategoryId == subcategoryid) > 0)
                    return true;
            }

            return false;
        }

        public async Task<bool> IsDependecyExists(int? id)
        {
            if (id != null)
            {
                if (await this._db.ExternalCourseCategoryAssociation.Where(s => s.IsDeleted == false && s.SubSubCategoryId == id).CountAsync() > 0)
                    return true;
            }
             
            return false;
        }

        public async Task<Message> UpdateSubSubCategories(List<APISubSubCategoryDTO> aPISubSubCategories)
        {
         
            List<SubSubCategory> SubSubCategories = new List<SubSubCategory>();
            int SequenceNo = 0;
            foreach (APISubSubCategoryDTO apisubsubcategories in aPISubSubCategories)
            {
                SubSubCategory SubSubCategory = await this.Get(apisubsubcategories.Id);
                if (SubSubCategory == null)
                    return Message.InvalidModel;
                SubSubCategory.CreatedDate = DateTime.UtcNow;

                SubSubCategory.ModifiedDate = DateTime.UtcNow;
                SequenceNo++;
                SubSubCategory.SequenceNo = SequenceNo;
                SubSubCategories.Add(SubSubCategory);
            }
            await this.UpdateRange(SubSubCategories.ToArray());
            return Message.Ok;
        }

        public async Task<int> GetSequenceNo()
        {
            int? SequenceNumber = await _db.SubSubCategory.OrderByDescending(c => c.SequenceNo).Select(c => c.SequenceNo).FirstOrDefaultAsync();
            if (SequenceNumber == null)
                return 1;
            SequenceNumber = SequenceNumber + 1;
            return (int)SequenceNumber;
        }

        public async void FindElementsNotInArray(int[] CurrentSubSubCategory, int[] aPIOldSubSubCategory, int CourseId)
        {
            var result = aPIOldSubSubCategory.Except(CurrentSubSubCategory);
            foreach (var res in result)
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            dbContext.Database.ExecuteSqlCommand("Update Course.ExternalCourseCategoryAssociation set IsDeleted = 1 where SubSubCategoryId = " + res + " and CourseId=" + CourseId);

                        }
                    }
                }

            }
            return;
        }
    }
}
