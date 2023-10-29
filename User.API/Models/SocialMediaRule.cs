using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using User.API.Validation;

namespace User.API.Models
{
    public class SocialMediaRule: BaseModel
    {
        public int Id { get; set;}
        public int? UserID { get; set;}
        [MaxLength(100)]
        public string EmailID { get; set;}
        [MaxLength(25)]
        public string MobileNumber { get; set;}
        [MaxLength(100)]
        public string ReportsTo { get; set;}
        [MaxLength(10)]
        public string UserType { get; set;}
        public int? Business { get; set;}
        public int? Group { get; set;}
        public int? Area { get; set;}
        public int? Location { get; set;}
        public int? ConfigurationColumn1 { get; set; }
        public int? ConfigurationColumn2 { get; set; }
        public int? ConfigurationColumn3 { get; set; }
        public int? ConfigurationColumn4 { get; set; }
        public int? ConfigurationColumn5 { get; set; }
        public int? ConfigurationColumn6 { get; set; }
        public int? ConfigurationColumn7 { get; set; }
        public int? ConfigurationColumn8 { get; set; }
        public int? ConfigurationColumn9 { get; set; }
        public int? ConfigurationColumn10 { get; set;}
        public int? ConfigurationColumn11 { get; set;}
        public int? ConfigurationColumn12 { get; set;}
        [MaxLength(100)]
        public string RuleAnticipation { get; set;}
        public int? TargetPeriod { get; set;}
        [MaxLength(10)]
        public string ConditionForRules { get; set;}
        [Required]
        public string MsgTemplate { get; set;}
        public int? GroupTemplateId { get; set;}
        [MaxLength(25)]
        [CommonValidationAttribute(AllowValue = new string[] { "SMS", "pushnotification","MAIL" })]
        public string Medium { get; set;}
        public string Guid { get; set;}
        public string Subject { get; set; }
        public string CommanGuid { get; set; }
    }
}
