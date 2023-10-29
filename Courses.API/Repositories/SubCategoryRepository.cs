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

namespace Courses.API.Repositories
{
    public class SubCategoryRepository : Repository<SubCategory>, ISubCategoryRepository
    {
        private CourseContext _db;
        public SubCategoryRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        public async Task<List<APICourseSubCategory>> Get(int page, int pageSize, string search = null, int? categoryId = null)
        {

            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id

                         select new APICourseSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             CategoryId = c.CategoryId,
                             SequenceNo=c.SequenceNo
                         });

            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
            {
                Query = Query.Where(r => r.Code.Contains(search) || r.Name.Contains(search) || r.CategoryName.Contains(search));
            }
            if (categoryId != null)
                Query = Query.Where(r => r.CategoryId == categoryId);
            //Query = Query.OrderBy(r => r.CategoryName);
           
            
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            Query = Query.Distinct();
            Query = Query.OrderBy(r => r.SequenceNo);
            return await Query.ToListAsync();

        }

        public async Task<List<APICourseSubCategory>> GetSubCategoty(int catgoryid)
        {
            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id
                         where c.Id == catgoryid
                         select new APICourseSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             CategoryId = c.CategoryId
                         });

            return await Query.Distinct().ToListAsync();

        }
        public async Task<List<APICourseSubCategory>> GetCategory(int categoryId)
        {
            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id
                         where (c.CategoryId == categoryId )
           select new APICourseSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             CategoryId = c.CategoryId,
                             SequenceNo= c.SequenceNo
                         })
                         .Distinct()
                         .OrderBy(c => c.SequenceNo);

            return await Query.ToListAsync();

        }

        public async Task<List<APICourseSubCategory>> GetAllSubcategories(int? categoryId=null)
        {
            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id
                         where (c.CategoryId == categoryId || categoryId==null )
                         orderby c.SequenceNo
                         select new APICourseSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             CategoryId = c.CategoryId,
                             SequenceNo = c.SequenceNo
                         })
                         .Distinct()
                         .OrderBy(c => c.SequenceNo);

            return await Query.ToListAsync();

        }

        public async Task<int> count(string search = null, int? categoryId = null)
        {

            var Query = (from c in _db.SubCategory
                         join cat in this._db.Category on c.CategoryId equals cat.Id

                         select new APICourseSubCategory
                         {
                             Id = c.Id,
                             Code = c.Code,
                             CategoryName = cat.Name,
                             Name = c.Name,
                             CategoryId = c.CategoryId
                         });
            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
            {
                Query = Query.Where(r => r.Code.StartsWith(search) || r.Name.Contains(search) || r.CategoryName.Contains(search));
            }

            if (categoryId != null)
                Query = Query.Where(r => r.CategoryId == categoryId);

            return await Query.Select(C => C.Id).CountAsync();
        }

        public async Task<bool> Exists(int? id, int categoryid, string code, string name)
        {
            if (id == null)
            {
                if (await _db.SubCategory.CountAsync(y => (y.Code == code && y.CategoryId == categoryid)) > 0)
                    return true;
                else if (await _db.SubCategory.CountAsync(y => (y.Name == name && y.CategoryId == categoryid)) > 0)
                    return true;
            }
            else
            {
                if (await _db.SubCategory.Where(y => y.Id != id).CountAsync(y => (y.Code == code && y.CategoryId == categoryid)) > 0)
                    return true;
                else if (await _db.SubCategory.Where(y => y.Id != id).CountAsync(y => y.Name == name && y.CategoryId == categoryid) > 0)
                    return true;
            }

            return false;
        }

        public async Task<bool> IsDependecyExists(int? id)
        {
            if (id != null)
            {
                if (await this._db.Course.Where(s => s.IsDeleted == false && s.SubCategoryId == id).CountAsync() > 0)
                    return true;
            }

            return false;
        }

        public async Task<Message> UpdateSubCategories(List<APISubCategoryDTO> aPISubCategories)
        {
         
            List<SubCategory> SubCategories = new List<SubCategory>();
            int SequenceNo = 0;
            foreach (APISubCategoryDTO apisubcategories in aPISubCategories)
            {
                SubCategory SubCategory = await this.Get(apisubcategories.Id);
                if (SubCategory == null)
                    return Message.InvalidModel;
                SubCategory.CreatedDate = DateTime.UtcNow;

                SubCategory.ModifiedDate = DateTime.UtcNow;
                SequenceNo++;
                SubCategory.SequenceNo = SequenceNo;
                SubCategories.Add(SubCategory);
            }
            await this.UpdateRange(SubCategories.ToArray());
            return Message.Ok;
        }

        public async Task<int> GetSequenceNo()
        {
            int? SequenceNumber = await _db.SubCategory.OrderByDescending(c => c.SequenceNo).Select(c => c.SequenceNo).FirstOrDefaultAsync();
            if (SequenceNumber == null)
                return 1;
            SequenceNumber = SequenceNumber + 1;
            return (int)SequenceNumber;
        }
    }
}
