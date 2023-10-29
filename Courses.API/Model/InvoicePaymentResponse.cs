using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
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
