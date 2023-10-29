using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Courses.API.Helper;
using log4net;

namespace Courses.API.Repositories
{
    public class LcmsQuestionAssociationRepository : Repository<LcmsQuestionAssociation>, ILcmsQuestionAssociation
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LcmsQuestionAssociationRepository));
        private CourseContext _db;
        public LcmsQuestionAssociationRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        public async Task<ApiAssesment> GetAssesment(int id)
        {
            return await (from lcms in _db.LCMS
                          where lcms.Id == id
                          select new ApiAssesment
                          {
                              Description = lcms.Description,
                              Id = lcms.Id,
                              MetaData = lcms.MetaData,
                              Name = lcms.Name

                          }).FirstOrDefaultAsync();

        }
        public async Task<int> DeleteQuestions(int lcmsId)
        {
            try
            {
                await _db.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }
        public async Task<int> Delete(int lcmsId)
        {
            try
            {
                var Lcms = (
                from lcms in _db.LCMS
                where lcms.Id == lcmsId
                select lcms);

                foreach (var lc in Lcms)
                {
                    lc.IsDeleted = true;
                    _db.LCMS.Update(lc);
                }

                await _db.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }

    }
}
