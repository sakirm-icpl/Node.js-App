using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class CourseAuthorAssociationRepository : Repository<CourseAuthorAssociation>, ICourseAuthorAssociation
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseAuthorAssociationRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;       
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
      

        public CourseAuthorAssociationRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;            
            _customerConnectionRepository = customerConnectionRepository;
           
        }

       
        public async Task<CourseAuthorAssociation> RecordExists(int courseId, int userId)
        {
            try
            {
                CourseAuthorAssociation courseauthor = await this._db.CourseAuthorAssociation.Where(p => p.CourseId == courseId && p.UserId == userId ).FirstOrDefaultAsync();

                return courseauthor;
            }
            catch (Exception ex)
            { }
            return null;
        }

        public async Task<List<APICourseAuthor>> GetAuthorsByCourseId(int CourseId)
        {

            List<APICourseAuthor> resultAuthors = (from courseauthors in this._db.CourseAuthorAssociation
                                                      join user in this._db.UserMaster on courseauthors.UserId equals user.Id
                                                      where user.IsDeleted == false && courseauthors.IsDeleted == 0 && courseauthors.CourseId == CourseId
                                                      select new APICourseAuthor
                                                      {
                                                          Id =Security.EncryptForUI( user.Id.ToString()),
                                                          Name = user.UserName 

                                                      }).ToList();

            return resultAuthors;
        }
        public async Task<int[]> getAuthorsCourseId(int CourseId)
        {

            int[] IdS = (from c in this._db.CourseAuthorAssociation                           
                         where c.IsDeleted == 0 && c.CourseId == CourseId
                         select Convert.ToInt32(c.UserId)).ToArray();
            IdS.LastOrDefault();
            return IdS;
        }

        public async void FindElementsNotInArray(int[] CurrentAuthors, int[] oldAuthors, int CourseId)
        {
            var result = oldAuthors.Except(CurrentAuthors);
            foreach (var res in result)
            {
                using (var dbContext = this._customerConnectionRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            dbContext.Database.ExecuteSqlCommand("Update Course.CourseAuthorAssociation set IsDeleted = 1 where UserId = " + res + " and CourseId=" + CourseId);

                        }
                    }
                }

            }
            return;
        }

    }

}
