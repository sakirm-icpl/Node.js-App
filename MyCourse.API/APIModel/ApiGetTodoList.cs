using System;

namespace MyCourse.API.APIModel
{
    public class ApiGetTodoList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ScheduleDate { get; set; }
        public string EndDate { get; set; }
        public bool Priority { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string ModuleType { get; set; }
    }
}
