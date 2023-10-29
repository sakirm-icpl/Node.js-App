using CourseApplicability.API.APIModel;
using Courses.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface IEmail
    { 
        Task<int> SendCourseRequestApprovalMailToUsers(List<APINodalRequestList> aPINodalRequestList);
        Task<int> SendSelfCourseRequestMail(APISelfCourseRequestEmail aPISelfCourseRequestEmail);
        Task<int> SendCourseRequestRejectedMailToUsers(List<APINodalRequestList> aPINodalRequestList);
        Task<int> UserSignUpMailToNodalOfficers(List<APINodalUserDetailsEmail> aPINodalUserDetailsList);
    }
}
