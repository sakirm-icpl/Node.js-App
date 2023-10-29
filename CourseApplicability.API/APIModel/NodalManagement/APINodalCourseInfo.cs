using CourseApplicability.API.APIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApplicability.API.APIModel.NodalManagement
{
    public class APICourseResponse
    {
        public int TotalRecords { get; set; }
        public List<APINodalCourseInfo> aPINodalCourseInfos { get; set; }
    }

    public class APINodalCourseInfo
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string CourseType { get; set; }
        public string Description { get; set; }
        public string ThumbnailPath { get; set; }
        public string Currency { get; set; }
        public float Cost { get; set; }
    }
    public class APINodalCourseTypeahead
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
    }
    public class APINodalUserDetails
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string UserName { get; set; }
    }

    public class APIGroupCertificateTemplatesResult : APICertificateTemplatesResult
    {
        public string Status { get; set; }
    }

    public class InvoiceBasePaymentJson
    {
        public string customer_name { get; set; }
        public string bill_delivery_type { get; set; }
        public string customer_mobile_no { get; set; }
        public string customer_email_id { get; set; }
        public string customer_email_subject { get; set; }
        public string invoice_description { get; set; }
        public string currency { get; set; }
        public string valid_for { get; set; }
        public string valid_type { get; set; }
        public string amount { get; set; }
        public string merchant_reference_no { get; set; }
        public string merchant_reference_no1 { get; set; }
        public string merchant_reference_no2 { get; set; }
        public string merchant_reference_no3 { get; set; }
        public string merchant_reference_no4 { get; set; }
        public string merchant_reference_no5 { get; set; }
        public string sub_acc_id { get; set; }
        public string terms_and_conditions { get; set; }
        public string sms_content { get; set; }

    }

    public class PaymentResponceJson
    {
        public string error_desc { get; set; }
        public string invoice_id { get; set; }
        public string tiny_url { get; set; }
        public string qr_code { get; set; }
        public int invoice_status { get; set; }
        public string error_code { get; set; }
        public string merchant_reference_no { get; set; }

    }
}

