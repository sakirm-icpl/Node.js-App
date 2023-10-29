using Courses.API.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class Banner : BaseModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string BannerType { get; set; }

        public string ThumbnailImage { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public int BannerNumber { get; set; }
    }
    public class BannerPayload
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string BannerType { get; set; }

        public string ThumbnailImage { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public int BannerNumber { get; set; }
    }

    public class ConfigurableParameter : CommonFields
    {
        public int Id { get; set; }
        public string Attribute { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string FieldType { get; set; }
        public string ParameterType { get; set; }
        public string VisibilityType { get; set; }
    }

    public class ThumbnailImage
    {
        public string Base64String { get; set; }
    }
}
