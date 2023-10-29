using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class OERProcessResultDetails : CommonFields
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
        
    }
}
