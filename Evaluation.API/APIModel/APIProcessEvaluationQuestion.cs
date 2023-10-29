using Evaluation.API.APIModel;
using Evaluation.API.Helper;
using Evaluation.API.Validations;
using System.ComponentModel.DataAnnotations;
using System;
//using Assessment.API.Models;

namespace Evaluation.API.APIModel
{
   
    public class APIPEQuestionOption
    {
        public int ProcessQuestionOptionID { get; set; }
        public int ProcessQuestionID { get; set; }
        public string OptionText { get; set; }
        public bool IsAnswer { get; set; }
        public int? RefQuestionID { get; set; }       
        public decimal DevidedMarks { get; set; }
    }

    public class APIProcessEvaluationQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }      

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }        

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string OptionType { get; set; }
       
        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Marks { get; set; }
        public bool Status { get; set; }
        [Range(0, 5, ErrorMessage = "Options must in the range of 2 to 5")]
        public int Options { get; set; }
         public string Metadata { get; set; }      
        public APIPEQuestionOption[] aPIOptions { get; set; }          
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        public bool AllowTextReply { get; set; }
        public int? OptionCount { get; set; }
        public bool Allcorrect { get; set; }
    }

    public class APICriticalAuditQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string OptionType { get; set; }

        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Marks { get; set; }
        public bool Status { get; set; }
        [Range(0, 5, ErrorMessage = "Options must in the range of 2 to 5")]
        public int Options { get; set; }
        public string Metadata { get; set; }
        public APIPEQuestionOption[] aPIOptions { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        public bool AllowTextReply { get; set; }
        public int? OptionCount { get; set; }
        public bool Allcorrect { get; set; }
    }

    public class APINightAuditQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string OptionType { get; set; }

        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Marks { get; set; }
        public bool Status { get; set; }
        [Range(0, 5, ErrorMessage = "Options must in the range of 2 to 5")]
        public int Options { get; set; }
        public string Metadata { get; set; }
        public APIPEQuestionOption[] aPIOptions { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        public bool AllowTextReply { get; set; }
        public int? OptionCount { get; set; }
        public bool Allcorrect { get; set; }
    }

    public class APIOpsAuditQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string OptionType { get; set; }

        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Marks { get; set; }
        public bool Status { get; set; }
        [Range(0, 5, ErrorMessage = "Options must in the range of 2 to 5")]
        public int Options { get; set; }
        public string Metadata { get; set; }
        public APIPEQuestionOption[] aPIOptions { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        public bool AllowTextReply { get; set; }
        public int? OptionCount { get; set; }
        public bool Allcorrect { get; set; }
    }

    public class APIPMSEvaluationQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }

        public string QuestionText { get; set; }

        //[CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string OptionType { get; set; }

        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Marks { get; set; }
        public bool Status { get; set; }

        public string Metadata { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        public bool AllowTextReply { get; set; }

        public string ObservableBehaviorCompetency { get; set; }
        public string DefinitionOfTheMeasure { get; set; }
    }

    public class apiGetTransactionID
    {
        public int EvalUserID { get; set; }
        public string EvalUser { get; set; }
        public DateTime DateforEvaluation { get; set; }
        public string Region { get; set; }
    }
    public class  apiGetQuestionforChaipoint
    {
        public string section { get; set; }
 
    }

    public class APIPostProcessQuestionDetails
    {
        public int Id { get; set; }
        public int? ReferenceQuestionID { get; set; }
        public double? Marks { get; set; }
        public int?[] OptionAnswerId { get; set; }
        public string SelectedAnswer { get; set; }
        public string ImprovementAnswer { get; set; }
        public string OptionType { get; set; }
        public string?[] files { get; set; }

        public bool isAnsweredtrue { get; set; }
    }
    

   

    public class APIPostProcessEvaluationResult
    {
        public APIPostProcessQuestionDetails[] APIPostProcessQuestionDetails { get; set; }
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double obtainedMarks { get; set; }
        public decimal obtainedPercentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string evaluationType { get; set; }
        public string RestaurantManagerID { get; set; }
    }

    public class APIGetSubmmitedId
    {
        public string ManagerId { get; set; }
    }

    public class APIUserId
    {
        public string UserId { get; set; }
    }

    public class APIPostProcessEvaluationResultForChaipoint
    {
        public APIPostProcessQuestionDetails[] APIPostProcessQuestionDetails { get; set; }
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double obtainedMarks { get; set; }
        public decimal obtainedPercentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string evaluationType { get; set; }
        public string auditorName { get; set; }
        public string regionName { get; set; }
        public string siteName { get; set; }
        public string staffName { get; set; }
    }

    public class APILastSubmitedResult
    {
        public string UserId { get; set; }
        public string BranchId { get; set; }
    }

    public class APILastSubmitedResultKitchenAudit
    {
        public string UserId { get; set; }
        public string BranchName { get; set; }
    }
    public class APIPostProcessEvaluationDisplay
    {
        public APIQuestionDetails[] aPIQuestionDetails { get; set; }
        public int Id { get; set; }
        public string ProcessManagement { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public string EvaluationByUserId { get; set; }
        public string EvaluationDate { get; set; }
        public int TotalMarks { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }
        public string supervisorName { get; set; }
        public string AuditType { get; set; }
    }

    public class APIPostCriticalAuditProcessEvaluationDisplay
    {
        public APIQuestionDetails[] aPIQuestionDetails { get; set; }
        public int Id { get; set; }
        public string ProcessManagement { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public string EvaluationByUserId { get; set; }
        public string EvaluationDate { get; set; }
        public int TotalMarks { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }
        public string supervisorName { get; set; }
        public string AuditType { get; set; }
    }

    public class APIQuestionDetails
    {  
        public int? ReferenceQuestionID { get; set; }       
        public int?[] OptionAnswerId { get; set; }
        public string SelectedAnswer { get; set; }
        public string ImprovementAnswer { get; set; }
       
    }
    public class APIPostOERProcessEvaluationDisplay
    {
        public APIQuestionDetails[] aPIQuestionDetails { get; set; }
        public int Id { get; set; }
        public string ProcessManagement { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public string EvaluationByUserId { get; set; }
        public string EvaluationDate { get; set; }
        public int TotalMarks { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }
        public string supervisorName { get; set; }
        public string AuditType { get; set; }
    }
}
