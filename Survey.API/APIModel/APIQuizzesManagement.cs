// ======================================
// <copyright file="APIQuizzesManagement.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace Survey.API.APIModel
{
    public class APIQuizzesManagement//:CommonFields
    {
        public int? Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(200)]
        [CSVInjection]
        public string QuizTitle { get; set; }
        public string ApplicabilityParameter { get; set; }
        public string ApplicabilityParameterValue { get; set; }
        public int? ApplicabilityParameterValueId { get; set; }
        [Required]
        [Range(1, 100000, ErrorMessage = "Target Response Count must in the range of 1 to 100000")]
        public int TargetResponseCount { get; set; }
        public bool Status { get; set; }
    }

    public class APIQuizQuestionMaster //: CommonFields
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string Question { get; set; }
        [Required]
        public string PicturePath { get; set; }
        [Required]
        public int QuizId { get; set; }
        [MaxLength(100)]
        public string Hint { get; set; }
        public bool AnswersArePictures { get; set; }
        public bool RandomizeSequence { get; set; }
    }
    public class APIQuizOptionMaster //: CommonFields
    {
        public int? Id { get; set; }
        [MaxLength(400)]
        [CSVInjection]
        public string AnswerText { get; set; }
        public string AnswerPicturePath { get; set; }
        public int QuizQuestionId { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }
    public class APIMergeredModel
    {
        public APIQuizQuestionMaster aPIQuizQuestionMaster { get; set; }
        public APIQuizOptionMaster[] aPIQuizOptionMaster { get; set; }
    }
    public class APIQuizQuestionMergered
    {
        public int? Id { get; set; }
        [CSVInjection]
        public string Question { get; set; }
        public string PicturePath { get; set; }
        [Required]
        public int QuizId { get; set; }
        [CSVInjection]
        public string Hint { get; set; }
        public bool AnswersArePictures { get; set; }
        public bool RandomizeSequence { get; set; }
        [Range(2, 4, ErrorMessage = "Options must in the range of 2 to 4")]
        public int? NoOfOption { get; set; }
        [Required]
        [MaxLength(200)]
        [CSVInjection]
        public string QuizTitle { get; set; }
        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 10")]
        public int Mark { get; set; }
        public bool IsActive { get; set; }
        public APIQuizOptionMaster[] aPIQuizOptionMaster { get; set; }
    }
    public class APIQuizResult //: CommonFields
    {
        public int? Id { get; set; }
        public int QuizId { get; set; }
        public string QuizResultStatus { get; set; }
        public int UserId { get; set; }
    }
    public class APIQuizResultDetail //: CommonFields
    {
        public int? Id { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int QuizResultId { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int QuizQuestionId { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int QuizOptionId { get; set; }
    }
    public class APIQuizResultMerged //: CommonFields
    {
        public int? Id { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int QuizId { get; set; }
        public string QuizResultStatus { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int UserId { get; set; }
        public APIQuizResultDetail[] aPIQuizResultDetail { get; set; }
    }

}
