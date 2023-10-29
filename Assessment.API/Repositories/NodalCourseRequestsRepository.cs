using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Assessment.API.APIModel;
using Assessment.API.Models;

namespace Assessment.API.Repositories
{
    public class NodalCourseRequestsRepository : Repository<NodalCourseRequests>, INodalCourseRequestsRepository
    {
        private AssessmentContext _db;
        private ICustomerConnectionStringRepository _customerConnection;
        
        private IAccessibilityRule _accessibilityRule;
        private IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NodalCourseRequestsRepository));
        public NodalCourseRequestsRepository(AssessmentContext context,
            ICustomerConnectionStringRepository customerConnection,
           
            IAccessibilityRule accessibilityRule,
            IConfiguration configuration) : base(context)
        {
            _db = context;
            _customerConnection = customerConnection;
           
            _accessibilityRule = accessibilityRule;
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