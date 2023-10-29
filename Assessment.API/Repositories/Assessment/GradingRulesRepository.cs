using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using log4net;
using Assessment.API.Helper;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories
{
    public class GradingRulesRepository : Repository<GradingRules>, IGradingRules
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GradingRulesRepository));
        private AssessmentContext _db;
        private string url;
        private IConfiguration _configuration;
        private ICourseRepository _courseRepository;
        public GradingRulesRepository(AssessmentContext context, IConfiguration configuration, ICourseRepository courseRepository) : base(context)
        {
            this._courseRepository = courseRepository;
            configuration = _configuration;
            this._db = context;
        }
        public async Task<IEnumerable<APIGradingRules>> GetAllGradingRules(int page, int pageSize, string search = null)
        {
            try
            {

                IQueryable<APIGradingRules> Query = from user in this._db.GradingRules

                                                    select new APIGradingRules
                                                    {
                                                        CourseId = user.CourseId,
                                                        ModelId = user.ModelId,
                                                        Grade = user.Grade,
                                                        GradingRuleID = user.GradingRuleID,
                                                        Id = user.Id,
                                                        ScorePercentage = user.ScorePercentage
                                                    };

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.Grade.StartsWith(search) && v.IsDeleted == Record.NotDeleted);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                var Result = await Query.ToListAsync();
                foreach (APIGradingRules grades in Result)
                {
                    grades.CourseName = await _courseRepository.GetCourseNam(grades.CourseId);
                    grades.ModuleName = await _courseRepository.GetModuleName(grades.ModelId);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.GradingRules.Where(r => (r.Grade.StartsWith(search)) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.GradingRules.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<int> GetTotalGradingRulesCount()
        {
            return await this._db.GradingRules.CountAsync();
        }

        public async Task<bool> Exist(string grade, int CourseId, int ModelId)
        {

            var count = await this._db.GradingRules.Where(r => string.Equals(r.Grade, grade, StringComparison.CurrentCultureIgnoreCase) && r.CourseId == CourseId && r.ModelId == ModelId).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<IEnumerable<GradingRules>> Search(string query)
        {
            var gradeRulesList = (from gradeRules in this._db.GradingRules
                                  where
                                  (
                                    gradeRules.GradingRuleID.StartsWith(query) ||
                                    gradeRules.Grade.StartsWith(query)
                                  )
                                    && gradeRules.IsDeleted == false
                                  select gradeRules).ToListAsync();
            return await gradeRulesList;
        }

        private async Task<HttpResponseMessage> CallAPI(string url)
        {
            using (var client = new HttpClient())
            {

                string apiUrl = this.url;
                var response = await client.GetAsync(url);
                return response;
            }
        }

    }
}