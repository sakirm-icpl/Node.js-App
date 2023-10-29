using System;

namespace Courses.API.Model
{
    public class ToDoPriorityList
    {
        public int Id { get; set; }
        public int RefId { get; set; }
        public bool Priority { get; set; }
        public string Type { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
