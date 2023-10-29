using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class KitchenAuditQuestion :  CommonFields
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
