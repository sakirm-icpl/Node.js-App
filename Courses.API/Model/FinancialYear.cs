using System;

namespace Courses.API.Model
{
    public class FinancialYear : BaseModel
    {
        public int Id { get; set; }
        public DateTime YearStartDate { get; set; }
        public DateTime YearEndDate { get; set; }
        public string YearDescription { get; set; }
        public string Quarter { get; set; }
        public int QuarterSequence { get; set; }
        public DateTime QuarterStartDate { get; set; }
        public string Month { get; set; }
        public DateTime MonthStartDate { get; set; }
        public DateTime MonthEndDate { get; set; }
    }
}
