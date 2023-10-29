using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class ProcessResultDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int EvalResultID { get; set; }
        [Required]
        public int QuestionID { get; set; }
        public int? OptionAnswerId { get; set; }
        [MaxLength(500)]
        public string SelectedAnswer { get; set; }
        [MaxLength(500)]
        public string ImprovementAnswer { get; set; }
        public double? Marks { get; set; }
        public string? FilePath1 { get; set; }
        public string? FilePath2 { get; set; }
        public string? FilePath3 { get; set; }
        public string? FilePath4 { get; set; }
        public string? FilePath5 { get; set; }
        public string? FilePath6 { get; set; }
        public string? FilePath7 { get; set; }
        public string? FilePath8 { get; set; }
        public string? FilePath9 { get; set; }
        public string? FilePath10 { get; set; }
    }

    public class CriticalAuditProcessResultDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int EvalResultID { get; set; }
        [Required]
        public int QuestionID { get; set; }
        public int? OptionAnswerId { get; set; }
        [MaxLength(500)]
        public string SelectedAnswer { get; set; }
        [MaxLength(500)]
        public string ImprovementAnswer { get; set; }
        public double? Marks { get; set; }
        public string? FilePath1 { get; set; }
        public string? FilePath2 { get; set; }
        public string? FilePath3 { get; set; }
        public string? FilePath4 { get; set; }
        public string? FilePath5 { get; set; }
        public string? FilePath6 { get; set; }
    }

    public class NightAuditProcessResultDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int EvalResultID { get; set; }
        [Required]
        public int QuestionID { get; set; }
        public int? OptionAnswerId { get; set; }
        [MaxLength(500)]
        public string SelectedAnswer { get; set; }
        [MaxLength(500)]
        public string ImprovementAnswer { get; set; }
        public double? Marks { get; set; }
        public string? FilePath1 { get; set; }
        public string? FilePath2 { get; set; }
        public string? FilePath3 { get; set; }
        public string? FilePath4 { get; set; }
        public string? FilePath5 { get; set; }
        public string? FilePath6 { get; set; }
    }

    public class OpsAuditProcessResultDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int EvalResultID { get; set; }
        [Required]
        public int QuestionID { get; set; }
        public int? OptionAnswerId { get; set; }
        [MaxLength(500)]
        public string SelectedAnswer { get; set; }
        [MaxLength(500)]
        public string ImprovementAnswer { get; set; }
        public double? Marks { get; set; }
        public string? FilePath1 { get; set; }
        public string? FilePath2 { get; set; }
        public string? FilePath3 { get; set; }
        public string? FilePath4 { get; set; }
        public string? FilePath5 { get; set; }
        public string? FilePath6 { get; set; }
    }
}
