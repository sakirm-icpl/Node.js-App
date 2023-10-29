// ======================================
// <copyright file="CompetenciesMappingRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using log4net;

using Microsoft.Extensions.Configuration;

using System.Text;

using Courses.API.Model;
using Courses.API.Model.EdCastAPI;

namespace Courses.API.Repositories.Competency
{
    public class CourseGroupMappingRepository : Repository<CourseGroupMapping>, ICourseGroupMappingRepository
    {
        StringBuilder sb = new StringBuilder();


        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMappingRepository));
        private CourseContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IConfiguration _configuration;


        public CourseGroupMappingRepository(CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
        }
        public async Task<IEnumerable<APICourseGroupMappings>> GetAllCourseGroupMapping(int page, int pageSize, string search = null)
        {
            try
            {
                var result = (from courseGroupMapping in this.db.CourseGroupMapping
                              join course in this.db.Course on courseGroupMapping.CourseId equals course.Id
                              where courseGroupMapping.IsDeleted == Record.NotDeleted
                              select new APICourseGroupMappings
                              {
                                  CourseId = course.Id,
                                  Course = course.Title,
                                  CourseCode = course.Code,
                                  Status = course.IsActive,
                                  GroupCount = this.db.CourseGroupMapping.Where(x => x.CourseId == courseGroupMapping.CourseId && x.IsDeleted == false).Select(x => x.Id).Count(),
                              }).Distinct();
                //if (!string.IsNullOrEmpty(search))
                //{
                //    result = result.Where(a => ((Convert.ToString(a.CourseCategory).StartsWith(search) || Convert.ToString(a.Course).StartsWith(search) || Convert.ToString(a.Competency).StartsWith(search) || Convert.ToString(a.CompetencyLevel).StartsWith(search))));
                //}
                result = result.OrderByDescending(c => c.CourseId);
                if (page != -1)
                    result = result.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    result = result.Take(pageSize);

                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            using (var context = this.db)
            {
                var result = (from courseGroupMapping in this.db.CourseGroupMapping

                              where courseGroupMapping.IsDeleted == Record.NotDeleted
                              select new APICourseGroupMappings
                              {
                                  CourseId = courseGroupMapping.CourseId
                              }).Distinct();

                //if (!string.IsNullOrEmpty(search))
                //{
                //    result = result.Where(a => ((Convert.ToString(a.CourseCategory).StartsWith(search) || Convert.ToString(a.Course).StartsWith(search) || Convert.ToString(a.Competency).StartsWith(search) || Convert.ToString(a.CompetencyLevel).StartsWith(search))));
                //}

                return await result.CountAsync();
            }
        }

        public async Task<int> GetCountGroupByCourseId(int courseid)
        {
            using (var context = this.db)
            {
                
                    var result = (from courseGroupMapping in context.CourseGroupMapping
                                  join CourseGroup in this.db.CourseGroup on courseGroupMapping.GroupId equals CourseGroup.Id

                                  where courseGroupMapping.IsDeleted == Record.NotDeleted && courseGroupMapping.CourseId == courseid
                                  select new APICourseGroupMappings
                                  {
                                      Id = courseGroupMapping.Id,
                                      GroupId = courseGroupMapping.GroupId,
                                      GroupName = CourseGroup.GroupName,
                                      GroupCode = CourseGroup.GroupCode
                                  }).Distinct(); 

                  
                    return await result.CountAsync();
            }
        }
        public async Task<IEnumerable<APICourseGroupMappings>> GetAllGroupsMappingByCourse(int courseid, int page, int pageSize)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from courseGroupMapping in context.CourseGroupMapping
                                  join CourseGroup in this.db.CourseGroup on courseGroupMapping.GroupId equals CourseGroup.Id
                                 
                                  where courseGroupMapping.IsDeleted == Record.NotDeleted && courseGroupMapping.CourseId == courseid
                                  select new APICourseGroupMappings
                                  {
                                      Id = courseGroupMapping.Id,
                                      GroupId = courseGroupMapping.GroupId,                                      
                                      GroupName = CourseGroup.GroupName,
                                      GroupCode= CourseGroup.GroupCode
                                  }); 

                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);

                    if (pageSize != -1)
                        result = result.Take(pageSize);

                    return await result.ToListAsync();
                 
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int[]> getGroupIdByCourseId(int CourseId)
        {

            int[] IdS = (from c in this.db.CourseGroupMapping
                         where c.IsDeleted == false && c.CourseId == CourseId
                         select Convert.ToInt32(c.GroupId)).ToArray();
            IdS.LastOrDefault();
            return IdS;
        }
        public async Task<List<APICourseGroup>> GetCourseGroupTypeAhead(string search = null)
        {

            if (search == "null")
                search = null;

            var Query = (from courseGroup in this.db.CourseGroup

                         where courseGroup.IsDeleted==false && (courseGroup.GroupName.Contains(search) || search == null)
                         select new APICourseGroup
                         {
                             Id = courseGroup.Id,
                             GroupCode = courseGroup.GroupCode,
                             GroupName = courseGroup.GroupName,
                             CourseCount = courseGroup.NumberOfCourse,
                             Status = courseGroup.Status
                         });


            Query = Query.OrderByDescending(r => r.Id);
            return await Query.ToListAsync();
        }

        public async Task<int> GroupMappingDelete(int id)
        {
            CourseGroupMapping courseGroupMapping = await this.Get(id);
            if (courseGroupMapping != null)
            {
                courseGroupMapping.IsDeleted = true;
                await this.Update(courseGroupMapping);
                return 1;
            }
            return 0;
        }
        public async Task<CourseGroupMapping> Exists(int courseId, int GroupId)
        {
            CourseGroupMapping courseGroupMapping = await this.db.CourseGroupMapping.Where(p => ((p.CourseId == courseId) && (p.GroupId == GroupId))).FirstOrDefaultAsync();

            
            return courseGroupMapping;
        }

        public void UpdateCourseGroupCourseCount(int groupid)
        {
            CourseGroup courseGroup = this.db.CourseGroup.Where(a => a.Id == groupid).FirstOrDefault();
            if (courseGroup != null)
            {
                int count = GetCourseCountForMapping(groupid);
                courseGroup.NumberOfCourse = count;
                this.db.CourseGroup.Update(courseGroup);
                this.db.SaveChanges();
            }
        }
        private int GetCourseCountForMapping(int groupid)
        {
            List<int?> ints = this.db.CourseGroupMapping.Where(a => a.GroupId == groupid && a.IsDeleted == false).Select(a => a.Id).ToList();
            return ints.Count;
        }

        public async void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId)
        {
            var result = aPIOldCompetenciesId.Except(CurrentCompetencies);
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
                            dbContext.Database.ExecuteSqlCommand("Update Course.CourseGroupMapping set IsDeleted = 1 where GroupId = " + res + " and CourseId=" + CourseId);

                        }
                    }
                }

            }
            return;
        }

        public int SaveCourseGroup(CourseGroup courseGroup,int UserId)
        {
            CourseGroup CourseGroup1 = new CourseGroup();
            try
            {
                CourseGroup1 = db.CourseGroup.Where(a => a.GroupCode == courseGroup.GroupCode && a.IsDeleted == false).FirstOrDefault();

                if (CourseGroup1 != null)
                {
                    return -1;
                }

                CourseGroup1 = db.CourseGroup.Where(a => a.GroupName == courseGroup.GroupName && a.IsDeleted == false).FirstOrDefault();

                if (CourseGroup1 != null)
                {
                    return -2;
                }

                courseGroup.NumberOfCourse = 0;
                courseGroup.IsDeleted = false;
                courseGroup.ModifiedBy = UserId;
                courseGroup.CreatedBy = UserId;
                courseGroup.ModifiedDate = DateTime.Now;
                courseGroup.CreatedDate = DateTime.Now;

                db.CourseGroup.Add(courseGroup);
                db.SaveChanges();

                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return -3;
            }
            
        }
        public IEnumerable<APICourseGroup> GetAllCourseGroup(int page, int pageSize, string search = null)
        {
            try
            {
                var result = (from courseGroup in this.db.CourseGroup
                              where courseGroup.IsDeleted == Record.NotDeleted
                              select new APICourseGroup
                              {
                                  Id = courseGroup.Id,
                                  GroupCode = courseGroup.GroupCode,
                                  GroupName = courseGroup.GroupName,
                                  CourseCount = courseGroup.NumberOfCourse,
                                  Status = courseGroup.Status
                              });

                if(search != null)
                {
                    result = result.Where(a => a.GroupName.ToLower().StartsWith(search));
                }
                
                result = result.OrderByDescending(c => c.Id);
                if (page != -1)
                    result = result.Skip((page - 1) * pageSize);

                if (pageSize != -1)
                    result = result.Take(pageSize);

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public int GetAllCourseGroupCount(string search = null)
        {
            using (var context = this.db)
            {
                var result = (from courseGroup in this.db.CourseGroup

                              where courseGroup.IsDeleted == Record.NotDeleted
                              select new APICourseGroup
                              {
                                  GroupCode = courseGroup.GroupCode,
                                  GroupName = courseGroup.GroupName
                              }).Distinct();

                if (!string.IsNullOrEmpty(search))
                {
                    result = result.Where(a => a.GroupName.ToLower().StartsWith(search));
                }

                return result.Count();
            }
        }
        public int DeleteCourseGroup(string CourseGroupCode)
        {
            CourseGroup CourseGroup1 = db.CourseGroup.Where(a => a.GroupCode.ToLower() == CourseGroupCode.ToLower() && a.IsDeleted == false).FirstOrDefault();

            if (CourseGroup1 == null)
            {
                return -1;
            }

            CourseGroup1.IsDeleted = true;

            db.CourseGroup.Update(CourseGroup1);
            db.SaveChanges();

            return 0;
        }

        public int UpdateCourseGroup(CourseGroup courseGroup, int UserId)
        {
            CourseGroup courseGroup1 = new CourseGroup();
            try
            {
                courseGroup1 = db.CourseGroup.Where(a => a.Id == courseGroup.Id && a.IsDeleted == false).FirstOrDefault();

                if (courseGroup1 == null)
                {
                    return -1;
                }

                CourseGroup courseGroup2 = db.CourseGroup.Where(a => a.Id != courseGroup.Id && a.GroupCode == courseGroup.GroupCode).FirstOrDefault();

                if(courseGroup2 != null)
                {
                    return -2;
                }

                courseGroup1.ModifiedBy = UserId;
                courseGroup1.ModifiedDate = DateTime.Now;
                courseGroup1.GroupName = courseGroup.GroupName;
                courseGroup1.Status = courseGroup.Status;

                db.CourseGroup.Update(courseGroup);
                db.SaveChanges();

                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return -3;
            }

        }
    }
}
