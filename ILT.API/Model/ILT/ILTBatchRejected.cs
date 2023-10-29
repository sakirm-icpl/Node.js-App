using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Model.ILT
{
    public class ILTBatchRejected
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string BatchName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RegionName { get; set; }
        public string SeatCapacity { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
