using MyCourse.API.APIModel;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories
{
    public class ScormVarResultRepository : Repository<ScormVarResult>, IScormVarResultRepository
    {
        private CourseContext _db;
        public ScormVarResultRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        //public async Task<int> Count(string search = null)
        //{
        //    if (!string.IsNullOrWhiteSpace(search))
        //        return await _db.ScormVarResult.CountAsync();
        //    return await _db.ScormVarResult.CountAsync();
        //}

        public async Task<ScormVarResult> Get(int userId, int courseId, int moduleId)
        {
            return await _db.ScormVarResult.Where(r => r.UserId.Equals(userId) && r.CourseId == courseId && r.ModuleId == moduleId).OrderByDescending(r => r.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        //public async Task<APIScormCompletionDetails> GetScormDetails(int userId, int courseId, int moduleId)
        //{
        //    APIScormCompletionDetails scorm = await (from sr in _db.ScormVarResult
        //                                          where sr.CourseId == courseId && sr.UserId == userId && sr.ModuleId == moduleId 
        //                                          //&& s.VarName.Equals("cmi.core.session_time")
        //                                             orderby sr.Id descending
        //                                             select new APIScormCompletionDetails
        //                                             {
        //                                                 CompletionStatus = sr.Result == "passed" || sr.Result == "Completed" ? "Completed" : "Incompleted",
        //                                                 Result = sr.Result,
        //                                                 Score = sr.Score,
        //                                                 NoOfAttempts = sr.NoOfAttempts,
        //                                                 SessionTime = null 
        //                                             }).FirstOrDefaultAsync();
        //    return scorm;
        //}



        public async Task<int> Count(int userId, int courseId, int moduleId)
        {
            int Count = await _db.ScormVarResult.Where(r => r.UserId.Equals(userId) && r.CourseId == courseId && r.ModuleId == moduleId).CountAsync();
            return Count;
        }
        //public async Task<int> GetNoAttempt(int userId, int courseId, int moduleId)
        //{
        //    return await _db.ScormVarResult.AsNoTracking().Where(r => r.UserId == userId && r.CourseId == courseId && r.ModuleId == moduleId).OrderByDescending(s => s.Id).Select(s => s.NoOfAttempts).FirstOrDefaultAsync();
        //}

        public async Task<string> GetScore(int userId, int courseId, int moduleId)
        {
            return await _db.ScormVarResult.Where(r => r.UserId.Equals(userId) && r.CourseId == courseId && r.ModuleId == moduleId && r.Result == null).OrderByDescending(s => s.Id).Select(s => s.Score.ToString()).FirstOrDefaultAsync();
        }
        public async Task<string> GetResult(int userId, int courseId, int moduleId)
        {
            return await _db.ScormVarResult.Where(r => r.UserId.Equals(userId) && r.CourseId == courseId && r.ModuleId == moduleId && r.Score == null && (r.Result == "passed" || r.Result == "failed" || r.Result == "pass" || r.Result == "fail")).OrderByDescending(s => s.Id).Select(s => s.Result.ToString()).FirstOrDefaultAsync();
        }

        //public async Task<APIAssessmentCompletionDetails> GetAssessmentDetails(int userId, int courseId, int moduleId)
        //{
        //    APIAssessmentCompletionDetails APIAssessmentResult = await _db.PostAssessmentResult.Where(PostAssessment =>
        //                                                              PostAssessment.CourseID == courseId
        //                                                              && PostAssessment.CreatedBy == userId
        //                                                              && PostAssessment.ModuleId == moduleId)
        //                                                                .OrderByDescending(p => p.Id)
        //                                                                .Select(PostAssessment => new APIAssessmentCompletionDetails
        //                                                                {
        //                                                                    CompletionStatus = PostAssessment.PostAssessmentStatus,
        //                                                                    Result = PostAssessment.AssessmentResult,
        //                                                                    Score = (int?)PostAssessment.PassingPercentage,
        //                                                                    NoOfAttempts = PostAssessment.NoOfAttempts
        //                                                                }).FirstOrDefaultAsync();
        //    return APIAssessmentResult;
        //}
        public async Task<bool> IsContentCompleted(int UserId, int CourseId, int ModuleId)
        {
            int result = await _db.ScormVars.Where(svr => svr.UserId == UserId && svr.CourseId == CourseId && svr.ModuleId == ModuleId 
                                                && (svr.VarName == "cmi.core.lesson_status" || svr.VarName == "cmi.completion_status") 
                                                && (svr.VarValue == "completed" || svr.VarValue == "complete" || svr.VarValue == "passed" || svr.VarValue == "pass")).CountAsync();
            if (result>0)
                return true;
            else
                return false;
        }
    }
}
