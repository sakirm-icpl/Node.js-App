using System;

namespace Courses.API.Model.ILT
{
    public class ScheduleHolidayDetails : BaseModel
    {
        public int ID { get; set; }
        public int ReferenceID { get; set; }
        public bool IsHoliday { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
    }
}
