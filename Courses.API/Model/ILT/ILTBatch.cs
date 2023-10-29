using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.ILT
{
    public class ILTBatch : BaseModel
    {
        public int Id { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? SeatCapacity { get; set; }
        public string Description { get; set; }
        public string BatchType { get; set; }
        public string ReasonForCancellation { get; set; }
    }
}
