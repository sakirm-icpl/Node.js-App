using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class ApiSocialMedia
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public string MsgTemplate { get; set; }
        public string GroupTemplateId { get; set; }
        public string GroupTemplateName { get; set; }
        public string Medium { get; set; }
        public string Subject { get; set; }
        public SocialMediaRules[] SocialMediaRules { get; set; }
    }
    public class SocialMediaRules
    {
        [MaxLength(50)]
        public string AccessibilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }
    public class ApiSocialMediaRulesUserDetails
    {
          public string UserName { get; set; }
        public string EmailId { get; set; }
        public string UserId { get; set; }


    }
    public class APIGetRules
    {
        public string Guid { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class APISocialMediaRejected
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string UserId { get; set; }
        public int CreatedBy { get; set; }
        [MaxLength(250)]
        public DateTime CreatedDate { get; set; }
        [Required]
        [MaxLength(500)]
        public string ErrorMessage { get; set; }

    }
}
