using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class KitchenAuditOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }
    }
}
