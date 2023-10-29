using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Model
{
    public class CompetencyReviewParameters : BaseModel
    {
        public int Id { get; set; }
        public int CompetencyCategoryId { get; set; }
        public int? CompetencySubcategoryId { get; set; }
        public int? CompetencySubSubcategoryId { get; set; }
        public int CompetencyId { get; set; }
        public string? Level { get; set; }
        public string?  LevelDescription { get; set; }
        public int? JobRoleId { get; set; }
        public string ReviewParameter { get; set; }
    }

    public class CompetencyReviewParametersResult 
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string  CategoryName { get; set; }
        public string SubcategoryId { get; set; }
        public string SubcategoryName { get; set; } 
        public string SubSubcategoryId { get; set; }
        public string SubSubcategoryName { get; set; }
        public int CompetencyId { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyLevelId { get; set; }
        public string CompetencyLevelDescription { get; set; }
        public string JobRoleId { get; set; }
        public string JobRoleName { get; set; }
        public string ReviewParameter { get; set; }
    }

    public class ReviewParametersPostModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string FilterName { get; set; }
        public string FilterValue { get; set; }
    }

    public class GetReviewParametersPostModel
    {
        public int CompetencyID { get; set; }
        public int JobRoleID { get; set; }
    }
    public class GetReviewParametersPostModelResult
    {
        public int Id { get; set; }
        public string ReviewParameter { get; set; }
        public string LevelID { get; set; }
        public string LevelName { get; set; }
    }
    public class GetReviewParametersOptions
    {
        public int Id { get; set; }
        public string OptionName { get; set; }
        
    }

    public class CompetencyReviewParametersOptions :BaseModel
    {
        public int Id { get; set; }
        public string OptionName { get; set; }
        public bool IsSupervisor { get; set; }
    }

    public class CompetencyReviewParametersAssessment: BaseModel
    {
        public int Id { get; set; }
        public string AssessmentType { get; set; }
        public int UserId{ get; set; }
        public int SupervisorId { get; set; }
        public string SupervisorOverallRemark{ get; set; }
        public int CompetencyId{ get; set; }
        public string SupervisorLevel{ get; set; }
        public string UserLevel{ get; set; }
        public int ReviewParameterID{ get; set; }
        public string UserResponse{ get; set; }
        public string SupervisorResponse{ get; set; }
        public DateTime SupervisorDate{ get; set; }
        
    }
    public class CompetencySupervisorUpdate
    {
        public int Id { get; set; }
        public string SupervisorOverallRemark { get; set; }
        public string SupervisorLevel { get; set; }
        public APISelfAssessmentQuestionsSupervisor[] assessmentQuestions { get; set; }

    }
    public class APICompetencyReviewParametersSelfAssessment
    {

        public string AssessmentType { get; set; }
        public int CompetencyId { get; set; }
        public string UserLevel { get; set; }

        public APISelfAssessmentQuestions[] assessmentQuestions{get; set;}


    }
    public class APICompetencyReviewParametersSupervisorAssessment
    {

        public string AssessmentType { get; set; }
        public int CompetencyId { get; set; }
        public string UserLevel { get; set; }
        public string OverallRemark { get; set; }
        public APISelfAssessmentQuestions[] assessmentQuestions { get; set; }


    }
    public class APISelfAssessmentQuestions
    {
        public int ReviewParameterID { get; set; }
        public string UserResponse { get; set; }
    }  public class APISelfAssessmentQuestionsSupervisor
    {
        public int Id { get; set; }
        public int ReviewParameterID { get; set; }
        public string UserResponse { get; set; }
    }
    public class UserIdPayload
    {
        public string Id { get; set; }
    }

    public class SelfRatingForSupervisor
    {
        public string UserId { get; set; }
        public int CompetencyId { get; set; }
    }
}
