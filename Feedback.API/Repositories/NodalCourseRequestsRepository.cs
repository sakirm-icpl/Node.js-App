using Courses.API.Repositories.Interfaces;
using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Repositories;
using log4net;
using Feedback.API.APIModel;
using Feedback.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.API.Repositories
{
    public class NodalCourseRequestsRepository : Repository<NodalCourseRequests>, INodalCourseRequestsRepository
    {
        private FeedbackContext _db;
        private ICustomerConnectionStringRepository _customerConnection;
        private IEmail _email;
        
      
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NodalCourseRequestsRepository));
        public NodalCourseRequestsRepository(FeedbackContext context,
            ICustomerConnectionStringRepository customerConnection,
            IEmail email, 
           
            IConfiguration configuration) : base(context)
        {
            _db = context;
            _customerConnection = customerConnection;
            _email = email;
           
            
            _configuration = configuration;
        }

        public async Task<APIScormGroup> GetUserforCompletion(int GroupId)
        {
            var result = (from groups in _db.NodalUserGroups
                          join requests in _db.NodalCourseRequests on groups.Id equals requests.GroupId
                          join coursestatus in _db.CourseCompletionStatus on new { requests.UserId, requests.CourseId } equals new { coursestatus.UserId, coursestatus.CourseId } into CourseGroup
                          from CStatus in CourseGroup.DefaultIfEmpty()
                          where groups.IsDeleted == false
                          && requests.IsDeleted == false
                          && requests.IsApprovedByNodal == true
                          //&& requests.IsPaymentDone == true
                          && requests.GroupId == GroupId
                          && requests.Status == NodalCourseStatus.Inprogress
                          orderby requests.Id ascending
                          select new APIScormGroup
                          {
                              UserId = requests.UserId,
                              GroupId = (int)requests.GroupId,
                              RequestId = requests.Id,
                              Status = requests.Status == "inprogress" && CStatus.Status == null ? null : requests.Status
                          });

            APIScormGroup aPIScormGroup = await result.FirstOrDefaultAsync();
            if (aPIScormGroup != null)
                return aPIScormGroup;
            else
            {
                var completedresult = (from groups in _db.NodalUserGroups
                                       join requests in _db.NodalCourseRequests on groups.Id equals requests.GroupId
                                       where groups.IsDeleted == false
                                       && requests.IsDeleted == false
                                       && requests.IsApprovedByNodal == true
                                       //&& requests.IsPaymentDone == true
                                       && requests.GroupId == GroupId
                                       && requests.Status == NodalCourseStatus.Completed
                                       orderby requests.Id ascending
                                       select new APIScormGroup
                                       {
                                           UserId = requests.UserId,
                                           GroupId = (int)requests.GroupId,
                                           RequestId = requests.Id,
                                           Status = requests.Status
                                       });

                return await completedresult.FirstOrDefaultAsync();
            }
        }


    }
}
