using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class OERProcessResult : CommonFields
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
    }

    
}
