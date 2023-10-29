using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using log4net;
using Feedback.API.Models;
using Courses.API.Repositories.Interfaces;
using Feedback.API.Common;
using Feedback.API.APIModel;

namespace Feedback.API.Repositories
{
    public class FeedbackStatusRepository : Repository<FeedbackStatus>, IFeedbackStatus
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedbackStatusRepository));
        private FeedbackContext _db;
        IModuleCompletionStatusRepository _moduleCompletionStatusRepository;
        ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        private IMyCoursesRepository _myCoursesRepository;
        public FeedbackStatusRepository(FeedbackContext context,
            IModuleCompletionStatusRepository moduleCompletionStatusRepository,
            ICourseCompletionStatusRepository courseCompletionStatusRepository
            , IMyCoursesRepository myCoursesRepository) : base(context)
        {
            _db = context;
            _moduleCompletionStatusRepository = moduleCompletionStatusRepository;
            _courseCompletionStatusRepository = courseCompletionStatusRepository;
            this._myCoursesRepository = myCoursesRepository;
        }
        public bool Exists(int courseId, int moduleId, int userId, int? dpId = null, bool IsOJT = false)
        {
            if (_db.FeedbackStatus.Count(f => f.CourseId == courseId && f.ModuleId == moduleId && f.DPId == dpId && f.CreatedBy == userId && f.IsOJT == IsOJT && f.IsDeleted == false) > 0)
                return true;
            return false;
        }

        public async Task<FeedbackStatus> FeedbackStatusExists(int courseId, int moduleId, int userId)
        {
            FeedbackStatus fs = await _db.FeedbackStatus.Where(f => f.CourseId == courseId && f.ModuleId == moduleId && f.CreatedBy == userId && f.IsDeleted == false).FirstOrDefaultAsync();

            return fs;
        }
        public async Task<int> Count(string? search = null, string? filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await _db.FeedbackStatus.Where(f => f.IsDeleted == false).CountAsync();
            return await _db.FeedbackStatus.Where(f => f.IsDeleted == false).CountAsync();
        }

        public async Task<List<FeedbackStatus>> Get(int page, int pageSize, string? search = null, string? filter = null)
        {
            IQueryable<FeedbackStatus> Query = _db.FeedbackStatus.Where(c => c.IsDeleted == false);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(f => f.IsDeleted == false);
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.ToListAsync();
        }
        public async Task<int> AddModuleCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null)
        {
            ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
            ModuleStatus.CourseId = CourseId;
            ModuleStatus.ModuleId = ModuleId;
            ModuleStatus.UserId = UserId;
            ModuleStatus.Status = Status.Completed;
            ModuleStatus.CreatedDate = DateTime.UtcNow;
            ModuleStatus.ModifiedDate = DateTime.UtcNow;
            await _moduleCompletionStatusRepository.Post(ModuleStatus, null, null, OrgCode);
            return 1;
        }

        public async Task<int> AddModuleFeedbackCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null)
        {
            ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
            ModuleStatus.CourseId = CourseId;
            ModuleStatus.ModuleId = ModuleId;
            ModuleStatus.UserId = UserId;
            ModuleStatus.Status = Status.InProgress;
            ModuleStatus.CreatedDate = DateTime.UtcNow;
            ModuleStatus.ModifiedDate = DateTime.UtcNow;
            await _moduleCompletionStatusRepository.PostFeedbackCompletion(ModuleStatus, null, null, OrgCode);
            return 1;
        }

        public async Task<int> AddCourseCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null)
        {
            CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
            courseCompletionStatus.CourseId = CourseId;
            courseCompletionStatus.UserId = UserId;
            await this._courseCompletionStatusRepository.Post(courseCompletionStatus, OrgCode);
            return 1;
        }

        public async Task<bool> IsFeedbackSubmitted(int courseId, int moduleId, int userId, bool IsOJT = false)
        {
            if (await _db.FeedbackStatus.Where(f => f.IsDeleted == false && f.CourseId == courseId && f.ModuleId == moduleId && f.CreatedBy == userId && f.IsOJT == IsOJT && f.Status == "completed").CountAsync() > 0)

                return true;
            return false;
        }

        public async Task<bool> IsIdpFeedbackSubmited(int DpId, int userId)
        {
            if (await _db.FeedbackStatus.Where(f => f.IsDeleted == false && f.DPId == DpId && f.CreatedBy == userId && f.Status == "completed").CountAsync() > 0)
                return true;
            return false;
        }

        public async Task<bool> IsContentCompletedforFeeback(int userId, int courseId, int? moduleId, bool IsOJT = false)
        {
            try
            {
                if (moduleId == 0 && courseId != 0 && IsOJT == false)
                {
                    APIMyCoursesModule CourseInfo = await this._myCoursesRepository.GetModule(userId, courseId);

                    if (CourseInfo.ContentStatus.ToLower() != "completed" && (CourseInfo.IsAssessment = true && CourseInfo.AssessmentStatus.ToLower() != "completed"))
                        return false;
                }
                else if (moduleId == 0 && courseId != 0 && IsOJT == true)
                {
                    APIMyCoursesModule CourseInfo = await this._myCoursesRepository.GetModule(userId, courseId);

                    if (CourseInfo.ContentStatus.ToLower() != "completed" && (CourseInfo.IsAssessment = true && CourseInfo.AssessmentStatus.ToLower() != "completed") && (CourseInfo.IsFeedback = true && CourseInfo.FeedbackStatus.ToLower() != "completed"))
                        return false;
                }

                else if (moduleId != 0 && courseId != 0)
                {
                    ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(userId, courseId, moduleId);

                    if (CourseInfo.Modules.ContentStatus.ToLower() != "completed" && (CourseInfo.Modules.IsAssessment = true && (CourseInfo.Modules.AssessmentStatus == null ? "" : CourseInfo.Modules.AssessmentStatus.ToLower()) != "completed"))
                        return false;

                }
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return false;
            }
        }

        public async Task<bool> IsDevPlanCompletedforFeeback(int userId, int? dpId)
        {
            try
            {
                DevelopmentPlanForCourse olddevelopmentPlanForCourse = await _db.DevelopmentPlanForCourse.Where(a => a.Id == dpId && a.IsDeleted == false).FirstOrDefaultAsync();

                if (olddevelopmentPlanForCourse != null)
                {
                    var completedcourses = (from course in _db.CourseMappingToDevelopment
                                            join ccs in _db.CourseCompletionStatus on course.CourseId equals ccs.CourseId
                                            where course.DevelopmentPlanId == olddevelopmentPlanForCourse.Id && ccs.UserId == userId && ccs.Status == "completed"
                                            select new
                                            {
                                                ccs.Id
                                            }).AsNoTracking();
                    int completedcoursecount = await completedcourses.CountAsync();
                    if (olddevelopmentPlanForCourse.CountOfMappedCourses == completedcoursecount)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return false;
            }
        }

    }
}
