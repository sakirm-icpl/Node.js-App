using CourseApplicability.API.APIModel;
using CourseApplicability.API.Model;
using CourseApplicability.API.Models;
using CourseApplicability.API.Repositories;
using CourseApplicability.API.Repositories.Interfaces;
using Courses.API.APIModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CourseApplicability.API.Repositories
{
    public class CoursesEnrollRequestRepository : Repository<CoursesEnrollRequest>, ICoursesEnrollRequestRepository
    {
        private CoursesApplicabilityContext _db;
        public ICoursesEnrollRequestRepository _coursesEnrollRequestRepository;
        public CoursesEnrollRequestRepository(CoursesApplicabilityContext context) : base(context)
        {
            _db = context;

        }
        public async Task<bool> IsExist(int userId, int courseId, string status)
        {
            var req = await _db.CoursesEnrollRequest.Where(r => r.UserId == userId && r.CourseId == courseId && r.Status == status).Select(a => a.Status).FirstOrDefaultAsync();

            if (req == null) return false;
            return true;

        }
        public async Task<APITotalRequest> GetSupervisorCourseRequests(GetSupervisorData getSupervisorData, int userId)
        {
            // UserMaster um = await _db.UserMaster.Where(a=>a.Id==userId).FirstOrDefaultAsync();
            if (userId == 0)
                return null;
            APITotalRequest aPITotalRequest = new APITotalRequest();
            IQueryable<APIUserRequestedCourses> Query = (from request in this._db.CoursesEnrollRequest
                                                             //  join umd in _db.UserMasterDetails on request.UserId equals umd.UserMasterId 

                                                         where request.IsDeleted == false && request.UserId == userId
                                                         select new APIUserRequestedCourses
                                                         {
                                                             Id = request.Id,
                                                             CourseId = request.CourseId,
                                                             CourseTitle = request.CourseTitle,
                                                             UserId = request.UserId,
                                                             UserName = request.UserName,
                                                             Status = request.Status,
                                                             Date = request.Date
                                                         }).AsNoTracking();



            aPITotalRequest.TotalRecords = await Query.CountAsync();

            Query = Query.OrderByDescending(v => v.Id);
            if (getSupervisorData.page != -1)
            {
                Query = Query.Skip((getSupervisorData.page - 1) * getSupervisorData.pageSize);
            }
            if (getSupervisorData.pageSize != -1)
            {
                Query = Query.Take(getSupervisorData.pageSize);
            }

            List<APIUserRequestedCourses> userrequestedcourse = await Query.ToListAsync();

            aPITotalRequest.data = userrequestedcourse;
            return aPITotalRequest;

        }

        public async Task<string> GetStatus(int userId, int courseId)
        {
            var req = await _db.CoursesEnrollRequest.Where(r => r.UserId == userId && r.CourseId == courseId).Select(a => a.Status).FirstOrDefaultAsync();

            return req;

        }
    }

    public class CoursesEnrollRequestDetailsRepository : Repository<CoursesEnrollRequestDetails>, ICoursesEnrollRequestDetailsRepository
    {
        private CoursesApplicabilityContext _db;
        public CoursesEnrollRequestDetailsRepository(CoursesApplicabilityContext context) : base(context)
        {
            _db = context;
        }
    }

}

