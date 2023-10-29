using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    [Table("CertificateDownloadDetails", Schema = "Certification")]
    public class CertificateDownloadDetails
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SerialNumber { get; set; }
    }
}
