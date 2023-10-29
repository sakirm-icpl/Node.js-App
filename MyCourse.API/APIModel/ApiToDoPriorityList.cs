using MyCourse.API.Helper;
using MyCourse.API.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class ApiToDoPriorityList
    {
        [Required]
        public int RefId { get; set; }
        [Required]
        public bool Priority { get; set; }
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Course, CommonValidation.Survey })]
        public string Type { get; set; }
        [Required]
        [DataType(DataType.Date)]

        public DateTime ScheduleDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
