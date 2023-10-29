using MyCourse.API.APIModel;
//using MyCourse.API.APIModel.ILT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IEmail
    {
        //int SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null);
        //Task<int> SendILTNominationEmail(List<APIILTNominationEmail> objAPIILTNominationEmail);
        //Task<int> SendILTNominationCancellationEmail(List<APIILTNominationEmail> objAPIILTNominationEmail);
        //Task<int> SendILTNotificationSMS(List<APINominateUserSMS> objSMS);
        //Task<int> SendTNASubmissionByEndUserEmail(string userName, string ToEmail, string orgCode, string CC = null);
        //Task<int> SendTNASubmissionByLmEmail(string userName, string ToEmail, string orgCode, bool isApprovedByTa, string TNAYear, string CC = null);
        //Task<int> SendTNASubmissionByBUEmail(string userName, string ToEmail, string orgCode, string CC = null);
        //Task<int> SendCourseApplicabilityEmail(int CourseId, string orgCode);
        //Task<int> SendCourseAddedNotification(int courseId, string title, string courseCode, string token);
        //Task<int> SendScheduleCreationEmailNotification(int? CourseId, string orgCode, int scheduleId);

        //Task<int> CourseRequestMailToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode);
        //Task<int> ScheduleEnrollmentMailToLM(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string EmployeeName, string startTime, string endtime, string sdate, string edate, string venue);

        //Task<int> TrainingRequestApprovalByLM(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string EndUserName);

        //Task<int> EnrollmentNotificationToHR(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string endusername);
        //Task<int> TrainingRequestFullyApprovedToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue);

        //Task<int> TrainingRequestRejectedToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue);

        //Task<int> NominationToEmpInEnrollment(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string BU_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue);
        //Task<int> ScheduleCreationMailToSupervisor(string orgCode, string Email, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue);
        //Task<int> SendMailForAbsentUsers(APIILTAttendanceResponse aPIILTAttendanceResponse, string OrgCode);
        //Task<int> SendRemainderMailForCourse(int CourseID, string organizationCode);
        //Task<int> SendRemainderSMSForCourse(int CourseID, string organizationCode);
        //Task<int> SendScheduleCancellationEmail(string orgCode, int scheduleId);
        //Task<int> ScheduleRequestedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string EmployeeName, string startTime, string endtime, string sdate, string edate, string venue);
        //Task<int> TrainingRequestApprovalMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string courseName, int courseId, string EmpoweredHost);
        //Task<int> TrainingRequestRejectedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue,string reason);
        //Task<int> SendMailForAttendanceUsers(APIILTAttendanceResponse aPIILTAttendanceResponse, string OrgCode);
        //Task<int> SendNominationMail(string UserID, string OrganisationCode, string lblCourseName, string lblModuleName, string lblScheduleCode, string PlaceName, string Venue, string StartDate, string EndDate, string StartTime, string EndTime, string ContactPersonName,
        //  string ContactPersonNumber);
        //Task<int> SendNominationSMS(string lblUserId, string lblCourseName, string StartDate, string StartTime, string OTP, string OrgCode, string EndDate, string EndTime);

        Task<int> SendCourseCompletionStatusMail(string CourseTitle, int UserId, APIUserDetails aPIUserDetails, int CourseId);
        //int SendCourseApplicabilityPushNotification(int CourseId, string orgCode);
        //Task<int> SendScheduleCancellationNotificationSMS(List<APINominateUserSMS> objSMS);
        //Task<int> SendScheduleRequestApproveNotificationSMS(List<APINominateUserSMS> objSMS);
        //Task<int> SendScheduleRequestRejectNotificationSMS(List<APINominateUserSMS> objSMS);
        //Task<int> SendScheduleRequestNotificationSMSToManager(List<APINominateUserSMS> objSMS);
        //Task<int> TrainingBatchRequestApprovalMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue);
        //Task<int> TrainingBatchRequestRejectedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string reason);
        //Task<int> SendCourseRequestApprovalMailToUsers(List<APINodalRequestList> aPINodalRequestList);
        //Task<int> SendSelfCourseRequestMail(APISelfCourseRequestEmail aPISelfCourseRequestEmail);
        //Task<int> SendCourseRequestRejectedMailToUsers(List<APINodalRequestList> aPINodalRequestList);
        //Task<int> UserSignUpMailToNodalOfficers(List<APINodalUserDetailsEmail> aPINodalUserDetailsList);
        //Task<int> SendRemainderMailForUserWiseCourse(string UserId, int CourseID, string organizationCode);
    }
}
