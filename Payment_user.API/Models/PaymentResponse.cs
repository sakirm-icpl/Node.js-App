using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Models
{
    public class PaymentResponse
    {
        public int id { get; set; }
        public string payment_mode { get; set; }
        public string resp_message { get; set; }
        public string udf5 { get; set; }
        public string cust_email_id { get; set; }
        public string udf3 { get; set; }
        public string merchant_id { get; set; }
        public string txn_amount { get; set; }
        public string udf4 { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string pg_ref_id { get; set; }
        public string txn_id { get; set; }
        public string resp_date_time { get; set; }
        public string bank_ref_id { get; set; }
        public string resp_code { get; set; }
        public string txn_date_time { get; set; }
        public string trans_status { get; set; }
        public string cust_mobile_no { get; set; }
    }
}
