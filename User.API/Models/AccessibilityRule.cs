using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    [Table("AccessibilityRule", Schema = "Course")]
    public class AccessibilityRule : BaseModel
    {
        public int Id { get; set; }
        public int? UserID { get; set; }
        [DefaultValue(false)]
        public bool IsCourseFee { get; set; }
        [MaxLength(10)]
        public string ConditionForRules { get; set; }
        public int? CourseId { get; set; }
        public string RowGUID { get; set; }
        public int? UserTeamId { get; set; }
    }
}
