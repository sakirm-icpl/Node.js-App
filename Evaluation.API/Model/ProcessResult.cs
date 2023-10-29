using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class ProcessResult : CommonFields
    {
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }

        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string AuditorName { get; set; }
        public string RegionName { get; set; }
        public string SiteName { get; set; }
        public string StaffName { get; set; }
        public int? RestaurantManagerID { get; set; }
    }
    public class CriticalAuditProcessResult : CommonFields
    {
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }

        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string AuditorName { get; set; }
        public string RegionName { get; set; }
        public string SiteName { get; set; }
        public string StaffName { get; set; }
        public int? RestaurantManagerID { get; set; }
    }

    public class NightAuditProcessResult : CommonFields
    {
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }

        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string AuditorName { get; set; }
        public string RegionName { get; set; }
        public string SiteName { get; set; }
        public string StaffName { get; set; }
        public int? RestaurantManagerID { get; set; }
    }

    public class OpsAuditProcessResult : CommonFields
    {
        public int Id { get; set; }
        public int ManagementId { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        [MaxLength(30)]
        public string Result { get; set; }
        public double MarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public int NoOfAttempts { get; set; }
        public int? BranchId { get; set; }
        public int TotalMarks { get; set; }
        public string supervisorId { get; set; }
        public int? StarRating { get; set; }
        public int? CountOfExtremeViolation { get; set; }

        public DateTime EvaluationDate { get; set; }
        public string AuditType { get; set; }
        public string AuditorName { get; set; }
        public string RegionName { get; set; }
        public string SiteName { get; set; }
        public string StaffName { get; set; }
        public int? RestaurantManagerID { get; set; }
    }

}
