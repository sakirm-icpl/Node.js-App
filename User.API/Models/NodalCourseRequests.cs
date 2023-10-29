﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    [Table("NodalCourseRequests", Schema = "Course")]
    public class NodalCourseRequests
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public bool? IsApprovedByNodal { get; set; }
        public string Reason { get; set; }
        public bool IsPaymentDone { get; set; }
        public string PaymentUrl { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string RequestType { get; set; }
        public int? GroupId { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; }
        public bool IsAccepted { get; set; }
        public float CourseFee { get; set; }
    }
}
