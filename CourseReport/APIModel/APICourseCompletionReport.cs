using System.Collections.Generic;

namespace CourseReport.API.APIModel
{
    public class APICourseCompletionReport
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
      
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string CourseAssignedDate { get; set; }
        public string CourseStartDate { get; set; }
        public string CourseCompletionDate { get; set; }
        public string CourseStatus { get; set; }
        public bool IsRetraining { get; set; }
        public string LastActivityDate { get; set; }
        public string UserStatus { get; set; }
        public string UserEmailId { get; set; }
        public string MobileNumber { get; set; }
        public string DateOfJoining { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ReportsTo { get; set; }
        public string UserDuration { get; set; }

        public string Percentage { get; set; }
        public string FeedbackStatus { get; set; }
        public string AssessmentStatus { get; set; }
        
        public string AssessmentDate { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public string ConfigurationColumn1 { get; set; }
        public string ConfigurationColumn2 { get; set; }
        public string ConfigurationColumn3 { get; set; }
        public string ConfigurationColumn4 { get; set; }
        public string ConfigurationColumn5 { get; set; }
        public string ConfigurationColumn6 { get; set; }
        public string ConfigurationColumn7 { get; set; }
        public string ConfigurationColumn8 { get; set; }
        public string ConfigurationColumn9 { get; set; }
        public string ConfigurationColumn10 { get; set; }
        public string ConfigurationColumn11 { get; set; }
        public string ConfigurationColumn12 { get; set; }
        public int Id { get; set; }
       public int CourseProgress { get; set; }


    }
    public class APIDevPlanTotalReport
    {
        public List<APIDevPlanCompletionReport> data { get; set; }
        public int TotalRecord { get; set; }
    }
        public class APIDevPlanCompletionReport
    {
        public int Id { get; set; }
        public int DevPlanId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string DevelopementPlanCode { get; set; }
        public string DevelopementPlanName { get; set; }
        public string CourseStartDate { get; set; }
        public string CourseCompletionDate { get; set; }
        public string Status { get; set; }        
        public string LastActivityDate { get; set; }
        public string UserStatus { get; set; }
        public string UserEmailId { get; set; }
        public string MobileNumber { get; set; }
       
    }

    public class APIDevPlanDetailsReport
    {
        public string Id { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }      
        public string CourseStatus { get; set; }
        public string CompletionDate { get; set; }

    }

    public class APICourseProgressDetailsReport
    {
        public string Id { get; set; }
        public string CourseTitle { get; set; }      
        public bool IsAssessment { get; set; }
        public string AssessmentStatus { get; set; }
        public bool IsFeedback { get; set; }
        public string FeedbackStatus { get; set; }
        public bool IsAssignment { get; set; }
        public string AssignmentStatus { get; set; }
        public string CourseStatus { get; set; }

        public bool IsRetraining { get; set; }
        public string LastActivityDate { get; set; }
        public APICoureModuleDetails[] Moduleinfo { get; set; }
       


    }

    public class APICoureModuleDetails
    {
        public string Id { get; set; }      
        public string ModuleName { get; set; }
        public bool IsAssessment { get; set; }
        public string AssessmentStatus { get; set; }
        public bool IsFeedback { get; set; }
        public string FeedbackStatus { get; set; }      
        public string ModuleStatus { get; set; }

     
    }
}
