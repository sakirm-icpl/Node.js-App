using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApplicability.API.Model
{
    [Table("InvoicePaymentResponse", Schema = "Course")]
    public class InvoicePaymentResponse : BaseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string error_desc { get; set; }
        public string invoice_id { get; set; }
        public string tiny_url { get; set; }
        public string qr_code { get; set; }
        public string error_code { get; set; }
        public int invoice_status { get; set; }
        public string merchant_reference_no { get; set; }
    }
}

