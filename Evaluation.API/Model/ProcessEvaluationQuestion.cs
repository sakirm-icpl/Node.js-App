using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Evaluation.API.Models;

namespace Evaluation.API.Model
{
    public class ProcessEvaluationQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }       
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }        
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }


    }

    public class PMSEvaluationQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        //[Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }
        public string ObservableBehaviorCompetency { get; set; }
        public string DefinitionOfTheMeasure { get; set; }

    }

    public class PMSEvaluationSubmit
    {
        public int Id { get; set; }
        public int Userid { get; set; }
        public int ManagerId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }

    public class PMSEvaluationPoint
    {
        public int Id { get; set; }
        public string PointText { get; set; }
        public string Section { get; set; }
        public int RefQuestionId { get; set; }
    }

    public class PMSObsAndMeasureByBand
    {
        public int Id { get; set; }
        public string DefinitionOfTheMeasure { get; set; }
        public string ObservableBehaviorCompetency { get; set; }
    }

    public class PMSEvaluationPointResponse
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string DefinitionOfTheMeasure { get; set; }
        public string ObservableBehaviorCompetency { get; set; }

        public List<string> PointsText { get; set; }
    }

    public class PMSEvaluationResultPointResponse
    {
        public int Id { get; set; }
        public int SubmittedId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string UserFeedback { get; set; }
        public string SubjectiveUserFeedback { get; set; }
        public string ManagerFeedback { get; set; }
        public string SubjectiveManagerFeedback { get; set; }
        public string Section { get; set; }
        public string DefinitionOfTheMeasure { get; set; }
        public string ObservableBehaviorCompetency { get; set; }

        public List<string> PointsText { get; set; }
    }

    public class OEREvaluationQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }


    }

    public class CriticalAuditQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }


    }

    public class NightAuditQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }


    }

    public class OpsAuditQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string Section { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool AllowNA { get; set; }
        [Required]
        public bool IsSubquestion { get; set; }
        [Required]
        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
        public bool AllowTextReply { get; set; }


    }

}
