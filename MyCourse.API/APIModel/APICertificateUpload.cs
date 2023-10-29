using System;

namespace MyCourse.API.APIModel
{
    public class APICertificateUpload
    {
        public string File { get; set; }
    }

    public class APIExtCertificateReport
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public string TestScore { get; set; }
        public string Result { get; set; }
        public string Remark { get; set; }
        public string TotalNoHours { get; set; }
        public string PartnerName { get; set; }
        public int Cost { get; set; }
    }
}
