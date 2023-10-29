using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class NodalUserSignUp
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string UserName { get; set; }
        public string FHName { get; set; }
        public DateTime DateOfBirth { get; set; }
        [MaxLength(100)]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid Email Id")]
        public string EmailID { get; set; }
        [MaxLength(15)]
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$", ErrorMessage = "Invalid Mobile No")]
        public string MobileNumber { get; set; }
        public string AdhaarNumber { get; set; }
        public int AirportId { get; set; }
    }
}
