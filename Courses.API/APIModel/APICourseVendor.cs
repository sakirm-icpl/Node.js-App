using Courses.API.Helper;
using Courses.API.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICourseVendor
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Code { get; set; }
        [Required]
        [MaxLength(500)]
        public string Name { get; set; }
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.External, CommonValidation.Internal })]
        public string Type { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class APITotalCourseVendor
    {
        public List<APICourseVendor> data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class APIGetCourseVendor
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string ColumnName { get; set; }
       
    }

}
