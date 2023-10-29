using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Models;
using Assessment.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assessment.API.Helper;
using log4net;

namespace Assessment.API.Repositories
{
    public class SubjectiveAssessmentStatusRepository : Repository<SubjectiveAssessmentStatus>, ISubjectiveAssessmentStatus
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SubjectiveAssessmentStatusRepository));
        private AssessmentContext _db;

        public SubjectiveAssessmentStatusRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<SubjectiveAssessmentStatus>> GetAssessmentSheetByUserId()
        {
            try
            {
                var SubjectiveAssessmentStatusList = (from subjectiveAssessmentStatus in this._db.SubjectiveAssessmentStatus
                                                      where
                                                      (subjectiveAssessmentStatus.Status == "UNCHECKED" ||
                                                     subjectiveAssessmentStatus.Status == "PENDING"
                                                      )
                                                      select subjectiveAssessmentStatus).ToListAsync();
                return await SubjectiveAssessmentStatusList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
    }
}
