using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class ApiCertificationUpload
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate  { get; set; }
        public string TrainingCode { get; set; }
        public int BatchNo { get; set; }
        public string TrainingName { get; set; }
        public decimal PassPercentage { get; set; }
        public string TrainingMode { get; set; }
        public int TestScore { get; set; }
        public int NoofSessions { get; set; }
        public string Result { get; set; }
        public int TotalNoHours { get; set; }
        public string PartnerName { get; set; }
        public int Cost { get; set; }
        public string CertificatePath { get; set; }
        public string Remark { get; set; }
        public int? UserId { get; set; } 
        public string Username { get; set; } 

    }

    public class CertificationUpload : CommonFields

    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TrainingCode { get; set; }
        public int BatchNo { get; set; }
        public string TrainingName { get; set; }
        public decimal PassPercentage { get; set; }
        public string TrainingMode { get; set; }
        public int TestScore { get; set; }
        public int NoofSessions { get; set; }
        public string Result { get; set; }
        public int TotalNoHours { get; set; }
        public string PartnerName { get; set; }
        public int Cost { get; set; }
        public string CertificatePath { get; set; }
        public string Remark { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
       
    }
    public class APIDownloadFile
    {
        [Required]
        public int Id { get; set; }
    }
}

