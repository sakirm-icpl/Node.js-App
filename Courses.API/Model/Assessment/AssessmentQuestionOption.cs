﻿using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class AssessmentQuestionOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        [MaxLength(500)]
        public string UploadImage { get; set; }
        public string ContentType { get; set; }
        public string ContentPath { get; set; }
    }
}
