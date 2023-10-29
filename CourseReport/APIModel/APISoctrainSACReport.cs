using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APISoctrainSAC
    {
        public DateTime FromDate { get; set; }
        public int? PageSize { get; set; }
        public int? StartIndex { get; set; }
        public DateTime ToDate { get; set; }
        
    }
    public class APISoctrainCountSAC
    {
        public DateTime FromDate { get; set; }
        public int PageSize { get; set; }
        public int StartIndex { get; set; }
        public DateTime ToDate { get; set; }

    }
    public class APISoctrainSACReport
    {
        public string UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailID { get; set; }
        public string Business { get; set; }
        public string AccountCreatedDate { get; set; }
      
    }

    public class APISoctrainSACTrainingModule
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Search { get; set; }
        public string ColumnName { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
    public class APISoctrainSACTrainingCountModule
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Search { get; set; }
        public string ColumnName { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    public class APISoctrainSACTrainingReport
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string AccountCreatedDate { get; set; }
        public string Business { get; set; }
        public string TrainingName { get; set; }
        public string TrainingCompletionStatus { get; set; }
        public string TrainingStartDate { get; set; }
        public string TrainingCompletionDate { get; set; }

    }
}
