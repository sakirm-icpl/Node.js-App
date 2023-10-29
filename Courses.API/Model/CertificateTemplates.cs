using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    [Table("CertificateTemplates", Schema = "Certification")]
    public class CertificateTemplates : BaseModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int TemplateId { get; set; }
        public String TemplateDesign { get; set; }
        public bool Status { get; set; }
    }
}
