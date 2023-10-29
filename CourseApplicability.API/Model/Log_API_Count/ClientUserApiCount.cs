using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApplicability.API.Model.Log_API_Count
{
    [Table("ClientUserApiCount", Schema = "dbo")]
    public class ClientUserApiCount
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string ServiceName { get; set; }
        public string OrgCode { get; set; }
    }
}