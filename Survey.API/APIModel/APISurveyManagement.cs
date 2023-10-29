// ======================================
// <copyright file="APISurveyManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class APISurveyManagement//:CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(500)]
        //[RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        [CSVInjection]
        public string SurveySubject { get; set; }
        public string SurveyPurpose { get; set; }
        [Required]
        //[DataType(DataType.Date)]
        //[RestrictPastDtValidationAttribute("ValidityDate")]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ValidityDate { get; set; }
        [Required]
        [Range(1, 120, ErrorMessage = "Average Respond Time must in the range of 1 to 120")]
        public int AverageRespondTime { get; set; }
        public string ApplicabilityParameter { get; set; }
        public string ApplicabilityParameterValue { get; set; }
        public int? ApplicabilityParameterValueId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int TargetResponseCount { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int LcmsId { get; set; }
        public bool Status { get; set; }
        public string LcmsName { get; set; }

        public bool IsApplicableToAll { get; set; }
    }
    public class APISurveyQuestion //: CommonFields
    {
        public int? Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
        public int SurveyId { get; set; }
        public bool AllowSkipAswering { get; set; }
        public bool Status { get; set; }
    }
    public class APISurveyOption
    {
        public int? Id { get; set; }
        [CSVInjection]
        public string OptionText { get; set; }
        public int QuestionId { get; set; }
    }
    public class APISurveyMergeredModel
    {
        public int? Id { get; set; }
        public string Section { get; set; }
        //[RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        [CSVInjection]
        public string Question { get; set; }
        public string SurveySubject { get; set; }
        public bool AllowSkipAnswering { get; set; }
        public bool Status { get; set; }
        public string Error { get; set; }
        //  [Range(2, 10, ErrorMessage = "Options must in the range of 2 to 10")]
        public int? NoOfOption { get; set; }
        //[Range(2, 10, ErrorMessage = "Options must in the range of 2 to 10")]
        public int options { get; set; }
        public APISurveyOption[] aPISurveyOption { get; set; }
        public bool IsMultipleChoice { get; set; }
        public string OptionType { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class APISurveyQuestionMergered
    {
        public int? Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
        public int SurveyId { get; set; }
        public bool AllowSkipAswering { get; set; }
        public bool Status { get; set; }
        public APISurveyOption[] aPISurveyOption { get; set; }
        public string OptionType { get; set; }

    }

    public class APISurveyResult //: CommonFields
    {
        public int? Id { get; set; }
        public int SurveyId { get; set; }
        public string SurveyResultStatus { get; set; }
        public int UserId { get; set; }
    }
    public class APISurveyResultDetail //: CommonFields
    {
        public int? Id { get; set; }
        public int SurveyResultId { get; set; }
        public string Section { get; set; }
        public int ServeyQuestionId { get; set; }
        // public int? ServeyOptionId { get; set; }
        public int[] surveyOptionId { get; set; }
        [CSVInjection]
        public string SubjectiveAnswer { get; set; }
        //public string OptionType { get; set; } 
    }

    public class APISurveyResultMerged //: CommonFields
    {
        public int? Id { get; set; }
        public int SurveyId { get; set; }
        public string SurveyResultStatus { get; set; }
        //public int UserId { get; set; } 
        public APISurveyResultDetail[] aPISurveyResultDetail { get; set; }
    }
    public class ApiSurveyLcms
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string MetaData { get; set; }
        public int LcmsId { get; set; }
        public bool? IsNested { get; set; }
        public SurveyLcmsQuestion[] SurveyQuestion { get; set; }

    }
    public class SurveyLcmsQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
    }
    public class GetApiSurveyLcms
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string MetaData { get; set; }
        public int LcmsId { get; set; }
        public bool? IsNested { get; set; }
        public bool Ismodulecreate { get; set; }
        public List<SurveyLcmsQuestion> SurveyQuestion { get; set; }

    }
    public class APISurveyQuestionOption
    {
        public int surveyResultId { get; set; }

        public APISurveyAnswerDetails[] answerDeatils { get; set; }
        

    }

    public class APISurveyAnswerDetails
    {
        public int surveyQuestionId { get; set; }
        public string section { get; set; }
        public string optionType { get; set; }
        public string subjectiveAnswer { get; set; }
        public int [] surveyOptionId { get; set; }
    }

    public class ApiSurveyQuestionSearch
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }

    }


}
